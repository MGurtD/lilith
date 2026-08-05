import { defineStore } from "pinia";
import { palette, updatePrimaryPalette } from "@primeuix/themes";
import {
  DEFAULT_BRAND_NAME,
  DEFAULT_MAIN_LOGO,
  DEFAULT_SIDEBAR_LOGO,
  getBrandMonogram,
} from "@/config/branding";
import {
  BRANDING_PALETTE_TOKENS,
  DEFAULT_BRANDING_PALETTE,
  brandingService,
  normalizeBrandingPalette,
  type BrandingResponse,
} from "@/services/branding.service";
import { useStore } from "@/store";

export const useBrandingStore = defineStore("branding", {
  state: () => ({
    brandName: DEFAULT_BRAND_NAME,
    primaryColor: DEFAULT_BRANDING_PALETTE,
    hasMainLogo: false,
    hasSidebarLogo: false,
    version: "default",
    initialized: false,
  }),
  getters: {
    mainLogoUrl: (state): string =>
      state.hasMainLogo
        ? brandingService.getCurrentLogoUrl("main", state.version)
        : DEFAULT_MAIN_LOGO,
    sidebarLogoUrl(): string {
      if (this.hasSidebarLogo) {
        return brandingService.getCurrentLogoUrl("sidebar", this.version);
      }
      if (this.hasMainLogo) {
        return brandingService.getCurrentLogoUrl("main", this.version);
      }
      return DEFAULT_SIDEBAR_LOGO;
    },
    monogram: (state): string => getBrandMonogram(state.brandName),
  },
  actions: {
    async initialize() {
      try {
        const response = await brandingService.getCurrent();
        this.setBranding(response);
      } catch {
        this.resetToDefault();
      } finally {
        this.applyTheme();
        useStore().setBrandName(this.brandName);
        this.initialized = true;
      }
    },
    setBranding(response: BrandingResponse) {
      this.brandName = response.brandName?.trim() || DEFAULT_BRAND_NAME;
      this.primaryColor = normalizeBrandingPalette(response.primaryColor);
      this.hasMainLogo = response.hasMainLogo;
      this.hasSidebarLogo = response.hasSidebarLogo;
      this.version = response.version || "default";
    },
    resetToDefault() {
      this.brandName = DEFAULT_BRAND_NAME;
      this.primaryColor = DEFAULT_BRANDING_PALETTE;
      this.hasMainLogo = false;
      this.hasSidebarLogo = false;
      this.version = "default";
    },
    applyTheme() {
      const primaryPalette = palette(BRANDING_PALETTE_TOKENS[this.primaryColor]) as
        Parameters<typeof updatePrimaryPalette>[0];
      updatePrimaryPalette(primaryPalette);
    },
  },
});
