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
        <span class="text-900 font-bold">{{ t("warehouse.fields.warehouse") }}</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" :header="t('warehouse.fields.name')" style="width: 25%"></Column>
    <Column field="description" :header="t('common.description')" style="width: 50%"></Column>
    <Column :header="t('warehouse.fields.disabled')" style="width: 20%">
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
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useWarehouseStore } from "../store/warehouse";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { onMounted, watch } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Warehouse } from "../types";

const router = useRouter();
const { t, locale } = useI18n();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const warehouseStore = useWarehouseStore();
const plantmodelStore = usePlantModelStore();

const setMenuTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: t("warehouse.warehouses.title"),
  });
};

watch(locale, setMenuTitle, { immediate: true });

onMounted(async () => {
  await warehouseStore.fetchWarehouses();
  await plantmodelStore.fetchSites();
});

const createButtonClick = () => {
  router.push({ path: `/warehouse/${getNewUuid()}` });
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
    message: t("warehouse.messages.confirmDeleteWarehouse", { name: warehouse.name }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await warehouseStore.deleteWarehouse(warehouse.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("warehouse.messages.deleted"),
          life: 3000,
        });
        await warehouseStore.fetchWarehouses();
      }
    },
  });
};
</script>
