<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :metaKeySelection="false"
    :value="filteredOrders"
    selectionMode="multiple"
    v-model:selection="selectedOrders"
  >
    <template #header>
      <header class="selector-filter">
        <div class="selector-filter-field">
          <label for="">Buscar</label> &nbsp;
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
          >Comanda {{ slotProps.data.number }} -
          {{ formatDate(slotProps.data.date) }}</b
        >
      </div>
    </template>

    <Column selectionMode="multiple" style="width: 3%"></Column>
    <Column header="Número" field="number" style="width: 10%"></Column>
    <Column
      header="Número client"
      field="customerNumber"
      style="width: 20%"
    ></Column>

    <Column header="Data" field="date" style="width: 10%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { computed, ref } from "vue";
import { SalesOrderHeader } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { formatDate } from "../../../utils/functions";

const selectedOrders = ref([] as Array<SalesOrderHeader>);

const props = defineProps<{
  orders: Array<SalesOrderHeader> | undefined;
  headerVisible?: boolean;
}>();
const emits = defineEmits<{
  (e: "selected", orderDetails: Array<SalesOrderHeader>): void;
}>();

const selectedOrder = ref("");
const filteredOrders = computed(() => {
  if (selectedOrder.value === "") return props.orders || [];

  var filtered = [] as Array<SalesOrderHeader>;

  if (props.orders) {
    filtered = props.orders.filter(
      (o) =>
        o.number.includes(selectedOrder.value) ||
        o.customerNumber.includes(selectedOrder.value),
    );
  }

  return filtered;
});

const onSelectedClick = () => {
  if (selectedOrders.value.length === 0) return;

  const salesOrderDetails = selectedOrders.value;
  emits("selected", salesOrderDetails);
};
</script>
<style scoped>
.selector-filter {
  display: grid;
  grid-template-columns: 1fr 0.1fr;
}
</style>
