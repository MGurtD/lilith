import BaseService from "../../../api/base.service";
import { GenericResponse } from "../../../types";
import { SupplierReference } from "../../purchase/types";
import { Reference, ReferenceFormat } from "../types";

export class ReferenceService extends BaseService<Reference> {
  async getByModule(
    module: string,
    category?: string
  ): Promise<Array<Reference> | undefined> {
    let endpoint = `${this.resource}/${module}`;
    if (category) {
      endpoint += `/${category}`;
    }
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<Reference>;
    }
  }
  async getSalesReferencesByCustomer(
    customerId: string
  ): Promise<Array<Reference> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/sales/customer/${customerId}`
    );
    if (response.status === 200) {
      return response.data as Array<Reference>;
    }
  }
  async getReferenceFormats(): Promise<Array<ReferenceFormat> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Formats`);
    if (response.status === 200) {
      return response.data as Array<ReferenceFormat>;
    }
  }

  async getReferenceFormatById(
    id: string
  ): Promise<ReferenceFormat | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Formats/${id}`);
    if (response.status === 200) {
      return response.data as ReferenceFormat;
    }
  }

  async deleteReference(id: string): Promise<GenericResponse<Reference>> {
    const response = await this.apiClient.delete(`${this.resource}/${id}`);
    if (response.status === 200) {
      return response.data as GenericResponse<Reference>;
    } else {
      return response.data as GenericResponse<Reference>;
    }
  }

  async getReferenceSuppliers(referenceId: string) {
    const response = await this.apiClient.get(
      `${this.resource}/${referenceId}/Suppliers`
    );
    return response.data as Array<SupplierReference>;
  }

  async getPrice(_referenceId: string, _supplierId: string): Promise<number> {
    interface iRequest {
      referenceId: string;
      supplierId: string;
    }
    const request: iRequest = {
      referenceId: _referenceId,
      supplierId: _supplierId,
    };
    const response = await this.apiClient.post(
      `${this.resource}/GetPrice`,
      request
    );
    return response.data;
  }
  async addSupplier(model: SupplierReference): Promise<boolean> {
    const response = await this.apiClient.post(`Supplier/Reference`, model);
    return response.status === 200 || response.status === 201;
  }
  async updateSupplier(model: SupplierReference): Promise<boolean> {
    const response = await this.apiClient.put(
      `Supplier/Reference/${model.id}`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async removeSupplier(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(`Supplier/Reference/${id}`);
    return response.status === 200 || response.status === 201;
  }
}
