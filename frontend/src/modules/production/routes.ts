import { RouteRecordRaw } from "vue-router";

const Enterprises = () => import("./views/Enterprises.vue");
const Enterprise = () => import("./views/Enterprise.vue");
const Sites = () => import("./views/Sites.vue");
const Site = () => import("./views/Site.vue");
const Areas = () => import("./views/Areas.vue");
const Area = () => import("./views/Area.vue");
const WorkcenterTypes = () => import("./views/WorkcenterTypes.vue");
const WorkcenterType = () => import("./views/WorkcenterType.vue");
const Workcenters = () => import("./views/Workcenters.vue");
const Workcenter = () => import("./views/Workcenter.vue");
const Workcentercosts = () => import("./views/WorkcenterCosts.vue");
const Workcentercost = () => import("./views/WorkcenterCost.vue");
const MachineStatuses = () => import("./views/MachineStatuses.vue");
const MachineStatus = () => import("./views/MachineStatus.vue");
const Operators = () => import("./views/Operators.vue");
const Operator = () => import("./views/Operator.vue");
const OperatorTypes = () => import("./views/OperatorTypes.vue");
const OperatorType = () => import("./views/OperatorType.vue");
const RejectionReasons = () => import("./views/RejectionReasons.vue");
const RejectionReason = () => import("./views/RejectionReason.vue");

const Workmasters = () => import("./views/Workmasters.vue");
const Workmaster = () => import("./views/Workmaster.vue");
const WorkmasterPhase = () => import("./views/WorkmasterPhase.vue");

const Workorders = () => import("./views/Workorders.vue");
const Workorder = () => import("./views/Workorder.vue");
const WorkorderPhase = () => import("./views/WorkorderPhase.vue");

const ProductionParts = () => import("./views/ProductionParts.vue");

const Shifts = () => import("./views/Shifts.vue");

const CostDashboard = () => import("./views/CostDashboard.vue");
const ProductionDashboard = () => import("./views/ProductionDashboard.vue");

const PhaseTemplates = () => import("./views/PhaseTemplates.vue");
const PhaseTemplate = () => import("./views/PhaseTemplate.vue");

const WorkcenterShift = () => import("./views/WorkcenterShift.vue");
const WorkorderPlanning = () => import("./views/WorkorderPlanning.vue");
const WorkcenterSaturation = () => import("./views/WorkcenterSaturation.vue");

