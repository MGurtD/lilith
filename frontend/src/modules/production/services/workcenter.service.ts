import { Workcenter, WorkcenterTypeSaturation } from "../types";
import BaseService from "../../../api/base.service";

export class WorkcenterService extends BaseService<Workcenter> {
  async getVisibleInPlant(): Promise<Array<Workcenter> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/plant`);
    if (response.status === 200) return response.data as Array<Workcenter>;
  }

  async GetWorkcenterTypeSaturation(
    startDate: string,
    endDate: string,
  ): Promise<Array<WorkcenterTypeSaturation> | undefined> {
    const response = await this.apiClient.get(
      `/workcenter/workcenterload?startDate=${startDate}&endDate=${endDate}`,
    );
    if (response.status === 200)
      return response.data as Array<WorkcenterTypeSaturation>;
    if (response.status === 204) return [];
  }
}
