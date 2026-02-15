import { defineStore } from "pinia";
import { Tax } from "../types";
import Services from "../services";

export const useTaxesStore = defineStore({
  id: "tax",
  state: () => ({
    tax: undefined as Tax | undefined,
    taxes: undefined as Array<Tax> | undefined,
  }),
  getters: {},
  actions: {
    setNew(id: string) {
      this.tax = {
        id: id,
        name: "",
        disabled: false,
      } as Tax;
    },

    async fetchAll() {
      this.taxes = await Services.Tax.getAll();
    },
    async fetchOne(id: string) {
      this.tax = await Services.Tax.getById(id);
    },
    async create(model: Tax) {
      const result = await Services.Tax.create(model);
      if (result) await this.fetchAll();
      return result;
    },
    async update(id: string, model: Tax) {
      const result = await Services.Tax.update(id, model);
      if (result) await this.fetchAll();
      return result;
    },
    async delete(id: string) {
      const result = await Services.Tax.delete(id);
      if (result) await this.fetchAll();
      return result;
    },
  },
});
