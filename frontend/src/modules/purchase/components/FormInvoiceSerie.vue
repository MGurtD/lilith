<template>
  <form v-if="purchaseInvoiceSerie">
    <section class="three-columns">
      <BaseInput
        name="name"
        class="mb-2"
        :label="t('purchase.invoiceSeries.fields.name')"
        id="name"
        v-model="purchaseInvoiceSerie.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        name="description"
        class="mb-2"
        :label="t('purchase.invoiceSeries.fields.description')"
        id="description"
        v-model="purchaseInvoiceSerie.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.invoiceSeries.fields.disabled") }}</label>
        <Checkbox
          v-model="purchaseInvoiceSerie.disabled"
          class="w-full"
          :binary="true"
        />
      </div>
    </section>
    <section class="four-columns">
      <BaseInput
        name="prefix"
        class="mb-2"
        :label="t('purchase.invoiceSeries.fields.prefix')"
        id="prefix"
        v-model="purchaseInvoiceSerie.prefix"
        :class="{
          'p-invalid': validation.errors.prefix,
        }"
      ></BaseInput>
      <BaseInput
        name="suffix"
        class="mb-2"
        :label="t('purchase.invoiceSeries.fields.suffix')"
        id="suffix"
        v-model="purchaseInvoiceSerie.suffix"
        :class="{
          'p-invalid': validation.errors.suffix,
        }"
      ></BaseInput>
      <BaseInput
        name="nextNumber"
        class="mb-2"
        :label="t('purchase.invoiceSeries.fields.nextNumber')"
        id="nextNumber"
        :type="BaseInputType.NUMERIC"
        v-model.number="purchaseInvoiceSerie.nextNumber"
        :class="{
          'p-invalid': validation.errors.nextNumber,
        }"
      ></BaseInput>
      <BaseInput
        name="length"
        class="mb-2"
        :label="t('purchase.invoiceSeries.fields.length')"
        id="length"
        v-model.number="purchaseInvoiceSerie.length"
        :class="{
          'p-invalid': validation.errors.length,
        }"
      ></BaseInput>
    </section>
    <div class="mt-2">
      <Button :label="t('purchase.actions.save')" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>
<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { InvoiceSerie } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { BaseInputType } from "../../../types/component";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  purchaseInvoiceSerie: InvoiceSerie;
}>();

const emit = defineEmits<{
  (e: "submit", purchaseInvoiceSerie: InvoiceSerie): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const { t } = useI18n();

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("purchase.invoiceSeries.validation.nameRequired"))
    .max(50, t("purchase.invoiceSeries.validation.nameMaxLength")),
  description: Yup.string()
    .required(t("purchase.invoiceSeries.validation.descriptionRequired"))
    .max(250, t("purchase.invoiceSeries.validation.descriptionMaxLength")),
  prefix: Yup.string().max(10, t("purchase.invoiceSeries.validation.prefixMaxLength")),
  suffix: Yup.string().max(10, t("purchase.invoiceSeries.validation.suffixMaxLength")),
  nextNumber: Yup.number()
    .positive(t("purchase.invoiceSeries.validation.nextNumberPositive"))
    .integer(t("purchase.invoiceSeries.validation.nextNumberInteger"))
    .required(t("purchase.invoiceSeries.validation.nextNumberRequired")),
  length: Yup.number()
    .positive(t("purchase.invoiceSeries.validation.lengthPositive"))
    .integer(t("purchase.invoiceSeries.validation.lengthInteger"))
    .min(1, t("purchase.invoiceSeries.validation.lengthMinimum"))
    .max(20, t("purchase.invoiceSeries.validation.lengthMaxLength"))
    .required(t("purchase.invoiceSeries.validation.lengthRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.purchaseInvoiceSerie);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.purchaseInvoiceSerie);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("purchase.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
