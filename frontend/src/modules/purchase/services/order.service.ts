import {
  PurchaseOrder,
  PurchaseOrderDetail,
  PurchaseOrderReceiptDetail,
  PurchaseOrderFromWO,
  ReceiptOrderDetailGroup,
} from "../types";
import BaseService from "../../../api/base.service";
import { GenericResponse } from "../../../types";

export class OrderService extends BaseService<PurchaseOrder> {
  async GetReportDataById(id: string) {
    const endpoint = `${this.resource}/Report/${id}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data;
    }
  }

  async getBetweenDates(
    startTime: string,
    endTime: string,
    supplierId?: string,
    statusId?: string
  ): Promise<Array<PurchaseOrder> | undefined> {
    let endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    if (supplierId) endpoint += `&supplierId=${supplierId}`;
    if (statusId) endpoint += `&statusId=${statusId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<PurchaseOrder>;
    }
  }
  async getOrdersToReciptBySupplierId(
    supplierId: string
  ): Promise<Array<ReceiptOrderDetailGroup> | undefined> {
    let endpoint = `${this.resource}/ToReceipt?supplierId=${supplierId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<ReceiptOrderDetailGroup>;
    }
  }
  async createFromWo(
    model: Array<PurchaseOrderFromWO>
  ): Promise<GenericResponse<Boolean>> {
    const response = await this.apiClient.post(
      `${this.resource}/FromWo`,
      model
    );
    return response.data as GenericResponse<Boolean>;
  }

  async addDetail(
    model: PurchaseOrderDetail
  ): Promise<GenericResponse<PurchaseOrderDetail>> {
    const response = await this.apiClient.post(
      `${this.resource}/Detail`,
      model
    );
    return response.data as GenericResponse<PurchaseOrderDetail>;
  }

  async updateDetail(
    model: PurchaseOrderDetail
  ): Promise<GenericResponse<PurchaseOrderDetail>> {
    const response = await this.apiClient.put(
      `${this.resource}/Detail/${model.id}`,
      model
    );
    return response.data as GenericResponse<PurchaseOrderDetail>;
  }

  async removeDetail(
    id: string
  ): Promise<GenericResponse<PurchaseOrderDetail>> {
    const response = await this.apiClient.delete(
      `${this.resource}/Detail/${id}`
    );
    return response.data as GenericResponse<PurchaseOrderDetail>;
  }

  async getReceptions(orderId: string) {
    const response = await this.apiClient.get(
      `${this.resource}/${orderId}/Receptions`
    );
    return response.data as Array<PurchaseOrderReceiptDetail>;
  }
  async addReception(
    model: PurchaseOrderDetail
  ): Promise<GenericResponse<PurchaseOrderReceiptDetail>> {
    const response = await this.apiClient.post(
      `${this.resource}/Reception`,
      model
    );
    return response.data as GenericResponse<PurchaseOrderReceiptDetail>;
  }
  async removeReception(
    id: string
  ): Promise<GenericResponse<PurchaseOrderDetail>> {
    const response = await this.apiClient.delete(
      `${this.resource}/Reception/${id}`
    );
    return response.data as GenericResponse<PurchaseOrderDetail>;
  }
}
