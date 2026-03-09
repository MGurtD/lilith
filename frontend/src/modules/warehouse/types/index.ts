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

export interface StockMovement {
  id: string;
  stockId: string;
  movementType: string;
  locationId: null | string;
  referenceId: string;
  quantity: number;
  width: number;
  length: number;
  height: number;
  diameter: number;
  thickness: number;
  movementDate: any;
  description: string;
}

export interface Inventory {
  id: string;
  stockId: string;
  movementType: string;
  locationId: string;
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
  quantity: number;
}

