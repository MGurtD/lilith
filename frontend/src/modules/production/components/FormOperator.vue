<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { booleanValue, stringValue } from "@/components/forms/value-utils";
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePlantModelStore } from "../store/plantmodel";
import type { Operator } from "../types";

const props = defineProps<{
  operator: Operator;
}>();

const emit = defineEmits<{
  (event: "submit", operator: Operator): void;
}>();

const { t } = useI18n();
const plantModelStore = usePlantModelStore();

onMounted(async () => {
  await plantModelStore.fetchOperatorTypes();
});

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
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
        name: "surname",
        label: t("production.components.cognom"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.elCognomEsObligatori"))
          .max(
            250,
            t("production.validation.elCognomNoPotSuperarEls250Caracters"),
          ),
      },
      {
        name: "code",
        label: t("production.components.codi"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.elCodiEsObligatori"))
          .max(10, t("production.validation.elCodiNoPotSuperarEls10Caracters")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "vatNumber",
        label: t("production.fields.personalVatNumber"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("production.validation.elNifEsObligatori"))
          .max(20, t("production.validation.elNifNoPotSuperarEls20Caracters")),
      },
      {
        name: "operatorTypeId",
        label: t("production.components.tipusDOperari"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.operatorTypes,
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string()
          .nullable()
          .required(t("production.validation.elTipusDOperariEsObligatori")),
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
    ...props.operator,
    name: stringValue(values.name, ""),
    surname: stringValue(values.surname, ""),
    code: stringValue(values.code, ""),
    vatNumber: stringValue(values.vatNumber, ""),
    operatorTypeId: stringValue(
      values.operatorTypeId,
      props.operator.operatorTypeId,
    ),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="operator"
    @submit="submit"
  />
</template>
