import BaseService from "../../../api/base.service";
import { Shift, ShiftDetail } from "../types";

export default class ShiftService extends BaseService<Shift> {
  //Details
  async getDetailsByshiftId(
    id: string
  ): Promise<Array<ShiftDetail> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Detail/${id}`);
    if (response.status === 200) {
      return response.data as Array<ShiftDetail>;
    }
  }

  async createDetail(model: ShiftDetail): Promise<boolean> {
    const response = await this.apiClient.post(
      `${this.resource}/Detail`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async updateDetail(model: ShiftDetail): Promise<boolean> {
    const response = await this.apiClient.put(
      `${this.resource}/Detail/${model.id}`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async deleteDetail(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(
      `${this.resource}/Detail/${id}`
    );
    return response.status === 200 || response.status === 201;
  }
}
