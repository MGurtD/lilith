<template>
  <form v-if="phaseTemplate">
    <div class="grid_add_row_button">
      <Button label="Guardar" size="small" @click="submitForm" />
      <br />
    </div>
    <section class="three-columns">
      <div>
        <BaseInput
          label="Nom"
          v-model="phaseTemplate.name"
          :class="{ 'p-invalid': validation.errors.name }"
        />
      </div>
      <div>
        <BaseInput
          label="Descripció"
          v-model="phaseTemplate.description"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="phaseTemplate.disabled" class="w-full" :binary="true" />
      </div>
    </section>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { PhaseTemplate } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";

const props = defineProps<{
  phaseTemplate: PhaseTemplate;
}>();

const emit = defineEmits<{
  (e: "submit", phaseTemplate: PhaseTemplate): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string().required("El nom és obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.phaseTemplate);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.phaseTemplate);
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
