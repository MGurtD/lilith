import {
  RouteRecordRaw,
  NavigationGuardNext,
  RouteLocationNormalized,
} from "vue-router";
import { useStore } from "../../store";
import { usePlantOperatorStore } from "./store";

const MainPlant = () => import("./views/Main.vue");
const OperatorClockIn = () => import("./views/OperatorClockIn.vue");
const SiteAreas = () => import("./views/SiteAreas.vue");
const WorkcenterDetail = () => import("./views/WorkcenterDetail.vue");

// Navigation guard para verificar operador
const checkOperatorAuth = async (
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) => {
  const store = useStore();
  const plantOperatorStore = usePlantOperatorStore();

  // Intentar recuperar operador de localStorage si no está en memoria
  if (!plantOperatorStore.operator) {
    await plantOperatorStore.getOperator();
  }

  // Configurar sidebar según estado del operador
  store.sidebar.collapsed = true;
  store.sidebar.hideToggle = plantOperatorStore.operator ? true : false;

  // Si existe operador y estamos en clockin, redirigir a areas
  if (plantOperatorStore.operator && to.name === "OperatorClockIn") {
    next({ name: "SiteAreas" });
  }
  // Si NO existe operador y estamos en areas, redirigir a clockin
  else if (!plantOperatorStore.operator && to.name === "SiteAreas") {
    next({ name: "OperatorClockIn" });
  }
  // Permitir acceso
  else {
    next();
  }
};

export default [
  {
    path: "/plant",
    name: "MainPlant",
    component: MainPlant,
    redirect: { name: "OperatorClockIn" },
    children: [
      {
        path: "clockin",
        name: "OperatorClockIn",
        component: OperatorClockIn,
        beforeEnter: checkOperatorAuth,
      },
      {
        path: "areas",
        name: "SiteAreas",
        component: SiteAreas,
        beforeEnter: checkOperatorAuth,
      },
      {
        path: "/plant/workcenter/:id",
        name: "WorkcenterDetail",
        component: WorkcenterDetail,
        beforeEnter: checkOperatorAuth,
        props: true,
      },
    ],
  },
] as Array<RouteRecordRaw>;
