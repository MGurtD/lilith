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
      <PhaseQuantityForm
        :quantity-ok="loadedPhase?.quantityOk ?? 0"
        :quantity-ko="loadedPhase?.quantityKo ?? 0"
        :counter-ok="formData.counterOk"
        :counter-ko="formData.counterKo"
        @update:counter-ok="formData.counterOk = $event"
        @update:counter-ko="formData.counterKo = $event"
      />

      <!-- Options Section -->
      <div
        v-if="props.showNextPhaseOption !== false && nextAvailablePhase"
        class="options-section"
      >
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
            <div class="option-content">
              <label for="loadNextPhase" class="option-label">
                <span class="option-title">
                  Carregar fase {{ nextAvailablePhase.phaseCode }} -
                  {{ nextAvailablePhase.phaseDescription }}
                </span>
              </label>
              <SelectWorkOrderPhaseDetail
                v-if="formData.loadNextPhase && nextPhaseDetails.length > 0"
                v-model="formData.selectedNextMachineStatusId"
                :details="nextPhaseDetails"
                class="mt-2"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Material Consumption Dialog -->
      <MaterialConsumptionDialog
        v-model:visible="showConsumptionDialog"
        :bill-of-materials="activePhaseStore.billOfMaterials"
        :workcenter-id="formData.workcenterId"
        :work-order-phase-id="formData.workOrderPhaseId"
        @confirm="onConsumptionConfirmed"
      />

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
import { usePlantWorkcenterStore, usePlantActivePhaseStore } from "../../store";
import PhaseQuantityForm from "./PhaseQuantityForm.vue";
import SelectWorkOrderPhaseDetail from "./SelectWorkOrderPhaseDetail.vue";
import MaterialConsumptionDialog from "./MaterialConsumptionDialog.vue";
import type { ConsumeStockItem } from "../../../warehouse/types";
import ProductionServices from "../../../production/services";

interface Props {
  visible: boolean;
  nextMachineStatusId?: string;
  showNextPhaseOption?: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  (event: "update:visible", value: boolean): void;
  (event: "phase-unloaded", data: UnloadWorkOrderPhaseRequest): void;
}>();

const toast = useToast();
const workcenterStore = usePlantWorkcenterStore();
const activePhaseStore = usePlantActivePhaseStore();

// Get loaded work order data from store
const loadedWorkOrder = computed(
  () => workcenterStore.loadedWorkOrdersPhases[0],
);
const loadedPhase = computed(() => loadedWorkOrder.value?.phases?.[0]);
const nextAvailablePhase = computed(() => activePhaseStore.nextAvailablePhase);
const nextPhaseDetails = computed(
  () => nextAvailablePhase.value?.details ?? [],
);

// Validation state
const isValidating = ref(false);
const closingPhase = ref(false);

// Consumption dialog state
const showConsumptionDialog = ref(false);
const pendingUnloadRequest = ref<UnloadWorkOrderPhaseRequest | null>(null);

// Form state
interface FormData {
  workcenterId: string;
  workOrderPhaseId: string;
  counterOk: number;
  counterKo: number;
  loadNextPhase: boolean;
  selectedNextMachineStatusId: string;
}

const formData = reactive<FormData>({
  workcenterId: "",
  workOrderPhaseId: "",
  counterOk: 0,
  counterKo: 0,
  loadNextPhase: false,
  selectedNextMachineStatusId: "",
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
      await activePhaseStore.fetchNextPhaseForWorkcenter();
    }
  },
);

const resetForm = () => {
  formData.workcenterId = workcenterStore.workcenter?.id ?? "";
  formData.workOrderPhaseId = loadedPhase.value?.phaseId ?? "";
  formData.counterOk = 0;
  formData.counterKo = 0;
  formData.loadNextPhase = false;
  formData.selectedNextMachineStatusId = "";
};

const onCancel = () => {
  emit("update:visible", false);
};

const onConsumptionConfirmed = async (consumedItems: ConsumeStockItem[]) => {
  if (!pendingUnloadRequest.value) return;

  try {
    // Call the consumption API
    const success =
      await ProductionServices.WorkOrderStock.consumePhaseStock({
        workcenterId: formData.workcenterId,
        workOrderPhaseId: formData.workOrderPhaseId,
        consumedItems,
      });

    if (!success) {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "No s'ha pogut registrar el consum de materials",
        life: 6000,
      });
      return;
    }

    // Close the consumption dialog
    showConsumptionDialog.value = false;

    // Proceed with the normal unload flow
    emit("phase-unloaded", pendingUnloadRequest.value);
    pendingUnloadRequest.value = null;
  } catch (error) {
    console.error("Error consuming phase stock:", error);
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Error de connexió al registrar el consum",
      life: 6000,
    });
  }
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
    const validation = await activePhaseStore.validatePhaseQuantity(
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
    const statusId = await activePhaseStore.getPhaseExitStatusId(closePhase);

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
      // Validate activity selection when next phase has details
      if (
        nextPhaseDetails.value.length > 0 &&
        !formData.selectedNextMachineStatusId
      ) {
        toast.add({
          severity: "warn",
          summary: "Activitat requerida",
          detail: "Selecciona una activitat per a la fase següent",
          life: 4000,
        });
        return;
      }
      request.nextWorkOrderPhaseId = nextAvailablePhase.value.phaseId;
      // Use selected activity as the machine status for the next phase
      if (formData.selectedNextMachineStatusId) {
        request.nextMachineStatusId = formData.selectedNextMachineStatusId;
      }
    }

    // Include next machine status if provided and not already set by activity selection
    if (props.nextMachineStatusId && !request.nextMachineStatusId) {
      request.nextMachineStatusId = props.nextMachineStatusId;
    }

    // If closing phase and there are BOM materials, check provisioning and show consumption dialog
    // Skip if materials have already been consumed (e.g. re-opening a finalized phase)
    if (
      closePhase &&
      activePhaseStore.hasBillOfMaterials &&
      !activePhaseStore.hasMaterialsConsumed
    ) {
      // Ensure provisioning status is loaded
      await activePhaseStore.ensureMaterialsProvisioningLoaded(true);

      // Check all materials are provisioned
      const allProvisioned = Object.values(
        activePhaseStore.bomProvisioningById,
      ).every((v) => v === true);

      if (!allProvisioned) {
        toast.add({
          severity: "error",
          summary: "Materials no aprovisionats",
          detail:
            "Tots els materials han d'estar aprovisionats abans de finalitzar la fase",
          life: 6000,
        });
        return;
      }

      // Store the pending request and show consumption dialog
      pendingUnloadRequest.value = request;
      showConsumptionDialog.value = true;
      return;
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
}

.option-content {
  flex: 1;
  min-width: 0;
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
