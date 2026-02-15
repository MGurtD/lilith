<script setup lang="ts">
import { BaseInputType } from "../types/component";
import Password from "primevue/password";
import InputGroup from "primevue/inputgroup";
import InputGroupAddon from "primevue/inputgroupaddon";

defineProps<{
  type?: BaseInputType;
  id?: string;
  label?: string;
  icon?: string;
  decimals?: number;
  modelValue: string | number | null;
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

    <!-- Input with icon using InputGroup -->
    <InputGroup v-if="icon" class="mb-2 w-full">
      <InputGroupAddon>
        <i :class="icon" />
      </InputGroupAddon>

      <InputText
        v-if="!type || type === BaseInputType.TEXT"
        :id="id"
        class="flex-1"
        v-bind="$attrs"
        v-bind:model-value="modelValue as string"
        @input="
          emit('update:modelValue', ($event.target as HTMLInputElement).value)
        "
      />
      <InputNumber
        v-else-if="type === BaseInputType.NUMERIC"
        :id="id"
        class="flex-1"
        :minFractionDigits="decimals ?? 0"
        v-bind="$attrs"
        locale="en-US"
        v-bind:model-value="modelValue as number"
        @input="emit('update:modelValue', $event.value as any)"
      />
      <InputNumber
        v-else-if="type === BaseInputType.CURRENCY"
        :id="id"
        class="flex-1"
        :minFractionDigits="2"
        suffix=" €"
        locale="en-US"
        v-bind="$attrs"
        v-bind:model-value="modelValue as number"
        @input="emit('update:modelValue', $event.value as any)"
      />
      <Password
        v-else-if="type === BaseInputType.PASSWORD"
        :id="id"
        class="flex-1"
        v-bind="$attrs"
        v-bind:model-value="modelValue as string"
        @input="
          emit('update:modelValue', ($event.target as HTMLInputElement).value)
        "
      />
    </InputGroup>

    <!-- Input without icon -->
    <span v-else class="mb-2 w-full block">
      <InputText
        v-if="!type || type === BaseInputType.TEXT"
        :id="id"
        class="w-full"
        v-bind="$attrs"
        v-bind:model-value="modelValue as string"
        @input="
          emit('update:modelValue', ($event.target as HTMLInputElement).value)
        "
      />
      <InputNumber
        v-else-if="type === BaseInputType.NUMERIC"
        :id="id"
        class="w-full"
        :minFractionDigits="decimals ?? 0"
        v-bind="$attrs"
        locale="en-US"
        v-bind:model-value="modelValue as number"
        @input="emit('update:modelValue', $event.value as any)"
      />
      <InputNumber
        v-else-if="type === BaseInputType.CURRENCY"
        :id="id"
        class="w-full"
        :minFractionDigits="2"
        suffix=" €"
        locale="en-US"
        v-bind="$attrs"
        v-bind:model-value="modelValue as number"
        @input="emit('update:modelValue', $event.value as any)"
      />
      <Password
        v-else-if="type === BaseInputType.PASSWORD"
        :id="id"
        class="w-full"
        v-bind="$attrs"
        v-bind:model-value="modelValue as string"
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

/* Ensure InputGroup takes full width and handles flex properly */
.base_input :deep(.p-inputgroup) {
  display: flex;
  width: 100%;
}

.base_input :deep(.p-inputgroup > .p-inputtext),
.base_input :deep(.p-inputgroup > .p-inputnumber),
.base_input :deep(.p-inputgroup > .p-password) {
  flex: 1;
  width: 1%;
}

.base_input :deep(.p-inputnumber-input) {
  width: 100%;
}

.base_input :deep(.p-password-input) {
  width: 100%;
}

/* Mobile responsive adjustments */
@media (max-width: 768px) {
  .base_input :deep(.p-inputgroup-addon) {
    min-width: 2.5rem;
    padding: 0.5rem;
  }

  .base_input :deep(.p-inputgroup-addon i) {
    font-size: 1rem;
  }
}
</style>
