<template>
  <Button
    v-if="isPhone"
    icon="pi pi-search"
    severity="secondary"
    text
    rounded
    :aria-label="label"
    aria-haspopup="dialog"
    :aria-expanded="menuSearch.visible"
    @click="menuSearch.open()"
  />
  <Button
    v-else
    severity="secondary"
    outlined
    class="menu-search-trigger"
    :aria-label="label"
    aria-haspopup="dialog"
    :aria-expanded="menuSearch.visible"
    v-tooltip.bottom="label"
    @click="menuSearch.open()"
  >
    <i class="pi pi-search menu-search-trigger__icon" aria-hidden="true" />
    <span>{{ t("ui.menuSearch.trigger") }}</span>
    <kbd class="menu-search-trigger__kbd" aria-hidden="true">{{
      shortcutKeys
    }}</kbd>
  </Button>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { useIsPhone } from "@/composables/useIsPhone";
import { useMenuSearchStore } from "@/store/menuSearch";

const { t } = useI18n();
const isPhone = useIsPhone();
const menuSearch = useMenuSearchStore();

const isMac = /Mac|iPhone|iPad/.test(navigator.userAgent);
const shortcutKeys = isMac ? "⌘K" : "Ctrl K";
const label = computed(() =>
  t("ui.menuSearch.openTooltip", { shortcut: isMac ? "⌘K" : "Ctrl+K" }),
);
</script>

<style scoped>
.menu-search-trigger {
  gap: 0.5rem;
  height: 2.4286rem;
  padding: 0 0.375rem 0 0.625rem;
  border-color: var(--p-surface-300);
  background: var(--p-surface-0);
  color: var(--p-surface-700);
  font-weight: 400;
}

.menu-search-trigger:not(:disabled):hover {
  border-color: var(--p-surface-400);
  background: var(--p-surface-50);
  color: var(--p-surface-800);
}

.menu-search-trigger__icon {
  font-size: 1rem;
  color: var(--p-text-muted-color);
}

.menu-search-trigger__kbd {
  display: inline-flex;
  align-items: center;
  height: 1.4286rem;
  box-sizing: border-box;
  padding: 0 0.3571rem;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius-sm);
  background: var(--p-surface-50);
  color: var(--p-text-muted-color);
  font-family: inherit;
  font-size: 0.7857rem;
  font-weight: 500;
  line-height: 1;
}
</style>
