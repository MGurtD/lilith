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
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter-3">
          <div class="filter-field">
            <label>Codi</label>
            <BaseInput v-model="filter.code" />
          </div>
          <div class="filter-field">
            <label>Descripció</label>
            <BaseInput v-model="filter.description" />
          </div>
          <div class="filter-field">
            <label>Client</label>
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
    </template>
    <Column field="code" header="Codi" style="width: 15%"></Column>
    <Column field="description" header="Descripció" style="width: 35%"></Column>
    <Column field="version" header="Versió" style="width: 10%"></Column>
    <Column field="customerId" header="Client" style="width: 20%">
      <template #body="slotProps">
        <span>{{ getCustomerById(slotProps.data.customerId) }}</span>
      </template>
    </Column>
    <Column field="price" header="Preu" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.price) }}
      </template>
    </Column>
    <Column field="cost" header="Cost" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.workMasterCost) }}
      </template>
    </Column>
    <Column header="Servei" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isService" />
      </template>
    </Column>

    <Column style="width: 10%">
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
import BaseInput from "../../../components/BaseInput.vue";
import { computed, ref, onUnmounted, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Reference } from "../../shared/types";
import { useCustomersStore } from "../../sales/store/customers";
import { formatCurrency } from "../../../utils/functions";
import { useUserFilterStore } from "../../../store/userfilter";

const userFilterStore = useUserFilterStore();
const customerStore = useCustomersStore();
const filter = ref({
  code: "",
  description: "",
  customerId: "",
});

onMounted(() => {
  const userFilter = userFilterStore.getFilter("References", "");
  if (userFilter) {
    if (userFilter.code) filter.value.code = userFilter.code;
    if (userFilter.customerId) filter.value.customerId = userFilter.customerId;
  }
});
onUnmounted(async () => {
  await userFilterStore.addFilter("References", "", filter.value);
});

const cleanFilter = () => {
  filter.value.code = "";
  filter.value.customerId = "";
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
.references-header {
  display: grid;
  grid-template-columns: 3fr 0.1fr;
}
.references-filter {
  display: grid;
  grid-template-columns: 0.2fr 0.8fr;
  align-items: center;
  width: 25vw;
}
</style>
