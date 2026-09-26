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
import { usePlantModelStore } from "../../production/store/plantmodel";
import type { Warehouse } from "../types";

const props = defineProps<{
  warehouse: Warehouse;
}>();

const emit = defineEmits<{
  (event: "submit", warehouse: Warehouse): void;
}>();

const { t } = useI18n();
const plantmodelStore = usePlantModelStore();

onMounted(async () => {
  await plantmodelStore.fetchSites();
});

// Scalar snapshot only: the locations collection is owned by the parent's
// locations table and merged back from the live prop at submit.
const initialValues = computed(() => ({
  id: props.warehouse.id,
  name: props.warehouse.name,
  description: props.warehouse.description,
  siteId: props.warehouse.siteId,
  defaultLocationId: props.warehouse.defaultLocationId,
  disabled: props.warehouse.disabled,
}));

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("warehouse.fields.name"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("warehouse.validation.nameRequired"))
          .max(250, t("warehouse.validation.nameMaxLength")),
      },
      {
        name: "description",
        label: t("common.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("warehouse.validation.descriptionRequired"))
          .max(250, t("warehouse.validation.descriptionMaxLength")),
      },
      {
        name: "siteId",
        label: t("warehouse.fields.site"),
        type: FormFieldType.Select,
        props: {
          options: plantmodelStore.sites ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("warehouse.validation.siteRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "defaultLocationId",
        label: t("warehouse.fields.defaultLocation"),
        type: FormFieldType.Select,
        props: {
          options: props.warehouse.locations ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
      },
      {
        name: "disabled",
        label: t("warehouse.fields.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.warehouse,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    siteId: stringValue(values.siteId, props.warehouse.siteId),
    defaultLocationId: nullableStringValue(
      values.defaultLocationId,
      props.warehouse.defaultLocationId,
    ),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
  />
</template>
