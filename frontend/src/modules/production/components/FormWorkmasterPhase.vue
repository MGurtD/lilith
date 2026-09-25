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
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import { isEqual, omit } from "lodash";
import { computed, onMounted, ref, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { useExerciseStore } from "../../shared/store/exercise";
import { useReferenceStore } from "../../shared/store/reference";
import { ReferenceCategoryEnum, type Reference } from "../../shared/types";
import { usePlantModelStore } from "../store/plantmodel";
import type {
  WorkMaster,
  WorkMasterPhase,
  WorkcenterProfitPercentage,
} from "../types";

const props = defineProps<{
  /** Rendered inside a dialog: default Cancel/Save footer instead of the header. */
  inDialog?: boolean;
  workmaster: WorkMaster;
  phase: WorkMasterPhase;
}>();

const emit = defineEmits<{
  (event: "submit", phase: WorkMasterPhase): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const plantModelStore = usePlantModelStore();
const referencesStore = useReferenceStore();
const exerciseStore = useExerciseStore();
const form = ref<{
  getValues: () => FormValues;
  setValues: (values: FormValues) => void;
} | null>(null);

const serviceReferences = ref<Reference[] | undefined>(undefined);
const workcenterProfitPercentages = ref<WorkcenterProfitPercentage[]>([]);
let workcenterRequestSequence = 0;
let suppressCallbacks = false;

onMounted(async () => {
  await exerciseStore.fetchActive();
  serviceReferences.value =
    await referencesStore.getReferencesByModuleAndCategory(
      "purchase",
      ReferenceCategoryEnum.SERVICE,
    );
});

// The phase screen refreshes the store phase whenever a step or material is
// saved. Only the form-owned scalar values feed the form, so an unchanged
// refresh keeps unsaved edits; the latest collections are merged at submit.
const phaseSnapshot = (phase: WorkMasterPhase) =>
  omit(phase, ["details", "billOfMaterials"]);
const initialPhase = shallowRef(phaseSnapshot(props.phase));
const selectedWorkcenterTypeId = ref<string | null>(
  props.phase.workcenterTypeId ?? null,
);

watch(
  () => phaseSnapshot(props.phase),
  (snapshot) => {
    if (isEqual(snapshot, initialPhase.value)) return;
    if (snapshot.id !== initialPhase.value.id) {
      workcenterProfitPercentages.value = [];
    }
    workcenterRequestSequence += 1;
    selectedWorkcenterTypeId.value = snapshot.workcenterTypeId ?? null;
    initialPhase.value = snapshot;
  },
);

const currentExercise = computed(() => {
  const now = new Date();
  if (!exerciseStore.exercises) return undefined;

  return exerciseStore.exercises.find(
    (e) =>
      !e.disabled && new Date(e.startDate) <= now && new Date(e.endDate) >= now,
  );
});

const externalProfit = computed(
  () => currentExercise.value?.externalProfit || 0,
);

const preferredWorkcenters = computed(() =>
  selectedWorkcenterTypeId.value
    ? plantModelStore.getWorkcentersByTypeId(selectedWorkcenterTypeId.value)
    : [],
);

const setFormValues = (values: FormValues): void => {
  suppressCallbacks = true;
  try {
    form.value?.setValues(values);
  } finally {
    suppressCallbacks = false;
  }
};

const workcenterTypeProfit = (workcenterTypeId: string | null) =>
  plantModelStore.workcenterTypes?.find((wt) => wt.id === workcenterTypeId)
    ?.profitPercentage;

const workcenterTypeUpdated = (value: unknown): void => {
  selectedWorkcenterTypeId.value = nullableStringValue(value, null);
  if (suppressCallbacks) return;

  workcenterRequestSequence += 1;
  const profitPercentage = workcenterTypeProfit(selectedWorkcenterTypeId.value);
  setFormValues({
    preferredWorkcenterId: null,
    ...(profitPercentage !== undefined ? { profitPercentage } : {}),
  });
};

const loadWorkcenterProfit = async (
  workcenterId: string | null,
): Promise<void> => {
  const requestSequence = ++workcenterRequestSequence;
  const selectedWorkcenter = plantModelStore.workcenters?.find(
    (wc) => wc.id === workcenterId,
  );

  let percentages: WorkcenterProfitPercentage[] = [];
  if (workcenterId) {
    await plantModelStore.fetchWorkcenterProfitPercentagesByWorkcenterId(
      workcenterId,
    );
    if (requestSequence !== workcenterRequestSequence) return;
    percentages = plantModelStore.workcenterProfitPercentages ?? [];
  }
  workcenterProfitPercentages.value = percentages;

  if (percentages.length > 0) {
    setFormValues({ profitPercentage: percentages[0].profitPercentage });
  } else if (selectedWorkcenter && selectedWorkcenter.profitPercentage > 0) {
    setFormValues({ profitPercentage: selectedWorkcenter.profitPercentage });
  } else {
    setFormValues({
      profitPercentage:
        workcenterTypeProfit(selectedWorkcenterTypeId.value) || 0,
    });
  }
};

const workcenterUpdated = (value: unknown): void => {
  if (suppressCallbacks) return;
  void loadWorkcenterProfit(nullableStringValue(value, null));
};

const isExternalWorkChanged = (value: unknown): void => {
  if (suppressCallbacks) return;

  workcenterRequestSequence += 1;
  if (value === true) {
    setFormValues({
      operatorTypeId: null,
      workcenterTypeId: null,
      preferredWorkcenterId: null,
      profitPercentage: externalProfit.value,
    });
  } else {
    setFormValues({
      externalWorkCost: 0,
      transportCost: 0,
      serviceReferenceId: null,
      profitPercentage: 0,
    });
  }
};

const serviceReferenceChanged = (value: unknown): void => {
  if (suppressCallbacks) return;

  const selectedReference = serviceReferences.value?.find(
    (r) => r.id === value,
  );
  if (selectedReference) {
    setFormValues({
      externalWorkCost: selectedReference.price,
      transportCost: selectedReference.transportAmount,
    });
  }
};

const isInternalWork = (values: Readonly<FormValues>): boolean =>
  values.isExternalWork !== true;

const currencyProps = {
  locale: "en-US",
  minFractionDigits: 2,
  suffix: " €",
} as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "code",
        label: t("production.components.codiDeLaFase"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("production.validation.elCodiEsObligatori"),
        ),
      },
      {
        name: "description",
        label: t("production.components.descripcio"),
        type: FormFieldType.Text,
        span: { desktop: 3 },
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "workcenterTypeId",
        label: t("production.components.tipusDeMaquina"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.workcenterTypes ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        onChange: workcenterTypeUpdated,
      },
      {
        name: "preferredWorkcenterId",
        label: t("production.components.maquinaPreferida"),
        type: FormFieldType.Select,
        props: {
          options: preferredWorkcenters.value,
          optionLabel: "description",
          optionValue: "id",
        },
        onChange: workcenterUpdated,
      },
      {
        name: "profitPercentage",
        label: t("production.components.margeDeBenefici"),
        type: FormFieldType.Custom,
      },
      {
        name: "operatorTypeId",
        label: t("production.components.tipusDOperari"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.operatorTypes ?? [],
          optionLabel: "description",
          optionValue: "id",
        },
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "isExternalWork",
        label: t("production.components.externa"),
        type: FormFieldType.Checkbox,
        onChange: isExternalWorkChanged,
      },
      {
        name: "serviceReferenceId",
        label: t("production.components.servei"),
        type: FormFieldType.Select,
        props: {
          options: serviceReferences.value ?? [],
          optionLabel: (r: Reference) => `${r.code} - ${r.description}`,
          optionValue: "id",
        },
        disabled: isInternalWork,
        onChange: serviceReferenceChanged,
      },
      {
        name: "externalWorkCost",
        label: t("production.components.costServei"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: isInternalWork,
      },
      {
        name: "transportCost",
        label: t("production.components.costTransport"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: isInternalWork,
      },
    ],
  },
]);

