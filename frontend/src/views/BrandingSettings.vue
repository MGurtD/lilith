<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useToast } from "primevue/usetoast";
import Message from "primevue/message";
import { useBrandingStore } from "@/store/branding";
import type { Branding } from "@/types/branding";
import apiClient from "@/api/api.client";
import FormBranding from "@/components/forms/FormBranding.vue";
import { useStore } from "@/store";
import { PrimeIcons } from "@primevue/core/api";

interface Enterprise {
  id: string;
  name: string;
  description?: string;
  disabled: boolean;
  theme?: string | null;
  primaryColor?: string | null;
  logoMain?: string | null;
  logoSidebar?: string | null;
  titleSidebar?: string | null;
}

const HEX_COLOR_REGEX = /^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$/;

const brandingStore = useBrandingStore();
const toast = useToast();
const globalStore = useStore();

const enterprise = ref<Enterprise | undefined>(undefined);
const loading = ref(false);
const saving = ref(false);

const draft = ref<Branding>({
  theme: null,
  primaryColor: null,
  logoMain: null,
  logoSidebar: null,
  titleSidebar: null,
});

const previewLogoMain = computed(() => draft.value.logoMain ?? "");
const previewLogoSidebar = computed(() => draft.value.logoSidebar ?? "");
const previewCompanyName = computed(
  () => draft.value.titleSidebar?.trim() || "Lilith",
);

function syncDraftFromEnterprise() {
  if (!enterprise.value) return;
  draft.value = {
    theme: enterprise.value.theme ?? null,
    primaryColor: enterprise.value.primaryColor ?? null,
    logoMain: enterprise.value.logoMain ?? null,
    logoSidebar: enterprise.value.logoSidebar ?? null,
    titleSidebar: enterprise.value.titleSidebar ?? null,
  };
}

async function loadActiveEnterprise() {
  loading.value = true;
  try {
    const response = await apiClient.get("/Enterprise");
    if (response.status === 200) {
      const list = response.data as Array<Enterprise>;
      const first = list.find((e) => !e.disabled) ?? list[0];
      enterprise.value = first;
      syncDraftFromEnterprise();
    }
  } finally {
    loading.value = false;
  }
}

async function submit() {
  if (!enterprise.value) return;
  if (
    draft.value.primaryColor &&
    !HEX_COLOR_REGEX.test(draft.value.primaryColor)
  ) {
    toast.add({
      severity: "warn",
      summary: "Color invàlid",
      detail: "El color ha de ser un codi hexadecimal (#RRGGBB o #RRGGBBAA).",
      life: 5000,
    });
    return;
  }
  saving.value = true;
  try {
    const payload: Enterprise = {
      ...enterprise.value,
      theme: draft.value.theme,
      primaryColor: draft.value.primaryColor,
      logoMain: draft.value.logoMain,
      logoSidebar: draft.value.logoSidebar,
      titleSidebar: draft.value.titleSidebar,
    };
    const response = await apiClient.put(
      `/Enterprise/${enterprise.value.id}`,
      payload,
    );
    if (response.status === 200 || response.status === 204) {
      enterprise.value = payload;
      const ok = await brandingStore.update(draft.value);
      if (ok) {
        toast.add({
          severity: "success",
          summary: "Branding desat",
          detail: "Els canvis s'han aplicat immediatament.",
          life: 4000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: "No s'ha pogut desar el branding",
          life: 5000,
        });
      }
    } else {
      toast.add({
        severity: "error",
        summary: "No s'ha pogut desar el branding",
        life: 5000,
      });
    }
  } catch (err) {
    toast.add({
      severity: "error",
      summary: "Error en desar el branding",
      detail: (err as Error)?.message ?? "Error desconegut",
      life: 6000,
    });
  } finally {
    saving.value = false;
  }
}

function reset() {
  syncDraftFromEnterprise();
}

onMounted(async () => {
  globalStore.setMenuItem({
    icon: PrimeIcons.PALETTE,
    title: "Branding",
  });
  await loadActiveEnterprise();
});
</script>

