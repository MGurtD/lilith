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
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePlantModelStore } from "../store/plantmodel";
import type { Site } from "../types";

const props = defineProps<{
  site: Site;
}>();

const emit = defineEmits<{
  (event: "submit", site: Site): void;
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
        name: "vatNumber",
        label: t("production.fields.companyVatNumber"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "phoneNumber",
        label: t("production.components.telefon"),
        type: FormFieldType.Text,
      },
      {
        name: "email",
        label: t("production.components.emailGeneral"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .email(t("production.validation.elCorreuElectronicNoEsValid"))
          .required(t("production.validation.elCorreuElectronicEsObligatori")),
      },
      {
        name: "emailPurchase",
        label: t("production.components.emailCompres"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .email(t("production.validation.elCorreuElectronicDeCompresNoEsValid"))
          .required(
            t("production.validation.elCorreuElectronicDeCompresEsObligatori"),
          ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "emailSales",
        label: t("production.components.emailVentes"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .email(t("production.validation.elCorreuElectronicDeVentesNoEsValid"))
          .required(
            t("production.validation.elCorreuElectronicDeVentesEsObligatori"),
          ),
      },
      {
        name: "enterpriseId",
        label: t("production.components.empresa"),
        type: FormFieldType.Select,
        props: {
          options: plantStore.enterprises ?? [],
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("production.validation.lEmpresaEsObligatoria"),
        ),
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
    section: "location",
    fields: [
      {
        name: "country",
        label: t("location.country"),
        type: FormFieldType.Custom,
      },
      {
        name: "address",
        label: t("location.address"),
        type: FormFieldType.Custom,
      },
      {
        name: "city",
        label: t("location.city"),
        type: FormFieldType.Custom,
      },
      {
        name: "region",
        label: t("location.region"),
        type: FormFieldType.Custom,
      },
      {
        name: "postalCode",
        label: t("location.postalCode"),
        type: FormFieldType.Custom,
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
]);

onMounted(async () => {
  await plantStore.fetchEnterprises();
});

const locationValues = (values: FormValues): LocationData => ({
  country: stringValue(values.country, ""),
  address: stringValue(values.address, ""),
  city: stringValue(values.city, ""),
  region: stringValue(values.region, ""),
  postalCode: stringValue(values.postalCode, ""),
  latitude: finiteNumberValue(values.latitude, props.site.latitude),
  longitude: finiteNumberValue(values.longitude, props.site.longitude),
});

const setLocationValues = (
  location: LocationData,
  setValues: (values: FormValues) => void,
): void => {
  setValues({ ...location });
};

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.site,
    name: stringValue(values.name, ""),
    description: stringValue(values.description, ""),
    vatNumber: stringValue(values.vatNumber, props.site.vatNumber),
    phoneNumber: stringValue(values.phoneNumber, props.site.phoneNumber),
    email: stringValue(values.email, ""),
    emailPurchase: stringValue(values.emailPurchase, ""),
    emailSales: stringValue(values.emailSales, ""),
    enterpriseId: stringValue(values.enterpriseId, props.site.enterpriseId),
    disabled: booleanValue(values.disabled, false),
    ...locationValues(values),
  });
};
</script>

<template>
  <Form page-actions :rows="rows" :initial-values="site" @submit="submit">
    <template #section-location="{ values, errors, setValues, disabled }">
      <LocationFields
        :model-value="locationValues(values)"
        :validation-errors="errors"
        :disabled="disabled"
        @update:model-value="setLocationValues($event, setValues)"
      />
    </template>
  </Form>
</template>
