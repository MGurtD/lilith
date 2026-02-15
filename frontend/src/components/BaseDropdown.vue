<script setup lang="ts">
import { BaseInputType } from "../types/component";

defineProps<{
  type?: BaseInputType;
  id?: string;
  label?: string;
  icon?: string;
  modelValue: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();
</script>
<template>
  <div class="base_input">
    <label
      v-if="label"
      :for="id"
      class="base_input__label block text-900 mb-2"
      >{{ label }}</label
    >
    <span :class="{ 'p-input-icon-left': icon !== undefined }" class="mb-3">
      <i v-if="icon" :class="icon" />
      <InputText
        v-if="!type || type === BaseInputType.TEXT"
        :id="id"
        class="w-full"
        v-bind:model-value="modelValue"
        @input="
          emit('update:modelValue', ($event.target as HTMLInputElement).value)
        "
      />
    </span>
  </div>
</template>
<style scoped>
.base_input__label {
  text-align: left;
}
</style>
