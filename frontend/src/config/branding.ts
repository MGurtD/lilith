// Central branding configuration
// Extendable for theming (logo variants, colors) later.
export interface BrandingConfig {
  companyName: string;
  companyShortName?: string; // Used when sidebar collapsed
  logoPath?: string; // Relative to /src/assets or public root
  logoAlt?: string;
}

// Use explicit import so Vite handles hashing and correct URL emission
import logoHeaderWhite from "../assets/images/logo-header-white.png";

export const branding: BrandingConfig = {
  companyName: "Temges",
  companyShortName: "T",
  logoPath: logoHeaderWhite, // Built asset URL
  logoAlt: "Temges Logo",
};

export function getCompanyName(collapsed?: boolean) {
  return collapsed
    ? branding.companyShortName || branding.companyName.charAt(0)
    : branding.companyName;
}
