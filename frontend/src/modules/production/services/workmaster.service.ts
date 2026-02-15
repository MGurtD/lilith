import {
  WorkMaster,
  WorkMasterPhase,
  WorkMasterPhaseDetail,
  WorkMasterPhaseBillOfMaterials,
  WorkMasterToCopy,
  ProductionCosts,
} from "../types";
import BaseService from "../../../api/base.service";
import { GenericResponse } from "../../../types";

// Workmaster
export class WorkMasterService extends BaseService<WorkMaster> {
  async getByReferenceId(
    referenceId: string
  ): Promise<Array<WorkMaster> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/reference/${referenceId}`
    );
    if (response.status === 200) return response.data as Array<WorkMaster>;
  }
  async copyWorkMaster(
    workmasterToCopy: WorkMasterToCopy
  ): Promise<GenericResponse<boolean>> {
    const response = await this.apiClient.post(
      `${this.resource}/copy`,
      workmasterToCopy
    );
    return response.data as GenericResponse<boolean>;
  }
  async calculateCost(
    id: string,
    quantity?: number
  ): Promise<GenericResponse<number>> {
    let endpoint = `${this.resource}/cost/${id}`;
    if (quantity) {
      endpoint = endpoint.concat(`?quantity=${quantity}`);
    }

    const response = await this.apiClient.get(endpoint);
    return response.data;
  }
  async getCosts(
    id: string,
    quantity?: number
  ): Promise<GenericResponse<ProductionCosts>> {
    let endpoint = `${this.resource}/costs/${id}`;
    if (quantity) {
      endpoint = endpoint.concat(`?quantity=${quantity}`);
    }

    const response = await this.apiClient.get(endpoint);
    return response.data;
  }
}
// Phases
export class WorkMasterPhaseService extends BaseService<WorkMasterPhase> {
  async getByWorkMasterId(
    workMasterId: string
  ): Promise<Array<WorkMasterPhase> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}?workMasterId=${workMasterId}`
    );
    if (response.status === 200) return response.data as Array<WorkMasterPhase>;
  }
}
// Phase details
export class WorkMasterPhaseDetailService extends BaseService<WorkMasterPhaseDetail> {
  async getByWorkMasterPhaseId(
    workMasterPhaseId: string
  ): Promise<Array<WorkMasterPhaseDetail> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}?workMasterPhaseId=${workMasterPhaseId}`
    );
    if (response.status === 200)
      return response.data as Array<WorkMasterPhaseDetail>;
  }
}
export class WorkMasterPhaseBillOfMaterialsService extends BaseService<WorkMasterPhaseBillOfMaterials> {
  async getByWorkMasterPhaseId(
    workMasterPhaseId: string
  ): Promise<Array<WorkMasterPhaseBillOfMaterials> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}?workMasterPhaseId=${workMasterPhaseId}`
    );
    if (response.status === 200)
      return response.data as Array<WorkMasterPhaseBillOfMaterials>;
  }
}
