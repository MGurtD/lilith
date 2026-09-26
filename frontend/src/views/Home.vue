<template>
  <main class="home">
    <section class="home__welcome">
      <p class="home__date">{{ longDate }}</p>
      <h2 class="home__greeting">{{ greeting }}</h2>
    </section>

    <!-- Same brand motif as the login: a drawing inked in the tenant colour and
         its title block. It gives way to work blocks as the home grows. -->
    <section class="home__paper" aria-hidden="true">
      <ShaftDrawing class="home__drawing" />
      <TitleBlock
        class="home__title-block"
        :top="{ label: t('ui.titleBlock.system'), value: 'Zenith ERP' }"
        :cells="[
          { label: t('ui.titleBlock.company'), value: brandingStore.brandName, strong: true },
          { label: t('ui.titleBlock.user'), value: userName },
          { label: t('ui.titleBlock.date'), value: shortDate },
        ]"
      />
    </section>
  </main>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import ShaftDrawing from "@/components/brand/ShaftDrawing.vue";
import TitleBlock from "@/components/brand/TitleBlock.vue";
import { useBrandingStore } from "@/store/branding";
import { useStore } from "../store";

const store = useStore();
const brandingStore = useBrandingStore();
const { t } = useI18n();

// Refreshed every minute so the greeting follows the time of day.
const now = ref(new Date());
let timer: ReturnType<typeof setInterval> | undefined;

const firstName = computed(() => store.user?.firstName?.trim() || store.user?.username || "");
const userName = computed(() => {
  const user = store.user;
  if (!user) return "";
  return `${user.firstName ?? ""} ${user.lastName ?? ""}`.trim() || user.username;
});

const greeting = computed(() => {
  const hour = now.value.getHours();
  const key =
    hour >= 6 && hour < 14
      ? "home.greetingMorning"
      : hour >= 14 && hour < 21
        ? "home.greetingAfternoon"
        : "home.greetingEvening";
  return t(key, { name: firstName.value });
});

const longDate = computed(() => {
  const text = new Intl.DateTimeFormat(store.language.current, {
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(now.value);
  return text.charAt(0).toUpperCase() + text.slice(1);
});

const shortDate = computed(() =>
  new Intl.DateTimeFormat(store.language.current, {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(now.value),
);

onMounted(() => {
  timer = setInterval(() => (now.value = new Date()), 60000);
  store.setMenuItem({
    title: t("home.title"),
    icon: "",
  });
});

onUnmounted(() => clearInterval(timer));
</script>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  min-height: calc(100dvh - var(--top-panel-height) - 2rem);
}

.home__welcome {
  padding: 1rem 0.25rem 0;
}

.home__date {
  margin: 0 0 0.25rem;
  font-size: 1.0714rem;
  color: var(--p-text-muted-color);
}

.home__greeting {
  margin: 0;
  font-family: var(--font-condensed);
  font-size: 2.5714rem;
  line-height: 1.15;
  font-weight: 600;
  color: var(--p-text-color);
}

.home__paper {
  flex: 1;
  min-height: 24rem;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem 2rem 11rem;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius-md);
}

.home__drawing {
  max-width: 46rem;
}

.home__title-block {
  position: absolute;
  right: 2rem;
  bottom: 2rem;
  width: min(34rem, calc(100% - 4rem));
}

/* Phones: the title block follows the drawing instead of overlapping it. */
@media (max-width: 767.98px) {
  .home {
    min-height: 0;
  }

  .home__greeting {
    font-size: 2rem;
  }

  .home__paper {
    flex: none;
    flex-direction: column;
    gap: 1.5rem;
    min-height: 0;
    padding: 1.5rem 1rem;
  }

  .home__title-block {
    position: static;
    width: 100%;
  }
}
</style>
