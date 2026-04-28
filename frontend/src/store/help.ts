import { defineStore } from "pinia";
import { i18n } from "@/i18n";
import { useStore } from "@/store";

const supportedLocales = ["ca", "es", "en"] as const;
const defaultLocale = "ca";

const markdownModules = import.meta.glob("../help/**/*.md", {
  query: "?raw",
  import: "default",
}) as Record<string, () => Promise<string>>;

const normalizeLocale = (locale?: string): string => {
  const normalized = (locale ?? defaultLocale).slice(0, 2).toLowerCase();
  return supportedLocales.includes(normalized as (typeof supportedLocales)[number])
    ? normalized
    : defaultLocale;
};

export const useHelpStore = defineStore("helpStore", {
  state: () => ({
    visible: false,
    loading: false,
    key: undefined as string | undefined,
    markdown: "",
    error: undefined as string | undefined,
    requestId: 0,
  }),
  actions: {
    async openForRoute(helpKey?: string) {
      const requestId = this.requestId + 1;
      this.requestId = requestId;
      this.visible = true;
      this.loading = true;
      this.key = helpKey;
      this.markdown = "";
      this.error = undefined;

      if (!helpKey) {
        this.loading = false;
        this.error = i18n.global.t("help.messages.noRouteHelp");
        return;
      }

      const appStore = useStore();
      const locale = normalizeLocale(appStore.language.current);
      const candidates = [`../help/${locale}/${helpKey}.md`];

      if (locale !== defaultLocale) {
        candidates.push(`../help/${defaultLocale}/${helpKey}.md`);
      }

      try {
        for (const candidate of candidates) {
          const loader = markdownModules[candidate];
          if (!loader) {
            continue;
          }

          const markdown = await loader();
          if (requestId !== this.requestId) {
            return;
          }

          this.markdown = markdown;
          this.loading = false;
          this.error = undefined;
          return;
        }

        if (requestId !== this.requestId) {
          return;
        }

        this.error = i18n.global.t("help.messages.notAvailable");
      } catch {
        if (requestId !== this.requestId) {
          return;
        }

        this.error = i18n.global.t("help.messages.loadError");
      } finally {
        if (requestId === this.requestId) {
          this.loading = false;
        }
      }
    },
    async toggleForRoute(helpKey?: string) {
      if (this.visible && this.key === helpKey) {
        this.close();
        return;
      }

      await this.openForRoute(helpKey);
    },
    close() {
      this.visible = false;
    },
    reset() {
      this.requestId += 1;
      this.visible = false;
      this.loading = false;
      this.key = undefined;
      this.markdown = "";
      this.error = undefined;
    },
  },
});
