<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import LocationFields from "@/components/LocationFields.vue";
import type { LocationData } from "@/types";
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePaymentMethodStore } from "../../shared/store/paymentMethod";
import { useSuppliersStore } from "../store/suppliers";
import type { Supplier } from "../types";

const props = defineProps<{
  supplier: Supplier;
}>();

const emit = defineEmits<{
  (event: "submit", supplier: Supplier): void;
  (event: "cancel"): void;
}>();

const supplierStore = useSuppliersStore();
const paymentMethodStore = usePaymentMethodStore();
const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "comercialName",
        label: t("purchase.supplier.fields.commercialName"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplier.validation.commercialNameRequired"))
          .max(
            250,
            t("purchase.supplier.validation.commercialNameMaxLength"),
          ),
      },
      {
        name: "taxName",
        label: t("purchase.supplier.fields.taxName"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplier.validation.taxNameRequired")),
      },
      {
        name: "vatNumber",
        label: t("purchase.supplier.fields.vatNumber"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplier.validation.vatNumberRequired"))
          .max(15, t("purchase.supplier.validation.vatNumberMaxLength")),
      },
      {
        name: "supplierTypeId",
        label: t("purchase.supplier.fields.supplierType"),
        type: FormFieldType.Select,
        props: {
          options: supplierStore.supplierTypes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.supplier.validation.supplierTypeRequired"),
        ),
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
        validation: Yup.string().required(
          t("purchase.supplier.validation.addressRequired"),
        ),
      },
      {
        name: "city",
        label: t("location.city"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("purchase.supplier.validation.cityRequired"),
        ),
      },
      {
        name: "region",
        label: t("location.region"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("purchase.supplier.validation.regionRequired"),
        ),
      },
      {
        name: "postalCode",
        label: t("location.postalCode"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("purchase.supplier.validation.postalCodeRequired"),
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
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "phone",
        label: t("purchase.supplier.fields.phone"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplier.validation.phoneRequired")),
      },
      {
        name: "paymentMethodId",
        label: t("purchase.supplier.fields.paymentMethod"),
        type: FormFieldType.Select,
        props: {
          options: paymentMethodStore.paymentMethods ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.supplier.validation.paymentMethodRequired"),
        ),
      },
      {
        name: "accountNumber",
        label: t("purchase.supplier.fields.accountNumber"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplier.validation.accountNumberRequired"))
          .max(
            35,
            t("purchase.supplier.validation.accountNumberMaxLength"),
          ),
      },
    ],
  },
  {
    fields: [
      {
        name: "observations",
        label: t("purchase.supplier.fields.observations"),
        type: FormFieldType.Textarea,
      },
    ],
  },
  {
    fields: [
      {
        name: "notes",
        label: t("purchase.supplier.fields.purchaseOrderNotes"),
        type: FormFieldType.Textarea,
      },
    ],
  },
]);

onMounted(async () => {
  await paymentMethodStore.fetchAll();
});

const locationValues = (values: FormValues): LocationData => ({
  country: stringValue(values.country, ""),
  address: stringValue(values.address, ""),
  city: stringValue(values.city, ""),
  region: stringValue(values.region, ""),
  postalCode: stringValue(values.postalCode, ""),
  latitude: finiteNumberValue(values.latitude, props.supplier.latitude),
  longitude: finiteNumberValue(values.longitude, props.supplier.longitude),
  distanceFromSite: finiteNumberValue(
    values.distanceFromSite,
    props.supplier.distanceFromSite,
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
    ...props.supplier,
    comercialName: stringValue(values.comercialName, "").trim(),
    taxName: stringValue(values.taxName, "").trim(),
    vatNumber: stringValue(values.vatNumber, "").trim(),
    supplierTypeId: stringValue(values.supplierTypeId, ""),
    ...locationValues(values),
    phone: stringValue(values.phone, "").trim(),
    paymentMethodId: stringValue(values.paymentMethodId, ""),
    accountNumber: stringValue(values.accountNumber, "").trim(),
    observations: stringValue(values.observations, ""),
    notes: stringValue(values.notes, ""),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="supplier"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template
      #section-location="{ values, errors, setValues, disabled }"
    >
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
