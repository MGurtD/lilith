import assert from "node:assert/strict";
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import { runAudit } from "./audit-i18n.mjs";

const TYPESCRIPT_SOURCE = path.resolve("node_modules", "typescript");

function localeSource(locale, entries) {
  const lines = Object.entries(entries).map(([key, value]) => "    " + JSON.stringify(key) + ": " + JSON.stringify(value) + ",");
  return "const " + locale + " = { screen: {\n" + lines.join("\n") + "\n  } };\nexport default " + locale + ";\n";
}

function createFixture({ locales, view, script, primevue }) {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), "lilith-i18n-audit-"));
  const i18n = path.join(root, "frontend", "src", "i18n");
  const views = path.join(root, "frontend", "src", "views");
  fs.mkdirSync(i18n, { recursive: true });
  fs.mkdirSync(views, { recursive: true });
  for (const locale of ["ca", "es", "en"]) {
    fs.writeFileSync(path.join(i18n, locale + ".ts"), localeSource(locale, locales[locale]), "utf8");
  }
  if (primevue) {
    const primevueRoot = path.join(i18n, "primevue");
    fs.mkdirSync(primevueRoot, { recursive: true });
    for (const [name, source] of Object.entries(primevue)) {
      fs.writeFileSync(path.join(primevueRoot, name + ".ts"), source, "utf8");
    }
  }
  fs.writeFileSync(path.join(views, "Fixture.vue"), view, "utf8");
  if (script) fs.writeFileSync(path.join(views, "fixture.ts"), script, "utf8");
  return root;
}

function auditFixture(configuration) {
  const root = createFixture(configuration);
  try {
    return runAudit({
      repoRoot: root,
      scopes: configuration.script
        ? ["frontend/src/views"]
        : ["frontend/src/views/Fixture.vue"],
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

test("reports incompatible PrimeVue locale structures", () => {
  const source = (days) => `export default { clear: "Clear", dayNames: ${JSON.stringify(days)}, firstDayOfWeek: 1 };`;
  const report = auditFixture({
    locales: validLocales,
    view: "<template><span>{{ $t('screen.title') }}</span></template>",
    primevue: {
      catalan: source(["Diumenge", "Dilluns"]),
      spanish: source(["Domingo"]),
      english: source(["Sunday", "Monday"]),
    },
  });
  assert.ok(report.errors.some((item) => item.code === "PRIMEVUE_STRUCTURE_MISMATCH"));
});

test("reports a missing static reference", () => {
  const report = auditFixture({
    locales: validLocales,
    view: "<template><span>{{ $t(\"screen.unknown\") }}</span></template>",
  });
  assert.ok(report.errors.some((item) => item.code === "MISSING_TRANSLATION_KEY"));
});

test("warns about translation keys that are not camelCase", () => {
  const locales = {
    ca: { ...validLocales.ca, "desa-canvis": "Desa els canvis" },
    es: { ...validLocales.es, "desa-canvis": "Guarda los cambios" },
    en: { ...validLocales.en, "desa-canvis": "Save changes" },
  };
  const report = auditFixture({
    locales,
    view: "<template><span>{{ $t(\"screen.desa-canvis\") }}</span></template>",
  });
  const warning = report.warnings.find((item) => item.code === "INVALID_TRANSLATION_KEY_CASE");
  assert.equal(report.errors.length, 0);
  assert.equal(warning.key, "screen");
  assert.deepEqual(report.invalidKeyNames, ["screen.desa-canvis"]);
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

test("scans hardcoded user-facing properties in TypeScript files", () => {
  const report = auditFixture({
    locales: validLocales,
    view: "<template><span>{{ $t('screen.title') }}</span></template>",
    script: "toast.add({ summary: 'No s ha pogut desar' });",
  });
  assert.ok(report.warnings.some((item) => item.file.endsWith("fixture.ts") && item.code === "HARD_CODED_UI_TEXT"));
});

test("preserves source line numbers when template comments are removed", () => {
  const report = auditFixture({
    locales: validLocales,
    view: "<template>\n<!-- comment\ncontinued -->\n<Button label=\"Guardar\" />\n</template>",
  });
  const warning = report.warnings.find((item) => item.value === "Guardar");
  assert.equal(warning.line, 4);
});
