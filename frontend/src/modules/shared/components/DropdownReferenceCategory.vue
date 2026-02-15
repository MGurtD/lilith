<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      :options="referenceStore.referenceCategories"
      placeholder="Selecciona..."
      optionValue="code"
      optionLabel="description"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
    </Select>
  </div>
</template>
<script setup lang="ts">
import { useReferenceStore } from "../store/reference";

const props = defineProps<{
  label: string;
  modelValue: string | null | undefined;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();

const referenceStore = useReferenceStore();
</script>
