<template>
  <DataTable
    :value="filteredData"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    paginator
    :rows="20"
    @row-click="editRow"
  >
    <template #header>
      <TableFilter
        :config="filterConfig"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :show-filter-action="false"
        :body-width="filterBodyWidth"
        embedded
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Client</label
            >
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Data creació</label
            >
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
      </TableFilter>
    </template>
    <Column field="code" header="Codi" style="width: 10%"></Column>
    <Column field="description" header="Descripció" style="width: 30%"></Column>
    <Column field="version" header="Versió" style="width: 8%"></Column>
    <Column field="customerId" header="Client" style="width: 18%">
      <template #body="slotProps">
        <span>{{ getCustomerById(slotProps.data.customerId) }}</span>
      </template>
    </Column>
    <Column field="createdOn" header="Data creació" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.createdOn) }}
      </template>
    </Column>
    <Column field="price" header="Preu" style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.price) }}
      </template>
    </Column>
    <Column field="cost" header="Cost" style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.workMasterCost) }}
      </template>
    </Column>
    <Column header="Servei" style="width: 5%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isService" />
      </template>
    </Column>
    <Column style="width: 3%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import TableFilter from "../../../components/tables/TableFilter.vue";
import type {
  FilterConfig,
  FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";
import { computed, ref, onUnmounted, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Reference } from "../../shared/types";
import { useCustomersStore } from "../../sales/store/customers";
import { formatCurrency, formatDate } from "../../../utils/functions";
import { useUserFilterStore } from "../../../store/userfilter";

const userFilterStore = useUserFilterStore();
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

const filter = ref({
  code: "",
  description: "",
  customerId: "",
  dates: undefined as Array<Date> | undefined,
});

onMounted(() => {
  const userFilter = userFilterStore.getFilter("References", "");
  if (userFilter) {
    if (userFilter.code) filter.value.code = userFilter.code;
    if (userFilter.customerId) filter.value.customerId = userFilter.customerId;
    if (userFilter.dates) filter.value.dates = userFilter.dates;
  }
});
onUnmounted(async () => {
  await userFilterStore.addFilter("References", "", filter.value);
});

const cleanFilter = () => {
  filter.value.code = "";
  filter.value.customerId = "";
  filter.value.description = "";
  filter.value.dates = undefined;
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
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, reference: Reference) => {
  emit("delete", reference);
};

const getCustomerById = (customerId: string) => {
  const customer = customerStore.customers?.find((c) => c.id === customerId);
  return customer ? customer.comercialName : "";
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
