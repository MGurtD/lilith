<template>
  <div class="table-filter" :class="tableFilterClassName">
    <div class="table-filter__content">
      <div class="table-filter__row table-filter__row--main">
        <div v-if="showTitle" class="table-filter__title">
          <span v-if="showTitle" class="table-filter__title-text">Filtres</span>
        </div>

        <div
          class="table-filter__body table-filter__body--inline"
          :class="{ 'table-filter__body--constrained': !!bodyWidth }"
          :style="bodyWidthStyle"
        >
          <div v-if="rows.length > 0" class="table-filter__prepend">
            <slot name="prepend"></slot>
          </div>

          <div
            v-for="field in rows[0] ?? []"
            :key="field.key"
            class="table-filter__field min-w-0"
            :style="{ flex: fieldFlex(field.size) }"
          >
            <label
              v-if="field.label"
              :for="field.key"
              class="filter-label table-filter__label"
            >
              {{ field.label }}
            </label>

            <Select
              v-if="field.type === 'select'"
              :id="field.key"
              :model-value="fieldValue(field.key)"
              :options="field.options"
              :optionLabel="field.optionLabel || 'label'"
              :optionValue="field.optionValue || 'value'"
              :placeholder="field.placeholder || 'Selecciona...'"
              class="w-full"
              size="small"
              showClear
              :filter="true"
              @update:model-value="updateField(field.key, $event)"
            />

            <InputText
              v-else-if="field.type === 'text'"
              :id="field.key"
              :model-value="stringValue(field.key)"
              :placeholder="field.placeholder"
              class="w-full"
              size="small"
              @update:model-value="updateField(field.key, $event)"
            />

            <InputNumber
              v-else-if="field.type === 'number'"
              :id="field.key"
              :model-value="numberValue(field.key)"
              :placeholder="field.placeholder"
              class="w-full"
              size="small"
              @update:model-value="updateField(field.key, $event)"
            />

            <div v-else-if="field.type === 'checkbox'" class="table-filter__checkbox">
              <Checkbox
                :inputId="field.key"
                :model-value="booleanValue(field.key)"
                :binary="true"
                @update:model-value="updateField(field.key, $event)"
              />
            </div>
          </div>
        </div>

        <div class="table-filter__actions">
          <slot name="action-prepend"></slot>
          <Button
            v-if="showFilterAction"
            :label="showActionLabels ? 'Filtrar' : undefined"
            icon="pi pi-filter"
            @click="$emit('filter')"
            class="p-button-primary"
            size="small"
            rounded
            aria-label="Filtrar"
            v-tooltip.top="'Filtrar'"
          />
          <Button
            v-if="hasFilters"
            :label="showActionLabels ? 'Netejar' : undefined"
            icon="pi pi-filter-slash"
            @click="$emit('clear')"
            class="p-button-secondary p-button-outlined"
            size="small"
            rounded
            aria-label="Netejar"
            v-tooltip.top="'Netejar filtres'"
          />
          <div
            v-if="showCreate"
            class="table-filter__divider border-left-1 border-300"
          ></div>
          <Button
            v-if="showCreate"
            :label="showActionLabels ? 'Nou' : undefined"
            icon="pi pi-plus"
            @click="$emit('create')"
            class="p-button-success"
            size="small"
            rounded
            aria-label="Nou"
            v-tooltip.top="'Crear nou'"
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
            class="table-filter__field min-w-0"
            :style="{ flex: fieldFlex(field.size) }"
          >
            <label
              v-if="field.label"
              :for="field.key"
              class="filter-label table-filter__label"
            >
              {{ field.label }}
            </label>

            <!-- Select / Dropdown -->
            <Select
              v-if="field.type === 'select'"
              :id="field.key"
              :model-value="fieldValue(field.key)"
              :options="field.options"
              :optionLabel="field.optionLabel || 'label'"
              :optionValue="field.optionValue || 'value'"
              :placeholder="field.placeholder || 'Selecciona...'"
              class="w-full"
              size="small"
              showClear
              :filter="true"
              @update:model-value="updateField(field.key, $event)"
            />

            <!-- Text Input -->
            <InputText
              v-else-if="field.type === 'text'"
              :id="field.key"
              :model-value="stringValue(field.key)"
              :placeholder="field.placeholder"
              class="w-full"
              size="small"
              @update:model-value="updateField(field.key, $event)"
            />

            <!-- Number Input -->
            <InputNumber
              v-else-if="field.type === 'number'"
              :id="field.key"
              :model-value="numberValue(field.key)"
              :placeholder="field.placeholder"
              class="w-full"
              size="small"
              @update:model-value="updateField(field.key, $event)"
            />

            <div v-else-if="field.type === 'checkbox'" class="table-filter__checkbox">
              <Checkbox
                :inputId="field.key"
                :model-value="booleanValue(field.key)"
                :binary="true"
                @update:model-value="updateField(field.key, $event)"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PropType, computed, CSSProperties, useSlots } from "vue";

