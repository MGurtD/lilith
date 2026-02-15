<template>
  <DataTable
    sortMode="multiple"
    :multiSortMeta="sortingFields"
    :value="processedPhases"
  >
    <Column field="code" sortable header="Fase"></Column>
    <Column field="order" sortable header="Pas"></Column>
    <Column field="workcenterTypeId" header="Màquina">
      <template #body="slotProps">
        {{ getWorkcenterType(slotProps.data.workcenterTypeId) }}
      </template>
    </Column>
    <Column field="machineStatusId" header="Estat">
      <template #body="slotProps">
        {{ getStatusName(slotProps.data.machineStatusId) }}
      </template>
    </Column>
    <Column field="estimatedTime" header="Temps total"> </Column>
    <Column field="isCycleTime" header="Temps de cicle">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isCycleTime" />
      </template>
    </Column>
    <Column header="% de benefici">
      <template #body="slotProps">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :minFractionDigits="2"
          id="profitPercentage"
          v-model="slotProps.data.profitPercentage"
          @input="
            (event: any) => updateProfitPercentage(slotProps.data, event.value)
          "
          suffix="%"
        />
      </template>
    </Column>
    <template #footer>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-l text-900">Temps total operatiu </span>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :minFractionDigits="0"
          id="totalTime"
          :modelValue="totalTime"
          readonly
          disabled
        >
        </BaseInput>
        <span class="text-l text-900 font-bold">Benefici ponderat </span>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :minFractionDigits="2"
          id="profitAverage"
          :modelValue="profitAverage"
          suffix="%"
          readonly
        />
        <Button @click="emitProfitAverage" icon="pi pi-copy" label="Aplicar" />
      </div>
    </template>
  </DataTable>
</template>

<script setup lang="ts">
import { ref, watchEffect, computed, onMounted, reactive, watch } from "vue";
import { useWorkMasterStore } from "../../production/store/workmaster";
import {
  MachineStatus,
  WorkMasterPhase,
  WorkcenterType,
} from "../../production/types";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { DataTableSortMeta } from "primevue/datatable";
import BooleanColumn from "../../../components/tables/BooleanColumn.vue";

interface ProcessedPhase {
  id: string;
  code: string;
  order: number;
  workcenterTypeId: string;
  machineStatusId: string;
  estimatedTime: number;
  isCycleTime: boolean;
  profitPercentage: number;
}

const props = defineProps<{
  workMasterId: string | null;
  quantity: number;
}>();

const sortingFields = ref([
  { field: "code", order: 1 },
  { field: "order", order: 1 },
] as DataTableSortMeta[]);

const workMasterStore = useWorkMasterStore();
const plantModelStore = usePlantModelStore();
const phases = ref<WorkMasterPhase[]>([]);
const workcenterTypes = ref<WorkcenterType[] | undefined>(undefined);
const machineStatuses = ref<MachineStatus[] | undefined>(undefined);

const tableProfitPercentages = reactive<{ [key: string]: number }>({});
const tableEstimatedTimes = reactive<{ [key: string]: number }>({});

const profitAverage = ref(0);

const stepProfitPercentages = reactive<{ [key: string]: number }>({});

const emit = defineEmits(["updateProfitAverage"]);

const processPhases = (
  phases: WorkMasterPhase[],
  quantity: number
): ProcessedPhase[] => {
  return phases.flatMap((phase) =>
    phase.details!.map((detail) => {
      const key = `${phase.code}-${detail.order}-${phase.workcenterTypeId}`;
      // Inicializar profitPercentage a nivel de paso si no está definido
      if (!(key in stepProfitPercentages)) {
        stepProfitPercentages[key] = phase.profitPercentage ?? 0;
      }
      return {
        id: detail.id,
        code: phase.code,
        order: detail.order,
        workcenterTypeId: phase.workcenterTypeId || "",
        machineStatusId: detail.machineStatusId || "",
        estimatedTime: detail.isCycleTime
          ? detail.estimatedTime * quantity
          : detail.estimatedTime,
        isCycleTime: detail.isCycleTime,
        profitPercentage: phase.isExternalWork ? 0 : stepProfitPercentages[key],
      };
    })
  );
};

watch(
  () => props.quantity,
  (newQuantity, oldQuantity) => {
    // Recalcula las fases procesadas y el profitAverage cuando cambia quantity
    calculateWeightedProfit();
  }
);

const processedPhases = computed(() =>
  processPhases(phases.value, props.quantity)
);

const totalTime = computed(() => {
  return processedPhases.value.reduce((total, phase) => {
    return total + phase.estimatedTime;
  }, 0);
});

onMounted(async () => {
  await plantModelStore.fetchWorkcenterTypes();
  await plantModelStore.fetchMachineStatuses();
  workcenterTypes.value = plantModelStore.workcenterTypes || [];
  machineStatuses.value = plantModelStore.machineStatuses || [];

  // Inicializar tableProfitPercentages y tableEstimatedTimes
  processedPhases.value.forEach((phase) => {
    const key = createUniqueKey(phase);
    if (!(phase.workcenterTypeId in tableProfitPercentages)) {
      tableProfitPercentages[key] = phase.profitPercentage;
    }
    if (!(phase.workcenterTypeId in tableEstimatedTimes)) {
      tableEstimatedTimes[key] = phase.estimatedTime;
    }
  });
  calculateWeightedProfit();
});

watchEffect(async () => {
  if (props.workMasterId) {
    await workMasterStore.fetchOne(props.workMasterId);
    phases.value = workMasterStore.workmaster?.phases || [];
  } else {
    phases.value = [];
  }
  calculateWeightedProfit();
});

const createUniqueKey = (phase: ProcessedPhase) => {
  return `${phase.code}-${phase.order}-${phase.workcenterTypeId}`;
};

const getWorkcenterType = (workcenterTypeId: string | undefined) => {
  const workcenterType = workcenterTypes.value?.find(
    (p) => p.id === workcenterTypeId
  );
  return workcenterType?.name || "No definit";
};

const getStatusName = (machineStatusId: string | undefined) => {
  const machineStatus = machineStatuses.value?.find(
    (p) => p.id === machineStatusId
  );
  return machineStatus?.name || "No definit";
};

const emitProfitAverage = () => {
  emit("updateProfitAverage", profitAverage.value);
};

const updateProfitPercentage = (phase: ProcessedPhase, value: number) => {
  if (typeof value === "number") {
    const key = createUniqueKey(phase);
    stepProfitPercentages[key] = value;
    phase.profitPercentage = value;
    tableProfitPercentages[key] = value;
    calculateWeightedProfit();
  } else {
    console.error(
      "Attempted to set a non-number value for profit percentage:",
      value
    );
  }
};

const calculateWeightedProfit = () => {
  let totalTime = 0;
  let weightedSum = 0;

  processedPhases.value.forEach((phase) => {
    const key = createUniqueKey(phase);
    const profit = stepProfitPercentages[key];
    const time = tableEstimatedTimes[key] ?? phase.estimatedTime;

    totalTime += time;
    weightedSum += time * profit;
  });

  profitAverage.value =
    !isNaN(totalTime) && totalTime > 0 && !isNaN(weightedSum)
      ? Number((weightedSum / totalTime).toFixed(2))
      : 0;
};
</script>
