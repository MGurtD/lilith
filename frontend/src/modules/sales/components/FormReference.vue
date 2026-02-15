<template>
  <div>
    <Button
      label="Guardar"
      class="grid_add_row_button"
      size="small"
      @click="submitForm"
    />
    <br />
  </div>

  <form v-if="reference">
    <section class="four-columns">
      <div class="mt-1">
        <BaseInput
          class="mb-2"
          label="Codi"
          id="code"
          v-model="reference.code"
          :class="{
            'p-invalid': validation.errors.code,
          }"
        ></BaseInput>
      </div>
      <div class="mt-1">
        <BaseInput
          class="mb-2"
          label="Descripció"
          id="description"
          v-model="reference.description"
          :class="{
            'p-invalid': validation.errors.description,
          }"
        ></BaseInput>
      </div>
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.TEXT"
          label="Versió"
          id="version"
          v-model="reference.version"
        />
      </div>
      <div class="mt-1">
        <DropdownCustomers label="Client" v-model="reference.customerId" />
      </div>
    </section>
    <section class="five-columns">
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Cost Teóric Fabricació"
          id="workMasterCost"
          v-model="reference.workMasterCost"
          disabled
        />
      </div>
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Cost Última Fabricació"
          id="lastCost"
          v-model="reference.lastCost"
          disabled
        />
      </div>
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Preu unitari"
          id="price"
          v-model="reference.price"
        />
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">Impost</label>
        <Select
          v-model="reference.taxId"
          :options="taxesStore.taxes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.taxid,
          }"
        />
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">Servei</label>
        <Checkbox v-model="reference.isService" class="w-full" :binary="true" />
      </div>
    </section>
  </form>
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Reference } from "../../../modules/shared/types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import { useToast } from "primevue/usetoast";
import { BaseInputType } from "../../../types/component";
import { useTaxesStore } from "../../shared/store/tax";

const props = defineProps<{
  reference: Reference;
  defaultCustomerId?: string;
}>();

const emit = defineEmits<{
  (e: "submit", reference: Reference): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const taxesStore = useTaxesStore();

onMounted(() => {
  // Set customer when optional property is set
  if (props.defaultCustomerId && props.reference) {
    props.reference.customerId = props.defaultCustomerId!;
  }
});

const schema = Yup.object().shape({
  code: Yup.string()
    .required("El codi és obligatori")
    .max(50, "El codi no pot superar els 50 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció pot superar els 250 carácters"),
  version: Yup.string()
    .required("La versió és obligatoria")
    .max(20, "La versió pot superar els 20 carácters"),
  cost: Yup.number().required("El cost es obligatori"),
  price: Yup.number().required("El preu es obligatori"),
  taxId: Yup.string().required("El tipus d'iva es obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.reference);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.reference);
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
