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
import { useShiftStore } from "../store/shift";
import type { Workcenter } from "../types";

const props = defineProps<{
  workcenter: Workcenter;
}>();

const emit = defineEmits<{
  (event: "submit", workcenter: Workcenter): void;
}>();

const { t } = useI18n();
const plantModelStore = usePlantModelStore();
const shiftStore = useShiftStore();

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
        name: "profitPercentage",
        label: t("production.components.margeDeBenefici"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 2, suffix: "%" },
      },
      {
        name: "disabled",
        label: t("production.components.desactivat"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "workcenterTypeId",
        label: t("production.components.tipus"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.workcenterTypes ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("production.validation.elTipusEsObligatori"),
        ),
      },
      {
        name: "areaId",
        label: t("production.components.area"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.areas ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("production.validation.lAreaEsObligatoria"),
        ),
      },
      {
        name: "shiftId",
        label: t("production.components.torn"),
        type: FormFieldType.Select,
        props: {
          options: shiftStore.shifts ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("production.validation.elTornEsObligatori"),
        ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.workcenter,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    profitPercentage: finiteNumberValue(
      values.profitPercentage,
      props.workcenter.profitPercentage,
    ),
    workcenterTypeId: stringValue(
      values.workcenterTypeId,
      props.workcenter.workcenterTypeId,
    ),
    areaId: stringValue(values.areaId, props.workcenter.areaId),
    shiftId: stringValue(values.shiftId, props.workcenter.shiftId),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="workcenter"
    @submit="submit"
  />
</template>
