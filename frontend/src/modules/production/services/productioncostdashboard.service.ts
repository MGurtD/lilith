import BaseService from "../../../api/base.service";
import { ProductionCostDashboardGrouped } from "../types";

export default class ProductionCostDashboardService extends BaseService<ProductionCostDashboardGrouped> {
  async GetGroupedByMonthAndWorkcenterType(
    startTime: string,
    endTime: string
  ): Promise<Array<ProductionCostDashboardGrouped> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/GroupedByMonthAndWorkcenterType?startTime=${startTime}&endTime=${endTime}`
    );
    if (response.status === 200)
      return response.data as Array<ProductionCostDashboardGrouped>;
  }

  async GetGroupedByMonthAndWorkcenter(
    startTime: string,
    endTime: string
  ): Promise<Array<ProductionCostDashboardGrouped> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/GroupedByMonthAndWorkcenter?startTime=${startTime}&endTime=${endTime}`
    );
    if (response.status === 200)
      return response.data as Array<ProductionCostDashboardGrouped>;
  }

  async GroupedByMonthAndOperator(
    startTime: string,
    endTime: string
  ): Promise<Array<ProductionCostDashboardGrouped> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/GroupedByMonthAndOperator?startTime=${startTime}&endTime=${endTime}`
    );
    if (response.status === 200)
      return response.data as Array<ProductionCostDashboardGrouped>;
  }
}
