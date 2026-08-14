<template>
  <form>
    <BaseInput
      class="mb-2"
      :label="$t('shared.statusTransitions.form.name')"
      v-model="transition.name"
      :class="{
        'p-invalid': validation.errors.name,
      }"
    ></BaseInput>
    <section class="two-columns">
      <div>
        <label class="block text-900 mb-2">{{ $t('shared.statusTransitions.form.origin') }}</label>
        <Select
          v-model="transition.statusId"
          :options="statuses"
          optionValue="id"
          optionLabel="name"
        />
      </div>
      <div class="mb-4">
        <label class="block text-900 mb-2">{{ $t('shared.statusTransitions.form.destination') }}</label>
        <Select
          v-model="transition.statusToId"
          :options="statuses"
          optionValue="id"
          optionLabel="name"
        />
      </div>
    </section>

    <Button :label="$t('shared.statusTransitions.form.confirm')" @click="submitForm" style="float: right" />
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { Status, StatusTransition } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";

const { t } = useI18n();
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
      summary: t("shared.common.invalidForm"),
      detail: t("shared.statusTransitions.form.sameStatusError"),
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
      summary: t("shared.common.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
