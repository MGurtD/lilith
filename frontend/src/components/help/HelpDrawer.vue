<template>
  <Drawer
    :visible="helpStore.visible"
    position="right"
    class="help-drawer"
    :style="{ width: 'min(42rem, 100vw)' }"
    @update:visible="handleVisibleChange"
  >
    <template #header>
      <div class="help-drawer__header">
        <div class="help-drawer__title">{{ t("help.drawer.title") }}</div>
        <div class="help-drawer__subtitle">{{ t("help.drawer.subtitle") }}</div>
      </div>
    </template>

    <div class="help-drawer__content">
      <div v-if="helpStore.loading" class="help-drawer__state">
        <ProgressBar mode="indeterminate" style="height: 0.5rem" />
        <p>{{ t("help.messages.loading") }}</p>
      </div>

      <div v-else-if="helpStore.error" class="help-drawer__state help-drawer__state--empty">
        <i class="pi pi-info-circle help-drawer__icon"></i>
        <p>{{ helpStore.error }}</p>
      </div>

      <MarkdownRenderer v-else-if="helpStore.markdown" :markdown="helpStore.markdown" />

      <div v-else class="help-drawer__state help-drawer__state--empty">
        <i class="pi pi-info-circle help-drawer__icon"></i>
        <p>{{ t("help.messages.notAvailable") }}</p>
      </div>
    </div>
  </Drawer>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import MarkdownRenderer from "@/components/help/MarkdownRenderer.vue";
import { useHelpStore } from "@/store/help";

const helpStore = useHelpStore();
const { t } = useI18n();

const handleVisibleChange = (visible: boolean) => {
  if (!visible) {
    helpStore.close();
  }
};
</script>

<style scoped>
.help-drawer__header {
  display: grid;
  gap: 0.2rem;
}

.help-drawer__title {
  font-size: 1.1rem;
  font-weight: 700;
}

.help-drawer__subtitle {
  font-size: 0.9rem;
  color: var(--p-surface-500);
}

.help-drawer__content {
  display: grid;
  gap: 1rem;
  min-height: 100%;
}

.help-drawer__state {
  display: grid;
  gap: 1rem;
  padding: 1rem 0;
  color: var(--p-surface-700);
}

.help-drawer__state--empty {
  align-content: start;
  justify-items: start;
}

.help-drawer__icon {
  font-size: 1.5rem;
  color: var(--p-primary-500);
}
</style>
