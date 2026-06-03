<template>
  <DataTable
    :value="filteredInventories"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :paginator="(inventoryStore.inventories?.length ?? 0) > 20"
    :rows="20"
  >
    <template #header>
      <TableFilter
        :config="filterConfig"
        v-model="filter"
        :show-title="false"
        :show-filter-action="false"
        :body-width="filterBodyWidth"
        embedded
        @create="newMovement"
        @clear="cleanFilter"
      >
        <template #prepend>
          <div class="table-filter-prepend-field table-filter-prepend-field--md">
            <label class="filter-label table-filter-prepend-label">Ubicació</label>
            <DropdownWarehousesWithLocations
              label=""
              v-model="filter.locationId"
            />
          </div>
        </template>
        <template #append>
          <Button
            label="Guardar"
            icon="pi pi-save"
            size="small"
            rounded
            aria-label="Guardar moviments"
            @click="saveMovement"
          />
        </template>
      </TableFilter>
    </template>
    <Column field="referenceName" header="Referència" style="width: 28%">
    </Column>
    <Column field="locationName" header="Ubicació"></Column>
    <Column field="oldQuantity" header="Uds."></Column>
    <Column header="Recompte" style="width: 12%">
      <template #body="slotProps">
        <BaseInput
          label=""
          id="newQuantity"
          v-model="slotProps.data.newQuantity"
        ></BaseInput>
      </template>
    </Column>
    <Column field="width" header="Ample (x) mm"></Column>
    <Column field="length" header="Llarg (y) mm"></Column>
    <Column field="height" header="Alt (z) mm"></Column>
    <Column field="diameter" header="Diàmetre mm"></Column>
    <Column field="thickness" header="Gruix mm"></Column>
  </DataTable>
  <Dialog :closable="true" v-model:visible="isDialogVisible" :modal="true">
    <FormInventoryNewMovements
      :newMovement="newStockMovement"
      @submit="submitDetailForm"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import BaseInput from "../../../components/BaseInput.vue";
import TableFilter, {
  type FilterBodyWidth,
  type FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import { useStore } from "../../../store";
import { useStockStore } from "../store/stock";
import { useInventoryStore } from "../store/inventory";
import { useReferenceStore } from "../../shared/store/reference";

import { computed, onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { Inventory, StockMovement } from "../types";
import { useStockMovementStore } from "../store/stockMovement";
import FormInventoryNewMovements from "../components/FormInventoryNewMovements.vue";
import DropdownWarehousesWithLocations from "../components/DropdownWarehousesWithLocations.vue";
import { getNewUuid } from "../../../utils/functions";

const store = useStore();
const toast = useToast();

const stockStore = useStockStore();
const inventoryStore = useInventoryStore();
const stockMovementStore = useStockMovementStore();
const referenceStore = useReferenceStore();

const filter = ref({
  referenceName: "",
  locationId: undefined as string | undefined,
});

const filterConfig: Array<FilterConfig> = [
  {
    key: "referenceName",
    label: "Referència",
    type: "text",
    placeholder: "Referència",
    size: "md",
    row: 0,
  },
];

const filterBodyWidth: FilterBodyWidth = {
  desktop: "55%",
  tablet: "70%",
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: "Inventari",
  });

  await refreshData();
});

const refreshData = async () => {
  await stockStore.fetchStocks();
  inventoryStore.inventories = [];
  stockStore.stocks?.forEach((stock) => {
    let invent = {
      id: uuidv4(),
      stockId: stock.id,
      movementType: "bal",
      locationId: stock.locationId,
      locationName: stock.locationName,
      referenceId: stock.referenceId,
      referenceName: stock.referenceDisplay,
      oldQuantity: stock.quantity,
      newQuantity: stock.quantity,
      width: stock.width,
      length: stock.length,
      height: stock.height,
      diameter: stock.diameter,
      thickness: stock.thickness,
      movementDate: new Date(),
    } as Inventory;
    inventoryStore.inventories?.push(invent);
  });
};

const filteredInventories = computed(() => {
  let result = inventoryStore.inventories ?? [];

  if (filter.value.referenceName) {
    result = result.filter((inv) =>
      inv.referenceName
        ?.toLowerCase()
        .includes(filter.value.referenceName.toLowerCase()),
    );
  }

  if (filter.value.locationId) {
    result = result.filter((inv) => inv.locationId === filter.value.locationId);
  }

  return result;
});

const cleanFilter = () => {
  filter.value.referenceName = "";
  filter.value.locationId = undefined;
};

const isDialogVisible = ref(false);
const newStockMovement = ref({} as Inventory);

const submitDetailForm = (inventory: Inventory) => {
  inventory.referenceName = referenceStore.getFullNameById(inventory.referenceId);
  inventoryStore.inventories?.push(inventory);
  isDialogVisible.value = false;
};

const newMovement = () => {
  isDialogVisible.value = true;
  newStockMovement.value = {
    id: getNewUuid(),
    stockId: getNewUuid(),
    movementType: "",
    locationId: null,
    referenceId: "",
    oldQuantity: 0,
    newQuantity: 0,
    width: 0,
    length: 0,
    height: 0,
    diameter: 0,
    thickness: 0,
    movementDate: new Date(),
  } as Inventory;
};

const saveMovement = async () => {
  const promises = [] as Array<Promise<boolean>>;

  inventoryStore.inventories
    ?.filter((el) => el.newQuantity != el.oldQuantity)
    .forEach((m) => {
      const isOutput = m.newQuantity < m.oldQuantity;
      const stock: StockMovement = {
        id: m.id,
        stockId: m.stockId,
        movementType: isOutput ? "OUTPUT" : "INPUT",
        locationId: m.locationId || null,
        location: null,
        referenceId: m.referenceId,
        quantity: m.newQuantity - m.oldQuantity,
        width: m.width,
        length: m.length,
        height: m.height,
        diameter: m.diameter,
        thickness: m.thickness,
        movementDate: m.movementDate,
        description: isOutput
          ? "Sortida per inventari"
          : "Entrada per inventari",
      };

      promises.push(stockMovementStore.create(stock));
    });

  const results = await Promise.all(promises);
  // Check if all promises resolved successfully
  if (results.filter((p) => p === true).length === promises.length) {
    toast.add({
      severity: "success",
      summary: "Inventari creat correctament",
      life: 5000,
    });

    refreshData();
  } else {
    toast.add({
      severity: "error",
      summary: "Error al crear el moviment d'inventari",
      life: 5000,
    });
  }
};
</script>
