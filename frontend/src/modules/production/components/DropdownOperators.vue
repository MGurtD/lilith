<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      filter
      :options="plantModelStore.operators"
      optionValue="id"
      :optionLabel="formatLabel"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #option="{ option }">
        <div v-if="option" class="flex align-items-center">
          {{ formatLabel(option) }}
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { usePlantModelStore } from "../store/plantmodel";
import { Operator } from "../types";

const plantModelStore = usePlantModelStore();

const formatLabel = (operator: Operator) => {
  return `${operator.name} ${operator.surname}`;
};

defineProps<{
  label: string;
  modelValue: string | null | undefined;
  referenceId?: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();
</script>
