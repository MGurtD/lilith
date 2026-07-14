import apiClient from "@/api/api.client";
import type { Branding } from "@/types/branding";

export class BrandingService {
  public async GetByEnterpriseId(
    enterpriseId: string,
  ): Promise<Branding | undefined> {
    try {
      const response = await apiClient.get(`/Branding/${enterpriseId}`);
      if (response.status === 200) {
        return response.data as Branding;
      }
    } catch {
      // Interceptor handles user-facing error toasts
    }
    return undefined;
  }

  public async UpdateBranding(
    enterpriseId: string,
    payload: Branding,
  ): Promise<Branding | undefined> {
    try {
      const response = await apiClient.put(
        `/Enterprise/${enterpriseId}/branding`,
        payload,
      );
      if (response.status === 200) {
        return response.data as Branding;
      }
    } catch {
      // Interceptor handles user-facing error toasts
    }
    return undefined;
  }
}
