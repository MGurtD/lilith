import { defineStore } from "pinia";
import { TransportRate, TransportRateDetail } from "../types";
import { TransportRateService } from "../services/transportRate.service";
import { getNewUuid, formatDateForQueryParameter } from "@/utils/functions";

const service = new TransportRateService("/TransportRate");

export const useTransportRateStore = defineStore({
  id: "transportRate",
  state: () => ({
    transportRates: undefined as Array<TransportRate> | undefined,
    transportRate: undefined as TransportRate | undefined,
    transportRateDetails: undefined as Array<TransportRateDetail> | undefined,
  }),
  actions: {
    setNewTransportRate(supplierId: string): TransportRate {
      const rate: TransportRate = {
        id: getNewUuid(),
        name: "",
        description: "",
        supplierId: supplierId,
        validFrom: new Date(),
        validTo: new Date(new Date().setFullYear(new Date().getFullYear() + 1)),
        disabled: false,
        details: [],
      };
      return rate;
    },

    setNewTransportRateDetail(transportRateId: string): TransportRateDetail {
      return {
        id: getNewUuid(),
        transportRateId: transportRateId,
        minWeight: 0,
        maxWeight: 0,
        minVolume: 0,
        maxVolume: 0,
        minDistance: 0,
        maxDistance: 0,
        price: 0,
        disabled: false,
      };
    },

    async fetchTransportRatesBySupplierId(supplierId: string) {
      this.transportRates = await service.getBySupplierId(supplierId);
      this.transportRateDetails = undefined;
      this.transportRate = undefined;
    },

    async fetchTransportRateDetails(transportRate: TransportRate) {
      this.transportRate = transportRate;
      this.transportRateDetails = await service.getDetails(transportRate.id);
    },

    async createTransportRate(rate: TransportRate): Promise<boolean> {
      const model = { ...rate };
      model.validFrom = formatDateForQueryParameter(new Date(model.validFrom));
      model.validTo = formatDateForQueryParameter(new Date(model.validTo));

      const result = await service.create(model as any);
      if (result) await this.fetchTransportRatesBySupplierId(rate.supplierId);
      return result;
    },

    async updateTransportRate(rate: TransportRate): Promise<boolean> {
      const model = { ...rate };
      model.validFrom = formatDateForQueryParameter(new Date(model.validFrom));
      model.validTo = formatDateForQueryParameter(new Date(model.validTo));

      const result = await service.update(model.id, model as any);
      if (result) await this.fetchTransportRatesBySupplierId(rate.supplierId);
      return result;
    },


    async deleteTransportRate(rate: TransportRate): Promise<boolean> {
      const result = await service.delete(rate.id);
      if (result) {
        await this.fetchTransportRatesBySupplierId(rate.supplierId);
        if (this.transportRate?.id === rate.id) {
          this.transportRate = undefined;
          this.transportRateDetails = undefined;
        }
      }
      return result;
    },

    async createTransportRateDetail(detail: TransportRateDetail): Promise<boolean> {
      const result = await service.createDetail(detail);
      if (result && this.transportRate)
        await this.fetchTransportRateDetails(this.transportRate);
      return result;
    },

    async updateTransportRateDetail(detail: TransportRateDetail): Promise<boolean> {
      const result = await service.updateDetail(detail);
      if (result && this.transportRate)
        await this.fetchTransportRateDetails(this.transportRate);
      return result;
    },

    async deleteTransportRateDetail(detail: TransportRateDetail): Promise<boolean> {
      const result = await service.deleteDetail(detail.id);
      if (result && this.transportRate)
        await this.fetchTransportRateDetails(this.transportRate);
      return result;
    },
  },
});
