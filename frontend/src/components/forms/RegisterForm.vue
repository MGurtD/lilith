<template>
  <div class="register-card surface-card p-6 shadow-8 border-round-xl w-full">
    <div class="text-center mb-6">
      <div class="logo-container mb-4">
        <img src="../../assets/images/logo.jpg" alt="Logo" class="logo-image" />
      </div>
      <h1 class="text-3xl font-bold text-blue-700 mb-2">
        {{ $t("login.register") }}
      </h1>
      <p class="text-600 text-lg">
        {{ $t("login.registerSubtitle") || "Crea el teu compte per començar" }}
      </p>
    </div>

    <form @submit.prevent="register" class="register-form">
      <div class="form-row">
        <div class="form-group">
          <label for="firstName" class="form-label">
            {{ $t("login.firstName") }}
          </label>
          <span class="input-wrapper">
            <i class="pi pi-user input-icon" />
            <InputText
              id="firstName"
              type="text"
              class="form-input"
              :class="{ 'p-invalid': v$.firstName.$errors.length > 0 }"
              v-model="state.firstName"
              @keyup.enter="register"
            />
          </span>
        </div>

        <div class="form-group">
          <label for="lastName" class="form-label">
            {{ $t("login.lastName") }}
          </label>
          <span class="input-wrapper">
            <i class="pi pi-user input-icon" />
            <InputText
              id="lastName"
              type="text"
              class="form-input"
              v-model="state.lastName"
              :class="{ 'p-invalid': v$.lastName.$errors.length > 0 }"
              @keyup.enter="register"
            />
          </span>
        </div>
      </div>

      <div class="form-row">
        <div class="form-group">
          <label for="username" class="form-label">
            {{ $t("login.username") }}
          </label>
          <span class="input-wrapper">
            <i class="pi pi-user input-icon" />
            <InputText
              id="username"
              type="text"
              class="form-input"
              v-model="state.username"
              :class="{ 'p-invalid': v$.username.$errors.length > 0 }"
              @keyup.enter="register"
            />
          </span>
        </div>

        <div class="form-group">
          <label for="email" class="form-label">
            {{ $t("login.mail") }}
          </label>
          <span class="input-wrapper">
            <i class="pi pi-envelope input-icon" />
            <InputText
              id="email"
              type="email"
              class="form-input"
              v-model="state.mail"
              :class="{ 'p-invalid': v$.mail.$errors.length > 0 }"
              @keyup.enter="register"
            />
          </span>
        </div>
      </div>

      <div class="form-row mb-6">
        <div class="form-group">
          <label for="password1" class="form-label">
            {{ $t("login.password") }}
          </label>
          <span class="input-wrapper">
            <i class="pi pi-key input-icon" />
            <InputText
              id="password1"
              type="password"
              class="form-input"
              v-model="state.password"
              :class="{ 'p-invalid': !arePasswordsEqual }"
              @keyup.enter="register"
            />
          </span>
        </div>

        <div class="form-group">
          <label for="password2" class="form-label">
            {{ $t("login.repeatPassword") }}
          </label>
          <span class="input-wrapper">
            <i class="pi pi-verified input-icon" />
            <InputText
              id="password2"
              type="password"
              class="form-input"
              v-model="state.repeatPassword"
              :class="{ 'p-invalid': !arePasswordsEqual }"
              @keyup.enter="register"
            />
          </span>
        </div>
      </div>

      <Button
        type="submit"
        :label="$t('login.register')"
        class="register-button w-full mb-4"
        size="large"
        :disabled="!arePasswordsEqual || v$.$invalid"
      />

      <div class="text-center">
        <span class="text-600">{{
          $t("login.hasAccount") || "Ja tens compte?"
        }}</span>
        <Button
          :label="$t('login.signIn') || 'Inicia sessió'"
          link
          class="login-link p-0 ml-2"
          @click="loginClick"
        />
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive } from "vue";
import { UserRegister } from "../../services/authentications.service";
import { useVuelidate } from "@vuelidate/core";
import { required, email } from "@vuelidate/validators";
import { useToast } from "primevue/usetoast";
import InputText from "primevue/inputtext";
import Button from "primevue/button";

const emits = defineEmits(["register", "loginClick"]);
const toast = useToast();

const state = reactive({
  firstName: "",
  lastName: "",
  mail: "",
  username: "",
  password: "",
  repeatPassword: "",
} as UserRegister);

const arePasswordsEqual = computed(() => {
  if (v$.value.password.$dirty && v$.value.repeatPassword.$dirty) {
    return state.password === state.repeatPassword;
  } else {
    return true;
  }
});

const rules = computed(() => ({
  firstName: { required, $autoDirty: true },
  lastName: { required, $autoDirty: true },
  mail: { email, $autoDirty: true },
  username: { required, $autoDirty: true },
  password: { required, $autoDirty: true },
  repeatPassword: { required, $autoDirty: true },
}));

const v$ = useVuelidate(rules, state);

const register = () => {
  if (!arePasswordsEqual) {
    toast.add({
      severity: "error",
      summary: "Les contrasenyes introduïdes no coincideixen",
    });
    return;
  }

  emits("register", state);
};

