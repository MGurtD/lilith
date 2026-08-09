<template>
  <sidebar-menu
    :menu="store.sidebar.menus"
    :collapsed="store.sidebar.collapsed"
    :showOneChild="true"
    :hideToggle="store.sidebar.hideToggle"
    :style="sidebarTheme"
    @update:collapsed="toggleCollapse"
  >
    <template #header>
      <div class="brand" @click="() => router.push({ path: '/' })">
        <img
          v-if="!logoLoadFailed"
          :src="brandingStore.sidebarLogoUrl"
          :alt="brandingStore.brandName"
          class="brand-logo"
          draggable="false"
          @error="logoLoadFailed = true"
        />
        <span v-else class="brand-monogram">{{ brandingStore.monogram }}</span>
        <span
          v-if="!store.sidebar.collapsed"
          class="brand-name"
          :title="brandingStore.brandName"
          >{{ brandingStore.brandName }}</span
        >
      </div>
    </template>
    <template #footer>
      <div class="sidebar-footer">
        <Button
          :label="store.sidebar.collapsed ? '' : $t('support.request')"
          icon="pi pi-question-circle"
          severity="secondary"
          text
          class="support-btn"
          @click="showSupportDialog = true"
        />
      </div>
    </template>
  </sidebar-menu>

  <Dialog
    v-model:visible="showSupportDialog"
    :header="$t('support.request')"
    :modal="true"
    :style="{ width: '480px' }"
    @hide="showSupportDialog = false"
  >
    <FormSupportRequest @close="showSupportDialog = false" />
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { SidebarMenu } from "vue-sidebar-menu";
import "vue-sidebar-menu/dist/vue-sidebar-menu.css";
import { useStore } from "@/store";
import { useBrandingStore } from "@/store/branding";
import {
  BRANDING_PALETTE_OPTIONS,
  DEFAULT_BRANDING_PALETTE,
} from "@/services/branding.service";
import { useRouter } from "vue-router";
import FormSupportRequest from "../modules/shared/components/FormSupportRequest.vue";

type Rgb = { r: number; g: number; b: number };

const BLACK = "#000000";
const WHITE = "#FFFFFF";
const LIGHT_TEXT = "#F8FAFC";

function hexToRgb(hex: string): Rgb {
  const normalized = hex.replace("#", "");
  return {
    r: parseInt(normalized.slice(0, 2), 16),
    g: parseInt(normalized.slice(2, 4), 16),
    b: parseInt(normalized.slice(4, 6), 16),
  };
}

function rgbToHex({ r, g, b }: Rgb): string {
  return (
    "#" +
    [r, g, b]
      .map((channel) => Math.round(channel).toString(16).padStart(2, "0"))
      .join("")
  ).toUpperCase();
}

function mixColors(first: string, second: string, amount: number): string {
  const firstRgb = hexToRgb(first);
  const secondRgb = hexToRgb(second);
  return rgbToHex({
    r: firstRgb.r + (secondRgb.r - firstRgb.r) * amount,
    g: firstRgb.g + (secondRgb.g - firstRgb.g) * amount,
    b: firstRgb.b + (secondRgb.b - firstRgb.b) * amount,
  });
}

function relativeLuminance(color: string): number {
  const channels = Object.values(hexToRgb(color)).map((channel) => {
    const normalized = channel / 255;
    return normalized <= 0.03928
      ? normalized / 12.92
      : ((normalized + 0.055) / 1.055) ** 2.4;
  });

  return 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2];
}

function contrastRatio(first: string, second: string): number {
  const firstLuminance = relativeLuminance(first);
  const secondLuminance = relativeLuminance(second);
  const lighter = Math.max(firstLuminance, secondLuminance);
  const darker = Math.min(firstLuminance, secondLuminance);
  return (lighter + 0.05) / (darker + 0.05);
}

function adjustBackgroundForContrast(
  background: string,
  foreground: string,
  minimumRatio: number,
): string {
  if (contrastRatio(background, foreground) >= minimumRatio) {
    return background;
  }

  const candidates = [background];
  for (let step = 1; step <= 100; step += 1) {
    const amount = step / 100;
    candidates.push(mixColors(background, BLACK, amount));
    candidates.push(mixColors(background, WHITE, amount));
  }

  const accessibleCandidates = candidates.filter(
    (candidate) => contrastRatio(candidate, foreground) >= minimumRatio,
  );

  return (accessibleCandidates.length ? accessibleCandidates : candidates).reduce(
    (best, candidate) => {
      if (!best) return candidate;
      return colorDistance(candidate, background) <
        colorDistance(best, background)
        ? candidate
        : best;
    },
    "",
  );
}

