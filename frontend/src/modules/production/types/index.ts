import { Reference } from "../../shared/types";

export interface Enterprise {
  id: string;
  name: string;
  description: string;
  defaultSiteId?: string | null;
  disabled: boolean;
}

export interface Site {
  id: string;
  name: string;
  description: string;
  address: string;
  city: string;
  postalCode: string;
  region: string;
  country: string;
  phoneNumber: string;
  email: string;
  emailSales: string;
  emailPurchase: string;
  vatNumber: string;
  enterpriseId: string;
  disabled: boolean;
}

export interface Area {
  id: string;
  name: string;
  description: string;
  siteId: string;
  isVisibleInPlant: boolean;
  disabled: boolean;
  workcenters?: Array<Workcenter>;
}

export interface WorkcenterType {
  id: string;
  name: string;
  description: string;
  profitPercentage: number;
  disabled: boolean;
}

export interface Workcenter {
  id: string;
  name: string;
  description: string;
  workcenterTypeId: string;
  areaId: string;
  shiftId: string;
  profitPercentage: number;
  disabled: boolean;
}

export interface WorkOrderOrder {
  id: string;
  order: number;
}

export interface MachineStatus {
  id: string;
  name: string;
  description: string;
  color: string;
  stopped: boolean;
  operatorsAllowed: boolean;
  closed: boolean;
  preferred: boolean;
  workOrderAllowed: boolean;
  icon: string;
  disabled: boolean;
  reasons?: Array<MachineStatusReason>;
}

export interface MachineStatusReason {
  id: string;
  code: string;
  name: string;
  description: string;
  color: string;
  icon: string;
  machineStatusId: string;
  disabled: boolean;
}

export interface WorkcenterCost {
  id: string;
  workcenterId: string;
  machineStatusId: string;
  cost: number;
  disabled: boolean;
}

export interface Operator {
  id: string;
  code: string;
  name: string;
  surname: string;
  vatNumber: string;
  operatorTypeId: string;
  disabled: boolean;
}

export interface OperatorType {
  id: string;
  name: string;
  description: string;
  cost: number;
  disabled: boolean;
}

export interface ProductionCosts {
  operatorCost: number;
  machineCost: number;
  materialCost: number;
  externalServiceCost: number;
  externalTransportCost: number;
}

export interface WorkMaster {
  id: string;
  referenceId: string;
  reference?: Reference;
  baseQuantity: number;
  operatorCost: number;
  machineCost: number;
  externalCost: number;
  materialCost: number;
  totalWeight: number;
  mode: number;
  disabled: boolean;
  phases: Array<WorkMasterPhase>;
}

export interface WorkMasterPhase {
  id: string;
  code: string;
  description: string;
  workMasterId: string;
  disabled: boolean;
  workcenterTypeId?: string | null;
  profitPercentage: number;
  preferredWorkcenterId?: string | null;
  operatorTypeId?: string | null;
  isExternalWork: boolean;
  serviceReferenceId?: string | null;
  externalWorkCost: number;
  transportCost: number;
  details?: Array<WorkMasterPhaseDetail>;
  billOfMaterials?: Array<WorkMasterPhaseBillOfMaterials>;
}

export interface WorkMasterPhaseDetail {
  id: string;
  workMasterPhaseId: string;
  machineStatusId: string;
  estimatedTime: number;
  estimatedOperatorTime: number;
  isCycleTime: boolean;
  order: number;
  comment: string;
}

export interface WorkMasterPhaseBillOfMaterials {
  id: string;
  workMasterPhaseId: string;
  referenceId: string;
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}
export interface WorkMasterToCopy {
  referenceId: string | null | undefined;
  workmaster: WorkMaster;
  workmasterId: string;
  referenceCode: string;
  mode: number;
}

export interface WorkOrder {
  id: string;
  referenceId: string;
  workMasterId: string;
  code: string;
  reference?: Reference;
  plannedQuantity: number;
  plannedDate: any;
  statusId: string;
  exerciceId: string;
  startTime: any;
  endTime: any;
  order: number;
  comment: string;
  disabled: boolean;
  operatorCost: number;
  machineCost: number;
  operatorTime: number;
  machineTime: number;
  materialCost: number;
  totalQuantity: number;
  phases: Array<WorkOrderPhase>;
}

export interface WorkOrderPhaseShoopfloor {
  workOrderPhase: WorkOrderPhase;
  referencePdfs: Array<File>;
}

