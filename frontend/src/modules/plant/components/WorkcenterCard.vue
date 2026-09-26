<template>
  <!-- Phones: one row per machine. -->
  <button
    v-if="compact"
    type="button"
    class="wc-row"
    :aria-label="accessibleName"
    @click="handleClick"
  >
    <span class="wc-row__light" :style="lightStyle"></span>
    <span class="wc-row__text">
      <span class="wc-row__name">{{ workcenter.config.description }}</span>
      <span class="wc-row__line">{{ rowLine }}</span>
    </span>
    <span class="wc-time" :class="{ 'wc-time--none': signal.noData }">{{
      formattedTime
    }}</span>
  </button>

  <!-- Tablets: a tile that shows only what exists. -->
  <button
    v-else
    type="button"
    class="wc-tile"
    :aria-label="accessibleName"
    @click="handleClick"
  >
    <span class="wc-tile__band" :style="{ background: signal.band }"></span>
    <span class="wc-tile__body">
      <span class="wc-tile__name">{{ workcenter.config.description }}</span>
      <span class="wc-tile__status">
        <span
          class="wc-signal"
          :class="{ 'wc-signal--none': signal.noData }"
          :style="signal.style"
          >{{ statusName }}</span
        >
        <span class="wc-time" :class="{ 'wc-time--none': signal.noData }">{{
          formattedTime
        }}</span>
      </span>
      <span v-if="currentWorkOrder" class="wc-tile__order">
        <span class="wc-tile__of">
          {{ orderPhase }}
          <span v-if="extraOrders" class="wc-tile__more">{{
            t("plant.workcenterTile.moreOrders", { count: extraOrders })
          }}</span>
        </span>
        <span
          v-if="currentWorkOrder.workOrderPhaseDescription"
          class="wc-tile__phase"
          >{{ currentWorkOrder.workOrderPhaseDescription }}</span
        >
        <span class="wc-tile__reference">{{ formattedReference }}</span>
      </span>
      <span v-if="currentWorkOrder || operators.length" class="wc-tile__foot">
        <span v-if="currentWorkOrder" class="wc-tile__planned">{{
          t(
            "plant.workcenterTile.planned",
            { count: currentWorkOrder.plannedQuantity },
            currentWorkOrder.plannedQuantity,
          )
        }}</span>
        <span class="wc-tile__operators">
          <span
            v-for="operator in operators"
            :key="operator.id"
            class="wc-avatar"
            :title="operator.name"
            >{{ operator.initials }}</span
          >
        </span>
      </span>
    </span>
  </button>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { WorkcenterViewState } from "../types";
import { statusSignal } from "../utils/statusSignal";
import { usePlantDataStore } from "../store";
import { useNow } from "../composables/useNow";
import { NO_TIME, elapsedSeconds, formatElapsed } from "../utils/elapsed";

interface Props {
  workcenter: WorkcenterViewState;
  /** Phone row instead of the tablet tile. */
  compact?: boolean;
}

const props = withDefaults(defineProps<Props>(), { compact: false });

const emit = defineEmits<{
  (e: "click", workcenterId: string): void;
}>();

const dataStore = usePlantDataStore();
const { t } = useI18n();
const now = useNow();

const currentMachineStatus = computed(() => {
  const statusId = props.workcenter.realtime?.statusId;
  if (!statusId) return undefined;
  return dataStore.getMachineStatusById(statusId);
});

// Solid status signal: band and badge in the status colour, hatched with no data.
const signal = computed(() => statusSignal(currentMachineStatus.value?.color));

const lightStyle = computed(() => ({
  background: signal.value.band,
  boxShadow: signal.value.noData ? "inset 0 0 0 1px var(--p-steel-300)" : undefined,
}));

const statusName = computed(
  () => currentMachineStatus.value?.name || t("plant.workcenterTile.noData"),
);

const workorders = computed(() => props.workcenter.realtime?.workorders ?? []);
const currentWorkOrder = computed(() => workorders.value[0]);
const extraOrders = computed(() => Math.max(0, workorders.value.length - 1));

const orderPhase = computed(() => {
  const wo = currentWorkOrder.value;
  return wo
    ? t("plant.workcenterTile.orderPhase", {
        order: wo.workOrderCode,
        phase: wo.workOrderPhaseCode,
      })
    : "";
});

const formattedReference = computed(() => {
  const wo = currentWorkOrder.value;
  if (!wo) return "";
  return wo.referenceDescription && wo.referenceDescription !== wo.referenceCode
    ? `${wo.referenceCode} - ${wo.referenceDescription}`
    : wo.referenceCode;
});

