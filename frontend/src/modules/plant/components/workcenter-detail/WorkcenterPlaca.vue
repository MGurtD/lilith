<template>
  <section
    class="placa"
    :class="{ 'placa--compact': compact }"
    :aria-label="t('plant.placa.label')"
  >
    <span class="placa__rivet placa__rivet--tl" aria-hidden="true"></span>
    <span class="placa__rivet placa__rivet--tr" aria-hidden="true"></span>
    <span
      v-if="!compact"
      class="placa__rivet placa__rivet--bl"
      aria-hidden="true"
    ></span>
    <span
      v-if="!compact"
      class="placa__rivet placa__rivet--br"
      aria-hidden="true"
    ></span>

    <!-- Status and time: always the loudest thing on the screen. -->
    <div class="placa__status" role="status" :style="signal.style">
      <div class="placa__status-head">
        <div class="placa__status-name">
          <i v-if="status?.icon" :class="status.icon" aria-hidden="true"></i>
          <span>{{ statusName }}</span>
        </div>
        <span v-if="reasonName" class="placa__reason">{{
          t("plant.placa.reason", { reason: reasonName })
        }}</span>
      </div>
      <div class="placa__timer-block">
        <span v-if="!compact" class="placa__timer-label">{{
          t("plant.placa.timeInStatus")
        }}</span>
        <span class="placa__timer">{{ timer }}</span>
        <span v-if="since" class="placa__since">{{
          t("plant.placa.since", { time: since })
        }}</span>
      </div>
    </div>

    <!-- Order, phase and pieces. -->
    <div class="placa__order">
      <template v-if="order">
        <div class="placa__cell placa__cell--code">
          <span class="placa__label">{{ t("plant.placa.order") }}</span>
          <span class="placa__code">{{ order.code }}</span>
        </div>
        <div class="placa__cell">
          <span class="placa__label">{{ t("plant.placa.phase") }}</span>
          <span class="placa__value">{{ order.phase }}</span>
        </div>
        <div v-if="!compact" class="placa__cell placa__cell--code">
          <span class="placa__label">{{ t("plant.placa.customer") }}</span>
          <span class="placa__value">{{ order.customer || "—" }}</span>
        </div>
        <div class="placa__cell" :class="{ 'placa__cell--wide': compact }">
          <span class="placa__label">{{ t("plant.placa.reference") }}</span>
          <span class="placa__value">{{
            compact && order.customer
              ? `${order.reference} · ${order.customer}`
              : order.reference
          }}</span>
        </div>
        <div class="placa__cell placa__cell--wide placa__pieces">
          <div class="placa__counts">
            <span
              ><span class="placa__label">{{ t("plant.placa.good") }}</span>
              <strong class="placa__count">{{ order.good }}</strong>
              <span class="placa__muted">{{
                t("plant.placa.ofPlanned", { count: order.planned })
              }}</span></span
            >
            <span
              ><span class="placa__label">{{ t("plant.placa.bad") }}</span>
              <strong class="placa__count">{{ order.bad }}</strong></span
            >
            <span v-if="!compact && order.remaining > 0" class="placa__remaining">{{
              t("plant.placa.remaining", { count: order.remaining })
            }}</span>
          </div>
          <div class="placa__bar" aria-hidden="true">
            <span class="placa__bar-good" :style="{ width: order.goodPct }"></span>
            <span class="placa__bar-bad" :style="{ width: order.badPct }"></span>
          </div>
        </div>
      </template>
      <div v-else class="placa__empty">
        <strong>{{ t("plant.placa.noPhase") }}</strong>
        <span>{{ t("plant.placa.noPhaseHint") }}</span>
      </div>
    </div>

    <!-- Who works on the machine. -->
    <div class="placa__operators">
      <span v-if="!compact" class="placa__label">{{
        t("plant.placa.operators")
      }}</span>
      <ul v-if="operators.length" class="placa__operator-list">
        <li v-for="operator in operators" :key="operator.id" class="placa__operator">
          <span class="placa__avatar" aria-hidden="true">{{ operator.initials }}</span>
          <span class="placa__operator-name">
            <span>{{ operator.name }}</span>
            <span class="placa__muted">{{ operator.type }}</span>
          </span>
          <span class="placa__operator-time">{{ operator.time }}</span>
        </li>
      </ul>
      <span v-else class="placa__muted">{{ t("plant.placa.nobody") }}</span>
      <Button
        :icon="clockedIn ? PrimeIcons.SIGN_OUT : PrimeIcons.SIGN_IN"
        :label="clockedIn ? t('plant.placa.leave') : t('plant.placa.enter')"
        outlined
        class="placa__clock"
        :disabled="!canManageOperators"
        @click="clockedIn ? emit('clock-out') : emit('clock-in')"
      />
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { WorkcenterViewState } from "../../types";
import {
  usePlantDataStore,
  usePlantOperatorStore,
  usePlantWorkcenterStore,
} from "../../store";
import { useNow } from "../../composables/useNow";
import { statusSignal } from "../../utils/statusSignal";
import {
  elapsedSeconds,
  formatElapsed,
  formatElapsedClock,
} from "../../utils/elapsed";

