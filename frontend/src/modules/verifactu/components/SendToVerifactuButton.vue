<template>
  <Button
    :icon="icon"
    :size="size"
    :severity="severity"
    :disabled="disabled || loading"
    :loading="loading"
    :tooltip="$t('verifactu.invoiceIntegration.actions.integrate')"
    text
    rounded
    @click="onClick"
  />
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import { useVerifactuStore } from "../store/verifactu";

const props = withDefaults(
  defineProps<{
    invoiceId: string;
    invoiceNumber?: string;
    disabled?: boolean;
    icon?: string;
    size?: "small" | "large";
    severity?:
      | "primary"
      | "secondary"
      | "success"
      | "info"
      | "warning"
      | "help"
      | "danger"
      | "contrast"
      | undefined;
  }>(),
  {
    disabled: false,
    icon: "pi pi-check",
    size: "small",
    severity: "success",
  }
);

const emit = defineEmits<{
  (e: "sent"): void;
  (e: "error", message?: string): void;
}>();

const { t } = useI18n();
const toast = useToast();
const verifactuStore = useVerifactuStore();
const loading = ref(false);

async function onClick() {
  if (!props.invoiceId || loading.value) return;
  try {
    loading.value = true;
    const response = await verifactuStore.SendToVerifactu(props.invoiceId);
    if (response?.result) {
      toast.add({
        severity: "success",
        summary: t("common.success"),
        detail: t("verifactu.invoiceIntegration.messages.integrateSuccess", {
          number: props.invoiceNumber ?? "",
        }),
        life: 5000,
      });
      emit("sent");
    } else {
      const msg = response?.errors?.[0] ?? "Integration failed";
      throw new Error(msg);
    }
  } catch (err: any) {
    toast.add({
      severity: "error",
      summary: t("common.error"),
      detail: t("verifactu.invoiceIntegration.messages.integrateError", {
        number: props.invoiceNumber ?? "",
      }),
      life: 5000,
    });
    emit("error", err?.message);
  } finally {
    loading.value = false;
  }
}
</script>
