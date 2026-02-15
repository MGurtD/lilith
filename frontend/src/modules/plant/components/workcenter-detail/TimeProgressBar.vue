<template>
  <div class="time-progress-bar-compact">
    <div class="progress-wrapper">
      <ProgressBar
        :value="progressPercentage"
        :showValue="false"
        :pt="{
          root: { class: 'progress-root' },
          value: { style: { background: progressColor } },
        }"
      />
      <div class="progress-overlay">
        <span class="progress-info" :style="{ color: textColor }">
          {{ formattedActual }} / {{ formattedEstimated }}
          <span class="progress-percentage">({{ displayPercentage }}%)</span>
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { formatDuration } from "@/utils/functions";

interface Props {
  estimatedMinutes: number;
  actualMinutes: number;
}

const props = defineProps<Props>();

const progressPercentage = computed(() => {
  if (props.estimatedMinutes <= 0) return 0;
  const percentage = (props.actualMinutes / props.estimatedMinutes) * 100;
  return Math.min(percentage, 100); // Cap at 100% for the bar display
});

const actualPercentage = computed(() => {
  if (props.estimatedMinutes <= 0) return 0;
  return (props.actualMinutes / props.estimatedMinutes) * 100;
});

const displayPercentage = computed(() => {
  return Math.round(actualPercentage.value);
});

const progressColor = computed(() => {
  const pct = actualPercentage.value;
  if (pct < 90) return "var(--p-green-500)";
  if (pct <= 110) return "var(--p-yellow-500)";
  return "var(--p-red-500)";
});

const textColor = computed(() => {
  const pct = actualPercentage.value;
  // Dark text for gray/yellow backgrounds, white for green/red
  if (pct < 50) return "#1f2937"; // Gray/partial green background -> dark text
  if (pct < 90) return "#ffffff"; // Green background -> white text
  if (pct <= 110) return "#1f2937"; // Yellow background -> dark text
  return "#ffffff"; // Red background -> white text
});

const formattedEstimated = computed(() =>
  formatDuration(props.estimatedMinutes),
);
const formattedActual = computed(() => formatDuration(props.actualMinutes));
</script>

<style scoped>
.time-progress-bar-compact {
  width: 100%;
  margin-top: 0.5rem;
}

.progress-wrapper {
  position: relative;
  width: 100%;
}

:deep(.p-progressbar) {
  height: 1.5rem;
  border-radius: 0.375rem;
  background: var(--p-surface-200);
}

:deep(.p-progressbar-value) {
  border-radius: 0.375rem;
  transition:
    width 0.3s ease,
    background 0.3s ease;
}

.progress-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.progress-info {
  font-size: 0.8rem;
  font-weight: 600;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
  transition: color 0.3s ease;
}

.progress-percentage {
  margin-left: 0.25rem;
  opacity: 0.9;
}
</style>
