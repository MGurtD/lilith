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
    meta: { helpKey: "verifactu/find-invoices" },
  },
  {
    path: "/verifactu/invoice-integration",
    name: "InvoiceIntegration",
    component: InvoiceIntegration,
    meta: { helpKey: "verifactu/invoice-integration" },
  },
  {
    path: "/verifactu/integration-requests",
    name: "InvoiceIntegrationRequests",
    component: InvoiceIntegrationRequests,
    meta: { helpKey: "verifactu/integration-requests" },
  },
  {
    path: "/verifactu/responsabilities",
    name: "Responsabilities",
    component: Responsabilities,
    meta: { helpKey: "verifactu/responsabilities" },
  },
] as RouteRecordRaw[];
