// visual-shots.mjs — screenshots of the running frontend with a mocked API.
//
// Renders real screens (PrimeVue, theme, layout) without a backend, to compare
// visual changes before and after. Start the dev server first, then:
//
//   node scripts/visual-shots.mjs <outDir> /customers /budget/b0 ...
//
// Env options:
//   BASE      app URL (default http://127.0.0.1:5199)
//   W, H      viewport size (default 1440x900; e.g. 390x844 mobile, 1024x768 plant tablet)
//   BRAND     branding palette to mock (blue, orange, emerald, black…); default: no branding
//   CLICK     selector to click before each screenshot (e.g. "#page-actions button")
//   EVAL      JS expression evaluated after each screenshot; the result is printed
//   NAVFROM   open this route first and reach the target with the app router, for
//             screens that need parent state in a store (e.g. /workmaster/x → phase)
//   STACK=1   print page error stacks
//
// Every /api/ call is answered locally. Unknown endpoints get [] (some screens
// then log errors that also happen before any change — compare against a
// baseline run before blaming a change). api-calls.txt lists what was requested.
import { chromium } from "playwright";
import { mkdirSync, writeFileSync } from "node:fs";

const BASE = process.env.BASE || "http://127.0.0.1:5199";
const outDir = process.argv[2] || "shots";
const routes = process.argv.slice(3);
mkdirSync(outDir, { recursive: true });

const b64 = (o) => Buffer.from(JSON.stringify(o)).toString("base64url");
const token = `${b64({ alg: "HS256", typ: "JWT" })}.${b64({
  id: "11111111-1111-1111-1111-111111111111",
  role: "Admin",
  locale: "ca",
  exp: 4102444800,
})}.sig`;

const menu = {
  defaultScreen: "home",
  items: [
    { id: "1", key: "sales", title: "Vendes", icon: "pi pi-shopping-cart", sortOrder: 1, children: [
      { id: "11", key: "customers", title: "Clients", route: "/customers", sortOrder: 1, children: [] },
      { id: "12", key: "budget", title: "Pressupostos", route: "/budget", sortOrder: 2, children: [] },
      { id: "13", key: "salesorder", title: "Comandes", route: "/salesorder", sortOrder: 3, children: [] },
      { id: "14", key: "invoice", title: "Factures", route: "/sales-invoice", sortOrder: 4, children: [] },
    ] },
    { id: "2", key: "purchase", title: "Compres", icon: "pi pi-truck", sortOrder: 2, children: [
      { id: "21", key: "suppliers", title: "Proveïdors", route: "/suppliers", sortOrder: 1, children: [] },
      { id: "22", key: "po", title: "Comandes de compra", route: "/purchase-orders", sortOrder: 2, children: [] },
    ] },
    { id: "3", key: "production", title: "Producció", icon: "pi pi-cog", sortOrder: 3, children: [] },
    { id: "4", key: "warehouse", title: "Magatzem", icon: "pi pi-box", sortOrder: 4, children: [] },
    { id: "5", key: "plant", title: "Planta", icon: "pi pi-building", sortOrder: 5, children: [] },
    { id: "6", key: "system", title: "Sistema", icon: "pi pi-sliders-h", sortOrder: 6, children: [] },
  ],
};

const names = ["Mecanitzats Vallès SL", "Tallers Pujol i Fills", "Indústries Berguedà SA", "Foneria del Ter", "Estampacions Osona", "Calderers Riera SL", "Components Anoia", "Hidràulica Maresme"];
const customers = names.map((n, i) => ({
  id: `c${i}`, code: `CLI${String(i + 1).padStart(4, "0")}`, comercialName: n, taxName: n, vatNumber: `B6${1234560 + i}`,
  phone: "93 8" + (400000 + i * 137), email: `admin@${n.split(" ")[0].toLowerCase()}.cat`, disabled: i === 6,
}));

const statuses = [
  { id: "s1", name: "Esborrany" }, { id: "s2", name: "Emesa" }, { id: "s3", name: "Cobrada" }, { id: "s4", name: "Vençuda" },
];
const amounts = [12486.33, 847.0, 3420.5, 10319.28, 2167.05, 58.9, 1273.95, 24390.0, 612.4, 1111.11];
const invoices = amounts.map((a, i) => ({
  id: `i${i}`, invoiceNumber: `FV-2026/${String(142 - i).padStart(4, "0")}`,
  invoiceDate: `2026-09-${String(14 - i).padStart(2, "0")}T00:00:00`, dueDate: `2026-10-${String(14 - i).padStart(2, "0")}`,
  customerId: `c${i % 8}`, statusId: statuses[[1, 2, 1, 3, 2, 0, 2, 1, 2, 3][i]].id, netAmount: a,
}));
const phase = { id: "p1", code: "20", description: "Torneig CNC", disabled: false, operatorTypeId: null, workcenterTypeId: null, preferredWorkcenterId: null, isExternalWork: false, externalWorkCost: 0, transportCost: 0, serviceReferenceId: null, profitPercentage: 0, details: [], billOfMaterials: [] };

