import apiClient, { logException } from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import {
  PurchaseInvoiceDueDate,
  PurchaseInvoice,
  InvoiceSerie,
  PurchaseInvoiceUpdateStatues,
  PurchaseInvoiceImport,
} from "../types";

export class PurchaseInvoiceSerieService extends BaseService<InvoiceSerie> {}

export class PurchaseInvoiceService extends BaseService<PurchaseInvoice> {
  async GetBetweenDates(
    startTime: string,
    endTime: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<PurchaseInvoice>;
    }
  }

  async GetBetweenDatesAndStatus(
    startTime: string,
    endTime: string,
    statusId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&statusId=${statusId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<PurchaseInvoice>;
    }
  }

  async GetBetweenDatesAndExcludeStatus(
    startTime: string,
    endTime: string,
    excludeStatusId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&excludeStatusId=${excludeStatusId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<PurchaseInvoice>;
    }
  }

  async GetBetweenDatesAndSupplier(
    startTime: string,
    endTime: string,
    supplierId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&supplierId=${supplierId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<PurchaseInvoice>;
    }
  }

  async GetBetweenDatesAndExcludeStatusAndSupplier(
    startTime: string,
    endTime: string,
    excludeStatusId: string,
    supplierId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&excludeStatusId=${excludeStatusId}&supplierId=${supplierId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<PurchaseInvoice>;
    }
  }

  async GetDueDates(
    purchaseInvoice: PurchaseInvoice
  ): Promise<Array<PurchaseInvoiceDueDate> | undefined> {
    const response = await apiClient.post(
      `${this.resource}/DueDates`,
      purchaseInvoice
    );
    if (response.status === 200) {
      return response.data as Array<PurchaseInvoiceDueDate>;
    }
  }

  async RecreateDueDates(purchaseInvoice: PurchaseInvoice): Promise<boolean> {
    const response = await apiClient.post(
      `${this.resource}/RecreateDueDates`,
      purchaseInvoice
    );
    2;
    return response.status === 200;
  }

  async UpdateStatuses(
    request: PurchaseInvoiceUpdateStatues
  ): Promise<boolean> {
    const endpoint = `${this.resource}/UpdateStatuses`;
    const response = await apiClient.post(endpoint, request);
    return response.status === 200;
  }

  async CreateImport(request: PurchaseInvoiceImport): Promise<boolean> {
    const endpoint = `${this.resource}/Import`;
    const response = await apiClient.post(endpoint, request);
    return response.status === 200;
  }

  async UpdateImport(request: PurchaseInvoiceImport): Promise<boolean> {
    const endpoint = `${this.resource}/Import/${request.id}`;
    const response = await apiClient.put(endpoint, request);
    return response.status === 200;
  }

  async DeleteImport(request: PurchaseInvoiceImport): Promise<boolean> {
    const endpoint = `${this.resource}/Import/${request.id}`;
    const response = await apiClient.delete(endpoint);
    return response.status === 200;
  }

  async AddDueDates(dueDates: Array<PurchaseInvoiceDueDate>): Promise<boolean> {
    const endpoint = `${this.resource}/DueDate`;
    const response = await apiClient.post(endpoint, dueDates);
    return response.status === 200;
  }

  async RemoveDueDates(ids: Array<string>): Promise<boolean> {
    const params = ids.map((i) => `ids=${i}`).join("&");
    const endpoint = `${this.resource}/DueDate?${params}`;
    const response = await apiClient.delete(endpoint);
    return response.status === 200;
  }
}
