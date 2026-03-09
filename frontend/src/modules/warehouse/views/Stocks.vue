<template>
  <DataTable
    class="small-datatable"
    :value="filteredStocks"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    :sortOrder="1"
    :paginator="filteredStocks && filteredStocks.length > 20"
    :rows="20"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
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
    <Column
      header="Referència"
      field="referenceId"
      :sortable="true"
      style="width: 28%"
    >
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
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { useStore } from "../../../store";
import { useStockStore } from "../store/stock";
import { useReferenceStore } from "../../shared/store/reference";
import { useWarehouseStore } from "../store/warehouse";

import { onMounted, ref, computed } from "vue";
import { PrimeIcons } from "@primevue/core/api";

const store = useStore();

const stockStore = useStockStore();
const referenceStore = useReferenceStore();
const warehouseStore = useWarehouseStore();

const filter = ref({
  referenceId: undefined as string | undefined,
  warehouseId: undefined as string | undefined,
});

const filteredStocks = computed(() => {
  if (!stockStore.stocks) return [];

  let result = [...stockStore.stocks];

  // Filter by reference
  if (filter.value.referenceId) {
    result = result.filter((s) => s.referenceId === filter.value.referenceId);
  }

  // Filter by warehouse (need to check stock location's warehouse)
  if (filter.value.warehouseId) {
    const warehouseLocations =
      warehouseStore.warehouses
        ?.find((w) => w.id === filter.value.warehouseId)
        ?.locations?.map((l) => l.id) || [];

    result = result.filter((s) =>
      warehouseLocations.includes(s.locationId)
    );
  }

  // Sort by reference
  result.sort((a, b) => {
    const refA = referenceStore.getFullNameById(a.referenceId).toLowerCase();
    const refB = referenceStore.getFullNameById(b.referenceId).toLowerCase();
    return refA.localeCompare(refB);
  });

  return result;
});

const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.warehouseId = undefined;
};

onMounted(async () => {
  await stockStore.fetchStocks();
  await referenceStore.fetchReferences();
  await warehouseStore.fetchWarehousesWithLocations();
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    title: "Gestió de magatzems - Estocs",
  });
});
</script>
