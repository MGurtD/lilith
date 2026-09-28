import type { RouteRecordRaw } from "vue-router";

export default [
  {
    path: "/system/application-branding",
    name: "ApplicationBranding",
    component: () => import("./views/ApplicationBranding.vue"),
    meta: { helpKey: "system/branding" },
  },
  {
    path: "/users",
    name: "Users",
    component: () => import("./views/Users.vue"),
    meta: { helpKey: "system/user/list", roles: ["Admin"] },
  },
  {
    path: "/user/:id",
    name: "User",
    component: () => import("./views/User.vue"),
    props: true,
    meta: { helpKey: "system/user/detail", roles: ["Admin"] },
  },
  {
    path: "/reports",
    name: "Reports",
    component: () => import("./views/Reports.vue"),
    meta: { helpKey: "system/reports" },
  },
  {
    path: "/menuitems",
    name: "MenuItems",
    component: () => import("./views/MenuItems.vue"),
    meta: { helpKey: "system/menuitem/list", roles: ["Admin"] },
  },
  {
    path: "/menuitem/:id",
    name: "MenuItem",
    component: () => import("./views/MenuItem.vue"),
    props: true,
    meta: { helpKey: "system/menuitem/detail", roles: ["Admin"] },
  },
  {
    path: "/profiles",
    name: "Profiles",
    component: () => import("./views/Profiles.vue"),
    meta: { helpKey: "system/profile/list", roles: ["Admin"] },
  },
  {
    path: "/profile/:id",
    name: "Profile",
    component: () => import("./views/Profile.vue"),
    props: true,
    meta: { helpKey: "system/profile/detail", roles: ["Admin"] },
  },
  {
    path: "/apikeys",
    name: "ApiKeys",
    component: () => import("./views/ApiKeys.vue"),
    meta: { helpKey: "system/apikey/list" },
  },
  {
    path: "/data-migration",
    name: "DataMigration",
    component: () => import("./views/DataMigration.vue"),
    meta: { helpKey: "system/data-migration", roles: ["Admin"] },
  },
] as Array<RouteRecordRaw>;
