import { defineStore } from "pinia";
import PurchaseService from "../services";
import {
  PurchaseInvoice,
  PurchaseInvoiceImport,
  PurchaseInvoiceUpdateStatues,
  PurchaseInvoiceDueDate,
} from "../types";

export const usePurchaseInvoiceStore = defineStore({
  id: "purchaseInvoices",
  state: () => ({
    purchaseInvoice: undefined as PurchaseInvoice | undefined,
    purchaseInvoices: undefined as Array<PurchaseInvoice> | undefined,
  }),
  getters: {},
  actions: {
    setNewPurchaseInvoice(id: string) {
      this.purchaseInvoice = {
        id: id,
        number: "0",
        supplierNumber: "--",
        purchaseInvoiceDate: new Date(),
        baseAmount: 0.0,
        transportAmount: 0.0,
        subtotal: 0.0,
        taxAmount: 0.0,
        grossAmount: 0.0,
        netAmount: 0.0,
        discountPercentage: 0.0,
        discountAmount: 0.0,
        supplierId: "",
        taxId: "",
        exerciceId: "",
        purchaseInvoiceSerieId: "",
        paymentMethodId: "",
        statusId: "",
        extraTaxAmount: 0,
        extraTaxPercentatge: 0,
        purchaseInvoiceDueDates: [],
        purchaseInvoiceImports: [],
      } as PurchaseInvoice;
    },
    async Create(purchaseInvoice: PurchaseInvoice) {
      const created =
        await PurchaseService.PurchaseInvoice.create(purchaseInvoice);
      return created;
    },
    async GetById(id: string) {
      const data = await PurchaseService.PurchaseInvoice.getById(id);
      if (data) {
        // Convert ISO date string to Date object for PrimeVue 4 DatePicker
        if (data.purchaseInvoiceDate) {
          data.purchaseInvoiceDate = new Date(data.purchaseInvoiceDate) as any;
        }
      }
      this.purchaseInvoice = data;
    },
    async GetFiltered(
      startDate: string,
      endDate: string,
      statusId?: string,
      excludeStatusId?: string,
      supplierId?: string,
    ) {
      if (excludeStatusId && supplierId) {
        this.purchaseInvoices =
          await PurchaseService.PurchaseInvoice.GetBetweenDatesAndExcludeStatusAndSupplier(
            startDate,
            endDate,
            excludeStatusId,
            supplierId,
          );
      } else if (statusId) {
        this.purchaseInvoices =
          await PurchaseService.PurchaseInvoice.GetBetweenDatesAndStatus(
            startDate,
            endDate,
            statusId,
          );
      } else if (excludeStatusId) {
        this.purchaseInvoices =
          await PurchaseService.PurchaseInvoice.GetBetweenDatesAndExcludeStatus(
            startDate,
            endDate,
            excludeStatusId,
          );
      } else if (supplierId) {
        this.purchaseInvoices =
          await PurchaseService.PurchaseInvoice.GetBetweenDatesAndSupplier(
            startDate,
            endDate,
            supplierId,
          );
      } else {
        this.purchaseInvoices =
          await PurchaseService.PurchaseInvoice.GetBetweenDates(
            startDate,
            endDate,
          );
      }
    },
    async Update(purchaseInvoice: PurchaseInvoice) {
      const updated = await PurchaseService.PurchaseInvoice.update(
        purchaseInvoice.id,
        purchaseInvoice,
      );
      return updated;
    },
    async Delete(id: string): Promise<boolean> {
      const deleted = await PurchaseService.PurchaseInvoice.delete(id);
      return deleted;
    },

    async GetDueDates(purchaseInvoice: PurchaseInvoice) {
      const result =
        await PurchaseService.PurchaseInvoice.GetDueDates(purchaseInvoice);
      if (result) return result;
    },

    async UpdateInvoicesStatus(
      invoiceImport: PurchaseInvoiceUpdateStatues,
    ): Promise<boolean> {
      const updated =
        await PurchaseService.PurchaseInvoice.UpdateStatuses(invoiceImport);
      return updated;
    },

    async CreateInvoiceImport(
      invoiceImport: PurchaseInvoiceImport,
    ): Promise<boolean> {
      const created =
        await PurchaseService.PurchaseInvoice.CreateImport(invoiceImport);
      return created;
    },
    async UpdateInvoiceImport(
      invoiceImport: PurchaseInvoiceImport,
    ): Promise<boolean> {
      const created =
        await PurchaseService.PurchaseInvoice.UpdateImport(invoiceImport);
      return created;
    },
    async DeleteInvoiceImport(
      invoiceImport: PurchaseInvoiceImport,
    ): Promise<boolean> {
      const created =
        await PurchaseService.PurchaseInvoice.DeleteImport(invoiceImport);
      return created;
    },

    async AddDueDates(
      dueDates: Array<PurchaseInvoiceDueDate>,
    ): Promise<boolean> {
      const added = await PurchaseService.PurchaseInvoice.AddDueDates(dueDates);
      return added;
    },
    async RemoveDueDates(ids: Array<string>): Promise<boolean> {
      const removed = await PurchaseService.PurchaseInvoice.RemoveDueDates(ids);
      return removed;
    },
    async ReplaceDueDates(
      newDueDates: Array<PurchaseInvoiceDueDate>,
    ): Promise<boolean> {
      if (!this.purchaseInvoice) return false;
      // remove existing first
      const existingIds = this.purchaseInvoice.purchaseInvoiceDueDates.map(
        (d) => d.id,
      );
      if (existingIds.length) await this.RemoveDueDates(existingIds);
      const created = await this.AddDueDates(newDueDates);
      if (created) {
        this.purchaseInvoice.purchaseInvoiceDueDates = newDueDates;
      }
      return created;
    },
  },
});
