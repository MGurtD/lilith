import { AxiosInstance } from "axios";
import apiClient, { logException } from "./api.client";
import { CreatePurchaseDocumentRequest } from "../modules/purchase/types";

export default abstract class BaseService<T> {
  public apiClient: AxiosInstance;
  constructor(public resource: string) {
    this.apiClient = apiClient;
  }

  async getAll(): Promise<Array<T> | undefined> {
    try {
      const response = await apiClient.get(this.resource);
      if (response.status === 200) {
        return response.data as Array<T>;
      }
    } catch (err) {
      logException(err);
    }
  }
  async getActive(): Promise<Array<T> | undefined> {
    const response = await this.getAll();
    if (response && response.length > 0)
      return response.filter((e) => (e as any).disabled === false);
  }
  async getById(id: string): Promise<T | undefined> {
    const response = await apiClient.get(`${this.resource}/${id}`);
    if (response.status === 200) {
      return response.data as T;
    }
  }
  async create(model: T | CreatePurchaseDocumentRequest): Promise<boolean> {
    let result: boolean = false;
    const response = await apiClient.post(`${this.resource}`, model);
    if (response.status === 200 || response.status === 201) {
      result = true;
    }

    return result;
  }
  async update(id: string, model: T) {
    let result: boolean = false;

    const response = await apiClient.put(`${this.resource}/${id}`, model);
    if (response.status === 200 || response.status === 201) {
      result = true;
    }
    return result;
  }
  async delete(id: string) {
    let result: boolean = false;
    const response = await apiClient.delete(`${this.resource}/${id}`);
    if (response.status === 200 || response.status === 201) {
      result = true;
    }

    return result;
  }
}
