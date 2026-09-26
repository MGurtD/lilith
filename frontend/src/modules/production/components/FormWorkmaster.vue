<template>
  <div>
    <Form
      ref="form"
      page-actions
      :rows="rows"
      :initial-values="initialValues"
      @submit="submit"
    >
      <template #field-referenceId="{ value, setValue, disabled, inputId }">
        <DropdownReference
          :input-id="inputId"
          label=""
          :model-value="typeof value === 'string' ? value : null"
          :full-name="true"
          :disabled="disabled"
          @update:model-value="setValue"
        />
      </template>

      <template #actions="{ submit: submitForm, disabled }">
        <SplitButton
          icon="pi pi-save"
          :label="t('production.components.guardar')"
          :model="workmasterActions"
          :disabled="disabled"
          @click="save(submitForm)"
        />
      </template>
    </Form>
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
              <span class="cost-card-label">{{
                t("production.components.costOperari")
              }}</span>
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
              <span class="cost-card-label">{{
                t("production.components.costMaquina")
              }}</span>
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
              <span class="cost-card-label">{{
                t("production.components.costMaterial")
              }}</span>
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
              <span class="cost-card-label">{{
                t("production.components.costExtern")
              }}</span>
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
              <span class="cost-card-label">{{
                t("production.components.costTotal")
              }}</span>
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
              <span class="cost-card-label">{{
                t("production.components.pesTotal")
              }}</span>
              <span class="cost-card-value"
                >{{ workmaster.totalWeight }} KG</span
              >
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  finiteNumberValue,
  integerValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { formatCurrency } from "../../../utils/functions";
import { useWorkMasterStore } from "../store/workmaster";
import type { WorkMaster } from "../types";

const props = defineProps<{
  workmaster: WorkMaster;
}>();

const emit = defineEmits<{
  (e: "submit", workmaster: WorkMaster): void;
  (e: "calculateCost", workmaster: WorkMaster): void;
}>();

const { t } = useI18n();
const workmasterStore = useWorkMasterStore();
const form = ref<{ submit: () => void } | null>(null);

// Secondary action waiting for the next valid submit, so it receives the
// validated workmaster instead of the unsaved source prop.
const pendingAction = ref<"calculateCost" | null>(null);

const totalCost = computed(() => {
  return (
    props.workmaster.operatorCost +
    props.workmaster.machineCost +
    props.workmaster.materialCost +
    props.workmaster.externalCost
  );
});

// Scalar snapshot: phases and costs stay with the parent-owned workmaster and
// are merged back at submit.
const initialValues = computed(() => ({
  referenceId: props.workmaster.referenceId,
  baseQuantity: props.workmaster.baseQuantity,
  volume: props.workmaster.volume,
  mode: props.workmaster.mode,
  disabled: props.workmaster.disabled,
}));

const decimalProps = { locale: "en-US", minFractionDigits: 2 } as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 6 },
    fields: [
      {
        name: "referenceId",
        label: t("production.components.referencia"),
        type: FormFieldType.Custom,
        span: { mobile: 1, tablet: 2, desktop: 2 },
        validation: Yup.string()
          .nullable()
          .required(t("production.validation.laReferenciaEsObligatoria")),
      },
      {
        name: "baseQuantity",
        label: t("production.components.quantitatBase"),
        type: FormFieldType.Number,
        props: decimalProps,
        validation: Yup.number()
          .nullable()
          .typeError(t("production.validation.laQuanitatBaseEsObligatoria"))
          .required(t("production.validation.laQuanitatBaseEsObligatoria"))
          .min(1, t("production.validation.laQuantitatBaseHaDeSerSuperiorA0")),
      },
      {
        name: "volume",
        label: t("production.components.volumMm3"),
        type: FormFieldType.Number,
        props: decimalProps,
      },
      {
        name: "mode",
        label: t("production.components.mode"),
        type: FormFieldType.Select,
        props: {
          options: workmasterStore.workmasterModes,
          optionLabel: "value",
          optionValue: "id",
          placeholder: t("production.components.seleccioneElModo"),
        },
      },
      {
        name: "disabled",
        label: t("production.components.desactivat"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const workmasterActions = computed(() => [
  {
    label: t("production.components.calcularCost"),
    icon: "pi pi-calculator",
    command: () => {
      pendingAction.value = "calculateCost";
      form.value?.submit();
    },
  },
]);

const save = (submitForm: () => void): void => {
  pendingAction.value = null;
  submitForm();
};

const submit = (values: FormValues): void => {
  const workmaster: WorkMaster = {
    ...props.workmaster,
    referenceId: stringValue(values.referenceId, props.workmaster.referenceId),
    baseQuantity: finiteNumberValue(
      values.baseQuantity,
      props.workmaster.baseQuantity,
    ),
    volume: finiteNumberValue(values.volume, 0),
    mode: integerValue(values.mode, props.workmaster.mode),
    disabled: booleanValue(values.disabled, false),
  };

  const action = pendingAction.value;
  pendingAction.value = null;
  if (action === "calculateCost") emit("calculateCost", workmaster);
  else emit("submit", workmaster);
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
