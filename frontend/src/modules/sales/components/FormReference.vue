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
  nullableStringValue,
  optionalStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import DropdownReferenceType from "@/modules/shared/components/DropdownReferenceType.vue";
import { useTaxesStore } from "@/modules/shared/store/tax";
import type { Reference } from "@/modules/shared/types";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownCustomers from "./DropdownCustomers.vue";

const props = defineProps<{
  reference: Reference;
}>();

const emit = defineEmits<{
  (event: "submit", reference: Reference): void;
}>();

const { t } = useI18n();
const taxesStore = useTaxesStore();

const currencyProps = {
  locale: "en-US",
  minFractionDigits: 2,
  suffix: " €",
} as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, tablet: 2, desktop: 5 },
    fields: [
      {
        name: "code",
        label: t("sales.components.codi"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.codeRequired"))
          .max(50, t("sales.validation.codeMaxLength")),
      },
      {
        name: "description",
        label: t("sales.components.descripcio"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.descriptionRequired"))
          .max(250, t("sales.validation.descriptionMaxLength")),
      },
      {
        name: "referenceTypeId",
        label: t("sales.components.tipusDeMaterial"),
        type: FormFieldType.Custom,
      },
      {
        name: "version",
        label: t("sales.components.versio"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.versionRequired"))
          .max(20, t("sales.validation.versionMaxLength")),
      },
      {
        name: "customerId",
        label: t("sales.components.client"),
        type: FormFieldType.Custom,
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 5 },
    fields: [
      {
        name: "workMasterCost",
        label: t("sales.components.costTeoricFabricacio"),
        type: FormFieldType.Number,
        disabled: true,
        props: currencyProps,
      },
      {
        name: "lastCost",
        label: t("sales.components.costUltimaFabricacio"),
        type: FormFieldType.Number,
        disabled: true,
        props: currencyProps,
      },
      {
        name: "price",
        label: t("sales.components.preuUnitari"),
        type: FormFieldType.Number,
        props: currencyProps,
        validation: Yup.number()
          .typeError(t("sales.validation.priceRequired"))
          .required(t("sales.validation.priceRequired")),
      },
      {
        name: "taxId",
        label: t("sales.components.impost"),
        type: FormFieldType.Select,
        props: {
          options: taxesStore.taxes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(t("sales.validation.taxRequired")),
      },
      {
        name: "isService",
        label: t("sales.components.servei"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.reference,
    code: stringValue(values.code, ""),
    description: stringValue(values.description, ""),
    referenceTypeId: nullableStringValue(
      values.referenceTypeId,
      props.reference.referenceTypeId,
    ),
    version: stringValue(values.version, ""),
    customerId: nullableStringValue(
      values.customerId,
      props.reference.customerId,
    ),
    price: finiteNumberValue(values.price, props.reference.price),
    taxId: optionalStringValue(values.taxId, props.reference.taxId),
    isService: booleanValue(values.isService, false),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="reference"
    @submit="submit"
  >
    <template
      #field-referenceTypeId="{ value, setValue, disabled, inputId }"
    >
      <DropdownReferenceType
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
    <template #field-customerId="{ value, setValue, disabled, inputId }">
      <DropdownCustomers
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>
