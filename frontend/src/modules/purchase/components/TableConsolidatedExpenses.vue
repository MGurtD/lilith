<template>
  <DataTable :value="props.expenses" tableStyle="min-width: 100%">
    <Column field="type" header="Tipus de despesa" style="width: 10%"></Column>
    <Column field="typeDetail" header="Detall" style="width: 15%"></Column>
    <Column
      field="paymentDate"
      header="Data pagament"
      sortable
      style="width: 10%"
    >
      <template #body="slotProps">
        <span>{{ formatDate(slotProps.data.paymentDate) }}</span>
      </template>
    </Column>
    <Column field="amount" header="Import" style="width: 10%">
      <template #body="slotProps">
        <span>{{ slotProps.data.amount }} €</span>
      </template>
    </Column>
    <Column field="description" header="Descripció" style="width: 30%"></Column>
  </DataTable>
</template>
<script setup lang="ts">
import { ConsolidatedExpense } from "../types";
import { formatDate } from "../../../utils/functions";

const props = defineProps<{
  expenses: Array<ConsolidatedExpense>;
}>();
</script>
