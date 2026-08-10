<template>
  <form v-if="contact">
    <section class="three-columns">
      <BaseInput
        :label="t('purchase.supplierContact.fields.firstName')"
        id="firstName"
        v-model="contact.firstName"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.firstName,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('purchase.supplierContact.fields.lastName')"
        id="lastName"
        v-model="contact.lastName"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.lastName,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('purchase.supplierContact.fields.charge')"
        id="charge"
        v-model="contact.charge"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.charge,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns">
      <BaseInput
        class="mb-2"
        :label="t('purchase.supplierContact.fields.email')"
        id="email"
        v-model="contact.email"
        :class="{
          'p-invalid': validation.errors.email,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('purchase.supplierContact.fields.phone')"
        id="phone"
        v-model="contact.phone"
        :class="{
          'p-invalid': validation.errors.phone,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.supplierContact.fields.default") }}</label>
        <Checkbox v-model="contact.default" class="w-full" :binary="true" />
      </div>
    </section>
    <div>
      <label class="block text-900 mb-2">{{ t("purchase.supplierContact.fields.observations") }}</label>
      <Textarea v-model="contact.observations" class="w-full" />
    </div>

    <div class="mt-2 flex justify-content-end gap-2">
      <Button :label="t('purchase.supplierContact.actions.save')" @click="submitForm" />
      <Button :label="t('purchase.supplierContact.actions.cancel')" severity="secondary" @click="cancel" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { SupplierContact } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  contact: SupplierContact;
}>();

const emit = defineEmits<{
  (e: "submit", contact: SupplierContact): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const { t } = useI18n();

const schema = Yup.object().shape({
  firstName: Yup.string()
    .required(() => t("purchase.supplierContact.validation.firstNameRequired"))
    .max(250, () => t("purchase.supplierContact.validation.firstNameMaxLength")),
  lastName: Yup.string()
    .required(() => t("purchase.supplierContact.validation.lastNameRequired"))
    .max(250, () => t("purchase.supplierContact.validation.lastNameMaxLength")),
  charge: Yup.string(),
  email: Yup.string()
    .required(() => t("purchase.supplierContact.validation.emailRequired"))
    .email(() => t("purchase.supplierContact.validation.emailInvalid")),
  phone: Yup.string()
    .required(() => t("purchase.supplierContact.validation.phoneRequired"))
    .max(15, () => t("purchase.supplierContact.validation.phoneMaxLength")),
  phoneExtension: Yup.string(),
  observations: Yup.string(),
  disabled: Yup.boolean().required(),
  default: Yup.boolean().required(),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.contact);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.contact);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("purchase.supplierContact.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};

const cancel = () => {
  emit("cancel");
};
</script>
