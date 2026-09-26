import BaseService from "../../../api/base.service";
import { logException } from "../../../api/api.client";
import {
  Warehouse,
  Stock,
  StockListItem,
  Location,
  StockResponse,
  Lot,
} from "../types";

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
  async getAll(): Promise<StockListItem[]> {
    try {
      const response = await this.apiClient.get(this.resource);
      return response.data ?? [];
    } catch (err) {
      logException(err);
      return [];
    }
  }

  async getByReference(referenceId: string): Promise<StockListItem[]> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}?referenceId=${referenceId}`
      );
      return response.data ?? [];
    } catch (err) {
      logException(err);
      return [];
    }
  }

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
}

export class LotService extends BaseService<Lot> {
  // Lots oberts (no tancats) d'una referència, per seleccionar-los en un moviment manual
  async getOpenByReference(referenceId: string): Promise<Lot[]> {
    try {
      const response = await this.apiClient.get(
        `${this.resource}?referenceId=${referenceId}`
      );
      return response.data ?? [];
    } catch (err) {
      logException(err);
      return [];
    }
  }
}