function colorDistance(first: string, second: string): number {
  const firstRgb = hexToRgb(first);
  const secondRgb = hexToRgb(second);
  return Math.sqrt(
    (firstRgb.r - secondRgb.r) ** 2 +
      (firstRgb.g - secondRgb.g) ** 2 +
      (firstRgb.b - secondRgb.b) ** 2,
  );
}

function findAccessibleColor(
  seed: string,
  backgrounds: string[],
  minimumRatio: number,
): string {
  const candidates = [seed];
  for (let step = 1; step <= 100; step += 1) {
    const amount = step / 100;
    candidates.push(mixColors(seed, WHITE, amount));
    candidates.push(mixColors(seed, BLACK, amount));
  }

  const accessibleCandidates = candidates.filter((candidate) =>
    backgrounds.every(
      (background) => contrastRatio(candidate, background) >= minimumRatio,
    ),
  );

  return (accessibleCandidates.length ? accessibleCandidates : candidates).reduce(
    (best, candidate) => {
      if (!best) return candidate;
      return colorDistance(candidate, seed) < colorDistance(best, seed)
        ? candidate
        : best;
    },
    "",
  );
}

function getReadableText(background: string): string {
  return findAccessibleColor(LIGHT_TEXT, [background], 4.5);
}

function getPaletteOption(
  primaryColor: unknown,
): (typeof BRANDING_PALETTE_OPTIONS)[number] {
  const normalized =
    typeof primaryColor === "string"
      ? primaryColor.trim().toLowerCase()
      : DEFAULT_BRANDING_PALETTE;

  return (
    BRANDING_PALETTE_OPTIONS.find((option) => option.value === normalized) ??
    BRANDING_PALETTE_OPTIONS.find(
      (option) => option.value === DEFAULT_BRANDING_PALETTE,
    )!
  );
}