// The placa: the machine's identification plate. Status and time on the
// left, order and pieces in the middle, operators on the right; stacked
// on phones (plant-mes-redesign.md).
interface Props {
  workcenter: WorkcenterViewState;
  compact?: boolean;
}

const props = withDefaults(defineProps<Props>(), { compact: false });
const emit = defineEmits<{
  (e: "clock-in"): void;
  (e: "clock-out"): void;
}>();

const { t, locale } = useI18n();
const dataStore = usePlantDataStore();
const operatorStore = usePlantOperatorStore();
const workcenterStore = usePlantWorkcenterStore();
const now = useNow();

const realtime = computed(() => props.workcenter.realtime);

const status = computed(() => {
  const statusId = realtime.value?.statusId;
  return statusId ? dataStore.getMachineStatusById(statusId) : undefined;
});

const signal = computed(() => statusSignal(status.value?.color));

const statusName = computed(
  () => status.value?.name || t("plant.workcenterTile.noData"),
);

const reasonName = computed(() => {
  const reasonId = realtime.value?.statusReasonId;
  return reasonId
    ? status.value?.reasons?.find((reason) => reason.id === reasonId)?.name
    : undefined;
});

const statusSeconds = computed(() =>
  status.value ? elapsedSeconds(realtime.value?.statusStartTime, now.value) : null,
);

const timer = computed(() =>
  props.compact
    ? formatElapsedClock(statusSeconds.value).replace(/^0(\d):/, "$1:")
    : formatElapsedClock(statusSeconds.value),
);

const since = computed(() => {
  if (statusSeconds.value === null || !realtime.value?.statusStartTime) return "";
  return new Date(realtime.value.statusStartTime).toLocaleTimeString(locale.value, {
    hour: "2-digit",
    minute: "2-digit",
  });
});

const order = computed(() => {
  const active = realtime.value?.workorders?.[0];
  if (!active) return undefined;
  const loaded = workcenterStore.loadedWorkOrdersPhases?.[0];
  const phase = loaded?.phases.find((p) => p.phaseId === active.workOrderPhaseId);
  const planned = loaded?.plannedQuantity ?? active.plannedQuantity ?? 0;
  const good = phase?.quantityOk ?? 0;
  const bad = phase?.quantityKo ?? 0;
  const pct = (value: number) =>
    planned > 0 ? `${Math.min(100, Math.round((value / planned) * 100))}%` : "0%";
  const description = phase?.phaseDescription ?? active.workOrderPhaseDescription;
  return {
    code: loaded?.workOrderCode ?? active.workOrderCode,
    phase: description
      ? `${phase?.phaseCode ?? active.workOrderPhaseCode} · ${description}`
      : (phase?.phaseCode ?? active.workOrderPhaseCode),
    customer: loaded?.customerName ?? "",
    reference:
      loaded?.salesReferenceDisplay ??
      (active.referenceDescription
        ? `${active.referenceCode} - ${active.referenceDescription}`
        : active.referenceCode),
    planned,
    good,
    bad,
    remaining: Math.max(0, planned - good),
    goodPct: pct(good),
    badPct: pct(bad),
  };
});

const operators = computed(() =>
  (realtime.value?.operators ?? []).map((op) => {
    const name = `${op.operatorName} ${op.operatorSurname ?? ""}`.trim();
    return {
      id: op.operatorId,
      name,
      type: op.operatorTypeName,
      initials: name
        .split(/\s+/)
        .slice(0, 2)
        .map((part) => part.charAt(0).toUpperCase())
        .join(""),
      time: formatElapsed(elapsedSeconds(op.operatorStartTime, now.value)),
    };
  }),
);

const clockedIn = computed(
  () =>
    !!operatorStore.operator &&
    operators.value.some((op) => op.id === operatorStore.operator!.id),
);

const canManageOperators = computed(
  () => realtime.value?.statusOperatorsAllowed === true,
);
</script>

<style scoped>
.placa {
  position: relative;
  display: grid;
  grid-template-columns: 17.5rem minmax(0, 1fr) 15.5rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-steel-700);
  border-radius: 3px;
  overflow: hidden;
}

.placa__rivet {
  position: absolute;
  z-index: 1;
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--p-steel-100);
  box-shadow: inset 0 0 0 1px var(--p-steel-400);
}

.placa__rivet--tl { left: 5px; top: 5px; }
.placa__rivet--tr { right: 5px; top: 5px; }
.placa__rivet--bl { left: 5px; bottom: 5px; }
.placa__rivet--br { right: 5px; bottom: 5px; }

.placa__status {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 0.75rem;
  padding: 1rem 1.125rem 0.875rem;
  border-right: 1px solid var(--p-steel-700);
}

