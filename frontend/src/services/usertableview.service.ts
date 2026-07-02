import { AxiosInstance } from "axios";
import apiClient from "@/api/api.client";
import { UserTableView } from "@/types";

export class UserTableViewService {
  public apiClient: AxiosInstance;
  private resource: string;
  constructor() {
    this.apiClient = apiClient;
    this.resource = "/UserTableView";
  }

  public async GetByUserId(userId: string): Promise<UserTableView[] | undefined> {
    let response = await this.apiClient.get(`${this.resource}/${userId}`);
    if (response.status === 200) {
      const models = response.data as UserTableView[];
      return models;
    }
  }

  public async GetByUserAndPage(
    userId: string,
    page: string
  ): Promise<UserTableView[] | undefined> {
    let response = await this.apiClient.get(`${this.resource}/${userId}/${page}`);
    if (response.status === 200) {
      const models = response.data as UserTableView[];
      return models;
    }
  }

  public async GetById(id: string): Promise<UserTableView | undefined> {
    let response = await this.apiClient.get(`${this.resource}/detail/${id}`);
    if (response.status === 200) {
      const model = response.data as UserTableView;
      return model;
    }
  }

  public async Create(model: UserTableView): Promise<boolean> {
    let response = await this.apiClient.post(`${this.resource}`, model);
    return response.status === 201;
  }

  public async Update(id: string, model: UserTableView): Promise<boolean> {
    let response = await this.apiClient.put(`${this.resource}/${id}`, model);
    return response.status === 200;
  }

  public async Delete(id: string): Promise<boolean> {
    let response = await this.apiClient.delete(`${this.resource}/${id}`);
    return response.status === 200;
  }

  public async SetDefault(id: string, isDefault: boolean): Promise<boolean> {
    let response = await this.apiClient.patch(
      `${this.resource}/${id}/default?isDefault=${isDefault}`
    );
    return response.status === 200;
  }
}