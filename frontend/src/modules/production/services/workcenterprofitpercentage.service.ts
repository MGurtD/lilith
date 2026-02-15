import { WorkcenterProfitPercentage } from "../types";
import BaseService from "../../../api/base.service";

export class WorkcenterProfitPercentageService extends BaseService<WorkcenterProfitPercentage> {
  async getByWorkcenterId(
    workcenterId: string,
  ): Promise<Array<WorkcenterProfitPercentage> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/workcenter/${workcenterId}`,
    );
    if (response.status === 200)
      return response.data as Array<WorkcenterProfitPercentage>;
  }
}
