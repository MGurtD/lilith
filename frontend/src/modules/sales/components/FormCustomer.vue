<template>
  <form v-if="customer">
    <section class="three-columns">
      <BaseInput
        name="comercialName"
        label="Nom Comercial"
        id="comercialName"
        v-model="customer.comercialName"
        :class="{
          'p-invalid': validation.errors.comercialName,
        }"
      ></BaseInput>
      <BaseInput
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
        label="CIF"
        id="vatNumber"
        v-model="customer.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
      <BaseInput
        name="web"
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
    <div class="mb-2">
      <label class="block text-900 mb-2">Observacions</label>
      <Textarea v-model="customer.observations" class="w-full" />
    </div>
    <div class="mb-2">
      <label class="block text-900 mb-2">Notes de factura</label>
      <Textarea v-model="customer.invoiceNotes" class="w-full" />
    </div>
    <template v-if="showFiscalAddress && fiscalAddress">
      <h4 class="mt-4 mb-2">Adreça fiscal</h4>
      <section class="three-columns mb-2">
        <BaseInput
          id="fiscalAddressName"
          label="Nom"
          v-model="fiscalAddress.name"
          :class="{
            'p-invalid': addressValidation.errors.name,
          }"
        ></BaseInput>
      </section>
      <LocationFields
        :model-value="fiscalAddress"
        :validation-errors="addressValidation.errors"
      />
    </template>
    <div class="mt-2 flex justify-content-end gap-2">
      <Button label="Guardar" @click="submitForm" />
      <Button label="Cancelar" severity="secondary" @click="emit('cancel')" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
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
import LocationFields from "@/components/LocationFields.vue";

// En l'alta el backend exigeix l'adreça fiscal principal, i la pestanya
// d'adreces només apareix quan el client ja existeix
const props = withDefaults(defineProps<{ showFiscalAddress?: boolean }>(), {
  showFiscalAddress: false,
});

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

const fiscalAddress = computed(() => customer.value?.address?.[0]);

const addressSchema = Yup.object().shape({
  name: Yup.string()
    .required("El nom de l'adreça fiscal és obligatori")
    .max(250, "El nom de l'adreça fiscal no pot superar els 250 caràcters"),
  country: Yup.string().required("El país és obligatori"),
  city: Yup.string().required("El municipi és obligatori"),
  postalCode: Yup.string().required("El codi postal és obligatori"),
  address: Yup.string().required("La direcció és obligatòria"),
});
const addressValidation = ref({
  result: true,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(customer.value);

  if (props.showFiscalAddress) {
    const addressFormValidation = new FormValidation(addressSchema);
    addressValidation.value = addressFormValidation.validate(
      fiscalAddress.value ?? {},
    );
  }
};

const submitForm = async () => {
  validate();
  if (validation.value.result && addressValidation.value.result) {
    emit("submit", customer.value as Customer);
  } else {
    let errors = "";
    Object.entries({
      ...validation.value.errors,
      ...addressValidation.value.errors,
    }).forEach((e) => {
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
