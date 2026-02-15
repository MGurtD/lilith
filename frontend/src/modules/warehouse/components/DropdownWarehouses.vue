<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      filter
      :options="warehousesStore.warehouses"
      :placeholder="placeholder || 'Selecciona un magatzem'"
      optionValue="id"
      optionLabel="name"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #option="slotProps">
        <div v-if="slotProps.option" class="flex align-items-center">
          {{ slotProps.option.name }}
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { onMounted } from "vue";
import { useWarehouseStore } from "../store/warehouse";

const warehouseStore = useWarehouseStore();

onMounted(() => {
  if (!warehouseStore.warehouses) warehouseStore.fetchWarehouses();
});

const props = defineProps<{
  label: string;
  modelValue: string | null | undefined;
  placeholder?: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();

const warehousesStore = useWarehouseStore();
</script>