const toPhase = (values: FormValues): WorkMasterPhase => {
  const preferredWorkcenterId = nullableStringValue(
    values.preferredWorkcenterId,
    props.phase.preferredWorkcenterId ?? null,
  );

  return {
    ...props.phase,
    code: stringValue(values.code, props.phase.code),
    description: stringValue(values.description, props.phase.description),
    workcenterTypeId: nullableStringValue(
      values.workcenterTypeId,
      props.phase.workcenterTypeId ?? null,
    ),
    preferredWorkcenterId:
      preferredWorkcenterId === "" ? null : preferredWorkcenterId,
    profitPercentage: finiteNumberValue(
      values.profitPercentage,
      props.phase.profitPercentage,
    ),
    operatorTypeId: nullableStringValue(
      values.operatorTypeId,
      props.phase.operatorTypeId ?? null,
    ),
    isExternalWork: booleanValue(
      values.isExternalWork,
      props.phase.isExternalWork,
    ),
    serviceReferenceId: nullableStringValue(
      values.serviceReferenceId,
      props.phase.serviceReferenceId ?? null,
    ),
    externalWorkCost: finiteNumberValue(values.externalWorkCost, 0),
    transportCost: finiteNumberValue(values.transportCost, 0),
  };
};

const submit = (values: FormValues): void => {
  emit("submit", toPhase(values));
};

// Unsaved header edits, used by the screen when saving steps or materials
// so the reload that follows does not discard them (no validation, as
// before the migration).
const currentPhase = (): WorkMasterPhase => {
  const values = form.value?.getValues();
  return values ? toPhase(values) : props.phase;
};
defineExpose({ currentPhase });
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="initialPhase"
    :page-actions="!inDialog"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-profitPercentage="{ value, setValue, disabled, inputId }">
      <Select
        v-if="workcenterProfitPercentages.length > 0"
        :label-id="inputId"
        :model-value="value"
        :options="workcenterProfitPercentages"
        option-value="profitPercentage"
        option-label="profitPercentage"
        class="w-full"
        :placeholder="t('production.components.seleccionaUnPercentatge')"
        :disabled="disabled"
        @update:model-value="setValue"
      >
        <template #value="slotProps">
          <span v-if="slotProps.value">{{ slotProps.value }}%</span>
          <span v-else>{{ slotProps.placeholder }}</span>
        </template>
        <template #option="slotProps">
          {{ slotProps.option.profitPercentage }}%
        </template>
      </Select>
      <InputNumber
        v-else
        :input-id="inputId"
        :model-value="typeof value === 'number' ? value : null"
        :min-fraction-digits="2"
        :max-fraction-digits="2"
        suffix="%"
        class="w-full"
        disabled
      />
    </template>
  </Form>
</template>
