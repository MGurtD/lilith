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
        :collapsed="rail"
        :showOneChild="true"
        :hideToggle="true"
        :relative="isPhone"
        :width="isPhone ? '100%' : undefined"
        :style="{ '--vsm-primary-color': sidebarAccent }"
      >
        <template #header>
          <div class="brand-row" :class="{ 'brand-row--rail': rail }">
            <RouterLink to="/" class="brand" :aria-label="brandingStore.brandName">
              <!-- Only a logo uploaded for the sidebar is shown: tenants preview it on a
                   dark background. Main logos and the bundled default mark are made for
                   white backgrounds, so the monogram stands in for them. -->
              <img
                v-if="brandingStore.hasSidebarLogo && !logoLoadFailed"
                :src="brandingStore.sidebarLogoUrl"
                alt=""
                class="brand-logo"
                draggable="false"
                @error="logoLoadFailed = true"
              />
              <span v-else class="brand-monogram">{{ brandingStore.monogram }}</span>
              <span v-if="!rail" class="brand-name" :title="brandingStore.brandName">{{
                brandingStore.brandName
              }}</span>
            </RouterLink>
            <button
              v-if="canToggle && !rail"
              type="button"
              class="sidebar-icon-button"
              :aria-label="t('ui.collapseMenu')"
              v-tooltip.right="t('ui.collapseMenu')"
              @click="store.sidebar.collapsed = true"
            >
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <rect x="3" y="4" width="18" height="16" rx="2" />
                <path d="M9 4v16M16 10l-2 2 2 2" />
              </svg>
            </button>
          </div>
          <!-- Collapsed: the expand control is the first row of the rail. -->
          <div v-if="canToggle && rail" class="rail-toggle">
            <button
              type="button"
              class="sidebar-icon-button"
              :aria-label="t('ui.expandMenu')"
              v-tooltip.right="t('ui.expandMenu')"
              @click="store.sidebar.collapsed = false"
            >
              <svg viewBox="0 0 24 24" aria-hidden="true">
                <rect x="3" y="4" width="18" height="16" rx="2" />
                <path d="M9 4v16M14 10l2 2-2 2" />
              </svg>
            </button>
          </div>
        </template>
        <template #footer>
          <div class="sidebar-footer" :class="{ 'sidebar-footer--rail': rail }">
            <Button
              :label="rail ? undefined : t('support.request')"
              :aria-label="t('support.request')"
              icon="pi pi-question-circle"
              severity="secondary"
              text
              class="support-btn"
              @click="openSupport"
            />
            <button
              v-if="account"
              type="button"
              class="user-button"
              :class="{ 'user-button--open': userMenuOpen }"
              :aria-label="`${t('ui.userMenu')}: ${account.name}`"
              aria-haspopup="true"
              :aria-expanded="userMenuOpen"
              @click="toggleUserMenu"
            >
              <Avatar
                :label="account.initial"
                shape="circle"
                class="user-avatar"
                :class="{ 'user-avatar--operator': account.operator }"
              />
              <template v-if="!rail">
                <span class="user-text">
                  <span class="user-name">{{ account.name }}</span>
                  <span class="user-detail">{{ account.detail }}</span>
                </span>
                <svg class="user-chevron" viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M8 9l4-4 4 4M8 15l4 4 4-4" />
                </svg>
              </template>
            </button>
          </div>
        </template>
      </sidebar-menu>
    </template>
  </component>

  <Popover ref="userMenu" @show="userMenuOpen = true" @hide="userMenuOpen = false">
    <div v-if="account" class="user-menu">
      <div class="user-menu__header">
        <Avatar
          :label="account.initial"
          size="large"
          shape="circle"
          class="user-menu__avatar"
          :class="{ 'user-avatar--operator': account.operator }"
        />
        <div class="user-menu__name">{{ account.name }}</div>
        <div class="user-menu__detail">
          <i :class="account.operator ? 'pi pi-user' : 'pi pi-shield'" class="mr-1"></i>
          {{ account.detail }}
        </div>
      </div>

      <template v-if="account.operator">
        <div class="user-menu__divider" />
        <!-- Shop-floor tablets: a large touch target to leave. -->
        <Button
          icon="pi pi-sign-out"
          :label="t('ui.exit')"
          class="w-full"
          size="large"
          @click="leave('logoutOperatorClick')"
        />
      </template>
      <template v-else-if="store.user">
        <div class="user-menu__divider" />
        <div class="user-menu__section">
          <label class="user-menu__label">{{ t("ui.language") }}</label>
          <LanguageSwitcher
            v-model="store.user.preferredLanguage"
            :changeAppLanguage="true"
          />
        </div>
        <div class="user-menu__divider" />
        <Button
          icon="pi pi-sign-out"
          :label="t('ui.signOut')"
          severity="secondary"
          text
          class="user-menu__item"
          @click="leave('logoutClick')"
        />
      </template>
    </div>
  </Popover>

  <Dialog
    v-model:visible="showSupportDialog"
    :header="t('support.request')"
    :modal="true"
    :style="{ width: '480px' }"
    @hide="showSupportDialog = false"
  >
    <FormSupportRequest
      :loading="supportStore.isSubmitting"
      @submit="submitSupportRequest"
      @cancel="showSupportDialog = false"
    />
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch, type FunctionalComponent } from "vue";
import Avatar from "primevue/avatar";
import Drawer from "primevue/drawer";
import Popover from "primevue/popover";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";
import { useRoute } from "vue-router";
import { SidebarMenu } from "vue-sidebar-menu";
import "vue-sidebar-menu/dist/vue-sidebar-menu.css";
import LanguageSwitcher from "@/components/LanguageSwitcher.vue";
import { useIsPhone } from "@/composables/useIsPhone";
import { usePlantOperatorStore } from "@/modules/plant/store";
import { useStore } from "@/store";
import { useBrandingStore } from "@/store/branding";
import FormSupportRequest from "../modules/shared/components/FormSupportRequest.vue";
import { useSupportStore } from "../modules/shared/store/support";

