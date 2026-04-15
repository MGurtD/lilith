import apiClient, { logException } from "./api.client";
import type { AddressAutocompleteResult } from "@/types";

class GeoapifyService {
  private readonly resource = "/Geoapify";

  async autocomplete(
    text: string,
    countryCode: string,
    limit?: number,
    type?: string,
  ): Promise<Array<AddressAutocompleteResult>> {
    try {
      const params: Record<string, string | number> = {
        text,
        countryCode,
      };
      if (limit !== undefined) params.limit = limit;
      if (type) params.type = type;

      const response = await apiClient.get(`${this.resource}/autocomplete`, {
        params,
      });
      if (response.status === 200) {
        return response.data as Array<AddressAutocompleteResult>;
      }
    } catch (err) {
      logException(err);
    }
    return [];
  }
}

export default new GeoapifyService();
