<script setup lang="ts">
import { computed, onMounted, onUnmounted, watch } from "vue";
import ScrollPanel from "primevue/scrollpanel";
import { applyPrimeVueLocale } from "./i18n";
import { usePrimeVue } from "primevue/config";
import { useRoute, useRouter } from "vue-router";
import HelpDrawer from "@/components/help/HelpDrawer.vue";
import MenuSearchDialog from "@/components/menu-search/MenuSearchDialog.vue";
import Header from "@/components/TheHeader.vue";
import PwaUpdatePrompt from "@/components/PwaUpdatePrompt.vue";
import SideBar from "@/components/TheSidebar.vue";
import PlantHeader from "@/modules/plant/components/PlantHeader.vue";
import { usePlantOperatorStore } from "@/modules/plant/store";
import { useStore } from "@/store";
import { useApiStore } from "@/store/backend";
import { useSpanishGeography } from "@/store/geography";
import { useHelpStore } from "@/store/help";
import { useMenuSearchStore } from "@/store/menuSearch";
import { findOwningEntry } from "@/utils/menuSearch";
import Login from "@/views/Login.vue";

const store = useStore();
const plantOperatorStore = usePlantOperatorStore();
const apiStore = useApiStore();
const spanishGeography = useSpanishGeography();
const helpStore = useHelpStore();
const menuSearch = useMenuSearchStore();
const route = useRoute();
const router = useRouter();
const primevue = usePrimeVue();

const resolveRouteHelpKey = (): string | undefined => {
  return typeof route.meta.helpKey === "string" ? route.meta.helpKey : undefined;
};

const isEditableTarget = (target: EventTarget | null): boolean => {
  const element = target instanceof HTMLElement ? target : null;
  if (!element) {
    return false;
  }

  const tagName = element.tagName;
  return (
    element.isContentEditable ||
    element.closest("[contenteditable='true']") !== null ||
    tagName === "INPUT" ||
    tagName === "TEXTAREA" ||
    tagName === "SELECT"
  );
};

const handleHelpShortcut = (event: KeyboardEvent) => {
  if (!store.authorization) {
    return;
  }

  if (
    !event.altKey ||
    event.ctrlKey ||
    event.metaKey ||
    event.shiftKey ||
    event.key.toLowerCase() !== "h" ||
    isEditableTarget(event.target)
  ) {
    return;
  }

  event.preventDefault();
  void helpStore.toggleForRoute(resolveRouteHelpKey());
};

// Ctrl/⌘+K also works while typing in a field: it is the only shortcut that
// takes over editable targets, as in most command palettes.
const handleMenuSearchShortcut = (event: KeyboardEvent) => {
  if (
    !store.authorization ||
    !(event.ctrlKey || event.metaKey) ||
    event.altKey ||
    event.shiftKey ||
    event.key.toLowerCase() !== "k"
  ) {
    return;
  }

  event.preventDefault();
  menuSearch.toggle();
};

onMounted(async () => {
  window.addEventListener("keydown", handleHelpShortcut);
  window.addEventListener("keydown", handleMenuSearchShortcut);
  spanishGeography.fetch();

  // Initialize language for anonymous users; JWT-based locale will be handled in setAuthorization
  await store.initLanguage();
  await store.getAuthorization();
});

onUnmounted(() => {
  window.removeEventListener("keydown", handleHelpShortcut);
  window.removeEventListener("keydown", handleMenuSearchShortcut);
});

// Recent screens are per user; every visit to a menu screen counts, whether it
// was opened from the searcher or the sidebar. Detail routes count as their list.
watch(
  () => store.user?.id,
  (userId) => menuSearch.loadRecents(userId),
  { immediate: true },
);

watch(
  [() => route.path, () => menuSearch.entries],
  ([path, entries]) => {
    const entry = findOwningEntry(entries, path);
    if (entry) menuSearch.recordVisit(entry.href);
  },
  { immediate: true },
);

watch(
  () => store.language.current,
  (val) => applyPrimeVueLocale(primevue.config, val),
);

watch(
  () => route.fullPath,
  () => {
    if (helpStore.visible) {
      void helpStore.openForRoute(resolveRouteHelpKey());
    }
  },
);

