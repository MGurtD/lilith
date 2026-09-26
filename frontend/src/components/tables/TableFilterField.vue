<template>
  <DatePicker
    v-if="field.type === 'date-range'"
    :id="field.key"
    :model-value="dateRangeValue"
    selectionMode="range"
    dateFormat="dd/mm/yy"
    :placeholder="field.placeholder"
    showIcon
    class="w-full"
    size="small"
    @update:model-value="emit('update:value', $event)"
  />

  <DatePicker
    v-else-if="field.type === 'date'"
    :id="field.key"
    :model-value="dateValue"
    dateFormat="dd/mm/yy"
    :placeholder="field.placeholder"
    showIcon
    class="w-full"
    size="small"
    @update:model-value="emit('update:value', $event)"
  />

  <Select
    v-else-if="field.type === 'select'"
    :id="field.key"
    :model-value="value"
    :options="field.options"
    :optionLabel="field.optionLabel || 'label'"
    :optionValue="field.optionValue || 'value'"
    :placeholder="field.placeholder || t('tables.filters.selectPlaceholder')"
    class="w-full"
    size="small"
    showClear
    :filter="field.filter ?? true"
    @update:model-value="emit('update:value', $event)"
  />

  <InputText
    v-else-if="field.type === 'text'"
    :id="field.key"
    :model-value="stringValue"
    :placeholder="field.placeholder"
    class="w-full"
    size="small"
    @update:model-value="emit('update:value', $event)"
  />

  <InputNumber
    v-else-if="field.type === 'number'"
    :id="field.key"
    :model-value="numberValue"
    :placeholder="field.placeholder"
    class="w-full"
    size="small"
    @update:model-value="emit('update:value', $event)"
  />

  <div v-else-if="field.type === 'checkbox'" class="table-filter__checkbox">
    <Checkbox
      :inputId="field.key"
      :model-value="value === true"
      :binary="true"
      @update:model-value="emit('update:value', $event)"
    />
  </div>

  <MultiSelect
    v-else-if="field.type === 'multiselect'"
    :inputId="field.key"
    :model-value="arrayValue"
    :options="field.options"
    :optionLabel="field.optionLabel || 'label'"
    :optionValue="field.optionValue || 'value'"
    :placeholder="field.placeholder || t('tables.filters.selectPlaceholder')"
    :display="field.display || 'chip'"
    :maxSelectedLabels="field.maxSelectedLabels ?? 3"
    :filter="field.filter ?? true"
    :showToggleAll="field.showToggleAll ?? true"
    showClear
    class="w-full"
    size="small"
    @update:model-value="emit('update:value', $event)"
  />
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import type { FilterConfig } from "./TableFilter.vue";

const props = defineProps<{
  field: FilterConfig;
  value: unknown;
}>();

const emit = defineEmits<{
  (event: "update:value", value: unknown): void;
}>();

const { t } = useI18n();

const stringValue = computed(() =>
  typeof props.value === "string" ? props.value : undefined,
);
const numberValue = computed(() =>
  typeof props.value === "number" ? props.value : undefined,
);
const arrayValue = computed(() =>
  Array.isArray(props.value) ? props.value : [],
);
const dateValue = computed(() =>
  props.value instanceof Date ? props.value : undefined,
);
const dateRangeValue = computed(() =>
  Array.isArray(props.value) ? (props.value as (Date | null)[]) : undefined,
);
</script>

<style scoped>
.table-filter__checkbox {
  min-height: 2.375rem;
  display: flex;
  align-items: center;
}
</style>
