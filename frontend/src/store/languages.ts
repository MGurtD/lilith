import { defineStore } from "pinia";
import { Language } from "../types";
import { LanguageService } from "../services/language.service";

export const useLanguageStore = defineStore({
  id: "languages",
  state: () => ({
    items: [] as Language[],
    current: undefined as string | undefined,
    isLoading: false,
  }),
  getters: {
    byCode: (state) => (code: string) =>
      state.items.find((l) => l.code === code),
    defaultLanguage: (state) => state.items.find((l) => l.isDefault),
    options: (state) =>
      state.items
        .slice()
        .sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
        .map((l) => ({ label: l.name, value: l.code, icon: l.icon })),
  },
  actions: {
    async fetchAll() {
      if (this.items.length) return;
      this.isLoading = true;
      try {
        const svc = new LanguageService();
        const langs = await svc.GetAll();
        if (langs?.length) this.items = langs;
      } finally {
        this.isLoading = false;
      }
    },
    async fetchDefault() {
      const svc = new LanguageService();
      return await svc.GetDefault();
    },
    setCurrent(code?: string) {
      this.current = code;
    },
  },
});
