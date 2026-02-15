import { AxiosInstance } from "axios";
import apiClient from "@/api/api.client";
import { Role } from "@/types";

export class RoleService {
  public apiClient: AxiosInstance;
  private resource: string;
  constructor() {
    this.apiClient = apiClient;
    this.resource = "/role";
  }

  public async GetAll() {
    try {
      let response = await this.apiClient.get(this.resource);
      if (response.status === 200) {
        const roles = response.data as Array<Role>;
        return roles;
      }
    } catch (error) {}
  }

  public async GetById(id: string) {
    try {
      let response = await this.apiClient.get(`${this.resource}/${id}`);
      if (response.status === 200) {
        const role = response.data as Role;
        return role;
      }
    } catch (error) {}
  }
}
