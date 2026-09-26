<script setup lang="ts">
import { computed, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { SortConfig } from "@/store/usertableview";
import type { Column } from "./types";

// Phone sort picker for the card layout, which has no column headers to
// click. Each tap cycles a column through ascending, descending and none,
// the same cycle as the view settings dialog.
const props = defineProps<{
  visible: boolean;
  columns: Column[];
  sort: SortConfig | null;
}>();

const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "update:sort", value: SortConfig | null): void;
}>();

const { t } = useI18n();

function cycle(field: string) {
  if (props.sort?.field !== field) {
    emit("update:sort", { field, order: 1 });
  } else if (props.sort.order === 1) {
    emit("update:sort", { field, order: -1 });
  } else {
    emit("update:sort", null);
  }
}

function icon(field: string) {
  if (props.sort?.field !== field) return "pi pi-sort-alt";
  return props.sort.order === 1
    ? "pi pi-sort-amount-up-alt"
    : "pi pi-sort-amount-down";
}

function state(field: string) {
  if (props.sort?.field !== field) return t("tables.sort.none");
  return props.sort.order === 1
    ? t("tables.views.ascending")
    : t("tables.views.descending");
}

// Read out after each tap, since the button's own state cycles.
const announcement = computed(() => {
  const col = props.columns.find((c) => c.field === props.sort?.field);
  if (!col || !props.sort) return t("tables.sort.none");
  return t("tables.sort.status", {
    column: col.header,
    direction: state(col.field),
  });
});

// The drawer does not give focus back when it closes: return it to the
// button that opened the sheet, as the filter sheet does.
let opener: HTMLElement | null = null;

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      opener =
        document.activeElement instanceof HTMLElement
          ? document.activeElement
          : null;
    }
  },
);

function restoreFocus() {
  if (opener?.isConnected) opener.focus();
  opener = null;
}
</script>

<template>
  <Drawer
    :visible="visible"
    position="bottom"
    :header="t('tables.sort.title')"
    blockScroll
    class="table-sort-sheet"
    @update:visible="emit('update:visible', $event)"
    @after-hide="restoreFocus"
  >
    <ul class="table-sort-sheet__list">
      <li v-for="col in columns" :key="col.field">
        <button
          type="button"
          class="table-sort-sheet__option"
          :class="{ 'table-sort-sheet__option--active': sort?.field === col.field }"
          @click="cycle(col.field)"
        >
          <span class="table-sort-sheet__label">{{ col.header }}</span>
          <span class="table-sort-sheet__state">{{ state(col.field) }}</span>
          <i :class="icon(col.field)" aria-hidden="true"></i>
        </button>
      </li>
    </ul>
    <p class="table-sort-sheet__status" aria-live="polite">
      {{ announcement }}
    </p>
    <template #footer>
      <Button
        :label="t('tables.filters.done')"
        class="table-sort-sheet__done"
        @click="emit('update:visible', false)"
      />
    </template>
  </Drawer>
</template>

<style scoped>
.table-sort-sheet__list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.table-sort-sheet__option {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  min-height: 2.75rem;
  padding: 0.5rem 0.75rem;
  border: 1px solid transparent;
  border-radius: var(--p-content-border-radius);
  background: transparent;
  color: var(--p-text-color);
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.table-sort-sheet__option:hover {
  background: var(--p-content-hover-background);
}

.table-sort-sheet__option:focus-visible {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 2px;
}

.table-sort-sheet__option--active {
  border-color: var(--p-primary-color);
  color: var(--p-primary-color);
}

.table-sort-sheet__label {
  flex: 1;
}

/* Visually hidden; screen readers hear the new sort after each tap. */
.table-sort-sheet__status {
  position: absolute;
  width: 1px;
  height: 1px;
  margin: -1px;
  overflow: hidden;
  clip-path: inset(50%);
  white-space: nowrap;
}

.table-sort-sheet__done {
  width: 100%;
  min-height: 2.75rem;
}

.table-sort-sheet__state {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}
</style>

<style>
/* Teleported to <body>, so styled globally, matching the filter sheet. */
.p-drawer.table-sort-sheet {
  height: auto;
  max-height: 85dvh;
  border-radius: var(--p-border-radius-xl) var(--p-border-radius-xl) 0 0;
}

.table-sort-sheet .p-drawer-title {
  font-family: var(--font-condensed);
  font-size: 1.4286rem;
  font-weight: 600;
}
</style>
