<script setup lang="ts">
import { ref, computed, watch } from "vue";
import type { Column } from "./types";
import type { FilterConfig } from "./TableFilter.vue";
import { useUserTableViewStore } from "@/store/usertableview";
import type { SortConfig } from "@/store/usertableview";
import { useStore } from "@/store";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { formatDate } from "@/utils/functions";
import { hydrateFilter } from "@/utils/filter-hydrate";


const props = defineProps<{
  visible: boolean;
  columns: Column[];
  page: string;
  activeViewId?: string;
  filterValues?: any;
  activeSortConfig?: SortConfig | null;
  filterConfig?: FilterConfig[];
  /**
   * User-friendly labels for filter keys that are NOT in `filterConfig`
   * (typically prepend-slot filters like DropdownCustomers).
   * Without this, prepend keys fall back to their raw key name
   * (e.g. "customerId" instead of "Client").
   */
  filterLabels?: Record<string, string>;
  /**
   * Optional resolvers for keys whose value is an ID (UUID/foreign key)
   * and needs to be translated to a human-readable label.
   * Used for prepend-slot filters like DropdownCustomers where the
   * options aren't passed in `filterConfig` because they are loaded
   * lazily by the dropdown component itself.
   * Receives the raw value, returns the display string.
   */
  filterValueResolvers?: Record<string, (value: unknown) => string>;
}>();

const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "apply-config", columns: Column[], viewId: string): void;
  (e: "update:filterValues", value: any): void;
  (e: "filter"): void;
  (e: "update:sortConfig", value: SortConfig | null): void;
}>();

const confirm = useConfirm();
const toast = useToast();
const store = useStore();
const viewStore = useUserTableViewStore();

// Local column configuration (clone for editing)
const localColumns = ref<Column[]>([]);
const selectedViewId = ref<string>("");
const newViewName = ref("");
const isDragging = ref(false);
const dragSourceIndex = ref<number | null>(null);
const dragOverIndex = ref<number | null>(null);
const newViewNameInput = ref<HTMLInputElement | null>(null);

// Local sort config
const localSortField = ref<string>("");
const localSortOrder = ref<1 | -1>(1);

// Cycle sort state for a column: none → asc → desc → none
// Emits immediately for hot reload
function cycleSortForColumn(field: string) {
  if (localSortField.value !== field) {
    localSortField.value = field;
    localSortOrder.value = 1;
  } else if (localSortOrder.value === 1) {
    localSortOrder.value = -1;
  } else {
    localSortField.value = "";
    localSortOrder.value = 1;
  }
  emit(
    "update:sortConfig",
    localSortField.value
      ? { field: localSortField.value, order: localSortOrder.value }
      : null
  );
}

// --- Saved filters (read-only) ---

// Source of truth is the table's live filterValues: the dialog mirrors the
// filters currently applied to the table so applying or clearing filters
// while the dialog is open immediately updates the section. The previous
// implementation read from the persisted viewConfig, which lagged behind
// any unsaved filter changes.
//
// `hydrateFilter` converts ISO date strings back into Date objects so
// downstream formatters see native Dates (matches the apply/restore paths
// in `usertableview.ts`).
const savedFilters = computed<Record<string, unknown> | null>(() => {
  const raw = props.filterValues;
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return null;
  return hydrateFilter(raw as Record<string, unknown>);
});

// Build a map of filter config by key for O(1) lookup
const filterConfigByKey = computed(() => {
  const map = new Map<string, FilterConfig>();
  for (const f of props.filterConfig ?? []) {
    map.set(f.key, f);
  }
  return map;
});

