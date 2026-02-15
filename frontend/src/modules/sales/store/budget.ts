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
      statusId?: string,
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

      if (statusId && this.budgets) {
        this.budgets = this.budgets.filter((b) => b.statusId === statusId);
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
      return updated;
    },
    async DeleteDetail(detail: BudgetDetail): Promise<boolean> {
      const deleted = await SalesServices.Budget.DeleteDetail(detail);
      return deleted;
    },
  },
});
