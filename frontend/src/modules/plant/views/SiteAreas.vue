<template>
  <main class="plant-areas">
    <div
      class="plant-areas__toolbar"
      role="toolbar"
      :aria-label="t('plant.areas.filterLabel')"
    >
      <button
        v-for="filter in statusFilters"
        :key="filter.id"
        type="button"
        class="plant-chip"
        :class="{ 'plant-chip--on': statusFilter === filter.id }"
        :aria-pressed="statusFilter === filter.id"
        @click="statusFilter = filter.id"
      >
        {{ filter.label }}
        <span class="plant-chip__count">{{ filter.count }}</span>
      </button>
      <span class="plant-areas__spacer"></span>
      <button
        type="button"
        role="switch"
        class="plant-chip plant-chip--switch"
        :aria-checked="showOnlyMyWorkcenters"
        @click="toggleMyWorkcenters"
      >
        <span
          class="plant-switch"
          :class="{ 'plant-switch--on': showOnlyMyWorkcenters }"
          aria-hidden="true"
        ></span>
        {{ t("plant.areas.onlyMine") }}
      </button>
    </div>

    <section
      v-for="area in areaSections"
      :key="area.id"
      class="plant-area"
      :aria-labelledby="`area-title-${area.id}`"
    >
      <h2 :id="`area-title-${area.id}`" class="plant-area__title">
        <button
          type="button"
          class="plant-area__toggle"
          :aria-expanded="area.expanded"
          :aria-controls="`area-${area.id}`"
          @click="toggleArea(area.id)"
        >
          <span class="plant-area__name">{{ area.name }}</span>
          <span class="plant-area__count">{{
            t("plant.areas.workcenterCount", { count: area.total })
          }}</span>
          <span class="plant-area__rule" aria-hidden="true"></span>
          <span class="plant-area__lights" role="img" :aria-label="area.summary">
            <span
              v-for="light in area.lights"
              :key="light.id"
              class="plant-area__light"
              :style="light.style"
            ></span>
          </span>
          <i
            class="pi plant-area__chevron"
            :class="area.expanded ? 'pi-chevron-up' : 'pi-chevron-down'"
            aria-hidden="true"
          ></i>
        </button>
      </h2>
      <div
        v-show="area.expanded"
        :id="`area-${area.id}`"
        :class="isPhone ? 'plant-area__list' : 'plant-area__grid'"
      >
        <WorkcenterCard
          v-for="view in area.workcenters"
          :key="view.config.id"
          :workcenter="view"
          :compact="isPhone"
          @click="(id: string) => $router.push(`/plant/workcenter/${id}`)"
        />
      </div>
    </section>

    <p v-if="areaSections.length === 0" class="plant-areas__empty">
      {{ t("plant.areas.empty") }}
    </p>
  </main>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import { useIsPhone } from "@/composables/useIsPhone";
import {
  usePlantDataStore,
  usePlantRealtimeStore,
  usePlantOperatorStore,
} from "../store";
import { WorkcenterViewState } from "../types";
import WorkcenterCard from "../components/WorkcenterCard.vue";
import { useStore } from "../../../store";
import {
  useWebSocketConnection,
  WS_ENDPOINTS,
} from "../composables/useWebSocketConnection";
import { statusSignal } from "../utils/statusSignal";

type StatusGroup = "run" | "stop" | "none";
type StatusFilter = "all" | StatusGroup;

const STORAGE_KEY = "temges.plant-visible-areas";
const FILTER_STORAGE_KEY = "temges.plant-filter-my-workcenters";

const store = useStore();
const { t } = useI18n();
const isPhone = useIsPhone();
const dataStore = usePlantDataStore();
const realtimeStore = usePlantRealtimeStore();
const operatorStore = usePlantOperatorStore();
const visibleAreas = ref<Set<string>>(new Set());
const showOnlyMyWorkcenters = ref(false);
const statusFilter = ref<StatusFilter>("all");
const { connect } = useWebSocketConnection();