// Resolved display rows: pairs of (label, value) for the template.
// Label resolution priority:
//   1. FilterConfig.label (if the key is declared in filterConfig)
//   2. props.filterLabels[key] (for prepend-slot filters like DropdownCustomers)
//   3. key (raw fallback, e.g. for stale entries from removed filters)
const savedFilterRows = computed(() => {
  if (!savedFilters.value) return [];
  const labels = props.filterLabels ?? {};
  const rows: Array<{ key: string; label: string; field: FilterConfig | null; value: unknown }> = [];
  for (const [key, value] of Object.entries(savedFilters.value)) {
    const field = filterConfigByKey.value.get(key) ?? null;
    const label = field?.label ?? labels[key] ?? key;
    rows.push({ key, label, field, value });
  }
  return rows;
});

// Resolved display value: applies an external resolver if one was provided
// for this key (e.g. customerId → "Acme Corp"). Used by the template so
// prepend-slot filters with no options in filterConfig can still show
// human-readable text instead of raw UUIDs.
function resolveDisplayValue(key: string, field: FilterConfig | null, value: unknown): string {
  const resolvers = props.filterValueResolvers ?? {};
  const resolver = resolvers[key];
  if (resolver && !Array.isArray(value)) {
    try {
      const resolved = resolver(value);
      if (resolved) return resolved;
    } catch {
      // fall through to default formatter
    }
  }
  return formatFilterValue(field, value);
}

// Format a single date (Date object or ISO string) using the project's
// standard formatDate. Falls back to the raw string if parsing fails.
function formatDateValue(value: unknown): string {
  if (value === null || value === undefined || value === "") return "—";
  if (value instanceof Date) {
    return Number.isNaN(value.getTime()) ? "—" : formatDate(value);
  }
  if (typeof value === "string" && value) {
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? "—" : formatDate(parsed);
  }
  if (typeof value === "object") return "—";
  return String(value ?? "");
}

// Format a stored filter value for display. Uses the FilterConfig metadata
// (label, type, options) to render the value the same way the user sees it
// in the TableFilter inputs. Empty values render as "—" so the user can
// distinguish "no filter" from "filter with empty value".
//
// Special case: date-range arrays (e.g. PrimeVue `selectionMode="range"`
// DatePicker) are formatted as "dd/MM/yyyy — dd/MM/yyyy" rather than raw
// JSON. Detected by: field is null/unknown AND value is an array of length 2
// where each entry parses to a Date.
function formatFilterValue(field: FilterConfig | null, value: unknown): string {
  if (value === null || value === undefined || value === "") return "—";
  if (value === false) return "No";

  // Date range: array of two date-like entries, no matching field config
  // (PrimeVue range DatePicker is typically a prepend-slot filter).
  // Treat empty or invalid endpoints as "no filter" (—) so an
  // accidentally-initialised empty range doesn't render as "[{},{}]".
  const isDateLike = (entry: unknown): boolean =>
    entry instanceof Date ||
    typeof entry === "string" ||
    entry === null ||
    entry === undefined ||
    (typeof entry === "object" && entry !== null);

  if (Array.isArray(value) && value.length === 2 &&
      isDateLike(value[0]) && isDateLike(value[1])) {
    const start = formatDateValue(value[0]);
    const end = formatDateValue(value[1]);
    if (start === "—" || end === "—") return "—";
    return `${start} — ${end}`;
  }

  if (!field) {
    if (value instanceof Date) return formatDate(value);
    if (typeof value === "string" || typeof value === "number") {
      return String(value);
    }
    return JSON.stringify(value);
  }

  if (field.type === "checkbox") {
    return value === true ? "Sí" : "No";
  }

  if (field.type === "select") {
    const optionValue = field.optionValue ?? "value";
    const optionLabel = field.optionLabel ?? "label";
    const option = (field.options ?? []).find(
      (o) => (o as Record<string, unknown>)[optionValue] === value,
    );
    return option
      ? String((option as Record<string, unknown>)[optionLabel] ?? value)
      : String(value);
  }

  if (field.type === "multiselect") {
    if (!Array.isArray(value)) return String(value);
    const optionValue = field.optionValue ?? "value";
    const optionLabel = field.optionLabel ?? "label";
    const labels = value.map((v) => {
      const option = (field.options ?? []).find(
        (o) => (o as Record<string, unknown>)[optionValue] === v,
      );
      return option
        ? String((option as Record<string, unknown>)[optionLabel] ?? v)
        : String(v);
    });
    return labels.join(", ");
  }

  if (field.type === "number") {
    return typeof value === "number" ? String(value) : String(value);
  }

  if (value instanceof Date) {
    return formatDate(value);
  }

  return String(value);
}

