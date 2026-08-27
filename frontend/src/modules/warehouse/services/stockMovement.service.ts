import { logException } from "../../../api/api.client";
import BaseService from "../../../api/base.service";
import { StockMovement } from "../types";
import { GenericResponse } from "../../../types";

export class StockMovementService extends BaseService<StockMovement> {
  async createMovement(
    request: StockMovement
  ): Promise<GenericResponse<StockMovement>> {
    try {
      const endpoint = `${this.resource}`;
      const response = await this.apiClient.post(endpoint, request);
      // Retornem el cos de GenericResponse tant en 2xx com en 4xx perquè qui
      // truqui pugui inspeccionar `errors` (p. ex. "LotClosed"). apiClient
      // tracta 200-404 com a resolt, així que un 400 arriba amb el cos intacte.
      return response.data as GenericResponse<StockMovement>;
    } catch (err) {
      logException(err);
      return { result: false, errors: ["Error de connexió"] };
    }
  }

  async getBetweenDates(
    startTime: string,
    endTime: string,
    locationId?: string
  ): Promise<Array<StockMovement> | undefined> {
    const endpoint = `${
      this.resource
    }?startTime=${startTime}&endTime=${endTime}${
      locationId ? `&locationId=${locationId}` : ""
    }`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<StockMovement>;
    }
  }

  async getByWorkOrderId(
    workOrderId: string
  ): Promise<Array<StockMovement> | undefined> {
    try {
      const endpoint = `${this.resource}/ByWorkOrder/${workOrderId}`;
      const response = await this.apiClient.get(endpoint);
      if (response.status === 200) {
        return response.data as Array<StockMovement>;
      }
    } catch (err) {
      logException(err);
    }
  }
}
