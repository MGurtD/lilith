<script setup lang="ts">
import { computed, useSlots, useAttrs, ref, watch, onMounted, onUnmounted } from "vue";
import TableFilter from "./TableFilter.vue";
import type { FilterConfig, FilterBodyWidth } from "./TableFilter.vue";
import TableViewConfig from "./TableViewConfig.vue";
import { useStore } from "@/store";
import { useUserTableViewStore } from "@/store/usertableview";

export type Aggregation = "sum" | "avg" | "count" | "min" | "max";

export type TablePreset = "crud-list" | "read-only" | "detail-lines" | "selector";

export interface Column {
  field: string;
  header: string;
  sortable?: boolean;
  total?: Aggregation;
  totalFormat?: (value: number) => string;
  visible?: boolean;
  order?: number;
  style?: string;
}

const PRESET_DEFAULTS: Record<TablePreset, Record<string, unknown>> = {
  "crud-list": {
    selectionMode: "single",
    paginator: "auto",
    rows: 20,
    scrollable: true,
    scrollHeight: "flex",
    stripedRows: true,
    rowHover: true,
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
    page?: string;
    showDeleteColumn?: boolean;
    canDelete?: (item: any) => boolean;
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
  // Exclude paginator, rows, scrollable, scrollHeight from the spread —
  // they are bound explicitly in the template to avoid PrimeVue coercion issues.
  const { paginator: _p, rows: _r, scrollable: _s, scrollHeight: _sh, ...presetRest } = preset;
  return { ...presetRest, ...explicit, ...attrs };
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
}>();

const slots = useSlots();
const attrs = useAttrs();
const store = useStore();
const viewStore = useUserTableViewStore();

// --- Table view management ---

const appliedColumns = ref<Column[]>([...props.columns]);
const activeViewId = ref<string>("");
const viewConfigVisible = ref(false);

function onApplyViewConfig(columns: Column[], viewId: string) {
  appliedColumns.value = columns;
  activeViewId.value = viewId;
  // Apply filter config if present
  const view = viewStore.views.find((v) => v.id === viewId);
  if (view) {
    const filterValues = viewStore.applyFilterConfig(view);
    if (filterValues) {
      emit("update:filterValues", filterValues);
      emit("filter");
    }
  }
}

async function loadDefaultView() {
  const userId = store.user?.id;
  if (!userId || !props.page) return;

  await viewStore.fetchViews(userId, props.page);
  const defaultView = viewStore.getDefaultView(userId, props.page);
  if (defaultView) {
    appliedColumns.value = viewStore.applyView(defaultView, props.columns);
    activeViewId.value = defaultView.id;
    // Apply filter config if present
    const filterValues = viewStore.applyFilterConfig(defaultView);
    if (filterValues) {
      emit("update:filterValues", filterValues);
      emit("filter");
    }
  } else {
    appliedColumns.value = [...props.columns];
    activeViewId.value = "";
  }
}

// Watch for user authentication to load default view after page refresh
watch(() => store.user?.id, (newUserId, oldUserId) => {
  if (newUserId && props.page && newUserId !== oldUserId) {
    loadDefaultView();
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
          loadDefaultView().then(restoreFiltersIfNeeded);
        }
      });
    } else {
      await loadDefaultView();
      restoreFiltersIfNeeded();
    }
  }
});

onUnmounted(() => {
  if (props.page && activeViewId.value === "" && props.filterValues) {
    const serialized: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(props.filterValues)) {
      if (value instanceof Date) {
        serialized[key] = value.toISOString();
      } else if (Array.isArray(value)) {
        serialized[key] = value.map((v) => v instanceof Date ? v.toISOString() : v);
      } else {
        serialized[key] = value;
      }
    }
    localStorage.setItem(`lilith-table-filters-${props.page}`, JSON.stringify(serialized));
  }
});

function restoreFiltersIfNeeded() {
  if (props.page && activeViewId.value === "") {
    const saved = localStorage.getItem(`lilith-table-filters-${props.page}`);
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        // Convert date strings back to Date objects
        const deserialized: Record<string, unknown> = {};
        for (const [key, value] of Object.entries(parsed)) {
          if (typeof value === 'string' && value.match(/^\d{4}-\d{2}-\d{2}T/)) {
            deserialized[key] = new Date(value);
          } else if (Array.isArray(value)) {
            deserialized[key] = value.map((v) =>
              typeof v === 'string' && v.match(/^\d{4}-\d{2}-\d{2}T/) ? new Date(v) : v
            );
          } else {
            deserialized[key] = value;
          }
        }
        emit("update:filterValues", deserialized);
        emit("filter");
      } catch {
        // Invalid JSON, ignore
      }
    }
  }
}

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
</script>

<template>
  <DataTable
    showGridlines
    v-bind="resolvedDataTableProps"
    :value="items"
    :paginator="resolvedPaginator"
    :rows="resolvedRows"
    :scrollable="resolvedScrollable"
    :scrollHeight="resolvedScrollHeight"
  >
    <!-- TableFilter embedded in DataTable's native header slot -->
    <template
      v-if="showFilters && filterConfig"
      #header
    >
      <TableFilter
        :config="filterConfig"
        :model-value="filterValues"
        :body-width="filterBodyWidth"
        :show-title="false"
        :show-action-labels="false"
        embedded
        @update:model-value="emit('update:filterValues', $event)"
        @filter="emit('filter')"
        @clear="emit('clear')"
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
    <Column
      v-for="col in visibleColumns"
      :key="col.field"
      :field="col.field"
      :header="col.header"
      :sortable="col.sortable ?? false"
      :style="col.style"
    >
      <template v-if="slots[`body-${col.field}`]" #body="slotProps">
        <slot :name="`body-${col.field}`" v-bind="slotProps" />
      </template>
    </Column>

    <!-- Delete action column -->
    <Column v-if="showDeleteColumn" :pt="{ bodyCell: { style: 'padding: 0 !important; position: relative;' } }" style="width: 3rem; min-width: 3rem; max-width: 3rem">
      <template #body="slotProps">
        <div
          v-if="canDelete ? canDelete(slotProps.data) : true"
          class="delete-cell"
          @click="emit('delete', slotProps.data)"
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
    @apply-config="onApplyViewConfig"
  />
</template>

<style scoped>
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
