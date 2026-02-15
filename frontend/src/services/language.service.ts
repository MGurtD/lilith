import { AxiosInstance } from "axios";
import apiClient, { logException } from "@/api/api.client";
import { Language } from "@/types";

export class LanguageService {
  public apiClient: AxiosInstance;
  private resource: string;
  constructor() {
    this.apiClient = apiClient;
    this.resource = "/language";
  }

  public async GetAll(): Promise<Language[] | undefined> {
    try {
      const response = await this.apiClient.get(this.resource);
      if (response.status === 200) return response.data as Language[];
    } catch (error) {
      logException(error);
    }
  }

  public async GetByCode(code: string): Promise<Language | undefined> {
    try {
      const response = await this.apiClient.get(`${this.resource}/${code}`);
      if (response.status === 200) return response.data as Language;
    } catch (error) {
      logException(error);
    }
  }

  public async GetDefault(): Promise<Language | undefined> {
    try {
      const response = await this.apiClient.get(`${this.resource}/default`);
      if (response.status === 200) return response.data as Language;
    } catch (error) {
      logException(error);
    }
  }
}

export default LanguageService;
