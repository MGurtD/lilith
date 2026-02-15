import { defineStore } from "pinia";
import Services from "../services";
import { ExpenseType, Expense } from "../types";

export const useExpenseStore = defineStore({
  id: "expense",
  state: () => ({
    expenseType: undefined as ExpenseType | undefined,
    expenseTypes: undefined as Array<ExpenseType> | undefined,
    expense: undefined as Expense | undefined,
    expenses: undefined as Array<Expense> | undefined,
  }),
  getters: {},
  actions: {
    //Expense Types
    setNewExpenseType(id: string) {
      this.expenseType = {
        id: id,
        name: "",
        description: "",
        disabled: false,
      } as ExpenseType;
    },
    async fetchExpenseTypes() {
      this.expenseTypes = await Services.ExpenseType.getAll();
    },
    async fetchExpenseType(id: string) {
      this.expenseType = await Services.ExpenseType.getById(id);
    },
    async createExpenseType(expenseType: ExpenseType) {
      const result = await Services.ExpenseType.create(expenseType);
      if (result) await this.fetchExpenseTypes();
      return result;
    },
    async updateExpenseType(id: string, expenseType: ExpenseType) {
      const result = await Services.ExpenseType.update(id, expenseType);
      if (result) await this.fetchExpenseTypes();
      return result;
    },
    async deleteExpenseType(id: string) {
      const result = await Services.ExpenseType.delete(id);
      if (result) await this.fetchExpenseTypes();
      return result;
    },
    //Expenses
    setNewExpense(id: string) {
      this.expense = {
        id: id,
        description: "",
        creationDate: new Date(),
        paymentDate: new Date(),
        endDate: new Date(),
        amount: 0,
        recurring: false,
        frecuency: 0,
        paymentDay: 0,
        relatedExpenseId: "",
        expenseTypeId: "",
        disabled: false,
      } as Expense;
    },
    async fetchExpenses(
      startDate: string,
      endDate: string,
      expenseTypeId?: string,
    ) {
      this.expenses = await Services.Expense.getBetweenDatesAndType(
        startDate,
        endDate,
        expenseTypeId,
      );
    },
    async fetchExpense(id: string) {
      const data = await Services.Expense.getById(id);
      if (data) {
        // Convert ISO date strings to Date objects for PrimeVue 4 DatePicker
        if (data.creationDate) {
          data.creationDate = new Date(data.creationDate) as any;
        }
        if (data.paymentDate) {
          data.paymentDate = new Date(data.paymentDate) as any;
        }
        if (data.endDate) {
          data.endDate = new Date(data.endDate) as any;
        }
      }
      this.expense = data;
    },
    async getFiltered(id: string) {
      this.expenses = await Services.Expense.getByExpenseType(id);
    },
    async createExpense(expense: Expense) {
      const result = await Services.Expense.create(expense);
      if (result) {
        this.expenses?.push(expense);
      }
      return result;
    },
    async updateExpense(id: string, expense: Expense) {
      const result = await Services.Expense.update(id, expense);
      return result;
    },
    async deleteExpense(id: string) {
      const result = await Services.Expense.delete(id);
      if (result) {
        this.expenses = this.expenses?.filter((e) => e.id !== id);
      }
      return result;
    },
  },
});
