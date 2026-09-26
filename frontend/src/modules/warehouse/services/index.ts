import { StockMovementService } from "./stockMovement.service";
import { WarehouseService, StockService, LotService } from "./warehouse.service";
import { LotTraceabilityService } from "./lotTraceability.service";

export default {
  Warehouse: new WarehouseService("/Warehouse"),
  Stock: new StockService("/Stock"),
  StockMovementService: new StockMovementService("/StockMovement"),
  Lot: new LotService("/Lot"),
  LotTraceability: new LotTraceabilityService("/LotTraceability"),
};
