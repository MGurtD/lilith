import fs from "node:fs";
import path from "node:path";

const rootDir = path.resolve(import.meta.dirname, "..");
const localesDir = path.join(rootDir, "src", "i18n");
const localeFiles = {
  ca: path.join(localesDir, "ca.ts"),
  es: path.join(localesDir, "es.ts"),
  en: path.join(localesDir, "en.ts"),
};

function parseLocaleFile(filePath, locale) {
  const source = fs.readFileSync(filePath, "utf8");
  const startToken = `const ${locale} = `;
  const endToken = `export default ${locale}`;
  const start = source.indexOf(startToken);
  const end = source.lastIndexOf(endToken);

  if (start === -1 || end === -1 || end <= start) {
    throw new Error(`Could not parse locale file: ${filePath}`);
  }

  const objectText = source
    .slice(start + startToken.length, end)
    .trim()
    .replace(/;\s*$/, "");

  return new Function(`return (${objectText})`)();
}

function collectLeafPaths(value, currentPath = "") {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    return currentPath ? [currentPath] : [];
  }

  const entries = Object.entries(value);

  if (entries.length === 0) {
    return currentPath ? [currentPath] : [];
  }

  return entries.flatMap(([key, nestedValue]) => {
    const nextPath = currentPath ? `${currentPath}.${key}` : key;
    return collectLeafPaths(nestedValue, nextPath);
  });
}

function difference(basePaths, comparisonPaths) {
  return [...basePaths].filter((key) => !comparisonPaths.has(key)).sort();
}

const localeTrees = Object.fromEntries(
  Object.entries(localeFiles).map(([locale, filePath]) => [
    locale,
    parseLocaleFile(filePath, locale),
  ]),
);

const localePathSets = Object.fromEntries(
  Object.entries(localeTrees).map(([locale, tree]) => [
    locale,
    new Set(collectLeafPaths(tree)),
  ]),
);

const referenceLocale = "ca";
const referencePaths = localePathSets[referenceLocale];
const problems = [];

for (const [locale, paths] of Object.entries(localePathSets)) {
  if (locale === referenceLocale) {
    continue;
  }

  const missingInLocale = difference(referencePaths, paths);
  const extraInLocale = difference(paths, referencePaths);

  if (missingInLocale.length > 0) {
    problems.push(`${locale}: missing ${missingInLocale.length} keys`);
    missingInLocale.forEach((key) => problems.push(`  - ${key}`));
  }

  if (extraInLocale.length > 0) {
    problems.push(`${locale}: extra ${extraInLocale.length} keys`);
    extraInLocale.forEach((key) => problems.push(`  - ${key}`));
  }
}

if (problems.length > 0) {
  console.error("i18n parity check failed:");
  problems.forEach((problem) => console.error(problem));
  process.exit(1);
}

console.log("i18n parity check passed");
