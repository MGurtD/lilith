import {
  Budget,
  BudgetDetail,
  CreateSalesHeaderRequest,
  SalesOrderReport,
} from "../types";
import apiClient from "../../../api/api.client";
import BaseService from "../../../api/base.service";

export class BudgetService extends BaseService<Budget> {
  async Create(request: CreateSalesHeaderRequest): Promise<boolean> {
    const endpoint = `${this.resource}`;
    const response = await this.apiClient.post(endpoint, request);
    return response.status === 200;
  }

  async GetBetweenDates(
    startTime: string,
    endTime: string
  ): Promise<Array<Budget> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Budget>;
    }
  }

  async GetBetweenDatesAndCustomer(
    startTime: string,
    endTime: string,
    customerId: string
  ): Promise<Array<Budget> | undefined> {
    const endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}&customerId=${customerId}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Budget>;
    }
  }

  async GetReportDataById(id: string) {
    const endpoint = `${this.resource}/Report/${id}`;
    const response = await apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as SalesOrderReport;
    }
  }

  async CreateDetail(request: BudgetDetail): Promise<boolean> {
    const endpoint = `${this.resource}/Detail`;
    const response = await apiClient.post(endpoint, request);
    return response.status === 200;
  }

  async UpdateDetail(request: BudgetDetail): Promise<boolean> {
    const endpoint = `${this.resource}/Detail/${request.id}`;
    const response = await apiClient.put(endpoint, request);
    return response.status === 200;
  }

  async DeleteDetail(request: BudgetDetail): Promise<boolean> {
    const endpoint = `${this.resource}/Detail/${request.id}`;
    const response = await apiClient.delete(endpoint);
    return response.status === 200;
  }
}
