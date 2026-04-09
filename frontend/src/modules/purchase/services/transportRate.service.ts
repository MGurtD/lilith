import { TransportRate, TransportRateDetail } from "../types";
import BaseService from "../../../api/base.service";

export class TransportRateService extends BaseService<TransportRate> {
  async getBySupplierId(supplierId: string): Promise<Array<TransportRate> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Supplier/${supplierId}`);
    if (response.status === 200) {
      return response.data as Array<TransportRate>;
    }
    return undefined;
  }

  async getDetails(transportRateId: string): Promise<Array<TransportRateDetail> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Detail/${transportRateId}`);
    if (response.status === 200) {
      return response.data as Array<TransportRateDetail>;
    }
    return undefined;
  }

  async createDetail(detail: TransportRateDetail): Promise<boolean> {
    const response = await this.apiClient.post(`${this.resource}/Detail`, detail);
    return response.status === 200 || response.status === 201;
  }

  async updateDetail(detail: TransportRateDetail): Promise<boolean> {
    const response = await this.apiClient.put(`${this.resource}/Detail/${detail.id}`, detail);
    return response.status === 200 || response.status === 201;
  }

  async deleteDetail(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(`${this.resource}/Detail/${id}`);
    return response.status === 200 || response.status === 201;
  }
}
