import {
  AddReceptionsRequest,
  PurchaseOrderReceiptDetail,
  Receipt,
  ReceiptDetail,
} from "../types";
import BaseService from "../../../api/base.service";
import { GenericResponse } from "../../../types";

export class ReceiptService extends BaseService<Receipt> {
  async getFiltered(
    startTime: string,
    endTime: string,
    supplierId?: string
  ): Promise<Array<Receipt> | undefined> {
    let endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    if (supplierId) endpoint += `&supplierId=${supplierId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Receipt>;
    }
  }

  async GetInvoiceable(
    supplierId: string
  ): Promise<Array<Receipt> | undefined> {
    let endpoint = `${this.resource}/ToInvoice/${supplierId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Receipt>;
    }
  }

  async GetByReferenceId(
    referenceId: string
  ): Promise<Array<Receipt> | undefined> {
    let endpoint = `${this.resource}/ByReferenceId/${referenceId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Receipt>;
    }
  }

  async GetByInvoice(invoiceId: string): Promise<Array<Receipt> | undefined> {
    let endpoint = `${this.resource}/Invoice/${invoiceId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Receipt>;
    }
  }

  async addDetail(
    model: ReceiptDetail
  ): Promise<GenericResponse<ReceiptDetail>> {
    const response = await this.apiClient.post(
      `${this.resource}/Detail`,
      model
    );
    return response.data as GenericResponse<ReceiptDetail>;
  }

  async updateDetail(
    model: ReceiptDetail
  ): Promise<GenericResponse<ReceiptDetail>> {
    const response = await this.apiClient.put(
      `${this.resource}/Detail/${model.id}`,
      model
    );
    return response.data as GenericResponse<ReceiptDetail>;
  }

  async removeDetail(id: string): Promise<GenericResponse<ReceiptDetail>> {
    const response = await this.apiClient.delete(
      `${this.resource}/Detail/${id}`
    );
    return response.data as GenericResponse<ReceiptDetail>;
  }

  async calculateDetailWeightAndPrice(
    model: ReceiptDetail
  ): Promise<GenericResponse<ReceiptDetail>> {
    const response = await this.apiClient.post(
      `${this.resource}/Detail/Calculate`,
      model
    );
    return response.data as GenericResponse<ReceiptDetail>;
  }

  async getReceptions(receiptId: string) {
    const response = await this.apiClient.get(
      `${this.resource}/${receiptId}/Receptions`
    );
    return response.data as GenericResponse<Array<PurchaseOrderReceiptDetail>>;
  }

  async addReceptions(
    request: AddReceptionsRequest
  ): Promise<GenericResponse<any>> {
    const response = await this.apiClient.post(
      `${this.resource}/AddReceptions`,
      request
    );
    return response.data as GenericResponse<any>;
  }
}
