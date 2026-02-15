<template>
  <form v-if="newMovement">
    <div>
        <DropdownReference
          label="Material"
          :fullName="true"
          v-model="newMovement.referenceId"
          :class="{
            'p-invalid': validation.errors.referenceId,
          }"
        ></DropdownReference>
      </div>

    <section class="three-columns">      
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Quantitat"
          v-model="newMovement.newQuantity"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Amplada (mm)"
          :decimals="2"
          v-model="newMovement.width"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Alçada (mm)"
          v-model="newMovement.height"
        />
      </div>
    </section>

    <section class="three-columns">
     
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Longitud (mm)"
          v-model="newMovement.length"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Diàmetre (mm)"
          v-model="newMovement.diameter"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Gruix (mm)"
          v-model="newMovement.thickness"
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
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { ref } from "vue";
import { Inventory } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { useReferenceStore } from "../../shared/store/reference";

const props = defineProps<{
  newMovement: Inventory;
}>();

const emit = defineEmits<{
  (e: "submit", newMovement: Inventory): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const referenceStore = useReferenceStore();

const schema = Yup.object().shape({
  newQuantity: Yup.number()
    .min(1)
    .required("La quantitat ha de ser superior a 1"),
  referenceId: Yup.string().required("La referencia és obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.newMovement);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.newMovement);
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
