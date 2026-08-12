<template>
  <DataTable
    sortMode="multiple"
    :multiSortMeta="sortingFields"
    :value="processedPhases"
  >
    <Column field="code" sortable :header="t('sales.components.fase')"></Column>
    <Column field="order" sortable :header="t('sales.components.pas')"></Column>
    <Column field="workcenterTypeId" :header="t('sales.components.maquina')">
      <template #body="slotProps">
        {{ getWorkcenterType(slotProps.data.workcenterTypeId) }}
      </template>
    </Column>
    <Column field="machineStatusId" :header="t('sales.components.estat')">
      <template #body="slotProps">
        {{ getStatusName(slotProps.data.machineStatusId) }}
      </template>
    </Column>
    <Column field="estimatedTime" :header="t('sales.components.tempsTotal')"> </Column>
    <Column field="isCycleTime" :header="t('sales.components.tempsDeCicle')">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isCycleTime" />
      </template>
    </Column>
    <Column :header="t('sales.components.deBenefici')">
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
        <span class="text-l text-900">{{ t("sales.componentMessages.totalOperatingTime") }}</span>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :minFractionDigits="0"
          id="totalTime"
          :modelValue="totalTime"
          readonly
          disabled
        >
        </BaseInput>
        <span class="text-l text-900 font-bold">{{ t("sales.componentMessages.weightedProfit") }}</span>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :minFractionDigits="2"
          id="profitAverage"
          :modelValue="profitAverage"
          suffix="%"
          readonly
        />
        <Button @click="emitProfitAverage" icon="pi pi-copy" :label="t('sales.components.aplicar')" />
      </div>
    </template>
  </DataTable>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
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
import { DetailPhaseProfit } from "../types";
import { getNewUuid } from "../../../utils/functions";

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

const { t } = useI18n();
const props = defineProps<{
  workMasterId: string | null;
  quantity: number;
  phaseProfits: DetailPhaseProfit[];
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

const emit = defineEmits(["updateProfitAverage", "update:phaseProfits"]);

const processPhases = (
  phases: WorkMasterPhase[],
  quantity: number
): ProcessedPhase[] => {
  return phases.flatMap((phase) =>
    phase.details!.map((detail) => {
      // Persist per-step profit keyed by the stable WorkMasterPhaseDetail id.
      const key = detail.id;
      if (!(key in stepProfitPercentages)) {
        const persisted = props.phaseProfits?.find(
          (p) => p.workMasterPhaseDetailId === key
        );
        stepProfitPercentages[key] =
          persisted?.profitPercentage ?? phase.profitPercentage ?? 0;
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

// NOTE: createUniqueKey and calculateWeightedProfit must be declared here,
// BEFORE the watchEffect below. watchEffect runs its callback synchronously
// during setup when the sync (no-await) branch executes (workMasterId is
// null), so calling a `const` function declared later in the file would hit
// the temporal dead zone (ReferenceError: Cannot access '...' before
// initialization). onMounted/watch callbacks are deferred, so they are safe
// regardless of declaration order, but watchEffect is not.
const createUniqueKey = (phase: ProcessedPhase) => {
  return phase.id;
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

// Rebuild the persisted per-step profit list from current steps and notify the parent.
const syncPhaseProfits = () => {
  const profits: DetailPhaseProfit[] = processedPhases.value.map((phase) => {
    const existing = props.phaseProfits?.find(
      (p) => p.workMasterPhaseDetailId === phase.id
    );
    return {
      id: existing?.id ?? getNewUuid(),
      workMasterPhaseDetailId: phase.id,
      profitPercentage: stepProfitPercentages[phase.id] ?? 0,
    };
  });
  emit("update:phaseProfits", profits);
};

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

watch(
  () => props.workMasterId,
  () => {
    // A different route invalidates any per-step profits kept for the previous one.
    Object.keys(stepProfitPercentages).forEach(
      (k) => delete stepProfitPercentages[k]
    );
    emit("update:phaseProfits", []);
  }
);

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
    syncPhaseProfits();
  } else {
    console.error(
      "Attempted to set a non-number value for profit percentage:",
      value
    );
  }
};
</script>
