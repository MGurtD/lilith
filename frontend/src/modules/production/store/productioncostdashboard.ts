import { defineStore } from "pinia";
import { ProductionCostDashboardGrouped } from "../types";
import services from "../services";

export const useProductionCostDashboardStore = defineStore(
  "productionCostDashboard",
  {
    state: () => ({
      productionCostDashboardGrouped: undefined as
        | Array<ProductionCostDashboardGrouped>
        | undefined,
    }),
    getters: {},
    actions: {
      async fetchGroupedByType(startTime: string, endTime: string) {
        this.productionCostDashboardGrouped =
          await services.ProductionCostDashboardService.GetGroupedByMonthAndWorkcenterType(
            startTime,
            endTime
          );
      },
      async fetchGroupedByWorkcenter(startTime: string, endTime: string) {
        this.productionCostDashboardGrouped =
          await services.ProductionCostDashboardService.GetGroupedByMonthAndWorkcenterType(
            startTime,
            endTime
          );
      },
      async fetchGroupedByOperator(startTime: string, endTime: string) {
        this.productionCostDashboardGrouped =
          await services.ProductionCostDashboardService.GroupedByMonthAndOperator(
            startTime,
            endTime
          );
      },
    },
  }
);
