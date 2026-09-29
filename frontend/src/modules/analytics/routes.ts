import { RouteRecordRaw } from "vue-router";

const IncomesAndExpensesDashboard = () =>
  import("./views/IncomesAndExpensesDashboard.vue");
const CustomerSalesRankingDashboard = () =>
  import("./views/CustomerSalesRankingDashboard.vue");
const BudgetConversionDashboard = () =>
  import("./views/BudgetConversionDashboard.vue");
const ProductionTimeDeviationDashboard = () =>
  import("./views/ProductionTimeDeviationDashboard.vue");
const AbcDashboard = () => import("./views/AbcDashboard.vue");
const ManagementDashboard = () => import("./views/ManagementDashboard.vue");

export default [
  {
    path: "/incomesandexpensesdashboard",
    name: "IncomesAndExpensesDashboard",
    component: IncomesAndExpensesDashboard,
    props: true,
    meta: { helpKey: "analytics/incomes-expenses" },
  },
  {
    path: "/customer-ranking",
    name: "CustomerSalesRankingDashboard",
    component: CustomerSalesRankingDashboard,
    props: true,
    meta: { helpKey: "analytics/customer-ranking" },
  },
  {
    path: "/budget-conversion",
    name: "BudgetConversionDashboard",
    component: BudgetConversionDashboard,
    props: true,
    meta: { helpKey: "analytics/budget-conversion" },
  },
  {
    path: "/production-time-deviation",
    name: "ProductionTimeDeviationDashboard",
    component: ProductionTimeDeviationDashboard,
    props: true,
    meta: { helpKey: "analytics/production-time-deviation" },
  },
  {
    path: "/abc-customers",
    name: "CustomerAbcDashboard",
    component: AbcDashboard,
    props: { mode: "customer" },
    meta: { helpKey: "analytics/abc/customers" },
  },
  {
    path: "/abc-suppliers",
    name: "SupplierAbcDashboard",
    component: AbcDashboard,
    props: { mode: "supplier" },
    meta: { helpKey: "analytics/abc/suppliers" },
  },
  {
    path: "/management-dashboard",
    name: "ManagementDashboard",
    component: ManagementDashboard,
    props: true,
    meta: { helpKey: "analytics/management" },
  },
] as Array<RouteRecordRaw>;
