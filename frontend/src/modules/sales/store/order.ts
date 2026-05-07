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

const normalizeDateForApi = (value: unknown) => {
  if (!value) return value;

  if (value instanceof Date) {
    return convertDateTimeToJSON(new Date(value));
  }

  if (typeof value === "string") {
    if (/^\d{4}-\d{2}-\d{2}T/.test(value)) {
      return value;
    }

    return convertDateTimeToJSON(value) ?? value;
  }

  return value;
};

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
    async Create(
      createRequest: CreateSalesHeaderRequest,
    ): Promise<GenericResponse<SalesOrderHeader>> {
      const response = await SalesServices.SalesOrder.Create(createRequest);
      return response;
    },
    async CreateFromBudget(
      budget: Budget,
    ): Promise<GenericResponse<SalesOrderHeader>> {
      const payload: Budget = {
        ...budget,
        date: normalizeDateForApi(budget.date),
        acceptanceDate: normalizeDateForApi(budget.acceptanceDate),
        details: budget.details?.map((detail) => ({ ...detail })),
        transports: budget.transports?.map((t) => ({ ...t })),
        externalServices: budget.externalServices?.map((es) => ({
          ...es,
          details: es.details?.map((d) => ({ ...d })),
        })),
      };

      const response = await SalesServices.SalesOrder.CreateFromBudget(payload);
      return response;
    },
    async GetFromBudgetId(budgetId: string) {
      const data = await SalesServices.SalesOrder.GetFromBudgetId(budgetId);
      return data;
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
      if (this.salesOrder && updatedOrder) {
        this.salesOrder.salesOrderDetails = updatedOrder?.salesOrderDetails;
        this.salesOrder.externalServices = updatedOrder?.externalServices;
        this.salesOrder.transports = updatedOrder?.transports;
      }
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
    async CreateTransport(transport: any): Promise<boolean> {
      const created = await SalesServices.SalesOrder.CreateTransport(transport);
      if (created) await this.GetById(transport.salesOrderHeaderId);
      return created;
    },
    async UpdateTransport(transport: any): Promise<boolean> {
      const updated = await SalesServices.SalesOrder.UpdateTransport(transport);
      if (updated) await this.GetById(transport.salesOrderHeaderId);
      return updated;
    },
    async DeleteTransport(
      id: string,
      salesOrderId: string
    ): Promise<boolean> {
      const deleted = await SalesServices.SalesOrder.DeleteTransport(id);
      if (deleted) await this.GetById(salesOrderId);
      return deleted;
    },
    async DistributeTransportCosts(salesOrderId: string): Promise<boolean> {
      const distributed =
        await SalesServices.SalesOrder.DistributeTransportCosts(salesOrderId);
      if (distributed) await this.GetById(salesOrderId);
      return distributed;
    },
    async DistributeAllCosts(salesOrderId: string): Promise<boolean> {
      const distributed =
        await SalesServices.SalesOrder.DistributeAllCosts(salesOrderId);
      if (distributed) await this.GetById(salesOrderId);
      return distributed;
    },
    async UpdateExternalService(externalService: any): Promise<boolean> {
      const updated = await SalesServices.SalesOrder.UpdateExternalService(
        externalService
      );
      if (updated) await this.GetById(externalService.salesOrderHeaderId);
      return updated;
    },
  },
});
