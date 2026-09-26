#!/usr/bin/env node

import fs from "node:fs";
import path from "node:path";
import { createRequire } from "node:module";
import { fileURLToPath, pathToFileURL } from "node:url";

const LOCALES = ["ca", "es", "en"];
const SOURCE_EXTENSIONS = new Set([".vue", ".ts", ".tsx", ".js", ".jsx", ".mjs"]);
const IGNORED_DIRECTORIES = new Set(["node_modules", "dist", "dist-test", "dist-preprod", ".git"]);
const VISIBLE_ATTRIBUTES = ["label", "header", "placeholder", "title", "aria-label", "tooltip", "empty-message", "emptyMessage"];
const VISIBLE_SCRIPT_PROPERTIES = ["message", "summary", "detail", "title", "header", "label", "placeholder"];

function parseArguments(argv) {
  const options = { scopes: [], format: "text", strict: false, repoRoot: undefined };
  for (let index = 0; index < argv.length; index += 1) {
    const argument = argv[index];
    if (argument === "--strict") {
      options.strict = true;
      continue;
    }
    if (["--scope", "--format", "--repo-root"].includes(argument)) {
      const value = argv[index + 1];
      if (!value || value.startsWith("--")) throw new Error("Missing value for " + argument);
      index += 1;
      if (argument === "--scope") options.scopes.push(value);
      if (argument === "--format") options.format = value;
      if (argument === "--repo-root") options.repoRoot = value;
      continue;
    }
    if (argument === "--help" || argument === "-h") {
      options.help = true;
      continue;
    }
    throw new Error("Unknown argument: " + argument);
  }
  if (!new Set(["text", "json"]).has(options.format)) throw new Error("Unsupported format: " + options.format);
  return options;
}

function helpText() {
  return [
    "Usage: node audit-i18n.mjs [options]",
    "",
    "  --scope <path>       File or directory to scan; repeatable",
    "  --format text|json   Output format (default: text)",
    "  --strict             Exit 1 when errors are found",
    "  --repo-root <path>   Override repository root discovery",
  ].join("\n");
}

function discoverRepositoryRoot(explicitRoot) {
  if (explicitRoot) return path.resolve(explicitRoot);
  const starts = [process.cwd(), path.dirname(fileURLToPath(import.meta.url))];
  for (const start of starts) {
    let current = path.resolve(start);
    while (true) {
      if (fs.existsSync(path.join(current, "frontend", "src", "i18n"))) return current;
      const parent = path.dirname(current);
      if (parent === current) break;
      current = parent;
    }
  }
  throw new Error("Could not find a repository containing frontend/src/i18n");
}

function loadTypeScript(repoRoot, explicitPath) {
  const require = createRequire(import.meta.url);
  const candidates = [explicitPath, path.join(repoRoot, "frontend", "node_modules", "typescript"), "typescript"].filter(Boolean);
  for (const candidate of candidates) {
    try {
      return require(candidate);
    } catch {
      // Try the next existing TypeScript installation.
    }
  }
  throw new Error("TypeScript is unavailable. Install frontend dependencies with pnpm first.");
}

function finding(severity, code, message, file, line, key, value) {
  const result = { severity, code, message };
  if (file) result.file = file;
  if (line) result.line = line;
  if (key) result.key = key;
  if (value !== undefined) result.value = value;
  return result;
}

function propertyName(node, ts) {
  if (ts.isIdentifier(node) || ts.isStringLiteral(node) || ts.isNumericLiteral(node)) return node.text;
  throw new Error("Unsupported localization property name");
}

function scalarValue(node, ts) {
  if (ts.isStringLiteral(node) || ts.isNoSubstitutionTemplateLiteral(node)) return node.text;
  throw new Error("Localization values must be strings");
}

function parseObjectLiteral(node, ts, sourceFile, prefix, flattened, errors) {
  const seen = new Set();
  for (const property of node.properties) {
    const line = sourceFile.getLineAndCharacterOfPosition(property.getStart()).line + 1;
    if (!ts.isPropertyAssignment(property)) {
      errors.push(finding("error", "UNSUPPORTED_LOCALE_PROPERTY", "Localization dictionaries may only contain explicit property assignments", sourceFile.fileName, line));
      continue;
    }
    let name;
    try {
      name = propertyName(property.name, ts);
    } catch (error) {
      errors.push(finding("error", "UNSUPPORTED_LOCALE_PROPERTY", error.message, sourceFile.fileName, line));
      continue;
    }
    const key = prefix ? prefix + "." + name : name;
    if (seen.has(name)) {
      errors.push(finding("error", "DUPLICATE_LOCALE_KEY", "Duplicate localization key: " + key, sourceFile.fileName, line, key));
      continue;
    }
    seen.add(name);
    if (ts.isObjectLiteralExpression(property.initializer)) {
      parseObjectLiteral(property.initializer, ts, sourceFile, key, flattened, errors);
      continue;
    }
    try {
      flattened.set(key, scalarValue(property.initializer, ts));
    } catch (error) {
      errors.push(finding("error", "UNSUPPORTED_LOCALE_VALUE", error.message + " (" + key + ")", sourceFile.fileName, line, key));
    }
  }
}

