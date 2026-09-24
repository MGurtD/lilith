<script setup lang="ts">
import { ref } from "vue";
import { UserLogin } from "../../services/authentications.service";
import { useToast } from "primevue/usetoast";
import InputText from "primevue/inputtext";
import Button from "primevue/button";
import { useI18n } from "vue-i18n";

const emits = defineEmits(["login", "registerClick"]);

const userLogin = ref({
  username: "",
  password: "",
} as UserLogin);

const toast = useToast();
const { t } = useI18n();

const login = () => {
  if (
    userLogin.value.username.length === 0 ||
    userLogin.value.password.length === 0
  ) {
    toast.add({
      severity: "error",
      summary: t("login.invalid"),
      detail: t("login.credentialsRequired"),
    });
    return;
  }
  emits("login", userLogin.value);
};
</script>

<template>
  <form @submit.prevent="login" class="login-form">
    <h1 class="login-form__title">{{ $t("login.signIn") }}</h1>
    <div class="login-form__field">
      <label for="username">{{ $t("login.username") }}</label>
      <InputText
        id="username"
        type="text"
        autocomplete="username"
        v-model="userLogin.username"
        fluid
      />
    </div>
    <div class="login-form__field">
      <label for="password">{{ $t("login.password") }}</label>
      <InputText
        id="password"
        type="password"
        autocomplete="current-password"
        v-model="userLogin.password"
        fluid
      />
    </div>
    <Button type="submit" :label="$t('login.btnSignIn')" class="login-form__submit" fluid />
    <!--<div class="text-center">
      <span class="text-600">{{ $t("login.noAccount") }}</span>
      <Button
        :label="$t('login.createAccount')"
        link
        class="register-link p-0 ml-2"
        @click="emits('registerClick')"
      />
    </div>
    -->
  </form>
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

.login-form__field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.login-form__field label {
  font-family: var(--font-condensed);
  font-weight: 500;
  color: var(--p-surface-700);
}

.login-form__field :deep(.p-inputtext) {
  height: 3.1429rem;
  font-size: 1.0714rem;
}

.login-form__submit {
  height: 3.1429rem;
  margin-top: 0.5rem;
  font-size: 1.0714rem;
}
</style>
