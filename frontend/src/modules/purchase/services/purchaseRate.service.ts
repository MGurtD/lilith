import { PurchaseRate, PurchaseRateDetail } from "../types";
import BaseService from "../../../api/base.service";

export class PurchaseRateService extends BaseService<PurchaseRate> {
  async getBySupplierId(supplierId: string): Promise<Array<PurchaseRate> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Supplier/${supplierId}`);
    if (response.status === 200) {
      return response.data as Array<PurchaseRate>;
    }
    return undefined;
  }

  async getDetails(purchaseRateId: string): Promise<Array<PurchaseRateDetail> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Detail/${purchaseRateId}`);
    if (response.status === 200) {
      return response.data as Array<PurchaseRateDetail>;
    }
    return undefined;
  }

  async createDetail(detail: PurchaseRateDetail): Promise<boolean> {
    const response = await this.apiClient.post(`${this.resource}/Detail`, detail);
    return response.status === 200 || response.status === 201;
  }

  async updateDetail(detail: PurchaseRateDetail): Promise<boolean> {
    const response = await this.apiClient.put(`${this.resource}/Detail/${detail.id}`, detail);
    return response.status === 200 || response.status === 201;
  }

  async deleteDetail(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(`${this.resource}/Detail/${id}`);
    return response.status === 200 || response.status === 201;
  }

  async duplicate(id: string, name: string, validFrom: string, validTo: string): Promise<boolean> {
    const response = await this.apiClient.post(`${this.resource}/${id}/Duplicate`, {
      name,
      validFrom,
      validTo,
    });
    return response.status === 200 || response.status === 201;
  }
}
