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
  {
    path: "/customers",
    name: "Customers",
    component: Customers,
    meta: { helpKey: "sales/customers/list" },
  },
  {
    path: "/customers/:id",
    name: "Customer",
    component: Customer,
    props: true,
    meta: { helpKey: "sales/customers/detail" },
  },
  {
    path: "/sales-invoice",
    name: "SalesInvoices",
    component: SalesInvoices,
    meta: { helpKey: "sales/salesinvoice/list" },
  },
  {
    path: "/sales-invoice/:id",
    name: "SalesInvoice",
    component: SalesInvoice,
    props: true,
    meta: { helpKey: "sales/salesinvoice/detail" },
  },
  {
    path: "/sales/reference",
    name: "References",
    component: References,
    meta: { helpKey: "sales/reference/list" },
  },
  {
    path: "/sales/reference/:id",
    name: "Reference",
    component: Reference,
    meta: { helpKey: "sales/reference/detail" },
  },
  {
    path: "/budget",
    name: "Budgets",
    component: Budgets,
    meta: { helpKey: "sales/budget/list" },
  },
  {
    path: "/budget/:id",
    name: "Budget",
    component: Budget,
    meta: { helpKey: "sales/budget/detail" },
  },
  {
    path: "/salesorder",
    name: "SalesOrders",
    component: SalesOrders,
    meta: { helpKey: "sales/salesorder/list" },
  },
  {
    path: "/salesorder/:id",
    name: "SalesOrder",
    component: SalesOrder,
    meta: { helpKey: "sales/salesorder/detail" },
  },
  {
    path: "/deliverynote",
    name: "DeliveryNotes",
    component: DeliveryNotes,
    meta: { helpKey: "sales/deliverynote/list" },
  },
  {
    path: "/deliverynote/:id",
    name: "DeliveryNote",
    component: DeliveryNote,
    meta: { helpKey: "sales/deliverynote/detail" },
  },
  {
    path: "/salesinvoices-by-period",
    name: "SalesInvoicesByDates",
    component: SalesInvoicesByDates,
    meta: { helpKey: "sales/salesinvoice/by-period" },
  },
] as Array<RouteRecordRaw>;
