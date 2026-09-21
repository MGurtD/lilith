<template>
  <div>
    <Table
      :items="filteredReferences"
      :columns="columns"
      :filter-config="filterConfig"
      v-model:filter-values="filter"
      :show-filter-action="false"
      preset="crud-list"
      tableStyle="min-width: 100%"
      :loading="loading"
      sortField="code"
      :sortOrder="1"
      removableSort
      @clear="clearFilter"
      @create="createReference"
      @row-click="openReference"
    >
      <template #empty>{{ t("shared.references.empty") }}</template>
    </Table>
  </div>
</template>

<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type { FilterConfig } from "@/components/tables/TableFilter.vue";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";

import { useStore } from "../../../store";
import { useReferenceStore } from "../store/reference";
import { getNewUuid } from "../../../utils/functions";

const router = useRouter();
const store = useStore();
const referenceStore = useReferenceStore();
const { t } = useI18n();

const { references } = storeToRefs(referenceStore);
const loading = ref(false);

const filter = ref({
  search: "",
});

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "search",
    label: t("shared.references.searchPlaceholder"),
    type: "text",
    placeholder: t("shared.references.searchPlaceholder"),
    size: "sm",
  },
]);

const columns = computed<Column[]>(() => [
  {
    field: "code",
    header: t("shared.references.columns.code"),
    sortable: true,
  },
  {
    field: "description",
    header: t("shared.references.columns.description"),
    sortable: true,
  },
  {
    field: "version",
    header: t("shared.references.columns.version"),
  },
  {
    field: "sales",
    header: t("shared.references.columns.sales"),
    columnType: ColumnType.Boolean,
    showColor: false,
  },
  {
    field: "purchase",
    header: t("shared.references.columns.purchase"),
    columnType: ColumnType.Boolean,
    showColor: false,
  },
  {
    field: "production",
    header: t("shared.references.columns.production"),
    columnType: ColumnType.Boolean,
    showColor: false,
  },
  {
    field: "active",
    header: t("shared.references.columns.active"),
    columnType: ColumnType.Boolean,
    showColor: false,
  },
]);

const tableItems = computed(() =>
  (references.value ?? []).map((reference) => ({
    ...reference,
    active: !reference.disabled,
  })),
);

const filteredReferences = computed(() => {
  const search = filter.value.search.trim().toLowerCase();
  if (!search) return tableItems.value;

  return tableItems.value.filter(
    (reference) =>
      reference.code.toLowerCase().includes(search) ||
      reference.description.toLowerCase().includes(search),
  );
});

const clearFilter = () => {
  filter.value.search = "";
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    backButtonVisible: false,
    title: t("shared.references.menuTitle"),
  });
  loading.value = true;
  await referenceStore.fetchReferences();
  loading.value = false;
});

const openReference = (row: DataTableRowClickEvent) => {
  router.push({ path: `/reference-management/${row.data.id}` });
};

const createReference = () => {
  router.push({ path: `/reference-management/${getNewUuid()}` });
};
</script>
