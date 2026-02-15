<template>
  <div class="mb-2">
    <label v-if="label && label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      filter
      :options="groupedWarehouseLocations"
      :placeholder="placeholder || 'Selecciona una ubicació'"
      optionLabel="label"
      optionValue="value"
      optionGroupLabel="label"
      optionGroupChildren="items"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #value="slotProps">
        <div v-if="slotProps.value" class="flex align-items-center">
          <span>{{ getSelectedLocationDisplay(slotProps.value) }}</span>
        </div>
        <span v-else>{{ placeholder || "Selecciona una ubicació" }}</span>
      </template>
      <template #optiongroup="slotProps">
        <div class="flex align-items-center">
          <i class="pi pi-warehouse mr-2" style="color: #6366f1"></i>
          <div class="font-semibold">{{ slotProps.option.label }}</div>
        </div>
      </template>
      <template #option="slotProps">
        <div v-if="slotProps.option" class="flex align-items-center pl-4">
          <i class="pi pi-map-marker mr-2" style="color: #64748b"></i>
          <div>{{ slotProps.option.label }}</div>
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { computed, onMounted } from "vue";
import { useWarehouseStore } from "../store/warehouse";

const warehouseStore = useWarehouseStore();

onMounted(() => {
  if (!warehouseStore.warehousesHaveLocations)
    warehouseStore.fetchWarehousesWithLocations();
});

const props = defineProps<{
  label?: string;
  modelValue: string | null | undefined;
  placeholder?: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();

const groupedWarehouseLocations = computed(() => {
  if (!warehouseStore.warehouses) return [];

  return warehouseStore.warehouses
    .filter((warehouse) => !warehouse.disabled)
    .map((warehouse) => ({
      label: warehouse.name,
      code: warehouse.id,
      items:
        warehouse.locations
          ?.filter((location) => !location.disabled)
          ?.map((location) => ({
            label: location.description,
            value: location.id,
            warehouseId: warehouse.id,
            warehouseName: warehouse.name,
          })) || [],
    }))
    .filter((group) => group.items.length > 0); // Only show warehouses that have locations
});

const getSelectedLocationDisplay = (locationId: string) => {
  if (!warehouseStore.warehouses || !locationId) return "";

  for (const warehouse of warehouseStore.warehouses) {
    if (warehouse.locations) {
      const location = warehouse.locations.find((loc) => loc.id === locationId);
      if (location) {
        return `${warehouse.name} - ${location.description}`;
      }
    }
  }
  return locationId;
};
</script>
