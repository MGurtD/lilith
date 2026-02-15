import {
  Supplier,
  SupplierContact,
  SupplierReference,
  SupplierType,
} from "../types";
import BaseService from "../../../api/base.service";

export class SupplierService extends BaseService<Supplier> {
  async getReference(id: string) {
    const response = await this.apiClient.get(
      `${this.resource}/Reference/${id}`
    );
    return response.data as SupplierReference;
  }
  async getReferences(supplierId: string) {
    const response = await this.apiClient.get(
      `${this.resource}/${supplierId}/Reference`
    );
    return response.data as Array<SupplierReference>;
  }
  async addReference(model: SupplierReference): Promise<boolean> {
    const response = await this.apiClient.post(
      `${this.resource}/Reference`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async updateReference(model: SupplierReference): Promise<boolean> {
    const response = await this.apiClient.put(
      `${this.resource}/Reference/${model.id}`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async removeReference(id: string): Promise<boolean> {
    const response = await this.apiClient.delete(
      `${this.resource}/Reference/${id}`
    );
    return response.status === 200 || response.status === 201;
  }

  async addContact(model: SupplierContact): Promise<boolean> {
    const response = await this.apiClient.post(
      `${this.resource}/Contact`,
      model
    );
    return response.status === 200 || response.status === 201;
  }
  async updateContact(model: SupplierContact): Promise<boolean> {
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
  async getSuppliersReferenceById(id: string): Promise<Array<Supplier> | null> {
    const response = await this.apiClient.get(
      `${this.resource}/GetByReference/${id}`
    );
    if (response.status === 200) {
      return response.data;
    }
    return null;
  }
  async getSupplierReferenceBySupplierIdAndReferenceId(
    supplierId: string,
    referenceId: string
  ): Promise<SupplierReference> {
    const response = await this.apiClient.get(
      `${this.resource}/${supplierId}/Reference/${referenceId}`
    );
    return response.data;
  }
}
export class SupplierTypeService extends BaseService<SupplierType> {}
