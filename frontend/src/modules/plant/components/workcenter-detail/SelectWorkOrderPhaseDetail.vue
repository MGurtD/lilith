<template>
  <Select
    v-model="selectedValue"
    :options="sortedDetails"
    optionLabel="machineStatusName"
    optionValue="machineStatusId"
    placeholder="Selecciona un estat..."
    class="w-full"
  >
    <template #value="slotProps">
      <div v-if="slotProps.value" class="selected-activity">
        <i :class="PrimeIcons.CHECK" class="activity-check-icon"></i>
        <span>{{ getDetailName(slotProps.value) }}</span>
        <span class="time-badge">
          <i :class="PrimeIcons.CLOCK"></i>
          {{ getDetail(slotProps.value)?.estimatedTime }} min
        </span>
        <span
          class="operator-time-badge"
          v-if="hasOperatorTime(slotProps.value)"
        >
          <i :class="PrimeIcons.USER"></i>
          {{ getDetail(slotProps.value)?.estimatedOperatorTime }} min
        </span>
      </div>
      <span v-else>{{ slotProps.placeholder }}</span>
    </template>
    <template #option="slotProps">
      <div class="activity-option">
        <i :class="PrimeIcons.WRENCH" class="option-icon"></i>
        <span class="status-name">{{ slotProps.option.machineStatusName }}</span>
        <span class="time-badge">
          <i :class="PrimeIcons.CLOCK"></i>
          {{ slotProps.option.estimatedTime }} min
        </span>
        <span
          class="operator-time-badge"
          v-if="(slotProps.option.estimatedOperatorTime ?? 0) > 0"
        >
          <i :class="PrimeIcons.USER"></i>
          {{ slotProps.option.estimatedOperatorTime }} min
        </span>
      </div>
    </template>
  </Select>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { PhaseDetailItem } from "../../../production/types";

interface Props {
  modelValue: string;
  details: PhaseDetailItem[];
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:modelValue", value: string): void;
}>();

const selectedValue = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value),
});

const sortedDetails = computed(() =>
  [...props.details].sort((a, b) => a.order - b.order)
);

const getDetail = (machineStatusId: string) =>
  props.details.find((d) => d.machineStatusId === machineStatusId);

const getDetailName = (machineStatusId: string): string =>
  getDetail(machineStatusId)?.machineStatusName || "";

const hasOperatorTime = (machineStatusId: string): boolean =>
  (getDetail(machineStatusId)?.estimatedOperatorTime ?? 0) > 0;
</script>

<style scoped>
.selected-activity {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 600;
}

.activity-check-icon {
  color: var(--p-green-600);
  font-size: 1rem;
}

.activity-option {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.option-icon {
  color: var(--p-primary-color);
  font-size: 1rem;
}

.status-name {
  font-weight: 600;
  font-size: 1rem;
}

.time-badge,
.operator-time-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  background: var(--p-blue-100);
  color: var(--p-blue-700);
  border-radius: var(--border-radius);
  font-size: 0.85rem;
  font-weight: 500;
}

.operator-time-badge {
  background: var(--purple-100);
  color: var(--purple-700);
}

.time-badge i,
.operator-time-badge i {
  font-size: 0.8rem;
}
</style>
