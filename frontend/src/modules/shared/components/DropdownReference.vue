<template>
  <div>
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
      :optionLabel="(r) => getReferenceNameById(r.id)"
      class="w-full"
      overlayClass="dropdown-reference-overlay"
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

interface ReferenceOption {
  id: string;
  code: string;
  description: string;
  customerId?: string | null;
}

const props = defineProps<{
  label: string;
  modelValue: string | null | undefined;
  fullName: boolean;
  customerId?: string;
  options?: ReferenceOption[];
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();

const referenceStore = useReferenceStore();

const getReferenceNameById = (id: string) => {
  const externalReference = props.options?.find((reference) => reference.id === id);
  if (externalReference) {
    return props.fullName
      ? `${externalReference.code} - ${externalReference.description}`
      : externalReference.code;
  }

  return props.fullName
    ? referenceStore.getFullNameById(id)
    : referenceStore.getShortNameById(id);
};

const filteredReferences = computed(() => {
  if (props.options) {
    if (!props.customerId) return props.options;

    return props.options.filter((reference) => {
      return (
        (props.customerId && reference.customerId === props.customerId) ||
        reference.customerId === null ||
        reference.customerId === undefined
      );
    });
  }

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

<style>
.dropdown-reference-overlay {
  min-width: 400px !important;
  width: auto !important;
}
</style>
