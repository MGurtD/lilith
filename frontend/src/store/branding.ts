import { defineStore } from "pinia";
import { BrandingService } from "@/services/branding.service";
import type { Branding } from "@/types/branding";

const FALLBACK_BRANDING: Branding = {
  theme: null,
  primaryColor: null,
  logoMain: null,
  logoSidebar: null,
  titleSidebar: null,
};

const brandingService = new BrandingService();

export const useBrandingStore = defineStore("branding", {
  state: () => ({
    branding: { ...FALLBACK_BRANDING } as Branding,
    loaded: false,
    loading: false,
  }),
  getters: {
    companyName: (state) =>
      state.branding.titleSidebar?.trim() || "Lilith",
    companyShortName(): string {
      const name = this.branding.titleSidebar?.trim();
      return (name?.charAt(0).toUpperCase() || "L") as string;
    },
    hasLogoMain: (state) =>
      !!state.branding.logoMain && state.branding.logoMain.length > 0,
    hasLogoSidebar: (state) =>
      !!state.branding.logoSidebar &&
      state.branding.logoSidebar.length > 0,
  },
  actions: {
    async load(enterpriseId: string) {
      if (this.loading) return;
      this.loading = true;
      try {
        const branding = await brandingService.GetByEnterpriseId(
          enterpriseId,
        );
        if (branding) {
          this.branding = branding;
        }
        this.loaded = true;
      } finally {
        this.loading = false;
      }
    },
    reset() {
      this.branding = { ...FALLBACK_BRANDING };
      this.loaded = false;
    },
  },
});