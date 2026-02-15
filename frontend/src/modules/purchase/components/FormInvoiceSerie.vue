<template>
  <form v-if="purchaseInvoiceSerie">
    <section class="three-columns">
      <BaseInput
        name="name"
        class="mb-2"
        label="Nom de la sèrie"
        id="name"
        v-model="purchaseInvoiceSerie.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        name="description"
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="purchaseInvoiceSerie.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">Desactivada</label>
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
        label="Prefix"
        id="prefix"
        v-model="purchaseInvoiceSerie.prefix"
        :class="{
          'p-invalid': validation.errors.prefix,
        }"
      ></BaseInput>
      <BaseInput
        name="suffix"
        class="mb-2"
        label="Sufix"
        id="suffix"
        v-model="purchaseInvoiceSerie.suffix"
        :class="{
          'p-invalid': validation.errors.suffix,
        }"
      ></BaseInput>
      <BaseInput
        name="nextNumber"
        class="mb-2"
        label="Següent número"
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
        label="Longitud"
        id="length"
        v-model.number="purchaseInvoiceSerie.length"
        :class="{
          'p-invalid': validation.errors.length,
        }"
      ></BaseInput>
    </section>
    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
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

const props = defineProps<{
  purchaseInvoiceSerie: InvoiceSerie;
}>();

const emit = defineEmits<{
  (e: "submit", purchaseInvoiceSerie: InvoiceSerie): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom de la sèrie és obligatori")
    .max(50, "El nom de la sèrie no pot superar els 50 caràcters"),
  description: Yup.string()
    .required("La descripció de la sèrie és obligatòria")
    .max(250, "La descripció de la sèrie no pot superar els 50 caràcters"),
  prefix: Yup.string().max(10, "El prefix no pot superar els 10 caràcters"),
  suffix: Yup.string().max(10, "El sufix no pot superar els 10 caràcters"),
  nextNumber: Yup.number()
    .positive("El següent número ha de ser positiu")
    .integer("El següent número ha de ser un número enter")
    .required("El següent número és obligatori"),
  length: Yup.number()
    .positive("La longitud ha de ser positiva")
    .integer("La longitud ha de ser un número enter")
    .min(1, "La longitud ha de ser almenys 1")
    .max(20, "La longitud no pot superar els 20 caràcters")
    .required("La longitud és obligatòria"),
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
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>