const loginClick = () => emits("loginClick");
</script>

<style lang="scss" scoped>
.register-card {
  backdrop-filter: blur(10px);
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: all 0.3s ease;
  max-width: 600px;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  }
}

.logo-container {
  position: relative;
  display: inline-block;
  background: linear-gradient(135deg, var(--p-blue-50) 0%, var(--p-blue-100) 100%);
  border-radius: 20px;
  box-shadow: 0 4px 15px rgba(var(--p-blue-500), 0.1);
  transition: all 0.3s ease;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(var(--p-blue-500), 0.15);
  }
}

.logo-image {
  height: 80px;
  width: auto;
  max-width: 180px;
  object-fit: contain;
  border-radius: 20px;
  border: 2px solid var(--p-blue-200);
  transition: all 0.3s ease;
  display: block;

  &:hover {
    transform: scale(1.02);
    border-color: var(--p-blue-400);
  }
}

.register-form {
  .form-row {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 1rem;
    margin-bottom: 1.5rem;

    @media (max-width: 640px) {
      grid-template-columns: 1fr;
      gap: 1rem;
    }
  }

  .form-group {
    position: relative;
  }

  .form-label {
    display: block;
    font-weight: 600;
    color: var(--text-color);
    margin-bottom: 0.5rem;
    font-size: 0.95rem;
    transition: color 0.3s ease;
  }

  .input-wrapper {
    position: relative;
    display: block;
    transition: all 0.3s ease;

    &:focus-within {
      transform: translateY(-1px);

      .input-icon {
        color: var(--p-blue-600);
      }

      .form-input {
        border-color: var(--p-blue-500);
        box-shadow: 0 8px 25px rgba(var(--p-blue-500), 0.15);
      }
    }
  }

  .input-icon {
    position: absolute;
    left: 1rem;
    top: 50%;
    transform: translateY(-50%);
    color: var(--text-color-secondary);
    z-index: 2;
    transition: all 0.3s ease;
  }

  .form-input {
    width: 100%;
    padding: 1rem 1rem 1rem 3rem;
    border: 1px solid var(--p-surface-300);
    font-size: 1rem;
    transition: all 0.3s ease;
    background: var(--p-surface-0);

    &:focus {
      outline: none;
      border-color: var(--p-blue-500);
      box-shadow: 0 0 0 3px rgba(var(--p-blue-500), 0.1);
    }

    &::placeholder {
      color: var(--text-color-secondary);
      opacity: 0.7;
    }

    &.p-invalid {
      border-color: var(--p-red-500);

      &:focus {
        border-color: var(--p-red-500);
        box-shadow: 0 0 0 3px rgba(var(--p-red-500), 0.1);
      }
    }
  }

  .register-button {
    background: linear-gradient(
      135deg,
      var(--p-blue-600) 0%,
      var(--p-blue-500) 100%
    );
    border: none;
    border-radius: 12px;
    padding: 1rem;
    font-weight: 600;
    font-size: 1.1rem;
    transition: all 0.3s ease;
    position: relative;
    overflow: hidden;

    &::before {
      content: "";
      position: absolute;
      top: 0;
      left: -100%;
      width: 100%;
      height: 100%;
      background: linear-gradient(
        90deg,
        transparent,
        rgba(255, 255, 255, 0.2),
        transparent
      );
      transition: left 0.5s ease;
    }

    &:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 10px 30px rgba(var(--p-blue-500), 0.3);

      &::before {
        left: 100%;
      }
    }

    &:active {
      transform: translateY(0);
    }

    &:focus {
      box-shadow: 0 0 0 3px rgba(var(--p-blue-500), 0.2);
    }

    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;

      &:hover {
        transform: none;
        box-shadow: none;
      }
    }
  }

  .login-link {
    color: var(--p-blue-600);
    font-weight: 600;
    text-decoration: none;
    transition: all 0.3s ease;

    &:hover {
      color: var(--p-blue-700);
      text-decoration: underline;
    }
  }
}

/* Enhanced responsive design */
@media (max-width: 768px) {
  .register-card {
    padding: 1.5rem;
    margin: 1rem;
  }

  .logo-image {
    height: 50px;
    width: auto;
    max-width: 75px;
    border-radius: 8px;
  }

  .register-form {
    .form-input {
      padding: 0.875rem 0.875rem 0.875rem 2.5rem;
    }

    .input-icon {
      left: 0.875rem;
    }

    .register-button {
      padding: 0.875rem;
      font-size: 1rem;
    }
  }
}

@media (max-width: 480px) {
  .register-card {
    padding: 1.25rem;
    margin: 0.5rem;
  }

  .register-form {
    .form-row {
      gap: 0.75rem;
      margin-bottom: 1rem;
    }
  }
}

/* Form validation feedback */
.form-input.p-invalid {
  animation: shake 0.5s ease-in-out;
}

/* Focus accessibility improvements */
.form-input:focus,
.register-button:focus,
.login-link:focus {
  outline: 2px solid var(--p-blue-400);
  outline-offset: 2px;
}
</style>
