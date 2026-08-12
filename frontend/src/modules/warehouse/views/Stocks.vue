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
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :show-filter-action="false"
        :show-create="false"
        :body-width="filterBodyWidth"
        embedded
        @clear="cleanFilter"
      >
        <template #prepend>
          <div class="table-filter-prepend-field table-filter-prepend-field--md">
            <label class="filter-label table-filter-prepend-label">Magatzem</label>
            <DropdownWarehouses label="" v-model="filter.warehouseId" />
          </div>
          <div class="table-filter-prepend-field table-filter-prepend-field--md">
            <label class="filter-label table-filter-prepend-label">Referència</label>
            <DropdownReference
              label=""
              :fullName="true"
              :options="stockStore.availableReferences"
              v-model="filter.referenceId"
            />
          </div>
        </template>
      </TableFilter>
    </template>
    <Column field="referenceDisplay" header="Referència" :sortable="true" style="width: 24%" />
    <Column header="Lot" style="width: 12%">
      <template #body="slotProps">
        <span class="flex align-items-center gap-2">
          {{ slotProps.data.lotCode || "—" }}
          <Tag
            v-if="slotProps.data.lotClosedDate"
            severity="secondary"
            value="Tancat"
            v-tooltip.top="'Lot tancat'"
          />
        </span>
      </template>
    </Column>
    <Column field="warehouseName" header="Magatzem" style="width: 14%" />
    <Column field="locationName" header="Ubicació" style="width: 14%" />
    <Column field="quantity" header="Uds." style="width: 10%" />
    <Column field="width" header="Ample (x) mm" style="width: 10%" />
    <Column field="length" header="Llarg (y) mm" style="width: 10%" />
    <Column field="height" header="Alt (z) mm" style="width: 10%" />
    <Column field="diameter" header="Diàmetre mm" style="width: 10%" />
    <Column field="thickness" header="Gruix mm" style="width: 10%" />
    <Column header="" style="width: 6%">
      <template #body="slotProps">
        <Button
          icon="pi pi-sitemap"
          text
          rounded
          size="small"
          :disabled="!slotProps.data.lotId"
          v-tooltip.top="'Veure traçabilitat del lot'"
          @click="goToLotTraceability(slotProps.data.referenceId, slotProps.data.lotId)"
        />
      </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import TableFilter, {
  type FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";
import DropdownWarehouses from "../components/DropdownWarehouses.vue";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { useStore } from "../../../store";
import { useStockStore } from "../store/stock";
import { useWarehouseStore } from "../store/warehouse";

const store = useStore();
const router = useRouter();
const stockStore = useStockStore();
const warehouseStore = useWarehouseStore();

const filter = ref({
  referenceId: undefined as string | undefined,
  warehouseId: undefined as string | undefined,
});

const filterBodyWidth: FilterBodyWidth = {
  desktop: "55%",
  tablet: "70%",
};

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

const goToLotTraceability = (referenceId: string, lotId?: string | null) => {
  if (!lotId) return;
  router.push({
    path: "/lot-traceability",
    query: { referenceId, lotId },
  });
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
