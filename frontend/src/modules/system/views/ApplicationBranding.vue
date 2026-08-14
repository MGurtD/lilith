<template>
  <div class="card">
    <ProgressSpinner v-if="loading" />
    <FormApplicationBranding v-else :can-edit="canEdit" />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useI18n } from "vue-i18n";

import FormApplicationBranding from "../components/FormApplicationBranding.vue";
import { useBrandingStore } from "@/store/branding";
import { useStore } from "@/store";

const { t } = useI18n();
const brandingStore = useBrandingStore();
const appStore = useStore();
const loading = ref(true);
const canEdit = computed(() => appStore.role?.toLowerCase() === "admin");

onMounted(async () => {
  appStore.setMenuItem({
    icon: PrimeIcons.PALETTE,
    backButtonVisible: false,
    title: t("branding.pageTitle"),
  });
  await brandingStore.initialize();
  loading.value = false;
});
</script>
