<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  dateValue,
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import { PrimeIcons } from "@primevue/core/api";
import { computed, ref, shallowRef, useId } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import type { WorkOrder } from "../types";

const props = defineProps<{
  workorder: WorkOrder;
  reportDownloading?: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", workorder: WorkOrder): void;
  (e: "cancel"): void;
  (e: "download"): void;
  (e: "downloadPdf"): void;
}>();

const { t } = useI18n();
const form = shallowRef<{ submit: () => void } | null>(null);
const statusTransitionsDropdown = ref<InstanceType<
  typeof DropdownLifecycleStatusTransitions
> | null>(null);
const executionPeriodInputId = `workorder-execution-period-${useId()}`;

const numberProps = { locale: "en-US", minFractionDigits: 0 } as const;

const items = computed(() => [
  {
    label: t("production.components.descarregarExcel"),
    icon: PrimeIcons.FILE_EXCEL,
    command: () => emit("download"),
  },
  {
    label: t("production.components.descarregarPdf"),
    icon: PrimeIcons.FILE_PDF,
    command: () => emit("downloadPdf"),
  },
]);

const toDate = (value: unknown): Date | null => {
  if (value instanceof Date) return value;
  if (typeof value === "string" && value !== "") return new Date(value);
  return null;
};

// Scalar snapshot: phases and other collections stay with the store owner and
// are merged back from the latest prop at submit.
const initialValues = computed<FormValues>(() => ({
  code: props.workorder.code,
  referenceId: props.workorder.referenceId,
  plannedDate: toDate(props.workorder.plannedDate),
  plannedQuantity: props.workorder.plannedQuantity,
  statusId: props.workorder.statusId,
  order: props.workorder.order,
  startTime: toDate(props.workorder.startTime),
  endTime: toDate(props.workorder.endTime),
  totalQuantity: props.workorder.totalQuantity,
  comment: props.workorder.comment,
}));

const executionPeriod = (values: Readonly<FormValues>): Date[] => {
  const range: Date[] = [];
  const startTime = dateValue(values.startTime, null);
  const endTime = dateValue(values.endTime, null);
  if (startTime) range.push(startTime);
  if (endTime) range.push(endTime);
  return range;
};

const setExecutionPeriod = (
  value: unknown,
  setValues: (values: FormValues) => void,
): void => {
  const range = Array.isArray(value) ? value : [];
  setValues({
    startTime: dateValue(range[0], null),
    endTime: dateValue(range[1], null),
  });
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "code",
        label: t("production.components.codi"),
        type: FormFieldType.Text,
        disabled: true,
      },
      {
        name: "referenceId",
        label: t("production.components.referencia"),
        type: FormFieldType.Custom,
        disabled: true,
        validation: Yup.string().required(
          t("production.validation.laReferenciaEsObligatoria"),
        ),
      },
      {
        name: "plannedDate",
        label: t("production.components.dataPrevista"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy", showTime: true, hourFormat: "24" },
        validation: Yup.date()
          .typeError(t("production.validation.laDataPrevistaEsObligatoria"))
          .required(t("production.validation.laDataPrevistaEsObligatoria")),
      },
      {
        name: "plannedQuantity",
        label: t("production.components.quantitatPrevista"),
        type: FormFieldType.Number,
        props: numberProps,
        validation: Yup.number()
          .min(1, t("production.validation.laQuantitatHaDeSerSuperiorA0"))
          .required(t("production.validation.laQuanitatEsObligatoria")),
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "statusId",
        label: t("production.components.estat"),
        type: FormFieldType.Custom,
      },
      {
        name: "order",
        label: t("production.components.prioritat"),
        type: FormFieldType.Number,
        props: numberProps,
        validation: Yup.number().required(
          t("production.validation.lOrdreEsObligatori"),
        ),
      },
      {
        name: "totalQuantity",
        label: t("production.components.quantitatTotal"),
        type: FormFieldType.Number,
        props: numberProps,
        disabled: true,
      },
    ],
  },
  {
    section: "executionPeriod",
    fields: [
      { name: "startTime", label: "", type: FormFieldType.Custom },
      { name: "endTime", label: "", type: FormFieldType.Custom },
    ],
  },
  {
    fields: [
      {
        name: "comment",
        label: t("production.components.comentariFabricacio"),
        type: FormFieldType.Textarea,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.workorder,
    plannedDate: dateValue(values.plannedDate, null),
    plannedQuantity: finiteNumberValue(
      values.plannedQuantity,
      props.workorder.plannedQuantity,
    ),
    statusId: stringValue(values.statusId, props.workorder.statusId),
    order: finiteNumberValue(values.order, props.workorder.order),
    startTime: dateValue(values.startTime, null),
    endTime: dateValue(values.endTime, null),
    comment: stringValue(values.comment, props.workorder.comment),
  });
};

const submitForm = (): void => form.value?.submit();

const reloadLifecycleTransitions = async (): Promise<void> => {
  await statusTransitionsDropdown.value?.reloadTransitions();
};

defineExpose({
  submitForm,
  reloadLifecycleTransitions,
});
</script>

<template>
  <Form
    ref="form"
    page-actions
    class="pt-3"
    :rows="rows"
    :initial-values="initialValues"
    :loading="reportDownloading"
    @submit="submit"
  >
    <template #field-referenceId="{ value, disabled, inputId }">
      <DropdownReference
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :full-name="true"
        :disabled="disabled"
      />
    </template>

    <template #field-statusId="{ value, setValue, disabled, inputId }">
      <DropdownLifecycleStatusTransitions
        ref="statusTransitionsDropdown"
        :input-id="inputId"
        label=""
        :status-id="workorder.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #section-executionPeriod="{ values, setValues, disabled }">
      <div class="execution-period">
        <label class="block text-900 mb-2" :for="executionPeriodInputId">
          {{ t("production.components.periodeExecucio") }}
        </label>
        <DatePicker
          :input-id="executionPeriodInputId"
          :model-value="executionPeriod(values)"
          selection-mode="range"
          date-format="dd/mm/yy"
          show-time
          hour-format="24"
          show-icon
          class="w-full"
          :disabled="disabled"
          @update:model-value="setExecutionPeriod($event, setValues)"
        />
      </div>
    </template>

    <template #actions="{ submit: submitWorkorder, loading, disabled }">
      <SplitButton
        icon="pi pi-save"
        :label="t('production.components.guardar')"
        :model="items"
        :loading="loading"
        :disabled="disabled"
        @click="submitWorkorder"
      />
    </template>
  </Form>
</template>

<style scoped>
.execution-period {
  min-width: 0;
}

@media (min-width: 768px) {
  .execution-period {
    width: calc(50% - 0.5rem);
  }
}
</style>
