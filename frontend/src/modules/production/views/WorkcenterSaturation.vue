<template>
  <div>
    <DataTable
      :value="groupedSaturation"
      :paginator="groupedSaturation && groupedSaturation.length > 10"
      :rows="20"
      :rowsPerPageOptions="[10, 25, 50]"
      stripedRows
      class="p-datatable-sm"
      tableStyle="min-width: 100%"
      sortField="totalEstimatedTime"
      :sortOrder="-1"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <div class="datatable-filter">
            <div class="filter-field">
              <ExerciseDatePicker
                :exercises="exerciseStore.exercises"
                @range-selected="filterData"
              />
            </div>
          </div>
          <div
            v-if="workingDaysInfo"
            class="flex align-items-center gap-2 text-700"
          >
            <i :class="PrimeIcons.CALENDAR" class="text-primary"></i>
            <span class="font-semibold">{{ workingDaysInfo }}</span>
          </div>
          <div class="datatable-buttons">
            <Button
              class="datatable-button mr-2"
              :icon="PrimeIcons.FILTER"
              rounded
              raised
              @click="filterData"
            />
            <Button
              class="datatable-button mr-2"
              :icon="PrimeIcons.FILTER_SLASH"
              rounded
              raised
              @click="cleanFilter"
            />
          </div>
        </div>
      </template>
      <Column
        field="workcenterTypeName"
        header="Tipus Centre Treball"
        sortable
        style="width: 40%"
      >
        <template #body="slotProps">
          {{ slotProps.data.workcenterTypeName }}
          <span class="text-500 ml-2"
            >({{ slotProps.data.workcenterCount }} centres)</span
          >
        </template>
      </Column>
      <Column
        field="totalEstimatedTime"
        header="Temps Total Estimat"
        sortable
        style="width: 30%"
      >
        <template #body="slotProps">
          {{ formatTime(slotProps.data.totalEstimatedTime) }}
        </template>
      </Column>
      <Column header="Detall" style="width: 30%">
        <template #body="slotProps">
          <Button
            :icon="PrimeIcons.SEARCH"
            rounded
            outlined
            severity="info"
            @click="showDetail(slotProps.data)"
            label="Veure detall"
            size="small"
          />
        </template>
      </Column>
    </DataTable>

    <!-- Dialog de detall -->
    <Dialog
      v-model:visible="detailDialogVisible"
      :header="`Detall de ${selectedWorkcenterTypeName}`"
      :style="{ width: '80vw' }"
      :modal="true"
    >
      <DataTable
        :value="selectedDetails"
        sortMode="multiple"
        :multiSortMeta="[
          { field: 'workOrderPriority', order: 1 },
          { field: 'workOrderPlannedDate', order: 1 },
        ]"
        stripedRows
        class="p-datatable-sm"
        :paginator="selectedDetails && selectedDetails.length > 10"
        :rows="20"
      >
        <Column
          field="workOrderCode"
          header="Ordre Treball"
          sortable
          style="width: 12%"
        ></Column>
        <Column
          field="workOrderPriority"
          header="Prioritat"
          sortable
          style="width: 8%"
        ></Column>
        <Column
          field="workOrderPlannedDate"
          header="Data Plan."
          sortable
          style="width: 12%"
        >
          <template #body="slotProps">
            {{
              new Date(slotProps.data.workOrderPlannedDate).toLocaleDateString(
                "ca-ES",
              )
            }}
          </template>
        </Column>
        <Column
          field="phaseCode"
          header="Codi Fase"
          sortable
          style="width: 8%"
        ></Column>
        <Column
          field="phaseDescription"
          header="Descripció Fase"
          style="width: 25%"
        ></Column>
        <Column
          field="plannedQuantity"
          header="Quantitat"
          sortable
          style="width: 10%"
        ></Column>
        <Column
          field="estimatedTime"
          header="Temps Estimat"
          sortable
          style="width: 15%"
        >
          <template #body="slotProps">
            {{ formatTime(slotProps.data.estimatedTime) }}
          </template>
        </Column>
      </DataTable>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { usePlantModelStore } from "../store/plantmodel";
