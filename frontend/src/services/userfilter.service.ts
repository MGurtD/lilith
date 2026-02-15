import { AxiosInstance } from "axios";
import apiClient from "@/api/api.client";
import { UserFilter } from "@/types";

export class UserFilterService {
  public apiClient: AxiosInstance;
  private resource: string;
  constructor() {
    this.apiClient = apiClient;
    this.resource = "/UserFilter";
  }

  public async GetByUserId(userId: string): Promise<UserFilter[] | undefined> {
    let response = await this.apiClient.get(`${this.resource}/${userId}`);
    if (response.status === 200) {
      const models = response.data as UserFilter[];
      return models;
    }
  }

  public async Get(
    userId: string,
    page: string,
    key: string
  ): Promise<UserFilter | undefined> {
    var endpoint = `${this.resource}/${userId}/${page}/${key}`;
    let response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      const model = response.data as UserFilter;
      return model;
    }
  }

  public async CreateOrUpdate(model: UserFilter): Promise<boolean> {
    let response = await this.apiClient.post(`${this.resource}`, model);
    return response.status === 200;
  }

  public async Delete(model: UserFilter): Promise<boolean> {
    let response = await this.apiClient.delete(`${this.resource}/${model.id}`);
    return response.status === 200;
  }
}