function buildSidebarTheme(primaryColor: unknown): Record<string, string> {
  const paletteOption = getPaletteOption(primaryColor);
  const paletteColor = paletteOption.swatch;
  const darkPalette = mixColors(paletteColor, BLACK, 0.8);
  const createSurface = (paletteAmount: number, neutralLift: number) =>
    adjustBackgroundForContrast(
      mixColors(
        mixColors(darkPalette, paletteColor, paletteAmount),
        WHITE,
        neutralLift,
      ),
      LIGHT_TEXT,
      4.5,
    );

  const createGrayscaleSurface = (gray: string) =>
    adjustBackgroundForContrast(gray, LIGHT_TEXT, 4.5);
  const surfaces =
    paletteOption.value === "black"
      ? {
          // Keep each navigation level distinct while validating every rung for text contrast.
          base: createGrayscaleSurface("#101010"),
          dropdown: createGrayscaleSurface("#1C1C1C"),
          submenu: createGrayscaleSurface("#2C2C2C"),
          hover: createGrayscaleSurface("#3C3C3C"),
          submenuHover: createGrayscaleSurface("#4C4C4C"),
          selected: createGrayscaleSurface("#585858"),
          submenuSelected: createGrayscaleSurface("#646464"),
          active: createGrayscaleSurface("#707070"),
        }
      : {
          base: createSurface(0.04, 0.03),
          dropdown: createSurface(0.08, 0.05),
          submenu: createSurface(0.16, 0.08),
          hover: createSurface(0.2, 0.08),
          submenuHover: createSurface(0.28, 0.12),
          selected: createSurface(0.34, 0.13),
          submenuSelected: createSurface(0.4, 0.16),
          active: createSurface(0.48, 0.18),
        };
  const {
    base,
    dropdown,
    submenu,
    hover,
    submenuHover,
    selected,
    submenuSelected,
    active,
  } = surfaces;
  const createIconSurface = (surface: string) =>
    adjustBackgroundForContrast(mixColors(surface, WHITE, 0.08), LIGHT_TEXT, 3);
  const primarySurfaces =
    paletteOption.value === "black"
      ? {
          base: createGrayscaleSurface("#101010"),
          hover: createGrayscaleSurface("#3C3C3C"),
          selected: createGrayscaleSurface("#585858"),
          active: createGrayscaleSurface("#707070"),
        }
      : {
          base: createSurface(0.18, 0.04),
          hover: createSurface(0.3, 0.1),
          selected: createSurface(0.42, 0.14),
          active: createSurface(0.54, 0.18),
        };
  const primaryIcon = createIconSurface(primarySurfaces.base);
  const primaryHoverIcon = createIconSurface(primarySurfaces.hover);
  const primarySelectedIcon = createIconSurface(primarySurfaces.selected);
  const primaryActiveIcon = createIconSurface(primarySurfaces.active);
  const icon = createIconSurface(base);
  const hoverIcon = createIconSurface(hover);
  const selectedIcon = createIconSurface(selected);
  const activeIcon = createIconSurface(active);
  const surfaceValues = [
    base,
    dropdown,
    submenu,
    hover,
    submenuHover,
    selected,
    submenuSelected,
    active,
  ];
  const linkColor = findAccessibleColor(LIGHT_TEXT, [base, dropdown], 4.5);
  const iconColor = findAccessibleColor(LIGHT_TEXT, [icon], 3);
  const accent = findAccessibleColor(
    paletteColor,
    paletteOption.value === "black"
      ? surfaceValues
      : [...surfaceValues, icon, hoverIcon, selectedIcon, activeIcon],
    3,
  );

  return {
    "--sidebar-base-bg": "var(--p-primary-900)",
    "--sidebar-primary-color": getReadableText(primarySurfaces.base),
    "--sidebar-primary-hover-bg": primarySurfaces.hover,
    "--sidebar-primary-hover-color": getReadableText(primarySurfaces.hover),
    "--sidebar-primary-selected-bg": primarySurfaces.selected,
    "--sidebar-primary-selected-color": getReadableText(primarySurfaces.selected),
    "--sidebar-primary-active-bg": primarySurfaces.active,
    "--sidebar-primary-active-color": getReadableText(primarySurfaces.active),
    "--sidebar-primary-icon-bg": primaryIcon,
    "--sidebar-primary-icon-color": getReadableText(primaryIcon),
    "--sidebar-primary-hover-icon-bg": primaryHoverIcon,
    "--sidebar-primary-hover-icon-color": getReadableText(primaryHoverIcon),
    "--sidebar-primary-selected-icon-bg": primarySelectedIcon,
    "--sidebar-primary-selected-icon-color": getReadableText(primarySelectedIcon),
    "--sidebar-primary-active-icon-bg": primaryActiveIcon,
    "--sidebar-primary-active-icon-color": getReadableText(primaryActiveIcon),
    "--sidebar-link-color": linkColor,
    "--sidebar-icon-bg": icon,
    "--sidebar-icon-color": iconColor,
    "--sidebar-active-bg": active,
    "--sidebar-active-color": getReadableText(active),
    "--sidebar-active-icon-bg": activeIcon,
    "--sidebar-active-icon-color": findAccessibleColor(accent, [activeIcon], 3),
    "--vsm-item-active-line-color": accent,
    "--sidebar-selected-bg": selected,
    "--sidebar-selected-color": getReadableText(selected),
    "--sidebar-selected-icon-bg": selectedIcon,
    "--sidebar-selected-icon-color": getReadableText(selectedIcon),
    "--sidebar-hover-bg": hover,
    "--sidebar-hover-color": getReadableText(hover),
    "--sidebar-hover-icon-bg": hoverIcon,
    "--sidebar-hover-icon-color": getReadableText(hoverIcon),
    "--sidebar-dropdown-bg": dropdown,
    "--sidebar-submenu-bg": submenu,
    "--sidebar-submenu-color": getReadableText(submenu),
    "--sidebar-submenu-hover-bg": submenuHover,
    "--sidebar-submenu-hover-color": getReadableText(submenuHover),
    "--sidebar-submenu-selected-bg": submenuSelected,
    "--sidebar-submenu-selected-color": getReadableText(submenuSelected),
    "--sidebar-mobile-bg": "var(--p-primary-900)",
    "--sidebar-muted-color": linkColor,
    "--sidebar-accent": accent,
  };
}