// Initialize local columns when dialog opens
watch(
  () => props.visible,
  async (newVal) => {
    if (newVal) {
      // Clone columns for local editing
      localColumns.value = props.columns.map((col, index) => ({
        ...col,
        visible: col.visible !== false ? true : col.visible,
        order: col.order ?? index,
      }));
      // Clear the new view name field — it is for creating only
      newViewName.value = "";
      // Initialize sort from active sort config
      localSortField.value = props.activeSortConfig?.field ?? "";
      localSortOrder.value = (props.activeSortConfig?.order ?? 1) as 1 | -1;
      // Load views for this user and page
      const userId = store.user?.id;
      if (userId) {
        await viewStore.fetchViews(userId, props.page);
      }
      // Set selected view after views are loaded so the watch can find it
      const activeView = props.activeViewId
        ? viewStore.views.find((view) => view.id === props.activeViewId)
        : undefined;
      const selectedView =
        activeView ??
        viewStore.views.find((view) => view.isDefault) ??
        viewStore.views[0];
      selectedViewId.value = selectedView?.id ?? "";
    }
  }
);

// Current views for dropdown
const views = computed(() => viewStore.views);

// User-saved views only. The synthetic "Per defecte (aplicació)" entry was
// removed: with the autoprovision guarantee, every (user, page) pair
// already has a real "Per defecte" view by the time the dialog opens.
const selectOptions = computed(() => [...views.value]);

// Selected view object
const selectedView = computed(() => {
  if (!selectedViewId.value) return null;
  return views.value.find((v) => v.id === selectedViewId.value) ?? null;
});

// Whether there are any saved views in the database
const hasSavedViews = computed(() => views.value.length > 0);

// Apply a stored view locally: columns + filters + sort, plus emit the
// authoritative apply-config event so Table.vue updates its active view.
// Centralized here so save / select / delete all share the same path.
//
// `clearMissingFilters` distinguishes explicit user-initiated view
// changes (selector dropdown) from implicit system re-assignments
// (saveAsNewView, deleteView fallback). Only the explicit path should
// clear live filters when the destination view has no persisted state
// — otherwise programmatic fallbacks wipe filters the user just set and
// `buildViewConfig` then writes them out as empty.
function applyViewLocally(viewId: string, clearMissingFilters = false) {
  const view = views.value.find((v) => v.id === viewId);
  if (!view) return;

  localColumns.value = viewStore.applyView(view, props.columns);
  emit("apply-config", localColumns.value, viewId);

  const filterValues = viewStore.applyFilterConfig(view);
  if (filterValues) {
    emit("update:filterValues", filterValues);
    emit("filter");
  } else if (clearMissingFilters && viewId !== props.activeViewId) {
    // The user explicitly switched to a view that has no persisted
    // filters — mirror that on the live state so the UI matches.
    emit("update:filterValues", {});
    emit("filter");
  }

  const sortConfig = viewStore.applySortConfig(view);
  localSortField.value = sortConfig?.field ?? "";
  localSortOrder.value = (sortConfig?.order ?? 1) as 1 | -1;
  emit("update:sortConfig", sortConfig);
}

// Reset to the base columns (no view selected). Used when the active
// view is removed and no replacement should be applied — restores the
// table to its default look and clears live filters/sort.
function resetToBaseColumns() {
  localColumns.value = props.columns.map((col, index) => ({
    ...col,
    visible: col.visible !== false ? true : col.visible,
    order: col.order ?? index,
  }));
  emit("apply-config", props.columns, "");
  emit("update:filterValues", {});
  emit("filter");
  localSortField.value = "";
  localSortOrder.value = 1;
  emit("update:sortConfig", null);
}

