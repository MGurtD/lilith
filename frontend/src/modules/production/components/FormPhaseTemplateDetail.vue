<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePlantModelStore } from "../store/plantmodel";
import type { PhaseTemplateDetail } from "../types";

const props = defineProps<{
  detail: PhaseTemplateDetail;
}>();

const emit = defineEmits<{
  (event: "submit", detail: PhaseTemplateDetail): void;
  (event: "cancel"): void;
}>();

const plantModelStore = usePlantModelStore();
const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "order",
        label: t("phaseTemplates.details.fields.order"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
        validation: Yup.number()
          .typeError(t("phaseTemplates.details.validation.orderRequired"))
          .required(t("phaseTemplates.details.validation.orderRequired"))
          .positive(t("phaseTemplates.details.validation.orderPositive")),
      },
      {
        name: "machineStatusId",
        label: t("phaseTemplates.details.fields.machineStatus"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.machineStatuses ?? [],
          optionLabel: "description",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("phaseTemplates.details.validation.machineStatusRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 1 },
    fields: [
      {
        name: "comment",
        label: t("phaseTemplates.details.fields.comment"),
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