const router = useRouter();
const store = useStore();
const showSupportDialog = ref(false);
const brandingStore = useBrandingStore();
const logoLoadFailed = ref(false);
const sidebarTheme = computed(() =>
  buildSidebarTheme(brandingStore.primaryColor),
);

watch(
  () => brandingStore.sidebarLogoUrl,
  () => (logoLoadFailed.value = false),
);

function toggleCollapse() {
  store.sidebar.collapsed = !store.sidebar.collapsed;
}
</script>

<style scoped>
.v-sidebar-menu {
  background-color: var(--sidebar-base-bg);
  width: var(--side-bar-width);

  /* Item height & icon size */
  --vsm-item-line-height: 22px;
  --vsm-item-padding: 7px 12px;
  --vsm-icon-height: 28px;
  --vsm-icon-width: 28px;
  --vsm-item-font-size: 14px;
}

.v-sidebar-menu .vsm--header {
  text-align: left;
  border-bottom: 1px solid var(--sidebar-hover-bg);
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  background: transparent;
  border: none;
  color: var(--sidebar-link-color);
  font-weight: 600;
  font-size: 1.4rem;
  letter-spacing: 0.5px;
  cursor: pointer;
  padding: 0.75rem;
  font-family: "Segoe UI", system-ui, sans-serif;
  text-transform: uppercase;
  white-space: nowrap;
}

.brand-logo {
  height: 40px;
  object-fit: contain;
  filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.4));
  transition: transform 0.25s ease;
}

.brand-name {
  line-height: 1;
}

.brand-monogram {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border: 2px solid var(--sidebar-accent);
  border-radius: 50%;
}

.sidebar-footer {
  padding: 0.65rem 0.85rem;
  color: var(--sidebar-muted-color);
  display: flex;
  justify-content: center;
  font-size: 0.7rem;
  letter-spacing: 1px;
  text-transform: uppercase;
}

.support-btn {
  width: 100%;
  color: var(--sidebar-muted-color) !important;
  justify-content: flex-start;
  font-size: 0.75rem;
  letter-spacing: 0.5px;
  text-transform: uppercase;
}

.support-btn:hover {
  background-color: var(--sidebar-hover-bg) !important;
  color: var(--sidebar-hover-color) !important;
}

:global(.v-sidebar-menu .vsm--link) {
  color: var(--sidebar-link-color) !important;
}

:global(.v-sidebar-menu .vsm--link_hover),
:global(.v-sidebar-menu .vsm--link:hover) {
  background-color: var(--sidebar-hover-bg) !important;
  color: var(--sidebar-hover-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-2:not(.vsm--link_active):not(.vsm--link_exact-active)) {
  background-color: var(--sidebar-submenu-bg) !important;
  color: var(--sidebar-submenu-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-2.vsm--link_hover:not(.vsm--link_active):not(.vsm--link_exact-active)),
:global(.v-sidebar-menu .vsm--link_level-2:hover:not(.vsm--link_active):not(.vsm--link_exact-active)) {
  background-color: var(--sidebar-submenu-hover-bg) !important;
  color: var(--sidebar-submenu-hover-color) !important;
}

:global(.v-sidebar-menu.vsm_expanded .vsm--item.vsm--item_open > .vsm--link) {
  background-color: var(--sidebar-selected-bg) !important;
  color: var(--sidebar-selected-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1 .vsm--icon) {
  background-color: var(--sidebar-icon-bg) !important;
  color: var(--sidebar-icon-color) !important;
}

:global(.v-sidebar-menu .vsm--link_hover .vsm--icon),
:global(.v-sidebar-menu .vsm--link:hover .vsm--icon) {
  background-color: var(--sidebar-hover-icon-bg) !important;
  color: var(--sidebar-hover-icon-color) !important;
}

:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1.vsm--link_hover),
:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1:hover) {
  background-color: transparent !important;
}

:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1.vsm--link_hover .vsm--icon),
:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1:hover .vsm--icon) {
  background-color: var(--sidebar-hover-icon-bg) !important;
  color: var(--sidebar-hover-icon-color) !important;
}

