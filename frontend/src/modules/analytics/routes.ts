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
  },
  {
    path: "/customer-ranking",
    name: "CustomerSalesRankingDashboard",
    component: CustomerSalesRankingDashboard,
    props: true,
  },
  {
    path: "/budget-conversion",
    name: "BudgetConversionDashboard",
    component: BudgetConversionDashboard,
    props: true,
  },
  {
    path: "/production-time-deviation",
    name: "ProductionTimeDeviationDashboard",
    component: ProductionTimeDeviationDashboard,
    props: true,
  },
  {
    path: "/abc-customers",
    name: "CustomerAbcDashboard",
    component: AbcDashboard,
    props: { mode: "customer" },
  },
  {
    path: "/abc-suppliers",
    name: "SupplierAbcDashboard",
    component: AbcDashboard,
    props: { mode: "supplier" },
  },
  {
    path: "/management-dashboard",
    name: "ManagementDashboard",
    component: ManagementDashboard,
    props: true,
  },
] as Array<RouteRecordRaw>;
