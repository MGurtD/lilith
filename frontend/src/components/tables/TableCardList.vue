<script setup lang="ts">
import { computed, h, useSlots, type FunctionalComponent, type VNodeChild } from "vue";
import { useI18n } from "vue-i18n";
import DataView from "primevue/dataview";
import Tag from "primevue/tag";
import type { DataTableRowClickEvent } from "primevue/datatable";
import BooleanColumn from "./BooleanColumn.vue";
import { resolveFieldValue } from "./field-value";
import {
  formatCellValue,
  hasValue,
  resolveBooleanValue,
  resolveCellValue,
  statusSeverity,
} from "./cell-format";
import { ColumnType, type CardLayout, type Column } from "./types";

/** Same shape as PrimeVue's row-reorder event, which consumers read. */
export interface CardRowReorderEvent {
  value: any[];
  dragIndex: number;
  dropIndex: number;
}

export interface CardTotal {
  field: string;
  label: string;
  value: string;
}

// Phone layout of Table.vue: one card per row, with the columns the
// resolved CardLayout puts in each slot.
const props = withDefaults(
  defineProps<{
    items: readonly any[];
    columns: Column[];
    layout: CardLayout;
    dataKey?: string;
    paginator?: boolean;
    rows?: number;
    sortField?: string;
    sortOrder?: number;
    showDelete?: boolean;
    canDelete?: (item: any) => boolean;
    showAttachments?: boolean;
    totals?: CardTotal[];
    loading?: boolean;
    /** False when nothing listens for row clicks, and in the settings preview. */
    interactive?: boolean;
    /** A checkbox per card, bound like the table's v-model:selection. */
    selectable?: boolean;
    selection?: readonly any[] | null;
    /** Move up/down buttons, the card version of the table's drag handle. */
    reorderable?: boolean;
  }>(),
  {
    paginator: false,
    showDelete: false,
    showAttachments: false,
    totals: () => [],
    interactive: true,
    selectable: false,
    selection: null,
    reorderable: false,
  },
);

const emit = defineEmits<{
  (e: "row-click", event: DataTableRowClickEvent): void;
  (e: "delete", item: any): void;
  (e: "attachments", item: any): void;
  (e: "update:selection", value: any[]): void;
  (e: "row-reorder", event: CardRowReorderEvent): void;
}>();

const slots = useSlots();
const { t } = useI18n();

const columnsByField = computed(
  () => new Map(props.columns.map((col) => [col.field, col])),
);

const column = (field: string | undefined) =>
  field ? columnsByField.value.get(field) : undefined;

const titleColumn = computed(() => column(props.layout.title));
const subtitleColumn = computed(() => column(props.layout.subtitle));
const badgeColumn = computed(() => column(props.layout.badge));
const trailingColumn = computed(() => column(props.layout.trailing));
const metaColumns = computed(() =>
  (props.layout.meta ?? [])
    .map((field) => column(field))
    .filter((col): col is Column => !!col),
);

const hasActions = computed(
  () =>
    props.showDelete ||
    props.showAttachments ||
    props.reorderable ||
    !!slots["card-actions"],
);

// A tap on the card opens the row, or else toggles it when selectable.
const clickable = computed(() => props.interactive || props.selectable);

// --- Selection ---

function sameRow(a: any, b: any) {
  return props.dataKey ? a?.[props.dataKey] === b?.[props.dataKey] : a === b;
}

function isSelected(item: any) {
  return (props.selection ?? []).some((row) => sameRow(row, item));
}

function toggleSelection(item: any) {
  const current = [...(props.selection ?? [])];
  emit(
    "update:selection",
    isSelected(item)
      ? current.filter((row) => !sameRow(row, item))
      : [...current, item],
  );
}

// Like the table's header checkbox: every row, not just this page.
const allSelected = computed(
  () => props.items.length > 0 && props.items.every(isSelected),
);