// Plant screens run on shop-floor tablets: keep the 16px root there so touch
// targets are not scaled down with the denser 14px office UI.
watch(
  () => route.path.startsWith("/plant"),
  (isPlant) => document.documentElement.classList.toggle("plant-mode", isPlant),
  { immediate: true },
);

// A clocked-in operator gets the shop-floor shell: no office sidebar, a
// plant header with shift, clock and the operator's menu. Clock-in keeps
// the office shell so an office user can still leave the plant.
const plantShell = computed(
  () =>
    route.path.startsWith("/plant") &&
    !!plantOperatorStore.operator &&
    route.name !== "OperatorClockIn",
);

const logout = async () => {
  helpStore.reset();
  menuSearch.reset();
  await store.removeAuthorization();
  router.push("/login");
};

const logoutOperator = () => {
  helpStore.reset();
  plantOperatorStore.removeOperator();
  router.push({ path: "/plant" });
};
</script>

<template>
  <div v-if="store.authorization">
    <PlantHeader v-if="plantShell" @exit="logoutOperator" />
    <template v-else>
      <Header />
      <SideBar @logout-click="logout" @logout-operator-click="logoutOperator" />
    </template>
    <HelpDrawer />
    <MenuSearchDialog />
    <main
      class="app__view"
      :class="{
        collapsed: store.sidebar.collapsed && !plantShell,
        'app__view--plant': plantShell,
      }"
    >
      <ScrollPanel class="app__scroll">
        <div class="app__content">
          <RouterView />
        </div>
      </ScrollPanel>
    </main>
  </div>

  <Login v-else />

  <!-- PWA Update Handler -->
  <PwaUpdatePrompt />

  <Toast position="bottom-right" />
  <ConfirmDialog />

  <!-- Subtle loading indicator -->
  <Transition name="loading-fade">
    <div v-if="apiStore.isWaiting" class="loading-indicator">
      <div class="loading-bar"></div>
    </div>
  </Transition>
</template>

<style lang="scss">
// Replaced deprecated @import with @use. Using 'as *' to preserve existing global class availability.
@use "./assets/styles.scss" as *;

.app__view {
  position: fixed;
  top: var(--top-panel-height);
  left: var(--side-bar-width);
  padding: 1rem;
  width: calc(100vw - var(--side-bar-width));
  transition: all 0.3s ease-in-out;
}

/* Only this panel scrolls, never the page, so mobile Chrome keeps its URL
   bar: dvh is the height actually visible, where vh would assume the bar
   hidden and push the end of the content below the screen. */
.app__scroll {
  height: calc(100dvh - 5rem);
}

/* Room above Android's gesture bar and the iOS home indicator, which the
   page draws under (viewport-fit=cover). */
.app__content {
  padding-bottom: env(safe-area-inset-bottom, 0px);
}

.collapsed {
  left: calc(var(--side-bar-collapsed-width) + var(--collapsed-side-padding));
  width: calc(
    100vw - var(--side-bar-collapsed-width) - var(--collapsed-side-padding)
  );
}

/* Operator shell: no sidebar, the content takes the full width. */
.app__view.app__view--plant {
  left: 0;
  width: 100vw;
}

/* Phones: the sidebar becomes a drawer, content takes the full width. */
@media (max-width: 767.98px) {
  .app__view,
  .app__view.collapsed {
    left: 0;
    width: 100vw;
    padding: 0.75rem;
  }
}

/* Subtle loading indicator - top progress bar */
.loading-indicator {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  z-index: 9999;
  overflow: hidden;
  background: transparent;
}

.loading-bar {
  height: 100%;
  width: 30%;
  background: linear-gradient(
    90deg,
    var(--p-primary-400),
    var(--p-primary-500),
    var(--p-primary-400)
  );
  border-radius: 0 2px 2px 0;
  animation: loading-slide 1.2s ease-in-out infinite;
  box-shadow: 0 0 8px var(--p-primary-400);
}

@keyframes loading-slide {
  0% {
    transform: translateX(-100%);
  }
  50% {
    transform: translateX(200%);
  }
  100% {
    transform: translateX(400%);
  }
}

/* Fade transition for loading indicator */
.loading-fade-enter-active {
  transition: opacity 0.15s ease-out;
}

.loading-fade-leave-active {
  transition: opacity 0.3s ease-out;
}

.loading-fade-enter-from,
.loading-fade-leave-to {
  opacity: 0;
}
</style>
