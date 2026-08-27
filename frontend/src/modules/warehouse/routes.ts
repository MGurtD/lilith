import { RouteRecordRaw } from "vue-router";

const Warehouse = () => import("./views/Warehouse.vue");
const Warehouses = () => import("./views/Warehouses.vue");
const Stocks = () => import("./views/Stocks.vue");
const StockMovements = () => import("./views/StockMovements.vue");
const Inventory = () => import("./views/Inventory.vue");
const LotTraceability = () => import("./views/LotTraceability.vue");

export default [
  {
    path: "/warehouse/:id",
    name: "Warehouse",
    component: Warehouse,
    meta: { helpKey: "warehouse/warehouse/detail" },
  },
  {
    path: "/warehouse",
    name: "Warehouses",
    component: Warehouses,
    meta: { helpKey: "warehouse/warehouse/list" },
  },
  {
    path: "/stocks",
    name: "Stocks",
    component: Stocks,
    meta: { helpKey: "warehouse/stock/list" },
  },
  {
    path: "/stockmovement",
    name: "stockMovements",
    component: StockMovements,
    meta: { helpKey: "warehouse/stockmovement/list" },
  },
  {
    path: "/inventory",
    name: "inventory",
    component: Inventory,
    meta: { helpKey: "warehouse/inventory/list" },
  },
  {
    path: "/lot-traceability",
    name: "lotTraceability",
    component: LotTraceability,
    meta: { helpKey: "warehouse/lot-traceability/list" },
  },
] as Array<RouteRecordRaw>;
