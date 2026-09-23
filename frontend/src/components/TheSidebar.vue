<template>
  <!-- Phones get the same menu inside a drawer opened from the header; the
       fixed sidebar would take most of a 390px screen. -->
  <component
    :is="isPhone ? Drawer : Passthrough"
    v-bind="isPhone ? drawerProps : {}"
    v-model:visible="store.sidebar.mobileOpen"
  >
    <template #container>
      <sidebar-menu
        :menu="store.sidebar.menus"
        :collapsed="!isPhone && store.sidebar.collapsed"
        :showOneChild="true"
        :hideToggle="isPhone || store.sidebar.hideToggle"
        :relative="isPhone"
        :width="isPhone ? '100%' : undefined"
        :style="{ '--vsm-primary-color': sidebarAccent }"
        @update:collapsed="toggleCollapse"
      >
        <template #header>
          <div class="brand" @click="() => router.push({ path: '/' })">
            <!-- Only a logo uploaded for the sidebar is shown: tenants preview it on a
                 dark background. Main logos and the bundled default mark are made for
                 white backgrounds, so the monogram stands in for them. -->
            <img
              v-if="brandingStore.hasSidebarLogo && !logoLoadFailed"
              :src="brandingStore.sidebarLogoUrl"
              :alt="brandingStore.brandName"
              class="brand-logo"
              draggable="false"
              @error="logoLoadFailed = true"
            />
            <span v-else class="brand-monogram">{{ brandingStore.monogram }}</span>
            <span
              v-if="isPhone || !store.sidebar.collapsed"
              class="brand-name"
              :title="brandingStore.brandName"
              >{{ brandingStore.brandName }}</span
            >
          </div>
        </template>
        <template #footer>
          <div class="sidebar-footer">
            <Button
              :label="!isPhone && store.sidebar.collapsed ? '' : $t('support.request')"
              icon="pi pi-question-circle"
              severity="secondary"
              text
              class="support-btn"
              @click="openSupport"
            />
          </div>
        </template>
      </sidebar-menu>
    </template>
  </component>

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
import { computed, ref, watch, type FunctionalComponent } from "vue";
import Drawer from "primevue/drawer";
import { SidebarMenu } from "vue-sidebar-menu";
import "vue-sidebar-menu/dist/vue-sidebar-menu.css";
import { useIsPhone } from "@/composables/useIsPhone";
import { useStore } from "@/store";
import { useBrandingStore } from "@/store/branding";
import { useRoute, useRouter } from "vue-router";
import FormSupportRequest from "../modules/shared/components/FormSupportRequest.vue";

const router = useRouter();
const route = useRoute();
const store = useStore();
const isPhone = useIsPhone();

// Desktop renders the menu as is; the drawer's container slot is reused for it.
const Passthrough: FunctionalComponent = (_props, { slots }) => slots.container?.();
const drawerProps = {
  position: "left",
  blockScroll: true,
  class: "nav-drawer",
};

// Leaving the screen or widening the window closes the phone drawer.
watch([() => route.fullPath, isPhone], () => (store.sidebar.mobileOpen = false));

const showSupportDialog = ref(false);
const brandingStore = useBrandingStore();
const logoLoadFailed = ref(false);

function openSupport() {
  store.sidebar.mobileOpen = false;
  showSupportDialog.value = true;
}

// The black palette has no light shade to stand out on the dark sidebar.
const sidebarAccent = computed(() =>
  brandingStore.primaryColor === "black"
    ? "var(--p-surface-0)"
    : "var(--p-primary-400)",
);

watch(
  () => brandingStore.sidebarLogoUrl,
  () => (logoLoadFailed.value = false),
);

function toggleCollapse() {
  store.sidebar.collapsed = !store.sidebar.collapsed;
}
</script>

<style>
/* The drawer panel is teleported to <body>, so it is styled globally. */
.p-drawer.nav-drawer {
  width: min(18rem, 85vw);
  border: none;
  background: var(--p-steel-850);
}
</style>

<style scoped>
/*
 * Grafit sidebar: a neutral dark steel frame. The tenant's branding colour only
 * marks the current screen, so any of the branding palettes works here and the
 * white "sidebar logo" tenants upload stays readable.
 */