const emits = defineEmits(["logoutClick", "logoutOperatorClick"]);

const { t } = useI18n();
const route = useRoute();
const store = useStore();
const brandingStore = useBrandingStore();
const plantOperatorStore = usePlantOperatorStore();
const isPhone = useIsPhone();

// Desktop renders the menu as is; the drawer's container slot is reused for it.
const Passthrough: FunctionalComponent = (_props, { slots }) => slots.container?.();
const drawerProps = {
  position: "left",
  blockScroll: true,
  class: "nav-drawer",
};

// The phone drawer is always expanded; desktop follows the collapsed setting.
const rail = computed(() => !isPhone.value && store.sidebar.collapsed);
// Plant screens with an operator keep the sidebar collapsed (hideToggle).
const canToggle = computed(() => !isPhone.value && !store.sidebar.hideToggle);

// Leaving the screen or widening the window closes the phone drawer.
watch([() => route.fullPath, isPhone], () => (store.sidebar.mobileOpen = false));

const showSupportDialog = ref(false);
const logoLoadFailed = ref(false);

function openSupport() {
  store.sidebar.mobileOpen = false;
  showSupportDialog.value = true;
}

const supportStore = useSupportStore();
const toast = useToast();

// On failure the dialog stays open so the request can be sent again.
async function submitSupportRequest(request: {
  resum: string;
  descripcio: string;
}) {
  const result = await supportStore.submit(request.resum, request.descripcio);

  if (result.ok) {
    toast.add({
      severity: "success",
      summary: t("shared.supportRequest.messages.sent"),
      detail: t("shared.supportRequest.messages.sentDetail"),
      life: 5000,
    });
    showSupportDialog.value = false;
  } else {
    toast.add({
      severity: "error",
      summary: t("shared.supportRequest.messages.error"),
      detail: result.error,
      life: 8000,
    });
  }
}

// Who is signed in: the plant operator once one has clocked in, else the user.
const account = computed(() => {
  const operator = plantOperatorStore.operator;
  if (operator) {
    return {
      operator: true,
      initial: operator.name.substring(0, 1).toUpperCase(),
      name: `${operator.name} ${operator.surname}`,
      detail: t("ui.operator"),
    };
  }
  const user = store.user;
  if (!user) return undefined;
  const fullName = `${user.firstName ?? ""} ${user.lastName ?? ""}`.trim();
  return {
    operator: false,
    initial: user.username.substring(0, 1).toUpperCase(),
    name: fullName || user.username,
    detail: `@${user.username}`,
  };
});

const userMenu = ref<InstanceType<typeof Popover>>();
const userMenuOpen = ref(false);
const toggleUserMenu = (event: Event) => userMenu.value?.toggle(event);
const leave = (event: "logoutClick" | "logoutOperatorClick") => {
  userMenu.value?.hide();
  emits(event);
};

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

