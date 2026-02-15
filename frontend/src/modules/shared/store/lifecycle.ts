import { defineStore } from "pinia";
import {
  Lifecycle,
  Status,
  StatusTransition,
  LifecycleTag,
  AvailableStatusTransitionDto,
} from "../types";
import Services from "../services";
import { ref } from "vue";

export const useLifecyclesStore = defineStore({
  id: "lifecycle",
  state: () => ({
    lifecycle: ref(undefined as Lifecycle | undefined),
    secondaryLifecycle: ref(undefined as Lifecycle | undefined),
    lifecycles: ref(undefined as Array<Lifecycle> | undefined),
    availableTransitions: ref(
      undefined as Array<AvailableStatusTransitionDto> | undefined
    ),
  }),
  getters: {
    transitions: (state): Array<StatusTransition> => {
      const transitions = [] as Array<StatusTransition>;
      if (state.lifecycle) {
        state.lifecycle.statuses.forEach((s) => {
          if (s.transitions) transitions.push(...s.transitions);
        });
      }
      return transitions;
    },
    getStatusName: (state) => {
      return (statusId: string) => {
        if (!statusId) return "";

        let statusName = "";
        if (state.lifecycle && state.lifecycle.statuses) {
          const status = state.lifecycle.statuses.find(
            (s) => s.id === statusId
          );
          if (status) statusName = status.name;
        }
        return statusName;
      };
    },
  },
  actions: {
    setNew(id: string) {
      this.lifecycle = {
        id,
        name: "",
        description: "",
        disabled: false,
        statuses: [],
      } as Lifecycle;
    },

    async getStatusNameById(lifecycleName: string, statusId: string) {
      const lifecycle = await Services.Lifecycle.getByName(lifecycleName);

      if (lifecycle && lifecycle.statuses.length > 0) {
        const status = lifecycle.statuses.find((s) => s.id === statusId);
        return status ? status.name : "";
      }
      return "";
    },

    async fetchAll() {
      this.lifecycles = await Services.Lifecycle.getAll();
    },
    async fetchOne(id: string) {
      this.lifecycle = await Services.Lifecycle.getById(id);
    },
    async fetchOneByName(name: string) {
      this.lifecycle = await Services.Lifecycle.getByName(name);
    },
    async fetchSecondaryByName(name: string) {
      this.secondaryLifecycle = await Services.Lifecycle.getByName(name);
    },
    async create(model: Lifecycle) {
      const result = await Services.Lifecycle.create(model);
      if (result) await this.fetchAll();
      return result;
    },
    async update(id: string, model: Lifecycle) {
      const result = await Services.Lifecycle.update(id, model);
      if (result) await this.fetchAll();
      return result;
    },
    async delete(id: string) {
      const result = await Services.Lifecycle.delete(id);
      if (result) await this.fetchAll();
      return result;
    },

    // Statuses
    async createStatus(model: Status) {
      const result = await Services.Lifecycle.createStatus(model);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async updateStatus(id: string, model: Status) {
      const result = await Services.Lifecycle.updateStatus(model);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async deleteStatus(id: string) {
      const result = await Services.Lifecycle.deleteStatus(id);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },

    // Transitions
    async createTransition(model: StatusTransition) {
      const result = await Services.Lifecycle.createTransition(model);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async updateTransition(id: string, model: StatusTransition) {
      const result = await Services.Lifecycle.updateTransition(model);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async deleteTransition(id: string) {
      const result = await Services.Lifecycle.deleteTransition(id);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },

    // Tags
    async createTag(lifecycleId: string, model: LifecycleTag) {
      const result = await Services.Lifecycle.createTag(lifecycleId, model);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async updateTag(model: LifecycleTag) {
      const result = await Services.Lifecycle.updateTag(model);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async deleteTag(id: string) {
      const result = await Services.Lifecycle.deleteTag(id);
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async fetchTagsByStatus(statusId: string) {
      return await Services.Lifecycle.getTagsByStatus(statusId);
    },
    async assignTagToStatus(statusId: string, tagId: string) {
      const result = await Services.Lifecycle.assignTagToStatus(
        statusId,
        tagId
      );
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },
    async removeTagFromStatus(statusId: string, tagId: string) {
      const result = await Services.Lifecycle.removeTagFromStatus(
        statusId,
        tagId
      );
      if (result) await this.fetchOne(this.lifecycle!.id);
      return result;
    },

    // Available Transitions
    async fetchAvailableTransitions(statusId: string) {
      this.availableTransitions =
        await Services.Lifecycle.getAvailableTransitions(statusId);
    },
  },
});
