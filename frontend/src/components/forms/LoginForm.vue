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
import type { UserLogin } from "../../services/authentications.service";

const emit = defineEmits<{
  (event: "login", userLogin: UserLogin): void;
  (event: "registerClick"): void;
}>();

const { t } = useI18n();

const initialValues: UserLogin = { username: "", password: "" };

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 1 },
    fields: [
      {
        name: "username",
        label: t("login.username"),
        type: FormFieldType.Text,
        props: { autocomplete: "username" },
        validation: Yup.string().required(t("login.usernameRequired")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 1 },
    fields: [
      {
        name: "password",
        label: t("login.password"),
        type: FormFieldType.Password,
        props: {
          feedback: false,
          toggleMask: true,
          fluid: true,
          inputProps: { autocomplete: "current-password" },
        },
        validation: Yup.string().required(t("login.passwordRequired")),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("login", {
    username: stringValue(values.username, ""),
    password: stringValue(values.password, ""),
  });
};
</script>

<template>
  <div class="login-form">
    <h1 class="login-form__title">{{ t("login.signIn") }}</h1>
    <Form :rows="rows" :initial-values="initialValues" @submit="submit">
      <template #actions>
        <Button
          type="submit"
          :label="t('login.btnSignIn')"
          class="login-form__submit"
          fluid
        />
      </template>
    </Form>
  </div>
</template>

<style scoped>
.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.login-form__title {
  margin: 0 0 0.75rem;
  font-family: var(--font-condensed);
  font-size: 2.4286rem;
  line-height: 1.2;
  font-weight: 600;
  color: var(--p-text-color);
}

.login-form :deep(.generic-form__label) {
  font-family: var(--font-condensed);
  font-weight: 500;
  color: var(--p-surface-700);
}

.login-form :deep(.p-inputtext) {
  height: 3.1429rem;
  font-size: 1.0714rem;
}

.login-form :deep(.generic-form__actions) {
  display: block;
}

.login-form__submit {
  height: 3.1429rem;
  margin-top: 0.5rem;
  font-size: 1.0714rem;
}
</style>
