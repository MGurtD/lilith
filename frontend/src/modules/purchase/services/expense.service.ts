import { ExpenseType, Expense, ConsolidatedExpense } from "../types";
import BaseService from "../../../api/base.service";

export class ExpenseTypeService extends BaseService<ExpenseType> {}

export class ExpenseService extends BaseService<Expense> {
  async getBetweenDatesAndType(
    startTime: string,
    endTime: string,
    expenseTypeId?: string
  ): Promise<Array<Expense> | undefined> {
    let endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    if (expenseTypeId) {
      endpoint += `&expenseTypeId=${expenseTypeId}`;
    }
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Expense>;
    }
  }
  async getConsolidated(
    startTime: string,
    endTime: string,
    type: string,
    typeDetail: string
  ): Promise<Array<ConsolidatedExpense> | undefined> {
    let endpoint = `${this.resource}/Consolidated?startTime=${startTime}&endTime=${endTime}&type=${type}&typeDetail=${typeDetail}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<ConsolidatedExpense>;
    }
  }
  async getByExpenseType(
    expensetypeid: string
  ): Promise<Array<Expense> | undefined> {
    const endpoint = `${this.resource}/ExpenseType/${expensetypeid}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Expense>;
    }
  }
}
