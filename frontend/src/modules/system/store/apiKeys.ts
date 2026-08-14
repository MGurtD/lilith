import { defineStore } from "pinia";
import Services from "@/modules/system/services";
import type { ApiKey, CreateApiKeyResponse } from "@/types";

interface CreateApiKeyRequest {
  id: string;
  name: string;
  description?: string;
  scopes?: string;
  expiresOn?: string | null;
}

interface State {
  items: ApiKey[];
  loading: boolean;
  saving: boolean;
}

export const useApiKeysStore = defineStore("apiKeys", {
  state: (): State => ({
    items: [],
    loading: false,
    saving: false,
  }),
  actions: {
    async fetchAll() {
      this.loading = true;
      try {
        const data = await Services.ApiKey.getAll();
        this.items = data ?? [];
      } finally {
        this.loading = false;
      }
    },

    async create(
      request: CreateApiKeyRequest
    ): Promise<CreateApiKeyResponse | undefined> {
      this.saving = true;
      try {
        const result = await Services.ApiKey.createKey(request);
        if (result) {
          await this.fetchAll();
        }
        return result;
      } finally {
        this.saving = false;
      }
    },

    async disable(id: string): Promise<boolean> {
      this.saving = true;
      try {
        const ok = await Services.ApiKey.disable(id);
        if (ok) await this.fetchAll();
        return ok;
      } finally {
        this.saving = false;
      }
    },
  },
});