const slots = useSlots();

export interface FilterConfig {
  key: string;
  label?: string;
  type: "select" | "text" | "number" | "checkbox";
  options?: any[];
  optionLabel?: string;
  optionValue?: string;
  placeholder?: string;
  row?: number;
  /** Flex sizing: 'sm' = 0.5, 'md' = 1 (default), 'lg' = 1.5, 'xl' = 2 */
  size?: "sm" | "md" | "lg" | "xl";
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
  embedded: {
    type: Boolean,
    default: false,
  },
  bodyWidth: {
    type: Object as PropType<FilterBodyWidth>,
    default: undefined,
  },
});

const emit = defineEmits(["update:modelValue", "filter", "clear", "create"]);

const hasFilters = computed(() => props.config.length > 0 || !!slots.prepend);

const sizeMap: Record<string, string> = {
  sm: "0.5",
  md: "1",
  lg: "1.5",
  xl: "2",
};

const fieldFlex = (size?: string): string => {
  return sizeMap[size || "md"] || "1";
};

const fieldValue = (key: string): unknown => props.modelValue[key];

const stringValue = (key: string): string | undefined => {
  const value = fieldValue(key);
  return typeof value === "string" ? value : undefined;
};

const numberValue = (key: string): number | null | undefined => {
  const value = fieldValue(key);
  return typeof value === "number" ? value : undefined;
};

const booleanValue = (key: string): boolean => {
  return fieldValue(key) === true;
};

const updateField = (key: string, value: unknown): void => {
  emit("update:modelValue", {
    ...props.modelValue,
    [key]: value,
  });
};

const rows = computed(() => {
  const grouped: Record<number, FilterConfig[]> = {};
  let maxRow = 0;

  props.config.forEach((field) => {
    const row = field.row || 0;
    if (!grouped[row]) grouped[row] = [];
    grouped[row].push(field);
    if (row > maxRow) maxRow = row;
  });

  const result = [];
  for (let i = 0; i <= maxRow; i++) {
    // Even if empty (e.g. if row 1 is skipped but 2 exists), we might want to preserve structure,
    // but for now let's just push existing ones or empty array.
    // If we want to guarantee DatePicker in row 0, we must ensure row 0 exists in the loop.
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

.table-filter__prepend {
  display: contents;
}

.table-filter__field {
  display: flex;
  flex-direction: column;
  gap: 0.18rem;
}

.table-filter__label {
  margin-bottom: 0;
  font-size: 0.68rem;
  line-height: 1;
}

.table-filter__checkbox {
  min-height: 2.375rem;
  display: flex;
  align-items: center;
}

.table-filter :deep(.p-inputtext),
.table-filter :deep(.p-select-label),
.table-filter :deep(.p-inputnumber-input),
.table-filter :deep(.p-datepicker-input) {
  font-size: 0.875rem;
  line-height: 1.25rem;
  padding-top: 0.55rem;
  padding-bottom: 0.55rem;
}

.table-filter :deep(.p-select),
.table-filter :deep(.p-inputtext),
.table-filter :deep(.p-inputnumber),
.table-filter :deep(.p-datepicker-input) {
  min-height: 2.375rem;
}

.table-filter :deep(.p-select-dropdown),
.table-filter :deep(.p-datepicker-dropdown) {
  width: 2.375rem;
}

.table-filter :deep(.table-filter-prepend-field) {
  display: flex;
  flex-direction: column;
  gap: 0.18rem;
  flex: 1 1 10rem;
  min-width: 8rem;
}

.table-filter :deep(.table-filter-prepend-field--sm) {
  flex: 0.7 1 8rem;
}

.table-filter :deep(.table-filter-prepend-field--md) {
  flex: 1 1 10rem;
}

.table-filter :deep(.table-filter-prepend-field--lg) {
  flex: 1.25 1 12rem;
}

.table-filter :deep(.table-filter-prepend-field--xl) {
  flex: 1.6 1 14rem;
}

.table-filter :deep(.table-filter-prepend-label) {
  margin-bottom: 0;
  font-size: 0.68rem;
  line-height: 1;
}

@media (max-width: 768px) {
  .table-filter {
    padding-inline: 0.75rem;
  }

  .table-filter--embedded {
    padding: 0;
  }

  .table-filter__row--main,
  .table-filter__body--inline {
    flex-direction: column;
    align-items: flex-start;
  }

  .table-filter__actions {
    width: 100%;
    margin-left: 0;
  }
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