export interface WorkOrderPhase {
  id: string;
  code: string;
  description: string;
  workOrderId: string;
  disabled: boolean;
  workcenterTypeId?: string | null;
  profitPercentage: number;
  preferredWorkcenterId?: string | null;
  operatorTypeId?: string | null;
  isExternalWork: boolean;
  serviceReferenceId?: string | null;
  externalWorkCost: number;
  transportCost: number;
  statusId: string;
  comment?: string;
  startTime: any;
  endTime: any;
  quantityOk: number;
  quantityKo: number;
  purchaseOrderId?: string | null;
  workOrder?: WorkOrder;
  details?: Array<WorkOrderPhaseDetail>;
  billOfMaterials?: Array<WorkOrderPhaseBillOfMaterials>;
}

export interface WorkOrderPhaseDetail {
  id: string;
  workOrderPhaseId: string;
  machineStatusId: string;
  estimatedTime: number;
  estimatedOperatorTime: number;
  isCycleTime: boolean;
  order: number;
  comment: string;
}

export interface PhaseTimeMetrics {
  phaseId: string;
  machineStatusId: string;
  operatorId?: string | null;
  estimatedMachineTimeMinutes: number;
  estimatedOperatorTimeMinutes: number;
  actualMachineTimeMinutes: number;
  actualOperatorTimeMinutes: number;
  calculatedAt: string;
}

export interface WorkOrderPhaseBillOfMaterials {
  id: string;
  workOrderPhaseId: string;
  referenceId: string;
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}

export interface CreateWorkOrderDto {
  workMasterId: string;
  plannedQuantity: number;
  plannedDate: any;
  comment: string;
}

export interface ValidatePreviousPhaseQuantityRequest {
  workOrderPhaseId: string;
  quantity: number;
}

export interface UpdatePhaseQuantitiesRequest {
  workcenterId: string;
  workOrderPhaseId: string;
  quantityOk: number;
  quantityKo: number;
}

export interface ProductionPart {
  id: string;
  workOrderId: string;
  workOrderPhaseId: string;
  workOrderPhaseDetailId: string;
  workcenterId: string;
  operatorId: string;
  operatorTime: number;
  operatorHourCost: number;
  workcenterTime: number;
  machineHourCost: number;
  date: any;
  quantity: number;
  workOrder?: WorkOrder;
  workOrderPhase?: WorkOrderPhase;
  workOrderPhaseDetail?: WorkOrderPhaseDetail;
}

export interface DetailedWorkOrder {
  workOrderId: string;
  workOrderCode: string;
  workOrderStatusCode: string;
  workOrderStatusDescription: string;
  plannedQuantity: number;
  workOrderStartTime: any;
  workOrderEndTime: any;
  workOrderOrder: number;
  workOrderComment: string;
  plannedDate: any;
  referenceCode: string;
  referenceDescription: string;
  referenceVersion: string;
  referenceCost: number;
  workOrderPhaseId: string;
  workOrderPhaseCode: string;
  workOrderPhaseDescription: string;
  workOrderPhaseComment: string;
  workOrderPhaseStatusCode: string;
  workOrderPhaseStatusDescription: string;
  workOrderPhaseStartTime: any;
  workOrderPhaseEndTime: any;
  workOrderPhaseDetailId: string;
  workOrderPhaseDetailOrder: number;
  workOrderPhaseDetailEstimatedTime: number;
  workOrderPhaseDetailComment: string;
  machineStatusName: string;
  machineStatusDescription: string;
  workcenterId: string;
  workcenterName: string;
  workcenterDescription: string;
  workcenterCost: number;
  preferredWorkcenter: boolean;
}

export interface Shift {
  id: string;
  name: string;
  disabled: boolean;
  details: Array<ShiftDetail>;
}

export interface ShiftDetail {
  id: string;
  startTime: any;
  endTime: any;
  isProductiveTime: boolean;
  shiftId: string;
}

export interface ProductionCostDashboardGrouped {
  workcenterName: string;
  operatorName: string;
  workcenterTypeName: string;
  year: number;
  month: number;
  totalWorkcenterTime: number;
  totalPartWorkcenterCost: number;
  totalCost: number;
}

export interface WorkcenterShiftGroup {
  groupKey: string;
  groupId: string | null;
  details: WorkDetail[];
  totalHours: number;
  totalCost: number;
  totalQuantityOk: number;
  totalQuantityKo: number;
  detailCount: number;
}

export interface WorkDetail {
  id: string;
  key: string;
  workcenter: string;
  machineStatus: string;
  startTime: string;
  endTime: string | null;
  current: boolean;
  operatorCost: number;
  workcenterCost: number;
  quantityOk: number;
  quantityKo: number;
  totalHours: number;
  totalCost: number;
}

export interface WorkcenterShiftRequest {
  startTime: Date;
  endTime: Date;
  groupBy: string;
  timeGroupBy: string;
}

export interface WorkcenterShiftHistorical {
  id: string;
  key: string | null;

  workcenter: string;
  machineStatus: string;
  operator: string;

