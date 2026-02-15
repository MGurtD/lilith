import { defineStore } from "pinia";
import { OperatorType } from "../types";
import { OperatorTypeService } from "../services";

const operatorTypeService = new OperatorTypeService("/operatortype");

export const useOperatorTypeStore = defineStore("operatortype", {
  state: () => ({
    operatorTypes: undefined as Array<OperatorType> | undefined,
  }),
  getters: {
    getOperatorTypeNameById: (state) => {
      return (id: string | undefined | null): string => {
        if (!id || !state.operatorTypes) return "";
        const ot = state.operatorTypes.find((o) => o.id === id);
        return ot ? ot.name : "";
      };
    },
  },
  actions: {
    async fetchOperatorTypes() {
      this.operatorTypes = await operatorTypeService.getAll();
    },
  },
});
