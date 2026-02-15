import BaseService from "../../../api/base.service";
import {
  WorkcenterShiftGroup,
  WorkcenterShiftHistorical,
  WorkcenterShiftRequest,
} from "../types";

export class WorkcenterShiftService extends BaseService<WorkcenterShiftGroup> {
  async Query(
    request: WorkcenterShiftRequest
  ): Promise<Array<WorkcenterShiftHistorical> | undefined> {
    const response = await this.apiClient.post(
      `${this.resource}/Historical`,
      request
    );
    return response.data as Array<WorkcenterShiftHistorical>;
  }
}
