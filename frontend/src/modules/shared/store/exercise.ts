import { defineStore } from "pinia";
import { Exercise } from "../types";
import Services from "../services";

export const useExerciseStore = defineStore({
  id: "exercise",
  state: () => ({
    exercise: undefined as Exercise | undefined,
    exercises: [] as Array<Exercise>,
  }),
  getters: {},
  actions: {
    setNew(id: string) {
      this.exercise = {
        id: id,
        name: "",
        description: "",
        startDate: new Date(),
        endDate: new Date(),
        materialProfit: 30,
        externalProfit: 30,
        disabled: false,
      } as Exercise;
    },

    async fetchAll() {
      this.exercises = (await Services.Exercice.getAll()) ?? [];
    },
    async fetchActive() {
      const all = (await Services.Exercice.getAll()) ?? [];
      this.exercises = all.filter((e) => !e.disabled);
    },
    async fetchOne(id: string) {
      const data = await Services.Exercice.getById(id);
      if (data) {
        // Convert ISO date strings to Date objects for PrimeVue 4 DatePicker
        if (data.startDate) {
          data.startDate = new Date(data.startDate) as any;
        }
        if (data.endDate) {
          data.endDate = new Date(data.endDate) as any;
        }
      }
      this.exercise = data;
    },
    async create(model: Exercise) {
      const result = await Services.Exercice.create(model);
      // Note: fetchAll() removed to prevent race condition with navigation
      // The list view will fetch fresh data on mount
      return result;
    },
    async update(id: string, model: Exercise) {
      const result = await Services.Exercice.update(id, model);
      // Note: fetchAll() removed to prevent race condition with navigation
      // The list view will fetch fresh data on mount
      return result;
    },
    async delete(id: string) {
      const result = await Services.Exercice.delete(id);
      if (result) await this.fetchAll();
      return result;
    },
  },
});
