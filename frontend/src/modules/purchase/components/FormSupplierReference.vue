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
import DropdownReference from "@/modules/shared/components/DropdownReference.vue";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { SupplierReference } from "../types";
import DropdownSupplier from "./DropdownSupplier.vue";

const props = defineProps<{
  referenceId?: string;
  supplierId?: string;
  referenceSupplier: SupplierReference;
}>();

const emit = defineEmits<{
  (event: "submit", reference: SupplierReference): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      ...(!props.referenceId
        ? [
            {
              name: "referenceId",
              label: t("purchase.supplierReference.fields.reference"),
              type: FormFieldType.Custom as const,
            },
          ]
        : []),
      ...(!props.supplierId
        ? [
            {
              name: "supplierId",
              label: t("purchase.supplierReference.fields.supplier"),
              type: FormFieldType.Custom as const,
            },
          ]
        : []),
      {
        name: "supplierCode",
        label: t("purchase.supplierReference.fields.supplierCode"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(
            t(
              "purchase.supplierReference.validation.supplierCodeRequired",
            ),
          ),
      },
      {
        name: "supplierDescription",
        label: t("purchase.supplierReference.fields.supplierDescription"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "supplierPrice",
        label: t("purchase.supplierReference.fields.supplierPrice"),
        type: FormFieldType.Currency,
        props: {
          currency: "EUR",
          locale: "en-US",
          minFractionDigits: 2,
        },
        validation: Yup.number()
          .typeError(
            t(
              "purchase.supplierReference.validation.supplierPriceRequired",
            ),
          )
          .required(
            t(
              "purchase.supplierReference.validation.supplierPriceRequired",
            ),
          ),
      },
      {
        name: "supplyDays",
        label: t("purchase.supplierReference.fields.supplyDays"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
        validation: Yup.number()
          .typeError(
            t("purchase.supplierReference.validation.supplyDaysRequired"),
          )
          .required(
            t("purchase.supplierReference.validation.supplyDaysRequired"),
          ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.referenceSupplier,
    referenceId: stringValue(values.referenceId, props.referenceId ?? ""),
    supplierId: stringValue(values.supplierId, props.supplierId ?? ""),
    supplierCode: stringValue(values.supplierCode, "").trim(),
    supplierDescription: stringValue(values.supplierDescription, "").trim(),
    supplierPrice: finiteNumberValue(
      values.supplierPrice,
      props.referenceSupplier.supplierPrice,
    ),
    supplyDays: finiteNumberValue(
      values.supplyDays,
      props.referenceSupplier.supplyDays,
    ),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="referenceSupplier"
    :show-cancel="false"
    @submit="submit"
  >
    <template #field-referenceId="{ value, setValue, disabled }">
      <DropdownReference
        label=""
        :model-value="typeof value === 'string' ? value : undefined"
        :full-name="true"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #field-supplierId="{ value, setValue, disabled }">
      <DropdownSupplier
        label=""
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>
