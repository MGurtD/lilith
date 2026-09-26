<template>
  <div class="phase-summary">
    <div class="phase-summary__times">
      <div v-for="row in timeRows" :key="row.id" class="phase-summary__time">
        <div class="phase-summary__time-head">
          <span class="phase-summary__title">{{ row.label }}</span>
          <span class="phase-summary__figures">
            <strong :class="{ 'phase-summary__over-text': row.over }">{{
              row.actual
            }}</strong>
            {{ t("plant.detail.ofEstimated", { time: row.estimated }) }}
          </span>
        </div>
        <div class="phase-summary__bar" aria-hidden="true">
          <span class="phase-summary__bar-done" :style="{ width: row.donePct }"></span>
          <span class="phase-summary__bar-over" :style="{ width: row.overPct }"></span>
        </div>
        <span v-if="row.over" class="phase-summary__over-text">{{
          t("plant.detail.overEstimate", { time: row.overBy })
        }}</span>
      </div>
      <p v-if="!metrics" class="phase-summary__empty">
        {{ t("plant.detail.noMetrics") }}
      </p>
    </div>

    <div v-if="activities.length" class="phase-summary__activities">
      <span class="phase-summary__title">{{ t("plant.detail.activities") }}</span>
      <ul>
        <li v-for="activity in activities" :key="activity.id">
          <span class="phase-summary__swatch" :style="{ background: activity.color }"></span>
          <span class="phase-summary__activity-name">{{ activity.name }}</span>
          <span v-if="activity.current" class="phase-summary__current">{{
            t("plant.detail.current")
          }}</span>
          <span class="phase-summary__muted">{{
            t("plant.detail.estimated", { time: activity.estimated })
          }}</span>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { normalizeColor } from "@/utils/functions";
import { usePlantActivePhaseStore, usePlantWorkcenterStore } from "../../store";
import { useNow } from "../../composables/useNow";
import { formatElapsed } from "../../utils/elapsed";

// "Fase actual" tab: machine and operator time against the estimate, and
// the phase's activities with their estimates.
const { t } = useI18n();
const activePhaseStore = usePlantActivePhaseStore();
const workcenterStore = usePlantWorkcenterStore();
const now = useNow();

const metrics = computed(() => activePhaseStore.phaseTimeMetrics);

// Metrics are a snapshot; time keeps running on the device since then.
const sinceSnapshotMinutes = computed(() => {
  const at = metrics.value?.calculatedAt;
  if (!at) return 0;
  const minutes = (now.value - new Date(at).getTime()) / 60000;
  return minutes > 0 ? minutes : 0;
});

const fmtMinutes = (minutes: number) => formatElapsed(Math.round(minutes * 60));

const timeRows = computed(() => {
  const m = metrics.value;
  if (!m) return [];
  const row = (id: string, label: string, estimated: number, actualBase: number) => {
    const actual = actualBase + sinceSnapshotMinutes.value;
    const over = estimated > 0 && actual > estimated;
    const total = Math.max(estimated, actual) || 1;
    return {
      id,
      label,
      actual: fmtMinutes(actual),
      estimated: fmtMinutes(estimated),
      over,
      overBy: fmtMinutes(actual - estimated),
      donePct: `${Math.round((Math.min(actual, estimated || actual) / total) * 100)}%`,
      overPct: over ? `${Math.round(((actual - estimated) / total) * 100)}%` : "0%",
    };
  };
  return [
    row("machine", t("plant.detail.machineTime"), m.estimatedMachineTimeMinutes, m.actualMachineTimeMinutes),
    row("operator", t("plant.detail.operatorTime"), m.estimatedOperatorTimeMinutes, m.actualOperatorTimeMinutes),
  ];
});

const activities = computed(() => {
  const phase = workcenterStore.loadedWorkOrdersPhases?.[0]?.phases?.[0];
  const currentStatusId = workcenterStore.workcenterRt?.statusId;
  return [...(phase?.details ?? [])]
    .sort((a, b) => a.order - b.order)
    .map((detail, index) => ({
      id: detail.machineStatusId ?? String(index),
      name: detail.machineStatusName,
      color: normalizeColor(detail.machineStatusColor),
      estimated: fmtMinutes(detail.estimatedTime),
      current: !!detail.machineStatusId && detail.machineStatusId === currentStatusId,
    }));
});
</script>

<style scoped>
.phase-summary {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1.75rem;
  padding: 0.875rem 1.125rem;
}

.phase-summary__times {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.phase-summary__time-head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: 0.5rem;
}

.phase-summary__title {
  font-family: var(--font-condensed);
  font-size: 1rem;
  font-weight: 600;
  color: var(--p-steel-900);
}

.phase-summary__figures {
  font-size: 0.9375rem;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-700);
}

.phase-summary__figures strong {
  font-family: var(--font-condensed);
  font-size: 1.1875rem;
  color: var(--p-steel-900);
}

.phase-summary__bar {
  display: flex;
  height: 10px;
  margin-top: 0.5rem;
  border-radius: 5px;
  overflow: hidden;
  background: var(--p-steel-100);
}

.phase-summary__bar-done {
  background: var(--p-steel-900);
}

.phase-summary__bar-over {
  background: var(--p-amber-500);
}

.phase-summary__figures strong.phase-summary__over-text,
.phase-summary__over-text {
  display: block;
  margin-top: 0.375rem;
  font-size: 0.875rem;
  color: var(--p-amber-800);
}

.phase-summary__figures strong.phase-summary__over-text {
  display: inline;
  margin: 0;
  font-size: 1.1875rem;
}

.phase-summary__activities ul {
  margin: 0.25rem 0 0;
  padding: 0;
  list-style: none;
}

.phase-summary__activities li {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  min-height: 3rem;
  border-bottom: 1px solid var(--p-steel-100);
  font-size: 0.9375rem;
}

.phase-summary__swatch {
  flex-shrink: 0;
  width: 14px;
  height: 14px;
  border-radius: 3px;
}

.phase-summary__activity-name {
  flex: 1;
  color: var(--p-steel-900);
}

.phase-summary__current {
  font-family: var(--font-condensed);
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-steel-700);
}

.phase-summary__empty {
  margin: 0;
  font-size: 0.9375rem;
  line-height: 1.375rem;
  color: var(--p-steel-700);
}

.phase-summary__muted {
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-600);
}

@media (max-width: 767.98px) {
  .phase-summary {
    grid-template-columns: minmax(0, 1fr);
    gap: 1rem;
    padding: 0.75rem;
  }
}
</style>
