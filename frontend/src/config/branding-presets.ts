import { definePreset, usePreset } from "@primeuix/themes";
import Lara from "@primeuix/themes/lara";
import Aura from "@primeuix/themes/aura";
import Material from "@primeuix/themes/material";
import Nora from "@primeuix/themes/nora";
import type { Branding } from "@/types/branding";

type ThemePresetName = "lara" | "aura" | "material" | "nora";

const PRESETS: Record<ThemePresetName, unknown> = {
  lara: Lara,
  aura: Aura,
  material: Material,
  nora: Nora,
};

function resolveThemeName(theme: string | null): ThemePresetName | null {
  if (!theme) return null;
  const lower = theme.toLowerCase();
  if (lower === "lara" || lower === "aura" || lower === "material" || lower === "nora") {
    return lower;
  }
  return null;
}

export function buildPreset(branding: Branding) {
  const name = resolveThemeName(branding.theme);
  const base = name ? (PRESETS[name] as Parameters<typeof definePreset>[0]) : Lara;

  if (!branding.primaryColor) {
    return definePreset(base, {});
  }

  return definePreset(base, {
    semantic: {
      primary: branding.primaryColor,
    },
  });
}

export function applyBrandingPreset(branding: Branding) {
  const resolved = resolveThemeName(branding.theme);

  if (resolved && resolved !== "lara") {
    const base = PRESETS[resolved] as Parameters<typeof usePreset>[0];
    const preset = branding.primaryColor
      ? definePreset(base, { semantic: { primary: branding.primaryColor } })
      : base;
    usePreset(preset);
    return;
  }

  const base = PRESETS.lara as Parameters<typeof usePreset>[0];
  const preset = branding.primaryColor
    ? definePreset(base, { semantic: { primary: branding.primaryColor } })
    : base;
  usePreset(preset);
}