<script setup lang="ts">
import { computed, useSlots, useAttrs, ref, watch, onMounted, onUnmounted } from "vue";
import { useI18n } from "vue-i18n";
import TableFilter from "./TableFilter.vue";
import type { FilterConfig, FilterBodyWidth } from "./TableFilter.vue";
import TableViewConfig from "./TableViewConfig.vue";
import TableAttachmentViewer from "./TableAttachmentViewer.vue";
import BooleanColumn from "./BooleanColumn.vue";
import TruncatedCell from "./TruncatedCell.vue";
import ProgressColumn from "@/components/ProgressColumn.vue";
import ColumnGroup from "primevue/columngroup";
import Row from "primevue/row";
import type { DataTableRowClickEvent } from "primevue/datatable";
import { useStore } from "@/store";
import { useUserTableViewStore } from "@/store/usertableview";
import type { SortConfig } from "@/store/usertableview";
import {
  formatDate,
  formatDateTime,
  formatTime,
  formatCurrency,
} from "@/utils/functions";
import {
  ColumnType,
  type Aggregation,
  type AttachmentConfig,
  type TablePreset,
  type Column,
} from "./types";

const PRESET_DEFAULTS: Record<TablePreset, Record<string, unknown>> = {
  "crud-list": {
    selectionMode: "single",
    paginator: "auto",
    rows: 20,
    scrollable: true,
    scrollHeight: "flex",
    stripedRows: true,
    rowHover: true,
    sortMode: "single",
  },
  "read-only": {
    paginator: false,
    stripedRows: true,
    rowHover: true,
  },
  "detail-lines": {
    paginator: false,
    scrollable: true,
    scrollHeight: "40vh",
    stripedRows: true,
    rowHover: true,
  },
  selector: {
    selectionMode: "single",
    paginator: "auto",
    rows: 10,
    scrollable: true,
    scrollHeight: "50vh",
  },
};

defineOptions({ inheritAttrs: false });

const props = withDefaults(
  defineProps<{
    columns: Column[];
    items: readonly any[];
    filterConfig?: FilterConfig[];
    filterLabels?: Record<string, string>;
    filterValueResolvers?: Record<string, (value: unknown) => string>;
    filterValues?: any;
    filterBodyWidth?: FilterBodyWidth;
    showFilters?: boolean;
    page?: string;
    showDeleteColumn?: boolean;
    canDelete?: (item: any) => boolean;
    attachmentConfig?: AttachmentConfig | null;
    preset?: TablePreset;
    loading?: boolean;
    dataKey?: string;
    stripedRows?: boolean;
    rowHover?: boolean;
    selectionMode?: "single" | "multiple";
    rowGroupMode?: "rowspan" | "subheader" | "subfooter";
    expandedRows?: any[] | null;
    paginator?: boolean | null;
    rows?: number;
    scrollable?: boolean | null;
    scrollHeight?: string;
    sortField?: string;
    sortOrder?: number;
  }>(),
  { showFilters: true, paginator: null, scrollable: null },
);

const resolvedDataTableProps = computed(() => {
  const preset = props.preset ? PRESET_DEFAULTS[props.preset] : {};
  const explicit: Record<string, unknown> = {};
  if (props.loading !== undefined) explicit.loading = props.loading;
  if (props.dataKey !== undefined) explicit.dataKey = props.dataKey;
  if (props.stripedRows !== undefined) explicit.stripedRows = props.stripedRows;
  if (props.rowHover !== undefined) explicit.rowHover = props.rowHover;
  if (props.selectionMode !== undefined) explicit.selectionMode = props.selectionMode;
  if (props.rowGroupMode !== undefined) explicit.rowGroupMode = props.rowGroupMode;
  if (props.expandedRows !== undefined) explicit.expandedRows = props.expandedRows;
  // Exclude paginator, rows, scrollable, scrollHeight, sortField, sortOrder from the spread —
  // they are bound explicitly in the template to avoid PrimeVue reactivity issues.
  const { paginator: _p, rows: _r, scrollable: _s, scrollHeight: _sh, ...presetRest } = preset;
  return { ...presetRest, ...explicit, ...attrs };
});

