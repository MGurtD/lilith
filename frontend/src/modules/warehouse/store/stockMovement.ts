import { defineStore } from "pinia";
import { StockMovement } from "../types";
import { GenericResponse } from "../../../types";
import StockMovementService from "../services";

export const useStockMovementStore = defineStore({
  id: "stockMovement",
  state: () => ({
    stockMovement: undefined as StockMovement | undefined,
    stockMovements: undefined as Array<StockMovement> | undefined,
  }),
  getters: {},
  actions: {
    async create(
      createRequest: StockMovement
    ): Promise<GenericResponse<StockMovement>> {
      const created =
        await StockMovementService.StockMovementService.createMovement(
          createRequest
        );
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
    async getByWorkOrderId(workOrderId: string) {
      this.stockMovements =
        await StockMovementService.StockMovementService.getByWorkOrderId(
          workOrderId
        );
    },
  },
});
