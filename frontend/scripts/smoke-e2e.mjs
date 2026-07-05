// smoke-e2e.mjs — Smoke test end-to-end amb backend .NET
// Credencials via env vars SMOKE_USER i SMOKE_PASS. Mai al codi ni al report.

import { chromium } from "playwright";
import { writeFileSync } from "node:fs";

const baseUrl = process.env.SMOKE_URL || "http://127.0.0.1:4173";
const user = process.env.SMOKE_USER;
const pass = process.env.SMOKE_PASS;
const screenshotPath = process.env.SMOKE_SHOT || "smoke-e2e.png";
const timeoutMs = parseInt(process.env.SMOKE_TIMEOUT_MS || "20000", 10);

if (!user || !pass) {
  console.error("ERROR: SMOKE_USER i SMOKE_PASS requerides via env vars");
  process.exit(2);
}

const consoleErrors = [];
const pageErrors = [];
const networkErrors = [];
const externalErrors = [];
const externalPatterns = [
  /^https:\/\/actions\./,
  /^https:\/\/reports\./,
  /^wss?:/,
  /\/sockjs-node\//,
];

function isExternal(url) {
  return externalPatterns.some((re) => re.test(url));
}

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
  if (msg.type() === "error") {
    consoleErrors.push({ text: msg.text(), location: msg.location() });
  }
});
page.on("pageerror", (err) => {
  pageErrors.push({ name: err.name, message: err.message });
});
page.on("response", (response) => {
  const status = response.status();
  const url = response.url();
  if (status >= 400) {
    const entry = { status, url };
    if (isExternal(url)) externalErrors.push(entry);
    else networkErrors.push(entry);
  }
});

console.log(`[e2e] Carregant ${baseUrl}...`);
await page.goto(baseUrl, { waitUntil: "domcontentloaded", timeout: timeoutMs });
await page.waitForSelector("#app > *", { timeout: timeoutMs });
console.log("[e2e] Vue munta OK");

// Esperar a estar a la pàgina de login
await page.waitForURL(/\/login/, { timeout: timeoutMs });
console.log("[e2e] A la pàgina de login:", page.url());

// Trobar inputs. Provar múltiples estratègies.
const userInput = page.locator(
  'input[type="text"], input[type="email"], input:not([type]), input[name*="user" i], input[name*="email" i]',
).first();
const passInput = page.locator('input[type="password"]').first();
const submitBtn = page.locator('button[type="submit"], button:has-text("Login"), button:has-text("Entrar"), button:has-text("Iniciar")').first();

const userCount = await userInput.count();
const passCount = await passInput.count();
const submitCount = await submitBtn.count();
console.log(`[e2e] Form fields: user=${userCount}, pass=${passCount}, submit=${submitCount}`);

if (userCount === 0 || passCount === 0) {
  await page.screenshot({ path: "smoke-e2e-fail.png", fullPage: true });
  const html = await page.content();
  writeFileSync("smoke-e2e-fail.html", html);
  console.error("[e2e] FAIL: inputs no trobats. HTML desat per debug.");
  await browser.close();
  process.exit(3);
}

// Omplir i submit
await userInput.fill(user);
await passInput.fill(pass);
console.log("[e2e] Credencials omplertes, fent submit...");
const tSubmit = Date.now();
await submitBtn.click();

// Esperar navegació (URL canvia des de /login)
try {
  await page.waitForURL(
    (url) => !url.toString().includes("/login"),
    { timeout: timeoutMs },
  );
  console.log(`[e2e] Login exitós en ${Date.now() - tSubmit}ms — URL: ${page.url()}`);
} catch (err) {
  await page.screenshot({ path: "smoke-e2e-fail.png", fullPage: true });
  console.error("[e2e] FAIL: no hem sortit de /login en", timeoutMs, "ms");
  console.error("[e2e] URL actual:", page.url());
  const bodyText = (await page.evaluate(() => document.body?.innerText)) || "";
  console.error("[e2e] Text visible (últims 300 chars):", bodyText.slice(-300));
  await browser.close();
  process.exit(4);
}

await page.waitForTimeout(2000);
await page.screenshot({ path: screenshotPath, fullPage: true });
console.log(`[e2e] Screenshot post-login: ${screenshotPath}`);

// Inspeccionar el que hi ha
const title = await page.title();
const finalUrl = page.url();
const bodyTextLen = (await page.evaluate(() => document.body?.innerText?.length)) || 0;
const bodyText = (await page.evaluate(() => document.body?.innerText)) || "";
console.log(`[e2e] Title: ${title}`);
console.log(`[e2e] URL: ${finalUrl}`);
console.log(`[e2e] Caràcters visibles: ${bodyTextLen}`);
console.log(`[e2e] Primers 300 chars: ${bodyText.slice(0, 300).replace(/\s+/g, " ").trim()}`);

// Navegar a una ruta autenticada (workorder és un mòdul central)
console.log("[e2e] Navegant a /workorder per validar ruta autenticada...");
const tNav = Date.now();
try {
  await page.goto(baseUrl + "/workorder", {
    waitUntil: "domcontentloaded",
    timeout: timeoutMs,
  });
  await page.waitForTimeout(1500);
  const woUrl = page.url();
  const woTitle = await page.title();
  const woTextLen = (await page.evaluate(() => document.body?.innerText?.length)) || 0;
  console.log(`[e2e] /workorder en ${Date.now() - tNav}ms — URL: ${woUrl}, text=${woTextLen} chars`);
  await page.screenshot({ path: "smoke-e2e-workorder.png", fullPage: true });
} catch (err) {
  console.error("[e2e] WARN: /workorder no ha carregat:", err.message);
}

await browser.close();

console.log("\n=== RESUM E2E ===");
console.log("Console errors:", consoleErrors.length);
consoleErrors.slice(0, 10).forEach((e, i) => console.log(`  [${i}]`, e.text.slice(0, 200)));
console.log("Page errors:", pageErrors.length);
pageErrors.slice(0, 10).forEach((e, i) => console.log(`  [${i}]`, e.message.slice(0, 200)));
console.log("Network errors (projecte):", networkErrors.length);
networkErrors.slice(0, 10).forEach((e, i) => console.log(`  [${i}] ${e.status} ${e.url.slice(0, 150)}`));
console.log("Network errors (externs):", externalErrors.length);

const report = {
  baseUrl,
  title,
  finalUrl,
  bodyTextLen,
  consoleErrors,
  pageErrors,
  networkErrors,
  externalErrors,
  // NO persistim credencials al report
};
writeFileSync("smoke-e2e-report.json", JSON.stringify(report, null, 2));
console.log("\n[e2e] Report JSON: smoke-e2e-report.json (sense credencials)");

if (pageErrors.length > 0) {
  console.log("\n[e2e] FAIL: page errors");
  process.exit(1);
}
console.log("\n[e2e] OK");
