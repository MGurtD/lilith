<template>
  <div class="realtime-panel-content">
    <!-- Workcenter Picture Section 
    <Panel
      v-if="workcenterStore.workcenterPictureUrl"
      header="Imatge del centre de treball"
      :toggleable="true"
      class="panel-section"
    >
      <div class="picture-container">
        <img
          :src="workcenterStore.workcenterPictureUrl"
          alt="Imatge del centre de treball"
          class="workcenter-picture"
        />
      </div>
    </Panel>
    -->

    <!-- Machine Status Section -->
    <Panel header="Estat actual" :toggleable="false" class="panel-section">
      <MachineStatusDetail
        :status="currentStatus"
        :reason="currentReason"
        :startTime="workcenter.realtime?.statusStartTime"
      />
      <TimeProgressBar
        v-if="workcenterStore.phaseTimeMetrics"
        :estimatedMinutes="
          workcenterStore.phaseTimeMetrics.estimatedMachineTimeMinutes
        "
        :actualMinutes="actualMachineTimeMinutes"
      />
    </Panel>

    <!-- Current Work Order Section -->
    <Panel
      header="Ordre de fabricació"
      :toggleable="false"
      class="panel-section"
    >
      <WorkOrderPhaseDetail
        v-if="currentWorkOrderData"
        :workOrderCode="currentWorkOrderData.workOrderCode"
        :customer="currentWorkOrderData.customer"
        :reference="currentWorkOrderData.reference"
        :phaseCode="currentWorkOrderData.phaseCode"
        :phaseDescription="currentWorkOrderData.phaseDescription"
        :loadedStartTime="currentWorkOrderData.loadedStartTime"
        :counterOk="currentWorkOrderData.counterOk"
        :counterKo="currentWorkOrderData.counterKo"
      />
      <div v-else class="no-data">
        <p>No hi ha cap ordre de fabricació carregada</p>
      </div>
    </Panel>

    <!-- Current Operators -->
    <Panel header="Operaris" :toggleable="false" class="panel-section">
      <div
        v-if="
          workcenter.realtime?.operators &&
          workcenter.realtime.operators.length > 0
        "
        class="operators-list"
      >
        <OperatorDetail
          v-for="operator in workcenter.realtime.operators"
          :key="operator.operatorId"
          :operator="operator"
        />
        <TimeProgressBar
          v-if="workcenterStore.phaseTimeMetrics?.operatorId"
          :estimatedMinutes="
            workcenterStore.phaseTimeMetrics.estimatedOperatorTimeMinutes
          "
          :actualMinutes="actualOperatorTimeMinutes"
        />
      </div>
      <div v-else class="no-data">
        <p>No hi ha operaris fitxats</p>
      </div>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { WorkcenterViewState } from "../../types";
import OperatorDetail from "./OperatorDetail.vue";
import WorkOrderPhaseDetail from "./WorkOrderPhaseDetail.vue";
import MachineStatusDetail from "../MachineStatusDetail.vue";
import TimeProgressBar from "./TimeProgressBar.vue";
import { usePlantWorkcenterStore, usePlantDataStore } from "../../store";

interface Props {
  workcenter: WorkcenterViewState;
}

const props = defineProps<Props>();
const workcenterStore = usePlantWorkcenterStore();
const dataStore = usePlantDataStore();

// Timer for updating elapsed time every second
let timeUpdateInterval: ReturnType<typeof setInterval> | undefined;
const elapsedSeconds = ref(0);

const currentStatus = computed(() => {
  if (!props.workcenter.realtime?.statusId) return undefined;
  return dataStore.machineStatuses.find(
    (s) => s.id === props.workcenter.realtime?.statusId,
  );
});

const currentReason = computed(() => {
  if (!props.workcenter.realtime?.statusReasonId || !currentStatus.value)
    return undefined;
  return currentStatus.value.reasons?.find(
    (r) => r.id === props.workcenter.realtime?.statusReasonId,
  );
});

// Time progress computed properties
const actualMachineTimeMinutes = computed(() => {
  const baseMinutes =
    workcenterStore.phaseTimeMetrics?.actualMachineTimeMinutes ?? 0;
  return baseMinutes + elapsedSeconds.value / 60;
});

