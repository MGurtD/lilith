import { defineStore } from "pinia";
import { PurchaseRate, PurchaseRateDetail, CalculationType } from "../types";
import { PurchaseRateService } from "../services/purchaseRate.service";
import { getNewUuid, formatDateForQueryParameter } from "@/utils/functions";

const service = new PurchaseRateService("/PurchaseRate");

export const usePurchaseRateStore = defineStore({
  id: "purchaseRate",
  state: () => ({
    purchaseRates: undefined as Array<PurchaseRate> | undefined,
    purchaseRate: undefined as PurchaseRate | undefined,
    purchaseRateDetails: undefined as Array<PurchaseRateDetail> | undefined,
  }),
  actions: {
    setNewPurchaseRate(supplierId: string): PurchaseRate {
      const rate: PurchaseRate = {
        id: getNewUuid(),
        name: "",
        supplierId: supplierId,
        validFrom: new Date(),
        validTo: new Date(new Date().setFullYear(new Date().getFullYear() + 1)),
        disabled: false,
        details: [],
      };
      return rate;
    },

    setNewPurchaseRateDetail(purchaseRateId: string): PurchaseRateDetail {
      return {
        id: getNewUuid(),
        purchaseRateId: purchaseRateId,
        referenceId: "",
        from: 0,
        to: 0,
        calculationType: CalculationType.Units,
        price: 0,
        disabled: false,
      };
    },

    async fetchPurchaseRatesBySupplierId(supplierId: string) {
      this.purchaseRates = await service.getBySupplierId(supplierId);
      this.purchaseRateDetails = undefined;
      this.purchaseRate = undefined;
    },

    async fetchPurchaseRateDetails(purchaseRate: PurchaseRate) {
      this.purchaseRate = purchaseRate;
      this.purchaseRateDetails = await service.getDetails(purchaseRate.id);
    },

    async createPurchaseRate(rate: PurchaseRate): Promise<boolean> {
      const model = { ...rate };
      model.validFrom = formatDateForQueryParameter(new Date(model.validFrom));
      model.validTo = formatDateForQueryParameter(new Date(model.validTo));

      const result = await service.create(model as any);
      if (result) await this.fetchPurchaseRatesBySupplierId(rate.supplierId);
      return result;
    },

    async updatePurchaseRate(rate: PurchaseRate): Promise<boolean> {
      const model = { ...rate };
      model.validFrom = formatDateForQueryParameter(new Date(model.validFrom));
      model.validTo = formatDateForQueryParameter(new Date(model.validTo));

      const result = await service.update(model.id, model as any);
      if (result) await this.fetchPurchaseRatesBySupplierId(rate.supplierId);
      return result;
    },

    async deletePurchaseRate(rate: PurchaseRate): Promise<boolean> {
      const result = await service.delete(rate.id);
      if (result) {
        await this.fetchPurchaseRatesBySupplierId(rate.supplierId);
        if (this.purchaseRate?.id === rate.id) {
          this.purchaseRate = undefined;
          this.purchaseRateDetails = undefined;
        }
      }
      return result;
    },

    async createPurchaseRateDetail(detail: PurchaseRateDetail): Promise<boolean> {
      const result = await service.createDetail(detail);
      if (result && this.purchaseRate)
        await this.fetchPurchaseRateDetails(this.purchaseRate);
      return result;
    },

    async updatePurchaseRateDetail(detail: PurchaseRateDetail): Promise<boolean> {
      const result = await service.updateDetail(detail);
      if (result && this.purchaseRate)
        await this.fetchPurchaseRateDetails(this.purchaseRate);
      return result;
    },

    async deletePurchaseRateDetail(detail: PurchaseRateDetail): Promise<boolean> {
      const result = await service.deleteDetail(detail.id);
      if (result && this.purchaseRate)
        await this.fetchPurchaseRateDetails(this.purchaseRate);
      return result;
    },

    async duplicatePurchaseRate(rate: PurchaseRate, newName: string, validFrom: Date, validTo: Date): Promise<boolean> {
      const from = formatDateForQueryParameter(validFrom);
      const to = formatDateForQueryParameter(validTo);
      const result = await service.duplicate(rate.id, newName, from, to);
      if (result) await this.fetchPurchaseRatesBySupplierId(rate.supplierId);
      return result;
    }
  },
});