// Sort: active view config takes priority, then consumer prop.
// Bound explicitly in template so PrimeVue detects each prop change independently.
// Guarded against incomplete sort configs — PrimeVue's multiSortField accessor
// crashes on `multiSortMeta[0].field` when sortField is undefined but sortOrder
// is set (or vice versa), especially when sortMode="multiple".
// In sortMode="multiple", sortField/sortOrder are single-mode props and are
// ignored by PrimeVue — instead, multiSortMeta (array) is used. We adapt
// the single {field, order} sort config into a single-element multiSortMeta
// array, and skip the sortField/sortOrder bindings to avoid feeding PrimeVue
// an inconsistent state.
const isMultipleSort = computed(
  () => attrs.sortMode === "multiple" || attrs["sort-mode"] === "multiple",
);
const resolvedSortField = computed(() => {
  if (isMultipleSort.value) return undefined;
  const field = activeViewId.value
    ? activeSortConfig.value?.field
    : props.sortField ?? activeSortConfig.value?.field;
  return field ? field : undefined;
});
const resolvedSortOrder = computed(() => {
  if (isMultipleSort.value) return undefined;
  if (!resolvedSortField.value) return undefined;
  return activeViewId.value
    ? activeSortConfig.value?.order
    : props.sortOrder ?? activeSortConfig.value?.order;
});
const resolvedMultiSortMeta = computed(() => {
  if (!isMultipleSort.value) return undefined;
  const field = activeViewId.value
    ? activeSortConfig.value?.field
    : props.sortField ?? activeSortConfig.value?.field;
  const order = activeViewId.value
    ? activeSortConfig.value?.order
    : props.sortOrder ?? activeSortConfig.value?.order;
  if (!field || order === undefined) return undefined;
  return [{ field, order: order as 1 | -1 }];
});
// PrimeVue DataTable caches sort state internally and ignores subsequent
// sortField/sortOrder prop changes. A reactive :key forces re-mount when
// the sort config changes so the new props are picked up as initial state.
const sortKey = computed(() => {
  if (isMultipleSort.value) {
    const meta = resolvedMultiSortMeta.value;
    return `multi_${meta?.[0]?.field ?? ''}_${meta?.[0]?.order ?? ''}`;
  }
  return `${resolvedSortField.value ?? ''}_${resolvedSortOrder.value ?? ''}`;
});

const resolvedRows = computed(() => {
  if (props.rows !== undefined) return props.rows;
  if (props.preset) return PRESET_DEFAULTS[props.preset].rows as number | undefined;
  return undefined;
});

const resolvedScrollable = computed(() => {
  if (props.scrollable !== null) return props.scrollable;
  if (props.preset) return PRESET_DEFAULTS[props.preset].scrollable as boolean | undefined;
  return undefined;
});

const resolvedScrollHeight = computed(() => {
  if (props.scrollHeight !== undefined) return props.scrollHeight;
  if (props.preset) return PRESET_DEFAULTS[props.preset].scrollHeight as string | undefined;
  return undefined;
});

// paginator and rows are bound explicitly in the template to avoid v-bind object
// coercion issues with PrimeVue Boolean props.
// Preset "auto": paginator activates only when items.length > rows threshold.
// paginator default is null (not false) because Vue auto-casts unset Boolean
// props to false, making it impossible to distinguish "not passed" from "false".
const resolvedPaginator = computed(() => {
  if (props.paginator !== null) return props.paginator;
  if (props.preset) {
    const presetPaginator = PRESET_DEFAULTS[props.preset].paginator;
    if (presetPaginator === "auto") {
      const threshold = resolvedRows.value ?? 20;
      return props.items.length > threshold;
    }
    return presetPaginator as boolean | undefined;
  }
  return undefined;
});