// When the user picks a different view from the selector, apply it.
// Only this path passes `clearMissingFilters = true`; programmatic
// updates (saveAsNewView, deleteView fallback) bypass the watcher
// and call `applyViewLocally(viewId, false)` directly to avoid
// clobbering filters the user just configured.
watch(selectedViewId, (newId) => {
  if (newId) {
    applyViewLocally(newId, true);
  } else {
    resetToBaseColumns();
  }
});



// Drag and drop handlers
function onDragStart(index: number, event: DragEvent) {
  isDragging.value = true;
  dragSourceIndex.value = index;
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = "move";
    // Set drag image to the row itself
    const row = event.target as HTMLElement;
    if (row) {
      event.dataTransfer.setDragImage(row, 0, 0);
    }
  }
}

function onDragOver(index: number, event: DragEvent) {
  event.preventDefault();
  if (event.dataTransfer) {
    event.dataTransfer.dropEffect = "move";
  }
  dragOverIndex.value = index;
}

function onDragLeave(index: number, event: DragEvent) {
  // Only clear if we're leaving the row itself, not entering a child
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
  const x = event.clientX;
  const y = event.clientY;
  if (x < rect.left || x > rect.right || y < rect.top || y > rect.bottom) {
    dragOverIndex.value = null;
  }
}

function onDrop(targetIndex: number, event: DragEvent) {
  event.preventDefault();
  if (dragSourceIndex.value !== null && dragSourceIndex.value !== targetIndex) {
    const sourceIndex = dragSourceIndex.value;
    const columns = [...localColumns.value];
    const [movedColumn] = columns.splice(sourceIndex, 1);
    columns.splice(targetIndex, 0, movedColumn);

    // Update order values
    columns.forEach((col, idx) => {
      col.order = idx;
    });

    localColumns.value = columns;
  }
  isDragging.value = false;
  dragSourceIndex.value = null;
  dragOverIndex.value = null;
}

function onDragEnd() {
  isDragging.value = false;
  dragSourceIndex.value = null;
  dragOverIndex.value = null;
}

// Toggle column visibility
function toggleVisibility(index: number) {
  const column = localColumns.value[index];
  column.visible = column.visible === false ? true : false;
}

// Toggle column total
function toggleTotal(index: number) {
  const column = localColumns.value[index];
  if (column.total) {
    column.total = undefined;
  } else {
    column.total = "sum";
  }
}

// Save view (update existing) — only saves column/filter config, does NOT rename
async function saveView() {
  if (selectedViewId.value === "" || !selectedView.value) {
    toast.add({
      severity: "warn",
      summary: "Seleccioni una vista",
      detail: "Seleccioni una vista existent per actualitzar",
      life: 3000,
    });
    return;
  }

  const viewConfig = buildViewConfig();
  const updated = await viewStore.update(selectedView.value.id, {
    ...selectedView.value,
    viewConfig,
  });

  if (updated) {
    toast.add({
      severity: "success",
      summary: "Vista actualitzada",
      detail: "La configuració s'ha desat correctament",
      life: 3000,
    });
    emit("apply-config", localColumns.value, selectedViewId.value);
  }
}

// Save as new view
async function saveAsNewView() {
  const userId = store.user?.id;
  if (!userId) return;

  if (!newViewName.value.trim()) {
    toast.add({
      severity: "warn",
      summary: "Nom requerit",
      detail: "Introdueix un nom per a la nova vista",
      life: 3000,
    });
    return;
  }

  const savedName = newViewName.value.trim();
  const newView = viewStore.createNewView(
    userId,
    props.page,
    savedName,
    localColumns.value,
    props.filterValues,
    localSortField.value
      ? { field: localSortField.value, order: localSortOrder.value }
      : undefined,
  );

  const created = await viewStore.create(newView);
  if (!created) return;

  // viewStore.create already refreshes the views list. The newly created
  // view should now be present — apply it explicitly through the same path
  // the selector uses, so Table.vue picks up the new active view on mount
  // and F5 reads it as the single source of truth.
  const savedView = viewStore.views.find((v) => v.name === savedName);
  if (savedView) {
    applyViewLocally(savedView.id);
  }

  toast.add({
    severity: "success",
    summary: "Vista creada",
    detail: "La nova vista s'ha creat correctament",
    life: 3000,
  });
  newViewName.value = "";
}

