import { defineStore } from "pinia";
import { ProductionPart } from "../types";
import Services from "../services";

export const useProductionPartStore = defineStore("productionParts", {
  state: () => ({
    productionPart: undefined as ProductionPart | undefined,
    productionParts: undefined as Array<ProductionPart> | undefined,
  }),
  getters: {},
  actions: {
    setNew(id: string) {
      this.productionPart = {
        id,
        date: new Date(),
        workcenterId: "",
        operatorId: "",
        workOrderId: "",
        workOrderPhaseId: "",
        workOrderPhaseDetailId: "",
        quantity: 0,
        operatorTime: 0,
        workcenterTime: 0,
      } as ProductionPart;
    },
    async fetchAll() {
      this.productionParts = await Services.ProductionPart.getAll();
    },
    async fetchByWorkOrderId(workOrderId: string) {
      this.productionParts =
        await Services.ProductionPart.GetByWorkOrderId(workOrderId);
    },
    async fetchOne(id: string) {
      const data = await Services.ProductionPart.getById(id);
      if (data) {
        // Convert ISO date string to Date object for PrimeVue 4 DatePicker
        if (data.date) {
          data.date = new Date(data.date) as any;
        }
      }
      this.productionPart = data;
    },
    async fetchFiltered(
      startTime: string,
      endTime: string,
      workcenterid: string,
      operatorid: string,
      workorderid: string,
    ) {
      this.productionParts = await Services.ProductionPart.GetBetweenDates(
        startTime,
        endTime,
        workcenterid,
        operatorid,
        workorderid,
      );
    },
    async create(model: ProductionPart) {
      const result = await Services.ProductionPart.create(model);
      if (result) await this.fetchOne(model.id);
      return result;
    },
    async delete(id: string) {
      const result = await Services.ProductionPart.delete(id);
      return result;
    },
    async update(id: string, model: ProductionPart) {
      const result = await Services.ProductionPart.update(id, model);
      if (result) await this.fetchAll();
      return result;
    },
  },
});
