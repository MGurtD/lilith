import { defineStore } from "pinia";
import {
  AddReceptionsRequest,
  CreatePurchaseDocumentRequest,
  PurchaseOrderReceiptDetail,
  Receipt,
  ReceiptDetail,
} from "../types";
import Services from "../services";
import { GenericResponse } from "../../../types";

export const useReceiptsStore = defineStore({
  id: "receipts",
  state: () => ({
    receipts: undefined as Array<Receipt> | undefined,
    receipt: undefined as Receipt | undefined,
    receptions: undefined as Array<PurchaseOrderReceiptDetail> | undefined,
    selectorReceipts: undefined as Array<Receipt> | undefined,
  }),
  getters: {},
  actions: {
    setNewReceipt(id: string) {
      this.receipt = {
        id: id,
        date: "",
        details: [],
        disabled: false,
        exerciseId: "",
        statusId: "",
        supplierId: "",
        supplierNumber: "",
        number: "",
        purchaseInvoiceId: null,
      } as Receipt;
    },
    async fetchFiltered(
      startDate: string,
      endDate: string,
      supplierId?: string,
      statusId?: string,
    ) {
      this.receipts = await Services.Receipt.getFiltered(
        startDate,
        endDate,
        supplierId,
      );
    },
    async fetchInvoiceableBySupplier(supplierId: string) {
      this.selectorReceipts = await Services.Receipt.GetInvoiceable(supplierId);
    },
    async fetchByInvoice(invoiceId: string) {
      this.receipts = await Services.Receipt.GetByInvoice(invoiceId);
    },
    async fetchReceipts() {
      this.receipts = await Services.Receipt.getAll();
    },
    async fetchReceiptsByReferenceId(id: string) {
      this.receipts = await Services.Receipt.GetByReferenceId(id);
    },
    async fetchReceipt(id: string) {
      const data = await Services.Receipt.getById(id);
      if (data) {
        // Convert ISO date string to Date object for PrimeVue 4 DatePicker
        if (data.date) {
          data.date = new Date(data.date) as any;
        }
      }
      this.receipt = data;
    },
    async fetchReceiptDetails(id: string) {
      const updatedReceipt = await Services.Receipt.getById(id);
      if (this.receipt && updatedReceipt)
        this.receipt.details = updatedReceipt.details;
    },
    async createReceipt(createRequest: CreatePurchaseDocumentRequest) {
      const result = await Services.Receipt.create(createRequest);
      if (result) await this.fetchReceipts();
      return result;
    },
    async updateReceipt(id: string, receipt: Receipt) {
      const result = await Services.Receipt.update(id, receipt);
      if (result) await this.fetchReceipts();
      return result;
    },
    async associateInvoice(receipt: Receipt, invoiceId: string) {
      receipt.purchaseInvoiceId = invoiceId;
      const result = await Services.Receipt.update(receipt.id, receipt);
      return result;
    },
    async unassociateInvoice(receipt: Receipt, invoiceId: string) {
      receipt.purchaseInvoiceId = null;
      const result = await Services.Receipt.update(receipt.id, receipt);
      return result;
    },
    async deleteReceipt(id: string) {
      const response = await Services.Receipt.delete(id);
      if (response) await this.fetchReceipts();
      return response;
    },

    async createReceiptDetail(detail: ReceiptDetail) {
      const response = await Services.Receipt.addDetail(detail);
      if (response.result) {
        await this.fetchReceiptDetails(this.receipt!.id);
      }
      return response;
    },
    async updateReceiptDetail(id: string, detail: ReceiptDetail) {
      const response = await Services.Receipt.updateDetail(detail);
      if (response.result) {
        await this.fetchReceiptDetails(this.receipt!.id);
      }
      return response;
    },
    async deleteReceiptDetail(id: string) {
      const response = await Services.Receipt.removeDetail(id);
      if (response.result) {
        await this.fetchReceiptDetails(this.receipt!.id);
      }
      return response;
    },
    async calculateDetailWeightAndPrice(
      detail: ReceiptDetail,
    ): Promise<GenericResponse<ReceiptDetail>> {
      const response =
        await Services.Receipt.calculateDetailWeightAndPrice(detail);
      return response;
    },
    async getReceptions(orderId: string) {
      this.receptions = await Services.Order.getReceptions(orderId);
    },
    async addReceptions(
      request: AddReceptionsRequest,
    ): Promise<GenericResponse<any>> {
      return await Services.Receipt.addReceptions(request);
    },
  },
});
