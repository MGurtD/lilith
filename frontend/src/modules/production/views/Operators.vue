<template>
  <Table
    phone-layout="cards"
    :card-layout="cardLayout"
    :items="tableItems"
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
      <span class="text-900 font-bold">{{ pt("Operaris") }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "@/components/tables/types";
import { useI18n } from "vue-i18n";
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useConfirmDelete } from "@/composables/useConfirmDelete";
import { usePlantModelStore } from "../store/plantmodel";
import { useOperatorTypeStore } from "../store/operatortype";
import { computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Operator } from "../types";

const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const operatorTypeStore = useOperatorTypeStore();
const confirmDelete = useConfirmDelete();

const tableItems = computed(() =>
  (plantmodelStore.operators ?? []).map((operator) => ({
    ...operator,
    fullName: `${operator.name} ${operator.surname}`.trim(),
  })),
);

const columns = computed<Column[]>(() => [
  {
    field: "code",
    header: pt("Codi"),
    style: "width: 15%",
  },
  {
    field: "fullName",
    header: pt("Nom complet"),
    style: "width: 35%",
  },
  {
    field: "vatNumber",
    header: "NIF",
    style: "width: 15%",
  },
  {
    field: "operatorTypeId",
    header: pt("Tipus"),
    columnType: ColumnType.Lookup,
    resolver: (value) =>
      typeof value === "string"
        ? operatorTypeStore.getOperatorTypeNameById(value)
        : "",
    style: "width: 15%",
  },
  {
    field: "disabled",
    header: pt("Desactivat"),
    columnType: ColumnType.Boolean,
    style: "width: 10%",
  },
]);

const cardLayout: CardLayout = {
  title: "fullName",
  trailing: "code",
  subtitle: "operatorTypeId",
  meta: ["vatNumber", "disabled"],
};

onMounted(async () => {
  await plantmodelStore.fetchOperators();
  if (!operatorTypeStore.operatorTypes) {
    await operatorTypeStore.fetchOperatorTypes();
  }

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.detail.operatorsTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/operator/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  router.push({ path: `/operator/${row.data.id}` });
};

const deleteButton = (operator: Operator) =>
  confirmDelete({
    name: `${operator.surname}, ${operator.name}`,
    remove: () => plantmodelStore.deleteOperator(operator.id),
    onDeleted: () => plantmodelStore.fetchOperators(),
  });
</script>
