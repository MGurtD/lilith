<template>
  <header class="title-bar" :class="{ collapsed: store.sidebar.collapsed }">
    <div class="title-bar__page">
      <Button
        v-if="store.currentMenuItem.backButtonVisible"
        :icon="PrimeIcons.ARROW_LEFT"
        severity="secondary"
        text
        rounded
        :aria-label="t('ui.back')"
        @click="goBack"
      />
      <div class="title-bar__heading">
        <span v-if="moduleTitle" class="title-bar__module">{{ moduleTitle }}</span>
        <h1 class="title-bar__title">{{ store.currentMenuItem.title }}</h1>
      </div>
    </div>
    <div class="title-bar__right">
      <!-- Screens teleport their page-level actions (Save…) here. -->
      <div id="page-actions" class="title-bar__actions"></div>
      <Button
        v-if="helpKey"
        icon="pi pi-question-circle"
        severity="secondary"
        text
        rounded
        :aria-label="t('help.actions.openTooltip')"
        v-tooltip.bottom="t('help.actions.openTooltip')"
        @click="helpStore.toggleForRoute(helpKey)"
      />
      <div v-if="plantOperatorStore.operator" class="title-bar__user">
        <button
          type="button"
          class="avatar-button"
          :aria-label="t('ui.userMenu')"
          @click="showOverlayPanel"
        >
          <Avatar
            :label="
              plantOperatorStore.operator.name.substring(0, 1).toUpperCase()
            "
            class="title-bar__avatar title-bar__avatar--operator"
            shape="circle"
          />
        </button>
        <Popover ref="op">
          <div class="user-menu">
            <div class="user-menu__header">
              <Avatar
                :label="
                  plantOperatorStore.operator.name.substring(0, 1).toUpperCase()
                "
                class="user-menu__avatar user-menu__avatar--operator"
                size="large"
                shape="circle"
              />
              <div class="user-menu__name">
                {{ plantOperatorStore.operator.name }}
                {{ plantOperatorStore.operator.surname }}
              </div>
              <div class="user-menu__username">
                <i :class="PrimeIcons.USER" class="mr-1"></i>
                {{ $t("ui.operator") }}
              </div>
            </div>

            <div class="divider" />

            <div class="user-menu__actions">
              <Button
                :icon="PrimeIcons.SIGN_OUT"
                :label="$t('ui.exit')"
                class="w-full"
                size="large"
                @click="logoutOperator"
              />
            </div>
          </div>
        </Popover>
      </div>
      <div class="title-bar__user" v-else-if="store.user">
        <button
          type="button"
          class="avatar-button"
          :aria-label="t('ui.userMenu')"
          @click="showOverlayPanel"
        >
          <Avatar
            :label="store.user.username.substring(0, 1).toUpperCase()"
            class="title-bar__avatar title-bar__avatar--admin"
            shape="circle"
          />
        </button>
        <Popover ref="op">
          <div class="user-menu">
            <div class="user-menu__header">
              <Avatar
                :label="store.user.username.substring(0, 1).toUpperCase()"
                class="user-menu__avatar user-menu__avatar--admin"
                size="large"
                shape="circle"
              />
              <div class="user-menu__name">
                {{ store.user.firstName }} {{ store.user.lastName }}
              </div>
              <div class="user-menu__username">
                <i :class="PrimeIcons.SHIELD" class="mr-1"></i>
                @{{ store.user.username }}
              </div>
            </div>

            <div class="divider" />

            <div class="user-menu__section">
              <label class="user-menu__label">{{ $t("ui.language") }}</label>
              <LanguageSwitcher
                v-model="store.user.preferredLanguage"
                :changeAppLanguage="true"
              />
            </div>

            <div class="user-menu__actions">
              <Button
                :icon="PrimeIcons.SIGN_OUT"
                :label="$t('ui.signOut')"
                class="w-full"
                size="large"
                @click="logoutClick"
              />
            </div>
          </div>
        </Popover>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import Avatar from "primevue/avatar";
