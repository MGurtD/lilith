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
import type { OperatorType } from "../types";

const props = defineProps<{
  operatortype: OperatorType;
}>();

const emit = defineEmits<{
  (event: "submit", operatorType: OperatorType): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "name",
        label: t("production.components.nom"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.elNomEsObligatori"))
          .max(250, t("production.validation.elNomNoPotSuperarEls250Caracters")),
      },
      {
        name: "description",
        label: t("production.components.descripcio"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.laDescripcioEsObligatoria"))
          .max(
            250,
            t("production.validation.laDescripcioNoPotSuperarEls250Caracters"),
          ),
      },
      {
        name: "cost",
        label: t("production.components.costHora"),
        type: FormFieldType.Number,
        props: {
          locale: "en-US",
          minFractionDigits: 2,
          suffix: " €",
          min: 0,
        },
        validation: Yup.number()
          .typeError(t("production.validation.elCostEsObligatori"))
          .required(t("production.validation.elCostEsObligatori"))
          .min(0, t("production.validation.costCannotBeNegative")),
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

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.operatortype,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    cost: finiteNumberValue(values.cost, props.operatortype.cost),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="operatortype"
    @submit="submit"
  />
</template>
