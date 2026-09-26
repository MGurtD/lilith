<template>
  <!-- Phones and tablets: a filter button with the applied filters as chips;
       the fields open in a bottom sheet (phone) or a side drawer (tablet). -->
  <div
    v-if="compact"
    class="table-filter table-filter--compact"
    :class="tableFilterClassName"
  >
    <div class="table-filter-compact__bar">
      <Button
        v-if="hasFilters"
        severity="secondary"
        outlined
        class="table-filter-compact__open"
        aria-haspopup="dialog"
        :aria-expanded="sheetVisible"
        :aria-label="t('tables.filters.openCount', activeFilters.length)"
        @click="openSheet"
      >
        <i class="pi pi-filter" aria-hidden="true"></i>
        <span>{{ t("tables.filters.title") }}</span>
        <span
          v-if="activeFilters.length"
          class="table-filter-compact__badge"
          aria-hidden="true"
          >{{ activeFilters.length }}</span
        >
      </Button>
      <div v-if="slots.prepend" class="table-filter-compact__lead">
        <slot name="prepend"></slot>
      </div>
      <span
        v-if="resultCount !== undefined"
        class="table-filter-compact__count"
        role="status"
        >{{ t("tables.filters.resultCount", resultCount) }}</span
      >
      <div class="table-filter-compact__actions">
        <slot name="action-prepend"></slot>
        <Button
          v-if="showCreate"
          icon="pi pi-plus"
          @click="$emit('create')"
          class="p-button-success table-filter-compact__create"
          rounded
          :aria-label="t('tables.filters.create')"
          v-tooltip.top="t('tables.filters.createTooltip')"
        />
        <slot name="append"></slot>
      </div>
    </div>

    <ul
      v-if="activeFilters.length"
      class="table-filter-compact__chips"
      :aria-label="t('tables.filters.active')"
    >
      <li v-for="filter in activeFilters" :key="filter.key">
        <button
          type="button"
          class="table-filter-compact__chip"
          :aria-label="t('tables.filters.editFilter', { label: filter.label })"
          @click="openSheet"
        >
          <span class="table-filter-compact__chip-label"
            >{{ filter.label }}{{ filter.value ? ":" : "" }}</span
          >
          <span v-if="filter.value">{{ filter.value }}</span>
        </button>
      </li>
      <li v-if="showClearAction">
        <button
          type="button"
          class="table-filter-compact__clear"
          @click="$emit('clear')"
        >
          {{ t("tables.filters.clear") }}
        </button>
      </li>
    </ul>

    <Drawer
      v-if="hasFilters"
      v-model:visible="sheetVisible"
      :position="isPhone ? 'bottom' : 'right'"
      :header="t('tables.filters.title')"
      blockScroll
      @after-hide="restoreFocus"
      :class="[
        'table-filter-sheet',
        isPhone ? 'table-filter-sheet--bottom' : 'table-filter-sheet--side',
      ]"
    >
      <div class="table-filter-sheet__fields">
        <div
          v-for="field in config"
          :key="field.key"
          class="table-filter__field table-filter-sheet__field"
        >
          <label
            v-if="field.label"
            :for="field.key"
            class="filter-label table-filter__label"
          >
            {{ field.label }}
          </label>
          <slot
            v-if="field.type === 'slot'"
            :name="`filter-${field.key}`"
            :field="field"
            :value="modelValue[field.key]"
            :update="(value: unknown) => updateField(field.key, value)"
          ></slot>
          <TableFilterField
            v-else
            :field="field"
            :value="modelValue[field.key]"
            @update:value="updateField(field.key, $event)"
          />
        </div>
      </div>
      <template #footer>
        <div class="table-filter-sheet__footer">
          <Button
            v-if="showClearAction"
            :label="t('tables.filters.clear')"
            severity="secondary"
            outlined
            @click="clearFromSheet"
          />
          <Button
            v-if="showFilterAction"
            :label="t('tables.filters.apply')"
            icon="pi pi-filter"
            @click="applyFromSheet"
          />
          <Button
            v-else
            :label="t('tables.filters.done')"
            @click="sheetVisible = false"
          />
        </div>
      </template>
    </Drawer>
  </div>

  <div v-else class="table-filter" :class="tableFilterClassName">
    <div class="table-filter__content">
      <div class="table-filter__row table-filter__row--main">
        <div v-if="showTitle" class="table-filter__title">
          <span class="table-filter__title-text">{{
            t("tables.filters.title")
          }}</span>
        </div>

        <div
          class="table-filter__body table-filter__body--inline"
          :class="{ 'table-filter__body--constrained': !!bodyWidth }"
          :style="bodyWidthStyle"
        >
          <!-- Leading content that is not a filter, e.g. a table title. -->
          <div v-if="slots.prepend" class="table-filter__lead">
            <slot name="prepend"></slot>
          </div>

          <div
            v-for="field in rows[0] ?? []"
            :key="field.key"
            class="table-filter__field"
            :class="`table-filter__field--${field.size ?? 'md'}`"
          >
            <label
              v-if="field.label"
              :for="field.key"
              class="filter-label table-filter__label"
            >
              {{ field.label }}
            </label>
            <slot
              v-if="field.type === 'slot'"
              :name="`filter-${field.key}`"
              :field="field"
              :value="modelValue[field.key]"
              :update="(value: unknown) => updateField(field.key, value)"
            ></slot>
            <TableFilterField
              v-else
              :field="field"
              :value="modelValue[field.key]"
              @update:value="updateField(field.key, $event)"
            />
          </div>
        </div>

        <div class="table-filter__actions">
          <slot name="action-prepend"></slot>
          <Button
            v-if="showFilterAction"
            :label="showActionLabels ? t('tables.filters.apply') : undefined"
            icon="pi pi-filter"
            @click="$emit('filter')"
            class="p-button-primary"
            size="small"
            rounded
            :aria-label="t('tables.filters.apply')"
            v-tooltip.top="t('tables.filters.apply')"
          />
          <Button
            v-if="showClearAction && hasFilters"
            :label="showActionLabels ? t('tables.filters.clear') : undefined"
            icon="pi pi-filter-slash"
            @click="$emit('clear')"
            class="p-button-secondary p-button-outlined"
            size="small"
            rounded
            :aria-label="t('tables.filters.clear')"
            v-tooltip.top="t('tables.filters.clearTooltip')"
          />
          <div
            v-if="showCreate"
            class="table-filter__divider border-left-1 border-300"
          ></div>
          <Button
            v-if="showCreate"
            :label="showActionLabels ? t('tables.filters.create') : undefined"
            icon="pi pi-plus"
            @click="$emit('create')"
            class="p-button-success"
            size="small"
            rounded
            :aria-label="t('tables.filters.create')"
            v-tooltip.top="t('tables.filters.createTooltip')"
          />
          <slot name="append"></slot>
        </div>
      </div>

      <div v-if="rows.length > 1" class="table-filter__body">
        <div
          v-for="(rowFields, rowIndex) in rows.slice(1)"
          :key="rowIndex"
          class="table-filter__row"
        >
          <div
            v-for="field in rowFields"
            :key="field.key"
            class="table-filter__field"
            :class="`table-filter__field--${field.size ?? 'md'}`"
          >
            <label
              v-if="field.label"
              :for="field.key"
              class="filter-label table-filter__label"
            >
              {{ field.label }}
            </label>
            <slot
              v-if="field.type === 'slot'"
              :name="`filter-${field.key}`"
              :field="field"
              :value="modelValue[field.key]"
              :update="(value: unknown) => updateField(field.key, value)"
            ></slot>
            <TableFilterField
              v-else
              :field="field"
              :value="modelValue[field.key]"
              @update:value="updateField(field.key, $event)"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PropType, computed, CSSProperties, ref, useSlots } from "vue";
