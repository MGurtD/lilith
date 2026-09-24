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
import { computed, onMounted, ref, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useReferenceStore } from "../../shared/store/reference";
import { type Reference, ReferenceCategoryEnum } from "../../shared/types";
import { usePlantModelStore } from "../store/plantmodel";
import type { WorkOrder, WorkOrderPhase } from "../types";

const props = defineProps<{
  /** Rendered inside a dialog: keep Save in the dialog footer instead of the header. */
  inDialog?: boolean;
  workorder: WorkOrder;
  phase: WorkOrderPhase;
}>();

const emit = defineEmits<{
  (event: "submit", phase: WorkOrderPhase): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const plantModelStore = usePlantModelStore();
const referencesStore = useReferenceStore();
const form = ref<{
  submit: () => void;
  setValues: (values: FormValues) => void;
} | null>(null);
const statusTransitionsDropdown = ref<InstanceType<
  typeof DropdownLifecycleStatusTransitions
> | null>(null);
const serviceReferences = ref<Reference[]>([]);
let suppressCascades = false;

// The parent refetches the phase (and its details and materials) after every
// detail or material change, so the form receives a stable scalar snapshot
// instead of the whole entity; it only resets when a form-owned value changes.
type PhaseScalars = Pick<
  WorkOrderPhase,
  | "id"
  | "code"
  | "description"
  | "statusId"
  | "workcenterTypeId"
  | "preferredWorkcenterId"
  | "profitPercentage"
  | "operatorTypeId"
  | "isExternalWork"
  | "serviceReferenceId"
  | "externalWorkCost"
  | "transportCost"
>;

const scalarSnapshot = (phase: WorkOrderPhase): PhaseScalars => ({
  id: phase.id,
  code: phase.code,
  description: phase.description,
  statusId: phase.statusId,
  workcenterTypeId: phase.workcenterTypeId ?? null,
  preferredWorkcenterId: phase.preferredWorkcenterId ?? null,
  profitPercentage: phase.profitPercentage,
  operatorTypeId: phase.operatorTypeId ?? null,
  isExternalWork: phase.isExternalWork,
  serviceReferenceId: phase.serviceReferenceId ?? null,
  externalWorkCost: phase.externalWorkCost,
  transportCost: phase.transportCost,
});

const initialValues = shallowRef<PhaseScalars>(scalarSnapshot(props.phase));
// Mirrors the workcenter type so the preferred workcenter options follow it.
const workcenterTypeId = ref<string | null>(
  initialValues.value.workcenterTypeId ?? null,
);

watch(
  () => scalarSnapshot(props.phase),
  (next) => {
    const current = initialValues.value;
    const changed = (Object.keys(next) as Array<keyof PhaseScalars>).some(
      (key) => next[key] !== current[key],
    );
    if (changed) {
      initialValues.value = next;
      workcenterTypeId.value = next.workcenterTypeId ?? null;
    }
  },
);

onMounted(async () => {
  serviceReferences.value =
    (await referencesStore.getReferencesByModuleAndCategory(
      "purchase",
      ReferenceCategoryEnum.SERVICE,
    )) ?? [];
});

const preferredWorkcenters = computed(() =>
  workcenterTypeId.value
    ? plantModelStore.getWorkcentersByTypeId(workcenterTypeId.value)
    : [],
);

const setFormValues = (values: FormValues): void => {
  suppressCascades = true;
  try {
    form.value?.setValues(values);
  } finally {
    suppressCascades = false;
  }
};

const workcenterTypeProfit = (typeId: unknown): number | undefined =>
  plantModelStore.workcenterTypes?.find((type) => type.id === typeId)
    ?.profitPercentage;

const workcenterTypeUpdated = (value: unknown): void => {
  if (suppressCascades) return;
  workcenterTypeId.value = nullableStringValue(value, null);

  const profitPercentage = workcenterTypeProfit(value);
  setFormValues(
    profitPercentage === undefined
      ? { preferredWorkcenterId: null }
      : { preferredWorkcenterId: null, profitPercentage },
  );
};

const workcenterUpdated = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  if (suppressCascades) return;
  const workcenter = plantModelStore.workcenters?.find(
    (item) => item.id === value,
  );
  if (!workcenter) return;

  const profitPercentage =
    workcenter.profitPercentage > 0
      ? workcenter.profitPercentage
      : workcenterTypeProfit(values.workcenterTypeId);
  if (profitPercentage !== undefined) setFormValues({ profitPercentage });
};

