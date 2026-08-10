<template>
  <form v-if="reason">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        :label="t('production.components.codi')"
        id="code"
        v-model="reason.code"
        :class="{ 'p-invalid': validation.errors.code }"
      />
      <BaseInput
        class="mb-2"
        :label="t('production.components.nom')"
        id="name"
        v-model="reason.name"
        :class="{ 'p-invalid': validation.errors.name }"
      />
    </section>
    <section class="one-column">
      <BaseInput
        class="mb-2"
        :label="t('production.components.descripcio')"
        id="description"
        v-model="reason.description"
        :class="{ 'p-invalid': validation.errors.description }"
      />
    </section>
    <section class="two-columns">
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.color") }}</label>
        <ColorPicker
          v-model="reason.color"
          class="mb-2"
          :class="{ 'p-invalid': validation.errors.color }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.icona") }}</label>
        <IconPicker v-model="reason.icon" :placeholder="t('production.components.seleccionaUnaIcona')" />
      </div>
    </section>
    <section class="mt-2 flex justify-content-end">
      <Button :label="t('production.components.guardar')" @click="submitForm" />
    </section>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
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
    .required(t("production.validation.elCodiEsObligatori"))
    .max(20, t("production.validation.elCodiNoPotSuperarEls20Caracters"))
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
    .required(t("production.validation.elNomEsObligatori"))
    .max(100, t("production.validation.elNomNoPotSuperarEls100Caracters")),
  description: Yup.string(),
  color: Yup.string().required(t("production.validation.elColorEsObligatori")),
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
      summary: t("production.components.formulariInvalid"),
      detail: errors,
      life: 5000,
    });
  }
};

const onCancel = () => {
  emit("cancel");
};
</script>
