import {
  Customer,
  CustomerContact,
  CustomerType,
  CustomerAddress,
} from "../types";
import BaseService from "../../../api/base.service";

export class CustomerTypeService extends BaseService<CustomerType> {}

export class CustomerService extends BaseService<Customer> {
  async addContact(model: CustomerContact): Promise<boolean> {
    const response = await this.apiClient.post(
      `${this.resource}/Contact`,
      model
    );
    return response.status === 200 || response.status === 201;
  }

  async updateContact(model: CustomerContact): Promise<boolean> {
    const response = await this.apiClient.put(
      `${this.resource}/Contact/${model.id}`,
      model
    );
    return response.status === 200 || response.status === 201;
  }

  async removeContact(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(
      `${this.resource}/Contact/${id}`
    );
    return response.status === 200 || response.status === 201;
  }

  async addAddress(model: CustomerAddress): Promise<boolean> {
    const response = await this.apiClient.post(
      `${this.resource}/Address`,
      model
    );
    return response.status === 200 || response.status === 201;
  }

  async updateAddress(model: CustomerAddress): Promise<boolean> {
    const response = await this.apiClient.put(
      `${this.resource}/Address/${model.id}`,
      model
    );
    return response.status === 200 || response.status === 201;
  }

  async removeAddress(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(
      `${this.resource}/Address/${id}`
    );
    return response.status === 200 || response.status === 201;
  }
}
