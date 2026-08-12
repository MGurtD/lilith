export interface Warehouse {
  id: string;
  name: string;
  description: string;
  siteId: string;
  defaultLocationId: string | null;
  disabled: boolean;
  locations: Array<Location>;
}

export interface Location {
  id: string;
  name: string;
  description: string;
  warehouseId: string;
  disabled: boolean;
  locationType?: string | null;
}

export interface LocationTypeOption {
  value: string;
  label: string;
}

export const LOCATION_TYPE_OPTIONS: LocationTypeOption[] = [
  { value: "Supply", label: "Subministrament" },
  { value: "Receiving", label: "Recepció" },
  { value: "Shipping", label: "Expedició" },
  { value: "Storage", label: "Emmagatzematge" },
];

export const getLocationTypeLabel = (value: string | null | undefined): string => {
  if (!value) return "";
  return LOCATION_TYPE_OPTIONS.find((opt) => opt.value === value)?.label ?? value;
};

export interface Stock {
  id: string;
  locationId: string;
  referenceId: string;
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}

export interface StockListItem extends Stock {
  referenceCode: string;
  referenceDescription: string;
  referenceDisplay: string;
  locationName: string;
  locationDescription: string;
  warehouseId: string;
  warehouseName: string;
  warehouseDescription: string;
  lotId?: string | null;
  lotCode?: string;
  lotClosedDate?: string | null;
}

// Matèria primera: lot de traçabilitat associat a una referència
export interface Lot {
  id: string;
  referenceId: string;
  code: string;
  supplierLotCode?: string | null;
  expirationDate?: any;
  closedDate?: any;
  remainingQuantity: number;
  comment?: string | null;
}

// Traçabilitat de lots: origen de compra d'una fulla de l'arbre "cap enrere"
export interface PurchaseOrigin {
  lotId: string;
  lotCode: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
  supplierId: string;
  supplierName: string;
  receiptId: string;
  receiptNumber: string;
  receiptDate: any;
}

// Traçabilitat de lots: destí de venda d'una fulla de l'arbre "cap endavant"
export interface SalesDestination {
  lotId: string;
  lotCode: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
  customerId: string;
  customerName: string;
  deliveryNoteId: string;
  deliveryNoteNumber: string;
  deliveryDate: any;
}

// Node recursiu de l'arbre de traçabilitat (backward i forward comparteixen forma)
export interface LotTraceabilityNode {
  lotId: string;
  lotCode: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
  children?: LotTraceabilityNode[];
  purchaseOrigins?: PurchaseOrigin[];
  salesDestinations?: SalesDestination[];
}

export interface LotBackwardTraceability {
  lotId: string;
  lotCode: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  root: LotTraceabilityNode;
}

export interface LotForwardTraceability {
  lotId: string;
  lotCode: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  root: LotTraceabilityNode;
}

export interface RecallDeliveryNote {
  deliveryNoteId: string;
  deliveryNoteNumber: string;
  deliveryDate: any;
  lotId: string;
  lotCode: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  quantity: number;
}

export interface RecallCustomer {
  customerId: string;
  customerName: string;
  deliveryNotes: RecallDeliveryNote[];
}

export interface RecallReport {
  lotId: string;
  lotCode: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  totalAffectedDeliveryNotes: number;
  totalAffectedQuantity: number;
  affectedCustomers: RecallCustomer[];
}

export interface StockMovement {
  id: string;
  stockId: string;
  movementType: string;
  locationId: null | string;
  location: Location | null;
  referenceId: string;
  reference?: { id: string; code: string; description: string } | null;
  lotId?: string | null;
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
  movementDate: any;
  description: string;
  entity?: string | null;
  entityId?: string | null;
}

export interface Inventory {
  id: string;
  stockId: string;
  movementType: string;
  locationId: string | null;
  locationName?: string; // Optional for UI purposes
  referenceId: string;
  referenceName?: string; // Optional for UI purposes
  lotId?: string | null;
  lotCode?: string;
  oldQuantity: number;
  newQuantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
  movementDate: any;
}

/**
 * Rich stock read-model returned by GET /Stock/ByBillOfMaterials/{id}.
 * Includes resolved reference, location and warehouse names.
 */
export interface StockResponse {
  stockId: string;
  referenceId: string;
  referenceCode: string;
  referenceDescription: string;
  referenceFormatId: string;
  referenceFormatCode: string;
  referenceFormatDescription: string;
  locationId: string;
  locationName: string;
  locationDescription: string;
  warehouseId: string;
  warehouseName: string;
  warehouseDescription: string;
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}

export interface MoveStockToWorkcenterSupplyRequest {
  stockId: string;
  workcenterId: string;
  workOrderPhaseId: string;
  quantity: number;
}

export interface ReturnStockFromSupplyRequest {
  stockId: string;
  workcenterId: string;
  workOrderPhaseId: string;
  quantity: number;
}

export interface RemainingPiece {
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
}

export interface ConsumeStockEntry {
  stockId: string;
  remainingPieces: RemainingPiece[];
}

export interface ConsumePhaseStockRequest {
  workcenterId: string;
  workOrderPhaseId: string;
  entries: ConsumeStockEntry[];
}

export const StockMovementEntity = {
  WorkOrderPhase: "WorkOrderPhase",
  DeliveryNote: "DeliveryNote",
  Receipt: "Receipt",
  WorkOrder: "WorkOrder",
} as const;

