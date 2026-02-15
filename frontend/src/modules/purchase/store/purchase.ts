import { defineStore } from "pinia";
import SharedServices from "../../../modules/shared/services";
import PurchaseServices from "../services";
import {
  PurchaseInvoice,
  InvoiceSerie,
  PurchaseInvoiceStatus,
  PurchaseMasterData,
} from "../types";
import { Exercise, PaymentMethod, Tax } from "../../../modules/shared/types";
import { Supplier } from "../types";

export const usePurchaseMasterDataStore = defineStore({
  id: "purchaseMasterData",
  state: () => ({
    masterData: {
      paymentMethods: [] as Array<PaymentMethod>,
      series: [] as Array<InvoiceSerie>,
      suppliers: [] as Array<Supplier>,
      taxes: [] as Array<Tax>,
      exercises: [] as Array<Exercise>,
    } as PurchaseMasterData,
    purchaseInvoices: undefined as Array<PurchaseInvoice> | undefined,
  }),
  getters: {},
  actions: {
    async fetchMasterData() {
      this.masterData.series =
        await PurchaseServices.PurchaseInvoiceSerie.getActive();
      this.masterData.suppliers = await PurchaseServices.Supplier.getActive();
      this.masterData.paymentMethods =
        await SharedServices.PaymentMethod.getActive();
      this.masterData.exercises = await SharedServices.Exercice.getActive();
      this.masterData.taxes = await SharedServices.Tax.getActive();
    },
  },
});
