import BaseService from "../../../api/base.service";
import { logException } from "../../../api/api.client";
import { Warehouse, Stock, Location, StockResponse, MoveStockToWorkcenterSupplyRequest } from "../types";

export class WarehouseService extends BaseService<Warehouse> {
  async getAllWithLocations(): Promise<Array<Warehouse>> {
    const endpoint = `${this.resource}/WithLocations`;
    const response = await this.apiClient.get(endpoint);
    return response.data;
  }
  async getBySite(siteId: string): Promise<Array<Warehouse>> {
    const endpoint = `${this.resource}/Site/${siteId}`;
    const response = await this.apiClient.get(endpoint);
    return response.data;
  }

  async createLocation(request: Location): Promise<boolean> {
    const endpoint = `${this.resource}/Location`;
    const response = await this.apiClient.post(endpoint, request);
    return response.status === 200;
  }

  async updateLocation(id: string, request: Location): Promise<boolean> {
    const endpoint = `${this.resource}/Location/${request.id}`;
    const response = await this.apiClient.put(endpoint, request);
    return response.status === 200;
  }

  async deleteLocation(id: string): Promise<boolean> {
    const endpoint = `${this.resource}/Location/${id}`;
    const response = await this.apiClient.delete(endpoint);
    return response.status === 200;
  }
}

export class StockService extends BaseService<Stock> {
  async getByBillOfMaterialsId(bomId: string): Promise<StockResponse[]> {
    try {
      const endpoint = `${this.resource}/ByBillOfMaterials/${bomId}`;
      const response = await this.apiClient.get(endpoint);
      return response.data ?? [];
    } catch (err) {
      logException(err);
      return [];
    }
  }

  async moveToWorkcenterSupply(request: MoveStockToWorkcenterSupplyRequest): Promise<boolean> {
    try {
      const endpoint = `${this.resource}/MoveToWorkcenterSupply`;
      const response = await this.apiClient.post(endpoint, request);
      return response.status === 200;
    } catch (err) {
      logException(err);
      return false;
    }
  }
}
