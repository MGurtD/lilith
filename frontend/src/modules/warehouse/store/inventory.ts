import { defineStore } from "pinia";
import { Inventory } from "../types";

export const useInventoryStore = defineStore({
    id: "inventory",
    state: () => ({
        inventory: undefined as Inventory | undefined,
        inventories : undefined as Array<Inventory> | undefined,
    }),
    getters: {},
    actions: {
        setNewInventory(id: string) {
            this.inventory = {
                id: id,
                stockId: "",
                movementType: "",
                locationId: "",
                referenceId: "",
                oldQuantity: 0.0,
                newQuantity: 0.0,
                width: 0.0,
                length: 0.0,
                height: 0.0,
                diameter: 0.0,
                thickness: 0.0,
                movementDate: new Date(),
            } as Inventory;
        },
    }
})