const emit = defineEmits<{
  (e: "update:filterValues", value: any): void;
  (e: "filter"): void;
  (e: "clear"): void;
  (e: "create"): void;
  (e: "delete", item: any): void;
  (e: "update:sortConfig", value: SortConfig | null): void;
  (e: "row-click", event: DataTableRowClickEvent): void;
}>();

const slots = useSlots();
const attrs = useAttrs();
const store = useStore();
const viewStore = useUserTableViewStore();
const { t } = useI18n();

const attachmentViewer = ref<InstanceType<typeof TableAttachmentViewer> | null>(null);

function openAttachments(item: unknown): void {
  attachmentViewer.value?.open(item);
}

// --- Table view management ---

const appliedColumns = ref<Column[]>([...props.columns]);
const activeViewId = ref<string>("");
const activeIsDefault = ref(false);
const viewConfigVisible = ref(false);
const activeSortConfig = ref<SortConfig | null>(null);

// Increments each time the user triggers a "clear filters" action. Used as
// the Vue :key on the embedded TableFilter so PrimeVue InputText/Select
// inputs are forced to remount with the cleared modelValue. PrimeVue's
// InputText does not always re-sync its internal <input> when an external
// modelValue flips from a typed string back to "" — a remount guarantees
// a clean DOM. Only bumped on @clear, never on @filter, so the user's
// focus and selection state are preserved while typing.
const clearKey = ref(0);

function bumpClearKey() {
  clearKey.value++;
}

// Tracks whether the user has changed ANY part of the table state
// (filters, columns visibility/order, or sort) since the last successful
// auto-save (or since mount). Set to true on:
//   - @filter / @clear (filter values)
//   - onSortConfigUpdate (sort field/order via dialog)
//   - watch on appliedColumns (column visibility/order via dialog)
// Cleared in onRowClick right after dispatching the save. Drives whether
// the row-click handler fires a PUT to the backend.
const stateDirty = ref(false);

function markStateDirty() {
  stateDirty.value = true;
}

function onApplyViewConfig(columns: Column[], viewId: string) {
  appliedColumns.value = columns;
  activeViewId.value = viewId;
  // Refresh the cached default flag from the (possibly updated) store
  // snapshot. Falls back to false when the view isn't in the list yet
  // (e.g. a brand-new view created in the same tick before
  // `viewStore.create`'s internal fetchViews has resolved).
  const matched = viewStore.views.find((v) => v.id === viewId);
  activeIsDefault.value = matched?.isDefault ?? false;
  // Applying a saved view restores persisted state — it's NOT a user
  // mutation. Reset the dirty flag so the next row click doesn't
  // re-save the same config we just loaded.
  stateDirty.value = false;
}

function onSortConfigUpdate(sortConfig: SortConfig | null) {
  activeSortConfig.value = sortConfig;
  // Sort was changed by the user via the dialog → state is dirty.
  markStateDirty();
  emit("update:sortConfig", sortConfig);
}

// Build the columns payload to persist: only the fields needed to
// reconstruct visibility + order. Mirrors what TableViewConfig.buildViewConfig
// does on save (filters out columns that don't carry user choices).
function buildColumnsPayload(): Array<{ field: string; visible?: boolean; order?: number }> {
  return appliedColumns.value
    .filter((col) => col.order !== undefined || col.visible === false)
    .map((col) => ({
      field: col.field,
      visible: col.visible,
      order: col.order,
    }));
}

