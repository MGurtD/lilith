<template>
  <Table
    :items="warehouseStore.warehouses ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{
        t("warehouse.fields.warehouse")
      }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useWarehouseStore } from "../store/warehouse";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { computed, onMounted, watch } from "vue";
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

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("warehouse.fields.name"),
    style: "width: 25%",
  },
  {
    field: "description",
    header: t("common.description"),
    style: "width: 50%",
  },
  {
    field: "disabled",
    header: t("warehouse.fields.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 20%",
  },
]);

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
  router.push({ path: `/warehouse/${row.data.id}` });
};

const deleteButton = (warehouse: Warehouse) => {
  confirm.require({
    message: t("warehouse.messages.confirmDeleteWarehouse", {
      name: warehouse.name,
    }),
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
