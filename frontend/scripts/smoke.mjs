// smoke.mjs — Smoke test del frontend Lilith amb Playwright
// Fases:
//   1. Carrega el bundle
//   2. Espera que Vue munti (un element fill dins #app)
//   3. Captura console errors, page errors, network errors
//   4. Fes una screenshot a smoke.png
//   5. Torna un resum + codi de sortida (0=OK, 1=consola, 2=network)

import { chromium } from "playwright";
import { writeFileSync } from "node:fs";

const url = process.env.SMOKE_URL || "http://localhost:4173";
const screenshotPath = process.env.SMOKE_SHOT || "smoke.png";
const timeoutMs = parseInt(process.env.SMOKE_TIMEOUT_MS || "20000", 10);

// Patrons de recursos externs que NO són errors del frontend
const externalPatterns = [
  /^https:\/\/actions\./,
  /^https:\/\/reports\./,
  /^wss?:/,
  /^https:\/\/[^/]*zenith\.ovh/,
  /\/sockjs-node\//,
  /\/ws\b/,
];

function isExternal(url) {
  return externalPatterns.some((re) => re.test(url));
}

const consoleErrors = [];
const pageErrors = [];
const networkErrors = [];
const externalErrors = [];
const interestingRequests = [];

const browser = await chromium.launch({
  headless: true,
  args: ["--no-sandbox", "--disable-setuid-sandbox"],
});
const context = await browser.newContext({
  ignoreHTTPSErrors: true,
  viewport: { width: 1280, height: 800 },
});
const page = await context.newPage();

page.on("console", (msg) => {
  const type = msg.type();
  if (type === "error") {
    consoleErrors.push({ text: msg.text(), location: msg.location() });
  }
});
page.on("pageerror", (err) => {
  pageErrors.push({ name: err.name, message: err.message, stack: err.stack });
});
page.on("response", (response) => {
  const status = response.status();
  const respUrl = response.url();
  if (status === 200 && respUrl.includes("login")) {
    interestingRequests.push(`${status} ${respUrl}`);
  }
  if (status >= 400) {
    const entry = { status, url: respUrl };
    if (isExternal(respUrl)) externalErrors.push(entry);
    else networkErrors.push(entry);
  }
});

console.log(`[smoke] Carregant ${url}...`);
const t0 = Date.now();
const response = await page.goto(url, {
  waitUntil: "domcontentloaded",
  timeout: timeoutMs,
});
console.log(`[smoke] domcontentloaded en ${Date.now() - t0}ms — status:`, response?.status());

// Esperar que Vue munti — un element fill dins #app
try {
  await page.waitForSelector("#app > *", { timeout: timeoutMs });
  console.log("[smoke] Vue munta OK (#app té fills)");
} catch (err) {
  console.error("[smoke] Vue NO ha muntat en", timeoutMs, "ms");
}

// Esperar una mica més per veure console errors reactius
await page.waitForTimeout(2000);

const title = await page.title();
const finalUrl = page.url();
const bodyTextLen = (await page.evaluate(() => document.body?.innerText?.length)) || 0;
const formCount = await page.locator("form").count();
const inputCount = await page.locator("input").count();

await page.screenshot({ path: screenshotPath, fullPage: true });

// Tancar
await browser.close();

// Report resumit
console.log("\n=== RESUM ===");
console.log("Títol:", title);
console.log("URL final:", finalUrl);
console.log("Caràcters visibles:", bodyTextLen);
console.log("Forms:", formCount, "Inputs:", inputCount);
console.log("Console errors:", consoleErrors.length);
consoleErrors.slice(0, 10).forEach((e, i) => console.log(`  [${i}]`, e.text.slice(0, 200)));
console.log("Page errors (uncaught):", pageErrors.length);
pageErrors.slice(0, 10).forEach((e, i) => console.log(`  [${i}]`, e.message.slice(0, 200)));
console.log("Network errors (projecte):", networkErrors.length);
networkErrors.slice(0, 10).forEach((e, i) =>
  console.log(`  [${i}] ${e.status} ${e.url.slice(0, 150)}`),
);
console.log("Network errors (externs):", externalErrors.length);
externalErrors.slice(0, 5).forEach((e, i) =>
  console.log(`  [${i}] ${e.status} ${e.url.slice(0, 100)}`),
);

const report = {
  url,
  title,
  finalUrl,
  bodyTextLen,
  formCount,
  inputCount,
  consoleErrors,
  pageErrors,
  networkErrors,
  externalErrors,
};
writeFileSync("smoke-report.json", JSON.stringify(report, null, 2));

console.log(`\n[smoke] Screenshot: ${screenshotPath}`);
console.log("[smoke] Report JSON: smoke-report.json");

// Sortida
if (pageErrors.length > 0 || consoleErrors.length > 0) {
  console.log("\n[smoke] FAIL: errors de consola/page");
  process.exit(1);
}
if (networkErrors.length > 0) {
  console.log("\n[smoke] FAIL: errors de xarxa del projecte");
  process.exit(2);
}
console.log("\n[smoke] OK");
