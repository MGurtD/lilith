<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useToast } from "primevue/usetoast";
import { useRegisterSW } from "virtual:pwa-register/vue";
import { useI18n } from "vue-i18n";

const toast = useToast();
const { t } = useI18n();
const showUpdateToast = ref(false);

// Configuración del service worker con auto-update
const { offlineReady, needRefresh, updateServiceWorker } = useRegisterSW({
  immediate: true,
  onRegisteredSW(swUrl, registration) {
    console.log("[PWA] Service Worker registrado:", swUrl);

    // Check for updates every hour
    if (registration) {
      setInterval(
        () => {
          registration.update();
        },
        60 * 60 * 1000
      ); // 1 hora
    }
  },
  onRegisterError(error) {
    console.error("[PWA] Error al registrar Service Worker:", error);
  },
});

// Auto-actualizar cuando detecte nueva versión
onMounted(() => {
  // Notificar cuando la app está lista para funcionar offline
  if (offlineReady.value) {
    toast.add({
      severity: "success",
      summary: t("pwa.offlineReady"),
      detail: t("pwa.offlineReadyDetail"),
      life: 5000,
    });
  }

  // Detectar y aplicar actualizaciones automáticamente
  if (needRefresh.value) {
    showUpdateToast.value = true;

    toast.add({
      severity: "info",
      summary: t("pwa.updateAvailable"),
      detail: t("pwa.updating"),
      life: 3000,
    });

    // Auto-actualizar después de 2 segundos
    setTimeout(() => {
      updateServiceWorker(true); // true = recargar página automáticamente
    }, 2000);
  }
});
</script>

<template>
  <!-- Este componente no tiene UI visible, trabaja en segundo plano -->
  <div style="display: none"></div>
</template>
