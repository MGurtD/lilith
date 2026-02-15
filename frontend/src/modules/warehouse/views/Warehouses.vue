<template>
  <DataTable
    :value="warehouseStore.warehouses"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Magatzem</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 25%"></Column>
    <Column field="description" header="Descripció" style="width: 50%"></Column>
    <Column header="Desactivada" style="width: 20%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useWarehouseStore } from "../store/warehouse";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Warehouse } from "../types";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const warehouseStore = useWarehouseStore();
const plantmodelStore = usePlantModelStore();

onMounted(async () => {
  await warehouseStore.fetchWarehouses();
  await plantmodelStore.fetchSites();

  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: "Gestió de magatzems",
  });
});

const createButtonClick = () => {
  router.push({ path: `/warehouse/${uuidv4()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/warehouse/${row.data.id}` });
  }
};

const deleteButton = (event: any, warehouse: Warehouse) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el magatzem ${warehouse.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await warehouseStore.deleteWarehouse(warehouse.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await warehouseStore.fetchWarehouses();
      }
    },
  });
};
</script>
