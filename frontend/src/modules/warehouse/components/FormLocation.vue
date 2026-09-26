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
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { LOCATION_TYPE_OPTIONS, type Location } from "../types";

const props = defineProps<{
  location: Location;
}>();

const emit = defineEmits<{
  (event: "submit", location: Location): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const locationTypeLabels = computed<Record<string, string>>(() => ({
  Supply: t("warehouse.locationTypes.supply"),
  Receiving: t("warehouse.locationTypes.receiving"),
  Shipping: t("warehouse.locationTypes.shipping"),
  Storage: t("warehouse.locationTypes.storage"),
}));

const locationTypeOptions = computed(() =>
  LOCATION_TYPE_OPTIONS.map((option) => ({
    value: option.value,
    label: locationTypeLabels.value[option.value] ?? option.label,
  })),
);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
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
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "locationType",
        label: t("warehouse.fields.locationType"),
        type: FormFieldType.Select,
        props: {
          options: locationTypeOptions.value,
          optionLabel: "label",
          optionValue: "value",
          placeholder: t("warehouse.placeholders.noLocationType"),
          showClear: true,
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
    ...props.location,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    locationType: nullableStringValue(
      values.locationType,
      props.location.locationType ?? null,
    ),
    disabled: booleanValue(values.disabled, false),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="location"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>
