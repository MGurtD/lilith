<script setup lang="ts">
import { onMounted, onUnmounted, watch } from "vue";
import ScrollPanel from "primevue/scrollpanel";
import { applyPrimeVueLocale } from "./i18n";
import { usePrimeVue } from "primevue/config";
import { useRoute, useRouter } from "vue-router";
import HelpDrawer from "@/components/help/HelpDrawer.vue";
import Header from "@/components/TheHeader.vue";
import PwaUpdatePrompt from "@/components/PwaUpdatePrompt.vue";
import SideBar from "@/components/TheSidebar.vue";
import { usePlantOperatorStore } from "@/modules/plant/store";
import { useStore } from "@/store";
import { useApiStore } from "@/store/backend";
import { useSpanishGeography } from "@/store/geography";
import { useHelpStore } from "@/store/help";
import Login from "@/views/Login.vue";

const store = useStore();
const plantOperatorStore = usePlantOperatorStore();
const apiStore = useApiStore();
const spanishGeography = useSpanishGeography();
const helpStore = useHelpStore();
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

onMounted(async () => {
  window.addEventListener("keydown", handleHelpShortcut);
  spanishGeography.fetch();

  // Initialize language for anonymous users; JWT-based locale will be handled in setAuthorization
  await store.initLanguage();
  await store.getAuthorization();
});

onUnmounted(() => {
  window.removeEventListener("keydown", handleHelpShortcut);
});

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

const logout = async () => {
  helpStore.reset();
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
    <Header @logout-click="logout" @logout-operator-click="logoutOperator" />
    <SideBar />
    <HelpDrawer />
    <main class="app__view" :class="{ collapsed: store.sidebar.collapsed }">
      <ScrollPanel style="height: calc(100vh - 5rem)">
        <RouterView />
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

.collapsed {
  left: calc(var(--side-bar-collapsed-width) + var(--collapsed-side-padding));
  width: calc(
    100vw - var(--side-bar-collapsed-width) - var(--collapsed-side-padding)
  );
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
