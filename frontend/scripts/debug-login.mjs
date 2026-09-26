import { chromium } from "playwright";

const browser = await chromium.launch({
  headless: true,
  args: ["--no-sandbox", "--disable-setuid-sandbox"],
});
const context = await browser.newContext({ ignoreHTTPSErrors: true });
const page = await context.newPage();

const consoleAll = [];
const pageErrors = [];
const requests = [];

page.on("console", (msg) =>
  consoleAll.push({ type: msg.type(), text: msg.text() }),
);
page.on("pageerror", (err) => pageErrors.push(err.message));
page.on("request", (req) => {
  if (req.url().includes("7284") || req.url().includes("/api/")) {
    requests.push({ method: req.method(), url: req.url() });
  }
});
page.on("response", (res) => {
  if (res.url().includes("7284") || res.url().includes("/api/")) {
    requests.push({ method: res.request().method(), url: res.url(), status: res.status() });
  }
});

await page.goto("http://localhost:8100", { waitUntil: "domcontentloaded" });
await page.waitForSelector("#app > *");

const userInput = page.locator('input[type="text"], input[type="email"], input:not([type])').first();
const passInput = page.locator('input[type="password"]').first();
const submit = page.locator('button[type="submit"], button:has-text("Iniciar")').first();

await userInput.fill("marcgurt");
await passInput.fill("Carretera@1");
console.log("Before submit, requests:", requests.length);
await submit.click();
await page.waitForTimeout(5000);
console.log("After submit, requests:", requests.length);
console.log("URL:", page.url());

console.log("\n=== Requests to backend ===");
requests.forEach((r, i) => console.log(`  [${i}] ${r.method} ${r.url}${r.status ? " → " + r.status : ""}`));

console.log("\n=== Page errors ===");
pageErrors.forEach((e, i) => console.log(`  [${i}] ${e}`));

console.log("\n=== Console errors/warnings ===");
consoleAll.filter((m) => m.type === "error" || m.type === "warning").forEach((m, i) => console.log(`  [${i}] [${m.type}] ${m.text.slice(0, 300)}`));

await browser.close();