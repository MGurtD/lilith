import axios, { type AxiosInstance } from "axios";
import apiClient, { logException } from "@/api/api.client";

const baseUrl = (import.meta.env.VITE_API_BASE_URL as string) || "";

/**
 * Plain Axios instance WITHOUT interceptors for the refresh endpoint.
 * This prevents infinite loops when the main client's 401 interceptor
 * triggers a refresh that itself might get a 401.
 */
const plainClient = axios.create({
  baseURL: baseUrl,
  timeout: 10000,
  headers: { "Content-Type": "application/json" },
});

export class AuthenticationService {
  public apiClient: AxiosInstance;
  private resource: string;

  constructor() {
    this.apiClient = apiClient;
    this.resource = "/Authentication";
  }

  public async Login(userLogin: UserLogin) {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/Login`,
        userLogin,
      );
      return response.data;
    } catch (error) {
      // Error handled by interceptor
    }
  }

  public async ChangePassword(
    request: ChangePasswordRequest,
  ): Promise<boolean> {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/ChangePassword`,
        request,
      );
      return response.status === 200;
    } catch (err) {
      logException(err);
    }
    return false;
  }

  public async Register(userRegister: UserRegister) {
    try {
      const response = await this.apiClient.post(
        `${this.resource}/Register`,
        userRegister,
      );
      return response.data;
    } catch (error) {
      console.error(error);
    }
  }

  /**
   * Refresh the JWT token using the expired token + refresh token.
   * Uses a plain Axios client to avoid triggering the 401 interceptor.
   */
  public async Refresh(
    token: string,
    refreshToken: string,
  ): Promise<any | undefined> {
    try {
      const response = await plainClient.post(
        `${this.resource}/RefreshToken`,
        { token, refreshToken },
      );
      return response.data;
    } catch (err) {
      logException(err);
      throw err;
    }
  }

  /**
   * Logout: revoke all refresh tokens for the current user.
   */
  public async Logout(): Promise<boolean> {
    try {
      const response = await this.apiClient.post(`${this.resource}/Logout`);
      return response.status === 200;
    } catch (err) {
      logException(err);
    }
    return false;
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

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface Role {
  id: string;
  name: string;
}