export default [
  {
    path: "/enterprise/:id",
    name: "Enterprise",
    component: Enterprise,
    meta: { helpKey: "production/enterprise/detail" },
  },
  {
    path: "/enterprise",
    name: "Enterprises",
    component: Enterprises,
    meta: { helpKey: "production/enterprise/list" },
  },
  {
    path: "/site/:id",
    name: "Site",
    component: Site,
    meta: { helpKey: "production/site/detail" },
  },
  {
    path: "/site",
    name: "Sites",
    component: Sites,
    meta: { helpKey: "production/site/list" },
  },
  {
    path: "/area/:id",
    name: "Area",
    component: Area,
    meta: { helpKey: "production/area/detail" },
  },
  {
    path: "/area",
    name: "Areas",
    component: Areas,
    meta: { helpKey: "production/area/list" },
  },
  {
    path: "/workcentertype/:id",
    name: "WorkcenterType",
    component: WorkcenterType,
    meta: { helpKey: "production/workcentertype/detail" },
  },
  {
    path: "/workcentertype",
    name: "WorkcenterTypes",
    component: WorkcenterTypes,
    meta: { helpKey: "production/workcentertype/list" },
  },
  {
    path: "/workcenter/:id",
    name: "Workcenter",
    component: Workcenter,
    meta: { helpKey: "production/workcenter/detail" },
  },
  {
    path: "/workcenter",
    name: "Workcenters",
    component: Workcenters,
    meta: { helpKey: "production/workcenter/list" },
  },
  {
    path: "/workcentercost/:id",
    name: "Workcentercost",
    component: Workcentercost,
    meta: { helpKey: "production/workcentercost/detail" },
  },
  {
    path: "/workcentercost",
    name: "Workcentercosts",
    component: Workcentercosts,
    meta: { helpKey: "production/workcentercost/list" },
  },
  {
    path: "/operatortype/:id",
    name: "Operatortype",
    component: OperatorType,
    meta: { helpKey: "production/operatortype/detail" },
  },
  {
    path: "/operatortype",
    name: "Operatortypes",
    component: OperatorTypes,
    meta: { helpKey: "production/operatortype/list" },
  },
  {
    path: "/rejectionreason/:id",
    name: "Rejectionreason",
    component: RejectionReason,
    meta: { helpKey: "production/rejectionreason/detail" },
  },
  {
    path: "/rejectionreason",
    name: "Rejectionreasons",
    component: RejectionReasons,
    meta: { helpKey: "production/rejectionreason/list" },
  },
  {
    path: "/operator/:id",
    name: "Operator",
    component: Operator,
    meta: { helpKey: "production/operator/detail" },
  },
  {
    path: "/operator",
    name: "Operators",
    component: Operators,
    meta: { helpKey: "production/operator/list" },
  },
  {
    path: "/machinestatus/:id",
    name: "MachineStatus",
    component: MachineStatus,
    meta: { helpKey: "production/machinestatus/detail" },
  },
  {
    path: "/machinestatus",
    name: "MachineStatuses",
    component: MachineStatuses,
    meta: { helpKey: "production/machinestatus/list" },
  },
  {
    path: "/workmaster",
    name: "Workmasters",
    component: Workmasters,
    meta: { helpKey: "production/workmaster/list" },
  },
  {
    path: "/workmaster/:id",
    name: "Workmaster",
    component: Workmaster,
    meta: { helpKey: "production/workmaster/detail" },
  },
  {
    path: "/workmaster/:id/phase/:phaseid",
    name: "WorkmasterPhase",
    component: WorkmasterPhase,
    meta: { helpKey: "production/workmaster/phase" },
  },
  {
    path: "/workorder",
    name: "Workorders",
    component: Workorders,
    meta: { helpKey: "production/workorder/list" },
  },
  {
    path: "/workorder/:id",
    name: "workorder",
    component: Workorder,
    meta: { helpKey: "production/workorder/detail" },
  },
  {
    path: "/workorder/:id/phase/:phaseid",
    name: "WorkorderPhase",
    component: WorkorderPhase,
    meta: { helpKey: "production/workorder/phase" },
  },
  {
    path: "/productionpart",
    name: "productionparts",
    component: ProductionParts,
    meta: { helpKey: "production/productionpart/list" },
  },
  {
    path: "/shifts",
    name: "shifts",
    component: Shifts,
    meta: { helpKey: "production/shift/list" },
  },
  {
    path: "/productioncost",
    name: "productioncost",
    component: CostDashboard,
    meta: { helpKey: "production/cost/dashboard" },
  },
  {
    path: "/production-dashboard",
    name: "productiondashboard",
    component: ProductionDashboard,
    meta: { helpKey: "production/dashboard" },
  },
  {
    path: "/workcentershift",
    name: "workcentershift",
    component: WorkcenterShift,
    meta: { helpKey: "production/workcentershift/list" },
  },
  {
    path: "/workorderplanning",
    name: "workorderplanning",
    component: WorkorderPlanning,
    meta: { helpKey: "production/workorder/planning" },
  },
  {
    path: "/workcentersaturation",
    name: "workcentersaturation",
    component: WorkcenterSaturation,
    meta: { helpKey: "production/workcenter/saturation" },
  },
  {
    path: "/phasetemplate",
    name: "PhaseTemplates",
    component: PhaseTemplates,
    meta: { helpKey: "production/phasetemplate/list" },
  },
  {
    path: "/phasetemplate/:id",
    name: "PhaseTemplate",
    component: PhaseTemplate,
    meta: { helpKey: "production/phasetemplate/detail" },
  },
] as Array<RouteRecordRaw>;
