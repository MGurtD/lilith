import apiClient from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import type { ProductionTimeDeviationResult } from "../types";

export class ProductionTimeDeviationService extends BaseService<ProductionTimeDeviationResult> {
  async GetDeviation(
    startTime: string,
    endTime: string,
    workOrderId?: string
  ): Promise<ProductionTimeDeviationResult | undefined> {
    let endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    if (workOrderId) endpoint += `&workOrderId=${workOrderId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as ProductionTimeDeviationResult;
    }
  }
}
