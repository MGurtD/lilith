<template>
  <Table
    :items="plantmodelStore.machineStatuses ?? []"
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
      <span class="text-900 font-bold">{{ pt("Estats de màquina") }}</span>
    </template>
    <template #body-color="{ data }">
      <ColorColumn :value="data.color" />
    </template>
    <template #body-icon="{ data }">
      <IconColumn :value="data.icon" />
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { useI18n } from "vue-i18n";
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { MachineStatus } from "../types";
import ColorColumn from "../../../components/tables/ColorColumn.vue";
import IconColumn from "../../../components/tables/IconColumn.vue";

const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const toast = useToast();
const confirm = useConfirm();

const columns = computed<Column[]>(() => [
  { field: "name", header: pt("Nom"), style: "width: 25%" },
  {
    field: "description",
    header: pt("Descripció"),
    style: "width: 40%",
  },
  {
    field: "color",
    header: pt("Color"),
    style: "width: 5%",
    truncate: false,
  },
  {
    field: "icon",
    header: pt("Icona"),
    style: "width: 5%",
    truncate: false,
  },
  {
    field: "stopped",
    header: pt("Aturada"),
    columnType: ColumnType.Boolean,
    style: "width: 2%",
  },
  {
    field: "operatorsAllowed",
    header: pt("Operaris"),
    columnType: ColumnType.Boolean,
    style: "width: 2%",
  },
  {
    field: "closed",
    header: pt("Tancada"),
    columnType: ColumnType.Boolean,
    style: "width: 2%",
  },
  {
    field: "preferred",
    header: pt("Preferit"),
    columnType: ColumnType.Boolean,
    style: "width: 2%",
  },
  {
    field: "workOrderAllowed",
    header: pt("Permet OF"),
    columnType: ColumnType.Boolean,
    style: "width: 2%",
  },
  {
    field: "disabled",
    header: pt("Desactivat"),
    columnType: ColumnType.Boolean,
    style: "width: 2%",
  },
]);

onMounted(async () => {
  await plantmodelStore.fetchMachineStatuses();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.detail.machineStatusesTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/machinestatus/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/machinestatus/${row.data.id}` });
};
const deleteButton = (machineStatus: MachineStatus) => {
  confirm.require({
    message: t("production.messages.confirmDeleteMachineStatus", {
      name: machineStatus.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteMachineStatus(
        machineStatus.id,
      );

      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminat"),
          life: 3000,
        });
        await plantmodelStore.fetchMachineStatuses();
      }
    },
  });
};
</script>
