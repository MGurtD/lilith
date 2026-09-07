<template>
  <form v-if="rejectionReason">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        :label="t('production.components.codi')"
        id="code"
        v-model="rejectionReason.code"
        :class="{ 'p-invalid': validation.errors.code }"
      />
      <BaseInput
        class="mb-2"
        :label="t('production.components.nom')"
        id="name"
        v-model="rejectionReason.name"
        :class="{ 'p-invalid': validation.errors.name }"
      />
    </section>
    <section class="one-column">
      <BaseInput
        class="mb-2"
        :label="t('production.components.descripcio')"
        id="description"
        v-model="rejectionReason.description"
        :class="{ 'p-invalid': validation.errors.description }"
      />
    </section>
    <section class="two-columns">
      <div>
        <label class="block text-900 mb-2">{{
          t("production.components.color")
        }}</label>
        <ColorPicker v-model="rejectionReason.color" class="mb-2" />
      </div>
      <div>
        <label class="block text-900 mb-2">{{
          t("production.components.desactivat")
        }}</label>
        <Checkbox
          v-model="rejectionReason.disabled"
          class="w-full"
          :binary="true"
        />
      </div>
    </section>

    <div class="mt-2">
      <Button
        :label="t('production.components.guardar')"
        class="mr-2"
        @click="submitForm"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import * as Yup from "yup";

import BaseInput from "../../../components/BaseInput.vue";
import { RejectionReason } from "../types";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";

const { t } = useI18n();

const props = defineProps<{
  rejectionReason: RejectionReason;
}>();

const emit = defineEmits<{
  (e: "submit", rejectionReason: RejectionReason): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  code: Yup.string()
    .required(t("production.validation.elCodiEsObligatori"))
    .max(20, t("production.validation.elCodiNoPotSuperarEls20Caracters")),
  name: Yup.string()
    .required(t("production.validation.elNomEsObligatori"))
    .max(100, t("production.validation.elNomNoPotSuperarEls100Caracters")),
  description: Yup.string(),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.rejectionReason);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.rejectionReason);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("production.components.formulariInvalid"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