:global(.v-sidebar-menu .vsm--link_active:not(.vsm--link_exact-active)) {
  background-color: var(--sidebar-selected-bg) !important;
  color: var(--sidebar-selected-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-2.vsm--link_active:not(.vsm--link_exact-active)) {
  background-color: var(--sidebar-submenu-selected-bg) !important;
  color: var(--sidebar-submenu-selected-color) !important;
}

:global(.v-sidebar-menu .vsm--link_exact-active) {
  background-color: var(--sidebar-active-bg) !important;
  color: var(--sidebar-active-color) !important;
  box-shadow: inset 3px 0 0 0 var(--sidebar-accent) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_active) {
  box-shadow: 3px 0 0 0 var(--sidebar-accent) inset !important;
}

:global(.v-sidebar-menu .vsm--link_active:not(.vsm--link_exact-active) .vsm--icon) {
  background-color: var(--sidebar-selected-icon-bg) !important;
  color: var(--sidebar-selected-icon-color) !important;
}

:global(.v-sidebar-menu.vsm_collapsed .vsm--link_active),
:global(.v-sidebar-menu.vsm_collapsed .vsm--link_exact-active) {
  background-color: transparent !important;
}

:global(.v-sidebar-menu .vsm--link_exact-active .vsm--icon) {
  background-color: var(--sidebar-active-icon-bg) !important;
  color: var(--sidebar-active-icon-color) !important;
}

:global(.v-sidebar-menu .vsm--item.vsm--item_open > .vsm--link .vsm--icon) {
  background-color: var(--sidebar-selected-icon-bg) !important;
  color: var(--sidebar-selected-icon-color) !important;
}

:global(.v-sidebar-menu .vsm--dropdown) {
  background-color: var(--sidebar-dropdown-bg) !important;
}

:global(.v-sidebar-menu .vsm--mobile-bg) {
  background-color: var(--sidebar-mobile-bg) !important;
}

:global(.v-sidebar-menu .vsm--mobile-item),
:global(.v-sidebar-menu .vsm--link_mobile) {
  background-color: var(--sidebar-mobile-bg) !important;
  color: var(--sidebar-link-color) !important;
}

:global(.v-sidebar-menu .vsm--link_mobile:hover),
:global(.v-sidebar-menu .vsm--mobile-item:hover) {
  background-color: var(--sidebar-hover-bg) !important;
  color: var(--sidebar-hover-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1) {
  background-color: var(--sidebar-base-bg) !important;
  color: var(--sidebar-primary-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_hover),
:global(.v-sidebar-menu .vsm--link_level-1:hover) {
  background-color: var(--sidebar-primary-hover-bg) !important;
  color: var(--sidebar-primary-hover-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_active:not(.vsm--link_exact-active)),
:global(.v-sidebar-menu.vsm_expanded .vsm--item.vsm--item_open > .vsm--link_level-1) {
  background-color: var(--sidebar-primary-selected-bg) !important;
  color: var(--sidebar-primary-selected-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_exact-active) {
  background-color: var(--sidebar-primary-active-bg) !important;
  color: var(--sidebar-primary-active-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1 .vsm--icon) {
  background-color: var(--sidebar-primary-icon-bg) !important;
  color: var(--sidebar-primary-icon-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_hover .vsm--icon),
:global(.v-sidebar-menu .vsm--link_level-1:hover .vsm--icon) {
  background-color: var(--sidebar-primary-hover-icon-bg) !important;
  color: var(--sidebar-primary-hover-icon-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_active:not(.vsm--link_exact-active) .vsm--icon),
:global(.v-sidebar-menu.vsm_expanded .vsm--item.vsm--item_open > .vsm--link_level-1 .vsm--icon) {
  background-color: var(--sidebar-primary-selected-icon-bg) !important;
  color: var(--sidebar-primary-selected-icon-color) !important;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_exact-active .vsm--icon) {
  background-color: var(--sidebar-primary-active-icon-bg) !important;
  color: var(--sidebar-primary-active-icon-color) !important;
}

:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1.vsm--link_hover),
:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1:hover),
:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1.vsm--link_active),
:global(.v-sidebar-menu.vsm_collapsed .vsm--link_level-1.vsm--link_exact-active) {
  background-color: transparent !important;
}
</style>
