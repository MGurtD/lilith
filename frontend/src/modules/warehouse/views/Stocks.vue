<template>
  <DataTable
    class="small-datatable"
    :value="filteredStocks"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    :sortOrder="1"
    :paginator="filteredStocks.length > 20"
    :rows="20"
  >
    <template #header>
      <div class="flex flex-wrap align-items-center justify-content-between gap-2">
        <div class="datatable-filter">
          <div class="filter-field">
            <label class="block text-900 mb-2">Magatzem</label>
            <DropdownWarehouses label="" v-model="filter.warehouseId" />
          </div>
          <div class="filter-field">
            <label class="block text-900 mb-2">Referència</label>
            <DropdownReference
              label=""
              :fullName="true"
              :options="stockStore.availableReferences"
              v-model="filter.referenceId"
            />
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button"
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
        </div>
      </div>
    </template>
    <Column field="referenceDisplay" header="Referència" :sortable="true" style="width: 28%" />
    <Column field="warehouseName" header="Magatzem" style="width: 16%" />
    <Column field="locationName" header="Ubicació" style="width: 16%" />
    <Column field="quantity" header="Uds." style="width: 12%" />
    <Column field="width" header="Ample (x) mm" style="width: 12%" />
    <Column field="length" header="Llarg (y) mm" style="width: 12%" />
    <Column field="height" header="Alt (z) mm" style="width: 12%" />
    <Column field="diameter" header="Diàmetre mm" style="width: 12%" />
    <Column field="thickness" header="Gruix mm" style="width: 12%" />
  </DataTable>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import DropdownWarehouses from "../components/DropdownWarehouses.vue";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { useStore } from "../../../store";
import { useStockStore } from "../store/stock";
import { useWarehouseStore } from "../store/warehouse";

const store = useStore();
const stockStore = useStockStore();
const warehouseStore = useWarehouseStore();

const filter = ref({
  referenceId: undefined as string | undefined,
  warehouseId: undefined as string | undefined,
});

const filteredStocks = computed(() => {
  if (!stockStore.stocks) return [];

  let result = [...stockStore.stocks];

  if (filter.value.referenceId) {
    result = result.filter((stock) => stock.referenceId === filter.value.referenceId);
  }

  if (filter.value.warehouseId) {
    result = result.filter((stock) => stock.warehouseId === filter.value.warehouseId);
  }

  return result.sort((left, right) =>
    left.referenceDisplay.localeCompare(right.referenceDisplay),
  );
});

const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.warehouseId = undefined;
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: "Gestió de magatzems - Estocs",
  });

  await stockStore.fetchStocks();
  await warehouseStore.fetchWarehousesWithLocations();
});
</script>
