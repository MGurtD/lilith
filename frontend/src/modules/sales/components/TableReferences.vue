<template>
  <Table
    :columns="columns"
    :items="filteredData"
    :filter-config="filterConfig"
    :filter-labels="filterMetadata.filterLabels"
    :filter-value-resolvers="filterMetadata.filterValueResolvers"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    preset="crud-list"
    page="References"
    :attachment-config="{
      entity: 'referenceMaps',
      title: 'Adjunts de la referència',
      titleField: 'code',
    }"
    showDeleteColumn
    :canDelete="() => true"
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="onDeleteRow"
    @row-click="editRow"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">Client</label>
        <DropdownCustomers label="" v-model="filter.customerId" />
      </div>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">Data creació</label>
        <DatePicker
          v-model="filter.dates"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          :showIcon="true"
          class="w-full"
          size="small"
          placeholder="Selecciona periode"
        />
      </div>
    </template>
    <template #body-cost="{ data }">
      {{ formatCurrency(data.workMasterCost) }}
    </template>
  </Table>
</template>

<script setup lang="ts">
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";
import type { FilterConfig, FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { computed, ref } from "vue";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Reference } from "../../shared/types";
import { useCustomersStore } from "../../sales/store/customers";
import { formatCurrency } from "../../../utils/functions";
import { createSalesTableViewFilterMetadata } from "@/modules/sales/utils/sales-table-view-filter-metadata";

const customerStore = useCustomersStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "75%" };

const filterConfig: FilterConfig[] = [
  {
    key: "code",
    label: "Codi",
    type: "text",
    placeholder: "Codi",
    size: "sm",
  },
  {
    key: "description",
    label: "Descripció",
    type: "text",
    placeholder: "Descripció",
    size: "md",
  },
];

const columns = ref<Column[]>([
  { field: "code", header: "Codi", style: "width: 10%" },
  { field: "description", header: "Descripció", style: "width: 30%" },
  { field: "version", header: "Versió", style: "width: 8%" },
  {
    field: "customerId",
    header: "Client",
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerNameById,
    style: "width: 18%",
  },
  { field: "createdOn", header: "Data creació", sortable: true, columnType: ColumnType.Date, style: "width: 10%" },
  { field: "price", header: "Preu", columnType: ColumnType.Currency, style: "width: 8%" },
  { field: "cost", header: "Cost", style: "width: 8%" },
  { field: "isService", header: "Servei", columnType: ColumnType.Boolean, style: "width: 5%" },
]);

const filterMetadata = createSalesTableViewFilterMetadata(columns.value, {
  dateLabel: "Data creació",
});

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

<style scoped>
.filter-toolbar {
  align-items: flex-start;
}

.filter-toolbar__actions {
  align-self: flex-end;
}

.filter-toolbar__field--date {
  min-width: 15rem;
}
</style>
