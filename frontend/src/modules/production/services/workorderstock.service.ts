import BaseService from "../../../api/base.service";
import { logException } from "../../../api/api.client";
import { MoveStockToWorkcenterSupplyRequest } from "../../warehouse/types";

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
}
