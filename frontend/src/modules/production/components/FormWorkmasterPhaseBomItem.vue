<template>
  <form v-if="bomItem">
    <div>
        <DropdownReference
          label="Material"
          :fullName="true"
          v-model="bomItem.referenceId"
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
          v-model="bomItem.quantity"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Amplada (mm)"
          :decimals="2"
          v-model="bomItem.width"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Alçada (mm)"
          v-model="bomItem.height"
        />
      </div>
    </section>

    <section class="three-columns">
     
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Longitud (mm)"
          v-model="bomItem.length"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Diàmetre (mm)"
          v-model="bomItem.diameter"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Gruix (mm)"
          v-model="bomItem.thickness"
        />
      </div>
    </section>

    <br />
    <div>
      <Button
        label="Guardar material"
        style="float: right"
        size="small"
        @click="submitForm"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { ref } from "vue";
import { WorkMasterPhaseBillOfMaterials } from "../types";
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
  bomItem: WorkMasterPhaseBillOfMaterials;
}>();

const emit = defineEmits<{
  (e: "submit", phase: WorkMasterPhaseBillOfMaterials): void;
  (e: "cancel"): void;
}>();

const referenceStore = useReferenceStore();

const toast = useToast();
const schema = Yup.object().shape({
  referenceId: Yup.string().required("El material de consum és obligatori"),
  quantity: Yup.number()
    .min(1, "La quantitat a consumir ha de ser positiva")
    .required("La quantitat a consumir és obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.bomItem);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.bomItem);
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
