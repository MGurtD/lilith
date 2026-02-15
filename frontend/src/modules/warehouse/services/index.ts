import { StockMovementService } from "./stockMovement.service";
import { WarehouseService, StockService } from "./warehouse.service";

export default {
  Warehouse: new WarehouseService("/Warehouse"),
  Stock: new StockService("/Stock"),
  StockMovementService: new StockMovementService("/StockMovement"),
};
