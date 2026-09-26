<template>
  <div v-if="counterKo > 0" class="rejection-section">
    <h4 class="section-title">
      <i :class="PrimeIcons.EXCLAMATION_TRIANGLE" class="mr-2"></i>
      {{ $t("plant.rejections.title") }}
    </h4>
    <p class="section-hint">{{ $t("plant.rejections.hint") }}</p>

    <div
      v-for="(rejection, index) in modelValue"
      :key="index"
      class="rejection-row"
    >
      <Select
        :model-value="rejection.rejectionReasonId"
        :options="availableReasons(rejection.rejectionReasonId)"
        option-label="name"
        option-value="id"
        :placeholder="$t('plant.rejections.selectReason')"
        class="rejection-reason"
        @update:model-value="updateReason(index, $event)"
      />
      <InputNumber
        :model-value="rejection.quantity"
        :min="0"
        :useGrouping="false"
        class="rejection-quantity"
        showButtons
        buttonLayout="horizontal"
        :step="1"
        decrementButtonClass="p-button-secondary"
        incrementButtonClass="p-button-secondary"
        incrementButtonIcon="pi pi-plus"
        decrementButtonIcon="pi pi-minus"
        @update:model-value="updateQuantity(index, $event ?? 0)"
      />
      <Button
        :icon="PrimeIcons.TRASH"
        severity="danger"
        text
        :aria-label="$t('plant.rejections.removeRow')"
        @click="removeRow(index)"
      />
    </div>

    <div class="rejection-actions">
      <Button
        :icon="PrimeIcons.PLUS"
        :label="$t('plant.rejections.addReason')"
        severity="secondary"
        text
        :disabled="!canAddRow"
        @click="addRow"
      />
      <span class="rejection-summary" :class="{ invalid: !isBalanced }">
        {{
          $t("plant.rejections.assigned", {
            assigned: assignedQuantity,
            total: counterKo,
          })
        }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";

import { useRejectionReasonStore } from "../../../production/store/rejectionreason";
import {
  RejectionReason,
  WorkOrderPhaseRejectionRequest,
} from "../../../production/types";

interface Props {
  modelValue: Array<WorkOrderPhaseRejectionRequest>;
  counterKo: number;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  (
    event: "update:modelValue",
    value: Array<WorkOrderPhaseRejectionRequest>,
  ): void;
}>();

const rejectionReasonStore = useRejectionReasonStore();
const { activeRejectionReasons } = storeToRefs(rejectionReasonStore);

const reasons = computed<Array<RejectionReason>>(
  () => activeRejectionReasons.value ?? [],
);

// The declared KO units must be fully assigned to reasons, or none at all.
const assignedQuantity = computed(() =>
  props.modelValue.reduce((total, r) => total + (r.quantity ?? 0), 0),
);

const isBalanced = computed(
  () => props.modelValue.length === 0 || assignedQuantity.value === props.counterKo,
);

const canAddRow = computed(
  () => props.modelValue.length < reasons.value.length,
);

const availableReasons = (currentReasonId: string) =>
  reasons.value.filter(
    (reason) =>
      reason.id === currentReasonId ||
      !props.modelValue.some((r) => r.rejectionReasonId === reason.id),
  );

const emitRows = (rows: Array<WorkOrderPhaseRejectionRequest>) =>
  emit("update:modelValue", rows);

const addRow = () => {
  emitRows([
    ...props.modelValue,
    { rejectionReasonId: "", quantity: pendingQuantity() },
  ]);
};

// A new row defaults to whatever is still unassigned, so the common
// single-reason case needs no typing at all.
const pendingQuantity = () =>
  Math.max(props.counterKo - assignedQuantity.value, 0);

const removeRow = (index: number) => {
  emitRows(props.modelValue.filter((_, i) => i !== index));
};

const updateReason = (index: number, rejectionReasonId: string) => {
  emitRows(
    props.modelValue.map((row, i) =>
      i === index ? { ...row, rejectionReasonId } : row,
    ),
  );
};

const updateQuantity = (index: number, quantity: number) => {
  emitRows(
    props.modelValue.map((row, i) => (i === index ? { ...row, quantity } : row)),
  );
};

onMounted(async () => {
  if (!activeRejectionReasons.value) {
    await rejectionReasonStore.fetchActive();
  }
});

// Keep the breakdown consistent when the declared KO units change.
watch(
  () => props.counterKo,
  (counterKo) => {
    if (counterKo <= 0) {
      if (props.modelValue.length > 0) emitRows([]);
      return;
    }
    if (props.modelValue.length === 1) {
      emitRows([{ ...props.modelValue[0], quantity: counterKo }]);
    }
  },
);

defineExpose({ isBalanced, assignedQuantity });
</script>

<style scoped>
.rejection-section {
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
  border-left: 4px solid var(--p-red-500);
  border-radius: 8px;
  padding: 1rem;
}

.section-title {
  margin: 0 0 0.75rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text-color);
  display: flex;
  align-items: center;
}

.section-hint {
  margin: 0 0 1rem 0;
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

.rejection-row {
  display: grid;
  grid-template-columns: 1fr 12rem auto;
  gap: 0.75rem;
  align-items: center;
  margin-bottom: 0.75rem;
}

.rejection-reason,
.rejection-quantity {
  width: 100%;
}

/* Shop-floor touch targets. */
.rejection-reason,
.rejection-quantity :deep(.p-inputtext),
.rejection-quantity :deep(.p-button),
.rejection-row > :deep(.p-button),
.rejection-actions :deep(.p-button) {
  min-height: 48px;
}

.rejection-row > :deep(.p-button) {
  min-width: 48px;
}

.rejection-reason :deep(.p-select-label) {
  display: flex;
  align-items: center;
}

.rejection-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.rejection-summary {
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

.rejection-summary.invalid {
  color: var(--p-red-700);
  font-weight: 600;
}

@media (max-width: 768px) {
  .rejection-row {
    grid-template-columns: 1fr auto;
  }
}
</style>
