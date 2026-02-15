import BaseService from "../../../api/base.service";
import { GenericResponse } from "../../../types";
import { PurchaseInvoiceUpdateStatues as InvoiceUpdateStatues } from "../../purchase/types";
import {
  CreateRectificativeInvoiceRequest,
  CreateSalesHeaderRequest,
  DeliveryNote,
  SalesInvoice,
  SalesInvoiceDetail,
} from "../types";

export class SalesInvoiceService extends BaseService<SalesInvoice> {
  async Create(
    request: CreateSalesHeaderRequest
  ): Promise<GenericResponse<SalesInvoice> | undefined> {
    const endpoint = `${this.resource}`;
    const response = await this.apiClient.post(endpoint, request);

    if (response.status === 200) {
      return response.data as GenericResponse<SalesInvoice>;
    }
  }
  async CreateRectificative(
    request: CreateRectificativeInvoiceRequest
  ): Promise<GenericResponse<SalesInvoice> | undefined> {
    const endpoint = `${this.resource}`;
    const response = await this.apiClient.post(
      `${endpoint}/rectificative`,
      request
    );

    if (response.status === 200)
      return response.data as GenericResponse<SalesInvoice>;
  }
  async GetHeader(id: string): Promise<SalesInvoice | undefined> {
    const endpoint = `${this.resource}/header/${id}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as SalesInvoice;
    }
  }
  async GetBetweenDates(
    startTime: string,
    endTime: string
  ): Promise<Array<SalesInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<SalesInvoice>;
    }
  }

  async GetBetweenDatesAndStatus(
    startTime: string,
    endTime: string,
    statusId: string
  ): Promise<Array<SalesInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&statusId=${statusId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<SalesInvoice>;
    }
  }
  async GetBetweenDatesAndExcludeStatus(
    startTime: string,
    endTime: string,
    excludeStatusId: string
  ): Promise<Array<SalesInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&excludeStatusId=${excludeStatusId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<SalesInvoice>;
    }
  }

  async GetBetweenDatesAndCustomer(
    startTime: string,
    endTime: string,
    customerId: string
  ): Promise<Array<SalesInvoice> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&customerId=${customerId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<SalesInvoice>;
    }
  }

  async GetReportDataById(id: string) {
    const endpoint = `${this.resource}/Report/${id}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as any;
    }
  }

  async Update(request: SalesInvoice) {
    const endpoint = `${this.resource}/${request.id}`;
    const response = await this.apiClient.put(endpoint, request);
    return response.status === 200;
  }

  async UpdateStatuses(request: InvoiceUpdateStatues): Promise<boolean> {
    const endpoint = `${this.resource}/UpdateStatuses`;
    const response = await this.apiClient.post(endpoint, request);
    return response.status === 200;
  }

  async Delete(request: SalesInvoice) {
    const endpoint = `${this.resource}/${request.id}`;
    const response = await this.apiClient.delete(endpoint);
    return response.status === 200;
  }

  async AddDeliveryNote(
    id: string,
    deliveryNote: DeliveryNote
  ): Promise<GenericResponse<any>> {
    const endpoint = `${this.resource}/${id}/AddDeliveryNote`;
    const response = await this.apiClient.post(endpoint, deliveryNote);
    return response.data as GenericResponse<any>;
  }
  async RemoveDeliveryNote(
    id: string,
    deliveryNote: DeliveryNote
  ): Promise<GenericResponse<any>> {
    const endpoint = `${this.resource}/${id}/RemoveDeliveryNote`;
    const response = await this.apiClient.post(endpoint, deliveryNote);
    return response.data as GenericResponse<any>;
  }

  async CreateDetail(
    request: SalesInvoiceDetail
  ): Promise<GenericResponse<any>> {
    const endpoint = `${this.resource}/Detail`;
    const response = await this.apiClient.post(endpoint, request);
    return response.data;
  }

  async UpdateDetail(request: SalesInvoiceDetail) {
    const endpoint = `${this.resource}/Detail/${request.id}`;
    const response = await this.apiClient.put(endpoint, request);
    return response.status === 200;
  }

  async DeleteDetail(request: SalesInvoiceDetail) {
    const endpoint = `${this.resource}/Detail/${request.id}`;
    const response = await this.apiClient.delete(endpoint);
    return response.status === 200;
  }
}
