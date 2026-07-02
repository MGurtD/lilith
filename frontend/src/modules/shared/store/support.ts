import { defineStore } from "pinia";
import SupportService from "../services/support.service";

const service = new SupportService();

export const useSupportStore = defineStore({
  id: "support",
  state: () => ({
    isSubmitting: false,
  }),
  actions: {
    async submit(resum: string, descripcio: string): Promise<{ ok: boolean; error?: string }> {
      this.isSubmitting = true;
      try {
        await service.createRequest(resum, descripcio);
        return { ok: true };
      } catch (error: any) {
        const message = error?.message ?? "Error desconegut";
        return { ok: false, error: message };
      } finally {
        this.isSubmitting = false;
      }
    },
  },
});
