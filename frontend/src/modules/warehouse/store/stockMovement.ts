import { defineStore } from "pinia";
import { StockMovement } from "../types";
import StockMovementService from "../services";

export const useStockMovementStore = defineStore({
  id: "stockMovement",
  state: () => ({
    stockMovement: undefined as StockMovement | undefined,
    stockMovements: undefined as Array<StockMovement> | undefined,
  }),
  getters: {},
  actions: {
    async create(createRequest: StockMovement) {
      const created =
        StockMovementService.StockMovementService.create(createRequest);
      return created;
    },
    async getBetweenDates(
      startTime: string,
      endTime: string,
      locationId?: string
    ) {
      this.stockMovements =
        await StockMovementService.StockMovementService.getBetweenDates(
          startTime,
          endTime,
          locationId
        );
    },
  },
});
