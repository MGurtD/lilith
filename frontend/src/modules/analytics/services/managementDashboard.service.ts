import apiClient from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import type { ManagementDashboardResult } from "../types";

export class ManagementDashboardService extends BaseService<ManagementDashboardResult> {
  async GetDashboard(): Promise<ManagementDashboardResult | undefined> {
    const response = await apiClient.get(this.resource);
    if (response.status === 200) {
      return response.data as ManagementDashboardResult;
    }
  }
}
