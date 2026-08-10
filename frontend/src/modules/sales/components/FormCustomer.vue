<template>
  <form v-if="customer">
    <section class="three-columns">
      <BaseInput
        name="comercialName"
        :label="t('sales.components.nomComercial')"
        id="comercialName"
        v-model="customer.comercialName"
        :class="{
          'p-invalid': validation.errors.comercialName,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('sales.components.nomFiscal')"
        id="taxName"
        v-model="customer.taxName"
        :class="{
          'p-invalid': validation.errors.taxName,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t('sales.components.tipusClient') }}</label>
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
        label="CIF"
        id="vatNumber"
        v-model="customer.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
      <BaseInput
        name="web"
        :label="t('sales.components.web')"
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
        :label="t('sales.components.numeroDeCompte')"
        id="accountNumber"
        v-model="customer.accountNumber"
        :class="{
          'p-invalid': validation.errors.accountNumber,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t('sales.components.formaDePagament') }}</label>
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
    <div class="mb-2">
      <label class="block text-900 mb-2">{{ t('sales.components.observacions') }}</label>
      <Textarea v-model="customer.observations" class="w-full" />
    </div>
    <div class="mb-2">
      <label class="block text-900 mb-2">{{ t('sales.components.notesDeFactura') }}</label>
      <Textarea v-model="customer.invoiceNotes" class="w-full" />
    </div>
    <div class="mt-2 flex justify-content-end gap-2">
      <Button :label="t('sales.components.guardar')" @click="submitForm" />
      <Button :label="t('sales.components.cancelar')" severity="secondary" @click="emit('cancel')" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
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

const { t } = useI18n();
const emit = defineEmits<{
  (e: "submit", customer: Customer): void;
  (e: "cancel"): void;
}>();

const customerStore = useCustomersStore();
const sharedData = useSharedDataStore();
const { customer } = storeToRefs(customerStore);
const toast = useToast();

const schema = Yup.object().shape({
  comercialName: Yup.string()
    .required(t("sales.validation.commercialNameRequired"))
    .max(250, "El nom comercial no pot superar els 250 carácters"),
  taxName: Yup.string().required(t("sales.validation.taxNameRequired")),
  vatNumber: Yup.string().required(t("sales.validation.vatNumberRequired")),
  accountNumber: Yup.string().required(t("sales.validation.accountNumberRequired")),
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
      summary: t('sales.components.formulariInvalid'),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