const operators = computed(() =>
  (props.workcenter.realtime?.operators ?? []).map((op) => {
    const name = `${op.operatorName} ${op.operatorSurname ?? ""}`.trim();
    const initials = name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join("");
    return { id: op.operatorId, name, initials };
  }),
);

// Time in the current status, ticking on the device between messages.
const formattedTime = computed((): string =>
  signal.value.noData
    ? NO_TIME
    : formatElapsed(
        elapsedSeconds(props.workcenter.realtime?.statusStartTime, now.value),
      ),
);

const rowLine = computed(() =>
  currentWorkOrder.value
    ? `${statusName.value} · ${orderPhase.value}`
    : statusName.value,
);

const accessibleName = computed(() =>
  t("plant.workcenterTile.label", {
    name: props.workcenter.config.description,
    status: statusName.value,
    time: formattedTime.value,
  }) + (currentWorkOrder.value ? `, ${orderPhase.value}` : ""),
);

const handleClick = () => {
  emit("click", props.workcenter.config.id);
};
</script>

<style scoped>
.wc-tile,
.wc-row {
  box-sizing: border-box;
  width: 100%;
  border: none;
  font: inherit;
  color: var(--p-steel-900);
  text-align: left;
  background: var(--p-surface-0);
  cursor: pointer;
}

.wc-tile:focus-visible,
.wc-row:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: 2px;
}

/* Tile */
.wc-tile {
  display: flex;
  flex-direction: column;
  padding: 0;
  border-radius: 6px;
  overflow: hidden;
  box-shadow: inset 0 0 0 1px var(--p-steel-200);
}

.wc-tile:hover {
  box-shadow: inset 0 0 0 1px var(--p-steel-400);
}

.wc-tile__band {
  display: block;
  height: 8px;
}

.wc-tile__body {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
  padding: 0.75rem 0.875rem 0.875rem;
}

.wc-tile__name {
  font-family: var(--font-condensed);
  font-size: 1.1875rem;
  line-height: 1.4375rem;
  font-weight: 600;
}

.wc-tile__status {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}

.wc-signal {
  display: inline-flex;
  align-items: center;
  min-height: 1.625rem;
  padding: 0 0.5625rem;
  border-radius: 4px;
  font-family: var(--font-condensed);
  font-size: 0.875rem;
  font-weight: 600;
  white-space: nowrap;
}

.wc-signal--none {
  font-weight: 500;
}

.wc-time {
  font-family: var(--font-condensed);
  font-size: 1.375rem;
  line-height: 1.625rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.wc-time--none {
  color: var(--p-steel-500);
}

.wc-tile__order {
  display: flex;
  flex-direction: column;
  gap: 0.1875rem;
  padding-top: 0.625rem;
  border-top: 1px solid var(--p-steel-100);
}

.wc-tile__of {
  font-family: var(--font-condensed);
  font-size: 0.9375rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.wc-tile__more {
  margin-left: 0.25rem;
  color: var(--p-steel-600);
  font-weight: 500;
}

.wc-tile__phase {
  font-size: 0.9375rem;
  line-height: 1.25rem;
}

.wc-tile__reference {
  font-size: 0.8125rem;
  line-height: 1.125rem;
  color: var(--p-steel-600);
}

.wc-tile__foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  font-size: 0.8125rem;
  color: var(--p-steel-700);
}

.wc-tile__operators {
  display: flex;
  gap: 0.25rem;
  margin-left: auto;
}

.wc-avatar {
  width: 1.625rem;
  height: 1.625rem;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--p-steel-100);
  color: var(--p-steel-700);
  font-family: var(--font-condensed);
  font-size: 0.75rem;
  font-weight: 600;
}

/* Phone row */
.wc-row {
  min-height: 72px;
  display: grid;
  grid-template-columns: 44px minmax(0, 1fr) auto;
  align-items: center;
  gap: 0.75rem;
  padding: 0.625rem 0.875rem;
  border-bottom: 1px solid var(--p-steel-100);
}

.wc-row__light {
  width: 44px;
  height: 44px;
  border-radius: 6px;
}

.wc-row__text {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.125rem;
}

.wc-row__name {
  font-family: var(--font-condensed);
  font-size: 1.0625rem;
  line-height: 1.3125rem;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.wc-row__line {
  font-size: 0.875rem;
  line-height: 1.1875rem;
  color: var(--p-steel-700);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.wc-row .wc-time {
  font-size: 1.125rem;
}
</style>
