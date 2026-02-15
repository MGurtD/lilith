import { RouteRecordRaw } from "vue-router";

const Warehouse = () => import("./views/Warehouse.vue");
const Warehouses = () => import("./views/Warehouses.vue");
const Stocks = () => import("./views/Stocks.vue");
const StockMovements = () => import("./views/StockMovements.vue");
const Inventory = () => import("./views/Inventory.vue");

export default [
  {
    path: "/warehouse/:id",
    name: "Warehouse",
    component: Warehouse,
  },
  {
    path: "/warehouse",
    name: "Warehouses",
    component: Warehouses,
  },
  {
    path: "/stocks",
    name: "Stocks",
    component: Stocks,
  },
  {
    path: "/stockmovement",
    name: "stockMovements",
    component: StockMovements,
  },
  {
    path: "/inventory",
    name: "inventory",
    component: Inventory,
  },
] as Array<RouteRecordRaw>;
