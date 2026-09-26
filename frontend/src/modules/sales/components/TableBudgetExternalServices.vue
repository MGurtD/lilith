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
    <Column field="description" :header="t('sales.components.descripcio')" style="width: 25%" />
    <Column :header="t('sales.components.referencia')" style="width: 15%">
      <template #body="slotProps">
        <LinkReference :id="slotProps.data.referenceId" />
      </template>
    </Column>
    <Column :header="t('sales.components.proveidor')" style="width: 20%">
      <template #body="slotProps">
        <Select
          v-model="slotProps.data.supplierId"
          :options="slotProps.data.availableSuppliers"
          optionLabel="comercialName"
          optionValue="id"
          :placeholder="t('sales.components.seleccionaProveidor')"
          class="w-full"
          :size="'small'"
          @change="onSupplierChange(slotProps.data)"
        />
      </template>
    </Column>
    <Column field="quantity" :header="t('sales.components.quantitat')" style="width: 8%" />
    <Column field="weight" :header="t('sales.components.pesKg')" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.weight.toFixed(2) }}
      </template>
    </Column>
    <Column field="volume" :header="t('sales.components.volumMm')" style="width: 10%">
      <template #body="slotProps">
        {{ slotProps.data.volume.toFixed(2) }}
      </template>
    </Column>
    <Column field="unitPrice" :header="t('sales.components.preuUnit')" style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.unitPrice) }}
      </template>
    </Column>
    <Column field="totalPrice" :header="t('sales.components.total')" style="width: 8%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.totalPrice) }}
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
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

const { t } = useI18n();
const emit = defineEmits<{
  (e: "supplierChange", row: BudgetExternalServiceRow): void;
}>();

const onSupplierChange = (row: BudgetExternalServiceRow) => {
  emit("supplierChange", row);
};
</script>
