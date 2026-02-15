import { AxiosInstance } from "axios";
import apiClient, { logException } from "@/api/api.client";

export class AuthenticationService {
  public apiClient: AxiosInstance;
  private resource: string;
  constructor() {
    this.apiClient = apiClient;
    this.resource = "/Authentication";
  }

  public async Login(UserLogin: UserLogin) {
    try {
      let response = await this.apiClient.post(
        `${this.resource}/Login`,
        UserLogin
      );

      return response.data;
    } catch (error) {}
  }

  public async ChangePassword(UserLogin: UserLogin): Promise<boolean> {
    try {
      let response = await this.apiClient.post(
        `${this.resource}/ChangePassword`,
        UserLogin
      );
      return response.status === 200;
    } catch (err) {
      logException(err);
    }
    return false;
  }

  public async Register(UserRegister: UserRegister) {
    try {
      let response = await this.apiClient.post(
        `${this.resource}/Register`,
        UserRegister
      );

      return response.data;
    } catch (error) {
      console.error(error);
    }
  }

  public async Refresh(refreshToken: string) {
    try {
      const response = await this.apiClient.post(`${this.resource}/Refresh`, {
        refreshToken,
      });
      return response.data;
    } catch (err) {
      logException(err);
    }
  }
}

export interface UserRegister {
  username: string;
  password: string;
  repeatPassword: string;
  firstName: string;
  lastName: string;
  mail: string;
}

export interface UserLogin {
  username: string;
  password: string;
}

export interface Role {
  id: string;
  name: string;
}