function toggleAll() {
  emit("update:selection", allSelected.value ? [] : [...props.items]);
}

// --- Reordering ---

function compareValues(a: unknown, b: unknown): number {
  if (a == null) return b == null ? 0 : -1;
  if (b == null) return 1;
  if (typeof a === "number" && typeof b === "number") return a - b;
  return String(a).localeCompare(String(b), undefined, { numeric: true });
}

// The rows in card order. A move reorders these, as the table's drag
// reorders its sorted rows, and emits the same payload.
const orderedItems = computed(() => {
  const rows = [...props.items];
  const field = props.sortField;
  const order = props.sortOrder;
  if (!field || !order) return rows;
  return rows.sort(
    (a, b) =>
      order *
      compareValues(resolveFieldValue(a, field), resolveFieldValue(b, field)),
  );
});

function moveRow(item: any, step: -1 | 1) {
  const rows = [...orderedItems.value];
  const dragIndex = rows.indexOf(item);
  const dropIndex = dragIndex + step;
  if (dragIndex < 0 || dropIndex < 0 || dropIndex >= rows.length) return;
  rows.splice(dropIndex, 0, ...rows.splice(dragIndex, 1));
  emit("row-reorder", { value: rows, dragIndex, dropIndex });
}

// A consumer's #card-{field} slot wins, then its #body-{field} slot, as
// in the table cells.
function renderValue(col: Column, data: unknown, index: number): VNodeChild {
  const custom = slots[`card-${col.field}`] ?? slots[`body-${col.field}`];
  if (custom) return custom({ data, field: col.field, index });
  if (col.columnType === ColumnType.Boolean) {
    return h(BooleanColumn, {
      value: resolveBooleanValue(data, col.field),
      showColor: col.showColor,
    });
  }
  if (!hasValue(resolveCellValue(col, data))) return null;
  if (col.columnType === ColumnType.Status) {
    return h(Tag, {
      value: formatCellValue(col, data),
      severity: statusSeverity(col, data),
      class: "lifecycle-status-tag",
    });
  }
  return formatCellValue(col, data);
}

const CardValue: FunctionalComponent<{
  col: Column;
  data: unknown;
  index: number;
}> = (p) => renderValue(p.col, p.data, p.index);

function rowKey(item: any, index: number | string) {
  return props.dataKey ? item?.[props.dataKey] : index;
}

// Names each card's action buttons after its title ("Delete" + "25686").
const listId = `table-cards-${Math.random().toString(36).slice(2, 9)}`;
function titleId(item: any) {
  return `${listId}-${props.items.indexOf(item)}`;
}

// Same payload shape the table's row click emits, so consumers'
// handlers (which read `event.data`) work unchanged.
function onCardClick(originalEvent: Event, item: any) {
  if (!props.interactive) {
    if (props.selectable) toggleSelection(item);
    return;
  }
  emit("row-click", {
    originalEvent,
    data: item,
    index: props.items.indexOf(item),
  } as DataTableRowClickEvent);
}
</script>

