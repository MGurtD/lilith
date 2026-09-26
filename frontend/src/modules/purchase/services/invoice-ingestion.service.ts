import apiClient, { logException } from "@/api/api.client";
import type { IngestPurchaseInvoiceResponse } from "../types";

// Binary upload service. NOT extending BaseService<T> — mirrors FileService.upload()
// pattern because the payload is multipart/form-data, not a JSON entity.
export class PurchaseInvoiceIngestionService {
  async ingest(
    file: File,
  ): Promise<IngestPurchaseInvoiceResponse | undefined> {
    const form = new FormData();
    form.append("pdfFile", file);
    try {
      const response = await apiClient.post(
        "/PurchaseInvoice/Ingest",
        form,
        {
          headers: { "Content-Type": "multipart/form-data" },
          timeout: 120000, // 120s — LlamaParse can be slow on first call
        },
      );
      if (response.status === 200) {
        return response.data as IngestPurchaseInvoiceResponse;
      }
    } catch (err) {
      logException(err);
    }
    return undefined;
  }
}