import { ProductionPart } from "../types";
import BaseService from "../../../api/base.service";

export class ProductionPartService extends BaseService<ProductionPart> {
  async GetBetweenDates(
    startTime: string,
    endTime: string,
    workcenterId?: string,
    operatorId?: string,
    workorderId?: string
  ): Promise<Array<ProductionPart> | undefined> {
    let endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;

    if (workcenterId) {
      endpoint += `&workcenterId=${workcenterId}`;
    }
    if (operatorId) {
      endpoint += `&operatorId=${operatorId}`;
    }
    if (workorderId) {
      endpoint += `&workorderId=${workorderId}`;
    }

    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<ProductionPart>;
    }
  }
  async GetByWorkOrderId(
    workOrderId: string
  ): Promise<Array<ProductionPart> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/WorkOrder/${workOrderId}`
    );
    if (response.status === 200) {
      return response.data as Array<ProductionPart>;
    }
  }
}
