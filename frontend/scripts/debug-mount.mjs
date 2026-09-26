import { chromium } from "playwright";

const browser = await chromium.launch({ headless: true, args: ["--no-sandbox"] });
const page = await browser.newPage();

const consoleMsgs = [];
const errors = [];
page.on("console", (m) => consoleMsgs.push({ type: m.type(), text: m.text() }));
page.on("pageerror", (e) => errors.push(e.message));
page.on("requestfailed", (r) => errors.push(`REQ_FAIL: ${r.url()} ${r.failure()?.errorText}`));

console.log("Navigating...");
try {
  await page.goto("http://127.0.0.1:8100", { waitUntil: "domcontentloaded", timeout: 60000 });
  console.log("DOM loaded");
} catch (e) {
  console.log("Goto error:", e.message);
}

await page.waitForTimeout(10000);
const html = await page.content();
console.log("HTML length:", html.length);
console.log("HTML first 500 chars:", html.slice(0, 500));

const appHasChildren = await page.evaluate(() => {
  const el = document.querySelector("#app");
  return el ? { childCount: el.children.length, innerHTML: el.innerHTML.slice(0, 500) } : null;
});
console.log("#app:", JSON.stringify(appHasChildren));

console.log("\n=== console messages ===");
consoleMsgs.slice(0, 30).forEach((m, i) => console.log(`[${i}] [${m.type}] ${m.text.slice(0, 300)}`));

console.log("\n=== errors ===");
errors.slice(0, 20).forEach((e, i) => console.log(`[${i}] ${e.slice(0, 400)}`));

await browser.close();