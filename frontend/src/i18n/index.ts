import { createI18n } from "vue-i18n";
import messages from "./translations";
import { PrimeVueConfiguration } from "primevue/config";
import spanish from "./primevue/spanish";
import english from "./primevue/english";
import catalan from "./primevue/catalan";

const localStorageLangKey = "app.lang";
const initial = (
  localStorage.getItem(localStorageLangKey) ||
  (navigator.language || "ca").slice(0, 2) ||
  "ca"
).toLowerCase();

export const i18n = createI18n({
  legacy: false,
  globalInjection: true,
  locale: initial,
  fallbackLocale: "es",
  messages,
});

export const applyPrimeVueLocale = (
  primevue: PrimeVueConfiguration,
  code: string,
) => {
  const lang = (code || "ca").toLowerCase();
  primevue.locale = (
    lang === "es" ? spanish : lang === "en" ? english : catalan
  ) as any;
  setI18nLocale(lang);
};

export function setI18nLocale(locale: string) {
  const lang = (locale || "ca").slice(0, 2).toLowerCase();
  i18n.global.locale.value = lang as "ca" | "es" | "en";
}
