import { RouteRecordRaw } from "vue-router";

const PaymentMethods = () => import("./views/PaymentMethods.vue");
const PaymentMethod = () => import("./views/PaymentMethod.vue");
const Exercise = () => import("./views/Exercise.vue");
const Exercises = () => import("./views/Exercises.vue");
const Taxes = () => import("./views/Taxes.vue");
const Tax = () => import("./views/Tax.vue");
const Lifecycles = () => import("./views/Lifecycles.vue");
const Lifecycle = () => import("./views/Lifecycle.vue");
const ReferenceTypes = () => import("./views/ReferenceTypes.vue");
const ReferenceType = () => import("./views/ReferenceType.vue");
const ReferenceManagementList = () =>
  import("./views/ReferenceManagementList.vue");
const ReferenceManagement = () => import("./views/ReferenceManagement.vue");

export default [
  {
    path: "/payment-methods",
    name: "PaymentMethods",
    component: PaymentMethods,
    meta: { helpKey: "shared/paymentmethod/list" },
  },
  {
    path: "/payment-methods/:id",
    name: "PaymentMethod",
    component: PaymentMethod,
    props: true,
    meta: { helpKey: "shared/paymentmethod/detail" },
  },
  {
    path: "/exercise",
    name: "Exercises",
    component: Exercises,
    meta: { helpKey: "shared/exercise/list" },
  },
  {
    path: "/exercise/:id",
    name: "Exercise",
    component: Exercise,
    props: true,
    meta: { helpKey: "shared/exercise/detail" },
  },
  {
    path: "/taxes",
    name: "Taxes",
    component: Taxes,
    meta: { helpKey: "shared/tax/list" },
  },
  {
    path: "/tax/:id",
    name: "Tax",
    component: Tax,
    props: true,
    meta: { helpKey: "shared/tax/detail" },
  },
  {
    path: "/lifecycle",
    name: "Lifecycles",
    component: Lifecycles,
    meta: { helpKey: "shared/lifecycle/list" },
  },
  {
    path: "/lifecycle/:id",
    name: "Lifecycle",
    component: Lifecycle,
    props: true,
    meta: { helpKey: "shared/lifecycle/detail" },
  },
  {
    path: "/referencetype",
    name: "ReferenceTypes",
    component: ReferenceTypes,
    meta: { helpKey: "shared/referencetype/list" },
  },
  {
    path: "/referencetype/:id",
    name: "ReferenceType",
    component: ReferenceType,
    meta: { helpKey: "shared/referencetype/detail" },
  },
  {
    path: "/reference-management",
    name: "ReferenceManagementList",
    component: ReferenceManagementList,
    meta: { helpKey: "shared/reference/list" },
  },
  {
    path: "/reference-management/:id",
    name: "ReferenceManagement",
    component: ReferenceManagement,
    props: true,
    meta: { helpKey: "shared/reference/detail" },
  },
] as Array<RouteRecordRaw>;