const mocks = [
  [/menu/i, () => menu],
  [/\/User\/[0-9a-f-]{36}$/i, () => ({ id: "11111111-1111-1111-1111-111111111111", username: "marc", firstName: "Marc", lastName: "Gurt", preferredLanguage: "ca", disabled: false })],
  [/\/Customer$/i, () => customers],
  [/\/Customer\/c\d$/i, () => ({ ...customers[6], accountNumber: "43000123", observations: "", invoiceNotes: "" })],
  [/\/Lifecycle\/name\/SalesInvoice$/i, () => ({ id: "lc", name: "SalesInvoice", statuses })],
  [/\/SalesInvoice$/i, () => invoices],
  [/\/Budget\/b0$/i, () => ({ id: "b0", customerId: "c0", exerciseId: "e1", date: "2026-09-10T00:00:00", number: "PRE-2026/0087", deliveryDays: 15, amount: 10319.28, acceptanceDate: null, statusId: "s2", notes: "", totalWeight: 0, details: [], transports: [], externalServices: [], userNotes: "" })],
  [/\/WorkMaster\/x$/i, () => ({ id: "x", code: "RUT-0012", description: "Eix de transmissió", referenceId: null, baseQuantity: 1, mode: 1, disabled: false, phases: [] })],
  [/\/WorkMaster\/Phase\/p1$/i, () => ({ ...phase, workMasterId: "x" })],
  [/\/WorkOrder\/Phase\/p1$/i, () => ({ ...phase, workOrderId: "w1", statusId: null })],
];

const seen = new Set();
const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: +(process.env.W || 1440), height: +(process.env.H || 900) } });
const page = await ctx.newPage();
page.on("pageerror", (e) =>
  console.log("pageerror:", e.message, process.env.STACK ? "\n" + (e.stack || "").split("\n").slice(0, 14).join("\n") : ""),
);

await page.route((url) => url.pathname.startsWith("/api/"), async (route) => {
  const p = new URL(route.request().url()).pathname;
  seen.add(`${route.request().method()} ${p}`);
  const json = (body, status = 200) => route.fulfill({ status, contentType: "application/json", body: JSON.stringify(body) });
  if (/branding/i.test(p)) {
    return process.env.BRAND
      ? json({ brandName: "Temges", primaryColor: process.env.BRAND, hasMainLogo: false, hasSidebarLogo: false, version: "t" })
      : json({}, 404);
  }
  const hit = mocks.find(([re]) => re.test(p));
  return json(hit ? hit[1]() : []);
});

async function shot(name, path) {
  if (process.env.NAVFROM && !["/", "/login"].includes(path)) {
    await page.goto(BASE + process.env.NAVFROM, { waitUntil: "networkidle" }).catch(() => {});
    await page.waitForTimeout(800);
    await page.evaluate((p) => document.querySelector("#app").__vue_app__.config.globalProperties.$router.push(p), path);
    await page.waitForLoadState("networkidle").catch(() => {});
  } else {
    await page.goto(BASE + path, { waitUntil: "networkidle" }).catch(() => {});
  }
  await page.waitForTimeout(900);
  if (process.env.CLICK) {
    await page.click(process.env.CLICK, { timeout: 5000 }).catch((e) => console.log("click:", e.message.split("\n")[0]));
    await page.waitForTimeout(600);
  }
  await page.screenshot({ path: `${outDir}/${name}.png` });
  if (process.env.EVAL) console.log(name, JSON.stringify(await page.evaluate(process.env.EVAL), null, 1));
  console.log("shot", name);
}

await page.goto(BASE + "/login");
await page.evaluate(() => localStorage.clear());
await shot("login", "/login");

await page.evaluate((t) => {
  localStorage.setItem("temges.authorization", JSON.stringify({ token: t, refreshToken: "r" }));
  localStorage.setItem("app.lang", "ca");
}, token);
await shot("home", "/");
for (const r of routes) await shot(r.replace(/[\/:]+/g, "_").replace(/^_/, "") || "root", r);

writeFileSync(`${outDir}/api-calls.txt`, [...seen].sort().join("\n"));
await browser.close();
