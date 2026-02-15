<template>
  <form v-if="reason">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        label="Codi"
        id="code"
        v-model="reason.code"
        :class="{ 'p-invalid': validation.errors.code }"
      />
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="reason.name"
        :class="{ 'p-invalid': validation.errors.name }"
      />
    </section>
    <section class="one-column">
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="reason.description"
        :class="{ 'p-invalid': validation.errors.description }"
      />
    </section>
    <section class="two-columns">
      <div>
        <label class="block text-900 mb-2">Color</label>
        <ColorPicker
          v-model="reason.color"
          class="mb-2"
          :class="{ 'p-invalid': validation.errors.color }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Icona</label>
        <IconPicker v-model="reason.icon" placeholder="Selecciona una icona" />
      </div>
    </section>
    <section class="mt-2 flex justify-content-end">
      <Button label="Guardar" @click="submitForm" />
    </section>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import IconPicker from "../../../components/IconPicker.vue";
import { MachineStatusReason } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  reason: MachineStatusReason;
  existingReasons: Array<MachineStatusReason>;
}>();

const emit = defineEmits<{
  (e: "submit", reason: MachineStatusReason): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  code: Yup.string()
    .required("El codi és obligatori")
    .max(20, "El codi no pot superar els 20 caràcters")
    .test(
      "unique-code",
      "Ja existeix un motiu amb aquest codi per aquest estat de màquina",
      function (value) {
        if (!value) return true;
        const isDuplicate = props.existingReasons.some(
          (r) =>
            r.code.toLowerCase() === value.toLowerCase() &&
            r.id !== props.reason.id
        );
        return !isDuplicate;
      }
    ),
  name: Yup.string()
    .required("El nom és obligatori")
    .max(100, "El nom no pot superar els 100 caràcters"),
  description: Yup.string(),
  color: Yup.string().required("El color és obligatori"),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.reason);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.reason);
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

const onCancel = () => {
  emit("cancel");
};
</script>
