import { defineStore } from "pinia";
import Services from "../services";
import {
  LotBackwardTraceability,
  LotForwardTraceability,
  RecallReport,
} from "../types";

export const useLotTraceabilityStore = defineStore({
  id: "lotTraceability",
  state: () => ({
    backward: undefined as LotBackwardTraceability | undefined,
    forward: undefined as LotForwardTraceability | undefined,
    recall: undefined as RecallReport | undefined,
    loadingBackward: false,
    loadingForward: false,
    loadingRecall: false,
  }),
  actions: {
    async fetchBackward(lotId: string) {
      this.loadingBackward = true;
      this.backward = await Services.LotTraceability.getBackward(lotId);
      this.loadingBackward = false;
      return this.backward;
    },
    async fetchForward(lotId: string) {
      this.loadingForward = true;
      this.forward = await Services.LotTraceability.getForward(lotId);
      this.loadingForward = false;
      return this.forward;
    },
    async fetchRecall(lotId: string) {
      this.loadingRecall = true;
      this.recall = await Services.LotTraceability.getRecall(lotId);
      this.loadingRecall = false;
      return this.recall;
    },
    reset() {
      this.backward = undefined;
      this.forward = undefined;
      this.recall = undefined;
    },
  },
});