.brand-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  height: var(--top-panel-height);
  box-sizing: border-box;
  padding: 0 0.5rem 0 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.brand-row--rail {
  justify-content: center;
  padding: 0;
}

.brand {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 0.625rem;
  border-radius: var(--p-border-radius-md);
  color: var(--p-surface-0);
  text-decoration: none;
  white-space: nowrap;
}

.brand-row--rail .brand {
  flex: none;
}

.brand:focus-visible {
  outline: 2px solid var(--p-surface-0);
  outline-offset: 2px;
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

.sidebar-icon-button {
  all: unset;
  box-sizing: border-box;
  width: 32px;
  height: 32px;
  flex-shrink: 0;
  display: grid;
  place-items: center;
  border-radius: var(--p-border-radius-md);
  color: var(--p-steel-400);
  cursor: pointer;
}

.sidebar-icon-button:hover {
  background-color: rgba(255, 255, 255, 0.08);
  color: var(--p-surface-0);
}

.sidebar-icon-button:focus-visible {
  outline: 2px solid var(--p-surface-0);
  outline-offset: -2px;
}

.sidebar-icon-button svg,
.user-chevron {
  width: 20px;
  height: 20px;
  fill: none;
  stroke: currentColor;
  stroke-width: 1.75;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.rail-toggle {
  display: flex;
  justify-content: center;
  padding-top: 0.5rem;
}

.sidebar-footer {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0.5rem 0.625rem;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.sidebar-footer--rail {
  align-items: center;
  padding: 0.5rem 0;
}

.support-btn {
  width: 100%;
  justify-content: flex-start;
  color: var(--p-steel-300);
}

.sidebar-footer--rail .support-btn {
  width: auto;
}

.support-btn:hover {
  background-color: rgba(255, 255, 255, 0.06);
  color: var(--p-surface-0);
}

/* Signed-in user: identity at the foot of the sidebar, menu opens above it. */
.user-button {
  all: unset;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  gap: 0.625rem;
  width: 100%;
  min-height: 3.25rem;
  padding: 0.5rem;
  border-radius: var(--p-border-radius-md);
  color: var(--p-surface-0);
  cursor: pointer;
}

.sidebar-footer--rail .user-button {
  width: auto;
  min-height: 0;
  padding: 0.25rem;
}

.user-button:hover,
.user-button--open {
  background-color: rgba(255, 255, 255, 0.08);
}

.user-button:focus-visible {
  outline: 2px solid var(--p-surface-0);
  outline-offset: -2px;
}

.user-avatar {
  width: 2.25rem;
  height: 2.25rem;
  flex-shrink: 0;
  background: var(--p-steel-700);
  color: var(--p-surface-0);
  font-family: var(--font-condensed);
  font-weight: 600;
}

.user-avatar--operator,
.user-menu__avatar.user-avatar--operator {
  background: var(--p-primary-100);
  color: var(--p-primary-800);
}

.user-text {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.user-name {
  font-weight: 500;
  line-height: 1.3;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-detail {
  font-size: 0.8571rem;
  line-height: 1.3;
  color: var(--p-steel-400);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-chevron {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
  stroke-width: 2;
  color: var(--p-steel-400);
}

/* User menu (popover) */
.user-menu {
  min-width: 16rem;
  padding: 0.25rem;
}

.user-menu__header {
  display: grid;
  grid-template-columns: 3rem 1fr;
  grid-template-rows: auto auto;
  column-gap: 0.75rem;
  align-items: center;
}

.user-menu__avatar {
  grid-row: span 2;
  background: var(--p-surface-100);
  color: var(--p-surface-700);
  font-family: var(--font-condensed);
  font-weight: 600;
}

.user-menu__name {
  font-weight: 600;
  color: var(--p-text-color);
}

.user-menu__detail {
  display: flex;
  align-items: center;
  font-size: 0.9286rem;
  color: var(--p-text-muted-color);
}

.user-menu__divider {
  height: 1px;
  background: var(--p-surface-200);
  margin: 0.75rem 0;
}

.user-menu__section {
  display: grid;
  gap: 0.5rem;
}

.user-menu__label {
  font-family: var(--font-condensed);
  font-size: 0.9286rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.user-menu .user-menu__item {
  width: 100%;
  justify-content: flex-start;
  color: var(--p-text-color);
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
</style>
