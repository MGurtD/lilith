import {
  Customer,
  CustomerContact,
  CustomerType,
  CustomerAddress,
} from "../types";
import BaseService from "../../../api/base.service";
import type { GenericResponse } from "../../../types";

export class CustomerTypeService extends BaseService<CustomerType> {}

export class CustomerService extends BaseService<Customer> {
  /**
   * Creates a Customer. Returns the backend's GenericResponse so the caller
   * can surface fiscal-validation errors (e.g. invalid CIF/NIF) instead of
   * silently failing — issue #69 follow-up.
   *
   * Named `createCustomer` (not overriding `BaseService.create`) because the
   * base returns `Promise<boolean>`; we need the full GenericResponse body.
   */
  async createCustomer(model: Customer): Promise<GenericResponse<Customer>> {
    const response = await this.apiClient.post(`${this.resource}`, model);
    return response.data as GenericResponse<Customer>;
  }

  /**
   * Updates a Customer. Returns the backend's GenericResponse so the caller
   * can surface fiscal-validation errors (e.g. invalid CIF/NIF) instead of
   * silently failing — issue #69 follow-up.
   *
   * Named `updateCustomer` (not overriding `BaseService.update`) for the same
   * reason as `createCustomer` above.
   */
  async updateCustomer(
    id: string,
    model: Customer,
  ): Promise<GenericResponse<Customer>> {
    const response = await this.apiClient.put(`${this.resource}/${id}`, model);
    return response.data as GenericResponse<Customer>;
  }

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
