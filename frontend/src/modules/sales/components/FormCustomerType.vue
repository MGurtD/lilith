<template>
  <form v-if="customerType">
    <BaseInput
      class="mb-2"
      :label="t('sales.components.nom')"
      id="name"
      v-model="customerType.name"
      :class="{
        'p-invalid': validation.errors.name,
      }"
    ></BaseInput>
    <BaseInput
      class="mb-2"
      :label="t('sales.components.descripcio')"
      id="description"
      v-model="customerType.description"
      :class="{
        'p-invalid': validation.errors.description,
      }"
    ></BaseInput>

    <div class="mt-2">
      <Button :label="t('sales.components.guardar')" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref } from "vue";
import { CustomerType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const { t } = useI18n();
const props = defineProps<{
  customerType: CustomerType;
}>();

const emit = defineEmits<{
  (e: "submit", customerType: CustomerType): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("sales.validation.nameRequired"))
    .max(250, "El nom comercial no pot superar els 250 carácters"),
  description: Yup.string()
    .required(t("sales.validation.descriptionRequired"))
    .max(250, "La descripció no pot superar els 250 carácters"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.customerType);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.customerType);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t('sales.components.formulariInvalid'),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
