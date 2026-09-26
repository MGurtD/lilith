<template>
  <div class="login">
    <section class="login__side">
      <img
        :src="logoLoadFailed ? DEFAULT_MAIN_LOGO : brandingStore.mainLogoUrl"
        :alt="brandingStore.brandName"
        class="login__logo"
        draggable="false"
        @error="logoLoadFailed = true"
      />

      <div class="login__form">
        <LoginForm
          v-if="showLogin"
          @login="loginHandler"
          @registerClick="navigateToRegister"
        />
        <RegisterForm v-else @register="registerHandler" @loginClick="navigateToLogin" />
      </div>

      <footer class="login__footer">
        <SelectButton
          :modelValue="store.language.current"
          :options="languageOptions"
          optionLabel="label"
          optionValue="value"
          :allowEmpty="false"
          size="small"
          :aria-label="t('ui.language')"
          @update:modelValue="changeLanguage"
        />
        <span class="login__credit">{{ t("login.productCredit") }}</span>
      </footer>
    </section>

    <!-- Brand panel: a drawing inked in the tenant's colour and its title block. -->
    <aside class="login__drawing">
      <ShaftDrawing class="login__shaft" />
      <TitleBlock
        class="login__title-block"
        :top="{ label: t('ui.titleBlock.system'), value: 'Zenith ERP' }"
        :cells="[
          { label: t('ui.titleBlock.company'), value: brandingStore.brandName, strong: true },
          { label: t('ui.titleBlock.scale'), value: '1:1' },
          { label: t('ui.titleBlock.date'), value: today },
        ]"
      />
    </aside>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import SelectButton from "primevue/selectbutton";
import { useI18n } from "vue-i18n";
import {
  UserLogin,
  AuthenticationService,
  UserRegister,
} from "../services/authentications.service";
import LoginForm from "../components/forms/LoginForm.vue";
import RegisterForm from "../components/forms/RegisterForm.vue";
import ShaftDrawing from "@/components/brand/ShaftDrawing.vue";
import TitleBlock from "@/components/brand/TitleBlock.vue";
import { DEFAULT_MAIN_LOGO } from "@/config/branding";
import { useStore } from "../store";
import { useBrandingStore } from "@/store/branding";
import { useLanguageStore } from "@/store/languages";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { AuthenticationResponse } from "../types";

const service = new AuthenticationService();
const store = useStore();
const brandingStore = useBrandingStore();
const languageStore = useLanguageStore();
const router = useRouter();
const toast = useToast();
const { t } = useI18n();

const showLogin = ref(true);
const logoLoadFailed = ref(false);

// Language names are endonyms, so they read the same in every locale. The
// backend list wins when it is reachable before signing in.
const FALLBACK_LANGUAGES = [
  { label: "Català", value: "ca" },
  { label: "Castellano", value: "es" },
  { label: "English", value: "en" },
];
const languageOptions = computed(() =>
  languageStore.options.length ? languageStore.options : FALLBACK_LANGUAGES,
);
const changeLanguage = (code: string) => {
  if (code) void store.changeLanguage(code);
};

const today = computed(() =>
  new Intl.DateTimeFormat(store.language.current, {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date()),
);

onMounted(() => {
  languageStore.fetchAll().catch(() => undefined);
});

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
.login {
  min-height: 100dvh;
  box-sizing: border-box;
  display: flex;
  background: var(--p-surface-0);
  border-top: 4px solid var(--p-primary-color);
}

.login__side {
  width: 40rem;
  max-width: 100%;
  flex-shrink: 0;
  box-sizing: border-box;
  padding: 3.5rem 6.25rem 4rem;
  display: flex;
  flex-direction: column;
}

.login__logo {
  align-self: flex-start;
  height: 4rem;
  max-width: 16rem;
  object-fit: contain;
}

.login__form {
  margin: auto 0;
  padding: 2.5rem 0;
}

.login__footer {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.login__credit {
  font-size: 0.9286rem;
  color: var(--p-text-muted-color);
}

.login__drawing {
  flex: 1;
  min-width: 0;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 3rem 3rem 12rem;
  background: var(--p-surface-50);
  border-left: 1px solid var(--p-surface-200);
}

.login__shaft {
  max-width: 50rem;
}

.login__title-block {
  position: absolute;
  right: 3.5rem;
  bottom: 3.5rem;
  width: min(34rem, calc(100% - 7rem));
}

/* Narrow screens: the form alone, full width. */
@media (max-width: 1023.98px) {
  .login__side {
    width: 100%;
    padding: 2rem 1.5rem;
  }

  .login__form {
    width: 100%;
    max-width: 28rem;
    align-self: center;
  }

  .login__drawing {
    display: none;
  }
}
</style>
