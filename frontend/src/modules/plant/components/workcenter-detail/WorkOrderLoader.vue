<template>
  <Dialog
    :visible="visible"
    modal
    :closable="true"
    :style="{ width: '90vw' }"
    @update:visible="$emit('update:visible', $event)"
  >
    <template #header>
      <div class="w-full flex align-items-center justify-content-between pr-4">
        <div class="flex align-items-center gap-3">
          <div
            class="flex align-items-center justify-content-center bg-primary-100 border-circle p-2"
            style="width: 3rem; height: 3rem"
          >
            <i :class="PrimeIcons.COG" class="text-primary text-xl"></i>
          </div>
          <div class="flex flex-column">
            <span class="font-bold text-lg text-900">Carregar Ordre</span>
            <span class="text-sm text-500">Selecció de fase i activitat</span>
          </div>
        </div>
        <div class="flex gap-4 flex-wrap">
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold">Ordre</span>
            <span class="font-medium text-900 text-lg">{{
              workOrderCode
            }}</span>
          </div>
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold"
              >Referència</span
            >
            <span class="font-medium text-900 text-lg">{{
              referenceCode
            }}</span>
          </div>
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold"
              >Quantitat</span
            >
            <span class="font-medium text-900 text-lg">{{ quantity }}</span>
          </div>
        </div>
      </div>
    </template>

    <div class="dialog-content">
      <DataTable
        :value="phases"
        :loading="loading"
        responsiveLayout="scroll"
        stripedRows
        :rowHover="true"
        :rowClass="getRowClass"
        class="p-datatable-sm clickable-rows"
        sortField="phaseCode"
        :sortOrder="1"
        @row-click="handleRowClick"
      >
        <!-- Phase Code -->
        <Column
          field="phaseCode"
          header="Codi"
          :sortable="true"
          style="max-width: 50px"
        />

        <!-- Phase Description -->
        <Column
          field="phaseDescription"
          header="Descripció"
          style="min-width: 200px"
        />

        <!-- Phase Status -->
        <Column header="Estat" style="min-width: 150px">
          <template #body="slotProps">
            <Tag :value="slotProps.data.phaseStatus" severity="info" rounded />
          </template>
        </Column>

        <!-- Start Time -->
        <Column header="Inici" style="min-width: 150px">
          <template #body="slotProps">
            <span v-if="slotProps.data.startTime">
              {{ formatDateTime(slotProps.data.startTime) }}
            </span>
          </template>
        </Column>

        <!-- End Time -->
        <Column header="Fi" style="min-width: 150px">
          <template #body="slotProps">
            <span v-if="slotProps.data.endTime">
              {{ formatDateTime(slotProps.data.endTime) }}
            </span>
          </template>
        </Column>

        <!-- Preferred Workcenter -->
        <Column
          field="preferredWorkcenterName"
          header="Màquina Preferida"
          style="min-width: 180px"
        />

        <Column header="Quant.">
          <template #body="slotProps">
            <span class="quantity-ok">{{ slotProps.data.quantityOk }}</span> /
            <span class="quantity-ko">{{ slotProps.data.quantityKo }}</span>
          </template>
        </Column>

        <template #empty>
          <div class="no-data">
            <i :class="PrimeIcons.INBOX" style="font-size: 2rem"></i>
            <p>No s'han trobat fases per aquesta ordre de fabricació</p>
          </div>
        </template>
      </DataTable>

      <!-- Warning message when there are loaded work orders -->
      <InfoPanel
        v-if="hasLoadedWorkOrders && phases.length > 0"
        severity="warn"
        text="No es pot carregar una nova ordre mentre hi hagi fases en procés a la màquina. Finalitza les fases carregades abans de carregar-ne una de nova."
      />

      <!-- No Valid Phases Message (only when no loaded work orders) -->
      <InfoPanel
        v-else-if="phases.length == 0"
        severity="warn"
        text="No hi ha fases disponibles per carregar en aquest centre de treball"
      />

      <!-- Bottom Panel with Dropdown and Button -->
      <div class="bottom-panel" v-if="!hasLoadedWorkOrders">
        <div class="panel-content">
          <div class="dropdown-container">
            <label for="activity-dropdown" class="dropdown-label">
              Activitat a carregar
            </label>
            <SelectWorkOrderPhaseDetail
              v-model="selectedDetailId"
              :details="selectedPhase?.details || []"
              class="activity-dropdown"
            />
          </div>
          <Button
            :icon="PrimeIcons.COG"
            label="Carregar"
            severity="success"
            :disabled="!selectedDetailId || hasLoadedWorkOrders"
            @click="onLoadActivity"
            class="load-button"
          />
        </div>
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { WorkOrderPhaseDetailed } from "../../../production/types";
import { WorkOrderPhaseService } from "../../../production/services/workorder.service";
import { useToast } from "primevue/usetoast";
import { formatDateTime } from "../../../../utils/functions";
import { usePlantWorkcenterStore } from "../../store/workcenter.store";
import SelectWorkOrderPhaseDetail from "./SelectWorkOrderPhaseDetail.vue";

interface Props {
  visible: boolean;
  workOrderId: string;
  workOrderCode: string;
  referenceCode: string;
  quantity: number;
  workcenterTypeId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (
    e: "phase-detail-selected",
    data: {
      workOrderId: string;
      workOrderPhaseId: string;
      machineStatusId: string;
    },
  ): void;
}>();