// Delete current view
function deleteView() {
  if (selectedViewId.value === "" || !selectedView.value) {
    toast.add({
      severity: "warn",
      summary: "Seleccioni una vista",
      detail: "Seleccioni una vista per eliminar",
      life: 3000,
    });
    return;
  }

  const deletedViewId = selectedView.value.id;
  const wasActiveInTable = deletedViewId === props.activeViewId;

  confirm.require({
    message: `Està segur que vol eliminar la vista "${selectedView.value.name}"?`,
    header: "Confirmació",
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const deleted = await viewStore.delete(deletedViewId);
      if (!deleted) return;

      toast.add({
        severity: "success",
        summary: "Vista eliminada",
        life: 3000,
      });

      // If the deleted view was the one Table.vue was applying, fall
      // back to the next default/remaining view. If none survives, the
      // user explicitly removed their default — force a hard reset so
      // the live filters/sort/columns in Table.vue clear immediately
      // (the same state F5 would load on next mount).
      const nextView =
        viewStore.views.find((view) => view.isDefault) ?? viewStore.views[0];
      if (nextView) {
        selectedViewId.value = nextView.id;
      } else {
        const userId = store.user?.id;
        if (userId) {
          await viewStore.ensureDefault(userId, props.page);
          const fallback =
            viewStore.views.find((view) => view.isDefault) ??
            viewStore.views[0] ??
            null;
          if (fallback) {
            selectedViewId.value = fallback.id;
          } else if (wasActiveInTable) {
            // Truly empty: no views survived. Reset the live UI now
            // so the user sees the same state they'd get on F5.
            selectedViewId.value = "";
            resetToBaseColumns();
          }
        } else if (wasActiveInTable) {
          selectedViewId.value = "";
          resetToBaseColumns();
        }
      }
    },
  });
}

// Toggle default view on/off
async function toggleDefault() {
  if (selectedViewId.value === "" || !selectedView.value) {
    toast.add({
      severity: "warn",
      summary: "Seleccioni una vista",
      detail: "Seleccioni una vista per establir com per defecte",
      life: 3000,
    });
    return;
  }

  const isDefault = selectedView.value.isDefault;
  const set = await viewStore.setDefault(selectedView.value.id, !isDefault);
  if (set) {
    toast.add({
      severity: "success",
      summary: isDefault ? "Vista normal" : "Vista per defecte",
      detail: isDefault
        ? "La vista ja no és la predeterminada"
        : "La vista s'ha establert com a per defecte",
      life: 3000,
    });
  }
}

