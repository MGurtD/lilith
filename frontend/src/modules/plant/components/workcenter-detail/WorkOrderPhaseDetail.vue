<template>
  <div class="phase-item">
    <div class="phase-content">
      <div class="phase-header">
        <div class="phase-main-info">
          <span class="work-order-code">{{ workOrderCode }}</span>
          <span v-if="customer" class="customer-name">{{ customer }}</span>
        </div>
        <div class="phase-time-info">
          <small>{{ formatDateTime(loadedStartTime) }}</small>
          <span class="duration">{{ phaseDuration }}</span>
        </div>
      </div>

      <div class="phase-details">
        <div class="detail-row">
          <span class="detail-label">Ref :</span>
          <span class="detail-value">{{ reference }}</span>
        </div>
        <div v-if="phaseCode" class="detail-row">
          <span class="detail-label">Fase:</span>
          <span class="detail-value">
            <span class="phase-code">{{ phaseCode }}</span>
            <span v-if="phaseDescription" class="phase-description">
              - {{ phaseDescription }}
            </span>
          </span>
        </div>
        <div
          v-if="counterOk !== undefined || counterKo !== undefined"
          class="counters-row"
        >
          <div v-if="counterOk !== undefined" class="counter-item">
            <span class="counter-label">OK:</span>
            <span class="counter-value counter-ok">{{ counterOk }}</span>
          </div>
          <div v-if="counterKo !== undefined" class="counter-item">
            <span class="counter-label">KO:</span>
            <span class="counter-value counter-ko">{{ counterKo }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import {
  formatDateTime,
  calculateTimeDifference,
} from "../../../../utils/functions";

interface Props {
  workOrderCode: string;
  customer?: string;
  reference: string;
  phaseCode?: string;
  phaseDescription?: string;
  loadedStartTime: string;
  counterOk?: number;
  counterKo?: number;
}

const props = defineProps<Props>();

const phaseDuration = ref<string>("00:00:00");
let intervalId: number | null = null;

const updateDuration = () => {
  const duration = calculateTimeDifference(props.loadedStartTime);
  phaseDuration.value = typeof duration === "string" ? duration : "00:00:00";
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
.phase-item {
  padding: 1rem;
  background: var(--p-surface-50);
  border-radius: var(--border-radius);
  border-left: 3px solid var(--p-blue-500);
}

.phase-content {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.phase-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.phase-main-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  flex: 1;
  min-width: 0; /* Allow flex item to shrink below content size */
}

.work-order-code {
  font-weight: 700;
  font-size: 1rem;
  color: var(--text-color);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.customer-name {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--text-color-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.phase-time-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
  color: var(--text-color-secondary);
  font-size: 0.85rem;
  flex-shrink: 0;
}

.duration {
  font-weight: 600;
  color: var(--p-blue-600);
  font-size: 0.9rem;
}

.phase-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.detail-row {
  display: flex;
  gap: 0.5rem;
  font-size: 0.9rem;
  align-items: flex-start;
}

.detail-label {
  color: var(--text-color-secondary);
  font-weight: 500;
  min-width: 4rem;
  flex-shrink: 0;
}

.detail-value {
  color: var(--text-color);
  flex: 1;
  word-wrap: break-word;
  overflow-wrap: break-word;
  line-height: 1.4;
}

.phase-code {
  font-weight: 600;
}

.phase-description {
  color: var(--text-color-secondary);
  font-style: italic;
  word-wrap: break-word;
  overflow-wrap: break-word;
}

.counters-row {
  display: flex;
  gap: 1rem;
  margin-top: 0.25rem;
}

.counter-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.counter-label {
  font-size: 0.85rem;
  color: var(--text-color-secondary);
  font-weight: 500;
}

.counter-value {
  font-weight: 700;
  font-size: 1rem;
}

.counter-ok {
  color: var(--p-green-600);
}

.counter-ko {
  color: var(--p-red-600);
}
</style>
