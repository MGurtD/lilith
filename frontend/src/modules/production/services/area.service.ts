import { Area } from "../types";
import BaseService from "../../../api/base.service";

export class AreaService extends BaseService<Area> {
  async getVisibleInPlant(): Promise<Array<Area> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/plant`);
    if (response.status === 200) return response.data as Array<Area>;
  }
}
