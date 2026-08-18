import apiClient from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import type { BudgetConversionResult } from "../types";

export class BudgetConversionService extends BaseService<BudgetConversionResult> {
  async GetConversion(
    startTime: string,
    endTime: string,
    customerId?: string
  ): Promise<BudgetConversionResult | undefined> {
    let endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    if (customerId) endpoint += `&customerId=${customerId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as BudgetConversionResult;
    }
  }
}
