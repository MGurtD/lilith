<template>
  <Table
    :items="plantmodelStore.operatorTypes ?? []"
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
        t("production.operatorTypes.title")
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
import { usePlantModelStore } from "../store/plantmodel";
import { computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { OperatorType } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const confirm = useConfirm();
const toast = useToast();
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("production.fields.name"),
    style: "width: 25%",
  },
  {
    field: "description",
    header: t("common.description"),
    style: "width: 50%",
  },
  {
    field: "disabled",
    header: t("production.fields.disabled"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await plantmodelStore.fetchOperatorTypes();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.operatorTypes.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/operatortype/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/operatortype/${row.data.id}` });
};

const deleteButton = (operatorType: OperatorType) => {
  confirm.require({
    message: t("production.messages.confirmDeleteOperatorType", {
      name: operatorType.name,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteOperatorType(operatorType.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("production.messages.deleted"),
          life: 3000,
        });
        await plantmodelStore.fetchOperatorTypes();
      }
    },
  });
};
</script>
