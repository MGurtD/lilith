<template>
  <div>
    <Table
      :items="groupedSaturation"
      :columns="columns"
      :filter-config="filterConfig"
      v-model:filter-values="filter"
      :filter-body-width="filterBodyWidth"
      :show-create="false"
      :paginator="groupedSaturation && groupedSaturation.length > 10"
      :rows="20"
      :rowsPerPageOptions="[10, 25, 50]"
      stripedRows
      class="p-datatable-sm"
      tableStyle="min-width: 100%"
      sortField="totalEstimatedTime"
      :sortOrder="-1"
      @filter="filterData"
      @clear="cleanFilter"
    >
      <template #action-prepend>
        <div
          v-if="workingDaysInfo"
          class="flex align-items-center gap-2 text-700 mr-2"
        >
          <i :class="PrimeIcons.CALENDAR" class="text-primary"></i>
          <span class="font-semibold">{{ workingDaysInfo }}</span>
        </div>
      </template>
      <template #body-workcenterTypeName="{ data }">
        {{ data.workcenterTypeName }}
        <span class="text-500 ml-2">({{ data.workcenterCount }} centres)</span>
      </template>
      <template #body-detailAction="{ data }">
        <Button
          :icon="PrimeIcons.SEARCH"
          rounded
          outlined
          severity="info"
          @click.stop="showDetail(data)"
          :label="pt('Veure detall')"
          size="small"
        />
      </template>
    </Table>

    <!-- Dialog de detall -->
    <Dialog
      v-model:visible="detailDialogVisible"
      :header="`Detall de ${selectedWorkcenterTypeName}`"
      :style="{ width: '80vw' }"
      :modal="true"
    >
      <Table
        :items="selectedDetails"
        :columns="detailColumns"
        :show-filters="false"
        sortMode="multiple"
        :multi-sort-meta="[
          { field: 'workOrderPriority', order: 1 },
          { field: 'workOrderPlannedDate', order: 1 },
        ]"
        stripedRows
        class="p-datatable-sm"
        :paginator="selectedDetails && selectedDetails.length > 10"
        :rows="20"
      />
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "@/components/tables/TableFilter.vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import { usePlantModelStore } from "../store/plantmodel";
import { useExerciseStore } from "../../shared/store/exercise";
import { useStore } from "../../../store";
import { storeToRefs } from "pinia";
import { ref, computed, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { formatDateForQueryParameter } from "../../../utils/functions";
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

const filterBodyWidth: FilterBodyWidth = { desktop: "30%", tablet: "50%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
});

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: pt("Període"),
    type: "date-range",
    placeholder: pt("Selecciona un període vàlid"),
    size: "md",
  },
]);

const columns = computed<Column[]>(() => [
  {
    field: "workcenterTypeName",
    header: pt("Tipus Centre Treball"),
    sortable: true,
    style: "width: 40%",
  },
  {
    field: "totalEstimatedTime",
    header: pt("Temps Total Estimat"),
    sortable: true,
    resolver: (value) => (typeof value === "number" ? formatTime(value) : ""),
    style: "width: 30%",
  },
  {
    field: "detailAction",
    header: pt("Detall"),
    style: "width: 30%",
    truncate: false,
  },
]);

const detailColumns = computed<Column[]>(() => [
  {
    field: "workOrderCode",
    header: pt("Ordre Treball"),
    sortable: true,
    style: "width: 12%",
  },
  {
    field: "workOrderPriority",
    header: pt("Prioritat"),
    sortable: true,
    columnType: ColumnType.Number,
    style: "width: 8%",
  },
  {
    field: "workOrderPlannedDate",
    header: pt("Data Plan."),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 12%",
  },
  {
    field: "phaseCode",
    header: pt("Codi Fase"),
    sortable: true,
    style: "width: 8%",
  },
  {
    field: "phaseDescription",
    header: pt("Descripció Fase"),
    style: "width: 25%",
  },
  {
    field: "plannedQuantity",
    header: pt("Quantitat"),
    sortable: true,
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "estimatedTime",
    header: pt("Temps Estimat"),
    sortable: true,
    resolver: (value) => (typeof value === "number" ? formatTime(value) : ""),
    style: "width: 15%",
  },
]);

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
  if (!filter.value.dates?.[0] || !filter.value.dates[1]) {
    return null;
  }

  const startDate = filter.value.dates[0];
  const endDate = filter.value.dates[1];
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
  if (!filter.value.dates?.[0] || !filter.value.dates[1]) {
    toast.add({
      severity: "info",
      summary: pt("Filtre invàlid"),
      detail: pt("Selecciona un període vàlid"),
      life: 3000,
    });
    return;
  }

  const startDate = formatDateForQueryParameter(filter.value.dates[0]);
  const endDate = formatDateForQueryParameter(filter.value.dates[1]);

  await plantModelStore.fetchWorkcenterTypeSaturation(startDate, endDate);
};

// Clean filter and reset to default
const cleanFilter = () => {
  filter.value.dates = undefined;
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
    filter.value.dates = [
      new Date(currentExercise.startDate),
      new Date(currentExercise.endDate),
    ];
  }
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CHART_BAR,
    title: pt("Saturació Centres de Treball"),
  });

  // Load necessary data
  if (!exerciseStore.exercises?.length) await exerciseStore.fetchActive();
  if (!plantModelStore.workcenterTypes)
    await plantModelStore.fetchWorkcenterTypes();
  if (!plantModelStore.workcenters) await plantModelStore.fetchWorkcenters();

  // Set default date range and fetch data
  setCurrentYear();
  if (filter.value.dates?.[0] && filter.value.dates[1]) {
    await filterData();
  }
});
</script>
