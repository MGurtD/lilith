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
                >{{ pt("Període") }}</label
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
      <Column field="workcenter" :header="pt('Centre de treball')" sortable> </Column>
      <Column field="operator" :header="pt('Operari')" sortable> </Column>
      <Column field="machineStatus" :header="pt('Estat del centre')"></Column>

      <Column field="startTime" :header="pt('Inici')" sortable>
        <template #body="slotProps">
          {{ formatDateTimeUTCWithSeconds(slotProps.data.startTime) }}
        </template>
      </Column>
      <Column field="endTime" :header="pt('Fi')" sortable>
        <template #body="slotProps">
          {{ formatDateTimeUTCWithSeconds(slotProps.data.endTime) }}
        </template>
      </Column>
      <Column field="quantityOk" :header="pt('Quantitat OK')" />
      <Column field="quantityKo" :header="pt('Quantitat KO')" />
      <Column field="plannedQuantity" :header="pt('Quantitat Prevista')" />
      <Column field="operatorCost" :header="pt('Cost Operari')">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.operatorCost) }}
        </template>
      </Column>
      <Column
        field="estimatedOperatorCost"
        :header="pt('Cost operari estimat (per OF)')"
      >
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.estimatedOperatorCost) }}
        </template>
      </Column>
      <Column field="workcenterCost" :header="pt('Cost centre')">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.workcenterCost) }}
        </template>
      </Column>
      <Column
        field="estimatedMachineCost"
        :header="pt('Cost centre estimat (per OF)')"
      >
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.estimatedMachineCost) }}
        </template>
      </Column>
      <Column field="totalCost" :header="pt('Cost Tall')">
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.totalCost) }}
        </template>
      </Column>
      <Column field="totalHours" :header="pt('Hores')">
        <template #body="slotProps">
          {{ slotProps.data.totalHours.toFixed(2) }}
        </template>
      </Column>
      <Column field="workOrderCode" :header="pt('Ordre de treball')" sortable></Column>
      <Column field="workOrderPhaseCode" :header="pt('Fase')" sortable></Column>
      <Column
        field="workOrderPhaseDescription"
        :header="pt('Descripcio fase')"
      ></Column>
      <Column field="referenceCode" :header="pt('Referencia')"></Column>
      <Column
        field="referenceDescription"
        :header="pt('Descripcio referencia')"
      ></Column>
      <Column field="customerComercialName" :header="pt('Client')"></Column>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
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
  { label: pt("Operari"), value: "Operator" },
  { label: pt("Centre de treball"), value: "Workcenter" },
  { label: pt("Ordre de treball"), value: "Workorder" },
  { label: pt("Cap"), value: "None" },
];

const timeGroupByOptions = [
  { label: pt("Dia"), value: "Day" },
  { label: pt("Setmana"), value: "Week" },
  { label: pt("Mes"), value: "Month" },
  { label: pt("Any"), value: "Year" },
  { label: pt("Cap"), value: "None" },
];

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  groupBy: "None",
  timeGroupBy: "None",
});

const filterConfig: Array<FilterConfig> = [
  {
    key: "groupBy",
    label: pt("Grup"),
    type: "select",
    options: groupByOptions,
    optionLabel: "label",
    optionValue: "value",
    size: "md",
    row: 0,
  },
  {
    key: "timeGroupBy",
    label: pt("Grup per temps"),
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
    title: pt("Històric"),
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
