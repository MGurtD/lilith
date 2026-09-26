<template>
  <Dialog
    :visible="visible"
    modal
    :closable="true"
    class="declare-dialog"
    :style="{ width: '46rem' }"
    :breakpoints="{ '767px': '96vw' }"
    @update:visible="$emit('update:visible', $event)"
  >
    <template #header>
      <div class="declare-dialog__header">
        <span class="declare-dialog__title">{{ t("plant.declare.title") }}</span>
        <span v-if="loadedWorkOrder" class="declare-dialog__context">{{
          t("plant.declare.context", {
            order: loadedWorkOrder.workOrderCode,
            phase: loadedPhase?.phaseCode ?? "",
            done: loadedPhase?.quantityOk ?? 0,
            planned: loadedWorkOrder.plannedQuantity,
          })
        }}</span>
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

      <PhaseRejectionReasons
        ref="rejectionReasons"
        v-model="formData.rejections"
        :counter-ko="formData.counterKo"
      />

      <div class="actions-panel">
        <Button
          :label="t('plant.declare.cancel')"
          severity="secondary"
          outlined
          :disabled="isSubmitting"
          class="action-button action-button--cancel"
          @click="onCancel"
        />
        <Button
          icon="pi pi-check"
          :label="submitLabel"
          :disabled="isSubmitting || !hasQuantity"
          :loading="isSubmitting"
          class="action-button"
          @click="onSubmit"
        />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { watch, computed, reactive, ref } from "vue";
import { useToast } from "primevue/usetoast";
import { usePlantWorkcenterStore, usePlantActivePhaseStore } from "../../store";
import PhaseQuantityForm from "./PhaseQuantityForm.vue";
import PhaseRejectionReasons from "./PhaseRejectionReasons.vue";
import { WorkOrderPhaseRejectionRequest } from "../../../production/types";

const { t } = useI18n();

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
  rejections: Array<WorkOrderPhaseRejectionRequest>;
}

const formData = reactive<FormData>({
  counterOk: 0,
  counterKo: 0,
  rejections: [],
});

const rejectionReasons = ref<InstanceType<typeof PhaseRejectionReasons>>();

// At least one quantity must be > 0 to enable the submit button
const hasQuantity = computed(() => {
  return formData.counterOk > 0 || formData.counterKo > 0;
});

// The button says what it will declare: "Declarar 5 bones i 1 dolenta".
const submitLabel = computed(() => {
  const parts = [
    formData.counterOk > 0
      ? t("plant.declare.goodCount", { count: formData.counterOk }, formData.counterOk)
      : "",
    formData.counterKo > 0
      ? t("plant.declare.badCount", { count: formData.counterKo }, formData.counterKo)
      : "",
  ].filter(Boolean);
  if (parts.length === 0) return t("plant.declare.submitEmpty");
  const joined =
    parts.length === 2
      ? t("plant.declare.joiner", { first: parts[0], second: parts[1] })
      : parts[0];
  return t("plant.declare.submit", { parts: joined });
});

// Reset form when dialog opens
watch(
  () => props.visible,
  (newValue) => {
    if (newValue) {
      formData.counterOk = 0;
      formData.counterKo = 0;
      formData.rejections = [];
    }
  },
);

const onCancel = () => {
  emit("update:visible", false);
};

// Reasons are optional, but a partial breakdown would misreport the KO units
const isRejectionBreakdownValid = () => {
  if (formData.rejections.length === 0) return true;

  if (formData.rejections.some((r) => !r.rejectionReasonId)) {
    toast.add({
      severity: "warn",
      summary: t("plant.rejections.title"),
      detail: t("plant.rejections.reasonRequired"),
      life: 6000,
    });
    return false;
  }

  if (!rejectionReasons.value?.isBalanced) {
    toast.add({
      severity: "warn",
      summary: t("plant.rejections.title"),
      detail: t("plant.rejections.quantityMismatch"),
      life: 6000,
    });
    return false;
  }

  return true;
};

const onSubmit = async () => {
  if (!hasQuantity.value) return;

  // The rejection breakdown must cover every declared KO unit
  if (!isRejectionBreakdownValid()) return;

  isSubmitting.value = true;
  try {
    // Validate quantity against previous phase
    const totalQuantity = formData.counterOk + formData.counterKo;
    const validation =
      await activePhaseStore.validatePhaseQuantity(totalQuantity);

    if (!validation.valid) {
      toast.add({
        severity: "warn",
        summary: t("plant.validaci-u00f3-de-quantitat"),
        detail: validation.error,
        life: 6000,
      });
      return;
    }

    // Call the store action to update quantities
    const result = await activePhaseStore.updatePhaseQuantities(
      formData.counterOk,
      formData.counterKo,
      formData.rejections,
    );

    if (result) {
      toast.add({
        severity: "success",
        summary: t("plant.quantitat-afegida-correctament"),
        life: 4000,
      });
      emit("quantities-updated");
      emit("update:visible", false);
    } else {
      toast.add({
        severity: "error",
        summary: t("plant.error-al-afegir-la-quantitat"),
        life: 4000,
      });
    }
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
.declare-dialog__header {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.declare-dialog__title {
  font-family: var(--font-condensed);
  font-size: 1.625rem;
  line-height: 2rem;
  font-weight: 600;
  color: var(--p-steel-900);
}

.declare-dialog__context {
  font-size: 0.9375rem;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-700);
}

.dialog-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--p-steel-200);
}

.actions-panel {
  display: flex;
  gap: 0.625rem;
  justify-content: flex-end;
  padding-top: 1rem;
  border-top: 1px solid var(--p-steel-200);
}

.action-button {
  min-height: 56px;
  padding-inline: 1.25rem;
  font-size: 1.0625rem;
}

.action-button--cancel {
  color: var(--p-steel-900);
  border-color: var(--p-steel-300);
}

@media (max-width: 767.98px) {
  .actions-panel {
    flex-direction: column-reverse;
  }

  .action-button {
    width: 100%;
  }
}
</style>
