<template>
  <div class="table-filter surface-section px-3 py-3 shadow-2 border-round">
    <div class="flex flex-column gap-2">
      <!-- Header / Actions Row -->
      <div class="flex justify-content-between align-items-center">
        <div class="flex align-items-center gap-2">
          <Button
            @click="isExpanded = !isExpanded"
            :icon="isExpanded ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
            class="p-button-text p-button-rounded p-button-secondary"
            :aria-label="isExpanded ? 'Collapse filters' : 'Expand filters'"
          />
          <span class="font-bold text-900">Filtres</span>

          <!-- Active Filters Chips (Visible when collapsed) -->
          <div v-if="!isExpanded" class="flex gap-2 ml-2 hidden md:flex">
            <Chip
              v-if="activeFiltersCount > 0"
              :label="`${activeFiltersCount} actius`"
              class="bg-primary text-white"
            />
          </div>
        </div>

        <div class="flex gap-2">
          <Button
            v-if="isExpanded"
            label="Filtrar"
            icon="pi pi-filter"
            @click="$emit('filter')"
            class="p-button-primary"
          />
          <Button
            v-if="isExpanded"
            label="Netejar"
            icon="pi pi-filter-slash"
            @click="$emit('clear')"
            class="p-button-secondary p-button-outlined"
          />
          <div v-if="isExpanded" class="border-left-1 border-300 mx-2"></div>
          <Button
            v-if="showCreate"
            label="Nou"
            icon="pi pi-plus"
            @click="$emit('create')"
            class="p-button-success"
          />
          <slot name="append"></slot>
        </div>
      </div>

      <!-- Collapsible Filter Content -->
      <div
        v-if="isExpanded"
        class="flex flex-column gap-2 fadein animation-duration-300"
      >
        <div
          v-for="(rowFields, rowIndex) in rows"
          :key="rowIndex"
          class="flex flex-column md:flex-row md:align-items-end gap-3"
        >
          <!-- Slot for custom filters (like DatePickers) - Only in first row -->
          <slot name="prepend" v-if="rowIndex === 0"></slot>

          <!-- Dynamic Filters for this row -->
          <div
            v-for="field in rowFields"
            :key="field.key"
            class="min-w-0"
            :style="{ flex: fieldFlex(field.size) }"
          >
            <label v-if="field.label" :for="field.key" class="filter-label">
              {{ field.label }}
            </label>

            <!-- Select / Dropdown -->
            <Select
              v-if="field.type === 'select'"
              :id="field.key"
              v-model="modelValue[field.key]"
              :options="field.options"
              :optionLabel="field.optionLabel || 'label'"
              :optionValue="field.optionValue || 'value'"
              :placeholder="field.placeholder || 'Selecciona...'"
              class="w-full"
              showClear
              :filter="true"
            />

            <!-- Text Input -->
            <InputText
              v-else-if="field.type === 'text'"
              :id="field.key"
              v-model="modelValue[field.key]"
              :placeholder="field.placeholder"
              class="w-full"
            />

            <!-- Number Input -->
            <InputNumber
              v-else-if="field.type === 'number'"
              :id="field.key"
              v-model="modelValue[field.key]"
              :placeholder="field.placeholder"
              class="w-full"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PropType, ref, computed } from "vue";

export interface FilterConfig {
  key: string;
  label?: string;
  type: "select" | "text" | "number";
  options?: any[];
  optionLabel?: string;
  optionValue?: string;
  placeholder?: string;
  row?: number;
  /** Flex sizing: 'sm' = 0.5, 'md' = 1 (default), 'lg' = 1.5, 'xl' = 2 */
  size?: "sm" | "md" | "lg" | "xl";
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
});

defineEmits(["update:modelValue", "filter", "clear", "create"]);

const isExpanded = ref(true);

const sizeMap: Record<string, string> = {
  sm: "0.5",
  md: "1",
  lg: "1.5",
  xl: "2",
};

const fieldFlex = (size?: string): string => {
  return sizeMap[size || "md"] || "1";
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

const activeFiltersCount = computed(() => {
  let count = 0;
  // Check dynamic fields
  props.config.forEach((field) => {
    const val = props.modelValue[field.key];
    if (val !== undefined && val !== null && val !== "") {
      count++;
    }
  });
  return count;
});
</script>

<style scoped>
/* Responsive improvements if needed */
@media (max-width: 768px) {
  .table-filter .flex-row {
    flex-direction: column !important;
  }
}
</style>