// Row-click handler: forwards the event to the consumer (for navigation)
// and, when state is dirty + the active view is the default view, kicks
// off a fire-and-forget save of the full table state (columns + sort +
// filters) to that default view. The save never blocks navigation;
// errors are logged, not surfaced.
//
// Auto-save is gated on `activeIsDefault` rather than `activeViewId !== ""`
// because the user may have selected a non-default view: in that case
// the row click should not overwrite state onto a view that isn't theirs.
function onRowClick(event: DataTableRowClickEvent) {
  emit("row-click", event);
  if (
    stateDirty.value &&
    activeIsDefault.value &&
    activeViewId.value !== "" &&
    store.user?.id &&
    props.page
  ) {
    stateDirty.value = false;
    viewStore
      .saveStateToDefault(store.user.id, props.page, {
        columns: buildColumnsPayload(),
        sort: activeSortConfig.value ?? undefined,
        filters: props.filterValues,
      })
      .catch((err) => {
        console.warn("[Table] auto-save state failed", err);
        // Re-arm so the next change retries instead of getting lost.
        stateDirty.value = true;
      });
  }
}

// Wraps the original @filter / @clear emits so we can also mark the
// state as dirty (which arms the row-click auto-save). Original emits
// are kept so consumers continue to receive the events. The clear path
// also bumps clearKey to force a remount of the embedded TableFilter,
// guaranteeing that PrimeVue <InputText>/<Select> inputs re-sync their
// DOM value when the consumer clears the filter model.
function onFilterApplied() {
  markStateDirty();
  emit("filter");
}

function onClearApplied() {
  bumpClearKey();
  markStateDirty();
  emit("clear");
}

async function loadDefaultView() {
  const userId = store.user?.id;
  if (!userId || !props.page) return;

  // Reset dirty flag: loading a saved view means we're starting from a
  // clean state — the user hasn't touched anything new yet.
  stateDirty.value = false;

  await viewStore.fetchViews(userId, props.page);

  const userViews = viewStore.views.filter(
    (v) => v.userId === userId && v.page === props.page
  );

  const defaultView =
    userViews.find((v) => v.isDefault) ?? userViews[0] ?? null;

  if (defaultView) {
    appliedColumns.value = viewStore.applyView(defaultView, props.columns);
    activeViewId.value = defaultView.id;
    activeIsDefault.value = defaultView.isDefault;
    const filterValues = viewStore.applyFilterConfig(defaultView);
    if (filterValues) {
      emit("update:filterValues", filterValues);
      emit("filter");
    }
    const sortConfig = viewStore.applySortConfig(defaultView);
    activeSortConfig.value = sortConfig;
    emit("update:sortConfig", sortConfig);
  } else {
    appliedColumns.value = [...props.columns];
    activeViewId.value = "";
    activeIsDefault.value = false;
    activeSortConfig.value = null;
    emit("update:sortConfig", null);
  }
}

// Provision a default only when there are no saved views for the user on this
// page. Existing views are loaded by `ensureDefaultAndLoad` afterwards.
async function provisionDefaultOnFirstVisit() {
  if (!props.page) return;
  const userId = store.user?.id;
  if (!userId) return;

  if (viewStore.views.some((v) => v.userId === userId && v.page === props.page))
    return;

  try {
    await viewStore.ensureDefault(userId, props.page);
  } catch (err) {
    console.warn("[Table] ensureDefault failed", err);
  }
}

// Watch for user authentication to load default view after page refresh
watch(() => store.user?.id, (newUserId, oldUserId) => {
  if (newUserId && props.page && newUserId !== oldUserId) {
    ensureDefaultAndLoad();
  }
}, { immediate: true });

// --- Filter persistence ---
onMounted(async () => {
  if (props.page) {
    // If no user yet, trigger load when user becomes available
    if (!store.user?.id) {
      const unwatch = watch(() => store.user?.id, (userId) => {
        if (userId) {
          unwatch();
          ensureDefaultAndLoad();
        }
      });
    } else {
      await ensureDefaultAndLoad();
    }
  }
});

// Orchestrates the autoprovision + load + restore sequence on mount.
// Provisioning is conservative: the default view is auto-created only
// when the user has NO views on this page. If they explicitly deleted
// the default view, the first remaining view is loaded instead.
async function ensureDefaultAndLoad() {
  if (!props.page) return;
  const userId = store.user?.id;
  if (!userId) return;

  await provisionDefaultOnFirstVisit();
  await loadDefaultView();
}