const readStored = <T,>(key: string, fallback: T): T => {
  try {
    const stored = localStorage.getItem(key);
    return stored ? (JSON.parse(stored) as T) : fallback;
  } catch {
    return fallback;
  }
};

const writeStored = (key: string, value: unknown): void => {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch {
    // Storage full or blocked: preferences are a convenience only.
  }
};

onMounted(async () => {
  visibleAreas.value = new Set(readStored<string[]>(STORAGE_KEY, []));
  showOnlyMyWorkcenters.value = readStored<boolean>(FILTER_STORAGE_KEY, false);

  await Promise.all([
    dataStore.fetchAreasWithWorkcenters(),
    dataStore.fetchMachineStatuses(),
  ]);

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    title: t("plant.titles.siteAreas", {
      siteName: dataStore.site?.name || t("plant.titles.plant"),
    }),
  });

  realtimeStore.connectToGeneral();
  connect(WS_ENDPOINTS.GENERAL, { debug: true });
});

onUnmounted(() => {
  writeStored(STORAGE_KEY, Array.from(visibleAreas.value));
  writeStored(FILTER_STORAGE_KEY, showOnlyMyWorkcenters.value);
});

const toggleArea = (areaId: string) => {
  if (visibleAreas.value.has(areaId)) {
    visibleAreas.value.delete(areaId);
  } else {
    visibleAreas.value.add(areaId);
  }
};

const toggleMyWorkcenters = () => {
  showOnlyMyWorkcenters.value = !showOnlyMyWorkcenters.value;
  writeStored(FILTER_STORAGE_KEY, showOnlyMyWorkcenters.value);
};

const currentOperatorId = computed(() => operatorStore.operator?.id);

const isMyWorkcenter = (view: WorkcenterViewState): boolean =>
  !!currentOperatorId.value &&
  (view.realtime?.operators?.some(
    (op) => op.operatorId === currentOperatorId.value,
  ) ??
    false);

// Running and stopped come from the status flags; a status missing from
// the catalogue counts as no data, as the tile shows it.
const statusGroup = (view: WorkcenterViewState): StatusGroup => {
  const rt = view.realtime;
  if (!rt?.statusId || !dataStore.getMachineStatusById(rt.statusId)) return "none";
  return rt.statusStopped || rt.statusClosed ? "stop" : "run";
};

const scope = computed(() =>
  realtimeStore.areasWorkcentersView.filter(
    (view) => !showOnlyMyWorkcenters.value || isMyWorkcenter(view),
  ),
);

const statusFilters = computed(() => {
  const count = (group: StatusFilter) =>
    group === "all"
      ? scope.value.length
      : scope.value.filter((view) => statusGroup(view) === group).length;
  const labels: Record<StatusFilter, string> = {
    all: t("plant.areas.all"),
    run: t("plant.areas.running"),
    stop: t("plant.areas.stopped"),
    none: t("plant.areas.noData"),
  };
  return (["all", "run", "stop", "none"] as StatusFilter[]).map((id) => ({
    id,
    label: labels[id],
    count: count(id),
  }));
});

const statusColor = (view: WorkcenterViewState) => {
  const statusId = view.realtime?.statusId;
  return statusId ? dataStore.getMachineStatusById(statusId)?.color : undefined;
};

const areaSections = computed(() =>
  dataStore.areas
    .map((area) => {
      const all = realtimeStore.areasWorkcentersView.filter(
        (view) => view.config.areaId === area.id,
      );
      const workcenters = scope.value.filter(
        (view) =>
          view.config.areaId === area.id &&
          (statusFilter.value === "all" ||
            statusGroup(view) === statusFilter.value),
      );
      const groups = all.map(statusGroup);
      return {
        id: area.id,
        name: area.name,
        total: all.length,
        workcenters,
        // A status filter opens every area that has a match.
        expanded: statusFilter.value !== "all" || visibleAreas.value.has(area.id),
        summary: t("plant.areas.summary", {
          run: groups.filter((g) => g === "run").length,
          stop: groups.filter((g) => g === "stop").length,
          none: groups.filter((g) => g === "none").length,
        }),
        lights: all.map((view) => {
          const signal = statusSignal(statusColor(view));
          return {
            id: view.config.id,
            style: {
              background: signal.band,
              boxShadow: signal.noData
                ? "inset 0 0 0 1px var(--p-steel-300)"
                : undefined,
            },
          };
        }),
      };
    })
    .filter((area) => area.workcenters.length > 0),
);
</script>

