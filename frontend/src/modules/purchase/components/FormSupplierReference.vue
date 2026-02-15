<template>
  <form v-if="referenceSupplier">
    <section class="three-columns pt-5">
      <DropdownReference
        v-if="!referenceId"
        label="Referència"
        v-model="referenceSupplier.referenceId"
        :fullName="true"
      ></DropdownReference>
      <DropdownSupplier
        v-if="!supplierId"
        label="Proveïdor"
        v-model="referenceSupplier.supplierId"
      />
      <BaseInput
        label="Codi proveïdor"
        id="supplierCode"
        v-model="referenceSupplier.supplierCode"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.supplierCode,
        }"
      ></BaseInput>
      <BaseInput
        label="Descripció proveïdor"
        id="supplierDescription"
        v-model="referenceSupplier.supplierDescription"
        class="mb-2"
      ></BaseInput>
    </section>
    <section class="three-columns">
      <BaseInput
        label="Preu proveïdor"
        id="supplierPrice"
        v-model="referenceSupplier.supplierPrice"
        :type="BaseInputType.CURRENCY"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.supplierPrice,
        }"
      ></BaseInput>
      <BaseInput
        label="Dies de subministrament"
        id="supplyDays"
        v-model="referenceSupplier.supplyDays"
        :type="BaseInputType.NUMERIC"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.supplyDays,
        }"
      ></BaseInput>
    </section>
    <div class="mt-2">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import BaseInput from "../../../components/BaseInput.vue";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import DropdownReference from "../../../modules/shared/components/DropdownReference.vue";
import { ref } from "vue";
import { Supplier, SupplierReference } from "../types";
import * as Yup from "yup";
import { BaseInputType } from "../../../types/component";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  referenceId?: string;
  supplierId?: string;
  referenceSupplier: SupplierReference;
}>();

const emit = defineEmits<{
  (e: "submit", reference: SupplierReference): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  supplierCode: Yup.string().required("El codi és obligatori"),
  supplierPrice: Yup.number().required("El preu és obligatori"),
  supplyDays: Yup.number().required(
    "Els dies de subministrament són obligatoris"
  ),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.referenceSupplier);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.referenceSupplier);
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
