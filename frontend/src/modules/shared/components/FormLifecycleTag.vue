<template>
  <form>
    <section class="two-columns">
      <BaseInput
        class="mb-3"
        label="Nom"
        v-model="tag.name"
        :class="{ 'p-invalid': validation.errors.name }"
      />

      <BaseInput
        class="mb-3"
        label="Descripció"
        v-model="tag.description"
        :class="{ 'p-invalid': validation.errors.description }"
      />

      <div class="mb-3">
        <label class="block text-900 mb-2">Color</label>
        <Select
          v-model="tag.color"
          :options="colors"
          optionValue="id"
          optionLabel="value"
          class="w-full"
        />
      </div>

      <div class="mb-3">
        <label class="block text-900 mb-2">Icona</label>
        <IconPicker v-model="tag.icon" />
      </div>
    </section>

    <div class="flex justify-content-end gap-2 mt-4">
      <Button
        label="Cancel·lar"
        severity="secondary"
        @click="$emit('cancel')"
      />
      <Button label="Confirmar" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import IconPicker from "../../../components/IconPicker.vue";
import { LifecycleTag } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";

const toast = useToast();

const props = defineProps<{
  tag: LifecycleTag;
  formAction: FormActionMode;
}>();

const emit = defineEmits<{
  (e: "submit", tag: LifecycleTag): void;
  (e: "cancel"): void;
}>();

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 caràcters"),
  description: Yup.string().max(
    250,
    "La descripció no pot superar els 250 caràcters",
  ),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.tag);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.tag);
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

type Color = {
  id: string;
  value: string;
};

const colors: Color[] = [
  { id: "", value: "Cap" },
  { id: "info", value: "Blau" },
  { id: "secondary", value: "Gris" },
  { id: "help", value: "Lila" },
  { id: "contrast", value: "Negre" },
  { id: "warn", value: "Taronja" },
  { id: "success", value: "Verd" },
  { id: "danger", value: "Vermell" },
];
</script>
