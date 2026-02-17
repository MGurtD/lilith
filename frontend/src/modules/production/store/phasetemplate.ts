import { defineStore } from "pinia";
import { PhaseTemplate, PhaseTemplateDetail } from "../types";
import Services from "../services";

export const usePhaseTemplateStore = defineStore("phaseTemplate", {
  state: () => ({
    phaseTemplate: undefined as PhaseTemplate | undefined,
    phaseTemplates: undefined as Array<PhaseTemplate> | undefined,
  }),
  actions: {
    setNew(id: string) {
      this.phaseTemplate = {
        id: id,
        name: "",
        description: "",
        disabled: false,
        details: [],
      } as PhaseTemplate;
    },
    async fetchAll() {
      this.phaseTemplates = await Services.PhaseTemplate.getAll();
    },
    async fetchOne(id: string) {
      this.phaseTemplate = await Services.PhaseTemplate.getById(id);
    },
    async create(model: PhaseTemplate) {
      const result = await Services.PhaseTemplate.create(model);
      if (result) await this.fetchOne(model.id);
      return result;
    },
    async update(id: string, model: PhaseTemplate) {
      const result = await Services.PhaseTemplate.update(id, model);
      if (result) await this.fetchOne(id);
      return result;
    },
    async delete(id: string) {
      const result = await Services.PhaseTemplate.delete(id);
      if (result) await this.fetchAll();
      return result;
    },

    // Details
    async createDetail(model: PhaseTemplateDetail) {
      const result = await Services.PhaseTemplateDetail.create(model);
      if (result) await this.fetchOne(model.phaseTemplateId);
      return result;
    },
    async updateDetail(id: string, model: PhaseTemplateDetail) {
      const result = await Services.PhaseTemplateDetail.update(id, model);
      if (result) await this.fetchOne(model.phaseTemplateId);
      return result;
    },
    async deleteDetail(id: string) {
      const result = await Services.PhaseTemplateDetail.delete(id);
      if (result && this.phaseTemplate) await this.fetchOne(this.phaseTemplate.id);
      return result;
    },
  },
});
