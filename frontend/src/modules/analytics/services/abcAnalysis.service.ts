import apiClient from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import type { AbcAnalysisResult } from "../types";

export class AbcAnalysisService extends BaseService<AbcAnalysisResult> {
  async GetCustomers(
    startTime: string,
    endTime: string
  ): Promise<AbcAnalysisResult | undefined> {
    const endpoint = `${this.resource}/customers?startTime=${startTime}&endTime=${endTime}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as AbcAnalysisResult;
    }
  }

  async GetSuppliers(
    startTime: string,
    endTime: string
  ): Promise<AbcAnalysisResult | undefined> {
    const endpoint = `${this.resource}/suppliers?startTime=${startTime}&endTime=${endTime}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as AbcAnalysisResult;
    }
  }
}
