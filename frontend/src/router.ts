import { createWebHistory, createRouter } from "vue-router";
import SharedRoutes from "./modules/shared/routes";
import PurchaseRoutes from "./modules/purchase/routes";
import SalesRoutes from "./modules/sales/routes";
import ProductionRoutes from "./modules/production/routes";
import WarehouseRoutes from "./modules/warehouse/routes";
import ShoopfloorRoutes from "./modules/plant/routes";
import AnalyticsRoutes from "./modules/analytics/routes";
import VerifactuRoutes from "./modules/verifactu/routes";

const Home = () => import("./views/Home.vue");
const Users = () => import("./views/Users.vue");
const User = () => import("./views/User.vue");
const Reports = () => import("./views/Reports.vue");
const MenuItems = () => import("./views/MenuItems.vue");
const MenuItem = () => import("./views/MenuItem.vue");
const Profiles = () => import("./views/Profiles.vue");
const Profile = () => import("./views/Profile.vue");

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", name: "Home", component: Home },

    { path: "/users", name: "Users", component: Users },
    { path: "/user/:id", name: "User", component: User },
    { path: "/reports", name: "Reports", component: Reports },
    { path: "/menuitems", name: "MenuItems", component: MenuItems },
    { path: "/menuitem/:id", name: "MenuItem", component: MenuItem },
    { path: "/profiles", name: "Profiles", component: Profiles },
    { path: "/profile/:id", name: "Profile", component: Profile },
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

// PWA: Redirigir a /plant cuando la app está instalada como PWA
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

export default router;