// The database is the single source of truth for table state. Any
// stale localStorage snapshot from earlier iterations could shadow a
// freshly-saved server view on remount, so we clear it on unmount.
onUnmounted(() => {
  if (props.page) {
    localStorage.removeItem(`lilith-table-filters-${props.page}`);
  }
});

watch(() => props.columns, (newColumns) => {
  if (props.page && activeViewId.value) {
    const view = viewStore.views.find((v) => v.id === activeViewId.value);
    if (view) {
      appliedColumns.value = viewStore.applyView(view, newColumns);
    } else {
      appliedColumns.value = [...newColumns];
    }
  } else {
    appliedColumns.value = [...newColumns];
  }
}, { deep: true });

// Watch for user-driven changes to column visibility or order applied
// through TableViewConfig. The dialog mutates `appliedColumns` via its
// own local state and applies it back through onApplyViewConfig — but
// we also catch manual mutations here so any path that flips a column
// off or reorders it marks the state dirty for the row-click save.
watch(appliedColumns, () => {
  if (props.page && activeViewId.value !== "") {
    // Skip the initial assignment that loadDefaultView performs
    // (stateDirty is already false there). Other changes are user-driven.
    markStateDirty();
  }
}, { deep: true });

// --- Columns with visibility and order ---

const visibleColumns = computed(() => {
  let result = appliedColumns.value.filter((c) => c.visible !== false);
  result.sort((a, b) => {
    const orderA = a.order ?? Number.MAX_VALUE;
    const orderB = b.order ?? Number.MAX_VALUE;
    return orderA - orderB;
  });
  return result;
});

// --- Totals ---

const columnsWithTotal = computed(() => visibleColumns.value.filter((c) => c.total));

const hasFooter = computed(
  () =>
    columnsWithTotal.value.length > 0 ||
    visibleColumns.value.some((c) => !!slots[`footer-${c.field}`]),
);

function aggregate(
  items: readonly any[],
  field: string,
  kind: Aggregation,
): number {
  const nums = items
    .map((i) => i[field])
    .filter((v): v is number => typeof v === "number");
  if (nums.length === 0) return 0;
  switch (kind) {
    case "sum":
      return nums.reduce((a, b) => a + b, 0);
    case "avg":
      return nums.reduce((a, b) => a + b, 0) / nums.length;
    case "min":
      return Math.min(...nums);
    case "max":
      return Math.max(...nums);
    case "count":
      return items.filter(
        (i) => i[field] !== undefined && i[field] !== null,
      ).length;
  }
}

const totals = computed(() => {
  const map: Record<string, number> = {};
  for (const col of columnsWithTotal.value) {
    map[col.field] = aggregate(props.items, col.field, col.total!);
  }
  return map;
});

function formatTotal(col: Column): string {
  const raw = totals.value[col.field] ?? 0;
  return col.totalFormat ? col.totalFormat(raw) : String(raw);
}

// --- Filter slot helpers ---

function isFilterSlot(name: string | number | symbol): boolean {
  return typeof name === "string" && name.startsWith("filter-");
}

function filterSlotName(name: string | number | symbol): string {
  return typeof name === "string" ? name.slice(7) : "";
}

// Empty-value guard: prevents Date/DateTime/Time columns from rendering
// the epoch (01/01/1970) when the field is null/undefined/empty string.
// `new Date(null)` is `Date(0)` → epoch, which Intl then formats as 01/01/1970.
function hasValue(value: unknown): boolean {
  if (value === null || value === undefined) return false;
  if (typeof value === "string" && value.trim() === "") return false;
  return true;
}

