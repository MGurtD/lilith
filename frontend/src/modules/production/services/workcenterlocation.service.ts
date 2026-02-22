import { WorkcenterLocation } from "../types";
import BaseService from "../../../api/base.service";

export class WorkcenterLocationService extends BaseService<WorkcenterLocation> {
  async getByWorkcenterId(
    workcenterId: string,
  ): Promise<Array<WorkcenterLocation> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/workcenter/${workcenterId}`,
    );
    if (response.status === 200)
      return response.data as Array<WorkcenterLocation>;
  }
}
