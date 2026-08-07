import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { runAudit } from "./audit-i18n.mjs";

const TYPESCRIPT_SOURCE = path.resolve("frontend", "node_modules", "typescript");

function localeSource(locale, entries) {
  const lines = Object.entries(entries).map(([key, value]) => "    " + key + ": " + JSON.stringify(value) + ",");
  return "const " + locale + " = { screen: {\n" + lines.join("\n") + "\n  } };\nexport default " + locale + ";\n";
}

function createFixture({ locales, view }) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "lilith-i18n-audit-"));
  const i18n = path.join(root, "frontend", "src", "i18n");
  const views = path.join(root, "frontend", "src", "views");
  fs.mkdirSync(i18n, { recursive: true });
  fs.mkdirSync(views, { recursive: true });
  for (const locale of ["ca", "es", "en"]) {
    fs.writeFileSync(path.join(i18n, locale + ".ts"), localeSource(locale, locales[locale]), "utf8");
  }
  fs.writeFileSync(path.join(views, "Fixture.vue"), view, "utf8");
  return root;
}

function auditFixture(configuration) {
  const root = createFixture(configuration);
  try {
    return runAudit({
      repoRoot: root,
      scopes: ["frontend/src/views/Fixture.vue"],
      typescriptPath: TYPESCRIPT_SOURCE,
    });
  } finally {
    fs.rmSync(root, { recursive: true, force: true });
  }
}

const validLocales = {
  ca: { title: "Hola {name}", save: "Desar" },
  es: { title: "Hola {name}", save: "Guardar" },
  en: { title: "Hello {name}", save: "Save" },
};

test("accepts a complete translated view", () => {
  const view = "<script setup lang=\"ts\">\nimport { useI18n } from \"vue-i18n\";\nconst { t } = useI18n();\n</script>\n<template><h1>{{ t(\"screen.title\", { name: \"Lilith\" }) }}</h1><Button :label=\"t('screen.save')\" /></template>";
  const report = auditFixture({ locales: validLocales, view });
  assert.equal(report.errors.length, 0);
  assert.equal(report.warnings.length, 0);
});

test("reports a locale key missing", () => {
  const report = auditFixture({
    locales: { ...validLocales, en: { save: "Save" } },
    view: "<template><span>{{ $t(\"screen.title\") }}</span></template>",
  });
  assert.ok(report.errors.some((item) => item.code === "LOCALE_KEY_MISSING"));
});

test("reports incompatible placeholders", () => {
  const report = auditFixture({
    locales: { ...validLocales, en: { title: "Hello {user}", save: "Save" } },
    view: "<template><span>{{ $t(\"screen.title\") }}</span></template>",
  });
  assert.ok(report.errors.some((item) => item.code === "PLACEHOLDER_MISMATCH"));
});

test("reports a missing static reference", () => {
  const report = auditFixture({
    locales: validLocales,
    view: "<template><span>{{ $t(\"screen.unknown\") }}</span></template>",
  });
  assert.ok(report.errors.some((item) => item.code === "MISSING_TRANSLATION_KEY"));
});

test("warns about literal fallbacks and hardcoded UI text", () => {
  const report = auditFixture({
    locales: validLocales,
    view: "<template><h1>Crear comanda</h1><Button label=\"Guardar\" />{{ $t(\"screen.save\") || \"Guardar\" }}</template>",
  });
  assert.ok(report.warnings.some((item) => item.code === "LITERAL_TRANSLATION_FALLBACK"));
  assert.ok(report.warnings.some((item) => item.code === "HARD_CODED_UI_TEXT"));
});


test("scans attributes after nested Vue templates", () => {
  const report = auditFixture({
    locales: validLocales,
    view: "<template><template #body><span>Interior</span></template><Button label=\"Exterior\" /></template>",
  });
  const values = report.warnings.filter((item) => item.code === "HARD_CODED_UI_TEXT").map((item) => item.value);
  assert.ok(values.includes("Interior"));
  assert.ok(values.includes("Exterior"));
});