.v-sidebar-menu {
  width: var(--side-bar-width);

  --vsm-base-bg: var(--p-steel-850);
  --vsm-item-color: var(--p-steel-300);
  --vsm-item-active-color: var(--p-surface-0);
  --vsm-item-active-bg: transparent;
  --vsm-item-active-line-color: transparent;
  --vsm-item-open-color: var(--p-surface-0);
  --vsm-item-open-bg: transparent;
  --vsm-item-hover-color: var(--p-surface-0);
  --vsm-item-hover-bg: rgba(255, 255, 255, 0.06);
  --vsm-icon-color: var(--p-steel-400);
  --vsm-icon-bg: transparent;
  --vsm-icon-active-color: var(--p-surface-0);
  --vsm-icon-active-bg: transparent;
  --vsm-icon-open-color: var(--p-surface-0);
  --vsm-icon-open-bg: transparent;
  --vsm-dropdown-bg: var(--p-steel-800);
  --vsm-header-item-color: var(--p-steel-400);
  --vsm-toggle-btn-color: var(--p-steel-400);
  --vsm-toggle-btn-bg: var(--p-steel-850);
  --vsm-mobile-item-color: var(--p-surface-0);
  --vsm-mobile-item-bg: var(--p-steel-800);
  --vsm-mobile-icon-color: var(--p-surface-0);
  --vsm-mobile-icon-bg: transparent;

  --vsm-item-font-size: 1rem;
  --vsm-item-line-height: 22px;
  --vsm-item-padding: 7px 10px;
  --vsm-icon-height: 22px;
  --vsm-icon-width: 22px;
}

.v-sidebar-menu.vsm_relative {
  width: 100%;
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  height: var(--top-panel-height);
  box-sizing: border-box;
  padding: 0 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  color: var(--p-surface-0);
  cursor: pointer;
  white-space: nowrap;
}

.brand-logo {
  height: 32px;
  max-width: 140px;
  object-fit: contain;
}

.brand-name {
  font-family: var(--font-condensed);
  font-size: 1.2857rem;
  font-weight: 600;
  line-height: 1;
  overflow: hidden;
  text-overflow: ellipsis;
}

.brand-monogram {
  width: 28px;
  height: 28px;
  flex-shrink: 0;
  display: grid;
  place-items: center;
  border-radius: var(--p-border-radius-md);
  background: var(--vsm-primary-color);
  color: var(--p-steel-900);
  font-family: var(--font-condensed);
  font-weight: 600;
}

.sidebar-footer {
  padding: 0.625rem;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.support-btn {
  width: 100%;
  justify-content: flex-start;
  color: var(--p-steel-300);
}

.support-btn:hover {
  background-color: rgba(255, 255, 255, 0.06);
  color: var(--p-surface-0);
}

:global(.v-sidebar-menu .vsm--scroll-wrapper) {
  padding: 0.5rem 0.625rem;
}

:global(.v-sidebar-menu .vsm--link) {
  border-radius: var(--p-border-radius-md);
}

/* Expanded submenus sit on the sidebar itself; the dropdown colour is only
   for the flyout shown when the sidebar is collapsed. */
:global(.v-sidebar-menu.vsm_expanded .vsm--dropdown) {
  background-color: transparent;
}

:global(.v-sidebar-menu .vsm--link_level-1.vsm--link_open) {
  font-weight: 600;
}

/* Current screen: lighter row plus a small marker in the branding colour.
   vue-sidebar-menu also flags the open parent module as active; it stays plain. */
:global(.v-sidebar-menu .vsm--item .vsm--link.vsm--link_active:not(.vsm--link_open)) {
  background-color: rgba(255, 255, 255, 0.1);
  color: var(--p-surface-0);
  font-weight: 500;
}

:global(.v-sidebar-menu .vsm--link_level-2) {
  padding-left: 1.25rem;
}

/* Third level (module > group > screen): indented past the group's marker. */
:global(.v-sidebar-menu .vsm--link_level-3) {
  padding-left: calc(1.25rem + 16px);
}

:global(.v-sidebar-menu .vsm--link_level-2::before),
:global(.v-sidebar-menu .vsm--link_level-3::before) {
  content: "";
  flex-shrink: 0;
  width: 6px;
  height: 6px;
  margin-right: 10px;
  border-radius: 1px;
  background: transparent;
}

/* The marker follows the current screen at any depth; an open group holding it
   stays unmarked, a closed one is marked so the screen can still be found. */
:global(.v-sidebar-menu .vsm--item .vsm--link_level-2.vsm--link_active:not(.vsm--link_open)::before),
:global(.v-sidebar-menu .vsm--item .vsm--link_level-3.vsm--link_active::before) {
  background: var(--vsm-primary-color);
}

:global(.v-sidebar-menu .vsm--toggle-btn) {
  height: 44px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}
</style>
