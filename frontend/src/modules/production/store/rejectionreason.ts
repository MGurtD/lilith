import { defineStore } from "pinia";
import { RejectionReason } from "../types";
import Services from "../services";

export const useRejectionReasonStore = defineStore("rejectionReason", {
  state: () => ({
    rejectionReason: undefined as RejectionReason | undefined,
    rejectionReasons: undefined as Array<RejectionReason> | undefined,
    activeRejectionReasons: undefined as Array<RejectionReason> | undefined,
  }),
  getters: {
    getRejectionReasonNameById: (state) => {
      return (id: string | undefined | null): string => {
        if (!id || !state.rejectionReasons) return "";
        const reason = state.rejectionReasons.find((r) => r.id === id);
        return reason ? reason.name : "";
      };
    },
  },
  actions: {
    setNew(id: string) {
      this.rejectionReason = {
        id: id,
        code: "",
        name: "",
        description: "",
        color: "",
        disabled: false,
      } as RejectionReason;
    },
    async fetchAll() {
      this.rejectionReasons = await Services.RejectionReason.getAll();
    },
    async fetchActive() {
      this.activeRejectionReasons =
        (await Services.RejectionReason.getActive()) ?? [];
    },
    async fetchOne(id: string) {
      this.rejectionReason = await Services.RejectionReason.getById(id);
    },
    async create(model: RejectionReason) {
      const result = await Services.RejectionReason.create(model);
      if (result) await this.fetchAll();
      return result;
    },
    async update(id: string, model: RejectionReason) {
      const result = await Services.RejectionReason.update(id, model);
      if (result) await this.fetchAll();
      return result;
    },
    async delete(id: string) {
      const result = await Services.RejectionReason.delete(id);
      if (result) await this.fetchAll();
      return result;
    },
  },
});
