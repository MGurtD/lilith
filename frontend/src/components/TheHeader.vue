<template>
  <div class="title-bar" :class="{ collapsed: store.sidebar.collapsed }">
    <div class="title-bar__page">
      <i
        v-if="store.currentMenuItem.backButtonVisible"
        class="title-bar__back"
        :class="PrimeIcons.ARROW_LEFT"
        @click="goBack"
      ></i>
      <span class="title-bar__page__text">{{
        store.currentMenuItem.title
      }}</span>
    </div>
    <div v-if="plantOperatorStore.operator" class="title-bar__user">
      <div class="avatar-container" @click="showOverlayPanel">
        <Avatar
          :label="
            plantOperatorStore.operator.name.substring(0, 1).toUpperCase()
          "
          class="title-bar__user__avatar title-bar__user__avatar--operator"
          size="large"
          shape="circle"
        />
      </div>
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
              Operari
            </div>
          </div>

          <div class="divider" />

          <div class="user-menu__actions">
            <Button
              :icon="PrimeIcons.SIGN_OUT"
              label="Sortir"
              class="w-full"
              size="large"
              @click="logoutOperator"
            />
          </div>
        </div>
      </Popover>
    </div>
    <div class="title-bar__user" v-else-if="store.user">
      <div class="avatar-container" @click="showOverlayPanel">
        <Avatar
          :label="store.user.username.substring(0, 1).toUpperCase()"
          class="title-bar__user__avatar title-bar__user__avatar--admin"
          size="large"
          shape="circle"
        />
      </div>
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
            <label class="user-menu__label">Idioma</label>
            <LanguageSwitcher
              v-model="store.user.preferredLanguage"
              :changeAppLanguage="true"
            />
          </div>

          <div class="user-menu__actions">
            <Button
              :icon="PrimeIcons.SIGN_OUT"
              label="Tancar sessió"
              class="w-full"
              size="large"
              @click="logoutClick"
            />
          </div>
        </div>
      </Popover>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useStore } from "../store";
import Avatar from "primevue/avatar";
import Badge from "primevue/badge";
import Popover from "primevue/popover";
import { PrimeIcons } from "@primevue/core/api";
import { useRouter } from "vue-router";
import { usePlantOperatorStore } from "../modules/plant/store";
import LanguageSwitcher from "./LanguageSwitcher.vue";

const emits = defineEmits(["logoutClick", "logoutOperatorClick"]);
const plantOperatorStore = usePlantOperatorStore();

const store = useStore();
const op = ref();
const showOverlayPanel = (event: any) => {
  op.value.toggle(event);
};
const logoutClick = () => emits("logoutClick");
const logoutOperator = () => emits("logoutOperatorClick");

const router = useRouter();
const goBack = () => router.back();
</script>

<style scoped>
.title-bar {
  position: fixed;
  background-color: var(--p-blue-900);
  color: var(--p-surface-400);
  top: 0;
  left: var(--side-bar-width);
  height: var(--top-panel-height);
  display: grid;
  grid-template-columns: 0.7fr 0.3fr;
  padding-top: 0.3rem;
  padding-bottom: 0.5rem;
  padding-top: 0.3rem;
  padding-bottom: 0.5rem;
  width: calc(100vw - var(--side-bar-width));
  transition: all 0.3s ease-in-out;
}

.collapsed {
  left: 60px;
  width: calc(100vw - var(--side-bar-collapsed-width));
}

.title-bar__page {
  margin-left: 1vw;
  margin-top: 0.6rem;
  margin-top: 0.6rem;
  text-align: left;
}

.title-bar__page__text {
  font-size: 1.5rem;
}

.title-bar__back {
  font-size: 1.3rem;
  font-size: 1.3rem;
  margin-right: 1rem;
  cursor: pointer;
}

.title-bar__back:hover {
  color: #fff;
}

.title-bar__user {
  padding-top: 0.2rem;
  margin-right: 1.5vw;
  padding-top: 0.2rem;
  margin-right: 1.5vw;
  font-size: 1rem;
  text-align: right;
}

/* Avatar mejorado con badge */
.avatar-container {
  position: relative;
  display: inline-block;
  cursor: pointer;
}

.title-bar__user__avatar {
  cursor: pointer;
  font-weight: 700;
  font-size: 1.1rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  transition: transform 0.2s ease;
}

.title-bar__user__avatar:hover {
  transform: scale(1.05);
}

/* Gradientes por rol */
.title-bar__user__avatar--operator {
  background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);
  color: white;
}

.title-bar__user__avatar--admin {
  background: linear-gradient(135deg, #bfdbfe 0%, #93c5fd 100%);
  color: #1e3a8a;
}

.avatar-badge {
  position: absolute;
  bottom: -2px;
  right: -2px;
  min-width: 1.5rem;
  height: 1.5rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.7rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

/* New user overlay menu styles */
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
  font-weight: 700;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.user-menu__avatar--operator {
  background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);
  color: white;
}

.user-menu__avatar--admin {
  background: linear-gradient(135deg, #bfdbfe 0%, #93c5fd 100%);
  color: #1e3a8a;
}

.user-menu__name {
  font-weight: 600;
  color: var(--p-surface-900, #1f2937);
}

.user-menu__username {
  font-size: 0.85rem;
  color: var(--p-surface-500, #6b7280);
  display: flex;
  align-items: center;
}

.divider {
  height: 1px;
  background: var(--p-surface-200, #e5e7eb);
  margin: 0.75rem 0;
}

.user-menu__section {
  display: grid;
  gap: 0.5rem;
}

.user-menu__label {
  font-size: 0.85rem;
  color: var(--p-surface-600, #4b5563);
}

.user-menu__actions {
  margin-top: 0.75rem;
}

.title-bar__operator {
  display: grid;
  grid-template-columns: 1fr 0.4fr;
  align-items: center;
  gap: 1rem;
}
</style>