function invalidKeySegments(key) {
  return key.split(".").filter((segment) => !/^[a-z][A-Za-z0-9]*$/.test(segment));
}

function parseLocaleFile(filePath, ts) {
  const sourceText = fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
  const sourceFile = ts.createSourceFile(filePath, sourceText, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
  const declarations = new Map();
  let exportedName;
  for (const statement of sourceFile.statements) {
    if (ts.isVariableStatement(statement)) {
      for (const declaration of statement.declarationList.declarations) {
        if (ts.isIdentifier(declaration.name) && declaration.initializer && ts.isObjectLiteralExpression(declaration.initializer)) {
          declarations.set(declaration.name.text, declaration.initializer);
        }
      }
    }
    if (ts.isExportAssignment(statement) && ts.isIdentifier(statement.expression)) exportedName = statement.expression.text;
  }
  if (!exportedName || !declarations.has(exportedName)) throw new Error("Could not resolve the default localization object in " + filePath);
  const errors = [];
  const values = new Map();
  parseObjectLiteral(declarations.get(exportedName), ts, sourceFile, "", values, errors);
  return { values, errors };
}

function parseStructuredLocaleFile(filePath, ts) {
  const sourceText = fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
  const sourceFile = ts.createSourceFile(filePath, sourceText, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);
  const declarations = new Map();
  let root;
  for (const statement of sourceFile.statements) {
    if (ts.isVariableStatement(statement)) {
      for (const declaration of statement.declarationList.declarations) {
        if (ts.isIdentifier(declaration.name) && declaration.initializer && ts.isObjectLiteralExpression(declaration.initializer)) {
          declarations.set(declaration.name.text, declaration.initializer);
        }
      }
    }
    if (ts.isExportAssignment(statement)) {
      if (ts.isObjectLiteralExpression(statement.expression)) root = statement.expression;
      if (ts.isIdentifier(statement.expression)) root = declarations.get(statement.expression.text);
    }
  }
  if (!root) throw new Error("Could not resolve the default localization object in " + filePath);

  const shapes = new Map();
  const errors = [];
  const walk = (node, prefix) => {
    const seen = new Set();
    for (const property of node.properties) {
      const line = sourceFile.getLineAndCharacterOfPosition(property.getStart()).line + 1;
      if (!ts.isPropertyAssignment(property)) {
        errors.push(finding("error", "UNSUPPORTED_PRIMEVUE_PROPERTY", "PrimeVue dictionaries may only contain explicit property assignments", filePath, line));
        continue;
      }
      let name;
      try {
        name = propertyName(property.name, ts);
      } catch (error) {
        errors.push(finding("error", "UNSUPPORTED_PRIMEVUE_PROPERTY", error.message, filePath, line));
        continue;
      }
      const key = prefix ? prefix + "." + name : name;
      if (seen.has(name)) {
        errors.push(finding("error", "DUPLICATE_PRIMEVUE_KEY", "Duplicate PrimeVue localization key: " + key, filePath, line, key));
        continue;
      }
      seen.add(name);
      const value = property.initializer;
      if (ts.isObjectLiteralExpression(value)) {
        walk(value, key);
      } else if (ts.isArrayLiteralExpression(value)) {
        const elementKinds = [...new Set(value.elements.map((element) => {
          if (ts.isStringLiteral(element) || ts.isNoSubstitutionTemplateLiteral(element)) return "string";
          if (ts.isNumericLiteral(element)) return "number";
          return "unsupported";
        }))].sort();
        shapes.set(key, "array:" + elementKinds.join("|") + ":" + value.elements.length);
      } else if (ts.isStringLiteral(value) || ts.isNoSubstitutionTemplateLiteral(value)) {
        shapes.set(key, "string");
      } else if (ts.isNumericLiteral(value)) {
        shapes.set(key, "number");
      } else if (value.kind === ts.SyntaxKind.TrueKeyword || value.kind === ts.SyntaxKind.FalseKeyword) {
        shapes.set(key, "boolean");
      } else {
        errors.push(finding("error", "UNSUPPORTED_PRIMEVUE_VALUE", "Unsupported PrimeVue localization value: " + key, filePath, line, key));
      }
    }
  };
  walk(root, "");
  return { shapes, errors };
}

function placeholderSet(value) {
  if (typeof value !== "string") return [];
  return [...value.matchAll(/\{([A-Za-z_][\w.-]*|\d+)\}/g)].map((match) => match[1]).sort();
}

function collectSourceFiles(scopePath) {
  const stat = fs.statSync(scopePath);
  if (stat.isFile()) return SOURCE_EXTENSIONS.has(path.extname(scopePath)) ? [scopePath] : [];
  const result = [];
  for (const entry of fs.readdirSync(scopePath, { withFileTypes: true })) {
    if (entry.isDirectory() && IGNORED_DIRECTORIES.has(entry.name)) continue;
    const child = path.join(scopePath, entry.name);
    if (entry.isDirectory()) result.push(...collectSourceFiles(child));
    else if (SOURCE_EXTENSIONS.has(path.extname(entry.name))) result.push(child);
  }
  return result;
}

function lineAt(source, index) {
  return source.slice(0, index).split(/\r?\n/).length;
}

function shouldIgnoreVisibleText(value) {
  const normalized = value.replace(/\s+/g, " ").trim();
  if (!normalized || !/\p{L}/u.test(normalized)) return true;
  if (/^(https?:|\/|\.\/|@\/|[\w-]+\.(vue|ts|js|css|scss|json|svg|png|jpg))/.test(normalized)) return true;
  if (/^[A-Z0-9_:-]+$/.test(normalized) && !normalized.includes(" ")) return true;
  return false;
}

function scanSourceFile(filePath) {
  const source = fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
  const usages = [];
  const warnings = [];
  const usageRegex = /(?:\$t|\bt|i18n\.global\.t)\s*\(\s*(["'])([^"']+)\1/g;
  for (const match of source.matchAll(usageRegex)) {
    usages.push({ key: match[2], file: filePath, line: lineAt(source, match.index) });
  }

  const fallbackRegex = /(?:\$t|\bt|i18n\.global\.t)\s*\([^)]*\)\s*(?:\|\||\?\?)\s*(["'\x60])([^"'\x60\r\n]*)\1/g;
  for (const match of source.matchAll(fallbackRegex)) {
    warnings.push(finding("warning", "LITERAL_TRANSLATION_FALLBACK", "Translation call uses a literal fallback", filePath, lineAt(source, match.index), undefined, match[2].replace(/\s+/g, " ").trim()));
  }

  const propertyRegex = new RegExp("\\b(" + VISIBLE_SCRIPT_PROPERTIES.join("|") + ")\\s*:\\s*([\"'\\x60])([^\"'\\x60\\r\\n]+)\\2", "g");
  for (const match of source.matchAll(propertyRegex)) {
    const value = match[3].trim();
    if (shouldIgnoreVisibleText(value)) continue;
    warnings.push(finding("warning", "HARD_CODED_UI_TEXT", "Possible hardcoded user-facing property: " + match[1], filePath, lineAt(source, match.index), undefined, value));
  }

  if (path.extname(filePath) !== ".vue") return { usages, warnings };
  const templateStart = source.match(/<template(?:\s[^>]*)?>/i);
  const templateEnd = source.lastIndexOf("</template>");
  if (templateStart && templateEnd > templateStart.index) {
    const templateOffset = templateStart.index + templateStart[0].length;
    const templateText = source
      .slice(templateOffset, templateEnd)
      .replace(/<!--[\s\S]*?-->/g, (comment) => comment.replace(/[^\r\n]/g, " "));
    const textRegex = />([^<>{}]+)</g;
    for (const match of templateText.matchAll(textRegex)) {
      const value = match[1].replace(/\s+/g, " ").trim();
      if (shouldIgnoreVisibleText(value)) continue;
      warnings.push(finding("warning", "HARD_CODED_UI_TEXT", "Possible hardcoded template text", filePath, lineAt(source, templateOffset + match.index), undefined, value));
    }

    const attributeNames = VISIBLE_ATTRIBUTES.map((name) => name.replace("-", "\\-")).join("|");
    const attributeRegex = new RegExp("(?<![:\\w-])(" + attributeNames + ")\\s*=\\s*([\"'])([^\"'{}]+)\\2", "g");
    for (const match of templateText.matchAll(attributeRegex)) {
      const value = match[3].trim();
      if (shouldIgnoreVisibleText(value)) continue;
      warnings.push(finding("warning", "HARD_CODED_UI_TEXT", "Possible hardcoded visible attribute: " + match[1], filePath, lineAt(source, templateOffset + match.index), undefined, value));
    }
  }

  return { usages, warnings };
}

function relativeFinding(item, repoRoot) {
  return item.file ? { ...item, file: path.relative(repoRoot, item.file).replaceAll("\\", "/") } : item;
}

export function runAudit({ repoRoot: requestedRoot, scopes = [], typescriptPath } = {}) {
  const repoRoot = discoverRepositoryRoot(requestedRoot);
  const ts = loadTypeScript(repoRoot, typescriptPath);
  const errors = [];
  const warnings = [];
  const dictionaries = {};

  for (const locale of LOCALES) {
    const filePath = path.join(repoRoot, "frontend", "src", "i18n", locale + ".ts");
    if (!fs.existsSync(filePath)) throw new Error("Missing locale file: " + filePath);
    const parsed = parseLocaleFile(filePath, ts);
    dictionaries[locale] = parsed.values;
    errors.push(...parsed.errors);
  }

  const primeVueNames = { ca: "catalan", es: "spanish", en: "english" };
  const primeVueRoot = path.join(repoRoot, "frontend", "src", "i18n", "primevue");
  if (fs.existsSync(primeVueRoot)) {
    const primeVueDictionaries = {};
    for (const locale of LOCALES) {
      const filePath = path.join(primeVueRoot, primeVueNames[locale] + ".ts");
      if (!fs.existsSync(filePath)) {
        errors.push(finding("error", "PRIMEVUE_LOCALE_MISSING", "Missing PrimeVue locale file for " + locale, filePath));
        continue;
      }
      const parsed = parseStructuredLocaleFile(filePath, ts);
      primeVueDictionaries[locale] = parsed.shapes;
      errors.push(...parsed.errors);
    }
    const primeVueKeys = [...new Set(LOCALES.flatMap((locale) => [...(primeVueDictionaries[locale]?.keys() ?? [])]))].sort();
    for (const key of primeVueKeys) {
      const available = LOCALES.filter((locale) => primeVueDictionaries[locale]?.has(key));
      if (available.length !== LOCALES.length) {
        for (const locale of LOCALES.filter((candidate) => !available.includes(candidate))) {
          errors.push(finding("error", "PRIMEVUE_KEY_MISSING", "PrimeVue key is missing from locale " + locale + ": " + key, path.join(primeVueRoot, primeVueNames[locale] + ".ts"), undefined, key));
        }
        continue;
      }
      const shapes = Object.fromEntries(LOCALES.map((locale) => [locale, primeVueDictionaries[locale].get(key)]));
      if (new Set(Object.values(shapes)).size > 1) {
        errors.push(finding("error", "PRIMEVUE_STRUCTURE_MISMATCH", "PrimeVue structure mismatch for " + key + ": " + JSON.stringify(shapes), undefined, undefined, key));
      }
    }
  }

  const allKeys = [...new Set(LOCALES.flatMap((locale) => [...dictionaries[locale].keys()]))].sort();
  const invalidKeyNames = allKeys.filter((key) => invalidKeySegments(key).length > 0);
  const invalidKeysByNamespace = new Map();
  for (const key of invalidKeyNames) {
    const namespace = key.split(".")[0];
    invalidKeysByNamespace.set(namespace, [...(invalidKeysByNamespace.get(namespace) ?? []), key]);
  }
  for (const [namespace, keys] of invalidKeysByNamespace) {
    warnings.push(finding(
      "warning",
      "INVALID_TRANSLATION_KEY_CASE",
      `Translation keys must use English camelCase segments; namespace ${namespace} contains ${keys.length} non-camelCase keys`,
      path.join(repoRoot, "frontend", "src", "i18n", "ca.ts"),
      undefined,
      namespace,
    ));
  }
  for (const key of allKeys) {
    for (const locale of LOCALES) {
      if (!dictionaries[locale].has(key)) {
        errors.push(finding("error", "LOCALE_KEY_MISSING", "Key is missing from locale " + locale + ": " + key, path.join(repoRoot, "frontend", "src", "i18n", locale + ".ts"), undefined, key));
      }
    }
    if (LOCALES.every((locale) => dictionaries[locale].has(key))) {
      const placeholders = Object.fromEntries(LOCALES.map((locale) => [locale, placeholderSet(dictionaries[locale].get(key))]));
      if (new Set(LOCALES.map((locale) => JSON.stringify(placeholders[locale]))).size > 1) {
        errors.push(finding("error", "PLACEHOLDER_MISMATCH", "Placeholder mismatch for " + key + ": " + JSON.stringify(placeholders), undefined, undefined, key));
      }
    }
  }

  const requestedScopes = scopes.length > 0 ? scopes : [path.join("frontend", "src")];
  const resolvedScopes = requestedScopes.map((scope) => path.resolve(repoRoot, scope));
  for (const scope of resolvedScopes) {
    if (!fs.existsSync(scope)) throw new Error("Scope does not exist: " + scope);
  }
  const i18nRoot = path.join(repoRoot, "frontend", "src", "i18n") + path.sep;
  const sourceFiles = [...new Set(resolvedScopes.flatMap(collectSourceFiles))]
    .filter((file) => !file.startsWith(i18nRoot))
    .sort();

  const usages = [];
  for (const file of sourceFiles) {
    const scanned = scanSourceFile(file);
    usages.push(...scanned.usages);
    warnings.push(...scanned.warnings);
  }

  for (const usage of usages) {
    const absent = LOCALES.filter((locale) => !dictionaries[locale].has(usage.key));
    if (absent.length > 0) {
      errors.push(finding("error", "MISSING_TRANSLATION_KEY", "Static reference is missing from locales " + absent.join(", ") + ": " + usage.key, usage.file, usage.line, usage.key));
    }
  }

  const globalScope = resolvedScopes.length === 1 && path.resolve(resolvedScopes[0]) === path.resolve(repoRoot, "frontend", "src");
  if (globalScope) {
    const usedKeys = new Set(usages.map((usage) => usage.key));
    for (const key of allKeys.filter((candidate) => !usedKeys.has(candidate))) {
      warnings.push(finding("warning", "UNUSED_STATIC_KEY", "No static translation reference found; verify dynamic usage before removal", undefined, undefined, key));
    }
  }

  const report = {
    repoRoot,
    scopes: resolvedScopes.map((scope) => path.relative(repoRoot, scope).replaceAll("\\", "/") || "."),
    localeCounts: Object.fromEntries(LOCALES.map((locale) => [locale, dictionaries[locale].size])),
    scannedFiles: sourceFiles.length,
    staticReferences: usages.length,
    invalidKeyNames,
    errors: errors.map((item) => relativeFinding(item, repoRoot)),
    warnings: warnings.map((item) => relativeFinding(item, repoRoot)),
  };
  report.summary = { errors: report.errors.length, warnings: report.warnings.length };
  return report;
}

function printTextReport(report) {
  console.log("Frontend localization audit");
  console.log("Scopes: " + report.scopes.join(", "));
  console.log("Locale keys: " + Object.entries(report.localeCounts).map(([locale, count]) => locale + "=" + count).join(", "));
  console.log("Scanned files: " + report.scannedFiles + "; static references: " + report.staticReferences);
  console.log("Errors: " + report.summary.errors + "; warnings: " + report.summary.warnings);

  const printItems = (title, items) => {
    if (items.length === 0) return;
    console.log("\n" + title + ":");
    const maximum = 50;
    for (const item of items.slice(0, maximum)) {
      const location = item.file ? item.file + (item.line ? ":" + item.line : "") + ": " : "";
      console.log("- [" + item.code + "] " + location + item.message + (item.value ? " — " + JSON.stringify(item.value) : ""));
    }
    if (items.length > maximum) console.log("- ... " + (items.length - maximum) + " additional findings omitted; use --format json for the full report.");
  };
  printItems("Errors", report.errors);
  printItems("Warnings", report.warnings);
}

async function main() {
  try {
    const options = parseArguments(process.argv.slice(2));
    if (options.help) {
      console.log(helpText());
      return;
    }
    const report = runAudit(options);
    if (options.format === "json") console.log(JSON.stringify(report, null, 2));
    else printTextReport(report);
    if (options.strict && report.errors.length > 0) process.exitCode = 1;
  } catch (error) {
    console.error("Localization audit failed: " + (error instanceof Error ? error.message : String(error)));
    process.exitCode = 2;
  }
}

const isEntryPoint = process.argv[1] && pathToFileURL(path.resolve(process.argv[1])).href === import.meta.url;
if (isEntryPoint) await main();

export { parseArguments };