  startTime: string;
  endTime: string | null;

  quantityOk: number;
  quantityKo: number;

  totalHours: number;
  operatorCost: number;
  workcenterCost: number;
  totalCost: number;

  workOrderCode: string;
  plannedQuantity: number;

  estimatedMachineCost: number;
  estimatedMachineTime: number;
  estimatedOperatorCost: number;
  estimatedOperatorTime: number;

  workOrderPhaseCode: string;
  workOrderPhaseDescription: string;

  isPreferredWorkcenter: boolean;

  referenceCode: string;
  referenceDescription: string;
  customerComercialName: string;

  operatorId: string;
  workcenterId: string;
  referenceId: string;
  customerId: string;
  workOrderId: string;
  workOrderPhaseId: string;
}

export interface WorkOrderPhaseDisplay {
  // Work Order Info
  workOrderId: string;
  workOrderCode: string;
  customerName: string;

  // Phase Info
  phaseId: string;
  phaseCode: string;
  phaseDescription: string;
  phaseDisplay: string;

  // Reference (Product) Info
  salesReferenceCode: string;
  salesReferenceDescription: string;
  salesReferenceDisplay: string;

  // Production
  plannedQuantity: number;
  plannedDate: Date | string;
  phaseStatus: string;
  priority: number; // Added for consistency with backend
}

/**
 * Hierarchical DTO for work orders with their planned phases.
 * Used for TreeTable display in plant module.
 */
export interface WorkOrderWithPhases {
  workOrderId: string;
  workOrderCode: string;
  customerName: string;
  salesReferenceId: string;
  salesReferenceDisplay: string;
  plannedQuantity: number;
  plannedDate: Date | string;
  startTime?: Date | string | null;
  workOrderStatus: string;
  priority: number;
  phases: PlannedPhase[];
  comment?: string;
}

/**
 * Individual phase within a work order.
 * Child node in TreeTable structure.
 */
export interface PlannedPhase {
  phaseId: string;
  phaseCode: string;
  phaseDescription: string;
  phaseDisplay: string;
  phaseStatusId: string;
  phaseStatus: string;
  isExternalWork: boolean;
  quantityOk: number;
  quantityKo: number;
  startTime?: Date | string | null;
  preferredWorkcenterName: string;
  comment?: string;
  /** Phase operation details for activity buttons */
  details?: PhaseDetailForButtons[];
}

/**
 * Simplified phase detail for plant module activity buttons.
 * Contains only the information needed for dynamic status buttons.
 */
export interface PhaseDetailForButtons {
  /** Machine status ID */
  machineStatusId?: string;
  /** Machine status name (display text for button) */
  machineStatusName: string;
  /** Machine status color (hex code for button background) */
  machineStatusColor: string;
  /** Machine status icon (PrimeIcons class for button icon) */
  machineStatusIcon: string;
  /** Display order for buttons */
  order: number;
  /** Estimated machine time in minutes */
  estimatedTime: number;
  /** Estimated operator time in minutes */
  estimatedOperatorTime: number;
  /** Detail comment */
  comment?: string;
}

/**
 * Detailed phase information including work order context,
 * phase details, and bill of materials.
 * Used for Step 2 of work order selection process.
 */
export interface WorkOrderPhaseDetailed {
  workOrderId: string;
  workOrderCode: string;
  salesReferenceDisplay: string;
  plannedQuantity: number;
  phaseId: string;
  phaseCode: string;
  phaseDescription: string;
  phaseStatus: string;
  startTime?: Date | string | null;
  endTime?: Date | string | null;
  preferredWorkcenterName: string;
  workcenterTypeId: string;
  quantityOk: number;
  quantityKo: number;
  isExternalWork: boolean;
  comment?: string;

  details: PhaseDetailItem[];
  billOfMaterials: BillOfMaterialsItem[];
}

/**
 * Individual operation detail within a phase.
 * Represents a specific machine status and time estimate.
 */
export interface PhaseDetailItem {
  comment: string;
  machineStatusId?: string;
  machineStatusName: string;
  estimatedTime: number;
  estimatedOperatorTime: number;
  isCycleTime: boolean;
  order: number;
}

/**
 * Material component required for a phase.
 * References product/material catalog.
 */
export interface BillOfMaterialsItem {
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
}

export interface WorkcenterProfitPercentage {
  id: string;
  workcenterId: string;
  profitPercentage: number;
  disabled: boolean;
}
export interface WorkcenterTypeSaturation {
  workOrderCode: string;
  workOrderPriority: number;
  workOrderPlannedDate: string;
  plannedQuantity: number;
  phaseCode: string;
  phaseDescription: string;
  workcenterTypeId: string;
  estimatedTime: number;
}
