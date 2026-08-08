<template>
  <form v-if="address">
    <section class="three-columns mb-2">
      <BaseInput
        id="name"
        :label="t('sales.components.nom')"
        v-model="address.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t('sales.components.principal') }}</label>
        <Checkbox v-model="address.main" class="w-full" :binary="true" />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t('sales.components.desactivada') }}</label>
        <Checkbox v-model="address.disabled" class="w-full" :binary="true" />
      </div>
    </section>

    <LocationFields
      :model-value="address"
      :show-distance="true"
      :validation-errors="validation.errors"
    />

    <div>
      <label class="block text-900 mb-2">{{ t('sales.components.observacions') }}</label>
      <Textarea v-model="address.observations" class="w-full" />
    </div>

    <div class="mt-2 flex justify-content-end gap-2">
      <Button :label="t('sales.components.guardar')" @click="submitForm" />
      <Button :label="t('sales.components.cancelar')" severity="secondary" @click="emit('cancel')" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import LocationFields from "@/components/LocationFields.vue";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { CustomerAddress } from "../types";

const { t } = useI18n();
const props = defineProps<{
  address: CustomerAddress;
}>();

const emit = defineEmits<{
  (e: "submit", address: CustomerAddress): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  name: Yup.string()
    .required(t("sales.validation.nameRequired"))
    .max(250, "El nom no pot superar els 250 caràcters"),
  country: Yup.string().required(t("sales.validation.countryRequired")),
  region: Yup.string().required(t("sales.validation.regionRequired")),
  city: Yup.string().required(t("sales.validation.cityRequired")),
  postalCode: Yup.string().required(t("sales.validation.postalCodeRequired")),
  address: Yup.string().required(t("sales.validation.addressRequired")),
  main: Yup.boolean().required(),
  disabled: Yup.boolean().required(),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.address);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.address);
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

