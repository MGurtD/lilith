<template>
  <DataTable
    :value="props.costs"
    tableStyle="min-width: 100%"
    paginator
    :rows="20"
    class="p-datatable-sm"
    sortMode="multiple"
    stripedRows
    :rowHover="true"
  >
    <Column field="year" header="Any"></Column>
    <Column field="month" header="Mes" sortable></Column>
    <Column field="workcenterName" header="Centre de treball" sortable></Column>
    <Column
      field="workcenterTypeName"
      header="Tipus de centre"
      sortable
    ></Column>
    <Column field="operatorName" header="Operari" sortable></Column>
    <Column field="totalTime" header="Temps mensual">
      <template #body="slotProps">
        <span>{{ formatToTwoDecimals(slotProps.data.totalTime) }} hores</span>
      </template>
    </Column>
    <Column field="totalCost" header="Cost mensual">
      <template #body="slotProps">
        <span>{{ slotProps.data.totalCost }} €</span>
      </template></Column
    >
  </DataTable>
</template>
<script setup lang="ts">
import { ProductionCostDashboardGrouped } from "../types";
const props = defineProps<{
  costs: Array<ProductionCostDashboardGrouped>;
}>();
const formatToTwoDecimals = (value: number): string => {
  return value.toFixed(2);
};
</script>
