<template>
  <form v-if="invoice">
    <section class="four-columns">
      <div class="mt-2">
        <BaseInput
          v-model="invoice.invoiceNumber"
          label="Número"
          :disabled="true"
        />
      </div>
      <div class="mt-2">
        <label class="block text-900 mb-2">Data Factura</label>
        <DatePicker v-model="invoice.invoiceDate" dateFormat="dd/mm/yy" />
      </div>
      <div class="mt-2">
        <DropdownLifecycleStatusTransitions
          label="Estat"
          :statusId="invoice.statusId"
          v-model="invoice.statusId"
        />
      </div>
      <div class="mt-2">
        <label class="block text-900 mb-2">Métode Pagament</label>
        <Select
          v-model="invoice.paymentMethodId"
          :options="sharedData.paymentMethods"
          optionValue="id"
          optionLabel="name"
          class="w-full"
        />
      </div>
    </section>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { SalesInvoice } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useSharedDataStore } from "../../shared/store/masterData";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";

const props = defineProps<{
  invoice: SalesInvoice;
}>();

const emit = defineEmits<{
  (e: "submit", invoice: SalesInvoice): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const sharedData = useSharedDataStore();

const schema = Yup.object().shape({
  invoiceDate: Yup.string().required("La data és obligatoria"),
  paymentMethodId: Yup.string().required("El métode de pagament és obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.invoice);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.invoice);
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