const isExternalWorkChanged = (value: unknown): void => {
  if (suppressCascades) return;
  if (value === true) {
    workcenterTypeId.value = null;
    setFormValues({
      operatorTypeId: null,
      workcenterTypeId: null,
      preferredWorkcenterId: null,
    });
  } else {
    setFormValues({
      externalWorkCost: 0,
      transportCost: 0,
      serviceReferenceId: null,
    });
  }
};

const onServiceReferenceChanged = (value: unknown): void => {
  if (suppressCascades) return;
  const selectedReference = serviceReferences.value.find(
    (reference) => reference.id === value,
  );
  if (selectedReference) {
    setFormValues({
      externalWorkCost: selectedReference.price,
      transportCost: selectedReference.transportAmount,
    });
  }
};

const isNotExternalWork = (values: Readonly<FormValues>): boolean =>
  values.isExternalWork !== true;

const currencyProps = {
  locale: "en-US",
  minFractionDigits: 2,
  suffix: " €",
} as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
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
      },
      {
        name: "statusId",
        label: t("production.components.estat"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("production.validation.lEstatEsObligatori"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "workcenterTypeId",
        label: t("production.components.tipusDeMaquina"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.workcenterTypes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        onChange: workcenterTypeUpdated,
      },
      {
        name: "preferredWorkcenterId",
        label: t("production.components.maquinaPreferida"),
        type: FormFieldType.Select,
        props: {
          options: preferredWorkcenters.value,
          optionValue: "id",
          optionLabel: "description",
        },
        onChange: workcenterUpdated,
      },
      {
        name: "profitPercentage",
        label: t("production.components.margeDeBenefici"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 2, suffix: "%" },
      },
      {
        name: "operatorTypeId",
        label: t("production.components.tipusDOperari"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.operatorTypes ?? [],
          optionValue: "id",
          optionLabel: "description",
        },
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 4 },
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
          options: serviceReferences.value,
          optionValue: "id",
          optionLabel: (reference: Reference) =>
            `${reference.code} - ${reference.description}`,
        },
        disabled: isNotExternalWork,
        onChange: onServiceReferenceChanged,
      },
      {
        name: "externalWorkCost",
        label: t("production.components.costServei"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: isNotExternalWork,
      },
      {
        name: "transportCost",
        label: t("production.components.costTransport"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: isNotExternalWork,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  const preferredWorkcenterId = nullableStringValue(
    values.preferredWorkcenterId,
    props.phase.preferredWorkcenterId ?? null,
  );

  emit("submit", {
    ...props.phase,
    code: stringValue(values.code, props.phase.code),
    description: stringValue(values.description, props.phase.description),
    statusId: stringValue(values.statusId, props.phase.statusId),
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
    externalWorkCost: finiteNumberValue(
      values.externalWorkCost,
      props.phase.externalWorkCost,
    ),
    transportCost: finiteNumberValue(
      values.transportCost,
      props.phase.transportCost,
    ),
  });
};

const submitForm = (): void => form.value?.submit();

const reloadLifecycleTransitions = async (): Promise<void> => {
  await statusTransitionsDropdown.value?.reloadTransitions();
};

defineExpose({ submitForm, reloadLifecycleTransitions });
</script>

<template>
  <Form
    ref="form"
    :page-actions="!inDialog"
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-statusId="{ value, setValue, disabled, inputId }">
      <DropdownLifecycleStatusTransitions
        ref="statusTransitionsDropdown"
        :input-id="inputId"
        label=""
        :status-id="phase.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>
