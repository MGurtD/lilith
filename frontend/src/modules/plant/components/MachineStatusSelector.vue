<template>
  <Dialog
    v-model:visible="dialogVisible"
    :header="dialogTitle"
    :modal="true"
    :closable="true"
    :draggable="false"
    :style="{ width: '90vw', maxWidth: '800px' }"
    @hide="resetSelection"
  >
    <!-- Step 1: Status Selection -->
    <div v-if="currentStep === 'status'" class="status-selection">
      <div class="status-grid">
        <div
          v-for="status in availableStatuses"
          :key="status.id"
          class="status-card"
          :class="{ 'status-card--selected': selectedStatus?.id === status.id }"
          :style="getStatusCardStyle(status)"
          @click="selectStatus(status)"
        >
          <div class="status-card__content">
            <div class="status-card__icon">
              <i :class="status.icon || PrimeIcons.CIRCLE"></i>
            </div>
            <h3 class="status-card__name">{{ status.name }}</h3>
          </div>
          <div
            v-if="selectedStatus?.id === status.id"
            class="status-card__check"
          >
            <i :class="PrimeIcons.CHECK"></i>
          </div>
        </div>
      </div>
    </div>

    <!-- Step 2: Reason Selection (conditional) -->
    <div v-if="currentStep === 'reason'" class="reason-selection">
      <div class="reason-list">
        <div
          v-for="reason in availableReasons"
          :key="reason.id"
          class="reason-item"
          :class="{ 'reason-item--selected': selectedReason?.id === reason.id }"
          @click="selectReason(reason)"
        >
          <div
            class="reason-item__icon"
            :style="{ color: getReasonColor(reason) }"
          >
            <i :class="reason.icon || PrimeIcons.TAG"></i>
          </div>
          <div class="reason-item__content">
            <div class="reason-item__header">
              <span class="reason-item__text"
                >{{ reason.code }} - {{ reason.name }}</span
              >
            </div>
            <p v-if="reason.description" class="reason-item__description">
              {{ reason.description }}
            </p>
          </div>
          <div
            v-if="selectedReason?.id === reason.id"
            class="reason-item__check"
          >
            <i :class="PrimeIcons.CHECK_CIRCLE"></i>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state for reasons -->
    <div
      v-if="currentStep === 'reason' && availableReasons.length === 0"
      class="empty-state"
    >
      <i :class="PrimeIcons.INFO_CIRCLE" style="font-size: 2.5rem"></i>
      <p>No hi ha motius disponibles per aquest estat</p>
    </div>

    <!-- Footer Actions -->
    <template #footer>
      <div class="dialog-footer">
        <Button
          :label="currentStep === 'status' ? 'Cancel·lar' : 'Tornar'"
          severity="secondary"
          :icon="
            currentStep === 'status' ? PrimeIcons.TIMES : PrimeIcons.ARROW_LEFT
          "
          @click="handleCancel"
        />
        <Button
          v-if="currentStep === 'status'"
          label="Confirmar"
          severity="success"
          :icon="PrimeIcons.CHECK"
          :disabled="!canConfirmStatus"
          @click="confirmStatusSelection"
        />
        <Button
          v-if="currentStep === 'reason'"
          label="Confirmar"
          severity="success"
          :icon="PrimeIcons.CHECK"
          :disabled="!selectedReason"
          @click="confirmReasonSelection"
        />
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { MachineStatus, MachineStatusReason } from "../../production/types";
import { ChangeMachineStatusRequest } from "../types";
import {
  normalizeColor,
  isColorLight,
  adjustBrightness,
} from "../../../utils/functions";

interface Props {
  visible: boolean;
  statuses: MachineStatus[];
  workOrderPhaseId?: string;
  excludeIds?: string[];
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "status-changed", request: ChangeMachineStatusRequest): void;
}>();

type SelectionStep = "status" | "reason";

const currentStep = ref<SelectionStep>("status");
const selectedStatus = ref<MachineStatus | undefined>();
const selectedReason = ref<MachineStatusReason | undefined>();

// Computed for dialog visibility (two-way binding)
const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit("update:visible", value),
});

const dialogTitle = computed(() => {
  if (currentStep.value === "status") {
    return "Seleccionar estat de la màquina";
  }
  return `Seleccionar motiu - ${selectedStatus.value?.name}`;
});

// Filter active statuses, exclude specified IDs, and sort by preferred
const availableStatuses = computed(() => {
  const excludeSet = new Set(props.excludeIds ?? []);
  return props.statuses
    .filter((s) => !s.disabled && !excludeSet.has(s.id))
    .sort((a, b) => {
      // Preferred first
      if (a.preferred && !b.preferred) return -1;
      if (!a.preferred && b.preferred) return 1;
      return a.name.localeCompare(b.name);
    });
});

// Get reasons for selected status
const availableReasons = computed(() => {
  if (!selectedStatus.value?.reasons) return [];
  return selectedStatus.value.reasons.filter((r) => !r.disabled);
});

// Can confirm status if:
// - Status is selected
// - AND (status doesn't require reason OR has no reasons available)
const canConfirmStatus = computed(() => {
  if (!selectedStatus.value) return false;

  // If status is stopped and has reasons, need to go to step 2
  if (selectedStatus.value.stopped && availableReasons.value.length > 0) {
    return true; // Can proceed to reason selection
  }

  // Otherwise can confirm directly
  return true;
});

