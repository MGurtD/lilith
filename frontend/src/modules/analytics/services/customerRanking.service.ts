import { CustomerSalesRanking } from "../types";
import apiClient from "../../../api/api.client";
import BaseService from "../../../api/base.service";

export class CustomerRankingService extends BaseService<CustomerSalesRanking> {
  async GetAnnualRanking(
    year: number
  ): Promise<Array<CustomerSalesRanking> | undefined> {
    const endpoint = `${this.resource}/annual?year=${year}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<CustomerSalesRanking>;
    }
  }
}
