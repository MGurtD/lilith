<script setup lang="ts">
import {
  computed,
  getCurrentInstance,
  useSlots,
  useAttrs,
  ref,
  watch,
  onMounted,
  onUnmounted,
} from "vue";
import { useI18n } from "vue-i18n";
import TableFilter from "./TableFilter.vue";
import type { FilterConfig, FilterBodyWidth } from "./TableFilter.vue";
import TableViewConfig from "./TableViewConfig.vue";
import TableAttachmentViewer from "./TableAttachmentViewer.vue";
import TableCardList, {
  type CardRowReorderEvent,
  type CardTotal,
} from "./TableCardList.vue";
import TableSortSheet from "./TableSortSheet.vue";
import BooleanColumn from "./BooleanColumn.vue";
import TruncatedCell from "./TruncatedCell.vue";
import ProgressColumn from "@/components/ProgressColumn.vue";
import ColumnGroup from "primevue/columngroup";
import Row from "primevue/row";
import type { DataTableRowClickEvent } from "primevue/datatable";
import { useStore } from "@/store";
import { useUserTableViewStore } from "@/store/usertableview";
import type { SortConfig } from "@/store/usertableview";
import { useIsPhone } from "@/composables/useIsPhone";
import { resolveFieldValue } from "./field-value";
import {
  formatCellValue,
  hasValue,
  resolveBooleanValue,
  resolveCellValue,
  statusSeverity,
} from "./cell-format";
import { resolveCardLayout } from "./card-layout";
import { createReusableTemplate } from "./reusable-template";
import {
  ColumnType,
  type Aggregation,
  type AttachmentConfig,
  type CardLayout,
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
    filterValues?: any;
    filterBodyWidth?: FilterBodyWidth;
    showFilters?: boolean;
    showFilterActions?: boolean;
    showFilterAction?: boolean | null;
    showClearAction?: boolean | null;
    showCreate?: boolean;
    page?: string;
    showSelectionColumn?: boolean;
    selectionColumnWidth?: string;
    showRowReorderColumn?: boolean;
    rowReorderColumnWidth?: string;
    showDeleteColumn?: boolean;
    deleteColumnWidth?: string;
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
    multiSortMeta?: Array<{ field: string; order: 1 | -1 }>;
    /** The screen's default phone card; a saved view may override it. */
    cardLayout?: CardLayout;
    /**
     * Phone layout. "auto" shows cards on list screens (those with a
     * `page`), "cards" opts any other table in, "table" never shows cards.
     */
    phoneLayout?: "auto" | "cards" | "table";
  }>(),
  {
    phoneLayout: "auto",
    showFilters: true,
    showFilterActions: true,
    showFilterAction: null,
    showClearAction: null,
    showCreate: true,
    selectionColumnWidth: "3rem",
    rowReorderColumnWidth: "3rem",
    deleteColumnWidth: "3%",
    paginator: null,
    scrollable: null,
  },
);

const resolvedDataTableProps = computed(() => {
  const preset = props.preset ? PRESET_DEFAULTS[props.preset] : {};
  const explicit: Record<string, unknown> = {};
  if (props.loading !== undefined) explicit.loading = props.loading;
  if (props.dataKey !== undefined) explicit.dataKey = props.dataKey;
  if (props.stripedRows !== undefined) explicit.stripedRows = props.stripedRows;
  if (props.rowHover !== undefined) explicit.rowHover = props.rowHover;
  if (props.selectionMode !== undefined)
    explicit.selectionMode = props.selectionMode;
  if (props.rowGroupMode !== undefined)
    explicit.rowGroupMode = props.rowGroupMode;
  if (props.expandedRows !== undefined)
    explicit.expandedRows = props.expandedRows;
  // Exclude paginator, rows, scrollable, scrollHeight, sortField, sortOrder from the spread —
  // they are bound explicitly in the template to avoid PrimeVue reactivity issues.
  const {
    paginator: _p,
    rows: _r,
    scrollable: _s,
    scrollHeight: _sh,
    ...presetRest
  } = preset;
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
    : (props.sortField ?? activeSortConfig.value?.field);
  return field ? field : undefined;
});
const resolvedSortOrder = computed(() => {
  if (isMultipleSort.value) return undefined;
  if (!resolvedSortField.value) return undefined;
  return activeViewId.value
    ? activeSortConfig.value?.order
    : (props.sortOrder ?? activeSortConfig.value?.order);
});
const resolvedMultiSortMeta = computed(() => {
  if (!isMultipleSort.value) return undefined;
  if (!activeViewId.value && props.multiSortMeta?.length) {
    return props.multiSortMeta;
  }
  const field = activeViewId.value
    ? activeSortConfig.value?.field
    : (props.sortField ?? activeSortConfig.value?.field);
  const order = activeViewId.value
    ? activeSortConfig.value?.order
    : (props.sortOrder ?? activeSortConfig.value?.order);
  if (!field || order === undefined) return undefined;
  return [{ field, order: order as 1 | -1 }];
});
// PrimeVue DataTable caches sort state internally and ignores subsequent
// sortField/sortOrder prop changes. A reactive :key forces re-mount when
// the sort config changes so the new props are picked up as initial state.
const sortKey = computed(() => {
  if (isMultipleSort.value) {
    const meta = resolvedMultiSortMeta.value;
    return `multi_${meta?.map((item) => `${item.field}_${item.order}`).join("_") ?? ""}`;
  }
  return `${resolvedSortField.value ?? ""}_${resolvedSortOrder.value ?? ""}`;
});

