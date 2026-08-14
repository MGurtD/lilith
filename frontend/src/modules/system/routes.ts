import type { RouteRecordRaw } from "vue-router";

export default [
  {
    path: "/system/application-branding",
    name: "ApplicationBranding",
    component: () => import("./views/ApplicationBranding.vue"),
  },
  {
    path: "/users",
    name: "Users",
    component: () => import("./views/Users.vue"),
    meta: { roles: ["Admin"] },
  },
  {
    path: "/user/:id",
    name: "User",
    component: () => import("./views/User.vue"),
    props: true,
    meta: { roles: ["Admin"] },
  },
  {
    path: "/reports",
    name: "Reports",
    component: () => import("./views/Reports.vue"),
  },
  {
    path: "/menuitems",
    name: "MenuItems",
    component: () => import("./views/MenuItems.vue"),
    meta: { roles: ["Admin"] },
  },
  {
    path: "/menuitem/:id",
    name: "MenuItem",
    component: () => import("./views/MenuItem.vue"),
    props: true,
    meta: { roles: ["Admin"] },
  },
  {
    path: "/profiles",
    name: "Profiles",
    component: () => import("./views/Profiles.vue"),
    meta: { roles: ["Admin"] },
  },
  {
    path: "/profile/:id",
    name: "Profile",
    component: () => import("./views/Profile.vue"),
    props: true,
    meta: { roles: ["Admin"] },
  },
  {
    path: "/apikeys",
    name: "ApiKeys",
    component: () => import("./views/ApiKeys.vue"),
  },
] as Array<RouteRecordRaw>;