// Generate dynamic styles for status cards
const getStatusCardStyle = (status: MachineStatus) => {
  const baseColor = normalizeColor(status.color);
  const isLightColor = isColorLight(baseColor);

  return {
    "--status-color": baseColor,
    "--status-gradient": isLightColor
      ? `linear-gradient(135deg, ${baseColor}, ${adjustBrightness(baseColor, -10)})`
      : `linear-gradient(135deg, ${baseColor}, ${adjustBrightness(baseColor, -15)})`,
    "--status-text-color": isLightColor ? "#000000" : "#ffffff",
  };
};

// Get reason color with # prefix
const getReasonColor = (reason: MachineStatusReason): string => {
  return normalizeColor(reason.color);
};

// Get reason tag styles with proper text color
const getReasonTagStyle = (reason: MachineStatusReason) => {
  const bgColor = getReasonColor(reason);
  const isLight = isColorLight(bgColor);

  return {
    backgroundColor: bgColor,
    color: isLight ? "#000000" : "#ffffff",
  };
};

const selectStatus = (status: MachineStatus) => {
  selectedStatus.value = status;
};

const selectReason = (reason: MachineStatusReason) => {
  selectedReason.value = reason;
};

const confirmStatusSelection = () => {
  if (!selectedStatus.value) return;

  // If status is stopped and has reasons, go to reason selection
  if (selectedStatus.value.stopped && availableReasons.value.length > 0) {
    currentStep.value = "reason";
    return;
  }

  // Otherwise, confirm without reason
  emitStatusChange();
};

const confirmReasonSelection = () => {
  if (!selectedReason.value) return;
  emitStatusChange();
};

const emitStatusChange = () => {
  if (!selectedStatus.value) return;

  const request: ChangeMachineStatusRequest = {
    workcenterId: "", // Will be set by parent component
    statusId: selectedStatus.value.id,
    statusReasonId: selectedReason.value?.id,
    workOrderPhaseId: props.workOrderPhaseId,
  };

  emit("status-changed", request);
  closeDialog();
};

const goBackToStatus = () => {
  currentStep.value = "status";
  selectedReason.value = undefined;
};

const handleCancel = () => {
  if (currentStep.value === "reason") {
    goBackToStatus();
  } else {
    closeDialog();
  }
};

const resetSelection = () => {
  currentStep.value = "status";
  selectedStatus.value = undefined;
  selectedReason.value = undefined;
};

const closeDialog = () => {
  dialogVisible.value = false;
  resetSelection();
};

// Reset when dialog opens
watch(
  () => props.visible,
  (newValue) => {
    if (newValue) {
      resetSelection();
    }
  },
);
</script>

<style scoped>
/* Status Selection Grid */
.status-selection {
  padding: 1rem 0;
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1rem;
}

.status-card {
  position: relative;
  background: var(--status-gradient);
  border: 3px solid transparent;
  border-radius: 12px;
  padding: 1rem 1.25rem;
  cursor: pointer;
  transition: all 0.3s ease;
  color: var(--status-text-color, white);
  min-height: 70px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.status-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
}

.status-card--selected {
  border-color: var(--status-text-color, white);
  box-shadow: 0 0 0 4px var(--status-color);
  transform: scale(1.02);
}

.status-card__content {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.status-card__icon {
  font-size: 2rem;
  opacity: 0.9;
  display: flex;
  align-items: center;
}

.status-card__name {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 700;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
}

.status-card__check {
  font-size: 1.25rem;
  animation: checkPop 0.3s ease;
  flex-shrink: 0;
}

@keyframes checkPop {
  0% {
    transform: scale(0);
  }
  50% {
    transform: scale(1.2);
  }
  100% {
    transform: scale(1);
  }
}

/* Reason Selection List */
.reason-selection {
  padding: 1rem 0;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.reason-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  max-height: 400px;
  overflow-y: auto;
  padding-right: 0.5rem;
}

.reason-item {
  position: relative;
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 2px solid var(--p-surface-border);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.reason-item:hover {
  background: var(--p-surface-hover);
  border-color: var(--p-primary-color);
}

.reason-item--selected {
  background: var(--p-primary-50);
  border-color: var(--p-primary-color);
  box-shadow: 0 0 0 3px var(--p-primary-100);
}

.reason-item__icon {
  flex-shrink: 0;

  i {
    font-size: 1.5rem;
  }
}

.reason-item__content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.reason-item__header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.reason-item__text {
  font-weight: 600;
  font-size: 1.2rem;
  color: var(--text-color);
}

.reason-item__description {
  margin: 0;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
  line-height: 1.4;
}

.reason-item__check {
  font-size: 1.5rem;
  color: var(--p-primary-color);
  flex-shrink: 0;
  animation: checkPop 0.3s ease;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding: 3rem 1rem;
  color: var(--text-color-secondary);
}

.empty-state i {
  color: var(--text-color-secondary);
  opacity: 0.5;
}

.empty-state p {
  margin: 0;
  font-size: 1rem;
  text-align: center;
}

/* Dialog Footer */
.dialog-footer {
  padding-top: 1rem;
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}

/* Responsive */
@media (max-width: 768px) {
  .status-grid {
    grid-template-columns: 1fr;
  }

  .status-card {
    min-height: 120px;
  }

  .reason-list {
    max-height: 300px;
  }
}

/* Scrollbar styling */
.reason-list::-webkit-scrollbar {
  width: 8px;
}

.reason-list::-webkit-scrollbar-track {
  background: var(--p-surface-100);
  border-radius: 4px;
}

.reason-list::-webkit-scrollbar-thumb {
  background: var(--p-surface-400);
  border-radius: 4px;
}

.reason-list::-webkit-scrollbar-thumb:hover {
  background: var(--p-surface-500);
}
</style>