// Single source of truth for the display string of a cell value.
// Used by the default/typed body templates so the rendered text matches
// what consumers see, regardless of columnType.
function formatCellValue(col: Column, data: any): string {
  const value = data[col.field];
  switch (col.columnType) {
    case ColumnType.Date: return formatDate(value);
    case ColumnType.DateTime: return formatDateTime(value);
    case ColumnType.Time: return formatTime(value);
    case ColumnType.Currency: return formatCurrency(value);
    case ColumnType.Lookup: return col.resolver?.(value) ?? "";
    case ColumnType.Number: return String(value);
    default: return String(value ?? "");
  }
}
</script>

<template>
  <DataTable
    :key="sortKey"
    showGridlines
    v-bind="resolvedDataTableProps"
    :value="items"
    :paginator="resolvedPaginator"
    :rows="resolvedRows"
    :scrollable="resolvedScrollable"
    :scrollHeight="resolvedScrollHeight"
    :sortField="resolvedSortField"
    :sortOrder="resolvedSortOrder"
    :multiSortMeta="resolvedMultiSortMeta"
    @row-click="onRowClick"
  >
    <!-- TableFilter embedded in DataTable's native header slot -->
    <template
      v-if="showFilters && filterConfig"
      #header
    >
      <TableFilter
        :key="clearKey"
        :config="filterConfig"
        :model-value="filterValues"
        :body-width="filterBodyWidth"
        :show-title="false"
        :show-action-labels="false"
        embedded
        @update:model-value="emit('update:filterValues', $event)"
        @filter="onFilterApplied"
        @clear="onClearApplied"
        @create="emit('create')"
      >
        <!-- Forward #prepend and #append to TableFilter -->
        <template v-if="slots.prepend" #prepend>
          <slot name="prepend" />
        </template>
        <template v-if="slots.append" #append>
          <slot name="append" />
        </template>
        <!-- Table view config button as first action -->
        <template v-if="page" #action-prepend>
          <Button
            icon="pi pi-cog"
            size="small"
            text
            rounded
            aria-label="Configuració de la vista"
            v-tooltip.top="'Configuració de la vista'"
            @click="viewConfigVisible = true"
          />
        </template>
        <!-- Forward #filter-{name} slots -->
        <template
          v-for="(_, name) in slots"
          :key="String(name)"
          #[filterSlotName(name)]
        >
          <template v-if="isFilterSlot(name)">
            <slot :name="name" />
          </template>
        </template>
      </TableFilter>
    </template>

    <!-- Dynamic columns -->
    <template v-for="col in visibleColumns" :key="col.field">
      <ProgressColumn
        v-if="col.columnType === ColumnType.ProgressBar"
        :field="col.field"
        :header="col.header"
        :sortable="col.sortable || activeSortConfig?.field === col.field"
        :style="col.style"
        :show-value="col.props?.showValue"
        :cap="col.props?.cap"
        :overrun-severity="col.props?.overrunSeverity"
        :tooltip="col.props?.tooltip"
      />
      <Column
        v-else
        :field="col.field"
        :header="col.header"
        :sortable="col.sortable || activeSortConfig?.field === col.field"
        :style="col.style"
        :pt="col.truncate !== false ? { bodyCell: { class: 'truncate-cell' } } : undefined"
      >
      <!-- Custom body slot from consumer takes priority -->
      <template v-if="slots[`body-${col.field}`]" #body="slotProps">
        <slot :name="`body-${col.field}`" v-bind="slotProps" />
      </template>

      <!-- Boolean: not affected by truncation (its own component) -->
      <template v-else-if="col.columnType === ColumnType.Boolean" #body="slotProps">
        <BooleanColumn
          :value="slotProps.data[col.field]"
          :show-color="col.showColor"
        />
      </template>
      <!-- Default + all text-typed columns: route through TruncatedCell.
           Default is true; opt out per column with `truncate: false`. -->
      <template v-else #body="slotProps">
        <TruncatedCell
          v-if="hasValue(slotProps.data[col.field])"
          :value="formatCellValue(col, slotProps.data)"
          :truncate="col.truncate !== false"
        />
      </template>
      </Column>
    </template>

    <!-- Read-only attachment action column -->
    <Column
      v-if="attachmentConfig"
      :pt="{ bodyCell: { style: 'padding: 0 !important; position: relative;' } }"
      style="width: 3rem; min-width: 3rem; max-width: 3rem"
    >
      <template #body="slotProps">
        <div
          class="attachment-cell"
          @click.stop="openAttachments(slotProps.data)"
          v-tooltip.top="t('table.attachments.tooltip')"
        >
          <i class="pi pi-paperclip attachment-icon"></i>
        </div>
      </template>
    </Column>
    <!-- Delete action column -->
    <Column v-if="showDeleteColumn" :pt="{ bodyCell: { style: 'padding: 0 !important; position: relative;' } }" style="width: 3rem; min-width: 3rem; max-width: 3rem">
      <template #body="slotProps">
        <div
          v-if="canDelete ? canDelete(slotProps.data) : true"
          class="delete-cell"
          @click.stop="emit('delete', slotProps.data)"
          v-tooltip.top="'Eliminar'"
        >
          <i class="pi pi-trash delete-icon"></i>
        </div>
      </template>
    </Column>

    <!-- Footer totals row (only if any column has total or footer slot) -->
    <ColumnGroup v-if="hasFooter" type="footer">
      <Row>
        <Column v-for="col in visibleColumns" :key="col.field">
          <template #footer>
            <slot
              v-if="slots[`footer-${col.field}`]"
              :name="`footer-${col.field}`"
            />
            <span v-else-if="col.total">{{ formatTotal(col) }}</span>
          </template>
        </Column>
        <Column v-if="attachmentConfig" class="attachment-column" style="width: 3rem; min-width: 3rem; max-width: 3rem" />
        <!-- Empty footer cell for delete column alignment -->
        <Column v-if="showDeleteColumn" class="delete-column" style="width: 3rem; min-width: 3rem; max-width: 3rem" />
      </Row>
    </ColumnGroup>

    <!-- PrimeVue slot passthrough -->
    <template v-if="slots.empty" #empty>
      <slot name="empty" />
    </template>
    <template v-if="slots.loading" #loading>
      <slot name="loading" />
    </template>
    <template v-if="slots.paginatorstart" #paginatorstart>
      <slot name="paginatorstart" />
    </template>
    <template v-if="slots.paginatorend" #paginatorend>
      <slot name="paginatorend" />
    </template>
  </DataTable>

  <TableViewConfig
    v-if="page"
    v-model:visible="viewConfigVisible"
    :columns="props.columns"
    :page="page"
    :active-view-id="activeViewId"
    :filter-values="filterValues"
    :active-sort-config="activeSortConfig"
    :filter-config="filterConfig"
    :filter-labels="filterLabels"
    :filter-value-resolvers="filterValueResolvers"
    @apply-config="onApplyViewConfig"
    @update:sort-config="onSortConfigUpdate"
    @update:filter-values="emit('update:filterValues', $event)"
  />

  <TableAttachmentViewer
    v-if="attachmentConfig"
    ref="attachmentViewer"
    :config="attachmentConfig"
  />
</template>

<style scoped>
.attachment-cell {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--p-primary-color);
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.attachment-cell:hover {
  background-color: var(--p-primary-50);
}

.attachment-icon {
  font-size: 0.9rem;
  pointer-events: none;
}


.delete-cell {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ef4444;
  cursor: pointer;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.delete-cell:hover {
  background-color: #ef4444;
  color: #ffffff;
}

.delete-icon {
  font-size: 0.75rem;
  pointer-events: none;
}
</style>

<style>
/* Non-scoped: targets the <td> cells marked via Column's `pt` prop.
   Constrains cell width so the inner TruncatedCell span can apply ellipsis.
   In table-layout: auto (PrimeVue default) the column sizes to the widest
   cell, so the max-width effectively becomes the column width. */
.p-datatable .truncate-cell {
  max-width: var(--table-cell-truncate-max-width, 300px);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
