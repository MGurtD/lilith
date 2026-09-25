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
} from "@/components/forms/value-utils";
import { computed, useId } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { CreateRectificativeInvoiceRequest } from "../types";

const props = defineProps<{
  rectificativeInvoice: CreateRectificativeInvoiceRequest;
  maximumQuantity: number;
}>();

const emit = defineEmits<{
  (
    event: "submit",
    rectificativeInvoice: CreateRectificativeInvoiceRequest,
  ): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

// The section renders its own control, so its ID is generated here.
const quantityInputId = `rectificative-quantity-${useId()}`;

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "createCorrectionInvoice",
        label: t("sales.components.crearFacturaAmbImportCorregit"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
  {
    // Registered while hidden; validated only when a corrected invoice is
    // requested, capped at the invoice base amount.
    section: "quantity",
    fields: [
      {
        name: "quantity",
        label: t("sales.components.importAFacturarSenseIVA"),
        type: FormFieldType.Custom,
        validation: Yup.number()
          .nullable()
          .when("createCorrectionInvoice", {
            is: true,
            then: (schema) =>
              schema
                .required(t("sales.validation.quantityRequired"))
                .min(1, t("sales.validation.quantityGreaterThanOneRequired"))
                .max(
                  props.maximumQuantity,
                  t(
                    "sales.components.laQuantitatIntroduidaNoPotSerSuperiorALaQuantitatDeLaFactura",
                  ),
                ),
          }),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.rectificativeInvoice,
    createCorrectionInvoice: booleanValue(
      values.createCorrectionInvoice,
      props.rectificativeInvoice.createCorrectionInvoice,
    ),
    quantity: finiteNumberValue(
      values.quantity,
      props.rectificativeInvoice.quantity,
    ),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="rectificativeInvoice"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #section-quantity="{ values, errors, setFieldValue, disabled }">
      <div v-if="values.createCorrectionInvoice === true">
        <label class="block text-900 mb-2" :for="quantityInputId">
          {{ t("sales.components.importAFacturarSenseIVA") }}
        </label>
        <InputNumber
          :input-id="quantityInputId"
          class="w-full"
          locale="en-US"
          :min-fraction-digits="2"
          suffix=" €"
          :model-value="finiteNumberValue(values.quantity, null)"
          :disabled="disabled"
          :invalid="Boolean(errors.quantity)"
          @update:model-value="setFieldValue('quantity', $event)"
        />
        <small v-if="errors.quantity" class="p-error" role="alert">
          {{ errors.quantity }}
        </small>
      </div>
    </template>
  </Form>
</template>
