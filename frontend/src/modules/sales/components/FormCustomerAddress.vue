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
import LocationFields from "@/components/LocationFields.vue";
import type { LocationData } from "@/types";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { CustomerAddress } from "../types";

const props = defineProps<{
  address: CustomerAddress;
}>();

const emit = defineEmits<{
  (event: "submit", address: CustomerAddress): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("sales.components.nom"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.nameRequired"))
          .max(250, t("sales.validation.nameMaxLength")),
      },
      {
        name: "main",
        label: t("sales.components.principal"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
        validation: Yup.boolean().required(),
      },
      {
        name: "disabled",
        label: t("sales.components.desactivada"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
        validation: Yup.boolean().required(),
      },
    ],
  },
  {
    section: "location",
    fields: [
      {
        name: "country",
        label: t("location.country"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.countryRequired"),
        ),
      },
      {
        name: "address",
        label: t("location.address"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.addressRequired"),
        ),
      },
      {
        name: "city",
        label: t("location.city"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(t("sales.validation.cityRequired")),
      },
      {
        name: "region",
        label: t("location.region"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.regionRequired"),
        ),
      },
      {
        name: "postalCode",
        label: t("location.postalCode"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.postalCodeRequired"),
        ),
      },
      {
        name: "latitude",
        label: t("location.latitude"),
        type: FormFieldType.Custom,
      },
      {
        name: "longitude",
        label: t("location.longitude"),
        type: FormFieldType.Custom,
      },
    ],
  },
  {
    fields: [
      {
        name: "observations",
        label: t("sales.components.observacions"),
        type: FormFieldType.Textarea,
      },
    ],
  },
]);

const locationValues = (values: FormValues): LocationData => ({
  country: stringValue(values.country, ""),
  address: stringValue(values.address, ""),
  city: stringValue(values.city, ""),
  region: stringValue(values.region, ""),
  postalCode: stringValue(values.postalCode, ""),
  latitude: finiteNumberValue(values.latitude, props.address.latitude),
  longitude: finiteNumberValue(values.longitude, props.address.longitude),
  distanceFromSite: finiteNumberValue(
    values.distanceFromSite,
    props.address.distanceFromSite,
  ),
});

const setLocationValues = (
  location: LocationData,
  setValues: (values: FormValues) => void,
): void => {
  setValues({ ...location });
};

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.address,
    name: stringValue(values.name, ""),
    main: booleanValue(values.main, false),
    disabled: booleanValue(values.disabled, false),
    ...locationValues(values),
    observations: stringValue(values.observations, props.address.observations),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="address"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #section-location="{ values, errors, setValues, disabled }">
      <LocationFields
        :model-value="locationValues(values)"
        :show-distance="true"
        :validation-errors="errors"
        :show-validation-messages="true"
        :disabled="disabled"
        @update:model-value="setLocationValues($event, setValues)"
      />
    </template>
  </Form>
</template>
