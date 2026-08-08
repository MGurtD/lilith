<template>
  <form v-if="invoiceImport">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        :label="t('purchase.purchaseInvoiceImport.fields.baseAmount')"
        v-model="invoiceImport.baseAmount"
        :type="BaseInputType.CURRENCY"
        :class="{
          'p-invalid': validation.errors.baseAmount,
        }"
        @update:modelValue="calcAmounts()"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.purchaseInvoiceImport.fields.tax") }}</label>
        <Select
          v-model="invoiceImport.taxId"
          :options="purchaseMasterData.masterData.taxes"
          optionValue="id"
          optionLabel="name"
          @update:modelValue="calcAmounts()"
        />
      </div>
    </section>
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        :label="t('purchase.purchaseInvoiceImport.fields.taxAmount')"
        v-model="invoiceImport.taxAmount"
        :type="BaseInputType.CURRENCY"
        disabled
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('common.total')"
        v-model="invoiceImport.netAmount"
        :type="BaseInputType.CURRENCY"
        disabled
      ></BaseInput>
    </section>

    <Button
      :label="textActionButton"
      @click="submitForm"
      style="float: right"
    />
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { PurchaseInvoiceImport } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { BaseInputType, FormActionMode } from "../../../types/component";
import { isNumber, round } from "lodash";
import { useI18n } from "vue-i18n";

const purchaseMasterData = usePurchaseMasterDataStore();
const toast = useToast();
const { t } = useI18n();

const props = defineProps<{
  formAction: FormActionMode;
  invoiceImport: PurchaseInvoiceImport;
}>();
const emit = defineEmits<{
  (e: "submit", invoiceImport: PurchaseInvoiceImport): void;
}>();

const textActionButton = computed(() => {
  return props.formAction === FormActionMode.CREATE
    ? t("purchase.purchaseInvoiceImport.actions.add")
    : t("purchase.purchaseInvoiceImport.actions.update");
});

const calcAmounts = () => {
  const tax = purchaseMasterData.masterData.taxes!.find(
    (t) => t.id === props.invoiceImport.taxId,
  );

  if (tax && isNumber(props.invoiceImport.baseAmount)) {
    const baseAmount = props.invoiceImport.baseAmount;
    const taxAmount = tax.isReverseCharge ? 0 : (baseAmount / 100) * tax.percentatge;
    const netAmount = baseAmount + taxAmount;

    props.invoiceImport.baseAmount = baseAmount;
    props.invoiceImport.taxAmount = round(taxAmount, 2);
    props.invoiceImport.netAmount = round(netAmount, 2);
  }
};

const schema = Yup.object().shape({
  baseAmount: Yup.number().required(
    t("purchase.purchaseInvoiceImport.validation.baseAmountRequired"),
  ),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.invoiceImport);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.invoiceImport);
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
