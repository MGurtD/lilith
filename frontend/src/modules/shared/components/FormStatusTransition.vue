<template>
  <form>
    <BaseInput
      class="mb-2"
      label="Nom"
      v-model="transition.name"
      :class="{
        'p-invalid': validation.errors.name,
      }"
    ></BaseInput>
    <section class="two-columns">
      <div>
        <label class="block text-900 mb-2">Origen</label>
        <Select
          v-model="transition.statusId"
          :options="statuses"
          optionValue="id"
          optionLabel="name"
        />
      </div>
      <div class="mb-4">
        <label class="block text-900 mb-2">Destí</label>
        <Select
          v-model="transition.statusToId"
          :options="statuses"
          optionValue="id"
          optionLabel="name"
        />
      </div>
    </section>

    <Button label="Confirmar" @click="submitForm" style="float: right" />
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { Status, StatusTransition } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";

const toast = useToast();

const props = defineProps<{
  formAction: FormActionMode;
  transition: StatusTransition;
  statuses: Array<Status>;
}>();
const emit = defineEmits<{
  (e: "submit", status: StatusTransition): void;
}>();

const schema = Yup.object().shape({
  name: Yup.string().required("El nom és obligatori"),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.transition);
};

const submitForm = async () => {
  if (props.transition.statusId === props.transition.statusToId) {
    toast.add({
      severity: "warn",
      summary: "Formulari inválid",
      detail: "Els estats d'origen i destí han de ser diferents",
      life: 5000,
    });
    return;
  }

  validate();
  if (validation.value.result) {
    emit("submit", props.transition);
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
