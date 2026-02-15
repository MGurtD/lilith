import { defineStore } from "pinia";
import Services from "../services";
import { Warehouse, Location } from "../types";

export const useWarehouseStore = defineStore({
  id: "warehouse",
  state: () => ({
    warehouse: undefined as Warehouse | undefined,
    warehouses: undefined as Array<Warehouse> | undefined,
  }),
  getters: {
    warehousesHaveLocations: (state) => {
      if (!state.warehouses || state.warehouses.length === 0) return false;
      return state.warehouses.some(
        (warehouse) => warehouse.locations && warehouse.locations.length > 0
      );
    },
    getLocationName:
      (state) =>
      (locationId: string): string | undefined => {
        if (!state.warehouses || state.warehouses.length === 0)
          return undefined;

        for (const warehouse of state.warehouses) {
          if (warehouse.locations && warehouse.locations.length > 0) {
            const location = warehouse.locations.find(
              (loc) => loc.id === locationId
            );
            if (location) {
              return location.name;
            }
          }
        }
        return undefined;
      },
  },
  actions: {
    // Warehouse
    setNewWarehouse(id: string) {
      this.warehouse = {
        id: id,
        name: "",
        description: "",
        defaultLocationId: "",
        siteId: "",
        disabled: false,
      } as Warehouse;
    },
    async fetchWarehouses() {
      this.warehouses = await Services.Warehouse.getAll();
    },
    async fetchWarehousesBySite(siteId: string) {
      this.warehouses = await Services.Warehouse.getBySite(siteId);
    },
    async fetchWarehousesWithLocations() {
      this.warehouses = await Services.Warehouse.getAllWithLocations();
    },
    async fetchWarehouse(id: string) {
      this.warehouse = await Services.Warehouse.getById(id);
    },
    async createWarehouse(warehouse: Warehouse) {
      const result = await Services.Warehouse.create(warehouse);
      if (result) await this.fetchWarehouses();
      return result;
    },
    async updateWarehouse(id: string, warehouse: Warehouse) {
      const result = await Services.Warehouse.update(id, warehouse);
      if (result) await this.fetchWarehouses();
      return result;
    },
    async deleteWarehouse(id: string) {
      const result = await Services.Warehouse.delete(id);
      if (result) await this.fetchWarehouses();
      return result;
    },

    async createLocation(location: Location) {
      const result = await Services.Warehouse.createLocation(location);
      if (result) await this.fetchWarehouse(location.warehouseId);
      return result;
    },
    async updateLocation(id: string, location: Location) {
      const result = await Services.Warehouse.updateLocation(id, location);
      if (result) await this.fetchWarehouse(location.warehouseId);
      return result;
    },
    async deleteLocation(id: string) {
      const result = await Services.Warehouse.deleteLocation(id);
      if (result && this.warehouse)
        await this.fetchWarehouse(this.warehouse.id);
      return result;
    },
  },
});
