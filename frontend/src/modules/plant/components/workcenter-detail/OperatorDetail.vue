<template>
  <div class="operator-item">
    <div class="operator-info">
      <span class="operator-name">{{ operator.operatorName }}</span>
    </div>
    <div class="operator-time">
      <div class="time-info">
        <small>{{ formatDateTime(operator.operatorStartTime) }}</small>
        <span class="duration">{{ operatorDuration }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { OperatorRealtime } from "../../types";
import {
  formatDateTime,
  calculateTimeDifference,
} from "../../../../utils/functions";

interface Props {
  operator: OperatorRealtime;
}

const props = defineProps<Props>();

const operatorDuration = ref<string>("00:00:00");
let intervalId: number | null = null;

const updateDuration = () => {
  const duration = calculateTimeDifference(props.operator.operatorStartTime);
  operatorDuration.value = typeof duration === "string" ? duration : "00:00:00";
};

onMounted(() => {
  updateDuration();
  // Actualitzar cada segon
  intervalId = window.setInterval(updateDuration, 1000);
});

onUnmounted(() => {
  if (intervalId !== null) {
    clearInterval(intervalId);
  }
});
</script>

<style scoped>
.operator-item {
  padding: 0.75rem;
  background: var(--p-surface-50);
  border-radius: var(--border-radius);
  border-left: 3px solid var(--p-primary-color);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.operator-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.operator-name {
  font-weight: 600;
  font-size: 0.95rem;
}

.operator-time {
  color: var(--text-color-secondary);
  font-size: 0.85rem;
}

.time-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
}

.duration {
  font-weight: 600;
  color: var(--p-primary-color);
  font-size: 0.9rem;
}
</style>
