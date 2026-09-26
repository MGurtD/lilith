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
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePlantModelStore } from "../store/plantmodel";
import type { WorkOrderPhaseDetail } from "../types";

const props = defineProps<{
  detail: WorkOrderPhaseDetail;
}>();

const emit = defineEmits<{
  (event: "submit", detail: WorkOrderPhaseDetail): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const plantModelStore = usePlantModelStore();

const decimalProps = { locale: "en-US", minFractionDigits: 2 } as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "order",
        label: t("production.components.ordre"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
        validation: Yup.number()
          .typeError(t("production.validation.lOrdreEsObligatori"))
          .required(t("production.validation.lOrdreEsObligatori"))
          .positive(t("production.validation.orderMustBePositive")),
      },
      {
        name: "machineStatusId",
        label: t("production.components.estat"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.machineStatuses ?? [],
          optionValue: "id",
          optionLabel: "description",
        },
      },
      {
        name: "isCycleTime",
        label: t("production.components.tempsDeCicle"),
        type: FormFieldType.Checkbox,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "estimatedTime",
        label: t("production.components.tempsMaquinaMin"),
        type: FormFieldType.Number,
        props: decimalProps,
        validation: Yup.number()
          .typeError(t("production.validation.elTempsEstimatEsObligatori"))
          .required(t("production.validation.elTempsEstimatEsObligatori")),
      },
      {
        name: "estimatedOperatorTime",
        label: t("production.components.tempsOperariMin"),
        type: FormFieldType.Number,
        props: decimalProps,
      },
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
    ...props.detail,
    order: finiteNumberValue(values.order, props.detail.order),
    machineStatusId: stringValue(
      values.machineStatusId,
      props.detail.machineStatusId,
    ),
    isCycleTime: booleanValue(values.isCycleTime, props.detail.isCycleTime),
    estimatedTime: finiteNumberValue(
      values.estimatedTime,
      props.detail.estimatedTime,
    ),
    estimatedOperatorTime: finiteNumberValue(
      values.estimatedOperatorTime,
      props.detail.estimatedOperatorTime,
    ),
    comment: stringValue(values.comment, props.detail.comment),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="detail"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>
