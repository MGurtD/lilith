<template>
  <form v-if="supplier">
    <section class="four-columns mb-2">
      <BaseInput
        name="comercialName"
        label="Nom Comercial"
        id="comercialName"
        v-model="supplier.comercialName"
        :class="{
          'p-invalid': validation.errors.comercialName,
        }"
      ></BaseInput>
      <BaseInput
        label="Nom Fiscal"
        id="taxName"
        v-model="supplier.taxName"
        :class="{
          'p-invalid': validation.errors.taxName,
        }"
      ></BaseInput>
      <BaseInput
        label="CIF"
        id="vatNumber"
        v-model="supplier.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Tipus Proveïdor</label>
        <Select
          v-model="supplier.supplierTypeId"
          :options="supplierStore.supplierTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.supplierTypeId,
          }"
        />
      </div>
    </section>

    <LocationFields
      :model-value="supplier"
      :show-distance="true"
      :validation-errors="validation.errors"
    />

    <section class="three-columns mb-2">
      <BaseInput
        label="Telèfon"
        id="phone"
        v-model="supplier.phone"
        :class="{
          'p-invalid': validation.errors.phone,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Forma de pagament</label>
        <Select
          v-model="supplier.paymentMethodId"
          :options="paymentMethodStore.paymentMethods"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.paymentMethodId,
          }"
        />
      </div>
      <BaseInput
        label="Número de compte"
        id="accountNumber"
        v-model="supplier.accountNumber"
        :class="{
          'p-invalid': validation.errors.accountNumber,
        }"
      ></BaseInput>
    </section>

    <div class="mt-2">
      <label class="block text-900 mb-2">Observacions</label>
      <Textarea v-model="supplier.observations" class="w-full" />
    </div>

    <div class="mt-2">
      <label class="block text-900 mb-2">Notes</label>
      <Textarea v-model="supplier.notes" class="w-full" />
    </div>

    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import LocationFields from "@/components/LocationFields.vue";
import { useSuppliersStore } from "../store/suppliers";
import { Supplier } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { usePaymentMethodStore } from "../../shared/store/paymentMethod";

const props = defineProps<{
  supplier: Supplier;
}>();

const emit = defineEmits<{
  (e: "submit", supplier: Supplier): void;
}>();

const supplierStore = useSuppliersStore();
const paymentMethodStore = usePaymentMethodStore();
const toast = useToast();

onMounted(async () => {
  await paymentMethodStore.fetchAll();
});

const schema = Yup.object().shape({
  comercialName: Yup.string()
    .required("El nom comercial és obligatori")
    .max(250, "El nom comercial no pot superar els 250 caràcters"),
  vatNumber: Yup.string()
    .required("El CIF és obligatori")
    .max(15, "El CIF no pot superar els 15 caràcters"),
  taxName: Yup.string().required("El nom fiscal és obligatori"),
  region: Yup.string().required("La província és obligatòria"),
  city: Yup.string().required("El municipi és obligatori"),
  postalCode: Yup.string().required("El codi postal és obligatori"),
  address: Yup.string().required("La direcció és obligatòria"),
  phone: Yup.string().required("El telèfon és obligatori"),
  accountNumber: Yup.string()
    .required("El número de compte és obligatori")
    .max(35, "El número de compte no pot superar el 35 dígits"),
  supplierTypeId: Yup.string().required("El tipus de proveïdor és obligatori"),
  paymentMethodId: Yup.string().required("La forma de pagament és obligatòria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.supplier);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.supplier);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: "Formulari invàlid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>

