<template>
  <form v-if="invoiceDetail">
    <section class="two-columns-7525">
      <div class="mt-2">
        <BaseInput label="Descripció" v-model="invoiceDetail.description" />
      </div>
      <div class="mt-2">
        <label class="block text-900 mb-2">Impost</label>
        <Select
          v-model="invoiceDetail.taxId"
          :options="sharedData.taxes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
        />
      </div>
    </section>

    <section class="three-columns">
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Quantitat"
          v-model="invoiceDetail.quantity"
          @update:model-value="calcAmount"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Preu Unitat"
          v-model:model-value="invoiceDetail.unitPrice"
          @update:model-value="calcAmount"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Total"
          v-model:model-value="invoiceDetail.amount"
          disabled
        />
      </div>
    </section>

    <Button
      label="Crear"
      @click="submitForm"
      style="float: right"
      :size="'small'"
      class="mt-2"
    />
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { SalesInvoiceDetail } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useSharedDataStore } from "../../shared/store/masterData";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { round } from "lodash";

const props = defineProps<{
  invoiceDetail: SalesInvoiceDetail;
}>();

const emit = defineEmits<{
  (e: "submit", invoiceDetail: SalesInvoiceDetail): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const sharedData = useSharedDataStore();

const calcAmount = () => {
  const detail = props.invoiceDetail;

  if (detail.quantity && detail.quantity > 0 && detail.unitPrice) {
    props.invoiceDetail.unitCost = detail.unitPrice;
    props.invoiceDetail.amount = round(detail.quantity * detail.unitPrice, 2);
    props.invoiceDetail.totalCost = props.invoiceDetail.amount;
  }
};

const schema = Yup.object().shape({
  quantity: Yup.number().min(1).required("La quantitat ha de ser superior a 1"),
  unitPrice: Yup.number().required("El preu unitat és obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.invoiceDetail);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.invoiceDetail);
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