.placa__status-head {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.placa__status-name {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  font-family: var(--font-condensed);
  font-size: 1.625rem;
  line-height: 1.875rem;
  font-weight: 600;
}

.placa__status-name i {
  font-size: 1.5rem;
}

.placa__reason,
.placa__since,
.placa__timer-label {
  font-size: 0.875rem;
  opacity: 0.85;
}

.placa__timer-label {
  font-family: var(--font-condensed);
  font-weight: 500;
}

.placa__timer-block {
  display: flex;
  flex-direction: column;
}

.placa__timer {
  font-family: var(--font-condensed);
  font-size: 3.375rem;
  line-height: 3.5rem;
  font-weight: 600;
  letter-spacing: -0.5px;
  font-variant-numeric: tabular-nums;
}

.placa__order {
  display: grid;
  grid-template-columns: 9.5rem minmax(0, 1fr);
  align-content: start;
  border-right: 1px solid var(--p-steel-200);
}

.placa__cell {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.125rem;
  padding: 0.5625rem 0.875rem;
  border-bottom: 1px solid var(--p-steel-200);
}

.placa__cell--code {
  border-right: 1px solid var(--p-steel-200);
}

.placa__cell--wide {
  grid-column: span 2;
}

.placa__pieces {
  gap: 0.5rem;
  padding-bottom: 0.75rem;
  border-bottom: none;
}

.placa__label {
  font-family: var(--font-condensed);
  font-size: 0.8125rem;
  font-weight: 500;
  color: var(--p-steel-600);
}

.placa__value {
  font-size: 1rem;
  line-height: 1.375rem;
  font-weight: 500;
  color: var(--p-steel-900);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.placa__code {
  font-family: var(--font-condensed);
  font-size: 1.5rem;
  line-height: 1.75rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-900);
}

.placa__counts {
  display: flex;
  align-items: baseline;
  gap: 1.25rem;
}

.placa__count {
  margin: 0 0.25rem;
  font-family: var(--font-condensed);
  font-size: 1.375rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-900);
}

.placa__muted {
  font-size: 0.875rem;
  color: var(--p-steel-600);
}

.placa__remaining {
  margin-left: auto;
  font-size: 0.875rem;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-700);
}

.placa__bar {
  display: flex;
  height: 8px;
  border-radius: 4px;
  overflow: hidden;
  background: var(--p-steel-100);
}

.placa__bar-good {
  background: var(--p-steel-900);
}

.placa__bar-bad {
  background: var(--p-red-700);
}

.placa__empty {
  grid-column: span 2;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 0.375rem;
  padding: 1rem 1.25rem;
}

.placa__empty strong {
  font-family: var(--font-condensed);
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--p-steel-900);
}

.placa__empty span {
  max-width: 24rem;
  font-size: 0.9375rem;
  line-height: 1.375rem;
  color: var(--p-steel-700);
}

.placa__operators {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.625rem 0.875rem 0.75rem;
}

.placa__operator-list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.placa__operator {
  display: flex;
  align-items: center;
  gap: 0.625rem;
}

.placa__avatar {
  flex-shrink: 0;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--p-steel-100);
  color: var(--p-steel-700);
  font-family: var(--font-condensed);
  font-size: 0.875rem;
  font-weight: 600;
}

.placa__operator-name {
  min-width: 0;
  display: flex;
  flex-direction: column;
  font-size: 0.9375rem;
  font-weight: 500;
  color: var(--p-steel-900);
}

.placa__operator-name > span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.placa__operator-time {
  margin-left: auto;
  font-family: var(--font-condensed);
  font-size: 1.0625rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
  color: var(--p-steel-900);
}

.placa__clock {
  margin-top: auto;
  min-height: 48px;
}

/* Phones: the plate stacks, status on top. */
.placa--compact {
  grid-template-columns: minmax(0, 1fr);
}

.placa--compact .placa__status {
  flex-direction: row;
  align-items: center;
  padding: 0.75rem 1rem 0.75rem 1.125rem;
  border-right: none;
  border-bottom: 1px solid var(--p-steel-700);
}

.placa--compact .placa__status-name {
  font-size: 1.375rem;
  line-height: 1.625rem;
}

.placa--compact .placa__timer-block {
  align-items: flex-end;
}

.placa--compact .placa__timer {
  font-size: 2.375rem;
  line-height: 2.5rem;
}

.placa--compact .placa__order {
  grid-template-columns: 6rem minmax(0, 1fr);
  border-right: none;
  border-bottom: 1px solid var(--p-steel-200);
}

.placa--compact .placa__operators {
  flex-direction: row;
  flex-wrap: wrap;
  align-items: center;
}

.placa--compact .placa__operator-list {
  flex: 1;
  min-width: 0;
}

.placa--compact .placa__clock {
  flex-basis: 100%;
  margin-top: 0;
  min-height: 44px;
}
</style>
