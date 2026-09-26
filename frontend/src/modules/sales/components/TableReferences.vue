<template>
  <Table
    :columns="columns"
    :items="filteredData"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    preset="crud-list"
    page="References"
    :attachment-config="{
      entity: 'referenceMaps',
      title: t('sales.components.adjuntsDeLaReferencia'),
      titleField: 'code',
    }"
    showDeleteColumn
    :canDelete="() => true"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="onDeleteRow"
    @row-click="editRow"
  >
    <template #filter-customerId="{ value, update }">
      <DropdownCustomers size="small" label="" :model-value="value" @update:model-value="update" />
    </template>
    <template #body-cost="{ data }">
      {{ formatCurrency(data.workMasterCost) }}
    </template>
  </Table>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";
import type { FilterConfig, FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { computed, ref } from "vue";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Reference } from "../../shared/types";
import { useCustomersStore } from "../../sales/store/customers";
import { formatCurrency } from "../../../utils/functions";

const { t } = useI18n();
const customerStore = useCustomersStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "75%" };

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "customerId",
    label: t('sales.components.client'),
    type: "slot",
    valueLabel: (value) => customerStore.getCustomerNameById(String(value)),
  },
  {
    key: "dates",
    label: t('sales.components.dataCreacio'),
    type: "date-range",
    placeholder: t('sales.components.seleccionaPeriode'),
  },
  {
    key: "code",
    label: t('sales.components.codi'),
    type: "text",
    placeholder: t('sales.components.codi'),
    size: "sm",
  },
  {
    key: "description",
    label: t('sales.components.descripcio'),
    type: "text",
    placeholder: t('sales.components.descripcio'),
    size: "md",
  },
]);

const columns = ref<Column[]>([
  { field: "code", header: t('sales.components.codi'), style: "width: 10%" },
  { field: "description", header: t('sales.components.descripcio'), style: "width: 30%" },
  { field: "version", header: t('sales.components.versio'), style: "width: 8%" },
  {
    field: "customerId",
    header: t('sales.components.client'),
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerNameById,
    style: "width: 18%",
  },
  { field: "createdOn", header: t('sales.components.dataCreacio'), sortable: true, columnType: ColumnType.Date, style: "width: 10%" },
  { field: "price", header: t('sales.components.preu'), columnType: ColumnType.Currency, style: "width: 8%" },
  { field: "cost", header: t('sales.components.cost'), style: "width: 8%" },
  { field: "isService", header: t('sales.components.servei'), columnType: ColumnType.Boolean, style: "width: 5%" },
]);

const filter = ref({
  code: "",
  description: "",
  customerId: "",
  dates: undefined as Array<Date> | undefined,
});

const cleanFilter = () => {
  filter.value = {
    code: "",
    description: "",
    customerId: "",
    dates: undefined,
  };
};

const props = defineProps<{
  references: Array<Reference> | undefined;
}>();

const emit = defineEmits<{
  (e: "add"): void;
  (e: "edit", reference: Reference): void;
  (e: "delete", reference: Reference): void;
}>();

const filteredData = computed(() => {
  if (!props.references) return [];
  let filteredReferences = props.references;

  // Customer filter
  if (filter.value.customerId && filter.value.customerId!.length > 0) {
    filteredReferences = filteredReferences.filter(
      (r) => r.customerId === filter.value.customerId,
    );
  }
  // Code filter
  if (filter.value.code && filter.value.code.length > 0) {
    filteredReferences = filteredReferences.filter((r) =>
      r.code.toLowerCase().includes(filter.value.code.toLowerCase()),
    );
  }

  // Description filter
  if (filter.value.description && filter.value.description.length > 0) {
    filteredReferences = filteredReferences.filter((r) =>
      r.description
        .toLowerCase()
        .includes(filter.value.description.toLowerCase()),
    );
  }

  // Date range filter
  if (filter.value.dates && filter.value.dates.length > 0) {
    const startDate = filter.value.dates[0];
    if (startDate) {
      filteredReferences = filteredReferences.filter(
        (r) => new Date(r.createdOn) >= startDate,
      );
    }
    if (filter.value.dates.length > 1 && filter.value.dates[1]) {
      const endDate = new Date(filter.value.dates[1]);
      endDate.setHours(23, 59, 59, 999);
      filteredReferences = filteredReferences.filter(
        (r) => new Date(r.createdOn) <= endDate,
      );
    }
  }

  return filteredReferences;
});

const createButtonClick = () => {
  emit("add");
};

const editRow = (row: DataTableRowClickEvent) => {
  emit("edit", row.data);
};

const onDeleteRow = (reference: Reference) => {
  emit("delete", reference);
};
</script>
