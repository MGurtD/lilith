<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      filter
      :options="plantModelStore.workcenters ?? []"
      optionValue="id"
      optionLabel="description"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #option="{ option }">
        <div v-if="option" class="flex align-items-center">
          {{ option.description }}
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { usePlantModelStore } from "../store/plantmodel";

const plantModelStore = usePlantModelStore();

defineProps<{
  label: string;
  modelValue: string | null | undefined;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();
</script>