import { useI18n } from "vue-i18n";
import { useIsCompact, useIsPhone } from "@/composables/useIsPhone";
import { activeFilterSummaries } from "./filterDisplay";
import TableFilterField from "./TableFilterField.vue";

const slots = useSlots();
const { t } = useI18n();

/**
 * One filter of a table. Every filter is declared here: `label` names it in
 * the filter bar, the phone filter sheet and saved views, and `valueLabel`
 * turns its value into readable text when the value alone is not (e.g. a
 * supplier id). Use `type: "slot"` for inputs that need their own component
 * (e.g. DropdownSupplier) and render it in the `#filter-{key}` slot, which
 * receives `{ field, value, update }`.
 */
export interface FilterConfig {
  key: string;
  label?: string;
  type:
    | "select"
    | "text"
    | "number"
    | "checkbox"
    | "multiselect"
    | "date"
    | "date-range"
    | "slot";
  options?: any[];
  optionLabel?: string;
  optionValue?: string;
  placeholder?: string;
  row?: number;
  /** Relative width: 'sm', 'md' (default), 'lg' or 'xl'. */
  size?: "sm" | "md" | "lg" | "xl";
  /** Readable text for the value in summaries (chips, saved views). */
  valueLabel?: (value: unknown) => string;
  /** Only for type: 'multiselect'. How selected items are displayed. Default: 'chip' */
  display?: "comma" | "chip";
  /** Only for type: 'multiselect'. Max labels shown before '+N more'. Default: 3 */
  maxSelectedLabels?: number;
  /** For type: 'select' and 'multiselect'. Show the search box. Default: true */
  filter?: boolean;
  /** Only for type: 'multiselect'. Show the select-all checkbox. Default: true */
  showToggleAll?: boolean;
}

