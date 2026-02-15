<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      filter
      :options="customersStore.customers"
      :placeholder="placeholder || 'Selecciona un client'"
      optionValue="id"
      optionLabel="comercialName"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #option="slotProps">
        <div v-if="slotProps.option" class="flex align-items-center">
          {{ slotProps.option.comercialName }}
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { onMounted } from "vue";
import { useCustomersStore } from "../store/customers";

const customerStore = useCustomersStore();

onMounted(() => {
  if (!customerStore.customers) customerStore.fetchCustomers();
});

const props = defineProps<{
  label: string;
  modelValue: string | null | undefined;
  placeholder?: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();

const customersStore = useCustomersStore();
</script>
