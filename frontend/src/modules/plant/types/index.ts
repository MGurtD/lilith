import { Workcenter as WorkcenterMaster } from "../../production/types";

// Re-export del modelo maestro para claridad
export type WorkcenterConfig = WorkcenterMaster;

// Active work order on workcenter
export interface WorkOrderActive {
  workOrderPhaseId: string;
  workOrderCode: string;
  workOrderPhaseCode: string;
  workOrderPhaseDescription: string;
  plannedQuantity: number;
  referenceCode: string;
  referenceDescription: string;
  startTime: string; // ISO 8601 datetime string
}

// Snapshot de datos en tiempo real desde WebSocket
export interface WorkcenterRealtime {
  workcenterId: string;
  workcenterName: string;
  workcenterDescription: string;
  multiWoAvailable: boolean;
  areaId: string;
  areaDescription: string;
  shiftId: string;
  shiftName: string;
  shiftDetailId: string;
  shiftDetailStartTime: string; // Time format: "HH:mm:ss"
  shiftDetailEndTime: string; // Time format: "HH:mm:ss"
  shiftDetailsIsProductiveTime: boolean;
  statusId: string;
  statusReasonId?: string;
  statusName: string;
  statusOperatorsAllowed: boolean;
  statusClosed: boolean;
  statusStopped: boolean;
  statusColor: string;
  statusStartTime: string; // ISO 8601 datetime string
  operators: OperatorRealtime[];
  workorders: WorkOrderActive[];
}

// Estado combinado para la vista (maestro + realtime)
export interface WorkcenterViewState {
  config: WorkcenterConfig;
  realtime?: WorkcenterRealtime;
}

export interface OperatorRealtime {
  operatorId: string;
  operatorCode: string;
  operatorName: string;
  operatorSurname: string;
  operatorTypeId: string;
  operatorTypeName: string;
  operatorTypeDescription: string;
  operatorTypeCost: number;
  operatorStartTime: string;
}

export interface OperatorClockInOutRequest {
  workcenterId: string;
  operatorId: string;
}

export interface ChangeMachineStatusRequest {
  workcenterId: string;
  statusId: string;
  statusReasonId?: string;
  workOrderPhaseId?: string;
}

export interface LoadWorkOrderPhaseRequest {
  workcenterId: string;
  workOrderPhaseId: string;
  machineStatusId: string;
}

export interface UnloadWorkOrderPhaseRequest {
  workcenterId: string;
  workOrderPhaseId: string;
  workOrderStatusId: string;
  quantityOk: number;
  quantityKo: number;
  nextWorkOrderPhaseId?: string;
  nextMachineStatusId?: string;
}

export interface OperatorResponse {
  id: string;
  code: string;
  name: string;
  surname: string;
  operatorTypeId: string;
}

export interface WorkOrderPhaseRequest {
  workcenterId: string;
  workOrderPhaseId: string;
}

// Next phase info for auto-loading
export interface NextPhaseInfo {
  phaseId: string;
  phaseCode: string;
  phaseDescription: string;
  details: import("../../production/types").PhaseDetailItem[];
}

export interface WorkOrderPhaseResponse {
  code: string;
  description: string;
  workOrderId: string;
}

// WebSocket message wrapper
export interface WebSocketMessage<T = any> {
  type: string;
  payload: T;
}

// Payload type for general workcenter snapshots
export type WorkcentersSnapshotPayload = Record<string, WorkcenterRealtime>;

// Realtime handler interfaces
export interface RealtimeHandler {
  cleanup: () => void;
  onUpdate: (callback: (data: WorkcenterRealtime[]) => void) => void;
}

export interface WorkcenterRealtimeHandler {
  cleanup: () => void;
  onUpdate: (callback: (data: WorkcenterRealtime) => void) => void;
}
