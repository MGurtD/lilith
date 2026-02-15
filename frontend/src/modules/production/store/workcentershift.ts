import { defineStore } from "pinia";
import {
  WorkcenterShiftGroup,
  WorkcenterShiftHistorical,
  WorkcenterShiftRequest,
} from "../types";
import services from "../services";

export const useWorkcenterShiftStore = defineStore("workcentershift", {
  state: () => ({
    workcenterShifts: undefined as Array<WorkcenterShiftHistorical> | undefined,
    workcenterShift: undefined as WorkcenterShiftGroup | undefined,
  }),
  getters: {},
  actions: {
    async query(request: WorkcenterShiftRequest) {
      this.workcenterShifts = await services.WorkcenterShift.Query(request);
      return this.workcenterShifts;
    },
  },
});