// Build unified view config JSON from local columns, filter values, and sort
function buildViewConfig(): string {
  const columns = localColumns.value
    .filter((col) => col.order !== undefined || col.visible === false)
    .map((col) => ({
      field: col.field,
      visible: col.visible,
      order: col.order,
      total: col.total,
    }));
  const config: Record<string, unknown> = { columns };
  if (props.filterValues) {
    config.filters = props.filterValues;
  }
  if (localSortField.value) {
    config.sort = { field: localSortField.value, order: localSortOrder.value };
  }
  return JSON.stringify(config);
}
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="emit('update:visible', $event)"
    header="Configuració de la vista"
    :modal="true"
    :closable="true"
    :style="{ width: '600px' }"
    class="table-view-config-dialog"
  >
    <div class="view-config-content">
      <!-- Top: Current view management -->
      <div v-if="hasSavedViews" class="view-management-section">
        <label>Vista actual</label>
        <div class="view-management-row">
          <div class="view-selector-wrapper">
            <Select
              v-model="selectedViewId"
              :options="selectOptions"
              option-label="name"
              option-value="id"
              placeholder="Selecciona una vista..."
              class="w-full"
              size="small"
            >
              <template #option="slotProps">
                <div class="select-option">
                  <span>{{ slotProps.option.name }}</span>
                  <i
                    v-if="slotProps.option.isDefault"
                    class="pi pi-star-fill default-star"
                    title="Vista per defecte"
                  />
                </div>
              </template>
            </Select>
            <Button
              v-if="selectedViewId"
              :icon="selectedView?.isDefault ? 'pi pi-star-fill' : 'pi pi-star'"
              :severity="selectedView?.isDefault ? 'warning' : 'secondary'"
              size="small"
              rounded
              text
              :aria-label="selectedView?.isDefault ? 'Treure de per defecte' : 'Establir com a per defecte'"
              @click="toggleDefault"
              v-tooltip.top="selectedView?.isDefault ? 'Treure de per defecte' : 'Establir com a per defecte'"
            />
          </div>
          <div class="view-management-actions">
            <Button
              icon="pi pi-save"
              severity="secondary"
              size="small"
              rounded
              text
              aria-label="Desar canvis"
              v-tooltip.top="'Desar canvis'"
              @click="saveView"
              :disabled="selectedViewId === ''"
            />
            <Button
              icon="pi pi-trash"
              severity="danger"
              size="small"
              rounded
              text
              aria-label="Eliminar"
              v-tooltip.top="'Eliminar'"
              @click="deleteView"
              :disabled="selectedViewId === ''"
            />
          </div>
        </div>
      </div>

      <!-- Saved filters (read-only, sourced from DB via selectedView.viewConfig) -->
      <div v-if="savedFilterRows.length > 0" class="saved-filters-section">
        <label>Filtres desats</label>
        <div class="saved-filters-list">
          <div
            v-for="row in savedFilterRows"
            :key="row.key"
            class="saved-filters-row"
          >
            <span class="saved-filters-label">{{ row.label }}</span>
            <span class="saved-filters-value">
              {{ resolveDisplayValue(row.key, row.field, row.value) }}
            </span>
          </div>
        </div>
      </div>

      <!-- Column configuration list -->
      <div class="field">
        <label>Configuració de columnes</label>
        <div class="columns-config-list">
          <div
            v-for="(col, index) in localColumns"
            :key="col.field"
            class="column-config-row"
            :class="{
              'dragging': dragSourceIndex === index,
              'drag-over': dragOverIndex === index && dragSourceIndex !== index,
              'drop-before': dragOverIndex === index && dragSourceIndex !== null && dragSourceIndex > index,
              'drop-after': dragOverIndex === index && dragSourceIndex !== null && dragSourceIndex < index,
            }"
            draggable="true"
            @dragstart="onDragStart(index, $event)"
            @dragover="onDragOver(index, $event)"
            @dragleave="onDragLeave(index, $event)"
            @drop="onDrop(index, $event)"
            @dragend="onDragEnd"
          >
            <i class="pi pi-bars drag-handle"></i>
            <Checkbox
              :model-value="col.visible !== false"
              :binary="true"
              @update:model-value="toggleVisibility(index)"
            />
            <span class="column-name">{{ col.header }}</span>
            <Checkbox
              v-if="col.total !== undefined"
              :model-value="col.total !== undefined"
              :binary="true"
              @update:model-value="toggleTotal(index)"
            />
            <span v-if="col.total !== undefined" class="total-label">Total</span>
            <!-- Sort toggle -->
            <Button
              :icon="
                localSortField !== col.field
                  ? 'pi pi-sort'
                  : localSortOrder === 1
                    ? 'pi pi-sort-amount-up'
                    : 'pi pi-sort-amount-down'
              "
              :severity="localSortField === col.field ? 'primary' : 'secondary'"
              size="small"
              rounded
              text
              :aria-label="
                localSortField !== col.field
                  ? 'Sense ordenació'
                  : localSortOrder === 1
                    ? 'Ascendent'
                    : 'Descendent'
              "
              v-tooltip.top="
                localSortField !== col.field
                  ? 'Sense ordenació'
                  : localSortOrder === 1
                    ? 'Ascendent'
                    : 'Descendent'
              "
              @click.stop="cycleSortForColumn(col.field)"
            />
          </div>
        </div>
      </div>

      <!-- Bottom: Create new view -->
      <div class="create-view-section">
        <label>Nova vista</label>
        <div class="create-view-row">
          <InputText
            ref="newViewNameInput"
            v-model="newViewName"
            placeholder="Nom de la nova vista..."
            class="w-full"
            size="small"
          />
          <Button
            icon="pi pi-plus"
            severity="success"
            size="small"
            rounded
            text
            aria-label="Crear nova vista"
            v-tooltip.top="'Crear nova vista'"
            @click="saveAsNewView"
          />
        </div>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.view-config-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field label {
  font-weight: 600;
  color: var(--text-color-secondary);
}

