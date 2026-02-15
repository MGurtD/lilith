<template>
  <form v-if="invoiceImport">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        label="Import Base"
        v-model="invoiceImport.baseAmount"
        :type="BaseInputType.CURRENCY"
        :class="{
          'p-invalid': validation.errors.baseAmount,
        }"
        @update:modelValue="calcAmounts()"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">IVA</label>
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
        label="Import Impost"
        v-model="invoiceImport.taxAmount"
        :type="BaseInputType.CURRENCY"
        disabled
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Total"
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

const purchaseMasterData = usePurchaseMasterDataStore();
const toast = useToast();

const props = defineProps<{
  formAction: FormActionMode;
  invoiceImport: PurchaseInvoiceImport;
}>();
const emit = defineEmits<{
  (e: "submit", invoiceImport: PurchaseInvoiceImport): void;
}>();

const textActionButton = computed(() => {
  return props.formAction === FormActionMode.CREATE ? "Afegir" : "Modificar";
});

const calcAmounts = () => {
  const tax = purchaseMasterData.masterData.taxes!.find(
    (t) => t.id === props.invoiceImport.taxId,
  );

  if (tax && isNumber(props.invoiceImport.baseAmount)) {
    const baseAmount = props.invoiceImport.baseAmount;
    const taxAmount = (baseAmount / 100) * tax.percentatge;
    const netAmount = baseAmount + taxAmount;

    props.invoiceImport.baseAmount = baseAmount;
    props.invoiceImport.taxAmount = round(taxAmount, 2);
    props.invoiceImport.netAmount = round(netAmount, 2);
  }
};

const schema = Yup.object().shape({
  baseAmount: Yup.number().required("L'import base és obligatori"),
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
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>