<template>
  <DataView
    :value="items as any[]"
    layout="list"
    :data-key="dataKey"
    :paginator="paginator"
    :rows="rows"
    :sort-field="sortField"
    :sort-order="sortOrder"
    paginator-template="PrevPageLink CurrentPageReport NextPageLink"
    current-page-report-template="{currentPage} / {totalPages}"
    class="table-cards"
    :class="{ 'table-cards--loading': loading }"
    :aria-busy="loading || undefined"
  >
    <template v-if="slots.header" #header>
      <slot name="header" />
    </template>

    <template #list="{ items: pageItems }">
      <div v-if="selectable" class="table-cards__selection">
        <Checkbox
          :input-id="`${listId}-all`"
          :model-value="allSelected"
          binary
          @update:model-value="toggleAll"
        />
        <label :for="`${listId}-all`">{{ t("tables.cards.selectAll") }}</label>
        <span class="table-cards__selection-count">{{
          t("tables.cards.selectedCount", { count: selection?.length ?? 0 })
        }}</span>
      </div>
      <ul class="table-cards__list">
        <li v-for="(item, index) in pageItems" :key="rowKey(item, index)">
          <!-- The title is the card's main action; its hit area covers the
               whole card, while the action buttons stay separate stops. -->
          <div
            class="table-card"
            :class="{
              'table-card--interactive': clickable,
              'table-card--selected': selectable && isSelected(item),
            }"
          >
            <Checkbox
              v-if="selectable"
              class="table-card__select"
              :model-value="isSelected(item)"
              binary
              :aria-label="t('tables.cards.select')"
              :aria-labelledby="titleColumn ? titleId(item) : undefined"
              @update:model-value="toggleSelection(item)"
            />
            <div class="table-card__body">
              <div class="table-card__top">
                <button
                  v-if="clickable && titleColumn"
                  :id="titleId(item)"
                  type="button"
                  class="table-card__title table-card__primary"
                  @click="onCardClick($event, item)"
                >
                  <CardValue
                    :col="titleColumn"
                    :data="item"
                    :index="items.indexOf(item)"
                  />
                </button>
                <span v-else-if="titleColumn" class="table-card__title">
                  <CardValue
                    :col="titleColumn"
                    :data="item"
                    :index="items.indexOf(item)"
                  />
                </span>
                <span v-if="trailingColumn" class="table-card__trailing">
                  <CardValue
                    :col="trailingColumn"
                    :data="item"
                    :index="items.indexOf(item)"
                  />
                </span>
              </div>
              <div
                v-if="subtitleColumn || badgeColumn"
                class="table-card__second"
              >
                <span v-if="subtitleColumn" class="table-card__subtitle">
                  <CardValue
                    :col="subtitleColumn"
                    :data="item"
                    :index="items.indexOf(item)"
                  />
                </span>
                <span v-if="badgeColumn" class="table-card__badge">
                  <CardValue
                    :col="badgeColumn"
                    :data="item"
                    :index="items.indexOf(item)"
                  />
                </span>
              </div>
              <dl v-if="metaColumns.length" class="table-card__meta">
                <div
                  v-for="col in metaColumns"
                  :key="col.field"
                  class="table-card__meta-row"
                >
                  <dt>{{ col.header }}</dt>
                  <dd>
                    <CardValue
                      :col="col"
                      :data="item"
                      :index="items.indexOf(item)"
                    />
                  </dd>
                </div>
              </dl>
            </div>

            <div v-if="hasActions" class="table-card__actions">
              <slot name="card-actions" :data="item" />
              <template v-if="reorderable">
                <Button
                  icon="pi pi-arrow-up"
                  text
                  rounded
                  class="table-card__action"
                  :aria-label="t('tables.cards.moveUp')"
                  :aria-describedby="titleColumn ? titleId(item) : undefined"
                  :disabled="orderedItems.indexOf(item) <= 0"
                  @click="moveRow(item, -1)"
                />
                <Button
                  icon="pi pi-arrow-down"
                  text
                  rounded
                  class="table-card__action"
                  :aria-label="t('tables.cards.moveDown')"
                  :aria-describedby="titleColumn ? titleId(item) : undefined"
                  :disabled="orderedItems.indexOf(item) >= orderedItems.length - 1"
                  @click="moveRow(item, 1)"
                />
              </template>
              <Button
                v-if="showAttachments"
                icon="pi pi-paperclip"
                text
                rounded
                class="table-card__action"
                :aria-label="t('table.attachments.tooltip')"
                :aria-describedby="titleColumn ? titleId(item) : undefined"
                @click="emit('attachments', item)"
              />
              <Button
                v-if="showDelete && (canDelete ? canDelete(item) : true)"
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                class="table-card__action"
                :aria-label="t('tables.cards.delete')"
                :aria-describedby="titleColumn ? titleId(item) : undefined"
                @click="emit('delete', item)"
              />
            </div>
          </div>
        </li>
      </ul>
    </template>

    <template #empty>
      <slot name="empty">
        <p class="table-cards__empty">{{ t("tables.cards.empty") }}</p>
      </slot>
    </template>

    <template v-if="totals.length" #footer>
      <dl class="table-cards__totals">
        <div
          v-for="total in totals"
          :key="total.field"
          class="table-cards__total"
        >
          <dt>{{ total.label }}</dt>
          <dd>{{ total.value }}</dd>
        </div>
      </dl>
    </template>
  </DataView>
