import { RouteRecordRaw } from "vue-router";

const Suppliers = () => import("./views/Suppliers.vue");
const Supplier = () => import("./views/Supplier.vue");
const SupplierType = () => import("./views/SupplierType.vue");
const Receipts = () => import("./views/Receipts.vue");
const Receipt = () => import("./views/Receipt.vue");
const Orders = () => import("./views/Orders.vue");
const Order = () => import("./views/Order.vue");
const InvoiceSeries = () => import("./views/InvoiceSeries.vue");
const InvoiceSerie = () => import("./views/InvoiceSerie.vue");
const PurchaseInvoicesByDates = () =>
  import("./views/PurchaseInvoicesByDates.vue");
const PurchaseInvoices = () => import("./views/PurchaseInvoices.vue");
const PurchaseInvoice = () => import("./views/PurchaseInvoice.vue");

const ExpenseType = () => import("./views/ExpenseType.vue");
const ExpenseTypes = () => import("./views/ExpenseTypes.vue");
const Expense = () => import("./views/Expense.vue");
const Expenses = () => import("./views/Expenses.vue");
const ExpenseDashboard = () => import("./views/ExpenseDashboard.vue");
const Materials = () => import("./views/Materials.vue");
const Material = () => import("./views/Material.vue");

  const PhaseToPurchaseOrder = () => import("./views/PhaseToPurchaseOrder.vue");

export default [
  {
    path: "/material",
    name: "Materials",
    component: Materials,
    meta: { helpKey: "purchase/material/list" },
  },
  {
    path: "/material/:id/:category",
    name: "Material",
    component: Material,
    props: true,
    meta: { helpKey: "purchase/material/detail" },
  },
  {
    path: "/suppliers",
    name: "Suppliers",
    component: Suppliers,
    meta: { helpKey: "purchase/supplier/list" },
  },
  {
    path: "/suppliers/:id",
    name: "Supplier",
    component: Supplier,
    props: true,
    meta: { helpKey: "purchase/supplier/detail" },
  },
  {
    path: "/supplier-types/:id",
    name: "SupplierType",
    component: SupplierType,
    props: true,
    meta: { helpKey: "purchase/suppliertype/detail" },
  },
  {
    path: "/purchase-orders",
    name: "PurchaseOrders",
    component: Orders,
    meta: { helpKey: "purchase/order/list" },
  },
  {
    path: "/phase-to-purchase-order",
    name: "PhaseToPurchaseOrder",
    component: PhaseToPurchaseOrder,
    meta: { helpKey: "purchase/phase-to-order/detail" },
  },
  {
    path: "/purchase-orders/:id",
    name: "PurchaseOrder",
    component: Order,
    props: true,
    meta: { helpKey: "purchase/order/detail" },
  },
  {
    path: "/receipts",
    name: "Receipts",
    component: Receipts,
    meta: { helpKey: "purchase/receipt/list" },
  },
  {
    path: "/receipts/:id",
    name: "Receipt",
    component: Receipt,
    props: true,
    meta: { helpKey: "purchase/receipt/detail" },
  },
  {
    path: "/purchaseinvoiceserie",
    name: "PurchaseInvoiceSeries",
    component: InvoiceSeries,
    meta: { helpKey: "purchase/invoiceserie/list" },
  },
  {
    path: "/purchaseinvoiceserie/:id",
    name: "PurchaseInvoiceSerie",
    component: InvoiceSerie,
    props: true,
    meta: { helpKey: "purchase/invoiceserie/detail" },
  },
  {
    path: "/purchaseinvoice/:id",
    name: "PurchaseInvoice",
    component: PurchaseInvoice,
    props: true,
    meta: { helpKey: "purchase/purchaseinvoice/detail" },
  },
  {
    path: "/purchaseinvoice",
    name: "PurchaseInvoices",
    component: PurchaseInvoices,
    props: true,
    meta: { helpKey: "purchase/purchaseinvoice/list" },
  },
  {
    path: "/purchaseinvoices-by-period",
    name: "PurchaseInvoicesByDates",
    component: PurchaseInvoicesByDates,
    meta: { helpKey: "purchase/purchaseinvoice/by-period" },
  },
  {
    path: "/expensetype",
    name: "Expense Types",
    component: ExpenseTypes,
    meta: { helpKey: "purchase/expensetype/list" },
  },
  {
    path: "/expensetype/:id",
    name: "Expense Type",
    component: ExpenseType,
    props: true,
    meta: { helpKey: "purchase/expensetype/detail" },
  },
  {
    path: "/expense",
    name: "Expenses",
    component: Expenses,
    meta: { helpKey: "purchase/expense/list" },
  },
  {
    path: "/expense/:id",
    name: "Expense",
    component: Expense,
    props: true,
    meta: { helpKey: "purchase/expense/detail" },
  },
  {
    path: "/expense-dashboard",
    name: "ExpenseDashboard",
    component: ExpenseDashboard,
    meta: { helpKey: "purchase/expense/dashboard" },
  },
] as Array<RouteRecordRaw>;
