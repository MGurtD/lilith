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
  { path: "/material", name: "Materials", component: Materials },
  { path: "/material/:id/:category", name: "Material", component: Material },
  { path: "/suppliers", name: "Suppliers", component: Suppliers },
  {
    path: "/suppliers/:id",
    name: "Supplier",
    component: Supplier,
    props: true,
  },
  {
    path: "/supplier-types/:id",
    name: "SupplierType",
    component: SupplierType,
    props: true,
  },
  {
    path: "/purchase-orders",
    name: "PurchaseOrders",
    component: Orders,
  },
  {
    path: "/phase-to-purchase-order",
    name: "PhaseToPurchaseOrder",
    component: PhaseToPurchaseOrder,
  },
  {
    path: "/purchase-orders/:id",
    name: "PurchaseOrder",
    component: Order,
    props: true,
  },
  {
    path: "/receipts",
    name: "Receipts",
    component: Receipts,
  },
  {
    path: "/receipts/:id",
    name: "Receipt",
    component: Receipt,
    props: true,
  },
  {
    path: "/purchaseinvoiceserie",
    name: "PurchaseInvoiceSeries",
    component: InvoiceSeries,
  },
  {
    path: "/purchaseinvoiceserie/:id",
    name: "PurchaseInvoiceSerie",
    component: InvoiceSerie,
    props: true,
  },
  {
    path: "/purchaseinvoice/:id",
    name: "PurchaseInvoice",
    component: PurchaseInvoice,
    props: true,
  },
  {
    path: "/purchaseinvoice",
    name: "PurchaseInvoices",
    component: PurchaseInvoices,
    props: true,
  },
  {
    path: "/purchaseinvoices-by-period",
    name: "PurchaseInvoicesByDates",
    component: PurchaseInvoicesByDates,
  },
  {
    path: "/expensetype",
    name: "Expense Types",
    component: ExpenseTypes,
  },
  {
    path: "/expensetype/:id",
    name: "Expense Type",
    component: ExpenseType,
    props: true,
  },
  {
    path: "/expense",
    name: "Expenses",
    component: Expenses,
  },
  {
    path: "/expense/:id",
    name: "Expense",
    component: Expense,
    props: true,
  },
  {
    path: "/expense-dashboard",
    name: "ExpenseDashboard",
    component: ExpenseDashboard,
  },
] as Array<RouteRecordRaw>;