export interface FilterBodyWidth {
  /** Max width on desktop (>= 1200px). Default: '100%' */
  desktop?: string;
  /** Max width on tablet (769px–1199px). Default: '100%' */
  tablet?: string;
}

const props = defineProps({
  config: {
    type: Array as PropType<FilterConfig[]>,
    default: () => [],
  },
  modelValue: {
    type: Object,
    required: true,
  },
  showCreate: {
    type: Boolean,
    default: true,
  },
  showTitle: {
    type: Boolean,
    default: true,
  },
  showActionLabels: {
    type: Boolean,
    default: true,
  },
  showFilterAction: {
    type: Boolean,
    default: true,
  },
  showClearAction: {
    type: Boolean,
    default: true,
  },
  embedded: {
    type: Boolean,
    default: false,
  },
  bodyWidth: {
    type: Object as PropType<FilterBodyWidth>,
    default: undefined,
  },
  /** Rows the filters return; shown next to the filter button on phones and tablets. */
  resultCount: {
    type: Number,
    default: undefined,
  },
});

const emit = defineEmits(["update:modelValue", "filter", "clear", "create"]);

const hasFilters = computed(() => props.config.length > 0);

const compact = useIsCompact();
const isPhone = useIsPhone();
const sheetVisible = ref(false);

// Applied filters as "Label: value" chips, from the same config as the fields.
const activeFilters = computed(() =>
  activeFilterSummaries(props.modelValue, props.config),
);

// The drawer does not give focus back when it closes: return it to the
// button or chip that opened the sheet.
let sheetOpener: HTMLElement | null = null;

const openSheet = () => {
  sheetOpener =
    document.activeElement instanceof HTMLElement
      ? document.activeElement
      : null;
  sheetVisible.value = true;
};

const restoreFocus = () => {
  if (sheetOpener?.isConnected) sheetOpener.focus();
  sheetOpener = null;
};

const applyFromSheet = () => {
  sheetVisible.value = false;
  emit("filter");
};

const clearFromSheet = () => {
  sheetVisible.value = false;
  emit("clear");
};

