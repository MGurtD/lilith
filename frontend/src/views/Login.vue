<template>
  <div class="container">
    <LoginForm
      v-if="showLogin"
      class="login-form"
      @login="loginHandler"
      @registerClick="navigateToRegister"
    />
    <RegisterForm
      v-else
      class="login-form"
      @register="registerHandler"
      @loginClick="navigateToLogin"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import {
  UserLogin,
  AuthenticationService,
  UserRegister,
} from "../services/authentications.service";
import LoginForm from "../components/forms/LoginForm.vue";
import RegisterForm from "../components/forms/RegisterForm.vue";
import { useStore } from "../store";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { AuthenticationResponse } from "../types";

const service = new AuthenticationService();
const store = useStore();
const router = useRouter();
const toast = useToast();

const showLogin = ref(true);

const loginHandler = async (userLogin: UserLogin) => {
  const response = await service.Login(userLogin);
  manageAuthorizationResponse(response);
};

const registerHandler = async (userRegister: UserRegister) => {
  const response = await service.Register(userRegister);
  manageAuthorizationResponse(response);
};

const manageAuthorizationResponse = (response: AuthenticationResponse) => {
  if (!response.result) {
    toast.add({
      severity: "error",
      summary: response.errors[0],
      life: 8000,
    });
    return;
  } else {
    store.setAuthorization(response);
    router.push("/");
  }
};

const navigateToRegister = () => (showLogin.value = false);
const navigateToLogin = () => (showLogin.value = true);
</script>

<style scoped>
body {
  display: block;
  margin: 0px !important;
  padding: 0px;
}

.container {
  min-height: 100vh;
  background: linear-gradient(
    135deg,
    var(--p-blue-900) 0%,
    var(--p-blue-800) 50%,
    var(--p-blue-700) 100%
  );
  background-attachment: fixed;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  position: relative;
  overflow: hidden;
}

/* Decorative background elements */
.container::before {
  content: "";
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(
    circle,
    rgba(255, 255, 255, 0.1) 1px,
    transparent 1px
  );
  background-size: 50px 50px;
  animation: float 20s linear infinite;
  pointer-events: none;
}

@keyframes float {
  0% {
    transform: translate(0, 0) rotate(0deg);
  }
  100% {
    transform: translate(-50px, -50px) rotate(360deg);
  }
}

.login-form {
  width: 100%;
  max-width: 480px;
  animation: slideUp 0.6s ease-out;
  position: relative;
  z-index: 1;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Responsive breakpoints */
@media (max-width: 768px) {
  .container {
    padding: 0.5rem;
  }

  .login-form {
    max-width: 100%;
  }
}

@media (max-width: 480px) {
  .container {
    padding: 1rem 0.5rem;
  }
}

/* Enhance form appearance on larger screens */
@media (min-width: 1024px) {
  .login-form {
    max-width: 520px;
  }
}
</style>
