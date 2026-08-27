import BaseService from "../../../api/base.service";
import {
  LotBackwardTraceability,
  LotForwardTraceability,
  RecallReport,
} from "../types";

export class LotTraceabilityService extends BaseService<LotBackwardTraceability> {
  async getBackward(
    lotId: string,
  ): Promise<LotBackwardTraceability | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/Backward/${lotId}`,
    );
    return response.status === 200 ? response.data : undefined;
  }

  async getForward(lotId: string): Promise<LotForwardTraceability | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/Forward/${lotId}`,
    );
    return response.status === 200 ? response.data : undefined;
  }

  async getRecall(lotId: string): Promise<RecallReport | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/Recall/${lotId}`,
    );
    return response.status === 200 ? response.data : undefined;
  }
}
