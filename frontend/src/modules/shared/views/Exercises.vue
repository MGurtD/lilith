<template>
  <Table
    :items="exercises ?? []"
    :columns="columns"
    :filter-config="[]"
    :show-filter-actions="false"
    preset="crud-list"
    dataKey="id"
    tableStyle="min-width: 100%"
    @create="createButtonClick"
    @row-click="editExercise"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{
        t("shared.exercises.title")
      }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { PrimeIcons } from "@primevue/core/api";
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { storeToRefs } from "pinia";
import { useStore } from "../../../store";
import { useExerciseStore } from "../store/exercise";
import { getNewUuid } from "../../../utils/functions";

const router = useRouter();
const store = useStore();
const exerciseStore = useExerciseStore();
const { exercises } = storeToRefs(exerciseStore);
const { t } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("shared.exercises.columns.name"),
    style: "width: 15%",
  },
  {
    field: "description",
    header: t("shared.exercises.columns.description"),
    style: "width: 25%",
  },
  {
    field: "startDate",
    header: t("shared.exercises.columns.startDate"),
    columnType: ColumnType.Date,
    style: "width: 20%",
  },
  {
    field: "endDate",
    header: t("shared.exercises.columns.endDate"),
    columnType: ColumnType.Date,
    style: "width: 20%",
  },
  {
    field: "disabled",
    header: t("shared.exercises.columns.disabled"),
    columnType: ColumnType.Boolean,
    showColor: false,
    style: "width: 10%",
  },
]);

onMounted(async () => {
  await exerciseStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: t("shared.exercises.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/exercise/${getNewUuid()}` });
};

const editExercise = (row: DataTableRowClickEvent) => {
  router.push({ path: `/exercise/${row.data.id}` });
};
</script>