.view-management-section,
.create-view-section {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.view-management-section label,
.create-view-section label {
  font-weight: 600;
  color: var(--text-color-secondary);
}

.view-management-row {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.view-selector-wrapper {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  flex: 1;
}

.view-selector-wrapper .p-select {
  flex: 1;
}

.view-management-actions {
  display: flex;
  gap: 0.5rem;
}

.view-management-actions button,
.view-selector-wrapper button {
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.view-management-actions button:disabled,
.view-selector-wrapper button:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.create-view-row {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.create-view-row .p-inputtext {
  flex: 1;
}

.select-option {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.saved-filters-section {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.saved-filters-section label {
  font-weight: 600;
  color: var(--text-color-secondary);
}

.saved-filters-list {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid var(--surface-border);
  border-radius: var(--border-radius);
  padding: 0.5rem 0.75rem;
  background: var(--surface-ground);
}

.saved-filters-row {
  display: flex;
  gap: 0.75rem;
  align-items: baseline;
  font-size: 0.875rem;
  padding: 0.15rem 0;
}

.saved-filters-label {
  color: var(--text-color-secondary);
  font-weight: 500;
  min-width: 8rem;
  flex-shrink: 0;
}

.saved-filters-value {
  color: var(--text-color);
  word-break: break-word;
  flex: 1;
}

.default-star {
  color: var(--p-primary-color);
  font-size: 0.75rem;
}

.columns-config-list {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid var(--surface-border);
  border-radius: var(--border-radius);
  padding: 0.25rem;
}

.column-config-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  border-bottom: 1px solid var(--surface-border);
  cursor: grab;
  background: var(--surface-ground);
  transition: all 0.2s ease;
  position: relative;
  border-radius: 4px;
  margin-bottom: 2px;
}

.column-config-row:last-child {
  border-bottom: none;
  margin-bottom: 0;
}

.column-config-row:hover {
  background: var(--surface-hover);
}

.column-config-row.dragging {
  opacity: 0.5;
  background: var(--highlight-bg);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  z-index: 10;
}

.column-config-row.drag-over {
  background: var(--surface-hover);
}

/* Drop indicator lines */
.column-config-row.drop-before::before {
  content: '';
  position: absolute;
  top: -2px;
  left: 0;
  right: 0;
  height: 3px;
  background: var(--p-primary-color);
  border-radius: 2px;
  z-index: 5;
}

.column-config-row.drop-after::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  right: 0;
  height: 3px;
  background: var(--p-primary-color);
  border-radius: 2px;
  z-index: 5;
}

.drag-handle {
  color: var(--text-color-secondary);
  cursor: grab;
  transition: color 0.2s ease;
}

.column-config-row:hover .drag-handle {
  color: var(--text-color);
}

.column-config-row.dragging .drag-handle {
  color: var(--p-primary-color);
}

.column-name {
  flex: 1;
  font-size: 0.875rem;
}

.total-label {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
}

</style>
