#!/usr/bin/env node

// Audits the Alt+H contextual help (src/help/<locale>/<helpKey>.md) against the
// contract in docs/help-module.md: every route helpKey has a file in every
// locale, no file is orphaned, headings follow the mandatory order, and every
// Mermaid block parses. Exits with code 1 when an error is found.

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const FRONTEND_ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const HELP_ROOT = path.join(FRONTEND_ROOT, "src", "help");
const MODULES_ROOT = path.join(FRONTEND_ROOT, "src", "modules");
const LOCALES = ["ca", "es", "en"];
const HEADINGS = {
  ca: [
    "Per a que serveix aquesta pantalla",
    "Accions disponibles",
    "Flux habitual",
    "Aspectes importants",
    "Errors frequents",
    "Proces basic",
  ],
  es: [
    "Para qué sirve esta pantalla",
    "Acciones disponibles",
    "Flujo habitual",
    "Aspectos importantes",
    "Errores frecuentes",
    "Proceso básico",
  ],
  en: [
    "What this screen is for",
    "Available actions",
    "Usual flow",
    "Important notes",
    "Common errors",
    "Basic process",
  ],
};

const walk = (directory) =>
  fs.existsSync(directory)
    ? fs
        .readdirSync(directory, { withFileTypes: true })
        .flatMap((entry) =>
          entry.isDirectory()
            ? walk(path.join(directory, entry.name))
            : [path.join(directory, entry.name)],
        )
    : [];

const collectRouteKeys = () => {
  const keys = new Set();
  for (const file of walk(MODULES_ROOT).filter((f) => f.endsWith("routes.ts"))) {
    for (const match of fs.readFileSync(file, "utf8").matchAll(/helpKey:\s*"([^"]+)"/g)) {
      keys.add(match[1]);
    }
  }
  return keys;
};

const collectHelpKeys = (locale) => {
  const root = path.join(HELP_ROOT, locale);
  return new Set(
    walk(root)
      .filter((file) => file.endsWith(".md"))
      .map((file) => path.relative(root, file).replace(/\\/g, "/").replace(/\.md$/, "")),
  );
};

// Mermaid only needs the DOM to render. A DOMPurify error means the parse succeeded.
const createMermaidParser = async () => {
  const { default: mermaid } = await import("mermaid");
  return async (source) => {
    try {
      await mermaid.parse(source);
      return undefined;
    } catch (error) {
      const message = String(error?.message ?? error);
      return /DOMPurify/.test(message) ? undefined : message.split("\n").slice(0, 3).join(" ");
    }
  };
};

const main = async () => {
  const errors = [];
  const routeKeys = collectRouteKeys();
  const helpKeys = Object.fromEntries(LOCALES.map((locale) => [locale, collectHelpKeys(locale)]));
  const parseMermaid = await createMermaidParser();

  for (const key of routeKeys) {
    for (const locale of LOCALES) {
      if (!helpKeys[locale].has(key)) errors.push(`${locale}/${key}: missing file for route helpKey`);
    }
  }

  for (const locale of LOCALES) {
    for (const key of helpKeys[locale]) {
      const id = `${locale}/${key}`;
      if (!routeKeys.has(key)) errors.push(`${id}: no route declares this helpKey`);

      const text = fs.readFileSync(path.join(HELP_ROOT, locale, `${key}.md`), "utf8").replace(/\r\n/g, "\n");
      const lines = text.split("\n");

      if (!/^# \S/.test(lines[0])) errors.push(`${id}: first line must be the "# Title"`);
      if (lines.filter((line) => line.startsWith("# ")).length !== 1) errors.push(`${id}: exactly one H1 expected`);

      const headings = lines.filter((line) => line.startsWith("## ")).map((line) => line.slice(3).trim());
      if (JSON.stringify(headings) !== JSON.stringify(HEADINGS[locale])) {
        errors.push(`${id}: headings must be [${HEADINGS[locale].join(" | ")}], found [${headings.join(" | ")}]`);
      }

      const diagrams = [...text.matchAll(/```mermaid\n([\s\S]*?)```/g)];
      if (diagrams.length === 0) errors.push(`${id}: a Mermaid diagram is expected in the last section`);
      for (const [, source] of diagrams) {
        const problem = await parseMermaid(source);
        if (problem) errors.push(`${id}: Mermaid parse error: ${problem}`);
      }
    }
  }

  const counts = LOCALES.map((locale) => `${locale}=${helpKeys[locale].size}`).join(" ");
  console.log(`Route help keys: ${routeKeys.size}. Help files: ${counts}.`);
  if (errors.length === 0) {
    console.log("Contextual help audit passed.");
    return;
  }

  console.log(`Errors (${errors.length}):`);
  for (const error of errors.sort()) console.log(`- ${error}`);
  process.exitCode = 1;
};

await main();
