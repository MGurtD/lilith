<template>
  <header class="title-bar" :class="{ collapsed: store.sidebar.collapsed }">
    <div class="title-bar__page">
      <Button
        v-if="isPhone"
        icon="pi pi-bars"
        severity="secondary"
        text
        rounded
        :aria-label="t('ui.openMenu')"
        aria-haspopup="dialog"
        :aria-expanded="store.sidebar.mobileOpen"
        @click="store.sidebar.mobileOpen = true"
      />
      <Button
        v-if="store.currentMenuItem.backButtonVisible"
        :icon="PrimeIcons.ARROW_LEFT"
        severity="secondary"
        text
        rounded
        :aria-label="t('ui.back')"
        @click="goBack"
      />
      <div class="title-bar__heading">
        <span v-if="moduleTitle" class="title-bar__module">{{ moduleTitle }}</span>
        <h1 class="title-bar__title">{{ store.currentMenuItem.title }}</h1>
      </div>
    </div>
    <div class="title-bar__right">
      <!-- Screens teleport their page-level actions (Save…) here. -->
      <div id="page-actions" class="title-bar__actions"></div>
      <MenuSearchTrigger />
      <Button
        v-if="helpKey"
        icon="pi pi-question-circle"
        severity="secondary"
        text
        rounded
        :aria-label="t('help.actions.openTooltip')"
        v-tooltip.bottom="t('help.actions.openTooltip')"
        @click="helpStore.toggleForRoute(helpKey)"
      />
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import MenuSearchTrigger from "@/components/menu-search/MenuSearchTrigger.vue";
import { useIsPhone } from "@/composables/useIsPhone";
import { useStore } from "@/store";
import { useHelpStore } from "@/store/help";
import type { MenuItem } from "@/types/component";
import { ownsRoute } from "@/utils/menuSearch";

const store = useStore();
const isPhone = useIsPhone();

const { t } = useI18n();
const helpStore = useHelpStore();
const route = useRoute();
const router = useRouter();
const goBack = () => router.back();

const helpKey = computed(() =>
  typeof route.meta.helpKey === "string" ? route.meta.helpKey : undefined,
);

// Module that owns the current screen: the top-level sidebar entry whose menu
// tree (any depth) holds the route. Detail routes (/customers/:id) belong to
// their list entry (/customers).
const moduleTitle = computed<string | undefined>(() => {
  const owner = store.sidebar.menus.find(
    (module: MenuItem) => !module.href && ownsRoute(module, route.path),
  );
  return owner?.title;
});
</script>

<style scoped>
.title-bar {
  position: fixed;
  top: 0;
  left: var(--side-bar-width);
  width: calc(100vw - var(--side-bar-width));
  height: var(--top-panel-height);
  box-sizing: border-box;
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0 1.5rem 0 1rem;
  background-color: var(--p-surface-0);
  border-bottom: 1px solid var(--p-surface-200);
  color: var(--p-text-color);
  transition: all 0.3s ease-in-out;
}

.collapsed {
  left: calc(var(--side-bar-collapsed-width) + var(--collapsed-side-padding));
  width: calc(
    100vw - var(--side-bar-collapsed-width) - var(--collapsed-side-padding)
  );
}

/* Phones: navigation lives in a drawer, so the header spans the screen. */
@media (max-width: 767.98px) {
  .title-bar,
  .title-bar.collapsed {
    left: 0;
    width: 100vw;
    gap: 0.5rem;
    padding: 0 0.75rem 0 0.5rem;
  }

  .title-bar__heading {
    padding-left: 0.25rem;
  }

  .title-bar__title {
    font-size: 1.2857rem;
  }

  /* Page actions keep their icon; the label stays for screen readers only, so
     the page title keeps its room. */
  .title-bar__actions :deep(.p-button-label) {
    position: absolute;
    width: 1px;
    height: 1px;
    overflow: hidden;
    clip-path: inset(50%);
    white-space: nowrap;
  }
}

.title-bar__page > .p-button {
  flex-shrink: 0;
}

.title-bar__page {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.title-bar__heading {
  min-width: 0;
  display: flex;
  flex-direction: column;
  padding-left: 0.5rem;
}

.title-bar__module {
  font-size: 0.8571rem;
  line-height: 1.2;
  color: var(--p-text-muted-color);
}

.title-bar__title {
  margin: 0;
  font-family: var(--font-condensed);
  font-size: 1.4286rem;
  font-weight: 600;
  line-height: 1.25;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.title-bar__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.title-bar__actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.title-bar__actions:not(:empty) {
  margin-right: 0.5rem;
}
</style>