const toast = useToast();
const phaseService = new WorkOrderPhaseService("WorkOrderPhase");
const workcenterStore = usePlantWorkcenterStore();

const phases = ref<WorkOrderPhaseDetailed[]>([]);
const loading = ref(false);
const selectedDetailId = ref<string>("");
const selectedPhaseId = ref<string>("");

// Check if there are loaded work orders in the workcenter
const hasLoadedWorkOrders = computed(() => {
  return workcenterStore.loadedWorkOrdersPhases.length > 0;
});

// Auto-select first phase with endTime = null and matching workcenterTypeId
// Important: Sort by phaseCode first to get the first one in order
const autoSelectedPhase = computed(() => {
  const validPhases = phases.value
    .filter(
      (phase) =>
        !phase.endTime && phase.workcenterTypeId === props.workcenterTypeId,
    )
    .sort((a, b) => a.phaseCode.localeCompare(b.phaseCode));

  return validPhases[0];
});

// Get the phase selected by the user (or auto-selected if none chosen)
const selectedPhase = computed(() => {
  if (selectedPhaseId.value) {
    return phases.value.find((p) => p.phaseId === selectedPhaseId.value);
  }
  return autoSelectedPhase.value;
});

// Check if there are valid phases available and no loaded work orders
const hasValidPhases = computed(() => {
  return selectedPhase.value !== undefined && !hasLoadedWorkOrders.value;
});

const selectPhase = (phase: WorkOrderPhaseDetailed) => {
  selectedPhaseId.value = phase.phaseId;
  // Auto-select first detail when phase is selected
  if (phase.details && phase.details.length > 0) {
    selectedDetailId.value = phase.details[0].machineStatusId || "";
  } else {
    selectedDetailId.value = "";
  }
};

const handleRowClick = (event: any) => {
  const phase = event.data as WorkOrderPhaseDetailed;
  // Only allow selection if phase is valid (same criteria as button visibility)
  if (phase.workcenterTypeId === props.workcenterTypeId) {
    selectPhase(phase);
  }
};

const getRowClass = (data: WorkOrderPhaseDetailed) => {
  if (selectedPhaseId.value && data.phaseId === selectedPhaseId.value) {
    return "selected-phase-row";
  }
  return "";
};

const onLoadActivity = () => {
  if (!selectedPhase.value || !selectedDetailId.value) {
    toast.add({
      severity: "warn",
      summary: "Selecciona una fase i una activitat",
      life: 4000,
    });
    return;
  }

  emit("phase-detail-selected", {
    workOrderId: props.workOrderId,
    workOrderPhaseId: selectedPhase.value.phaseId,
    machineStatusId: selectedDetailId.value,
  });
};

const loadPhases = async () => {
  if (!props.workOrderId) return;

  loading.value = true;
  selectedDetailId.value = ""; // Reset selection
  selectedPhaseId.value = ""; // Reset phase selection
  try {
    const result = await phaseService.GetWorkOrderPhasesDetailed(
      props.workOrderId,
    );
    if (result) {
      phases.value = result;
      // Auto-select first valid phase
      if (autoSelectedPhase.value) {
        selectPhase(autoSelectedPhase.value);
      }
    } else {
      phases.value = [];
    }
  } catch (error) {
    console.error("Error loading work order phases:", error);
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Error al carregar les fases de l'ordre de fabricació",
      life: 4000,
    });
    phases.value = [];
  } finally {
    loading.value = false;
  }
};

watch(
  () => props.workOrderId,
  () => {
    if (props.visible && props.workOrderId) {
      loadPhases();
    }
  },
);

watch(
  () => props.visible,
  (newValue) => {
    if (newValue && props.workOrderId) {
      loadPhases();
    }
  },
);

onMounted(() => {
  if (props.visible && props.workOrderId) {
    loadPhases();
  }
});
</script>

<style scoped>
.dialog-content {
  display: flex;
  flex-direction: column;

  height: 100%;
  gap: 1rem;
}

.dialog-content :deep(.selected-phase-row) {
  background: var(--loaded-row-bg) !important;
  border-left: 4px solid var(--loaded-row-border) !important;
}

.dialog-content :deep(.p-datatable-tbody > tr > td .p-button) {
  width: 100%;
}

.no-data {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 3rem 1rem;
  color: var(--text-color-secondary);
}

.no-data i {
  color: var(--text-color-secondary);
  opacity: 0.5;
}

.no-data p {
  margin: 0;
  font-size: 1rem;
}

.bottom-panel {
  background: var(--p-surface-50);
  border-top: 2px solid var(--p-surface-border);
  padding: 1.25rem 1.5rem;
  border-radius: 0 0 var(--border-radius) var(--border-radius);
}

.panel-content {
  display: flex;
  align-items: flex-end;
  gap: 1.5rem;
}

.dropdown-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.dropdown-label {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--text-color);
}

.activity-dropdown {
  width: 100%;
}

.load-button {
  min-width: 150px;
  font-size: 1.05rem;
  padding: 0.75rem 1.5rem;
}

.load-button :deep(.p-button-icon) {
  font-size: 1.2rem;
}
</style>
