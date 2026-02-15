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
      <div class="w-full flex align-items-center justify-content-between pr-4">
        <div class="flex align-items-center gap-3">
          <div
            class="flex align-items-center justify-content-center bg-red-100 border-circle p-2"
            style="width: 3rem; height: 3rem"
          >
            <i :class="PrimeIcons.STOP" class="text-red-500 text-xl"></i>
          </div>
          <div class="flex flex-column">
            <span class="font-bold text-lg text-900">Finalitzar Fase</span>
            <span class="text-sm text-500">{{
              loadedPhase?.phaseDescription
            }}</span>
          </div>
        </div>
        <div class="flex gap-4 flex-wrap">
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold">Ordre</span>
            <span class="font-medium text-900 text-lg">{{
              loadedWorkOrder?.workOrderCode
            }}</span>
          </div>
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold">Ref.</span>
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
              Quantitat produïda
            </h4>
            <div class="produced-unit-card ok">
              <span class="produced-value">{{
                loadedPhase?.quantityOk ?? 0
              }}</span>
            </div>
          </div>
          <div class="produced-column">
            <h4 class="section-title">
              <i :class="PrimeIcons.EXCLAMATION_TRIANGLE" class="mr-2"></i>
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
          Afegir més quantitat
        </h4>
        <p class="section-hint">
          Introdueix la quantitat addicional produïda en aquesta sessió
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

      <!-- Options Section -->
      <div v-if="nextAvailablePhase" class="options-section">
        <h4 class="section-title">
          <i :class="PrimeIcons.COG" class="mr-2"></i>
          Opcions
        </h4>
        <div class="options-list">
          <div class="option-item">
            <Checkbox
              v-model="formData.loadNextPhase"
              :binary="true"
              inputId="loadNextPhase"
            />
            <label for="loadNextPhase" class="option-label">
              <span class="option-title">Carregar fase següent</span>
              <span class="option-description">
                {{ nextAvailablePhase.phaseCode }} -
                {{ nextAvailablePhase.phaseDescription }}
              </span>
            </label>
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
          :disabled="isValidating"
          class="action-button"
        />
        <Button
          :icon="PrimeIcons.PAUSE"
          label="Pausar"
          severity="warning"
          :disabled="isValidating"
          :loading="isValidating && !closingPhase"
          @click="onUnload(false)"
          class="action-button"
        />
        <Button
          :icon="PrimeIcons.STOP"
          label="Finalitzar"
          severity="danger"
          :disabled="isValidating"
          :loading="isValidating && closingPhase"
          @click="onUnload(true)"
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
import { UnloadWorkOrderPhaseRequest } from "../../types";
import { usePlantWorkcenterStore } from "../../store";

interface Props {
  visible: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  (event: "update:visible", value: boolean): void;
  (event: "phase-unloaded", data: UnloadWorkOrderPhaseRequest): void;
}>();

const toast = useToast();
const workcenterStore = usePlantWorkcenterStore();

// Get loaded work order data from store
const loadedWorkOrder = computed(
  () => workcenterStore.loadedWorkOrdersPhases[0],
);
const loadedPhase = computed(() => loadedWorkOrder.value?.phases?.[0]);
const nextAvailablePhase = computed(() => workcenterStore.nextAvailablePhase);

// Validation state
const isValidating = ref(false);
const closingPhase = ref(false);

// Form state
interface FormData {
  workcenterId: string;
  workOrderPhaseId: string;
  counterOk: number;
  counterKo: number;
  loadNextPhase: boolean;
}

const formData = reactive<FormData>({
  workcenterId: "",
  workOrderPhaseId: "",
  counterOk: 0,
  counterKo: 0,
  loadNextPhase: false,
});

// Computed: Form validation (always valid if counters >= 0)
const isFormValid = computed(() => {
  if (formData.counterOk < 0 || formData.counterKo < 0) return false;
  return true;
});

// Reset form and fetch next phase when dialog opens
watch(
  () => props.visible,
  async (newValue) => {
    if (newValue) {
      resetForm();
      // Fetch next available phase for this workcenter type
      await workcenterStore.fetchNextPhaseForWorkcenter();
    }
  },
);

const resetForm = () => {
  formData.workcenterId = workcenterStore.workcenter?.id ?? "";
  formData.workOrderPhaseId = loadedPhase.value?.phaseId ?? "";
  formData.counterOk = 0;
  formData.counterKo = 0;
  formData.loadNextPhase = false;
};

const onCancel = () => {
  emit("update:visible", false);
};

const onUnload = async (closePhase: boolean) => {
  if (!isFormValid.value) {
    toast.add({
      severity: "warn",
      summary: "Formulari incomplet",
      detail: "Si us plau, omple tots els camps obligatoris",
      life: 4000,
    });
    return;
  }

  isValidating.value = true;
  closingPhase.value = closePhase;
  try {
    // Validate quantity against previous phase
    const validation = await workcenterStore.validatePhaseQuantity(
      formData.counterOk + formData.counterKo,
    );

    if (!validation.valid) {
      toast.add({
        severity: "warn",
        summary: "Validació de quantitat",
        detail: validation.error,
        life: 6000,
      });
      return;
    }

    // Resolve status ID based on clicked button
    const statusId = await workcenterStore.getPhaseExitStatusId(closePhase);

    if (!statusId) {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "No s'ha pogut determinar l'estat de sortida de la fase",
        life: 6000,
      });
      return;
    }

    // Build the request
    const request: UnloadWorkOrderPhaseRequest = {
      workcenterId: formData.workcenterId,
      workOrderPhaseId: formData.workOrderPhaseId,
      workOrderStatusId: statusId,
      quantityOk: formData.counterOk,
      quantityKo: formData.counterKo,
    };

    // Add next phase if selected
    if (formData.loadNextPhase && nextAvailablePhase.value) {
      request.nextWorkOrderPhaseId = nextAvailablePhase.value.phaseId;
    }

    emit("phase-unloaded", request);
  } finally {
    isValidating.value = false;
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

/* Options Section */
.options-section {
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
  border-radius: 8px;
  padding: 1rem;
}

.options-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.option-item {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.75rem;
  background: var(--p-surface-50);
  border-radius: 6px;
  transition: background-color 0.2s;
}

.option-item:hover {
  background: var(--p-surface-100);
}

.option-label {
  display: flex;
  flex-direction: column;
  cursor: pointer;
}

.option-title {
  font-weight: 600;
  color: var(--text-color);
  font-size: 0.95rem;
}

.option-description {
  font-size: 0.85rem;
  color: var(--text-color-secondary);
  margin-top: 0.25rem;
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

  .header-details {
    flex-direction: column;
    gap: 0.5rem;
  }

  .actions-panel {
    flex-direction: column;
  }

  .action-button {
    width: 100%;
  }
}
</style>
