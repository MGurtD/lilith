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
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePlantModelStore } from "../store/plantmodel";
import type { WorkcenterCost } from "../types";

const props = defineProps<{
  workcentercost: WorkcenterCost;
}>();

const emit = defineEmits<{
  (event: "submit", workcentercost: WorkcenterCost): void;
}>();

const { t } = useI18n();
const plantModelStore = usePlantModelStore();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "workcenterId",
        label: t("production.components.maquina"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.workcenters ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("production.validation.laMaquinaEsObligatoria"),
        ),
      },
      {
        name: "machineStatusId",
        label: t("production.components.estatDeMaquina"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.machineStatuses ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("production.validation.lEstatDeMaquinaEsObligatori"),
        ),
      },
      {
        name: "cost",
        label: t("production.components.preuHora"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 2, suffix: " €" },
        validation: Yup.number()
          .typeError(t("production.validation.elCostEsObligatori"))
          .required(t("production.validation.elCostEsObligatori")),
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

onMounted(async () => {
  await plantModelStore.fetchMachineStatuses();
});

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.workcentercost,
    workcenterId: stringValue(
      values.workcenterId,
      props.workcentercost.workcenterId,
    ),
    machineStatusId: stringValue(
      values.machineStatusId,
      props.workcentercost.machineStatusId,
    ),
    cost: finiteNumberValue(values.cost, props.workcentercost.cost),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="workcentercost"
    @submit="submit"
  />
</template>
