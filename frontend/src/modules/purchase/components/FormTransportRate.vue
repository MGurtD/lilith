<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  dateValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { TransportRate } from "../types";

const props = defineProps<{
  transportRate: TransportRate;
}>();

const emit = defineEmits<{
  (event: "submit", rate: TransportRate): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "name",
        label: t("purchase.fields.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.transportRates.validation.nameRequired"))
          .max(250, t("purchase.transportRates.validation.nameMaxLength")),
      },
      {
        name: "description",
        label: t("purchase.fields.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(
            t("purchase.transportRates.validation.descriptionRequired"),
          )
          .max(
            250,
            t("purchase.transportRates.validation.descriptionMaxLength"),
          ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "validFrom",
        label: t("purchase.fields.startDate"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("purchase.transportRates.validation.startDateRequired"))
          .required(t("purchase.transportRates.validation.startDateRequired")),
      },
      {
        name: "validTo",
        label: t("purchase.fields.endDate"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("purchase.transportRates.validation.endDateRequired"))
          .required(t("purchase.transportRates.validation.endDateRequired"))
          .min(
            Yup.ref("validFrom"),
            t("purchase.transportRates.validation.endDateOnOrAfterStart"),
          ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.transportRate,
    name: stringValue(values.name, "").trim(),
    description: stringValue(values.description, "").trim(),
    validFrom: dateValue(values.validFrom, props.transportRate.validFrom),
    validTo: dateValue(values.validTo, props.transportRate.validTo),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="transportRate"
    :show-cancel="false"
    @submit="submit"
  />
</template>
