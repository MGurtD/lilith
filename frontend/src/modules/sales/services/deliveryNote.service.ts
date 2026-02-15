import {
  CreateSalesHeaderRequest,
  DeliveryNote,
  SalesOrderHeader,
} from "../types";
import apiClient from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import { GenericResponse } from "../../../types";

export class DeliveryNoteService extends BaseService<DeliveryNote> {
  async Create(
    request: CreateSalesHeaderRequest
  ): Promise<GenericResponse<DeliveryNote>> {
    const endpoint = `${this.resource}`;
    const response = await this.apiClient.post(endpoint, request);
    return response.data as GenericResponse<DeliveryNote>;
  }

  async GetBetweenDates(
    startTime: string,
    endTime: string
  ): Promise<Array<DeliveryNote> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<DeliveryNote>;
    }
  }

  async GetBetweenDatesAndCustomer(
    startTime: string,
    endTime: string,
    customerId: string
  ): Promise<Array<DeliveryNote> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&customerId=${customerId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<DeliveryNote>;
    }
  }

  async GetByInvoiceId(invoiceId: string) {
    const endpoint = `${this.resource}/Invoice/${invoiceId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<DeliveryNote>;
    }
  }

  async GetToInvoice(
    customerId: string
  ): Promise<Array<DeliveryNote> | undefined> {
    const endpoint = `${this.resource}/ToInvoice?customerId=${customerId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<DeliveryNote>;
    }
  }

  async GetReportDataById(id: string, showPrices: boolean) {
    const endpoint = `${this.resource}/Report/${id}?showPrices=${showPrices}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data;
    }
  }

  async AddOrder(
    id: string,
    order: SalesOrderHeader
  ): Promise<GenericResponse<any>> {
    const endpoint = `${this.resource}/${id}/AddOrder`;
    const response = await apiClient.post(endpoint, order);
    return response.data as GenericResponse<any>;
  }
  async DeleteOrder(
    id: string,
    order: SalesOrderHeader
  ): Promise<GenericResponse<any>> {
    const endpoint = `${this.resource}/${id}/RemoveOrder`;
    const response = await apiClient.post(endpoint, order);
    return response.data as GenericResponse<any>;
  }
}