<style scoped>
.plant-areas {
  display: flex;
  flex-direction: column;
  padding-bottom: 1.5rem;
}

.plant-areas__toolbar {
  position: sticky;
  top: 0;
  z-index: 2;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
  padding: 0.75rem;
  overflow-x: auto;
  scrollbar-width: none;
  background: var(--p-surface-0);
  border: 1px solid var(--p-steel-200);
  border-radius: 6px;
}

.plant-areas__toolbar::-webkit-scrollbar {
  display: none;
}

.plant-areas__spacer {
  flex: 1;
}

.plant-chip {
  flex-shrink: 0;
  min-height: 44px;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0 0.875rem;
  border: none;
  border-radius: 22px;
  box-shadow: inset 0 0 0 1px var(--p-steel-300);
  background: var(--p-surface-0);
  font: inherit;
  font-size: 0.9375rem;
  font-weight: 500;
  color: var(--p-steel-900);
  cursor: pointer;
  white-space: nowrap;
}

.plant-chip:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: 2px;
}

.plant-chip--on {
  box-shadow: none;
  background: var(--p-steel-900);
  color: var(--p-surface-0);
}

.plant-chip__count {
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-600);
}

.plant-chip--on .plant-chip__count {
  color: inherit;
  opacity: 0.8;
}

.plant-chip--switch {
  padding-left: 0.5rem;
}

.plant-switch {
  position: relative;
  width: 40px;
  height: 24px;
  border-radius: 12px;
  background: var(--p-steel-300);
  transition: background-color 0.15s;
}

.plant-switch::after {
  content: "";
  position: absolute;
  top: 2px;
  left: 2px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--p-surface-0);
  transition: transform 0.15s;
}

.plant-switch--on {
  background: var(--p-primary-color);
}

.plant-switch--on::after {
  transform: translateX(16px);
}

.plant-area__title {
  margin: 0;
}

.plant-area__toggle {
  width: 100%;
  min-height: 60px;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0;
  border: none;
  background: none;
  font: inherit;
  color: var(--p-steel-900);
  text-align: left;
  cursor: pointer;
}

.plant-area__toggle:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: 2px;
}

.plant-area__name {
  font-family: var(--font-condensed);
  font-size: 1.25rem;
  font-weight: 600;
}

.plant-area__count {
  font-size: 0.875rem;
  font-weight: 400;
  color: var(--p-steel-600);
  white-space: nowrap;
}

.plant-area__rule {
  flex: 1;
  height: 1px;
  background: var(--p-steel-200);
}

.plant-area__lights {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 4px;
  max-width: 40%;
}

.plant-area__light {
  width: 14px;
  height: 14px;
  border-radius: 3px;
}

.plant-area__chevron {
  width: 44px;
  text-align: center;
  color: var(--p-steel-700);
}

.plant-area__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(17rem, 1fr));
  align-items: start;
  gap: 0.75rem;
  padding-bottom: 0.5rem;
}

.plant-area__list {
  border: 1px solid var(--p-steel-200);
  border-radius: 6px;
  overflow: hidden;
}

.plant-areas__empty {
  margin: 3rem 0;
  text-align: center;
  font-size: 1rem;
  color: var(--p-steel-700);
}

@media (max-width: 767.98px) {
  .plant-areas__toolbar {
    padding: 0.5rem;
  }

  .plant-area__toggle {
    min-height: 48px;
    gap: 0.5rem;
  }

  .plant-area__name {
    font-size: 1.0625rem;
  }

  .plant-area__rule {
    display: none;
  }

  .plant-area__lights {
    margin-left: auto;
  }

  .plant-area__light {
    width: 12px;
    height: 12px;
  }
}
</style>
