<template>
  <form v-if="expenseType">
    <div class="three-columns">
      <BaseInput
        class="mb-2"
        :label="t('purchase.fields.name')"
        id="name"
        v-model="expenseType.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('purchase.fields.description')"
        id="description"
        v-model="expenseType.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.fields.disabled") }}</label>
        <Checkbox
          v-model="expenseType.disabled"
          class="w-full"
          :binary="true"
        />
      </div>
    </div>

    <div class="mt-2">
      <Button :label="t('purchase.actions.save')" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { ExpenseType } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  expenseType: ExpenseType;
}>();

const emit = defineEmits<{
  (e: "submit", expenseType: ExpenseType): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const { t } = useI18n();

const schema = Yup.object().shape({
  name: Yup.string()
    .required(() => t("purchase.validation.nameRequired"))
    .max(250, () => t("purchase.validation.nameMaxLength")),
  description: Yup.string()
    .required(() => t("purchase.validation.descriptionRequired"))
    .max(250, () => t("purchase.validation.descriptionMaxLength")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.expenseType);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.expenseType);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("purchase.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
