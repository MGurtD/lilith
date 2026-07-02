<template>
  <sidebar-menu
    :menu="store.sidebar.menus"
    :collapsed="store.sidebar.collapsed"
    :showOneChild="true"
    :hideToggle="store.sidebar.hideToggle"
    @update:collapsed="toggleCollapse"
  >
    <template #header>
      <div class="brand" @click="() => router.push({ path: '/' })">
        <img
          v-if="branding.logoPath"
          :src="branding.logoPath"
          :alt="branding.logoAlt || companyName"
          class="brand-logo"
          draggable="false"
        />
        <span
          v-if="!store.sidebar.collapsed"
          class="brand-name"
          :title="companyName"
          >{{ companyName }}</span
        >
      </div>
    </template>
    <template #footer>
      <div class="sidebar-footer">
        <Button
          :label="store.sidebar.collapsed ? '' : 'Sol·licitud de suport'"
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
    header="Sol·licitud de suport"
    :modal="true"
    :style="{ width: '480px' }"
    @hide="showSupportDialog = false"
  >
    <FormSupportRequest @close="showSupportDialog = false" />
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { SidebarMenu } from "vue-sidebar-menu";
import "vue-sidebar-menu/dist/vue-sidebar-menu.css";
import { useStore } from "../store";
import { branding, getCompanyName } from "../config/branding";
import { useRouter } from "vue-router";
import FormSupportRequest from "../modules/shared/components/FormSupportRequest.vue";

const router = useRouter();
const store = useStore();
const showSupportDialog = ref(false);

// Derive dynamic company name depending on collapsed state
const companyName = computed(() => getCompanyName(store.sidebar.collapsed));

function toggleCollapse() {
  store.sidebar.collapsed = !store.sidebar.collapsed;
}
</script>

<style scoped>
.v-sidebar-menu {
  background-color: var(--p-blue-900);
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
  border-bottom: 1px solid var(--p-surface-400);
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  background: transparent;
  border: none;
  color: var(--p-surface-0);
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

.sidebar-footer {
  padding: 0.65rem 0.85rem;
  color: var(--p-surface-300);
  display: flex;
  justify-content: center;
  font-size: 0.7rem;
  letter-spacing: 1px;
  text-transform: uppercase;
}

.support-btn {
  width: 100%;
  color: var(--p-surface-300) !important;
  justify-content: flex-start;
  font-size: 0.75rem;
  letter-spacing: 0.5px;
  text-transform: uppercase;
}

.support-btn:hover {
  background-color: var(--p-blue-700) !important;
  color: var(--p-surface-0) !important;
}
</style>
