import { AxiosInstance } from "axios";
import apiClient, { logException } from "@/api/api.client";
import { User } from "@/types";

export class UserService {
  public apiClient: AxiosInstance;
  private resource: string;
  constructor() {
    this.apiClient = apiClient;
    this.resource = "/User";
  }

  public async GetAll(): Promise<User[] | undefined> {
    try {
      let response = await this.apiClient.get(this.resource);
      if (response.status === 200) {
        const models = response.data as Array<User>;
        return models;
      }
    } catch (error) {
      logException(error);
    }
  }

  public async GetById(id: string): Promise<User | undefined> {
    try {
      let response = await this.apiClient.get(`${this.resource}/${id}`);
      if (response.status === 200) {
        return response.data as User;
      }
    } catch (error) {
      logException(error);
    }
  }

  public async Update(user: User): Promise<boolean> {
    try {
      let response = await this.apiClient.put(
        `${this.resource}/${user.id}`,
        user
      );
      if (response.status === 200) {
        return true;
      }
    } catch (error) {
      logException(error);
    }
    return false;
  }

  public async CreateManaged(
    request: CreateManagedUserRequest,
  ): Promise<User | undefined> {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/managed`,
        request,
      );
      if (response.status === 201 || response.status === 200) {
        return response.data as User;
      }
    } catch (error) {
      logException(error);
    }
  }
}

export interface CreateManagedUserRequest {
  username: string;
  password: string;
  repeatPassword: string;
  firstName: string;
  lastName: string;
  email: string;
  preferredLanguage: string;
  roleId: string;
  profileId?: string | null;
}