const updateField = (key: string, value: unknown): void => {
  emit("update:modelValue", {
    ...props.modelValue,
    [key]: value,
  });
};

// Fields grouped by their `row`; row 0 shares the line with the actions.
const rows = computed(() => {
  const grouped: Record<number, FilterConfig[]> = {};
  let maxRow = 0;

  props.config.forEach((field) => {
    const row = field.row || 0;
    if (!grouped[row]) grouped[row] = [];
    grouped[row].push(field);
    if (row > maxRow) maxRow = row;
  });

  const result: FilterConfig[][] = [];
  for (let i = 0; i <= maxRow; i++) {
    result.push(grouped[i] || []);
  }
  return result;
});

const bodyWidthStyle = computed<CSSProperties>(() => {
  if (!props.bodyWidth) return {};
  return {
    "--filter-body-max-desktop": props.bodyWidth.desktop || "100%",
    "--filter-body-max-tablet": props.bodyWidth.tablet || "100%",
  } as CSSProperties;
});

const tableFilterClassName = computed(() => ({
  "surface-section": !props.embedded,
  "shadow-1": !props.embedded,
  "border-round": !props.embedded,
  "table-filter--embedded": props.embedded,
  "table-filter--labelled-actions": props.showActionLabels,
}));
</script>

<style scoped>
.table-filter {
  padding: 0.65rem 0.85rem 0.75rem;
}

.table-filter--embedded {
  padding: 0;
  box-shadow: none;
}

