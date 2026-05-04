<template>
  <form v-if="customer">
    <section class="three-columns">
      <BaseInput
        name="comercialName"
        class="mb-2"
        label="Nom Comercial"
        id="comercialName"
        v-model="customer.comercialName"
        :class="{
          'p-invalid': validation.errors.comercialName,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Nom Fiscal"
        id="taxName"
        v-model="customer.taxName"
        :class="{
          'p-invalid': validation.errors.taxName,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Tipus Client</label>
        <Select
          v-model="customer.customerTypeId"
          :options="customerStore.customerTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.supplierTypeId,
          }"
        />
      </div>
    </section>

    <section class="three-columns mb-2">
      <BaseInput
        name="vatNumber"
        class="mb-2"
        label="CIF"
        id="vatNumber"
        v-model="customer.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
      <BaseInput
        name="web"
        class="mb-2"
        label="Web"
        id="web"
        v-model="customer.web"
        :class="{
          'p-invalid': validation.errors.web,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{
          $t("forms.user.languageLabel")
        }}</label>
        <LanguageSwitcher
          v-model="customer.preferredLanguage"
          :changeAppLanguage="false"
        />
      </div>
    </section>
    <section class="three-columns mb-2">
      <BaseInput
        name="accountNumber"
        class="mb-2"
        label="Número de compte"
        id="accountNumber"
        v-model="customer.accountNumber"
        :class="{
          'p-invalid': validation.errors.accountNumber,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Forma de pagament</label>
        <Select
          v-model="customer.paymentMethodId"
          :options="sharedData.paymentMethods"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.paymentMethodId,
          }"
        />
      </div>
    </section>
    <div>
      <label class="block text-900 mb-2">Observacions</label>
      <Textarea v-model="customer.observations" class="w-full" />
    </div>
    <div>
      <label class="block text-900 mb-2">Notes de factura</label>
      <Textarea v-model="customer.invoiceNotes" class="w-full" />
    </div>
    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useCustomersStore } from "../store/customers";
import { storeToRefs } from "pinia";
import { Customer } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useSharedDataStore } from "../../../modules/shared/store/masterData";
import LanguageSwitcher from "../../../components/LanguageSwitcher.vue";

const emit = defineEmits<{
  (e: "submit", customer: Customer): void;
}>();

const customerStore = useCustomersStore();
const sharedData = useSharedDataStore();
const { customer } = storeToRefs(customerStore);
const toast = useToast();

const schema = Yup.object().shape({
  comercialName: Yup.string()
    .required("El nom comercial és obligatori")
    .max(250, "El nom comercial no pot superar els 250 carácters"),
  taxName: Yup.string().required("El nom fiscal és obligatori"),
  vatNumber: Yup.string().required("El CIF és obligatori"),
  accountNumber: Yup.string().required("El número de compte es obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(customer.value);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", customer.value as Customer);
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
