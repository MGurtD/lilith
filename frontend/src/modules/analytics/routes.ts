import { RouteRecordRaw } from "vue-router";

const IncomesAndExpensesDashboard = () =>
  import("./views/IncomesAndExpensesDashboard.vue");
const CustomerSalesRankingDashboard = () =>
  import("./views/CustomerSalesRankingDashboard.vue");

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
] as Array<RouteRecordRaw>;
