<template>
  <form v-if="workmaster">
    <div class="grid_add_row_button">
      <Button :label="t('production.components.calcularCost')" size="small" @click="calculateCost" />
      &nbsp;
      <Button :label="t('production.components.guardar')" size="small" @click="submitForm" />
      <br />
    </div>
    <section class="six-columns">
      <div>
        <DropdownReference
          :label="t('production.components.referencia')"
          v-model="workmaster.referenceId"
          :fullName="true"
        ></DropdownReference>
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('production.components.quantitatBase')"
          :decimals="2"
          v-model="workmaster.baseQuantity"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('production.components.volumMm3')"
          :decimals="2"
          v-model="workmaster.volume"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.mode") }}</label>
        <Select
          v-model="workmaster.mode"
          :options="workmasterStore.workmasterModes"
          optionLabel="value"
          optionValue="id"
          :placeholder="t('production.components.seleccioneElModo')"
          class="w-full"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.desactivat") }}</label>
        <Checkbox v-model="workmaster.disabled" class="w-full" :binary="true" />
      </div>
    </section>
    <section class="costs-container">
      <div class="costs-section">
        <h4 class="costs-section-title">
          <i class="pi pi-euro" />
          {{ t("production.components.costos") }}
        </h4>
        <div class="costs-grid">
          <div class="cost-card">
            <div class="cost-card-icon">
              <i class="pi pi-user" />
            </div>
            <div class="cost-card-content">
              <span class="cost-card-label">{{ t("production.components.costOperari") }}</span>
              <span class="cost-card-value">{{
                formatCurrency(workmaster.operatorCost)
              }}</span>
            </div>
          </div>
          <div class="cost-card">
            <div class="cost-card-icon">
              <i class="pi pi-cog" />
            </div>
            <div class="cost-card-content">
              <span class="cost-card-label">{{ t("production.components.costMaquina") }}</span>
              <span class="cost-card-value">{{
                formatCurrency(workmaster.machineCost)
              }}</span>
            </div>
          </div>
          <div class="cost-card">
            <div class="cost-card-icon">
              <i class="pi pi-box" />
            </div>
            <div class="cost-card-content">
              <span class="cost-card-label">{{ t("production.components.costMaterial") }}</span>
              <span class="cost-card-value">{{
                formatCurrency(workmaster.materialCost)
              }}</span>
            </div>
          </div>
          <div class="cost-card">
            <div class="cost-card-icon">
              <i class="pi pi-truck" />
            </div>
            <div class="cost-card-content">
              <span class="cost-card-label">{{ t("production.components.costExtern") }}</span>
              <span class="cost-card-value">{{
                formatCurrency(workmaster.externalCost)
              }}</span>
            </div>
          </div>
          <div class="cost-card cost-card-total">
            <div class="cost-card-icon">
              <i class="pi pi-calculator" />
            </div>
            <div class="cost-card-content">
              <span class="cost-card-label">{{ t("production.components.costTotal") }}</span>
              <span class="cost-card-value">{{
                formatCurrency(totalCost)
              }}</span>
            </div>
          </div>
          <div class="cost-card">
            <div class="cost-card-icon">
              <i class="pi pi-objects-column" />
            </div>
            <div class="cost-card-content">
              <span class="cost-card-label">{{ t("production.components.pesTotal") }}</span>
              <span class="cost-card-value"
                >{{ workmaster.totalWeight }} KG</span
              >
            </div>
          </div>
        </div>
      </div>
    </section>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { ref, computed } from "vue";
import { WorkMaster } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { formatCurrency } from "../../../utils/functions";
import { useToast } from "primevue/usetoast";
import { useWorkMasterStore } from "../store/workmaster";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";

const props = defineProps<{
  workmaster: WorkMaster;
}>();

const emit = defineEmits<{
  (e: "submit", workmaster: WorkMaster): void;
  (e: "calculateCost", workmaster: WorkMaster): void;
  (e: "cancel"): void;
}>();

const workmasterStore = useWorkMasterStore();
const toast = useToast();

const totalCost = computed(() => {
  return (
    props.workmaster.operatorCost +
    props.workmaster.machineCost +
    props.workmaster.materialCost +
    props.workmaster.externalCost
  );
});

const schema = Yup.object().shape({
  baseQuantity: Yup.number()
    .min(1, t("production.validation.laQuantitatBaseHaDeSerSuperiorA0"))
    .required(t("production.validation.laQuanitatBaseEsObligatoria")),
  referenceId: Yup.string().required(t("production.validation.laReferenciaEsObligatoria")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.workmaster);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.workmaster);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("production.components.formulariInvalid"),
      detail: errors,
      life: 5000,
    });
  }
};

const calculateCost = () => {
  validate();
  if (validation.value.result) {
    emit("calculateCost", props.workmaster);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("production.components.formulariInvalid"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>

<style scoped>
.costs-container {
  margin-top: 0.5rem;
}

.costs-section-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0 0 0.75rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--p-text-color);
}

.costs-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 0.75rem;
}

.cost-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.85rem 1rem;
  background: var(--p-content-background, #fff);
  transition: box-shadow 0.15s ease;
}

.cost-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.cost-card-total {
  background: var(--p-primary-50, #eef2ff);
  border-color: var(--p-primary-200, #c7d2fe);
}

.cost-card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 8px;
  background: var(--p-surface-100, #f1f5f9);
  color: var(--p-primary-color, #3b82f6);
  font-size: 1rem;
  flex-shrink: 0;
}

.cost-card-total .cost-card-icon {
  background: var(--p-primary-100, #dbeafe);
  color: var(--p-primary-700, #1d4ed8);
}

.cost-card-content {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  min-width: 0;
}

.cost-card-label {
  font-size: 0.8rem;
  color: var(--p-text-muted-color);
  white-space: nowrap;
}

.cost-card-value {
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.cost-card-total .cost-card-value {
  color: var(--p-primary-700, #1d4ed8);
}

@media (max-width: 1200px) {
  .costs-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 640px) {
  .costs-grid {
    grid-template-columns: 1fr;
  }
}
</style>