import { useExerciseStore } from "../../shared/store/exercise";
import { useStore } from "../../../store";
import { storeToRefs } from "pinia";
import { ref, computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { formatDateForQueryParameter } from "../../../utils/functions";
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
import type { WorkcenterTypeSaturation } from "../types";

const plantModelStore = usePlantModelStore();
const exerciseStore = useExerciseStore();
const store = useStore();
const toast = useToast();

const { workcenterTypeSaturation } = storeToRefs(plantModelStore);

// Dialog state
const detailDialogVisible = ref(false);
const selectedDetails = ref<WorkcenterTypeSaturation[]>([]);
const selectedWorkcenterTypeName = ref("");

// Computed property to group data by workcenterTypeId
const groupedSaturation = computed(() => {
  if (
    !workcenterTypeSaturation.value ||
    workcenterTypeSaturation.value.length === 0
  ) {
    return [];
  }

  const grouped = new Map<
    string,
    {
      workcenterTypeId: string;
      workcenterTypeName: string;
      workcenterCount: number;
      totalEstimatedTime: number;
      details: WorkcenterTypeSaturation[];
    }
  >();

  workcenterTypeSaturation.value.forEach((item) => {
    if (!grouped.has(item.workcenterTypeId)) {
      const workcenterType = plantModelStore.workcenterTypes?.find(
        (wt) => wt.id === item.workcenterTypeId,
      );
      const workcenterCount =
        plantModelStore.getWorkcentersByTypeId(item.workcenterTypeId)?.length ||
        0;

      grouped.set(item.workcenterTypeId, {
        workcenterTypeId: item.workcenterTypeId,
        workcenterTypeName: workcenterType?.name || "Desconegut",
        workcenterCount: workcenterCount,
        totalEstimatedTime: 0,
        details: [],
      });
    }

    const group = grouped.get(item.workcenterTypeId)!;
    group.totalEstimatedTime += item.estimatedTime;
    group.details.push(item);
  });

  return Array.from(grouped.values());
});

// Calculate working days (excluding weekends)
const calculateWorkingDays = (startDate: Date, endDate: Date): number => {
  let count = 0;
  const current = new Date(startDate);

  while (current <= endDate) {
    const dayOfWeek = current.getDay();
    // 0 = Sunday, 6 = Saturday
    if (dayOfWeek !== 0 && dayOfWeek !== 6) {
      count++;
    }
    current.setDate(current.getDate() + 1);
  }

  return count;
};

// Computed property for working days information
const workingDaysInfo = computed(() => {
  if (!store.exercisePicker.dates || store.exercisePicker.dates.length !== 2) {
    return null;
  }

  const startDate = store.exercisePicker.dates[0];
  const endDate = store.exercisePicker.dates[1];
  const workingDays = calculateWorkingDays(startDate, endDate);

  const hoursPerDay = 8; // Hores per torn
  const hours1Shift = workingDays * hoursPerDay;
  const hours2Shifts = workingDays * hoursPerDay * 2;
  const hours3Shifts = workingDays * hoursPerDay * 3;

  return `${workingDays} dies - ${hours1Shift}h a 1 torn - ${hours2Shifts}h a 2 torns - ${hours3Shifts}h a 3 torns`;
});

// Format time from minutes to readable format
const formatTime = (minutes: number): string => {
  const hours = Math.floor(minutes / 60);
  const mins = Math.round(minutes % 60);

  if (hours === 0) return `${mins}m`;
  if (mins === 0) return `${hours}h`;
  return `${hours}h ${mins}m`;
};

// Filter data based on selected date range
const filterData = async () => {
  if (!store.exercisePicker.dates || store.exercisePicker.dates.length !== 2) {
    toast.add({
      severity: "info",
      summary: "Filtre invàlid",
      detail: "Selecciona un període vàlid",
      life: 3000,
    });
    return;
  }

  const startDate = formatDateForQueryParameter(store.exercisePicker.dates[0]);
  const endDate = formatDateForQueryParameter(store.exercisePicker.dates[1]);

  await plantModelStore.fetchWorkcenterTypeSaturation(startDate, endDate);
};

// Clean filter and reset to default
const cleanFilter = () => {
  store.cleanExercisePicker();
  plantModelStore.workcenterTypeSaturation = undefined;
};

// Show detail dialog
const showDetail = (data: {
  workcenterTypeName: string;
  details: WorkcenterTypeSaturation[];
}) => {
  selectedWorkcenterTypeName.value = data.workcenterTypeName;
  selectedDetails.value = data.details;
  detailDialogVisible.value = true;
};

// Set current year as default
const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = exerciseStore.exercises?.find((e) => e.name === year);

  if (currentExercise) {
    store.exercisePicker.exercise = currentExercise;
    store.exercisePicker.dates = [
      new Date(store.exercisePicker.exercise.startDate),
      new Date(store.exercisePicker.exercise.endDate),
    ];
  }
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CHART_BAR,
    title: "Saturació Centres de Treball",
  });

  // Load necessary data
  if (!exerciseStore.exercises?.length) await exerciseStore.fetchActive();
  if (!plantModelStore.workcenterTypes)
    await plantModelStore.fetchWorkcenterTypes();
  if (!plantModelStore.workcenters) await plantModelStore.fetchWorkcenters();

  // Set default date range and fetch data
  setCurrentYear();
  if (store.exercisePicker.dates && store.exercisePicker.dates.length === 2) {
    await filterData();
  }
});
</script>
