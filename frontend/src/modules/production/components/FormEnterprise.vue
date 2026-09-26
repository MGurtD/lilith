<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePlantModelStore } from "../store/plantmodel";
import type { Enterprise } from "../types";

const props = defineProps<{
  enterprise: Enterprise;
}>();

const emit = defineEmits<{
  (event: "submit", enterprise: Enterprise): void;
}>();

const { t } = useI18n();
const plantStore = usePlantModelStore();

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
        name: "defaultSiteId",
        label: t("production.components.seuPerDefecte"),
        type: FormFieldType.Select,
        props: {
          options: (plantStore.sites ?? []).filter(
            (site) => site.enterpriseId === props.enterprise.id,
          ),
          optionLabel: "name",
          optionValue: "id",
        },
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
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
  if (!plantStore.sites) await plantStore.fetchSites();
});

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.enterprise,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    defaultSiteId: nullableStringValue(
      values.defaultSiteId,
      props.enterprise.defaultSiteId ?? null,
    ),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="enterprise"
    @submit="submit"
  />
</template>
