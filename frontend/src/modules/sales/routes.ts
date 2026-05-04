import { RouteRecordRaw } from "vue-router";

const CustomerType = () => import("./views/CustomerType.vue");
const Customers = () => import("./views/Customers.vue");
const Customer = () => import("./views/Customer.vue");
const Budgets = () => import("./views/Budgets.vue");
const Budget = () => import("./views/Budget.vue");
const SalesOrders = () => import("./views/SalesOrders.vue");
const SalesOrder = () => import("./views/SalesOrder.vue");
const SalesInvoices = () => import("./views/SalesInvoices.vue");
const SalesInvoice = () => import("./views/SalesInvoice.vue");
const DeliveryNotes = () => import("./views/DeliveryNotes.vue");
const DeliveryNote = () => import("./views/DeliveryNote.vue");
const References = () => import("./views/References.vue");
const Reference = () => import("./views/Reference.vue");
const SalesInvoicesByDates = () => import("./views/SalesInvoicesByDates.vue");

export default [
  {
    path: "/customer-types/:id",
    name: "CustomerType",
    component: CustomerType,
    props: true,
  },
  { path: "/customers", name: "Customers", component: Customers },
  {
    path: "/customers/:id",
    name: "Customer",
    component: Customer,
    props: true,
  },
  { path: "/sales-invoice", name: "SalesInvoices", component: SalesInvoices },
  {
    path: "/sales-invoice/:id",
    name: "SalesInvoice",
    component: SalesInvoice,
    props: true,
  },
  { path: "/sales/reference", name: "References", component: References },
  { path: "/sales/reference/:id", name: "Reference", component: Reference },
  { path: "/budget", name: "Budgets", component: Budgets },
  { path: "/budget/:id", name: "Budget", component: Budget },
  { path: "/salesorder", name: "SalesOrders", component: SalesOrders },
  { path: "/salesorder/:id", name: "SalesOrder", component: SalesOrder },
  { path: "/deliverynote", name: "DeliveryNotes", component: DeliveryNotes },
  { path: "/deliverynote/:id", name: "DeliveryNote", component: DeliveryNote },
  {
    path: "/salesinvoices-by-period",
    name: "SalesInvoicesByDates",
    component: SalesInvoicesByDates,
  },
] as Array<RouteRecordRaw>;
