import { defineStore } from "pinia";
import { BrandingService } from "@/services/branding.service";
import type { Branding } from "@/types/branding";
import { applyBrandingPreset } from "@/config/branding-presets";

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
    activeEnterpriseId: undefined as string | undefined,
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
        this.activeEnterpriseId = enterpriseId;
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
    async update(payload: Branding): Promise<boolean> {
      const enterpriseId = this.activeEnterpriseId;
      if (!enterpriseId) {
        return false;
      }
      const updated = await brandingService.UpdateBranding(
        enterpriseId,
        payload,
      );
      if (!updated) {
        return false;
      }
      // Update reactive state first so subscribers see consistent branding,
      // then apply side effects (CSS variables, PrimeVue preset, etc.).
      this.branding = updated;
      applyBrandingPreset(updated);
      return true;
    },
    reset() {
      this.branding = { ...FALLBACK_BRANDING };
      this.loaded = false;
      this.activeEnterpriseId = undefined;
    },
  },
})