<script setup lang="ts">
import { computed, inject } from "vue";

/**
 * Page-level actions (Save and its menu) of a screen. They render in the
 * header slot provided by TheHeader, so every screen saves from the same
 * place. Dialogs keep their own footer buttons: forms reused inside a dialog
 * pass `inline` to render the actions in place instead.
 */
const props = defineProps<{ inline?: boolean }>();

// Tabs mount inactive panels hidden, so actions inside one would still reach
// the header. PrimeVue's TabPanel provides itself; only its active panel shows.
const tabPanel = inject<{ active: boolean } | null>("$pcTabPanel", null);
const visibleInHeader = computed(
  () => !props.inline && (!tabPanel || tabPanel.active),
);
</script>

<template>
  <div v-if="inline" class="page-actions-inline">
    <slot />
  </div>
  <Teleport v-else-if="visibleInHeader" defer to="#page-actions">
    <slot />
  </Teleport>
</template>

<style scoped>
.page-actions-inline {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.5rem;
}
</style>
