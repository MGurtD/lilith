<template>
  <div>
    <Table
      class="p-datatable-sm small-datatable"
      dataKey="key"
      :items="workcenterShifts"
      :columns="columns"
      :filter-config="filterConfig"
      v-model:filter-values="filter"
      :filter-body-width="filterBodyWidth"
      :show-create="false"
      page="WorkcenterShift"
      :paginator="true"
      :rows="25"
      tableStyle="min-width: 100%"
      scrollable
      scrollHeight="flex"
      sortMode="multiple"
      @filter="filterData"
      @clear="cleanFilter"
    />
  </div>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import { computed, ref, onMounted } from "vue";
import { useWorkcenterShiftStore } from "../store/workcentershift";
import type {
  WorkcenterShiftHistorical,
  WorkcenterShiftRequest,
} from "../types";
import { useToast } from "primevue/usetoast";
import { formatDateTimeUTCWithSeconds } from "../../../utils/functions";
import { useStore } from "@/store";
import type {
  FilterConfig,
  FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";

const store = useStore();
const toast = useToast();

const workcenterShifts = ref<WorkcenterShiftHistorical[]>([]);
const workcenterShiftStore = useWorkcenterShiftStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const columns = computed<Column[]>(() => [
  { field: "workcenter", header: pt("Centre de treball"), sortable: true },
  { field: "operator", header: pt("Operari"), sortable: true },
  { field: "machineStatus", header: pt("Estat del centre") },
  {
    field: "startTime",
    header: pt("Inici"),
    sortable: true,
    resolver: (value) =>
      typeof value === "string" ? formatDateTimeUTCWithSeconds(value) : "",
  },
  {
    field: "endTime",
    header: pt("Fi"),
    sortable: true,
    resolver: (value) =>
      typeof value === "string" ? formatDateTimeUTCWithSeconds(value) : "",
  },
  {
    field: "quantityOk",
    header: pt("Quantitat OK"),
    columnType: ColumnType.Number,
  },
  {
    field: "quantityKo",
    header: pt("Quantitat KO"),
    columnType: ColumnType.Number,
  },
  {
    field: "plannedQuantity",
    header: pt("Quantitat Prevista"),
    columnType: ColumnType.Number,
  },
  {
    field: "operatorCost",
    header: pt("Cost Operari"),
    columnType: ColumnType.Currency,
  },
  {
    field: "estimatedOperatorCost",
    header: pt("Cost operari estimat (per OF)"),
    columnType: ColumnType.Currency,
  },
  {
    field: "workcenterCost",
    header: pt("Cost centre"),
    columnType: ColumnType.Currency,
  },
  {
    field: "estimatedMachineCost",
    header: pt("Cost centre estimat (per OF)"),
    columnType: ColumnType.Currency,
  },
  {
    field: "totalCost",
    header: pt("Cost Tall"),
    columnType: ColumnType.Currency,
  },
  {
    field: "totalHours",
    header: pt("Hores"),
    resolver: (value) => (typeof value === "number" ? value.toFixed(2) : ""),
  },
  {
    field: "workOrderCode",
    header: pt("Ordre de treball"),
    sortable: true,
  },
  { field: "workOrderPhaseCode", header: pt("Fase"), sortable: true },
  { field: "workOrderPhaseDescription", header: pt("Descripcio fase") },
  { field: "referenceCode", header: pt("Referencia") },
  { field: "referenceDescription", header: pt("Descripcio referencia") },
  { field: "customerComercialName", header: pt("Client") },
]);

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  groupBy: "None",
  timeGroupBy: "None",
});

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "dates",
    label: pt("Període"),
    type: "date-range",
    placeholder: pt("Seleccioni un període"),
    size: "lg",
  },
  {
    key: "groupBy",
    label: pt("Grup"),
    type: "select",
    options: [
      { label: pt("Operari"), value: "Operator" },
      { label: pt("Centre de treball"), value: "Workcenter" },
      { label: pt("Ordre de treball"), value: "Workorder" },
      { label: pt("Cap"), value: "None" },
    ],
    optionLabel: "label",
    optionValue: "value",
    size: "md",
    row: 0,
  },
  {
    key: "timeGroupBy",
    label: pt("Grup per temps"),
    type: "select",
    options: [
      { label: pt("Dia"), value: "Day" },
      { label: pt("Setmana"), value: "Week" },
      { label: pt("Mes"), value: "Month" },
      { label: pt("Any"), value: "Year" },
      { label: pt("Cap"), value: "None" },
    ],
    optionLabel: "label",
    optionValue: "value",
    size: "md",
    row: 0,
  },
]);

onMounted(async () => {
  store.setMenuItem({
    title: pt("Històric"),
    icon: "pi pi-fw pi-clock",
    backButtonVisible: false,
  });
});

const filterData = async () => {
  if (filter.value.dates?.[0] && filter.value.dates[1]) {
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
      summary: pt("Filtre invàlid"),
      detail: pt("Seleccioni un període"),
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
