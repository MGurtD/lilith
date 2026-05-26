<template>
  <div>
    <DataTable
      class="p-datatable-sm small-datatable"
      dataKey="key"
      :value="workcenterShifts"
      :paginator="true"
      :rows="20"
      tableStyle="min-width: 100%"
      scrollable
      scrollHeight="flex"
      sortMode="multiple"
    >
      <template #header>
        <TableFilter
          :config="filterConfig"
          v-model="filter"
          :show-title="false"
          :show-action-labels="false"
          :show-create="false"
          :body-width="filterBodyWidth"
          embedded
          @filter="filterData"
          @clear="cleanFilter"
        >
          <template #prepend>
            <div
              class="table-filter-prepend-field table-filter-prepend-field--lg"
            >
              <label class="filter-label table-filter-prepend-label"
                >Període</label
              >
              <DatePicker
                v-model="filter.dates"
                selectionMode="range"
                dateFormat="dd/mm/yy"
                showIcon
                class="w-full"
                size="small"
              />
            </div>
          </template>
        </TableFilter>
      </template>
      <Column field="workcenter" header="Centre de treball" sortable> </Column>
      <Column field="operator" header="Operari" sortable> </Column>
      <Column field="machineStatus" header="Estat del centre"></Column>

      <Column field="startTime" header="Inici" sortable>
        <template #body="slotProps">
          {{ formatDateTimeUTCWithSeconds(slotProps.data.startTime) }}
        </template>
      </Column>
      <Column field="endTime" header="Fi" sortable>
        <template #body="slotProps">
          {{ formatDateTimeUTCWithSeconds(slotProps.data.endTime) }}
        </template>
      </Column>
      <Column field="quantityOk" header="Quantitat OK" />
      <Column field="quantityKo" header="Quantitat KO" />
      <Column field="plannedQuantity" header="Quantitat Prevista" />
      <Column field="operatorCost" header="Cost Operari">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.operatorCost) }}
        </template>
      </Column>
      <Column
        field="estimatedOperatorCost"
        header="Cost operari estimat (per OF)"
      >
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.estimatedOperatorCost) }}
        </template>
      </Column>
      <Column field="workcenterCost" header="Cost centre">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.workcenterCost) }}
        </template>
      </Column>
      <Column
        field="estimatedMachineCost"
        header="Cost centre estimat (per OF)"
      >
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.estimatedMachineCost) }}
        </template>
      </Column>
      <Column field="totalCost" header="Cost Tall">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.totalCost) }}
        </template>
      </Column>
      <Column field="totalHours" header="Hores">
        <template #body="slotProps">
          {{ slotProps.data.totalHours.toFixed(2) }}
        </template>
      </Column>
      <Column field="workOrderCode" header="Ordre de treball" sortable></Column>
      <Column field="workOrderPhaseCode" header="Fase" sortable></Column>
      <Column
        field="workOrderPhaseDescription"
        header="Descripcio fase"
      ></Column>
      <Column field="referenceCode" header="Referencia"></Column>
      <Column
        field="referenceDescription"
        header="Descripcio referencia"
      ></Column>
      <Column field="customerComercialName" header="Client"></Column>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useWorkcenterShiftStore } from "../store/workcentershift";
import type {
  WorkcenterShiftHistorical,
  WorkcenterShiftRequest,
} from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import {
  formatCurrency,
  formatDateTimeUTCWithSeconds,
} from "../../../utils/functions";
import { useStore } from "@/store";
import TableFilter, {
  type FilterConfig,
  type FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";

const store = useStore();
const toast = useToast();

const workcenterShifts = ref<WorkcenterShiftHistorical[]>([]);
const workcenterShiftStore = useWorkcenterShiftStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const groupByOptions = [
  { label: "Operari", value: "Operator" },
  { label: "Centre de treball", value: "Workcenter" },
  { label: "Ordre de treball", value: "Workorder" },
  { label: "Cap", value: "None" },
];

const timeGroupByOptions = [
  { label: "Dia", value: "Day" },
  { label: "Setmana", value: "Week" },
  { label: "Mes", value: "Month" },
  { label: "Any", value: "Year" },
  { label: "Cap", value: "None" },
];

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  groupBy: "None",
  timeGroupBy: "None",
});

const filterConfig: Array<FilterConfig> = [
  {
    key: "groupBy",
    label: "Grup",
    type: "select",
    options: groupByOptions,
    optionLabel: "label",
    optionValue: "value",
    size: "md",
    row: 0,
  },
  {
    key: "timeGroupBy",
    label: "Grup per temps",
    type: "select",
    options: timeGroupByOptions,
    optionLabel: "label",
    optionValue: "value",
    size: "md",
    row: 0,
  },
];

onMounted(async () => {
  store.setMenuItem({
    title: "Històric",
    icon: "pi pi-fw pi-clock",
    backButtonVisible: false,
  });
});

const filterData = async () => {
  if (filter.value.dates) {
    const startTime = filter.value.dates[0];
    const endTime = filter.value.dates[1];

    const request: WorkcenterShiftRequest = {
      startTime,
      endTime,
      groupBy: filter.value.groupBy,
      timeGroupBy: filter.value.timeGroupBy,
    };

    const response = await workcenterShiftStore.query(request);
    if (response) {
      workcenterShifts.value = response;
    }
  } else {
    toast.add({
      severity: "info",
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
      life: 5000,
    });
  }
};

const cleanFilter = () => {
  filter.value.dates = undefined;
  filter.value.groupBy = "None";
  filter.value.timeGroupBy = "None";
  workcenterShifts.value = [];
};
</script>
