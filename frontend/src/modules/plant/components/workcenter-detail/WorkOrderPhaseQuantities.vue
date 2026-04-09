<template>
  <Dialog
    :visible="visible"
    modal
    :closable="true"
    :style="{ width: '50vw' }"
    :breakpoints="{ '1024px': '80vw' }"
    @update:visible="$emit('update:visible', $event)"
  >
    <template #header>
      <div
        class="w-full flex align-items-center justify-content-between pr-4"
      >
        <div class="flex align-items-center gap-3">
          <div
            class="flex align-items-center justify-content-center bg-blue-100 border-circle p-2"
            style="width: 3rem; height: 3rem"
          >
            <i
              :class="PrimeIcons.PLUS_CIRCLE"
              class="text-blue-500 text-xl"
            ></i>
          </div>
          <div class="flex flex-column">
            <span class="font-bold text-lg text-900"
              >Afegir quantitat</span
            >
            <span class="text-sm text-500">{{
              loadedPhase?.phaseDescription
            }}</span>
          </div>
        </div>
        <div class="flex gap-4 flex-wrap">
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold"
              >Ordre</span
            >
            <span class="font-medium text-900 text-lg">{{
              loadedWorkOrder?.workOrderCode
            }}</span>
          </div>
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold"
              >Ref.</span
            >
            <span class="font-medium text-900 text-lg">{{
              loadedWorkOrder?.salesReferenceDisplay
            }}</span>
          </div>
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold"
              >Quantitat</span
            >
            <span class="font-medium text-900 text-lg">{{
              loadedWorkOrder?.plannedQuantity
            }}</span>
          </div>
        </div>
      </div>
    </template>

    <div class="dialog-content">
      <PhaseQuantityForm
        :quantity-ok="loadedPhase?.quantityOk ?? 0"
        :quantity-ko="loadedPhase?.quantityKo ?? 0"
        :counter-ok="formData.counterOk"
        :counter-ko="formData.counterKo"
        @update:counter-ok="formData.counterOk = $event"
        @update:counter-ko="formData.counterKo = $event"
      />

      <!-- Action Buttons -->
      <div class="actions-panel">
        <Button
          :icon="PrimeIcons.TIMES"
          label="Cancel·lar"
          severity="secondary"
          @click="onCancel"
          :disabled="isSubmitting"
          class="action-button"
        />
        <Button
          :icon="PrimeIcons.CHECK"
          label="Afegir"
          severity="primary"
          :disabled="isSubmitting || !hasQuantity"
          :loading="isSubmitting"
          @click="onSubmit"
          class="action-button"
        />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { watch, computed, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { usePlantWorkcenterStore, usePlantActivePhaseStore } from "../../store";
import PhaseQuantityForm from "./PhaseQuantityForm.vue";

interface Props {
  visible: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  (event: "update:visible", value: boolean): void;
  (event: "quantities-updated"): void;
}>();

const toast = useToast();
const workcenterStore = usePlantWorkcenterStore();
const activePhaseStore = usePlantActivePhaseStore();

// Get loaded work order data from store
const loadedWorkOrder = computed(
  () => workcenterStore.loadedWorkOrdersPhases[0],
);
const loadedPhase = computed(() => loadedWorkOrder.value?.phases?.[0]);

// Submission state
const isSubmitting = ref(false);

// Form state
interface FormData {
  counterOk: number;
  counterKo: number;
}

const formData = reactive<FormData>({
  counterOk: 0,
  counterKo: 0,
});

// At least one quantity must be > 0 to enable the submit button
const hasQuantity = computed(() => {
  return formData.counterOk > 0 || formData.counterKo > 0;
});

// Reset form when dialog opens
watch(
  () => props.visible,
  (newValue) => {
    if (newValue) {
      formData.counterOk = 0;
      formData.counterKo = 0;
    }
  },
);

const onCancel = () => {
  emit("update:visible", false);
};

const onSubmit = async () => {
  if (!hasQuantity.value) return;

  isSubmitting.value = true;
  try {
    // Validate quantity against previous phase
    const totalQuantity = formData.counterOk + formData.counterKo;
    const validation =
      await activePhaseStore.validatePhaseQuantity(totalQuantity);

    if (!validation.valid) {
      toast.add({
        severity: "warn",
        summary: "Validaci\u00f3 de quantitat",
        detail: validation.error,
        life: 6000,
      });
      return;
    }

    // Call the store action to update quantities
    const result = await activePhaseStore.updatePhaseQuantities(
      formData.counterOk,
      formData.counterKo,
    );

    if (result) {
      toast.add({
        severity: "success",
        summary: "Quantitat afegida correctament",
        life: 4000,
      });
      emit("quantities-updated");
      emit("update:visible", false);
    } else {
      toast.add({
        severity: "error",
        summary: "Error al afegir la quantitat",
        life: 4000,
      });
    }
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
.dialog-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  border-top: 1px solid var(--p-surface-border);
}

/* Actions Panel */
.actions-panel {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.action-button {
  min-width: 150px;
}

@media (max-width: 768px) {
  .actions-panel {
    flex-direction: column;
  }

  .action-button {
    width: 100%;
  }
}
</style>
