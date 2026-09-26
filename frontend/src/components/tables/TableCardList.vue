<script setup lang="ts">
import { computed, h, useSlots, type FunctionalComponent, type VNodeChild } from "vue";
import { useI18n } from "vue-i18n";
import DataView from "primevue/dataview";
import Tag from "primevue/tag";
import type { DataTableRowClickEvent } from "primevue/datatable";
import BooleanColumn from "./BooleanColumn.vue";
import {
  formatCellValue,
  hasValue,
  resolveBooleanValue,
  resolveCellValue,
  statusSeverity,
} from "./cell-format";
import { ColumnType, type CardLayout, type Column } from "./types";

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
    /** False for the settings preview: cards are not clickable. */
    interactive?: boolean;
  }>(),
  {
    paginator: false,
    showDelete: false,
    showAttachments: false,
    totals: () => [],
    interactive: true,
  },
);

const emit = defineEmits<{
  (e: "row-click", event: DataTableRowClickEvent): void;
  (e: "delete", item: any): void;
  (e: "attachments", item: any): void;
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

const hasActions = computed(() => props.showDelete || props.showAttachments);

// A consumer's #body-{field} slot wins, as it does in the table cells.
function renderValue(col: Column, data: unknown, index: number): VNodeChild {
  const custom = slots[`body-${col.field}`];
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

// Same payload shape the table's row click emits, so consumers'
// handlers (which read `event.data`) work unchanged.
function onCardClick(originalEvent: Event, item: any) {
  if (!props.interactive) return;
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
    class="table-cards"
    :class="{ 'table-cards--loading': loading }"
    :aria-busy="loading || undefined"
  >
    <template v-if="slots.header" #header>
      <slot name="header" />
    </template>

    <template #list="{ items: pageItems }">
      <ul class="table-cards__list">
        <li v-for="(item, index) in pageItems" :key="rowKey(item, index)">
          <div
            class="table-card"
            :class="{ 'table-card--interactive': interactive }"
            :role="interactive ? 'button' : undefined"
            :tabindex="interactive ? 0 : undefined"
            @click="onCardClick($event, item)"
            @keydown.enter.self="onCardClick($event, item)"
            @keydown.space.self.prevent="onCardClick($event, item)"
          >
            <div class="table-card__body">
              <div class="table-card__top">
                <span v-if="titleColumn" class="table-card__title">
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
              <Button
                v-if="showAttachments"
                icon="pi pi-paperclip"
                text
                rounded
                :aria-label="t('table.attachments.tooltip')"
                @click.stop="emit('attachments', item)"
                @keydown.stop
              />
              <Button
                v-if="showDelete && (canDelete ? canDelete(item) : true)"
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                :aria-label="t('tables.cards.delete')"
                @click.stop="emit('delete', item)"
                @keydown.stop
              />
            </div>
          </div>
        </li>
      </ul>
    </template>

    <template v-if="slots.empty" #empty>
      <slot name="empty" />
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

.table-card {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  padding: 0.75rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-content-border-radius);
  background: var(--p-content-background);
}

.table-card--interactive {
  cursor: pointer;
}

.table-card--interactive:active {
  background: var(--p-content-hover-background);
}

.table-card--interactive:focus-visible {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 2px;
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
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  text-align: right;
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

.table-card__actions {
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  margin: -0.5rem -0.5rem -0.5rem 0;
}
</style>
