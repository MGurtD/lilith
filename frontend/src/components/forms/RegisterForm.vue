<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { stringValue } from "@/components/forms/value-utils";
import Button from "primevue/button";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { UserRegister } from "../../services/authentications.service";

const emit = defineEmits<{
  (event: "register", userRegister: UserRegister): void;
  (event: "loginClick"): void;
}>();

const { t } = useI18n();

const initialValues: UserRegister = {
  firstName: "",
  lastName: "",
  mail: "",
  username: "",
  password: "",
  repeatPassword: "",
};

const passwordProps = { feedback: false, toggleMask: true, fluid: true };

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "firstName",
        label: t("login.firstName"),
        type: FormFieldType.Text,
        validation: Yup.string().required(t("login.firstNameRequired")),
      },
      {
        name: "lastName",
        label: t("login.lastName"),
        type: FormFieldType.Text,
        validation: Yup.string().required(t("login.lastNameRequired")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "username",
        label: t("login.username"),
        type: FormFieldType.Text,
        props: { autocomplete: "username" },
        validation: Yup.string().required(t("login.usernameRequired")),
      },
      {
        name: "mail",
        label: t("login.mail"),
        type: FormFieldType.Text,
        props: { type: "email", autocomplete: "email" },
        validation: Yup.string().email(t("login.emailInvalid")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "password",
        label: t("login.password"),
        type: FormFieldType.Password,
        props: {
          ...passwordProps,
          inputProps: { autocomplete: "new-password" },
        },
        validation: Yup.string().required(t("login.passwordRequired")),
      },
      {
        name: "repeatPassword",
        label: t("login.repeatPassword"),
        type: FormFieldType.Password,
        props: {
          ...passwordProps,
          inputProps: { autocomplete: "new-password" },
        },
        validation: Yup.string()
          .required(t("login.repeatPasswordRequired"))
          .oneOf([Yup.ref("password")], t("login.passwordsDoNotMatch")),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("register", {
    firstName: stringValue(values.firstName, ""),
    lastName: stringValue(values.lastName, ""),
    mail: stringValue(values.mail, ""),
    username: stringValue(values.username, ""),
    password: stringValue(values.password, ""),
    repeatPassword: stringValue(values.repeatPassword, ""),
  });
};
</script>

<template>
  <div class="register-card surface-card p-6 shadow-8 border-round-xl w-full">
    <div class="text-center mb-6">
      <div class="logo-container mb-4">
        <img src="../../assets/images/logo.jpg" alt="Logo" class="logo-image" />
      </div>
      <h1 class="text-3xl font-bold text-blue-700 mb-2">
        {{ t("login.register") }}
      </h1>
      <p class="text-600 text-lg">
        {{ t("login.registerSubtitle") }}
      </p>
    </div>

    <Form :rows="rows" :initial-values="initialValues" @submit="submit">
      <template #actions>
        <Button
          type="submit"
          :label="t('login.register')"
          class="w-full"
          size="large"
        />
      </template>
    </Form>

    <div class="text-center mt-4">
      <span class="text-600">{{ t("login.hasAccount") }}</span>
      <Button
        :label="t('login.signIn')"
        link
        class="p-0 ml-2"
        @click="emit('loginClick')"
      />
    </div>
  </div>
</template>

<style scoped>
.register-card {
  max-width: 600px;
}

.logo-container {
  display: inline-block;
}

.logo-image {
  display: block;
  height: 80px;
  width: auto;
  max-width: 180px;
  object-fit: contain;
}

.register-card :deep(.generic-form__actions) {
  display: block;
}
</style>
