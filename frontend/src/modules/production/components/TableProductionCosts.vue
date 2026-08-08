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
    <Column field="year" :header="t('production.components.any')"></Column>
    <Column field="month" :header="t('production.components.mes')" sortable></Column>
    <Column field="workcenterName" :header="t('production.components.centreDeTreball')" sortable></Column>
    <Column
      field="workcenterTypeName"
      :header="t('production.components.tipusDeCentre')"
      sortable
    ></Column>
    <Column field="operatorName" :header="t('production.components.operari')" sortable></Column>
    <Column field="totalTime" :header="t('production.components.tempsMensual')">
      <template #body="slotProps">
        <span>{{ formatToTwoDecimals(slotProps.data.totalTime) }} hores</span>
      </template>
    </Column>
    <Column field="totalCost" :header="t('production.components.costMensual')">
      <template #body="slotProps">
        <span>{{ slotProps.data.totalCost }} €</span>
      </template></Column
    >
  </DataTable>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { ProductionCostDashboardGrouped } from "../types";
const props = defineProps<{
  costs: Array<ProductionCostDashboardGrouped>;
}>();
const formatToTwoDecimals = (value: number): string => {
  return value.toFixed(2);
};
</script>
