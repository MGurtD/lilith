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

type Translate = (key: string) => string;

const LOCATION_TYPE_KEYS: Record<string, string> = {
  Supply: "warehouse.locationTypes.supply",
  Receiving: "warehouse.locationTypes.receiving",
  Shipping: "warehouse.locationTypes.shipping",
  Storage: "warehouse.locationTypes.storage",
};

export const getLocationTypeOptions = (t: Translate): LocationTypeOption[] =>
  Object.entries(LOCATION_TYPE_KEYS).map(([value, key]) => ({ value, label: t(key) }));

export const getLocationTypeLabel = (
  value: string | null | undefined,
  t: Translate,
): string => {
  if (!value) return "";
  const key = LOCATION_TYPE_KEYS[value];
  return key ? t(key) : value;
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
}

export interface StockMovement {
  id: string;
  stockId: string;
  movementType: string;
  locationId: null | string;
  location: Location | null;
  referenceId: string;
  reference?: { id: string; code: string; description: string } | null;
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

