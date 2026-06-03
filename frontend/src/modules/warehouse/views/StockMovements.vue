<template>
  <div>
    <DataTable
      class="p-datatable-sm"
      tableStyle="min-width:100%"
      scrollable
      scrollHeight="flex"
      sortField="movementDate"
      :sortOrder="1"
      :value="stockMovementStore.stockMovements"
      :paginator="(stockMovementStore.stockMovements?.length ?? 0) > 20"
      :rows="20"
    >
      <template #header>
        <TableFilter
          :config="[]"
          v-model="filter"
          :show-title="false"
          :show-action-labels="false"
          :show-create="false"
          :body-width="filterBodyWidth"
          embedded
          @filter="filterMovements"
          @clear="cleanFilter"
        >
          <template #prepend>
            <div class="table-filter-prepend-field table-filter-prepend-field--lg">
              <label class="filter-label table-filter-prepend-label">Període</label>
              <DatePicker
                v-model="filter.dates"
                selectionMode="range"
                dateFormat="dd/mm/yy"
                showIcon
                class="w-full"
                size="small"
                placeholder="Selecciona període"
              />
            </div>
            <div class="table-filter-prepend-field table-filter-prepend-field--md">
              <label class="filter-label table-filter-prepend-label">Ubicació</label>
              <DropdownWarehousesWithLocations
                label=""
                v-model="filter.locationId"
              />
            </div>
          </template>
        </TableFilter>
      </template>
      <Column header="Data" field="movementDate" sortable style="width: 10%">
        <template #body="slotProps">
          {{ formatDateTime(slotProps.data.movementDate) }}
        </template>
      </Column>
      <Column header="Referència" style="width: 15%">
        <template #body="slotProps">
          {{ referenceStore.getFullNameById(slotProps.data.referenceId) }}
        </template></Column
      >
      <Column header="Ubicació" style="width: 10%">
        <template #body="slotProps">
          {{ slotProps.data.location?.name }}
        </template>
      </Column>
      <Column field="width" header="Ample (x) mm" style="width: 5%"></Column>
      <Column field="length" header="Llarg (y) mm" style="width: 5%"></Column>
      <Column field="height" header="Alt (z) mm" style="width: 5%"></Column>
      <Column field="diameter" header="Diámetre mm" style="width: 5%"></Column>
      <Column field="thickness" header="Gruix mm" style="width: 5%"></Column>
      <Column
        field="description"
        header="Descripció"
        style="width: 25%"
      ></Column>
      <Column
        header="Tipus de moviment"
        field="movementType"
        style="width: 10%"
      >
        <template #body="slotProps">
          <TagMovementType :movementType="slotProps.data.movementType" />
        </template>
      </Column>
      <Column field="quantity" header="Quantitat" style="width: 10%"></Column>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import TableFilter, {
  type FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";
import DropdownWarehousesWithLocations from "../components/DropdownWarehousesWithLocations.vue";
import TagMovementType from "../../../components/TagMovementType.vue";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useStockMovementStore } from "../store/stockMovement";
import { useReferenceStore } from "../../shared/store/reference";
import { useExerciseStore } from "../../shared/store/exercise";
import { onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import {
  formatDateForQueryParameter,
  formatDateTime,
} from "../../../utils/functions";

const toast = useToast();
const store = useStore();
const stockMovementStore = useStockMovementStore();
const referenceStore = useReferenceStore();
const exerciseStore = useExerciseStore();

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  locationId: undefined as string | undefined,
});

const filterBodyWidth: FilterBodyWidth = {
  desktop: "55%",
  tablet: "70%",
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MAP,
    title: "Moviments de magatzem",
  });
  await exerciseStore.fetchAll();
  await referenceStore.fetchReferences();
  setCurrentYear();
});

const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = exerciseStore.exercises?.find((e) => e.name === year);

  if (currentExercise) {
    filter.value.dates = [
      new Date(currentExercise.startDate),
      new Date(currentExercise.endDate),
    ];
  }
};

const cleanFilter = () => {
  filter.value.dates = undefined;
  filter.value.locationId = undefined;
};

const filterMovements = async () => {
  if (filter.value.dates && filter.value.dates[1]) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await stockMovementStore.getBetweenDates(
      startTime,
      endTime,
      filter.value.locationId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
      life: 5000,
    });
  }
};
</script>
