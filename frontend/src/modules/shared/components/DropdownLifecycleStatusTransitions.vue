<template>
  <div class="mb-2">
    <label v-if="label && label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      :options="store.availableTransitions"
      :placeholder="
        currentStatusDescription || placeholder || 'Selecciona estat...'
      "
      optionValue="statusToId"
      optionLabel="statusToName"
      class="w-full"
      v-bind="$attrs"
      :model-value="modelValue"
      @change="emit('update:modelValue', $event.value)"
      :loading="isLoading"
      :disabled="!statusId || isLoading"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useLifecyclesStore } from "../store/lifecycle";

const props = defineProps<{
  label?: string;
  modelValue: string | undefined;
  statusId: string;
  placeholder?: string;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
}>();

const store = useLifecyclesStore();
const isLoading = ref(false);

const currentStatusDescription = computed(() => {
  if (!props.statusId || !store.lifecycle?.statuses) return "";
  const status = store.lifecycle.statuses.find((s) => s.id === props.statusId);
  return status?.name || "";
});

onMounted(async () => {
  if (!props.statusId) return;

  isLoading.value = true;
  try {
    await store.fetchAvailableTransitions(props.statusId);
  } finally {
    isLoading.value = false;
  }
});
</script>
