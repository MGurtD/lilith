<template>
  <Table
    :items="filteredPaymentMethods"
    :columns="columns"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :show-filter-action="false"
    preset="crud-list"
    tableStyle="min-width: 100%"
    @clear="cleanFilter"
    @create="createButtonClick"
    @row-click="editPaymentMethod"
  />
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type { FilterConfig } from "@/components/tables/TableFilter.vue";
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useStore } from "../../../store";
import { usePaymentMethodStore } from "../store/paymentMethod";

const router = useRouter();
const store = useStore();
const paymentMethodStore = usePaymentMethodStore();
const { t } = useI18n();

const filter = ref({
  search: "",
});

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "search",
    label: t("shared.paymentMethods.filters.searchLabel"),
    type: "text",
    placeholder: t("shared.paymentMethods.filters.searchPlaceholder"),
    size: "sm",
    row: 0,
  },
]);

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("shared.paymentMethods.columns.name"),
    style: "width: 20%",
  },
  {
    field: "description",
    header: t("shared.paymentMethods.columns.description"),
    style: "width: 20%",
  },
  {
    field: "dueDays",
    header: t("shared.paymentMethods.columns.dueDays"),
    columnType: ColumnType.Number,
    style: "width: 20%",
  },
  {
    field: "paymentDay",
    header: t("shared.paymentMethods.columns.paymentDay"),
    columnType: ColumnType.Number,
    style: "width: 20%",
  },
  {
    field: "disabled",
    header: t("shared.paymentMethods.columns.disabled"),
    columnType: ColumnType.Boolean,
    showColor: false,
    style: "width: 20%",
  },
]);

const filteredPaymentMethods = computed(() => {
  if (!paymentMethodStore.paymentMethods) return [];

  const search = filter.value.search.trim().toLowerCase();
  if (!search) return paymentMethodStore.paymentMethods;

  return paymentMethodStore.paymentMethods.filter((paymentMethod) => {
    return (
      paymentMethod.name.toLowerCase().includes(search) ||
      paymentMethod.description.toLowerCase().includes(search)
    );
  });
});

onMounted(async () => {
  await paymentMethodStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: t("shared.paymentMethods.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/payment-methods/${getNewUuid()}` });
};

const cleanFilter = () => {
  filter.value.search = "";
};

const editPaymentMethod = (row: DataTableRowClickEvent) => {
  router.push({ path: `/payment-methods/${row.data.id}` });
};
</script>

<style scoped>
:deep(.table-filter__field) {
  max-width: 25%;
}

@media (max-width: 1200px) {
  :deep(.table-filter__field) {
    max-width: 40%;
  }
}

@media (max-width: 768px) {
  :deep(.table-filter__field) {
    max-width: 100%;
  }
}
</style>
