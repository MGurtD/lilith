<template>
  <DataTable :value="props.expenses" tableStyle="min-width: 100%">
    <Column field="type" :header="t('purchase.dashboard.expenseType')" style="width: 10%"></Column>
    <Column field="typeDetail" :header="t('purchase.dashboard.detail')" style="width: 15%"></Column>
    <Column
      field="paymentDate"
      :header="t('purchase.fields.paymentDate')"
      sortable
      style="width: 10%"
    >
      <template #body="slotProps">
        <span>{{ formatDate(slotProps.data.paymentDate) }}</span>
      </template>
    </Column>
    <Column field="amount" :header="t('purchase.fields.amount')" style="width: 10%">
      <template #body="slotProps">
        <span>{{ slotProps.data.amount }} €</span>
      </template>
    </Column>
    <Column field="description" :header="t('purchase.fields.description')" style="width: 30%"></Column>
  </DataTable>
</template>
<script setup lang="ts">
import { ConsolidatedExpense } from "../types";
import { formatDate } from "../../../utils/functions";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  expenses: Array<ConsolidatedExpense>;
}>();

const { t } = useI18n();
</script>
