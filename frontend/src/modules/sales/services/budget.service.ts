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

  async CreateTransport(request: any): Promise<boolean> {
    const endpoint = `${this.resource}/Transport`;
    const response = await apiClient.post(endpoint, request);
    return response.status === 200;
  }

  async UpdateTransport(request: any): Promise<boolean> {
    const endpoint = `${this.resource}/Transport/${request.id}`;
    const response = await apiClient.put(endpoint, request);
    return response.status === 200;
  }

  async DeleteTransport(id: string): Promise<boolean> {
    const endpoint = `${this.resource}/Transport/${id}`;
    const response = await apiClient.delete(endpoint);
    return response.status === 200;
  }

  async DistributeTransportCosts(budgetId: string): Promise<boolean> {
    const endpoint = `${this.resource}/Transport/DistributeCosts/${budgetId}`;
    const response = await apiClient.put(endpoint, {});
    return response.status === 200;
  }

  async DistributeAllCosts(budgetId: string): Promise<boolean> {
    const endpoint = `${this.resource}/DistributeAllCosts/${budgetId}`;
    const response = await apiClient.put(endpoint, {});
    return response.status === 200;
  }

  async UpdateExternalService(request: any): Promise<boolean> {
    const endpoint = `${this.resource}/ExternalService/${request.id}`;
    const response = await apiClient.put(endpoint, request);
    return response.status === 200;
  }

  async Clone(id: string, newId: string): Promise<boolean> {
    const endpoint = `${this.resource}/${id}/Clone`;
    const response = await apiClient.post(endpoint, { newId });
    return response.status === 200;
  }
}
