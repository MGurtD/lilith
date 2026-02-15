<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :value="filteredOrders"
    selectionMode="multiple"
    v-model:selection="selectedDetails"
    :meta-key-selection="false"
    rowGroupMode="subheader"
    groupRowsBy="salesOrderNumber"
    sortMode="single"
    sortField="salesOrderNumber"
  >
    <template #header>
      <header class="selector-filter">
        <div class="selector-filter-field">
          <label for="">Comanda</label> &nbsp;
          <InputText
            style="width: 150px; height: 35px"
            v-model="selectedOrder"
            size="small"
          />
        </div>
        <div class="selector-filter-button">
          <Button
            @click="onSelectedClick"
            :size="'small'"
            :icon="PrimeIcons.CHECK_SQUARE"
          ></Button>
        </div>
      </header>
    </template>

    <template #groupheader="slotProps">
      <div class="flex align-items-center gap-2">
        <b
          >Comanda {{ slotProps.data.salesOrderNumber }} -
          {{ formatDate(slotProps.data.salesOrderDate) }}</b
        >
      </div>
    </template>

    <Column header="Quantitat" field="quantity" style="width: 10%"></Column>
    <Column header="Descripció" field="description" style="width: 50%"></Column>
    <Column header="Preu" field="amount" style="width: 10%">
      <template #body="slotProps"> {{ slotProps.data.amount }} € </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { computed, ref } from "vue";
import { InvoiceableOrderDetail, SalesOrderDetail } from "../types";
import { formatDate } from "../../../utils/functions";
import _ from "lodash";
import { PrimeIcons } from "@primevue/core/api";

const selectedDetails = ref([] as Array<InvoiceableOrderDetail>);

const props = defineProps<{
  details: Array<InvoiceableOrderDetail> | undefined;
  headerVisible?: boolean;
}>();
const emits = defineEmits<{
  (e: "selected", orderDetails: Array<SalesOrderDetail>): void;
}>();

const selectedOrder = ref("");
const filteredOrders = computed(() => {
  if (selectedOrder.value.length === 0) return props.details;
  return props.details?.filter((d) =>
    d.salesOrderNumber.toString().includes(selectedOrder.value),
  );
});

const onSelectedClick = () => {
  if (selectedDetails.value.length === 0) return;

  const salesOrderDetails = selectedDetails.value.map((sd) => {
    return {
      salesOrderHeaderId: sd.salesOrderId,
      description: sd.description,
      isInvoiced: true,
      isDelivered: sd.isServed,
      quantity: sd.quantity,
      referenceId: sd.referenceId,
      id: sd.id,
      unitCost: sd.unitCost,
      unitPrice: sd.unitPrice,
      totalCost: sd.totalCost,
      amount: sd.amount,
    } as SalesOrderDetail;
  });
  emits("selected", salesOrderDetails);
};
</script>
<style scoped>
.selector-filter {
  display: grid;
  grid-template-columns: 1fr 0.1fr;
}
</style>
