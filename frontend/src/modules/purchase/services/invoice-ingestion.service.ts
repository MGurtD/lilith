import type { AxiosError } from "axios";
import apiClient from "@/api/api.client";
import { parseAxiosError } from "@/utils/error-parser";
import type { IngestPurchaseInvoiceResponse } from "../types";

export type IngestionResult =
  | { ok: true; draft: IngestPurchaseInvoiceResponse }
  | { ok: false; error: string };

// Multipart upload, so it does not extend BaseService<T> (same as FileService.Upload).
export class PurchaseInvoiceIngestionService {
  // Feature flag: the backend offers PDF import only with a usable LlamaCloud setup.
  async isEnabled(): Promise<boolean> {
    try {
      const response = await apiClient.get<{ enabled: boolean }>(
        "/PurchaseInvoice/Ingest/Status",
      );
      return response.status === 200 && response.data.enabled === true;
    } catch {
      return false;
    }
  }

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
          // Failures are shown inline on the import screen; resolving every status
          // but 401 (token refresh) keeps the global error toast out.
          validateStatus: (status) => status !== 401,
        },
      );
      if (response.status === 200) return { ok: true, draft: response.data };
      const body = response.data as unknown as { errors?: string[] } | undefined;
      return { ok: false, error: body?.errors?.[0] ?? "" };
    } catch (error) {
      const info = parseAxiosError(error as AxiosError);
      return { ok: false, error: info.errors[0] ?? info.message };
    }
  }
}
