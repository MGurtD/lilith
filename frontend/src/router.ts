import { createWebHistory, createRouter } from "vue-router";
import SharedRoutes from "./modules/shared/routes";
import PurchaseRoutes from "./modules/purchase/routes";
import SalesRoutes from "./modules/sales/routes";
import ProductionRoutes from "./modules/production/routes";
import WarehouseRoutes from "./modules/warehouse/routes";
import ShoopfloorRoutes from "./modules/plant/routes";
import AnalyticsRoutes from "./modules/analytics/routes";
import VerifactuRoutes from "./modules/verifactu/routes";

const Login = () => import("./views/Login.vue");
const Home = () => import("./views/Home.vue");
const Users = () => import("./views/Users.vue");
const User = () => import("./views/User.vue");
const Reports = () => import("./views/Reports.vue");
const MenuItems = () => import("./views/MenuItems.vue");
const MenuItem = () => import("./views/MenuItem.vue");
const Profiles = () => import("./views/Profiles.vue");
const Profile = () => import("./views/Profile.vue");
const ApiKeys = () => import("./views/ApiKeys.vue");

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/login", name: "Login", component: Login, meta: { public: true } },

    { path: "/", name: "Home", component: Home },

    { path: "/users", name: "Users", component: Users, meta: { roles: ["Admin"] } },
    { path: "/user/:id", name: "User", component: User, meta: { roles: ["Admin"] } },
    { path: "/reports", name: "Reports", component: Reports },
    { path: "/menuitems", name: "MenuItems", component: MenuItems, meta: { roles: ["Admin"] } },
    { path: "/menuitem/:id", name: "MenuItem", component: MenuItem, meta: { roles: ["Admin"] } },
    { path: "/profiles", name: "Profiles", component: Profiles, meta: { roles: ["Admin"] } },
    { path: "/profile/:id", name: "Profile", component: Profile, meta: { roles: ["Admin"] } },
    { path: "/apikeys", name: "ApiKeys", component: ApiKeys },
    ...SharedRoutes,
    ...SalesRoutes,
    ...PurchaseRoutes,
    ...ProductionRoutes,
    ...WarehouseRoutes,
    ...ShoopfloorRoutes,
    ...AnalyticsRoutes,
    ...VerifactuRoutes,
  ],
});

// PWA: Redirigir a /plant quan l'app està instal·lada com a PWA
router.beforeEach((to, from, next) => {
  // Detectar si es PWA instalada (múltiples métodos para compatibilidad)
  const isPWAStandalone = window.matchMedia(
    "(display-mode: standalone)"
  ).matches;
  const isPWAiOS = (window.navigator as any).standalone === true;
  const isPWA = isPWAStandalone || isPWAiOS;

  // Si es PWA y el destino es la raíz (/), redirigir a /plant
  // Esto cubre tanto la carga inicial como después del login
  if (isPWA && to.path === "/") {
    console.log("[PWA Router] 🚀 Redirigiendo de / a /plant");
    next({ path: "/plant" });
  } else {
    next();
  }
});

// Auth: Redirigir a /login si l'usuari no està autenticat
router.beforeEach((to, _from, next) => {
  const isPublic = to.meta?.public === true;
  const isAuthenticated = !!localStorage.getItem("temges.authorization");

  if (!isPublic && !isAuthenticated) {
    // Redirigir a login conservant la destinació original
    next({ name: "Login", query: { redirect: to.fullPath } });
  } else if (to.name === "Login" && isAuthenticated) {
    // Ja autenticat, redirigir a l'inici
    next({ path: "/" });
  } else {
    next();
  }
});

// Role guard (soft check — logs warning but does not block navigation)
// To enforce: change console.warn to next({ path: "/unauthorized" }) or next(false)
router.beforeEach((to, _from, next) => {
  const requiredRoles = to.meta?.roles;
  if (!requiredRoles || requiredRoles.length === 0) {
    next();
    return;
  }

  try {
    const raw = localStorage.getItem("temges.authorization");
    if (raw) {
      const auth = JSON.parse(raw);
      if (auth?.token) {
        // Decode JWT payload without importing jwt-decode (lightweight)
        const payload = JSON.parse(atob(auth.token.split(".")[1]));
        const userRole = payload?.role as string | undefined;
        if (userRole && requiredRoles.some((r) => r.toLowerCase() === userRole.toLowerCase())) {
          next();
          return;
        }
      }
    }
  } catch {
    // Ignore decode errors
  }

  // Soft check: log warning but allow navigation
  console.warn(
    `[Router] Ruta "${to.path}" requereix rols [${requiredRoles.join(", ")}] — l'usuari no té el rol adequat.`
  );
  next();
});

export default router;
