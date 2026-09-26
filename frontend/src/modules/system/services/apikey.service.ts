import apiClient, { logException } from "@/api/api.client";
import BaseService from "@/api/base.service";
import type { ApiKey, CreateApiKeyResponse } from "@/types";

interface CreateApiKeyRequest {
  id: string;
  name: string;
  description?: string;
  scopes?: string;
  expiresOn?: string | null;
}

export class ApiKeyService extends BaseService<ApiKey> {
  constructor() {
    super("apikey");
  }

  async createKey(
    request: CreateApiKeyRequest
  ): Promise<CreateApiKeyResponse | undefined> {
    try {
      const response = await apiClient.post(this.resource, request);
      if (response.status === 200 || response.status === 201) {
        return response.data as CreateApiKeyResponse;
      }
    } catch (err) {
      logException(err);
    }
    return undefined;
  }

  async disable(id: string): Promise<boolean> {
    try {
      const response = await apiClient.post(`${this.resource}/${id}/disable`);
      return response.status === 200 || response.status === 204;
    } catch (err) {
      logException(err);
    }
    return false;
  }
}
