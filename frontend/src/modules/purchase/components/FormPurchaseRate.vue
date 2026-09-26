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
import type { PurchaseRate } from "../types";

const props = defineProps<{
  purchaseRate: PurchaseRate;
}>();

const emit = defineEmits<{
  (event: "submit", rate: PurchaseRate): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "name",
        label: t("purchase.purchaseRate.fields.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.purchaseRate.validation.nameRequired"))
          .max(250, t("purchase.purchaseRate.validation.nameMaxLength")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "validFrom",
        label: t("purchase.purchaseRate.fields.validFrom"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("purchase.purchaseRate.validation.startDateRequired"))
          .required(t("purchase.purchaseRate.validation.startDateRequired")),
      },
      {
        name: "validTo",
        label: t("purchase.purchaseRate.fields.validTo"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("purchase.purchaseRate.validation.endDateRequired"))
          .required(t("purchase.purchaseRate.validation.endDateRequired"))
          .min(
            Yup.ref("validFrom"),
            t("purchase.purchaseRate.validation.endDateOnOrAfterStart"),
          ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.purchaseRate,
    name: stringValue(values.name, "").trim(),
    validFrom: dateValue(values.validFrom, props.purchaseRate.validFrom),
    validTo: dateValue(values.validTo, props.purchaseRate.validTo),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="purchaseRate"
    :show-cancel="false"
    @submit="submit"
  />
</template>
