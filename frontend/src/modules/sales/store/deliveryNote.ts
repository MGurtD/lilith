import { defineStore } from "pinia";
import {
  DeliveryNote,
  DeliveryNoteDetail,
  CreateSalesHeaderRequest,
  SalesOrderHeader,
} from "../types";
import SalesServices from "../services";
import { GenericResponse } from "../../../types";

export const useDeliveryNoteStore = defineStore({
  id: "deliveryNote",
  state: () => ({
    deliveryNote: undefined as DeliveryNote | undefined,
    deliveryNotes: undefined as Array<DeliveryNote> | undefined,
    invoiceableDeliveryNotes: undefined as Array<DeliveryNote> | undefined,
  }),
  getters: {},
  actions: {
    async GetById(id: string) {
      const data = await SalesServices.DeliveryNote.getById(id);
      if (data) {
        // Convert ISO date string to Date object for PrimeVue 4 DatePicker
        if (data.deliveryDate) {
          data.deliveryDate = new Date(data.deliveryDate) as any;
        }
      }
      this.deliveryNote = data;
    },
    async GetDetailsById(id: string) {
      const updatedDeliveryNote = await SalesServices.DeliveryNote.getById(id);
      if (this.deliveryNote && updatedDeliveryNote) {
        this.deliveryNote.details = [];
        this.deliveryNote.details = updatedDeliveryNote?.details;
      }
    },
    async GetFiltered(startTime: string, endTime: string, customerId?: string) {
      if (customerId) {
        this.deliveryNotes =
          await SalesServices.DeliveryNote.GetBetweenDatesAndCustomer(
            startTime,
            endTime,
            customerId,
          );
      } else {
        this.deliveryNotes = await SalesServices.DeliveryNote.GetBetweenDates(
          startTime,
          endTime,
        );
      }
    },

    async GetByInvoiceId(invoiceId: string) {
      this.deliveryNotes =
        await SalesServices.DeliveryNote.GetByInvoiceId(invoiceId);
    },
    async GetToInvoice(customerId: string) {
      this.invoiceableDeliveryNotes =
        await SalesServices.DeliveryNote.GetToInvoice(customerId);
    },

    async Create(createRequest: CreateSalesHeaderRequest) {
      const response = await SalesServices.DeliveryNote.Create(createRequest);
      return response.result;
    },
    async Update(id: string, salesOrder: DeliveryNote) {
      const updated = await SalesServices.DeliveryNote.update(id, salesOrder);
      return updated;
    },
    async Delete(id: string): Promise<boolean> {
      const deleted = await SalesServices.DeliveryNote.delete(id);
      return deleted;
    },

    async AddOrder(
      id: string,
      order: SalesOrderHeader,
    ): Promise<GenericResponse<any>> {
      const response = await SalesServices.DeliveryNote.AddOrder(id, order);
      await this.GetDetailsById(id);
      return response;
    },
    async DeleteOrder(
      id: string,
      order: SalesOrderHeader,
    ): Promise<GenericResponse<any>> {
      const response = await SalesServices.DeliveryNote.DeleteOrder(id, order);
      await this.GetDetailsById(id);
      return response;
    },
  },
});
