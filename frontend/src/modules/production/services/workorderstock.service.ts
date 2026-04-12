import BaseService from "../../../api/base.service";
import { logException } from "../../../api/api.client";
import {
  MoveStockToWorkcenterSupplyRequest,
  ReturnStockFromSupplyRequest,
  ConsumePhaseStockRequest,
  StockMovement,
} from "../../warehouse/types";

export class WorkOrderStockService extends BaseService<never> {
  async moveToWorkcenterSupply(
    request: MoveStockToWorkcenterSupplyRequest,
  ): Promise<boolean> {
    try {
      const endpoint = `${this.resource}/MoveToWorkcenterSupply`;
      const response = await this.apiClient.post(endpoint, request);
      return response.status === 200;
    } catch (err) {
      logException(err);
      return false;
    }
  }

  async returnFromWorkcenterSupply(
    request: ReturnStockFromSupplyRequest,
  ): Promise<boolean> {
    try {
      const endpoint = `${this.resource}/ReturnFromWorkcenterSupply`;
      const response = await this.apiClient.post(endpoint, request);
      return response.status === 200;
    } catch (err) {
      logException(err);
      return false;
    }
  }

  async consumePhaseStock(
    request: ConsumePhaseStockRequest,
  ): Promise<boolean> {
    try {
      const endpoint = `${this.resource}/ConsumePhaseStock`;
      const response = await this.apiClient.post(endpoint, request);
      return response.status === 200;
    } catch (err) {
      logException(err);
      return false;
    }
  }

  async getPhaseConsumptions(
    workOrderPhaseId: string,
  ): Promise<StockMovement[]> {
    try {
      const endpoint = `${this.resource}/PhaseConsumptions/${workOrderPhaseId}`;
      const response = await this.apiClient.get(endpoint);
      if (response.status === 200) {
        return response.data as StockMovement[];
      }
      return [];
    } catch (err) {
      logException(err);
      return [];
    }
  }
}
