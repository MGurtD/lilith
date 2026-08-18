<template>
  <Table
    :items="plantmodelStore.workcenterTypes ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ pt("Tipus de màquina") }}</span>
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
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { WorkcenterType } from "../types";

const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: pt("Nom"),
    style: "width: 25%",
  },
  {
    field: "description",
    header: pt("Descripció"),
    style: "width: 50%",
  },
  {
    field: "profitPercentage",
    header: pt("% Benefici"),
    resolver: (value) => (typeof value === "number" ? `${value} %` : ""),
    style: "width: 10%",
  },
  {
    field: "disabled",
    header: pt("Desactivat"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await plantmodelStore.fetchWorkcenterTypes();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: pt("Gestió de tipus de màquina"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/workcentertype/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/workcentertype/${row.data.id}` });
};
const deleteButton = (entity: WorkcenterType) => {
  confirm.require({
    message: t("production.messages.confirmDeleteWorkcenterType", {
      name: entity.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteWorkcenterType(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminat"),
          life: 3000,
        });
        await plantmodelStore.fetchWorkcenterTypes();
      }
    },
  });
};
</script>
