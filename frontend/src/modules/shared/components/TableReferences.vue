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
        <div class="datatable-filter">
          <div class="filter-field">
            <label>Codi</label>
            <BaseInput v-model="filter.code" />
          </div>
          <div class="filter-field" v-if="isSales">
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
    <Column
      v-if="isSales"
      field="version"
      header="Versió"
      style="width: 10%"
    ></Column>
    <Column
      v-if="isSales"
      field="customerId"
      header="Client"
      style="width: 20%"
    >
      <template #body="slotProps">
        <span>{{ getCustomerById(slotProps.data.customerId) }}</span>
      </template>
    </Column>
    <Column v-if="isSales" field="price" header="Preu" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.price) }}
      </template>
    </Column>
    <Column v-if="isSales" field="cost" header="Cost" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.workMasterCost) }}
      </template>
    </Column>
    <Column header="Servei" v-if="isSales" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isService" />
      </template>
    </Column>
    <Column
      v-if="isPurchase"
      field="referenceTypeId"
      header="Tipus"
      style="width: 15%"
    >
      <template #body="slotProps">
        <span>{{ getTypeDescription(slotProps.data.referenceTypeId) }}</span>
      </template>
    </Column>
    <Column v-if="isPurchase" header="Densitat (mm)" style="width: 10%">
      <template #body="slotProps">
        {{ getReferenceTypeDensity(slotProps.data.referenceTypeId) }}
      </template>
    </Column>
    <Column
      v-if="isPurchase"
      field="referenceFormatId"
      header="Format"
      style="width: 10%"
    >
      <template #body="slotProps">
        <span>{{
          getFormatDescription(slotProps.data.referenceFormatId)
        }}</span>
      </template>
    </Column>
    <Column
      v-if="isPurchase"
      field="lastPurchaseCost"
      header="Última Compra"
      style="width: 10%"
    ></Column>

    <!-- <Column header="Desc." style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column> -->
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
import { computed, ref } from "vue";
import { useReferenceStore } from "../store/reference";

import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Reference } from "../types";
import { useReferenceTypeStore } from "../store/referenceType";
import { useCustomersStore } from "../../sales/store/customers";
import { formatCurrency } from "../../../utils/functions";

const referenceTypeStore = useReferenceTypeStore();
const referenceStore = useReferenceStore();
const customerStore = useCustomersStore();
const filter = ref({
  code: "",
  customerId: "",
});

const cleanFilter = () => {
  filter.value.code = "";
  filter.value.customerId = "";
};

const props = defineProps<{
  module: string;
  references: Array<Reference> | undefined;
}>();

const emit = defineEmits<{
  (e: "add"): void;
  (e: "edit", reference: Reference): void;
  (e: "delete", reference: Reference): void;
}>();

const isSales = computed(() => {
  return props.module === "sales";
});
const isPurchase = computed(() => {
  return props.module === "purchase";
});
const isProduction = computed(() => {
  return props.module === "production";
});

const filteredData = computed(() => {
  if (!props.references) return [];
  let filteredReferences = props.references;

  // Customer filter
  if (filter.value.customerId!.length > 0) {
    filteredReferences = filteredReferences.filter(
      (r) => r.customerId === filter.value.customerId,
    );
  }
  // Code filter
  if (filter.value.code.length > 0) {
    filteredReferences = filteredReferences.filter((r) =>
      r.code.toLowerCase().includes(filter.value.code.toLowerCase()),
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

const getReferenceTypeDensity = (referenceTypeId: string) => {
  const referenceType =
    referenceTypeStore.getReferenceTypeById(referenceTypeId);
  return referenceType ? referenceType.density : 0;
};

const getFormatDescription = (formatId: string) => {
  const format = referenceStore.referenceFormats?.find(
    (f) => f.id === formatId,
  );
  if (format) return format.description;
  else return "";
};

const getTypeDescription = (referenceTypeId: string) => {
  const referenceType =
    referenceTypeStore.getReferenceTypeById(referenceTypeId);
  return referenceType ? referenceType.description : "";
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
