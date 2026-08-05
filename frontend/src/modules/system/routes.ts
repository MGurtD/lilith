import type { RouteRecordRaw } from "vue-router";

export default [
  {
    path: "/system/application-branding",
    name: "ApplicationBranding",
    component: () => import("./views/ApplicationBranding.vue"),
  },
] as Array<RouteRecordRaw>;
