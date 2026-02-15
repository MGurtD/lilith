import {
  WorkOrder,
  WorkOrderPhase,
  WorkOrderPhaseDetail,
  WorkOrderPhaseBillOfMaterials,
  WorkOrderWithPhases,
  WorkOrderPhaseDetailed,
  CreateWorkOrderDto,
  DetailedWorkOrder,
  WorkOrderOrder,
  ValidatePreviousPhaseQuantityRequest,
  PhaseTimeMetrics,
} from "../types";
import BaseService from "../../../api/base.service";
import { NextPhaseInfo } from "../../plant/types";
import { GenericResponse } from "../../../types";

export class WorkOrderService extends BaseService<WorkOrder> {
  async GetReportDataById(id: string) {
    const endpoint = `${this.resource}/Report/${id}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data;
    }
  }

  async GetBetweenDatesAndStatus(
    startTime: string,
    endTime: string,
    statusId?: string,
    referenceId?: string,
    customerId?: string,
  ): Promise<Array<WorkOrder> | undefined> {
    let endpoint = `${this.resource}?startTime=${startTime}&endTime=${endTime}`;
    if (statusId) endpoint += `&statusId=${statusId}`;
    if (referenceId) endpoint += `&referenceId=${statusId}`;
    if (customerId) endpoint += `&customerId=${customerId}`;

    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<WorkOrder>;
    }
  }
  async GetBySalesOrderId(
    salesOrderId: string,
  ): Promise<Array<WorkOrder> | undefined> {
    let endpoint = `${this.resource}/SalesOrder/${salesOrderId}`;
    const response = await this.apiClient.get(endpoint);
    if (response.status === 200) {
      return response.data as Array<WorkOrder>;
    }
  }
  async GetPlannableWorkOrders(): Promise<Array<WorkOrder> | undefined> {
    const response = await this.apiClient.get(`${this.resource}/Plannable`);
    if (response.status === 200) return response.data as Array<WorkOrder>;
  }
  async Create(dto: CreateWorkOrderDto): Promise<GenericResponse<WorkOrder>> {
    const response = await this.apiClient.post(
      `${this.resource}/CreateFromWorkMaster`,
      dto,
    );
    return response.data as GenericResponse<WorkOrder>;
  }
  async CreateFromSalesOrderDetail(
    dto: CreateWorkOrderDto,
    salesOrderDetailId: string,
  ): Promise<GenericResponse<WorkOrder>> {
    const response = await this.apiClient.post(
      `${this.resource}/CreateFromSalesOrderDetail/${salesOrderDetailId}`,
      dto,
    );
    return response.data as GenericResponse<WorkOrder>;
  }
  async Priorize(
    workOrderOrders: WorkOrderOrder[],
  ): Promise<GenericResponse<boolean>> {
    const response = await this.apiClient.post(
      `${this.resource}/Priorize`,
      workOrderOrders,
    );
    return response.data as GenericResponse<boolean>;
  }
}
export class WorkOrderPhaseService extends BaseService<WorkOrderPhase> {
  async getByWorkOrderId(
    workOrderId: string,
  ): Promise<Array<WorkOrderPhase> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}?workOrderId=${workOrderId}`,
    );
    if (response.status === 200) return response.data as Array<WorkOrderPhase>;
  }

  async getExternalWorkOrderPhases(
    startTime: string,
    endTime: string,
  ): Promise<Array<WorkOrderPhase> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/External?startTime=${startTime}&endTime=${endTime}`,
    );
    if (response.status === 200) {
      const data = response.data as Array<{ workOrder: any; phase: any }>;
      const workOrderPhases: Array<WorkOrderPhase> = data.map((item) => ({
        ...item.phase,
        workOrder: item.workOrder,
      }));
      return workOrderPhases;
    }
  }

  async GetPlannedPhasesByWorkcenterType(
    workcenterTypeId: string,
  ): Promise<Array<WorkOrderWithPhases> | undefined> {
    const response = await this.apiClient.get(
      `/WorkOrder/Planned/WorkcenterType/${workcenterTypeId}`,
    );
    if (response.status === 200) {
      return response.data as Array<WorkOrderWithPhases>;
    }
  }

  async GetWorkOrderPhasesDetailed(
    workOrderId: string,
  ): Promise<Array<WorkOrderPhaseDetailed> | undefined> {
    const response = await this.apiClient.get(
      `/WorkOrder/${workOrderId}/PhasesDetailed`,
    );
    if (response.status === 200) {
      return response.data as Array<WorkOrderPhaseDetailed>;
    }
  }

  async GetLoadedByPhaseIds(
    phaseIds: string[],
  ): Promise<Array<WorkOrderWithPhases>> {
    const response = await this.apiClient.post(`/WorkOrder/Loaded`, phaseIds);
    if (response.status === 200) {
      return response.data as Array<WorkOrderWithPhases>;
    }
    return [];
  }

  async ValidatePreviousPhaseQuantity(
    request: ValidatePreviousPhaseQuantityRequest,
  ): Promise<GenericResponse<boolean>> {
    const response = await this.apiClient.post(
      `/WorkOrder/Phase/ValidatePreviousQuantity`,
      request,
    );
    return response.data as GenericResponse<boolean>;
  }

  async GetNextPhaseForWorkcenter(
    currentPhaseId: string,
    workcenterId: string,
  ): Promise<NextPhaseInfo | undefined> {
    const response = await this.apiClient.get(
      `/WorkOrder/Phase/${currentPhaseId}/NextForWorkcenter/${workcenterId}`,
    );
    if (response.status === 200) {
      return response.data as NextPhaseInfo;
    }
    return undefined;
  }

  async GetPhaseTimeMetrics(
    phaseId: string,
    machineStatusId: string,
    operatorId?: string,
  ): Promise<PhaseTimeMetrics | undefined> {
    let url = `/WorkOrder/Phase/${phaseId}/TimeMetrics?machineStatusId=${machineStatusId}`;
    if (operatorId) {
      url += `&operatorId=${operatorId}`;
    }
    const response = await this.apiClient.get(url);
    if (response.status === 200) {
      return response.data as PhaseTimeMetrics;
    }
    return undefined;
  }
}

export class WorkOrderPhaseDetailService extends BaseService<WorkOrderPhaseDetail> {
  async getByWorkOrderPhaseId(
    workOrderPhaseId: string,
  ): Promise<Array<WorkOrderPhaseDetail> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}?workOrderPhaseId=${workOrderPhaseId}`,
    );
    if (response.status === 200)
      return response.data as Array<WorkOrderPhaseDetail>;
  }
}
export class WorkOrderPhaseBillOfMaterialsService extends BaseService<WorkOrderPhaseBillOfMaterials> {
  async getByWorkOrderPhaseId(
    workOrderPhaseId: string,
  ): Promise<Array<WorkOrderPhaseBillOfMaterials> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}?workOrderPhaseId=${workOrderPhaseId}`,
    );
    if (response.status === 200)
      return response.data as Array<WorkOrderPhaseBillOfMaterials>;
  }
}
export class DetailedWorkOrderService extends BaseService<DetailedWorkOrder> {
  async getByWorkcenterId(
    workcenterId: string,
  ): Promise<Array<DetailedWorkOrder> | undefined> {
    const response = await this.apiClient.get(
      `${this.resource}/ByWorkcenter/${workcenterId}`,
    );
    if (response.status === 200)
      return response.data as Array<DetailedWorkOrder>;
  }
  async getByWorkOrderId(
    workOrderId: string,
  ): Promise<Array<DetailedWorkOrder> | undefined> {
    const response = await this.apiClient.get(
      `/WorkOrder/${workOrderId}/Detailed`,
    );
    if (response.status === 200)
      return response.data as Array<DetailedWorkOrder>;
  }
}
