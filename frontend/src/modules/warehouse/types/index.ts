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
