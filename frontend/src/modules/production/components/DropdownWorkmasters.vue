<template>
  <div class="mb-2">
    <label v-if="label.length > 0" class="block text-900 mb-2">{{
      label
    }}</label>
    <Select
      showClear
      filter
      :options="options"
      optionValue="id"
      :optionLabel="formatWorkMasterLabel"
      :loading="isLoading"
      :disabled="isLoading"
      class="w-full"
      v-bind="$attrs"
      v-bind:model-value="modelValue as string"
      @change="emit('update:modelValue', $event.value)"
    >
      <template #option="slotProps">
        <div v-if="slotProps.option" class="flex align-items-center">
          {{ formatWorkMasterLabel(slotProps.option) }}
        </div>
      </template>
    </Select>
  </div>
</template>
<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useReferenceStore } from "../../shared/store/reference";
import { useWorkMasterStore } from "../store/workmaster";
import { WorkMaster, WorkmastersOptionsLoadedPayload } from "../types";

const referenceStore = useReferenceStore();
const workmasterStore = useWorkMasterStore();

const props = defineProps<{
  label: string;
  modelValue: string | null | undefined;
  referenceId?: string;
  /** When true, the component self-fetches active workmasters for the given referenceId */
  activeByReference?: boolean;
}>();

const emit = defineEmits<{
  (event: "update:modelValue", payload: string): void;
  /** Emitted after a successful activeByReference fetch.
   *  Includes referenceId so consumers can discard stale events. */
  (event: "optionsLoaded", payload: WorkmastersOptionsLoadedPayload): void;
}>();

// Local state for the activeByReference mode — no Pinia state involved
const activeWorkmasters = ref<WorkMaster[]>([]);
const isLoadingActive = ref(false);

// Stale-response guard: each new fetch gets a unique id.
// Only the response whose id still matches the latest one is applied.
let latestRequestId = 0;

watch(
  () => [props.referenceId, props.activeByReference] as const,
  async ([newRef, activeByReference]) => {
    // Invalidate any in-flight request by advancing the counter.
    const requestId = ++latestRequestId;

    if (!activeByReference || !newRef) {
      activeWorkmasters.value = [];
      isLoadingActive.value = false;
      return;
    }

    activeWorkmasters.value = [];
    isLoadingActive.value = true;

    try {
      const result =
        await workmasterStore.fetchActiveWorkmastersByReference(newRef);

      // Discard the response if a newer request has already been started.
      if (requestId !== latestRequestId) return;

      activeWorkmasters.value = result;
      emit("optionsLoaded", { referenceId: newRef, options: result });
    } finally {
      // Only clear the loading flag if this is still the active request.
      if (requestId === latestRequestId) {
        isLoadingActive.value = false;
      }
    }
  },
  { immediate: true },
);

const options = computed(() => {
  if (props.activeByReference) {
    return activeWorkmasters.value;
  }
  if (props.referenceId) {
    return workmasterStore.getByReferenceId(props.referenceId);
  }
  return workmasterStore.workmasters;
});

const isLoading = computed(() => {
  if (props.activeByReference) {
    return isLoadingActive.value;
  }
  return false;
});

const formatWorkMasterLabel = (workMaster: WorkMaster) => {
  const referenceName = referenceStore.getShortNameById(workMaster.referenceId);
  let modeName = workmasterStore.workmasterModes.find(
    (mode) => mode.id === workMaster.mode,
  )?.value;

  return `${referenceName}  (Base = ${workMaster.baseQuantity} )  ${modeName}`;
};
</script>
