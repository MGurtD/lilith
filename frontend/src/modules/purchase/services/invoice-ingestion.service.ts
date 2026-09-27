import type { AxiosError } from "axios";
import apiClient from "@/api/api.client";
import { parseAxiosError } from "@/utils/error-parser";
import type { IngestPurchaseInvoiceResponse } from "../types";

export type IngestionResult =
  | { ok: true; draft: IngestPurchaseInvoiceResponse }
  | { ok: false; error: string };

// Multipart upload, so it does not extend BaseService<T> (same as FileService.Upload).
export class PurchaseInvoiceIngestionService {
  async ingest(file: File): Promise<IngestionResult> {
    const form = new FormData();
    form.append("pdfFile", file);
    try {
      const response = await apiClient.post<IngestPurchaseInvoiceResponse>(
        "/PurchaseInvoice/Ingest",
        form,
        {
          headers: { "Content-Type": "multipart/form-data" },
          // Extraction polls the provider for up to the backend's 90s budget.
          timeout: 120000,
        },
      );
      return { ok: true, draft: response.data };
    } catch (error) {
      const info = parseAxiosError(error as AxiosError);
      return { ok: false, error: info.errors[0] ?? info.message };
    }
  }
}