const actualOperatorTimeMinutes = computed(() => {
  const baseMinutes =
    workcenterStore.phaseTimeMetrics?.actualOperatorTimeMinutes ?? 0;
  return baseMinutes + elapsedSeconds.value / 60;
});

// Computed para mostrar datos de la orden de fabricación cargada
const currentWorkOrderData = computed(() => {
  // Usar la primera orden de trabajo cargada desde el WebSocket
  if (
    workcenterStore.loadedWorkOrdersPhases &&
    workcenterStore.loadedWorkOrdersPhases.length > 0 &&
    props.workcenter.realtime?.workorders &&
    props.workcenter.realtime.workorders.length > 0
  ) {
    const wo = workcenterStore.loadedWorkOrdersPhases[0];
    const activeWorkOrder = props.workcenter.realtime.workorders[0];
    // Find the current phase from the loaded work order
    const currentPhase = wo.phases.find(
      (p) => p.phaseId === activeWorkOrder.workOrderPhaseId,
    );

    return {
      workOrderCode: wo.workOrderCode,
      customer: wo.customerName,
      reference: wo.salesReferenceDisplay,
      phaseCode: currentPhase?.phaseCode,
      phaseDescription: currentPhase?.phaseDescription,
      loadedStartTime: activeWorkOrder.startTime,
      counterOk: undefined, // TODO: Obtener de ProductionParts si es necesario
      counterKo: undefined, // TODO: Obtener de ProductionParts si es necesario
    };
  }

  return null;
});

// Start/stop timer based on metrics availability
const startTimeUpdateInterval = () => {
  if (timeUpdateInterval) return;
  elapsedSeconds.value = 0;
  timeUpdateInterval = setInterval(() => {
    elapsedSeconds.value++;
  }, 1000);
};

const stopTimeUpdateInterval = () => {
  if (timeUpdateInterval) {
    clearInterval(timeUpdateInterval);
    timeUpdateInterval = undefined;
  }
  elapsedSeconds.value = 0;
};

onMounted(() => {
  // Start timer if metrics are already available
  if (workcenterStore.phaseTimeMetrics) {
    startTimeUpdateInterval();
  }
});

onUnmounted(() => {
  stopTimeUpdateInterval();
});

// Watch for metrics changes to start/stop timer and reset elapsed time
watch(
  () => workcenterStore.phaseTimeMetrics,
  (newMetrics) => {
    if (newMetrics) {
      // Reset and restart timer when new metrics are loaded
      elapsedSeconds.value = 0;
      startTimeUpdateInterval();
    } else {
      stopTimeUpdateInterval();
    }
  },
);
</script>

<style scoped>
.realtime-panel-content {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  overflow-y: auto;
}

.panel-section {
  margin-bottom: 0;
}

.panel-section :deep(.p-panel-header) {
  background: var(--p-surface-50);
  font-weight: 600;
  font-size: 0.95rem;
  padding: 0.75rem 1rem;
}

.panel-section :deep(.p-panel-content) {
  padding: 1rem;
}

.picture-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 120px;
  border-radius: 8px;
  overflow: hidden;
}

.workcenter-picture {
  max-width: 100%;
  max-height: 200px;
  object-fit: contain;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.75rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-item.full-width {
  grid-column: 1 / -1;
}

.info-item label {
  font-size: 0.85rem;
  color: var(--text-color-secondary);
  font-weight: 500;
}

.info-value {
  font-size: 0.95rem;
  color: var(--text-color);
}

.info-description {
  font-size: 0.9rem;
  color: var(--text-color-secondary);
  font-style: italic;
}

.counter-ok {
  color: var(--p-green-600);
  font-weight: 700;
  font-size: 1.1rem;
}

.counter-ko {
  color: var(--p-red-600);
  font-weight: 700;
  font-size: 1.1rem;
}

.operators-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.no-data {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  text-align: center;
  padding: 2rem 1rem;
  color: var(--text-color-secondary);
}

.no-data i {
  margin-bottom: 0.5rem;
}

.no-data p {
  margin: 0;
  font-size: 0.9rem;
}
</style>
