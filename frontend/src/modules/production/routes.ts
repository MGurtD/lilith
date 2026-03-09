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

const Workmasters = () => import("./views/Workmasters.vue");
const Workmaster = () => import("./views/Workmaster.vue");
const WorkmasterPhase = () => import("./views/WorkmasterPhase.vue");

const Workorders = () => import("./views/Workorders.vue");
const Workorder = () => import("./views/Workorder.vue");
const WorkorderPhase = () => import("./views/WorkorderPhase.vue");

const ProductionParts = () => import("./views/ProductionParts.vue");

const Shifts = () => import("./views/Shifts.vue");

const CostDashboard = () => import("./views/CostDashboard.vue");

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
  },
  {
    path: "/enterprise",
    name: "Enterprises",
    component: Enterprises,
  },
  {
    path: "/site/:id",
    name: "Site",
    component: Site,
  },
  {
    path: "/site",
    name: "Sites",
    component: Sites,
  },
  {
    path: "/area/:id",
    name: "Area",
    component: Area,
  },
  {
    path: "/area",
    name: "Areas",
    component: Areas,
  },
  {
    path: "/workcentertype/:id",
    name: "WorkcenterType",
    component: WorkcenterType,
  },
  {
    path: "/workcentertype",
    name: "WorkcenterTypes",
    component: WorkcenterTypes,
  },
  {
    path: "/workcenter/:id",
    name: "Workcenter",
    component: Workcenter,
  },
  {
    path: "/workcenter",
    name: "Workcenters",
    component: Workcenters,
  },
  {
    path: "/workcentercost/:id",
    name: "Workcentercost",
    component: Workcentercost,
  },
  {
    path: "/workcentercost",
    name: "Workcentercosts",
    component: Workcentercosts,
  },
  {
    path: "/operatortype/:id",
    name: "Operatortype",
    component: OperatorType,
  },
  {
    path: "/operatortype",
    name: "Operatortypes",
    component: OperatorTypes,
  },
  {
    path: "/operator/:id",
    name: "Operator",
    component: Operator,
  },
  {
    path: "/operator",
    name: "Operators",
    component: Operators,
  },
  {
    path: "/machinestatus/:id",
    name: "MachineStatus",
    component: MachineStatus,
  },
  {
    path: "/machinestatus",
    name: "MachineStatuses",
    component: MachineStatuses,
  },
  {
    path: "/workmaster",
    name: "Workmasters",
    component: Workmasters,
  },
  {
    path: "/workmaster/:id",
    name: "Workmaster",
    component: Workmaster,
  },
  {
    path: "/workmaster/:id/phase/:phaseid",
    name: "WorkmasterPhase",
    component: WorkmasterPhase,
  },
  {
    path: "/workorder",
    name: "Workorders",
    component: Workorders,
  },
  {
    path: "/workorder/:id",
    name: "workorder",
    component: Workorder,
  },
  {
    path: "/workorder/:id/phase/:phaseid",
    name: "WorkorderPhase",
    component: WorkorderPhase,
  },
  {
    path: "/productionpart",
    name: "productionparts",
    component: ProductionParts,
  },
  {
    path: "/shifts",
    name: "shifts",
    component: Shifts,
  },
  {
    path: "/productioncost",
    name: "productioncost",
    component: CostDashboard,
  },
  {
    path: "/workcentershift",
    name: "workcentershift",
    component: WorkcenterShift,
  },
  {
    path: "/workorderplanning",
    name: "workorderplanning",
    component: WorkorderPlanning,
  },
  {
    path: "/workcentersaturation",
    name: "workcentersaturation",
    component: WorkcenterSaturation,
  },
  {
    path: "/phasetemplate",
    name: "PhaseTemplates",
    component: PhaseTemplates,
  },
  {
    path: "/phasetemplate/:id",
    name: "PhaseTemplate",
    component: PhaseTemplate,
  },
] as Array<RouteRecordRaw>;
