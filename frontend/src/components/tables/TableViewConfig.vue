<script setup lang="ts">
import { ref, computed, watch } from "vue";
import type { Column } from "./types";
import { useUserTableViewStore } from "@/store/usertableview";
import type { SortConfig } from "@/store/usertableview";
import { useStore } from "@/store";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";


const props = defineProps<{
  visible: boolean;
  columns: Column[];
  page: string;
  activeViewId?: string;
  filterValues?: any;
  activeSortConfig?: SortConfig | null;
}>();

const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "apply-config", columns: Column[], viewId: string): void;
  (e: "update:filterValues", value: any): void;
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
      selectedViewId.value = props.activeViewId ?? "";
    }
  }
);

// Current views for dropdown
const views = computed(() => viewStore.views);

// Include default application view as first option
const selectOptions = computed(() => [
  { id: "", name: "Per defecte (aplicació)", isDefault: false, viewConfig: '{"columns":[]}' },
  ...views.value,
]);

// Selected view object
const selectedView = computed(() => {
  if (!selectedViewId.value) return null;
  return views.value.find((v) => v.id === selectedViewId.value) ?? null;
});

// Whether there are any saved views in the database
const hasSavedViews = computed(() => views.value.length > 0);

// When selected view changes, apply its configuration immediately
watch(selectedViewId, (newId) => {
  if (newId) {
    const view = views.value.find((v) => v.id === newId);
    if (view) {
      // Apply the stored configuration to local columns
      localColumns.value = viewStore.applyView(view, props.columns);
      emit("apply-config", localColumns.value, newId);
      // Apply filter configuration if present
      const filterValues = viewStore.applyFilterConfig(view);
      if (filterValues) {
        emit("update:filterValues", filterValues);
      }
      // Apply sort configuration if present
      const sortConfig = viewStore.applySortConfig(view);
      localSortField.value = sortConfig?.field ?? "";
      localSortOrder.value = (sortConfig?.order ?? 1) as 1 | -1;
      emit("update:sortConfig", sortConfig);
    }
  } else {
    // Reset to base columns
    localColumns.value = props.columns.map((col, index) => ({
      ...col,
      visible: col.visible !== false ? true : col.visible,
      order: col.order ?? index,
    }));
    emit("apply-config", props.columns, "");
    // Reset sort
    localSortField.value = "";
    localSortOrder.value = 1;
    emit("update:sortConfig", null);
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

  const newView = viewStore.createNewView(
    userId,
    props.page,
    newViewName.value.trim(),
    localColumns.value,
    props.filterValues
  );

  const created = await viewStore.create(newView);
  if (created) {
    toast.add({
      severity: "success",
      summary: "Vista creada",
      detail: "La nova vista s'ha creat correctament",
      life: 3000,
    });
    // Select the newly created view
    const savedName = newViewName.value.trim();
    const savedView = viewStore.views.find((v) => v.name === savedName);
    if (savedView) {
      selectedViewId.value = savedView.id;
    }
    newViewName.value = "";
  }
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

  confirm.require({
    message: `Està segur que vol eliminar la vista "${selectedView.value.name}"?`,
    header: "Confirmació",
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const deleted = await viewStore.delete(selectedView.value!.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Vista eliminada",
          life: 3000,
        });
        selectedViewId.value = "";
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
              :disabled="col.visible === undefined && col.visible !== false"
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
