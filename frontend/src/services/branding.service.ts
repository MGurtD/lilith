import apiClient from "@/api/api.client";

export type BrandingLogoSlot = "main" | "sidebar";

export type BrandingPalette =
  | "black"
  | "blue"
  | "indigo"
  | "emerald"
  | "teal"
  | "violet"
  | "orange"
  | "rose";

export const DEFAULT_BRANDING_PALETTE: BrandingPalette = "blue";

export const BRANDING_PALETTE_OPTIONS: ReadonlyArray<{
  value: BrandingPalette;
  label: string;
  swatch: string;
}> = [
  { value: "black", label: "Negre", swatch: "#000000" },
  { value: "blue", label: "Blau", swatch: "#3B82F6" },
  { value: "indigo", label: "Indi", swatch: "#6366F1" },
  { value: "emerald", label: "Maragda", swatch: "#10B981" },
  { value: "teal", label: "Turquesa", swatch: "#14B8A6" },
  { value: "violet", label: "Violeta", swatch: "#8B5CF6" },
  { value: "orange", label: "Taronja", swatch: "#F97316" },
  { value: "rose", label: "Rosa", swatch: "#F43F5E" },
];

export const BRANDING_PALETTE_TOKENS: Record<BrandingPalette, string> = {
  black: "#000000",
  blue: "{blue}",
  indigo: "{indigo}",
  emerald: "{emerald}",
  teal: "{teal}",
  violet: "{violet}",
  orange: "{orange}",
  rose: "{rose}",
};

export const isBrandingPalette = (value: unknown): value is BrandingPalette =>
  typeof value === "string" &&
  BRANDING_PALETTE_OPTIONS.some((option) => option.value === value.trim().toLowerCase());

export const normalizeBrandingPalette = (value: unknown): BrandingPalette =>
  isBrandingPalette(value)
    ? (value as string).trim().toLowerCase() as BrandingPalette
    : DEFAULT_BRANDING_PALETTE;

export interface BrandingResponse {
  brandName: string;
  primaryColor?: BrandingPalette | null;
  hasMainLogo: boolean;
  hasSidebarLogo: boolean;
  version: string;
  mainLogoVersion?: string | null;
  sidebarLogoVersion?: string | null;
}

export interface BrandingUpdateRequest {
  brandName: string | null;
  primaryColor: BrandingPalette | null;
}

const apiBaseUrl = ((import.meta.env.VITE_API_BASE_URL as string) || "")
  .replace(/\/$/, "");

export class BrandingService {
  async getCurrent(): Promise<BrandingResponse> {
    const response = await apiClient.get<BrandingResponse>("/Branding/current");
    if (response.status !== 200) {
      throw new Error("Branding unavailable");
    }
    return response.data;
  }

  async updateCurrent(
    request: BrandingUpdateRequest,
  ): Promise<BrandingResponse> {
    const response = await apiClient.put<BrandingResponse>(
      "/Branding/current",
      request,
    );
    if (response.status !== 200) {
      throw new Error("Branding update failed");
    }
    return response.data;
  }

  async uploadCurrentLogo(
    slot: BrandingLogoSlot,
    file: globalThis.File,
  ): Promise<void> {
    const formData = new FormData();
    formData.append("file", file);

    const response = await apiClient.put(
      "/Branding/current/logo/" + slot,
      formData,
      { headers: { "Content-Type": "multipart/form-data" } },
    );
    if (response.status !== 200) {
      throw new Error("Logo upload failed");
    }
  }

  async removeCurrentLogo(slot: BrandingLogoSlot): Promise<void> {
    const response = await apiClient.delete("/Branding/current/logo/" + slot);
    if (response.status !== 200) {
      throw new Error("Logo removal failed");
    }
  }

  getCurrentLogoUrl(slot: BrandingLogoSlot, version: string): string {
    return apiBaseUrl + "/Branding/current/logo/" + slot +
      "?v=" + encodeURIComponent(version);
  }
}

export const brandingService = new BrandingService();
