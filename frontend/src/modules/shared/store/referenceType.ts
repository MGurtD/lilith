import { defineStore } from "pinia";
import Services from "../services";
import { ReferenceType } from "../types";

export const useReferenceTypeStore = defineStore({
  id: "referenceType",
  state: () => ({
    referenceType: undefined as ReferenceType | undefined,
    referenceTypes: undefined as Array<ReferenceType> | undefined,
  }),
  getters: {},
  actions: {
    setNewReferenceType(id: string) {
      this.referenceType = {
        id: id,
        name: "",
        description: "",
        disabled: false,
      } as ReferenceType;
    },
    getReferenceTypeById(id: string) {
      return this.referenceTypes?.find((rt) => rt.id === id);
    },
    async fetchAll() {
      this.referenceTypes = await Services.ReferenceType.getAll();
    },
    async fetchActive() {
      this.referenceTypes = await Services.ReferenceType.getActive();
    },
    async fetchReferenceType(id: string) {
      this.referenceType = await Services.ReferenceType.getById(id);
    },
    async createReferenceType(rawmaterialType: ReferenceType) {
      const result = await Services.ReferenceType.create(rawmaterialType);
      if (result) await this.fetchAll();
      return result;
    },
    async updateReferenceType(id: string, rawmaterialType: ReferenceType) {
      const result = await Services.ReferenceType.update(id, rawmaterialType);
      if (result) await this.fetchAll();
      return result;
    },
    async deleteReferenceType(id: string) {
      const result = await Services.ReferenceType.delete(id);
      if (result) await this.fetchAll();
      return result;
    },
  },
});
