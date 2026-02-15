<script setup lang="ts">
import { ref } from "vue";
import { UserLogin } from "../../services/authentications.service";
import { useToast } from "primevue/usetoast";
import InputGroup from "primevue/inputgroup";
import InputGroupAddon from "primevue/inputgroupaddon";
import InputText from "primevue/inputtext";
import Button from "primevue/button";

const emits = defineEmits(["login", "registerClick"]);

const userLogin = ref({
  username: "",
  password: "",
} as UserLogin);

const toast = useToast();
const login = () => {
  if (
    userLogin.value.username.length === 0 ||
    userLogin.value.password.length === 0
  ) {
    toast.add({
      severity: "error",
      summary: "Login incorrecte",
      detail: "El nom d'usuari i la contrasenya són obligatoris",
    });
    return;
  }

  emits("login", userLogin.value);
};

const registerClick = () => {
  emits("registerClick");
};
</script>

<template>
  <div class="login-card surface-card p-6 shadow-8 border-round-xl w-full">
    <div class="text-center mb-6">
      <div class="logo-container mb-4">
        <img src="../../assets/images/logo.jpg" alt="Logo" class="logo-image" />
      </div>
      <h1 class="text-3xl font-bold text-900 mb-2">
        {{ $t("login.welcome") || "Benvingut" }}
      </h1>
      <p class="text-600 text-lg">
        {{ $t("login.subtitle") || "Inicia sessió per continuar" }}
      </p>
    </div>

    <form @submit.prevent="login" class="login-form">
      <div class="form-group mb-4">
        <label for="username" class="form-label">
          {{ $t("login.username") }}
        </label>
        <InputGroup class="input-group-enhanced">
          <InputGroupAddon class="input-addon">
            <i class="pi pi-user"></i>
          </InputGroupAddon>
          <InputText
            id="username"
            type="text"
            class="form-input"
            v-model="userLogin.username"
            @keyup.enter="login"
            :placeholder="
              $t('login.usernamePlaceholder') || 'Introdueix el teu usuari'
            "
          />
        </InputGroup>
      </div>

      <div class="form-group mb-6">
        <label for="password" class="form-label">
          {{ $t("login.password") }}
        </label>
        <InputGroup class="input-group-enhanced">
          <InputGroupAddon class="input-addon">
            <i class="pi pi-key"></i>
          </InputGroupAddon>
          <InputText
            id="password"
            type="password"
            class="form-input"
            v-model="userLogin.password"
            @keyup.enter="login"
            :placeholder="
              $t('login.passwordPlaceholder') ||
              'Introdueix la teva contrasenya'
            "
          />
        </InputGroup>
      </div>

      <Button
        type="submit"
        :label="$t('login.btnSignIn')"
        class="login-button w-full mb-4"
        size="large"
        :loading="false"
      />

      <!--<div class="text-center">
        <span class="text-600">{{
          $t("login.noAccount") || "No tens compte?"
        }}</span>
        <Button
          :label="$t('login.createAccount') || 'Crear compte'"
          link
          class="register-link p-0 ml-2"
          @click="registerClick"
        />
      </div>
      -->
    </form>
  </div>
</template>
<style lang="scss" scoped>
.login-card {
  backdrop-filter: blur(10px);
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: all 0.3s ease;

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
.login-form {
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

  .input-group-enhanced {
    transition: all 0.3s ease;
    overflow: hidden;

    &:focus-within {
      transform: translateY(-1px);
      box-shadow: 0 8px 25px rgba(var(--p-blue-500), 0.15);

      .input-addon {
        background: var(--p-blue-50);
        color: var(--p-blue-600);
      }

      .form-label {
        color: var(--p-blue-600);
      }
    }
  }

  .input-addon {
    background: var(--p-surface-100);
    border-color: var(--p-surface-300);
    color: var(--text-color-secondary);
    transition: all 0.3s ease;
    padding: 1rem;
  }

  .form-input {
    border-color: var(--p-surface-300);
    padding: 1rem;
    font-size: 1rem;
    transition: all 0.3s ease;

    &:focus {
      border-color: var(--p-blue-500);
      box-shadow: none;
    }

    &::placeholder {
      color: var(--text-color-secondary);
      opacity: 0.7;
    }
  }

  .login-button {
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

    &:hover {
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
  }

  .register-link {
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

/* Responsive design */
@media (max-width: 768px) {
  .login-card {
    padding: 1.5rem;
    margin: 1rem;
  }

  .logo-image {
    height: 60px;
    width: auto;
    max-width: 90px;
    border-radius: 10px;
  }

  .login-form {
    .form-input,
    .input-addon {
      padding: 0.875rem;
    }

    .login-button {
      padding: 0.875rem;
      font-size: 1rem;
    }
  }
}

@media (max-width: 480px) {
  .login-card {
    padding: 1.25rem;
    margin: 0.5rem;
  }

  .logo-image {
    height: 50px;
    width: auto;
    max-width: 75px;
    border-radius: 8px;
  }
}

/* Loading state animation */
@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
}

.login-button:disabled {
  animation: pulse 1.5s infinite;
}

/* Focus accessibility improvements */
.form-input:focus,
.login-button:focus,
.register-link:focus {
  outline: 2px solid var(--p-blue-400);
  outline-offset: 2px;
}
</style>
