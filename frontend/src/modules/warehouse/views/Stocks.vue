<template>
  <DataTable
    class="p-datatable-sm"
    :value="stockStore.stocks"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    paginator
    :rows="20"
    :rowsPerPageOptions="[20, 50]"
    paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
    currentPageReportTemplate="{first} a {last} de {totalRecords} entrades"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter flex flex-wrap gap-4 flex-1">
          <div class="filter-field flex gap-2">
            <label>Magatzem</label>
            <DropdownWarehouses label="" v-model="filter.warehouseId" />
          </div>
        </div>
      </div>
    </template>
    <Column header="Producte" style="width: 28%">
      <template #body="slotProps">
        {{ referenceStore.getFullNameById(slotProps.data.referenceId) }}
      </template>
    </Column>
    <Column field="quantity" header="Uds." style="width: 12%"></Column>
    <Column field="width" header="Ample (x) mm" style="width: 12%"></Column>
    <Column field="length" header="Llarg (y) mm" style="width: 12%"></Column>
    <Column field="height" header="Alt (z) mm" style="width: 12%"></Column>
    <Column field="diameter" header="Diàmetre mm" style="width: 12%"></Column>
    <Column field="thickness" header="Gruix mm" style="width: 12%"></Column>
  </DataTable>
</template>
<script setup lang="ts">
import DropdownWarehouses from "../components/DropdownWarehouses.vue";
import { useStore } from "../../../store";
import { useStockStore } from "../store/stock";
import { useReferenceStore } from "../../shared/store/reference";

import { onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";

const store = useStore();

const stockStore = useStockStore();
const referenceStore = useReferenceStore();

const filter = ref({
  warehouseId: undefined as string | undefined,
});

onMounted(async () => {
  await stockStore.fetchStocks();
  await referenceStore.fetchReferences();
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: "Gestió de magatzems - Estocs",
  });
});
</script>
