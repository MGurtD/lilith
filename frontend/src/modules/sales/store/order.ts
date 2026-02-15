import { defineStore } from "pinia";
import {
  SalesOrderHeader,
  SalesOrderDetail,
  CreateSalesHeaderRequest,
  Budget,
} from "../types";
import SalesServices from "../services";
import { GenericResponse } from "../../../types";
import { convertDateTimeToJSON } from "../../../utils/functions";

export const useSalesOrderStore = defineStore({
  id: "salesOrder",
  state: () => ({
    salesOrder: undefined as SalesOrderHeader | undefined,
    salesOrders: undefined as Array<SalesOrderHeader> | undefined,
    salesOrdersToDeliver: undefined as Array<SalesOrderHeader> | undefined,
    createWorkOrderDialogVisibility: false,
  }),
  getters: {},
  actions: {
    async Create(createRequest: CreateSalesHeaderRequest) {
      const created = await SalesServices.SalesOrder.Create(createRequest);
      return created;
    },
    async CreateFromBudget(
      budget: Budget,
    ): Promise<GenericResponse<SalesOrderHeader>> {
      budget.date = convertDateTimeToJSON(budget.date);
      if (budget.acceptanceDate) {
        budget.acceptanceDate = convertDateTimeToJSON(budget.acceptanceDate);
      }

      const response = await SalesServices.SalesOrder.CreateFromBudget(budget);
      return response;
    },
    async GetById(id: string) {
      const data = await SalesServices.SalesOrder.getById(id);
      if (data) {
        // Convert ISO date strings to Date objects for PrimeVue 4 DatePicker
        if (data.date) {
          data.date = new Date(data.date) as any;
        }
        if (data.expectedDate) {
          data.expectedDate = new Date(data.expectedDate) as any;
        }
      }
      this.salesOrder = data;
    },
    async GetDetailsById(id: string) {
      const updatedOrder = await SalesServices.SalesOrder.getById(id);
      if (this.salesOrder && updatedOrder)
        this.salesOrder.salesOrderDetails = updatedOrder?.salesOrderDetails;
    },
    async GetFiltered(
      startTime: string,
      endTime: string,
      customerId?: string,
      statusId?: string,
    ) {
      if (customerId) {
        this.salesOrders =
          await SalesServices.SalesOrder.GetBetweenDatesAndCustomer(
            startTime,
            endTime,
            customerId,
          );
      } else {
        this.salesOrders = await SalesServices.SalesOrder.GetBetweenDates(
          startTime,
          endTime,
        );
      }

      if (statusId && this.salesOrders) {
        this.salesOrders = this.salesOrders.filter(
          (b) => b.statusId === statusId,
        );
      }
    },
    async GetByDeliveryNote(deliveryNoteId: string) {
      this.salesOrders =
        await SalesServices.SalesOrder.GetByDeliveryNote(deliveryNoteId);
    },
    async GetToDeliver(customerId: string) {
      this.salesOrdersToDeliver =
        await SalesServices.SalesOrder.GetToDeliver(customerId);
    },
    async Update(id: string, salesOrder: SalesOrderHeader) {
      const updated = await SalesServices.SalesOrder.update(id, salesOrder);
      return updated;
    },
    async Delete(id: string): Promise<boolean> {
      const deleted = await SalesServices.SalesOrder.delete(id);
      return deleted;
    },
    async CreateDetail(detail: SalesOrderDetail): Promise<boolean> {
      const created = await SalesServices.SalesOrder.CreateDetail(detail);
      if (created) await this.GetDetailsById(detail.salesOrderHeaderId);
      return created;
    },
    async UpdateDetail(detail: SalesOrderDetail): Promise<boolean> {
      const updated = await SalesServices.SalesOrder.UpdateDetail(detail);
      //if (updated) await this.GetDetailsById(detail.salesOrderHeaderId);
      return updated;
    },
    async DeleteDetail(detail: SalesOrderDetail): Promise<boolean> {
      const deleted = await SalesServices.SalesOrder.DeleteDetail(detail);
      if (deleted) await this.GetDetailsById(detail.salesOrderHeaderId);
      return deleted;
    },
  },
});
