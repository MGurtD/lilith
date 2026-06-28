import BaseService from "../../../api/base.service";
import { GenericResponse } from "../../../types";
import { FindInvoicesResponse, VerifactuInvoiceRequest } from "../types";

export class VerifactuService extends BaseService<any> {
  private TIMEOUT_MS = 60000;

  async FindInvoices(
    month: number,
    year: number
  ): Promise<FindInvoicesResponse | undefined> {
    const endpoint = `${this.resource}?Month=${month}&Year=${year}`;
    const response = await this.apiClient.get(endpoint);

    if (response.status === 200) {
      return response.data as FindInvoicesResponse;
    }
  }

  async GetIntegrationsBetweenDates(
    fromDate: string | Date,
    toDate: string | Date
  ): Promise<any[] | undefined> {
    const fromIso = new Date(fromDate).toISOString();
    const toIso = new Date(toDate).toISOString();
    const endpoint = `${
      this.resource
    }/IntegrationsBetweenDates?fromDate=${encodeURIComponent(
      fromIso
    )}&toDate=${encodeURIComponent(toIso)}`;
    const response = await this.apiClient.get(endpoint);

    if (response.status === 200) {
      return response.data as any[];
    }
  }

  async GetPendingIntegration(
    toDate?: string | Date
  ): Promise<any[] | undefined> {
    let endpoint = `${this.resource}/PendingIntegrations`;
    if (toDate) {
      const iso = new Date(toDate).toISOString();
      endpoint += `?toDate=${encodeURIComponent(iso)}`;
    }
    const response = await this.apiClient.get(endpoint);

    if (response.status === 200) {
      return response.data;
    }
  }

  async GetInvoiceRequests(
    invoiceId: string
  ): Promise<VerifactuInvoiceRequest[] | undefined> {
    const endpoint = `${this.resource}/${invoiceId}/Requests`;
    const response = await this.apiClient.get(endpoint);

    if (response.status === 200) {
      return response.data as VerifactuInvoiceRequest[];
    }
  }

  async SendToVerifactu(
    invoiceId: string
  ): Promise<GenericResponse<any> | undefined> {
    const endpoint = `${this.resource}/${invoiceId}/SendToVerifactu`;
    const response = await this.apiClient.post(endpoint, null, {
      timeout: this.TIMEOUT_MS,
    });

    // Return the GenericResponse body regardless of 2xx/4xx so the caller can
    // surface AEAT rejection errors (result:false + errors[] from the backend).
    // The apiClient treats 200-404 as resolve, so a 400 from the backend
    // (e.g. AEAT status=Incorrecto) lands here with the body intact — we must
    // not drop it on the floor (issue #69 follow-up).
    return response.data as GenericResponse<any>;
  }

  async RemoveFromVerifactu(
    invoiceId: string
  ): Promise<GenericResponse<any> | undefined> {
    const endpoint = `${this.resource}/${invoiceId}/RemoveFromVerifactu`;
    const response = await this.apiClient.post(endpoint, null, {
      timeout: this.TIMEOUT_MS,
    });

    return response.data as GenericResponse<any>;
  }

  async ResendToVerifactu(
    invoiceId: string
  ): Promise<GenericResponse<any> | undefined> {
    const endpoint = `${this.resource}/${invoiceId}/ResendToVerifactu`;
    const response = await this.apiClient.post(endpoint, null, {
      timeout: this.TIMEOUT_MS,
    });

    if (response.status === 200) {
      return response.data as GenericResponse<any>;
    }
  }
}
