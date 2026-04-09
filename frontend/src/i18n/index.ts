import { createI18n } from "vue-i18n";
import messages from "./translations";
import { PrimeVueConfiguration } from "primevue/config";
import spanish from "./primevue/spanish";
import english from "./primevue/english";
import catalan from "./primevue/catalan";

const localStorageLangKey = "app.lang";
const defaultLocale = "ca";
const supportedLocales = ["ca", "es", "en"] as const;

const normalizeLocale = (locale?: string | null) => {
  const normalized = (locale || defaultLocale).slice(0, 2).toLowerCase();
  return supportedLocales.includes(
    normalized as (typeof supportedLocales)[number],
  )
    ? normalized
    : defaultLocale;
};

const initial = normalizeLocale(localStorage.getItem(localStorageLangKey));

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
  const lang = normalizeLocale(code);
  primevue.locale = (
    lang === "es" ? spanish : lang === "en" ? english : catalan
  ) as any;
  setI18nLocale(lang);
};

export function setI18nLocale(locale: string) {
  const lang = normalizeLocale(locale);
  i18n.global.locale.value = lang as "ca" | "es" | "en";
}