.table-filter__content {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.table-filter__title {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  flex: 0 0 auto;
}

.table-filter__title-text {
  font-weight: 700;
  color: var(--p-text-color);
}

.table-filter__actions {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  flex-wrap: wrap;
  margin-left: auto;
}

.table-filter--labelled-actions .table-filter__actions :deep(.p-button) {
  justify-content: center;
  min-width: 6.75rem;
}

.table-filter__divider {
  margin-inline: 0.15rem;
  height: 1.5rem;
}

.table-filter__body {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.table-filter__body--inline {
  flex: 1 1 auto;
  min-width: 0;
  flex-direction: row;
  align-items: end;
  align-self: stretch;
  gap: 0.65rem;
}

.table-filter__body--constrained {
  --filter-body-max-desktop: 100%;
  --filter-body-max-tablet: 100%;
}

.table-filter__row {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}

.table-filter__row--main {
  flex-direction: row;
  align-items: end;
  gap: 0.75rem;
}

.table-filter__lead {
  display: flex;
  align-items: center;
  align-self: center;
  min-width: 0;
}

.table-filter__field {
  display: flex;
  flex-direction: column;
  gap: 0.18rem;
  flex: 1 1 10rem;
  min-width: 8rem;
}

.table-filter__field--sm {
  flex: 0.7 1 8rem;
}

.table-filter__field--lg {
  flex: 1.25 1 12rem;
}

.table-filter__field--xl {
  flex: 1.6 1 14rem;
}

.table-filter__label {
  margin-bottom: 0;
  font-family: var(--font-condensed);
  font-size: 0.9286rem;
  font-weight: 500;
  line-height: 1.2;
  color: var(--p-text-muted-color);
}

.table-filter :deep(.p-multiselect) {
  display: flex;
  align-items: center;
  padding-block: 0.25rem;
}

.table-filter :deep(.p-multiselect-label-container) {
  padding: 0;
  overflow: hidden;
  max-height: 1.75rem;
  display: flex;
  align-items: center;
  flex-wrap: nowrap;
}

.table-filter :deep(.p-multiselect-chip-item) {
  font-size: 0.8571rem;
  padding-block: 0;
  padding-inline: 0.4rem;
  border-radius: 0.25rem;
  gap: 0.25rem;
  line-height: 1.5;
}

.table-filter :deep(.p-multiselect-chip-label) {
  font-size: 0.8571rem;
}

.table-filter :deep(.p-multiselect-chip-icon) {
  font-size: 0.8rem;
}

/* Phones and tablets (compact). */
.table-filter--compact {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

.table-filter-compact__bar {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  min-width: 0;
}

.table-filter-compact__open {
  gap: 0.5rem;
  min-height: 44px;
  flex-shrink: 0;
}

.table-filter-compact__badge {
  display: inline-grid;
  place-items: center;
  min-width: 1.4286rem;
  height: 1.4286rem;
  padding: 0 0.4286rem;
  border-radius: 999px;
  background: var(--p-primary-color);
  color: var(--p-primary-contrast-color);
  font-size: 0.8571rem;
  font-weight: 600;
  line-height: 1;
}

.table-filter-compact__lead {
  min-width: 0;
}

.table-filter-compact__count {
  font-size: 0.9286rem;
  color: var(--p-text-muted-color);
  white-space: nowrap;
}

.table-filter-compact__actions {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  margin-left: auto;
}

.table-filter-compact__create {
  width: 44px;
  height: 44px;
}

.table-filter-compact__chips {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.table-filter-compact__chip {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  min-height: 2.25rem;
  padding: 0 0.75rem;
  border: 1px solid var(--p-surface-200);
  border-radius: 999px;
  background: var(--p-surface-50);
  color: var(--p-text-color);
  font: inherit;
  font-size: 0.9286rem;
  white-space: nowrap;
  cursor: pointer;
}

.table-filter-compact__chip:hover {
  background: var(--p-surface-100);
}

.table-filter-compact__chip-label {
  color: var(--p-text-muted-color);
}

.table-filter-compact__clear {
  min-height: 2.25rem;
  padding: 0 0.5rem;
  border: 0;
  background: transparent;
  color: var(--p-primary-color);
  font: inherit;
  font-size: 0.9286rem;
  font-weight: 500;
  cursor: pointer;
}

.table-filter-compact__chip:focus-visible,
.table-filter-compact__clear:focus-visible {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 1px;
}

/* Phones: one chip row that scrolls sideways, so the table stays in view. */
@media (max-width: 767.98px) {
  .table-filter-compact__chips {
    flex-wrap: nowrap;
    overflow-x: auto;
    scrollbar-width: none;
  }

  .table-filter-compact__chips > li {
    flex-shrink: 0;
  }
}

.table-filter-sheet__fields {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.table-filter-sheet__field {
  flex: none;
  min-width: 0;
}

.table-filter-sheet__footer {
  display: flex;
  gap: 0.75rem;
}

.table-filter-sheet__footer :deep(.p-button) {
  flex: 1;
  min-height: 44px;
}

@media (min-width: 769px) {
  .table-filter__row {
    flex-direction: row;
    align-items: end;
  }

  .table-filter__body--constrained {
    max-width: var(--filter-body-max-tablet);
  }
}

@media (min-width: 1200px) {
  .table-filter__body--constrained {
    max-width: var(--filter-body-max-desktop);
  }
}
</style>

<style>
/* The filter sheet is teleported to <body>, so it is styled globally. */
.p-drawer.table-filter-sheet--bottom {
  height: auto;
  max-height: 85dvh;
  border-radius: var(--p-border-radius-xl) var(--p-border-radius-xl) 0 0;
}

.p-drawer.table-filter-sheet--side {
  width: min(24rem, 100vw);
}

.table-filter-sheet .p-drawer-title {
  font-family: var(--font-condensed);
  font-size: 1.4286rem;
  font-weight: 600;
}

/* Touch-sized inputs inside the sheet. */
.table-filter-sheet .p-inputtext,
.table-filter-sheet .p-select,
.table-filter-sheet .p-multiselect,
.table-filter-sheet .p-inputnumber {
  min-height: 2.75rem;
}

.table-filter-sheet .p-select,
.table-filter-sheet .p-multiselect {
  align-items: center;
}
</style>