const resolvedRows = computed(() => {
  if (props.rows !== undefined) return props.rows;
  if (props.preset)
    return PRESET_DEFAULTS[props.preset].rows as number | undefined;
  return undefined;
});

const resolvedScrollable = computed(() => {
  if (props.scrollable !== null) return props.scrollable;
  if (props.preset)
    return PRESET_DEFAULTS[props.preset].scrollable as boolean | undefined;
  return undefined;
});

const resolvedScrollHeight = computed(() => {
  if (props.scrollHeight !== undefined) return props.scrollHeight;
  if (props.preset)
    return PRESET_DEFAULTS[props.preset].scrollHeight as string | undefined;
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
const isPhone = useIsPhone();
const [DefineHeader, ReuseHeader] = createReusableTemplate();

const attachmentViewer = ref<InstanceType<typeof TableAttachmentViewer> | null>(
  null,
);

function openAttachments(item: unknown): void {
  attachmentViewer.value?.open(item);
}

// --- Table view management ---

const appliedColumns = ref<Column[]>([...props.columns]);
const activeViewId = ref<string>("");
const activeIsDefault = ref(false);
const viewConfigVisible = ref(false);
const activeSortConfig = ref<SortConfig | null>(null);
// The active view's phone card; null keeps the screen's `cardLayout`.
const activeCardConfig = ref<CardLayout | null>(null);

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

function onApplyViewConfig(
  columns: Column[],
  viewId: string,
  card: CardLayout | null,
) {
  appliedColumns.value = columns;
  activeViewId.value = viewId;
  activeCardConfig.value = card;
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
function buildColumnsPayload(): Array<{
  field: string;
  visible?: boolean;
  order?: number;
}> {
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
    (v) => v.userId === userId && v.page === props.page,
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
    activeCardConfig.value = viewStore.applyCardConfig(defaultView);
  } else {
    appliedColumns.value = [...props.columns];
    activeViewId.value = "";
    activeIsDefault.value = false;
    activeSortConfig.value = null;
    activeCardConfig.value = null;
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
watch(
  () => store.user?.id,
  (newUserId, oldUserId) => {
    if (newUserId && props.page && newUserId !== oldUserId) {
      ensureDefaultAndLoad();
    }
  },
  { immediate: true },
);

// --- Filter persistence ---
onMounted(async () => {
  if (props.page) {
    // If no user yet, trigger load when user becomes available
    if (!store.user?.id) {
      const unwatch = watch(
        () => store.user?.id,
        (userId) => {
          if (userId) {
            unwatch();
            ensureDefaultAndLoad();
          }
        },
      );
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

watch(
  () => props.columns,
  (newColumns) => {
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
  },
  { deep: true },
);

// Watch for user-driven changes to column visibility or order applied
// through TableViewConfig. The dialog mutates `appliedColumns` via its
// own local state and applies it back through onApplyViewConfig — but
// we also catch manual mutations here so any path that flips a column
// off or reorders it marks the state dirty for the row-click save.
watch(
  appliedColumns,
  () => {
    if (props.page && activeViewId.value !== "") {
      // Skip the initial assignment that loadDefaultView performs
      // (stateDirty is already false there). Other changes are user-driven.
      markStateDirty();
    }
  },
  { deep: true },
);

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

// The desktop table leaves out the columns meant only for cards.
const tableColumns = computed(() =>
  visibleColumns.value.filter((c) => !c.cardOnly),
);

// --- Totals ---

const columnsWithTotal = computed(() =>
  visibleColumns.value.filter((c) => c.total),
);

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
    .map((item) => resolveFieldValue(item, field))
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
      return items.filter((item) => {
        const value = resolveFieldValue(item, field);
        return value !== undefined && value !== null;
      }).length;
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

// Slots named filter-{key} render the inputs of `type: "slot"` filters.
const filterSlotNames = computed(() =>
  Object.keys(slots).filter((name) => name.startsWith("filter-")),
);

// Amounts and quantities are right-aligned so figures line up by place value.
function columnPt(col: Column) {
  const numeric =
    col.columnType === ColumnType.Currency ||
    col.columnType === ColumnType.Number
      ? "numeric-cell"
      : undefined;
  const truncate = col.truncate !== false ? "truncate-cell" : undefined;
  if (!numeric && !truncate) return undefined;
  return {
    headerCell: { class: numeric },
    bodyCell: { class: [truncate, numeric] },
  };
}

// --- Phone cards ---

// Cards replace the table on phones for list screens, or for any table
// that opts in, unless it relies on something a card cannot show
// (groups or expansion). Tablets keep the table.
const showCards = computed(() => {
  if (!isPhone.value || props.phoneLayout === "table") return false;
  if (props.rowGroupMode || props.expandedRows !== undefined) return false;
  return props.phoneLayout === "cards" || !!props.page;
});

// A card only reads as tappable when the screen handles row clicks.
const instance = getCurrentInstance();
const hasRowClickListener = !!instance?.vnode.props?.onRowClick;

// Selection (v-model:selection) and row-reorder reach DataTable as
// attrs; cards call the same consumer handlers.
function callAttrHandler(name: string, payload: unknown) {
  const handler = attrs[name];
  const handlers = Array.isArray(handler) ? handler : [handler];
  for (const fn of handlers) {
    if (typeof fn === "function") fn(payload);
  }
}

function onCardSelection(value: unknown[]) {
  callAttrHandler("onUpdate:selection", value);
}

function onCardRowReorder(event: CardRowReorderEvent) {
  callAttrHandler("onRowReorder", event);
}

const resolvedCardLayout = computed(() =>
  resolveCardLayout(
    activeCardConfig.value,
    props.cardLayout,
    visibleColumns.value,
  ),
);

// Cards sort by one field; in multiple-sort mode that is the first one.
const cardSort = computed<SortConfig | null>(() => {
  if (isMultipleSort.value) return resolvedMultiSortMeta.value?.[0] ?? null;
  if (!resolvedSortField.value || resolvedSortOrder.value === undefined)
    return null;
  return {
    field: resolvedSortField.value,
    order: resolvedSortOrder.value as 1 | -1,
  };
});

const sortableColumns = computed(() =>
  visibleColumns.value.filter((c) => c.sortable),
);

const sortSheetVisible = ref(false);

// The sort button shows the active sort, since cards have no headers.
const sortButtonIcon = computed(() => {
  if (!cardSort.value) return "pi pi-sort-alt";
  return cardSort.value.order === 1
    ? "pi pi-sort-amount-up-alt"
    : "pi pi-sort-amount-down";
});

const sortButtonLabel = computed(() => {
  const sort = cardSort.value;
  const col = sort && visibleColumns.value.find((c) => c.field === sort.field);
  if (!sort || !col) return t("tables.sort.open");
  return t("tables.sort.openActive", {
    column: col.header,
    direction:
      sort.order === 1
        ? t("tables.views.ascending")
        : t("tables.views.descending"),
  });
});

const cardTotals = computed<CardTotal[]>(() =>
  columnsWithTotal.value.map((col) => ({
    field: col.field,
    label: col.header,
    value: formatTotal(col),
  })),
);

// Consumer #body-{field} slots render the same values inside the cards.
// Cell templates, plus #card-{field} templates that apply only to cards.
const bodySlotNames = computed(() =>
  Object.keys(slots).filter(
    (name) => name.startsWith("body-") || name.startsWith("card-"),
  ),
);
</script>

<template>
  <!-- The filter bar, shared by the table header and the phone cards -->
  <DefineHeader>
    <TableFilter
      :key="clearKey"
      :config="filterConfig"
      :model-value="filterValues ?? {}"
      :body-width="filterBodyWidth"
      :result-count="items.length"
      :show-title="false"
      :show-action-labels="false"
      :show-filter-action="showFilterAction ?? showFilterActions"
      :show-clear-action="showClearAction ?? showFilterActions"
      :show-create="showCreate"
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
      <!-- Consumer actions and table view config appear before standard actions -->
      <template v-if="slots['action-prepend'] || page" #action-prepend>
        <slot name="action-prepend" />
        <Button
          v-if="page"
          icon="pi pi-cog"
          size="small"
          text
          rounded
          class="table-header-action"
          :aria-label="t('tables.views.configuration')"
          v-tooltip.top="t('tables.views.configuration')"
          @click="viewConfigVisible = true"
        />
        <Button
          v-if="showCards && sortableColumns.length"
          :icon="sortButtonIcon"
          size="small"
          text
          rounded
          class="table-header-action"
          aria-haspopup="dialog"
          :aria-expanded="sortSheetVisible"
          :aria-label="sortButtonLabel"
          @click="sortSheetVisible = true"
        />
      </template>
      <!-- Forward the #filter-{key} slots of type "slot" filter fields -->
      <template
        v-for="name in filterSlotNames"
        :key="name"
        #[name]="slotProps"
      >
        <slot :name="name" v-bind="slotProps" />
      </template>
    </TableFilter>
  </DefineHeader>

  <!-- Phones: one card per row -->
  <TableCardList
    v-if="showCards"
    :class="attrs.class"
    :items="items"
    :columns="visibleColumns"
    :layout="resolvedCardLayout"
    :data-key="dataKey"
    :paginator="resolvedPaginator ?? false"
    :rows="resolvedRows"
    :sort-field="cardSort?.field"
    :sort-order="cardSort?.order"
    :show-delete="showDeleteColumn"
    :can-delete="canDelete"
    :show-attachments="!!attachmentConfig"
    :totals="cardTotals"
    :loading="loading"
    :interactive="hasRowClickListener"
    :selectable="showSelectionColumn"
    :selection="(attrs.selection as unknown[] | null | undefined) ?? null"
    :reorderable="showRowReorderColumn"
    @row-click="onRowClick"
    @delete="emit('delete', $event)"
    @attachments="openAttachments"
    @update:selection="onCardSelection"
    @row-reorder="onCardRowReorder"
  >
    <template v-if="showFilters && filterConfig" #header>
      <ReuseHeader />
    </template>
    <template v-for="name in bodySlotNames" :key="name" #[name]="slotProps">
      <slot :name="name" v-bind="slotProps" />
    </template>
    <template v-if="slots.empty" #empty>
      <slot name="empty" />
    </template>
  </TableCardList>

  <DataTable
    v-else
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
    <template v-if="showFilters && filterConfig" #header>
      <ReuseHeader />
    </template>

    <!-- Selection system column -->
    <Column
      v-if="showSelectionColumn"
      selectionMode="multiple"
      :exportable="false"
      :style="{
        width: selectionColumnWidth,
        minWidth: selectionColumnWidth,
        maxWidth: selectionColumnWidth,
      }"
    />

    <!-- Row reorder system column -->
    <Column
      v-if="showRowReorderColumn"
      rowReorder
      :reorderableColumn="false"
      :headerStyle="{ width: rowReorderColumnWidth }"
      :style="{
        width: rowReorderColumnWidth,
        minWidth: rowReorderColumnWidth,
        maxWidth: rowReorderColumnWidth,
      }"
    />

    <!-- Dynamic columns -->
    <template v-for="col in tableColumns" :key="col.field">
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
        :frozen="col.frozen"
        :pt="columnPt(col)"
      >
        <!-- Custom body slot from consumer takes priority -->
        <template v-if="slots[`body-${col.field}`]" #body="slotProps">
          <slot :name="`body-${col.field}`" v-bind="slotProps" />
        </template>

        <!-- Boolean: not affected by truncation (its own component) -->
        <template
          v-else-if="col.columnType === ColumnType.Boolean"
          #body="slotProps"
        >
          <BooleanColumn
            :value="resolveBooleanValue(slotProps.data, col.field)"
            :show-color="col.showColor"
          />
        </template>
        <!-- Status: the resolved name as a Tag in the status colour. -->
        <template
          v-else-if="col.columnType === ColumnType.Status"
          #body="slotProps"
        >
          <Tag
            v-if="hasValue(resolveCellValue(col, slotProps.data))"
            :value="formatCellValue(col, slotProps.data)"
            :severity="statusSeverity(col, slotProps.data)"
            class="lifecycle-status-tag"
          />
        </template>
        <!-- Default + all text-typed columns: route through TruncatedCell.
           Default is true; opt out per column with `truncate: false`. -->
        <template v-else #body="slotProps">
          <TruncatedCell
            v-if="hasValue(resolveCellValue(col, slotProps.data))"
            :value="formatCellValue(col, slotProps.data)"
            :truncate="col.truncate !== false"
          />
        </template>
      </Column>
    </template>

    <!-- Read-only attachment action column -->
    <Column
      v-if="attachmentConfig"
      :pt="{
        bodyCell: { style: 'padding: 0 !important; position: relative;' },
      }"
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
    <Column
      v-if="showDeleteColumn"
      :pt="{
        bodyCell: { style: 'padding: 0 !important; position: relative;' },
      }"
      :style="{
        width: deleteColumnWidth,
        minWidth: deleteColumnWidth,
        maxWidth: deleteColumnWidth,
      }"
    >
      <template #body="slotProps">
        <div
          v-if="canDelete ? canDelete(slotProps.data) : true"
          class="delete-cell"
          @click.stop="emit('delete', slotProps.data)"
          v-tooltip.top="t('tables.cards.delete')"
        >
          <i class="pi pi-trash delete-icon"></i>
        </div>
      </template>
    </Column>

    <!-- Footer totals row (only if any column has total or footer slot) -->
    <ColumnGroup v-if="hasFooter" type="footer">
      <Row>
        <Column
          v-if="showRowReorderColumn"
          :style="{
            width: rowReorderColumnWidth,
            minWidth: rowReorderColumnWidth,
            maxWidth: rowReorderColumnWidth,
          }"
        />
        <Column
          v-for="col in tableColumns"
          :key="col.field"
          :style="col.style"
        >
          <template #footer>
            <slot
              v-if="slots[`footer-${col.field}`]"
              :name="`footer-${col.field}`"
            />
            <span v-else-if="col.total">{{ formatTotal(col) }}</span>
          </template>
        </Column>
        <Column
          v-if="attachmentConfig"
          class="attachment-column"
          style="width: 3rem; min-width: 3rem; max-width: 3rem"
        />
        <!-- Empty footer cell for delete column alignment -->
        <Column
          v-if="showDeleteColumn"
          class="delete-column"
          :style="{
            width: deleteColumnWidth,
            minWidth: deleteColumnWidth,
            maxWidth: deleteColumnWidth,
          }"
        />
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
    :card-layout="cardLayout"
    :active-card-config="activeCardConfig"
    :preview-item="items[0]"
    @apply-config="onApplyViewConfig"
    @update:sort-config="onSortConfigUpdate"
    @update:filter-values="emit('update:filterValues', $event)"
  />

  <TableSortSheet
    v-if="showCards"
    v-model:visible="sortSheetVisible"
    :columns="sortableColumns"
    :sort="cardSort"
    @update:sort="onSortConfigUpdate"
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
  transition:
    background-color 0.2s ease,
    color 0.2s ease;
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
  transition:
    background-color 0.2s ease,
    color 0.2s ease;
}

.delete-cell:hover {
  background-color: #ef4444;
  color: #ffffff;
}

.delete-icon {
  font-size: 0.8571rem;
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

/* Touch-sized header actions on phones and tablets. */
@media (max-width: 1024px) {
  .table-header-action.p-button.p-button-icon-only {
    width: 44px;
    height: 44px;
  }
}

.p-datatable .numeric-cell {
  text-align: right;
}

.p-datatable .numeric-cell .p-datatable-column-header-content {
  justify-content: flex-end;
}
</style>