import Popover from "primevue/popover";
import { PrimeIcons } from "@primevue/core/api";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import LanguageSwitcher from "@/components/LanguageSwitcher.vue";
import { usePlantOperatorStore } from "@/modules/plant/store";
import { useStore } from "@/store";
import { useHelpStore } from "@/store/help";
import type { MenuItem } from "@/types/component";

const emits = defineEmits(["logoutClick", "logoutOperatorClick"]);
const plantOperatorStore = usePlantOperatorStore();

const store = useStore();
const op = ref();
const showOverlayPanel = (event: Event) => {
  op.value.toggle(event);
};
const logoutClick = () => emits("logoutClick");
const logoutOperator = () => emits("logoutOperatorClick");

const { t } = useI18n();
const helpStore = useHelpStore();
const route = useRoute();
const router = useRouter();
const goBack = () => router.back();

const helpKey = computed(() =>
  typeof route.meta.helpKey === "string" ? route.meta.helpKey : undefined,
);

// Module that owns the current screen: the top-level sidebar entry whose menu
// tree (any depth) holds the route. Detail routes (/customers/:id) belong to
// their list entry (/customers).
const ownsRoute = (entry: MenuItem, path: string): boolean =>
  (!!entry.href && (path === entry.href || path.startsWith(`${entry.href}/`))) ||
  (entry.child ?? []).some((child) => ownsRoute(child, path));

const moduleTitle = computed<string | undefined>(() => {
  const owner = store.sidebar.menus.find(
    (module: MenuItem) => !module.href && ownsRoute(module, route.path),
  );
  return owner?.title;
});
</script>

<style scoped>
.title-bar {
  position: fixed;
  top: 0;
  left: var(--side-bar-width);
  width: calc(100vw - var(--side-bar-width));
  height: var(--top-panel-height);
  box-sizing: border-box;
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0 1.5rem 0 1rem;
  background-color: var(--p-surface-0);
  border-bottom: 1px solid var(--p-surface-200);
  color: var(--p-text-color);
  transition: all 0.3s ease-in-out;
}

.collapsed {
  left: calc(var(--side-bar-collapsed-width) + var(--collapsed-side-padding));
  width: calc(
    100vw - var(--side-bar-collapsed-width) - var(--collapsed-side-padding)
  );
}

.title-bar__page {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.title-bar__heading {
  min-width: 0;
  display: flex;
  flex-direction: column;
  padding-left: 0.5rem;
}

.title-bar__module {
  font-size: 0.8571rem;
  line-height: 1.2;
  color: var(--p-text-muted-color);
}

.title-bar__title {
  margin: 0;
  font-family: var(--font-condensed);
  font-size: 1.4286rem;
  font-weight: 600;
  line-height: 1.25;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.title-bar__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.title-bar__actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.title-bar__actions:not(:empty) {
  margin-right: 0.5rem;
}

.avatar-button {
  all: unset;
  display: inline-flex;
  border-radius: 50%;
  cursor: pointer;
}

.avatar-button:focus-visible {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 2px;
}

.title-bar__avatar {
  width: 2.25rem;
  height: 2.25rem;
  font-family: var(--font-condensed);
  font-weight: 600;
}

.title-bar__avatar--admin,
.user-menu__avatar--admin {
  background: var(--p-surface-100);
  color: var(--p-surface-700);
}

.title-bar__avatar--operator,
.user-menu__avatar--operator {
  background: var(--p-primary-100);
  color: var(--p-primary-800);
}

/* User menu (popover) */
.user-menu {
  min-width: 16rem;
  padding: 0.75rem;
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
  font-family: var(--font-condensed);
  font-weight: 600;
}

.user-menu__name {
  font-weight: 600;
  color: var(--p-text-color);
}

.user-menu__username {
  display: flex;
  align-items: center;
  font-size: 0.9286rem;
  color: var(--p-text-muted-color);
}

.divider {
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

.user-menu__actions {
  margin-top: 0.75rem;
}
</style>
