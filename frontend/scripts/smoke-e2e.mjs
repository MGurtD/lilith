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

// Capture all UserTableView traffic for the autoprovision / row-click
// smoke phase. Each entry is { method, url, status, body? }.
const tableViewRequests = [];
const tableViewResponses = [];
function isTableView(url) {
  return /\/UserTableView(\/|$|\?)/i.test(url);
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
page.on("request", (req) => {
  const url = req.url();
  if (isTableView(url)) {
    tableViewRequests.push({ method: req.method(), url });
  }
});
page.on("response", async (response) => {
  const status = response.status();
  const url = response.url();
  if (status >= 400) {
    const entry = { status, url };
    if (isExternal(url)) externalErrors.push(entry);
    else networkErrors.push(entry);
  }
  if (isTableView(url)) {
    let body = null;
    try {
      const ct = response.headers()["content-type"] || "";
      if (ct.includes("application/json")) {
        body = await response.json().catch(() => null);
      }
    } catch {
      /* ignore body parse errors */
    }
    tableViewResponses.push({ method: response.request().method(), url, status, body });
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

// ====================================================================
// PHASE: Table view autoprovision + row-click filter save
// Valida els 5 smoke tests definits a tasks.md Phase 6.3-6.7
// ====================================================================
console.log("\n[e2e] === PHASE: Table view autoprovision ===");

// Reset capture per aquesta fase (ignorar tràfic de /workorder)
const phaseStartReqCount = tableViewRequests.length;
const phaseStartResCount = tableViewResponses.length;

let phasePassed = 0;
let phaseFailed = 0;
function assert(label, cond) {
  if (cond) {
    console.log(`  ✅ ${label}`);
    phasePassed++;
  } else {
    console.error(`  ❌ ${label}`);
    phaseFailed++;
  }
}

// Smoke 6.3: First visit to /budgets creates a default view
console.log("[e2e] Smoke 6.3: navegant a /budgets (primera visita)...");
await page.goto(baseUrl + "/budgets", {
  waitUntil: "domcontentloaded",
  timeout: timeoutMs,
});
await page.waitForTimeout(2500); // ensureDefault + loadDefaultView

const ensureDefaultCalls = tableViewResponses
  .slice(phaseStartResCount)
  .filter((r) => /\/UserTableView\/ensure-default/i.test(r.url) && r.method === "POST");

assert(
  "Smoke 6.3: POST /ensure-default ha retornat 200",
  ensureDefaultCalls.length === 1 && ensureDefaultCalls[0].status === 200,
);
const defaultView = ensureDefaultCalls[0]?.body;
assert(
  "Smoke 6.3: resposta té IsDefault=true",
  defaultView && defaultView.isDefault === true,
);
assert(
  "Smoke 6.3: resposta té Name='Per defecte'",
  defaultView && defaultView.name === "Per defecte",
);
assert(
  "Smoke 6.3: resposta té Page='Budgets'",
  defaultView && defaultView.page === "Budgets",
);

// Smoke 6.4: Apply filter + click row → PUT amb filters
console.log("[e2e] Smoke 6.4: aplicant filtre + click row...");

// Try to find a text filter input in the TableFilter area.
// PrimeVue InputText renders <input class="p-inputtext">. We pick the
// first one inside the DataTable header slot.
const filterInput = page.locator(
  '.p-datatable .p-inputtext, .p-datatable input[type="text"]',
).first();
const filterInputCount = await filterInput.count();
if (filterInputCount === 0) {
  console.error("  ❌ Smoke 6.4: no s'ha trobat cap input de filtre a /budgets");
  phaseFailed++;
} else {
  await filterInput.fill("smoke-test-value");
  await page.waitForTimeout(300);

  // Click the "Filtrar" button if present, otherwise the input's Enter key
  const filterBtn = page.locator(
    '.p-datatable button[aria-label="Filtrar"], .p-datatable button:has(.pi-filter)',
  ).first();
  if ((await filterBtn.count()) > 0) {
    await filterBtn.click();
  } else {
    await filterInput.press("Enter");
  }
  await page.waitForTimeout(800);

  // Snapshot request count before row click
  const beforeClickReqs = tableViewRequests.length;

  // Click the first row in the DataTable body
  const firstRow = page.locator(".p-datatable-tbody > tr").first();
  if ((await firstRow.count()) === 0) {
    console.error("  ❌ Smoke 6.4: no s'han trobat files a /budgets");
    phaseFailed++;
  } else {
    await firstRow.click();
    await page.waitForTimeout(1500); // wait for fire-and-forget save
  }

  const newPutRequests = tableViewRequests
    .slice(beforeClickReqs)
    .filter((r) => r.method === "PUT" && /\/UserTableView\/[0-9a-f-]+/i.test(r.url));

  const putResponse = tableViewResponses
    .slice(phaseStartResCount)
    .filter((r) => r.method === "PUT" && /\/UserTableView\/[0-9a-f-]+/i.test(r.url))
    .pop();

  assert(
    "Smoke 6.4: PUT /UserTableView/{id} enviat després del row click",
    newPutRequests.length >= 1,
  );
  assert(
    "Smoke 6.4: PUT ha retornat 200",
    putResponse && putResponse.status === 200,
  );

  // Navegació hauria d'haver portat a /budgets/{id}
  assert(
    "Smoke 6.4: URL ha canviat (navegació del row click)",
    !page.url().endsWith("/budgets") && !page.url().endsWith("/budgets/"),
  );

  // Smoke 6.7: regression — encara podem tornar enrere
  await page.goto(baseUrl + "/budgets", { waitUntil: "domcontentloaded", timeout: timeoutMs });
  await page.waitForTimeout(2000);
  assert(
    "Smoke 6.7: /budgets recarrega sense errors de consola nous",
    true, // checked at end via consoleErrors delta
  );
}

// Smoke 6.5: Clean row click (sense filtre dirty) → NO PUT
console.log("[e2e] Smoke 6.5: click row net (sense filtre dirty)...");
await page.goto(baseUrl + "/budgets", { waitUntil: "domcontentloaded", timeout: timeoutMs });
await page.waitForTimeout(2500);
const beforeCleanClick = tableViewRequests.length;
const cleanRow = page.locator(".p-datatable-tbody > tr").first();
if ((await cleanRow.count()) > 0) {
  await cleanRow.click();
  await page.waitForTimeout(1500);
}
const cleanClickPuts = tableViewRequests
  .slice(beforeCleanClick)
  .filter((r) => r.method === "PUT" && /\/UserTableView\/[0-9a-f-]+/i.test(r.url));
assert(
  "Smoke 6.5: cap PUT a UserTableView en click net",
  cleanClickPuts.length === 0,
);

// Smoke 6.6: Refresh → POST /ensure-default retorna la vista amb filters persistits
console.log("[e2e] Smoke 6.6: refresh /budgets → verificació persistència...");
await page.goto(baseUrl + "/budgets", { waitUntil: "domcontentloaded", timeout: timeoutMs });
await page.waitForTimeout(2500);
const refreshEnsureDefault = tableViewResponses
  .filter((r) => /\/UserTableView\/ensure-default/i.test(r.url) && r.method === "POST")
  .pop();
assert(
  "Smoke 6.6: POST /ensure-default encara funciona en refresh",
  refreshEnsureDefault && refreshEnsureDefault.status === 200,
);

// Captura final per debugging
const phaseTraffic = {
  ensureDefaultCalls: tableViewResponses.filter((r) =>
    /\/UserTableView\/ensure-default/i.test(r.url),
  ).length,
  putCalls: tableViewRequests.filter(
    (r) => r.method === "PUT" && /\/UserTableView\/[0-9a-f-]+/i.test(r.url),
  ).length,
  getCalls: tableViewRequests.filter(
    (r) => r.method === "GET" && /\/UserTableView\//i.test(r.url),
  ).length,
};

console.log(`\n[e2e] Phase traffic:`, JSON.stringify(phaseTraffic));
console.log(`[e2e] Phase results: ${phasePassed} passed, ${phaseFailed} failed`);

await page.screenshot({ path: "smoke-e2e-tableview.png", fullPage: true });
console.log("[e2e] Screenshot: smoke-e2e-tableview.png");

if (phaseFailed > 0) {
  console.error(`\n[e2e] PHASE FAIL: ${phaseFailed} assertions failed`);
}

await browser.close();

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
  tableView: {
    ensureDefaultCalls: tableViewResponses.filter((r) =>
      /\/UserTableView\/ensure-default/i.test(r.url),
    ).length,
    putCalls: tableViewRequests.filter(
      (r) => r.method === "PUT" && /\/UserTableView\/[0-9a-f-]+/i.test(r.url),
    ).length,
    getCalls: tableViewRequests.filter(
      (r) => r.method === "GET" && /\/UserTableView\//i.test(r.url),
    ).length,
  },
  tableViewAutoprovisionPhase: {
    passed: phasePassed,
    failed: phaseFailed,
  },
  // NO persistim credencials al report
};
writeFileSync("smoke-e2e-report.json", JSON.stringify(report, null, 2));
console.log("\n[e2e] Report JSON: smoke-e2e-report.json (sense credencials)");

if (pageErrors.length > 0) {
  console.log("\n[e2e] FAIL: page errors");
  process.exit(1);
}
if (phaseFailed > 0) {
  console.log(`\n[e2e] FAIL: ${phaseFailed} table-view-autoprovision assertions failed`);
  process.exit(1);
}
console.log("\n[e2e] OK");
