import { defineStore } from "pinia";
import VerifactuService from "../services";
import {
  FindInvoicesResponse,
  VerifactuInvoice,
  VerifactuInvoiceRequest,
} from "../types";
import { GenericResponse } from "../../../types";

export const useVerifactuStore = defineStore({
  id: "verifactu",
  state: () => ({
    invoices: [] as VerifactuInvoice[],
    pendingInvoices: [] as any[],
    integrationsBetweenDates: [] as any[],
    invoiceRequests: [] as VerifactuInvoiceRequest[],
    loading: false as boolean,
  }),
  getters: {},
  actions: {
    async FindInvoices(
      month: number,
      year: number
    ): Promise<FindInvoicesResponse | undefined> {
      this.loading = true;
      try {
        const response = await VerifactuService.Verifactu.FindInvoices(
          month,
          year
        );
        if (response) {
          this.invoices = response.invoices;
          return response;
        }
      } finally {
        this.loading = false;
      }
    },

    async GetPendingIntegration(
      toDate?: string | Date
    ): Promise<any[] | undefined> {
      this.loading = true;
      try {
        const response = await VerifactuService.Verifactu.GetPendingIntegration(
          toDate
        );
        if (response) {
          this.pendingInvoices = response;
          return response;
        }
      } finally {
        this.loading = false;
      }
    },

    async GetIntegrationsBetweenDates(
      fromDate: string | Date,
      toDate: string | Date
    ): Promise<any[] | undefined> {
      this.loading = true;
      try {
        const response =
          await VerifactuService.Verifactu.GetIntegrationsBetweenDates(
            fromDate,
            toDate
          );
        if (response) {
          this.integrationsBetweenDates = response;
          return response;
        }
      } finally {
        this.loading = false;
      }
    },

    async GetInvoiceRequests(
      invoiceId: string
    ): Promise<VerifactuInvoiceRequest[] | undefined> {
      const response = await VerifactuService.Verifactu.GetInvoiceRequests(
        invoiceId
      );
      if (response) {
        this.invoiceRequests = response;
        return response;
      }
    },

    async SendToVerifactu(
      invoiceId: string
    ): Promise<GenericResponse<any> | undefined> {
      const response = await VerifactuService.Verifactu.SendToVerifactu(
        invoiceId
      );
      return response;
    },

    async RemoveFromVerifactu(
      invoiceId: string
    ): Promise<GenericResponse<any> | undefined> {
      const response = await VerifactuService.Verifactu.RemoveFromVerifactu(
        invoiceId
      );
      return response;
    },
  },
});
