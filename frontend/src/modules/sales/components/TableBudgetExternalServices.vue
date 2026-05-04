<template>
  <DataTable
    :value="externalServices"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
    sortMode="single"
    sortField="description"
    dataKey="id"
    :sortOrder="1"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <Column field="description" header="Descripció" style="width: 25%" />
    <Column header="Referència" style="width: 15%">
      <template #body="slotProps">
        <LinkReference :id="slotProps.data.referenceId" />
      </template>
    </Column>
    <Column header="Proveïdor" style="width: 20%">
      <template #body="slotProps">
        <Select
          v-model="slotProps.data.supplierId"
          :options="slotProps.data.availableSuppliers"
          optionLabel="comercialName"
          optionValue="id"
          placeholder="Selecciona proveïdor..."
          class="w-full"
          :size="'small'"
          @change="onSupplierChange(slotProps.data)"
        />
      </template>
    </Column>
    <Column field="quantity" header="Quantitat" style="width: 8%" />
    <Column field="weight" header="Pes (kg)" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.weight.toFixed(4) }}
      </template>
    </Column>
    <Column field="volume" header="Volum (m³)" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.volume.toFixed(4) }}
      </template>
    </Column>
    <Column field="unitPrice" header="Preu unit." style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.unitPrice) }}
      </template>
    </Column>
    <Column field="totalPrice" header="Total" style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.totalPrice) }}
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import LinkReference from "../../shared/components/LinkReference.vue";
import { BudgetExternalService } from "../types";
import { Supplier } from "../../purchase/types";
import { formatCurrency } from "../../../utils/functions";

export interface BudgetExternalServiceRow extends BudgetExternalService {
  availableSuppliers: Supplier[];
}

defineProps<{
  externalServices: BudgetExternalServiceRow[];
}>();

const emit = defineEmits<{
  (e: "supplierChange", row: BudgetExternalServiceRow): void;
}>();

const onSupplierChange = (row: BudgetExternalServiceRow) => {
  emit("supplierChange", row);
};
</script>
