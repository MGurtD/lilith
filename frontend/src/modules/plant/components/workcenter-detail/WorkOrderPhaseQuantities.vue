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
      <!-- Produced Quantity Section (Informative) -->
      <div class="info-section">
        <div class="produced-units-row">
          <div class="produced-column">
            <h4 class="section-title">
              <i :class="PrimeIcons.CHECK_CIRCLE" class="mr-2"></i>
              Quantitat produ&iuml;da
            </h4>
            <div class="produced-unit-card ok">
              <span class="produced-value">{{
                loadedPhase?.quantityOk ?? 0
              }}</span>
            </div>
          </div>
          <div class="produced-column">
            <h4 class="section-title">
              <i
                :class="PrimeIcons.EXCLAMATION_TRIANGLE"
                class="mr-2"
              ></i>
              Quantitat defectuosa
            </h4>
            <div class="produced-unit-card ko">
              <span class="produced-value">{{
                loadedPhase?.quantityKo ?? 0
              }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Add More Quantity Section (Input) -->
      <div class="input-section">
        <h4 class="section-title">
          <i :class="PrimeIcons.PLUS_CIRCLE" class="mr-2"></i>
          Afegir m&eacute;s quantitat
        </h4>
        <p class="section-hint">
          Introdueix la quantitat addicional produ&iuml;da en aquesta
          sessi&oacute;
        </p>
        <div class="counters-row">
          <div class="counter-field">
            <InputNumber
              v-model="formData.counterOk"
              :min="0"
              :useGrouping="false"
              class="w-full"
              showButtons
              buttonLayout="horizontal"
              :step="1"
              decrementButtonClass="p-button-secondary"
              incrementButtonClass="p-button-secondary"
              incrementButtonIcon="pi pi-plus"
              decrementButtonIcon="pi pi-minus"
            />
          </div>
          <div class="counter-field">
            <InputNumber
              v-model="formData.counterKo"
              :min="0"
              :useGrouping="false"
              class="w-full"
              showButtons
              buttonLayout="horizontal"
              :step="1"
              decrementButtonClass="p-button-secondary"
              incrementButtonClass="p-button-secondary"
              incrementButtonIcon="pi pi-plus"
              decrementButtonIcon="pi pi-minus"
            />
          </div>
        </div>
      </div>

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
import { usePlantWorkcenterStore } from "../../store";

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
      await workcenterStore.validatePhaseQuantity(totalQuantity);

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
    const result = await workcenterStore.updatePhaseQuantities(
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

.section-title {
  margin: 0 0 0.75rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text-color);
  display: flex;
  align-items: center;
}

.section-hint {
  margin: 0 0 1rem 0;
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

/* Produced Units Section */
.info-section {
  background: var(--p-surface-50);
  border-radius: 8px;
  padding: 1rem;
}

.produced-units-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.produced-column {
  display: flex;
  flex-direction: column;
}

.produced-unit-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  border-radius: 8px;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
}

.produced-unit-card.ok {
  border-left: 4px solid var(--p-green-500);
}

.produced-unit-card.ko {
  border-left: 4px solid var(--p-red-500);
}

.produced-value {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--text-color);
}

/* Input Section */
.input-section {
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
  border-radius: 8px;
  padding: 1rem;
}

.counters-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.counter-field {
  display: flex;
  flex-direction: column;
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
  .produced-units-row,
  .counters-row {
    grid-template-columns: 1fr;
  }

  .actions-panel {
    flex-direction: column;
  }

  .action-button {
    width: 100%;
  }
}
</style>
