<template>
  <form @submit.prevent="submit" class="create-user-form">
    <section class="three-columns">
      <BaseInput
        id="username"
        v-model="model.username"
        :label="t('forms.user.usernameLabel') as string"
        :class="{ 'p-invalid': validation.errors.username }"
      />
      <BaseInput
        id="firstName"
        v-model="model.firstName"
        :label="t('forms.user.firstNameLabel') as string"
        :class="{ 'p-invalid': validation.errors.firstName }"
      />
      <BaseInput
        id="lastName"
        v-model="model.lastName"
        :label="t('forms.user.lastNameLabel') as string"
        :class="{ 'p-invalid': validation.errors.lastName }"
      />
    </section>

    <section class="three-columns">
      <BaseInput
        id="email"
        v-model="model.email"
        :label="t('forms.user.emailLabel') as string"
        :class="{ 'p-invalid': validation.errors.email }"
      />
      <div>
        <label class="block text-900 mb-2">{{
          t("forms.user.roleLabel")
        }}</label>
        <Select
          v-model="model.roleId"
          :options="roles"
          optionLabel="name"
          optionValue="id"
          class="w-full"
          :class="{ 'p-invalid': validation.errors.roleId }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{
          t("forms.user.languageLabel")
        }}</label>
        <Select
          v-model="model.preferredLanguage"
          :options="languages"
          optionLabel="name"
          optionValue="code"
          class="w-full"
          :class="{ 'p-invalid': validation.errors.preferredLanguage }"
        />
      </div>
    </section>

    <section class="three-columns">
      <div>
        <label class="block text-900 mb-2">{{
          t("forms.user.profileLabel")
        }}</label>
        <Select
          v-model="model.profileId"
          :options="profiles"
          optionLabel="name"
          optionValue="id"
          class="w-full"
          showClear
        />
      </div>
      <BaseInput
        :type="BaseInputType.PASSWORD"
        id="password"
        v-model="model.password"
        :label="t('forms.user.passwordLabel') as string"
        :class="{ 'p-invalid': validation.errors.password }"
      />
      <BaseInput
        :type="BaseInputType.PASSWORD"
        id="repeatPassword"
        v-model="model.repeatPassword"
        :label="t('forms.user.passwordRepeatLabel') as string"
        :class="{ 'p-invalid': validation.errors.repeatPassword }"
      />
    </section>

    <div class="flex justify-content-end gap-2 mt-4">
      <Button
        type="button"
        :label="t('forms.user.cancelButton')"
        severity="secondary"
        @click="emit('cancel')"
      />
      <Button type="submit" :label="t('forms.user.createButton')" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import * as Yup from "yup";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import BaseInput from "@/components/BaseInput.vue";
import { BaseInputType } from "@/types/component";
import { FormValidation, FormValidationResult } from "@/utils/form-validator";
import type { Language, Profile, Role } from "@/types";
import type { CreateManagedUserRequest } from "@/services/user.service";

const props = defineProps<{
  roles: Role[];
  languages: Language[];
  profiles: Profile[];
  initialLanguage: string;
}>();

const { t } = useI18n();

const emit = defineEmits<{
  (e: "submit", payload: CreateManagedUserRequest): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const model = ref<CreateManagedUserRequest>({
  username: "",
  password: "",
  repeatPassword: "",
  firstName: "",
  lastName: "",
  email: "",
  preferredLanguage: props.initialLanguage,
  roleId: "",
  profileId: null,
});

const schema = Yup.object().shape({
  username: Yup.string().required(
    t("forms.user.validation.usernameRequired") as string,
  ),
  firstName: Yup.string().required(
    t("forms.user.validation.firstNameRequired") as string,
  ),
  lastName: Yup.string().required(
    t("forms.user.validation.lastNameRequired") as string,
  ),
  email: Yup.string()
    .required(t("forms.user.validation.emailRequired") as string)
    .email(t("forms.user.validation.emailInvalid") as string),
  preferredLanguage: Yup.string().required(
    t("forms.user.validation.languageRequired") as string,
  ),
  roleId: Yup.string().required(
    t("forms.user.validation.roleRequired") as string,
  ),
  password: Yup.string()
    .required(t("forms.user.validation.passwordRequired") as string)
    .min(5, t("forms.user.validation.passwordMin") as string),
  repeatPassword: Yup.string()
    .required(t("forms.user.validation.repeatPasswordRequired") as string)
    .oneOf(
      [Yup.ref("password")],
      t("forms.user.validation.passwordMismatch") as string,
    ),
});

const validation = ref<FormValidationResult>({
  result: false,
  errors: {},
});

const submit = () => {
  validation.value = new FormValidation(schema).validate(model.value);
  if (!validation.value.result) {
    const errors = Object.values(validation.value.errors).flat().join("\n");
    toast.add({
      severity: "warn",
      summary: t("forms.user.validation.reviewForm") as string,
      detail: errors,
      life: 6000,
    });
    return;
  }

  emit("submit", {
    ...model.value,
    profileId: model.value.profileId || null,
  });
};
</script>

<style scoped>
.create-user-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>
