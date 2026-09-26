<template>
  <header class="plant-header">
    <Button
      v-if="store.currentMenuItem.backButtonVisible"
      :icon="PrimeIcons.ARROW_LEFT"
      severity="secondary"
      text
      class="plant-header__icon"
      :aria-label="t('ui.back')"
      @click="router.back()"
    />
    <div class="plant-header__heading">
      <span class="plant-header__module">{{ t("plant.titles.plant") }}</span>
      <h1 class="plant-header__title">{{ store.currentMenuItem.title }}</h1>
    </div>

    <div v-if="shift && !isPhone" class="plant-header__shift">
      <span class="plant-header__label">{{ shift.name }}</span>
      <span class="plant-header__value">{{ shift.hours }}</span>
    </div>
    <span class="plant-header__clock" :aria-label="t('plant.shell.clock')">{{
      clock
    }}</span>
    <Button
      v-if="helpKey"
      icon="pi pi-question-circle"
      severity="secondary"
      text
      class="plant-header__icon"
      :aria-label="t('help.actions.openTooltip')"
      @click="helpStore.toggleForRoute(helpKey)"
    />
    <button
      v-if="operator"
      type="button"
      class="plant-header__operator"
      :aria-label="t('plant.shell.operatorMenu', { name: operatorName })"
      aria-haspopup="dialog"
      :aria-expanded="menuOpen"
      @click="toggleMenu"
    >
      <span class="plant-header__avatar" aria-hidden="true">{{ initials }}</span>
      <span v-if="!isPhone" class="plant-header__name">{{ operatorName }}</span>
    </button>

    <Popover ref="menu" @show="menuOpen = true" @hide="menuOpen = false">
      <div class="plant-header__menu">
        <span class="plant-header__menu-name">{{ operatorName }}</span>
        <span class="plant-header__label">{{ t("ui.operator") }}</span>
        <Button
          icon="pi pi-sign-out"
          :label="t('ui.exit')"
          size="large"
          class="plant-header__exit"
          @click="exit"
        />
      </div>
    </Popover>
  </header>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import Popover from "primevue/popover";
import { useIsPhone } from "@/composables/useIsPhone";
import { useStore } from "@/store";
import { useHelpStore } from "@/store/help";
import { useNow } from "../composables/useNow";
import {
  usePlantOperatorStore,
  usePlantRealtimeStore,
  usePlantWorkcenterStore,
} from "../store";

// Shop-floor shell for a clocked-in operator: replaces the office header
// and sidebar on /plant with the screen title, shift, clock and the
// operator's menu.
const emit = defineEmits<{ (e: "exit"): void }>();

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const store = useStore();
const helpStore = useHelpStore();
const operatorStore = usePlantOperatorStore();
const realtimeStore = usePlantRealtimeStore();
const workcenterStore = usePlantWorkcenterStore();
const isPhone = useIsPhone();
const now = useNow();

const operator = computed(() => operatorStore.operator);
const operatorName = computed(() =>
  operator.value ? `${operator.value.name} ${operator.value.surname}`.trim() : "",
);
const initials = computed(() =>
  operatorName.value
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part.charAt(0).toUpperCase())
    .join(""),
);

const helpKey = computed(() =>
  typeof route.meta.helpKey === "string" ? route.meta.helpKey : undefined,
);

const clock = computed(() =>
  new Date(now.value).toLocaleTimeString(store.language.current || "ca", {
    hour: "2-digit",
    minute: "2-digit",
  }),
);

// The site's current shift, from any workcenter's realtime snapshot.
const shift = computed(() => {
  const source =
    workcenterStore.workcenterRt ??
    realtimeStore.areasWorkcentersRt.find((rt) => rt.shiftName);
  if (!source?.shiftName) return undefined;
  const hhmm = (time?: string) => (time ?? "").slice(0, 5);
  return {
    name: source.shiftName,
    hours: `${hhmm(source.shiftDetailStartTime)}–${hhmm(source.shiftDetailEndTime)}`,
  };
});

const menu = ref<InstanceType<typeof Popover>>();
const menuOpen = ref(false);
const toggleMenu = (event: Event) => menu.value?.toggle(event);
const exit = () => {
  menu.value?.hide();
  emit("exit");
};
</script>

<style scoped>
.plant-header {
  position: fixed;
  inset: 0 0 auto 0;
  z-index: 10;
  height: var(--top-panel-height);
  box-sizing: border-box;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0 1rem 0 0.5rem;
  background: var(--p-surface-0);
  border-bottom: 1px solid var(--p-steel-200);
}

.plant-header__icon {
  width: 48px;
  height: 48px;
  flex-shrink: 0;
}

.plant-header__heading {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  padding-left: 0.5rem;
}

.plant-header__module,
.plant-header__label {
  font-size: 0.8125rem;
  line-height: 1rem;
  color: var(--p-steel-600);
}

.plant-header__title {
  margin: 0;
  font-family: var(--font-condensed);
  font-size: 1.375rem;
  line-height: 1.75rem;
  font-weight: 600;
  color: var(--p-steel-900);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.plant-header__shift {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.plant-header__value {
  font-size: 0.9375rem;
  font-weight: 500;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-900);
}

.plant-header__clock {
  padding: 0 0.5rem;
  font-family: var(--font-condensed);
  font-size: 1.625rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-900);
}

.plant-header__operator {
  flex-shrink: 0;
  height: 48px;
  display: inline-flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0 0.75rem 0 0.375rem;
  border: none;
  border-radius: 24px;
  box-shadow: inset 0 0 0 1px var(--p-steel-300);
  background: var(--p-surface-0);
  font: inherit;
  color: var(--p-steel-900);
  cursor: pointer;
}

.plant-header__operator:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: 2px;
}

.plant-header__avatar {
  width: 36px;
  height: 36px;
  border-radius: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--p-primary-50);
  color: var(--p-primary-700);
  font-family: var(--font-condensed);
  font-weight: 600;
  font-size: 0.9375rem;
}

.plant-header__name {
  font-size: 0.9375rem;
}

.plant-header__menu {
  min-width: 16rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.plant-header__menu-name {
  font-family: var(--font-condensed);
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--p-steel-900);
}

.plant-header__exit {
  margin-top: 0.75rem;
  min-height: 56px;
}

@media (max-width: 767.98px) {
  .plant-header {
    gap: 0.25rem;
    padding-right: 0.5rem;
  }

  .plant-header__icon {
    width: 44px;
    height: 44px;
  }

  .plant-header__title {
    font-size: 1.125rem;
    line-height: 1.375rem;
  }

  .plant-header__clock {
    font-size: 1.25rem;
  }

  .plant-header__operator {
    width: 44px;
    height: 44px;
    padding: 0;
    justify-content: center;
    box-shadow: none;
  }
}
</style>
