import { defineStore } from "pinia";
import { PaymentMethod } from "../types";
import Services from "../services";

export const usePaymentMethodStore = defineStore({
  id: "paymentMethod",
  state: () => ({
    paymentMethods: undefined as Array<PaymentMethod> | undefined,
    paymentMethod: undefined as PaymentMethod | undefined,
  }),
  getters: {},
  actions: {
    setNew(id: string) {
      this.paymentMethod = {
        id: id,
        name: "",
        description: "",
        disabled: false,
        dueDays: 0,
        paymentDay: 1,
        numberOfPayments: 1,
        frequency: 0,
      } as PaymentMethod;
    },
    async fetchAll() {
      this.paymentMethods = await Services.PaymentMethod.getAll();
    },
    async fetchOne(id: string) {
      this.paymentMethod = await Services.PaymentMethod.getById(id);
    },
    async create(model: PaymentMethod) {
      const result = await Services.PaymentMethod.create(model);
      if (result) await this.fetchAll();
      return result;
    },
    async update(id: string, model: PaymentMethod) {
      const result = await Services.PaymentMethod.update(id, model);
      if (result) await this.fetchAll();
      return result;
    },
    async delete(id: string) {
      const result = await Services.PaymentMethod.delete(id);
      if (result) await this.fetchAll();
      return result;
    },
  },
});