</template>

<style scoped>
.table-cards--loading .table-cards__list {
  opacity: 0.5;
}

.table-cards__list {
  list-style: none;
  margin: 0;
  padding: 0.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.table-cards__empty {
  margin: 0;
  padding: 1.5rem 0.75rem;
  text-align: center;
  color: var(--p-text-muted-color);
}

.table-card {
  position: relative;
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  padding: 0.75rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-content-border-radius);
  background: var(--p-content-background);
  -webkit-tap-highlight-color: transparent;
}

.table-card--selected {
  border-color: var(--p-primary-color);
  background: var(--p-highlight-background);
}

/* Above the title's stretched hit area, like the action buttons. */
.table-card__select {
  position: relative;
  z-index: 1;
  flex-shrink: 0;
  margin-top: 0.125rem;
}

.table-cards__selection {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem 0;
}

.table-cards__selection-count {
  margin-left: auto;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.table-card--interactive:has(.table-card__primary:active) {
  background: var(--p-content-hover-background);
}

.table-card--interactive:has(.table-card__primary:focus-visible) {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 2px;
}

/* A plain button that reads as the title. */
.table-card__primary {
  padding: 0;
  border: 0;
  background: none;
  color: inherit;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.table-card__primary:focus-visible {
  outline: none;
}

/* Stretch the title's hit area over the whole card. */
.table-card__primary::after {
  content: "";
  position: absolute;
  inset: 0;
  border-radius: inherit;
}

/* Controls a screen puts in a card, such as a select or a link, sit
   above the title's stretched hit area so they stay usable. */
.table-card__body :deep(:is(a, input, .p-select, .p-checkbox, .p-button)) {
  position: relative;
  z-index: 1;
}

.table-card__body {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.table-card__top,
.table-card__second {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  min-width: 0;
}

.table-card__title {
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.table-card__trailing {
  flex-shrink: 0;
  max-width: 50%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  text-align: right;
}

/* A row whose values all came out empty takes no space. */
.table-card__second:not(:has(> :not(:empty))) {
  display: none;
}

.table-card__subtitle {
  color: var(--p-text-muted-color);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.table-card__badge {
  flex-shrink: 0;
  margin-left: auto;
}

.table-card__meta,
.table-cards__totals {
  margin: 0.25rem 0 0;
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  font-size: 0.875rem;
}

.table-card__meta-row,
.table-cards__total {
  display: flex;
  gap: 0.5rem;
  min-width: 0;
}

.table-card__meta dt,
.table-cards__total dt {
  color: var(--p-text-muted-color);
  flex-shrink: 0;
  max-width: 50%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.table-card__meta dt::after,
.table-cards__total dt::after {
  content: ":";
}

.table-card__meta dd,
.table-cards__total dd {
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.table-card__meta dd:empty::before {
  content: "—";
  color: var(--p-text-muted-color);
}

.table-cards__totals {
  margin: 0;
  padding: 0.5rem 0.75rem;
  font-weight: 600;
}

/* Fixed width, so values line up whether or not a card can be deleted;
   above the title's stretched hit area. */
.table-card__actions {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  flex-shrink: 0;
  width: 44px;
  margin: -0.5rem -0.25rem -0.5rem 0;
}

.table-card__action.p-button {
  width: 44px;
  height: 44px;
}
</style>
