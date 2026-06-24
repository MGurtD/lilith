import { defineStore } from "pinia";
import {
  CreateSalesHeaderRequest,
  Budget,
  BudgetDetail,
  SalesOrderHeader,
} from "../types";
import SalesServices from "../services";

export const useBudgetStore = defineStore({
  id: "budget",
  state: () => ({
    budget: undefined as Budget | undefined,
    budgets: undefined as Array<Budget> | undefined,
    order: undefined as SalesOrderHeader | undefined,
  }),
  getters: {},
  actions: {
    async GetById(id: string) {
      const data = await SalesServices.Budget.getById(id);
      if (data) {
        // Convert ISO date strings to Date objects for PrimeVue 4 DatePicker
        if (data.date) {
          data.date = new Date(data.date) as any;
        }
        if (data.acceptanceDate) {
          data.acceptanceDate = new Date(data.acceptanceDate) as any;
        }
      }
      this.budget = data;
    },
    async GetFiltered(
      startTime: string,
      endTime: string,
      customerId?: string,
      statusIds?: string[],
    ) {
      if (customerId) {
        this.budgets = await SalesServices.Budget.GetBetweenDatesAndCustomer(
          startTime,
          endTime,
          customerId,
        );
      } else {
        this.budgets = await SalesServices.Budget.GetBetweenDates(
          startTime,
          endTime,
        );
      }

      if (statusIds && statusIds.length > 0 && this.budgets) {
        this.budgets = this.budgets.filter((b) => statusIds.includes(b.statusId));
      }
    },
    async GetAssociatedSalesOrders(budgetId: string) {
      this.order = await SalesServices.SalesOrder.GetFromBudgetId(budgetId);
    },
    async Create(createRequest: CreateSalesHeaderRequest) {
      const created = await SalesServices.Budget.Create(createRequest);
      return created;
    },
    async Update(id: string, budget: Budget) {
      const updated = await SalesServices.Budget.update(id, budget);
      return updated;
    },
    async Delete(id: string): Promise<boolean> {
      const deleted = await SalesServices.Budget.delete(id);
      return deleted;
    },
    async CreateDetail(detail: BudgetDetail): Promise<boolean> {
      const created = await SalesServices.Budget.CreateDetail(detail);
      if (created) await this.GetById(detail.budgetId);
      return created;
    },
    async UpdateDetail(detail: BudgetDetail): Promise<boolean> {
      const updated = await SalesServices.Budget.UpdateDetail(detail);
      if (updated) await this.GetById(detail.budgetId);
      return updated;
    },
    async DeleteDetail(detail: BudgetDetail): Promise<boolean> {
      const deleted = await SalesServices.Budget.DeleteDetail(detail);
      if (deleted) await this.GetById(detail.budgetId);
      return deleted;
    },
    async CreateTransport(transport: any): Promise<boolean> {
      const created = await SalesServices.Budget.CreateTransport(transport);
      if (created) await this.GetById(transport.budgetId);
      return created;
    },
    async UpdateTransport(transport: any): Promise<boolean> {
      const updated = await SalesServices.Budget.UpdateTransport(transport);
      if (updated) await this.GetById(transport.budgetId);
      return updated;
    },
    async DeleteTransport(transport: any): Promise<boolean> {
      const deleted = await SalesServices.Budget.DeleteTransport(transport.id);
      if (deleted) await this.GetById(transport.budgetId);
      return deleted;
    },
    async DistributeTransportCosts(budgetId: string): Promise<boolean> {
      const result = await SalesServices.Budget.DistributeTransportCosts(budgetId);
      if (result) await this.GetById(budgetId);
      return result;
    },
    async DistributeAllCosts(budgetId: string): Promise<boolean> {
      const result = await SalesServices.Budget.DistributeAllCosts(budgetId);
      if (result) await this.GetById(budgetId);
      return result;
    },
    async UpdateExternalService(externalService: any): Promise<boolean> {
      const result =
        await SalesServices.Budget.UpdateExternalService(externalService);
      if (result) await this.GetById(externalService.budgetId);
      return result;
    },
    async Clone(id: string, newId: string): Promise<boolean> {
      return await SalesServices.Budget.Clone(id, newId);
    },
  },
});