<template>
  <div class="branding-settings card">
    <h2 class="branding-settings__title">Branding</h2>
    <p class="branding-settings__intro">
      Configura el tema, color primari, logos i nom curt que veuran els
      usuaris. Els canvis s'apliquen immediatament.
    </p>

    <div v-if="loading" class="branding-settings__loading">
      Carregant empresa...
    </div>

    <div v-else-if="enterprise" class="branding-settings__grid">
      <div class="branding-settings__column branding-settings__column--form">
        <FormBranding
          v-model="draft"
          :saving="saving"
          @submit="submit"
          @reset="reset"
        />
        <Message
          severity="info"
          :closable="false"
          class="branding-settings__note"
        >
          La pujada directa de fitxers requereix un endpoint d'upload al
          backend. Per ara, pots fer servir una URL externa o una ruta servida
          per l'aplicació.
        </Message>
      </div>

      <aside class="branding-settings__column branding-settings__column--preview">
        <h3>Previsualització</h3>

        <div class="preview-block preview-block--header">
          <span class="preview-block__label">Capçalera</span>
          <div class="preview-header">
            <img
              v-if="previewLogoMain"
              :src="previewLogoMain"
              alt="Logo"
              class="preview-header__logo"
            />
            <span class="preview-header__title">Pàgina actual</span>
          </div>
        </div>

        <div class="preview-block preview-block--sidebar">
          <span class="preview-block__label">Sidebar</span>
          <div class="preview-sidebar">
            <img
              v-if="previewLogoSidebar"
              :src="previewLogoSidebar"
              alt="Logo sidebar"
              class="preview-sidebar__logo"
            />
            <span class="preview-sidebar__title">{{ previewCompanyName }}</span>
          </div>
        </div>

        <div class="preview-block preview-block--swatch">
          <span class="preview-block__label">Color primari</span>
          <div
            class="preview-swatch"
            :style="{ backgroundColor: draft.primaryColor ?? '#0ea5e9' }"
          ></div>
          <code class="preview-swatch__hex">
            {{ draft.primaryColor ?? "#0ea5e9" }}
          </code>
        </div>
      </aside>
    </div>

    <div v-else class="branding-settings__empty">
      No s'ha trobat cap empresa activa.
    </div>
  </div>
</template>

<style scoped>
.branding-settings {
  padding: 1.5rem;
}
.branding-settings__title {
  margin: 0 0 0.5rem 0;
}
.branding-settings__intro {
  margin: 0 0 1.5rem 0;
  color: var(--p-text-muted-color, #6b7280);
}
.branding-settings__grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}
@media (max-width: 900px) {
  .branding-settings__grid {
    grid-template-columns: 1fr;
  }
}
.branding-settings__note {
  margin-top: 1.5rem;
}
.branding-settings__loading,
.branding-settings__empty {
  padding: 1rem;
  color: var(--p-text-muted-color, #6b7280);
}
.preview-block {
  border: 1px solid var(--p-surface-300, #d1d5db);
  border-radius: 12px;
  padding: 1rem;
  margin-bottom: 1rem;
  background: var(--p-surface-0, #ffffff);
}
.preview-block__label {
  display: block;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--p-text-muted-color, #6b7280);
  margin-bottom: 0.5rem;
}
.preview-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--p-blue-900, #1e3a8a);
  color: var(--p-surface-0, #ffffff);
  padding: 0.75rem;
  border-radius: 8px;
}
.preview-header__logo {
  height: 32px;
  max-width: 140px;
  object-fit: contain;
}
.preview-header__title {
  font-size: 1.1rem;
}
.preview-sidebar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--p-blue-900, #1e3a8a);
  color: var(--p-surface-0, #ffffff);
  padding: 1rem;
  border-radius: 8px;
}
.preview-sidebar__logo {
  height: 32px;
  max-width: 140px;
  object-fit: contain;
}
.preview-sidebar__title {
  font-weight: 600;
  letter-spacing: 0.5px;
  text-transform: uppercase;
}
.preview-swatch {
  width: 100%;
  height: 64px;
  border-radius: 8px;
  border: 1px solid var(--p-surface-300, #d1d5db);
}
.preview-swatch__hex {
  display: block;
  margin-top: 0.5rem;
  font-family: monospace;
  font-size: 0.85rem;
  color: var(--p-text-muted-color, #6b7280);
}
</style>