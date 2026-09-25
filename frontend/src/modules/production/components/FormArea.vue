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
import type { Area } from "../types";

const props = defineProps<{
  area: Area;
}>();

const emit = defineEmits<{
  (event: "submit", area: Area): void;
}>();

const { t } = useI18n();
const plantStore = usePlantModelStore();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
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
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "siteId",
        label: t("production.components.local"),
        type: FormFieldType.Select,
        props: {
          options: plantStore.sites ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("production.validation.elLocalEsObligatori"),
        ),
      },
      {
        name: "isVisibleInPlant",
        label: t("production.components.visiblePlanta"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
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
  await plantStore.fetchSites();
});

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.area,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    siteId: stringValue(values.siteId, props.area.siteId),
    isVisibleInPlant: booleanValue(values.isVisibleInPlant, false),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form page-actions :rows="rows" :initial-values="area" @submit="submit" />
</template>
