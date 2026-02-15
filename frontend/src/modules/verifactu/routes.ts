import { RouteRecordRaw } from "vue-router";

const VerifactuFindInvoices = () => import("./views/VerifactuFindInvoices.vue");
const InvoiceIntegration = () => import("./views/InvoiceIntegration.vue");
const Responsabilities = () => import("./views/Responsabilities.vue");
const InvoiceIntegrationRequests = () =>
  import("./views/InvoiceIntegrationRequests.vue");

export default [
  {
    path: "/verifactu/find-invoices",
    name: "VerifactuFindInvoices",
    component: VerifactuFindInvoices,
  },
  {
    path: "/verifactu/invoice-integration",
    name: "InvoiceIntegration",
    component: InvoiceIntegration,
  },
  {
    path: "/verifactu/integration-requests",
    name: "InvoiceIntegrationRequests",
    component: InvoiceIntegrationRequests,
  },
  {
    path: "/verifactu/responsabilities",
    name: "Responsabilities",
    component: Responsabilities,
  },
] as RouteRecordRaw[];
