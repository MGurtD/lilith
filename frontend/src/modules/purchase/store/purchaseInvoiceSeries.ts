import { defineStore } from "pinia";
import PurchaseService from "../services";
import { InvoiceSerie } from "../types";

export const usePurchaseInvoiceSeries = defineStore({
  id: "purchaseInvoiceSeries",
  state: () => ({
    purchaseInvoiceSerie: undefined as InvoiceSerie | undefined,
    purchaseInvoiceSeries: undefined as Array<InvoiceSerie> | undefined,
  }),
  getters: {},
  actions: {
    //Series
    setNewPurchaseInvoiceSerie(id: string) {
      this.purchaseInvoiceSerie = {
        id: id,
        name: "",
        description: "",
        disabled: false,
      } as InvoiceSerie;
    },

    async fetchPurchaseInvoiceSeries() {
      this.purchaseInvoiceSeries =
        await PurchaseService.PurchaseInvoiceSerie.getAll();
    },

    async fetchPurchaseInvoiceSerie(id: string) {
      this.purchaseInvoiceSerie =
        await PurchaseService.PurchaseInvoiceSerie.getById(id);
    },

    async createPurchaseInvoiceSerie(purchaseInvoiceSerie: InvoiceSerie) {
      const result = await PurchaseService.PurchaseInvoiceSerie.create(
        purchaseInvoiceSerie
      );
      if (result) await this.fetchPurchaseInvoiceSeries();
      return result;
    },

    async updatePurchaseInvoiceSerie(
      id: string,
      purchaseInvoiceSerie: InvoiceSerie
    ) {
      const result = await PurchaseService.PurchaseInvoiceSerie.update(
        id,
        purchaseInvoiceSerie
      );
      if (result) await this.fetchPurchaseInvoiceSeries();
      return result;
    },

    async deletePurchaseInvoiceSerie(id: string) {
      const result = await PurchaseService.PurchaseInvoiceSerie.delete(id);
      if (result) await this.fetchPurchaseInvoiceSeries();
      return result;
    },
  },
});
