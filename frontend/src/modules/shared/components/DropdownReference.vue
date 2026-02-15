<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      :virtualScrollerOptions="{ itemSize: 38 }"
      showClear
      filter
      :filter-fields="['code', 'description']"
      :options="filteredReferences"
      placeholder="Selecciona..."
      optionValue="id"
      :optionLabel="
        (r) =>
          fullName
            ? referenceStore.getFullNameById(r.id)
            : referenceStore.getShortNameById(r.id)
      "
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #value="slotProps">
        <div v-if="slotProps.value" class="flex align-items-center">
          {{ getReferenceNameById(slotProps.value) }}
        </div>
        <span v-else>
          {{ slotProps.placeholder }}
        </span>
      </template>
      <template #option="slotProps">
        <div v-if="slotProps.option" class="flex align-items-center">
          {{ getReferenceNameById(slotProps.option.id) }}
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { computed } from "vue";
import { useReferenceStore } from "../store/reference";

const props = defineProps<{
  label: string;
  modelValue: string | null | undefined;
  fullName: boolean;
  customerId?: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();

const referenceStore = useReferenceStore();

const getReferenceNameById = (id: string) => {
  return props.fullName
    ? referenceStore.getFullNameById(id)
    : referenceStore.getShortNameById(id);
};

const filteredReferences = computed(() => {
  if (!referenceStore.references) return [];
  if (!props.customerId) return referenceStore.references;

  return referenceStore.references.filter((r) => {
    return (
      (props.customerId && r.customerId === props.customerId) ||
      r.customerId === null
    );
  });
});
</script>
