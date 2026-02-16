import BaseService from "../../../api/base.service";
import {
  UpdatePhaseQuantitiesRequest,
  WorkcenterShiftGroup,
  WorkcenterShiftHistorical,
  WorkcenterShiftRequest,
} from "../types";

export class WorkcenterShiftService extends BaseService<WorkcenterShiftGroup> {
  async Query(
    request: WorkcenterShiftRequest,
  ): Promise<Array<WorkcenterShiftHistorical> | undefined> {
    const response = await this.apiClient.post(
      `${this.resource}/Historical`,
      request,
    );
    return response.data as Array<WorkcenterShiftHistorical>;
  }

  async UpdatePhaseQuantities(
    dto: UpdatePhaseQuantitiesRequest,
  ): Promise<boolean> {
    try {
      const response = await this.apiClient.put(
        `${this.resource}/WorkOrderPhase/Quantities`,
        dto,
      );
      return response.status === 200;
    } catch (err) {
      console.error("Error updating phase quantities:", err);
      return false;
    }
  }
}
