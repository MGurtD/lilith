import type { AxiosError } from "axios";
import apiClient, { logException } from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import {
  ReceiptCandidate,
  PurchaseInvoiceDueDate,
  PurchaseInvoice,
  InvoiceSerie,
  PurchaseInvoiceUpdateStatues,
  PurchaseInvoiceImport,
} from "../types";

export class PurchaseInvoiceSerieService extends BaseService<InvoiceSerie> {}

type CreateErrorBody = {
  errors?: string[];
  errorCode?: string;
  content?: unknown;
};

export type PurchaseInvoiceCreateResult =
  | { ok: true }
  | { ok: false; error: string; duplicateOfId?: string };

export class PurchaseInvoiceService extends BaseService<PurchaseInvoice> {
  async GetReceiptCandidates(
    supplierId: string,
    deliveryNoteNumbers: string[],
    taxableBase: number,
  ): Promise<ReceiptCandidate[]> {
    const params = new URLSearchParams();
    deliveryNoteNumbers.forEach((n) => params.append("deliveryNoteNumbers", n));
    params.append("taxableBase", String(taxableBase));
    const response = await apiClient.get(
      `${this.resource}/ReceiptCandidates/${supplierId}?${params.toString()}`,
    );
    return response.status === 200 ? (response.data as ReceiptCandidate[]) : [];
  }

  // Creates the invoice and links the given receipts in one transaction. Keeps the
  // backend's reason so callers can link to the existing invoice on a duplicate.
  // The API client resolves 4xx up to 404 (no global toast), so the status is checked.
  async CreateWithReceipts(
    invoice: PurchaseInvoice,
    receiptIds: string[],
  ): Promise<PurchaseInvoiceCreateResult> {
    return this.checked(() =>
      apiClient.post(`${this.resource}/WithReceipts`, { invoice, receiptIds }),
    );
  }

  // Like update(), but keeps the backend's reason (e.g. a duplicate supplier invoice number).
  async UpdateChecked(
    invoice: PurchaseInvoice,
  ): Promise<PurchaseInvoiceCreateResult> {
    return this.checked(() =>
      apiClient.put(`${this.resource}/${invoice.id}`, invoice),
    );
  }

  private async checked(
    send: () => Promise<{ status: number; data: unknown }>,
  ): Promise<PurchaseInvoiceCreateResult> {
    let data: CreateErrorBody | undefined;
    try {
      const response = await send();
      if (response.status === 200 || response.status === 201) {
        return { ok: true };
      }
      data = response.data as CreateErrorBody | undefined;
    } catch (error) {
      data = (error as AxiosError<CreateErrorBody>).response?.data;
    }
    return {
      ok: false,
      error: data?.errors?.[0] ?? "",
      duplicateOfId:
        data?.errorCode === "PurchaseInvoiceDuplicate" &&
        typeof data.content === "string"
          ? data.content
          : undefined,
    };
  }

  async GetFiltered(
    startTime: string,
    endTime: string,
    supplierId?: string,
    statusId?: string,
    excludeStatusId?: string,
    paymentMethodId?: string,
    dueDateStartTime?: string,
    dueDateEndTime?: string,
    accountNumber?: string,
  ): Promise<Array<PurchaseInvoice> | undefined> {
    const params = new URLSearchParams();
    params.append("startTime", startTime);
    params.append("endTime", endTime);
    if (supplierId) params.append("supplierId", supplierId);
    if (statusId) params.append("statusId", statusId);
    if (excludeStatusId) params.append("excludeStatusId", excludeStatusId);
    if (paymentMethodId) params.append("paymentMethodId", paymentMethodId);
    if (dueDateStartTime) params.append("dueDateStartTime", dueDateStartTime);
    if (dueDateEndTime) params.append("dueDateEndTime", dueDateEndTime);
    if (accountNumber) params.append("accountNumber", accountNumber);

    const endpoint = `${this.resource}?${params.toString()}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<PurchaseInvoice>;
    }
  }

  async GetBetweenDates(
    startTime: string,
    endTime: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    return this.GetFiltered(startTime, endTime);
  }

  async GetBetweenDatesAndStatus(
    startTime: string,
    endTime: string,
    statusId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    return this.GetFiltered(startTime, endTime, undefined, statusId);
  }

  async GetBetweenDatesAndExcludeStatus(
    startTime: string,
    endTime: string,
    excludeStatusId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    return this.GetFiltered(
      startTime,
      endTime,
      undefined,
      undefined,
      excludeStatusId,
    );
  }

  async GetBetweenDatesAndSupplier(
    startTime: string,
    endTime: string,
    supplierId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    return this.GetFiltered(startTime, endTime, supplierId);
  }

  async GetBetweenDatesAndExcludeStatusAndSupplier(
    startTime: string,
    endTime: string,
    excludeStatusId: string,
    supplierId: string
  ): Promise<Array<PurchaseInvoice> | undefined> {
    return this.GetFiltered(
      startTime,
      endTime,
      supplierId,
      undefined,
      excludeStatusId,
    );
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
