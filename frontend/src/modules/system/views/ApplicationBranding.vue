<template>
  <div class="card">
    <ProgressSpinner v-if="loading || !branding" />
    <template v-else>
      <FormApplicationBranding
        :branding="branding"
        :can-edit="canEdit"
        :saving="saving"
        @submit="saveBranding"
      />

      <Message v-if="!canEdit" severity="info" class="mt-3">
        {{ t("branding.noPermission") }}
      </Message>

      <!-- The logos upload and delete through their own endpoints as soon as a
      file is chosen, so they sit next to the form instead of in its payload. -->
      <div class="branding-logos mt-4">
        <div class="branding-logo-card">
          <label class="block text-900 mb-2">{{ t("branding.logos.main.label") }}</label>
          <small class="branding-hint">{{ t("branding.logos.main.hint") }}</small>
          <img
            v-if="brandingStore.hasMainLogo"
            :src="brandingStore.mainLogoUrl"
            :alt="brandingStore.brandName"
            class="branding-preview"
          />
          <div v-else class="branding-preview branding-preview--empty">
            {{ t("branding.logos.main.empty") }}
          </div>
          <div class="branding-logo-actions">
            <FileUpload
              mode="basic"
              custom-upload
              auto
              :choose-label="t('branding.logos.select')"
              accept="image/png,image/jpeg,image/webp"
              :max-file-size="MAX_LOGO_SIZE"
              :invalid-file-size-message="t('branding.logos.fileSizeError')"
              :choose-button-props="{ loading: processingSlot === 'main' }"
              :disabled="!canEdit || processingSlot !== null"
              @select="uploadLogo('main', $event)"
            />
            <Button
              :label="t('branding.logos.delete')"
              severity="secondary"
              text
              :disabled="!canEdit || processingSlot !== null || !brandingStore.hasMainLogo"
              @click="removeLogo('main')"
            />
          </div>
        </div>

        <div class="branding-logo-card">
          <label class="block text-900 mb-2">{{ t("branding.logos.sidebar.label") }}</label>
          <img
            v-if="brandingStore.hasSidebarLogo"
            :src="brandingStore.sidebarLogoUrl"
            :alt="brandingStore.brandName"
            class="branding-preview branding-preview--dark"
          />
          <div v-else class="branding-preview branding-preview--dark branding-preview--empty">
            {{ t("branding.logos.sidebar.empty") }}
          </div>
          <div class="branding-logo-actions">
            <FileUpload
              mode="basic"
              custom-upload
              auto
              :choose-label="t('branding.logos.select')"
              accept="image/png,image/jpeg,image/webp"
              :max-file-size="MAX_LOGO_SIZE"
              :invalid-file-size-message="t('branding.logos.fileSizeError')"
              :choose-button-props="{ loading: processingSlot === 'sidebar' }"
              :disabled="!canEdit || processingSlot !== null"
              @select="uploadLogo('sidebar', $event)"
            />
            <Button
              :label="t('branding.logos.delete')"
              severity="secondary"
              text
              :disabled="!canEdit || processingSlot !== null || !brandingStore.hasSidebarLogo"
              @click="removeLogo('sidebar')"
            />
          </div>
        </div>
      </div>

      <!-- Like the logos, the watermark switch and image save as soon as they change. -->
      <section class="branding-documents mt-5">
        <h3 class="branding-section-title">{{ t("branding.documents.title") }}</h3>
        <p class="branding-hint">{{ t("branding.documents.description") }}</p>

        <div class="branding-logo-card mt-3">
          <div class="flex align-items-center gap-2">
            <Checkbox
              v-model="watermarkEnabled"
              input-id="branding-watermark-enabled"
              binary
              :disabled="!canEdit || processingSlot !== null || savingWatermark"
              @update:model-value="saveWatermarkEnabled"
            />
            <label for="branding-watermark-enabled" class="text-900">
              {{ t("branding.documents.watermark.enabled") }}
            </label>
          </div>
          <small class="branding-hint">{{ t("branding.documents.watermark.hint") }}</small>
          <img
            :src="brandingStore.watermarkUrl"
            :alt="t('branding.documents.watermark.label')"
            class="branding-preview branding-preview--watermark"
            :class="{ 'branding-preview--muted': !brandingStore.watermarkEnabled }"
          />
          <small class="branding-hint">
            {{
              brandingStore.hasWatermark
                ? t("branding.documents.watermark.custom")
                : t("branding.documents.watermark.default")
            }}
          </small>
          <div class="branding-logo-actions">
            <FileUpload
              mode="basic"
              custom-upload
              auto
              :choose-label="t('branding.logos.select')"
              accept="image/png,image/jpeg,image/webp"
              :max-file-size="MAX_LOGO_SIZE"
              :invalid-file-size-message="t('branding.logos.fileSizeError')"
              :choose-button-props="{ loading: processingSlot === 'watermark' }"
              :disabled="!canEdit || processingSlot !== null || !brandingStore.watermarkEnabled"
              @select="uploadLogo('watermark', $event)"
            />
            <Button
              :label="t('branding.documents.watermark.restoreDefault')"
              severity="secondary"
              text
              :disabled="!canEdit || processingSlot !== null || !brandingStore.hasWatermark"
              @click="removeLogo('watermark')"
            />
          </div>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { isAxiosError } from "axios";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import FileUpload from "primevue/fileupload";
import type { FileUploadSelectEvent } from "primevue/fileupload";
import Message from "primevue/message";
import ProgressSpinner from "primevue/progressspinner";

import FormApplicationBranding from "../components/FormApplicationBranding.vue";
import {
  brandingService,
  type BrandingLogoSlot,
  type BrandingUpdateRequest,
} from "@/services/branding.service";
import { useBrandingStore } from "@/store/branding";
import { useStore } from "@/store";

const MAX_LOGO_SIZE = 2 * 1024 * 1024;

const { t } = useI18n();
const toast = useToast();
const brandingStore = useBrandingStore();
const appStore = useStore();
const loading = ref(true);
const saving = ref(false);
const savingWatermark = ref(false);
const watermarkEnabled = ref(true);
const processingSlot = ref<BrandingLogoSlot | null>(null);
const canEdit = computed(() => appStore.role?.toLowerCase() === "admin");

// Replaced only on load and after a successful save, so logo uploads, which
// also reload the store, keep unsaved brand name and palette edits.
const branding = ref<BrandingUpdateRequest | null>(null);
const brandingFromStore = (): BrandingUpdateRequest => ({
  brandName: brandingStore.brandName,
  primaryColor: brandingStore.primaryColor,
});

const errorMessage = (error: unknown): string =>
  isAxiosError(error) && error.response?.status === 403
    ? t("branding.toasts.noPermission")
    : t("branding.toasts.error");

const saveBranding = async (request: BrandingUpdateRequest) => {
  if (!canEdit.value || saving.value) return;

  saving.value = true;
  try {
    await brandingService.updateCurrent(request);
    await brandingStore.initialize();
    branding.value = brandingFromStore();
    toast.add({ severity: "success", summary: t("branding.toasts.updated"), life: 5000 });
  } catch (error: unknown) {
    toast.add({ severity: "error", summary: errorMessage(error), life: 5000 });
  } finally {
    saving.value = false;
  }
};

const saveWatermarkEnabled = async (enabled: boolean) => {
  if (!canEdit.value) return;

  savingWatermark.value = true;
  try {
    await brandingService.updateCurrentWatermark(enabled);
    await brandingStore.initialize();
    toast.add({ severity: "success", summary: t("branding.toasts.updated"), life: 5000 });
  } catch (error: unknown) {
    toast.add({ severity: "error", summary: errorMessage(error), life: 5000 });
  } finally {
    watermarkEnabled.value = brandingStore.watermarkEnabled;
    savingWatermark.value = false;
  }
};

const uploadLogo = async (slot: BrandingLogoSlot, event: FileUploadSelectEvent) => {
  const file = (event.files as File[])[0];
  if (!file || !canEdit.value) return;

  if (file.size > MAX_LOGO_SIZE) {
    toast.add({ severity: "warn", summary: t("branding.logos.fileSizeError"), life: 5000 });
    return;
  }

  processingSlot.value = slot;
  try {
    await brandingService.uploadCurrentLogo(slot, file);
    await brandingStore.initialize();
    toast.add({
      severity: "success",
      summary: t(slot === "watermark" ? "branding.toasts.watermarkUpdated" : "branding.toasts.logoUpdated"),
      life: 5000,
    });
  } catch (error: unknown) {
    toast.add({ severity: "error", summary: errorMessage(error), life: 5000 });
  } finally {
    processingSlot.value = null;
  }
};

const removeLogo = async (slot: BrandingLogoSlot) => {
  if (!canEdit.value) return;

  processingSlot.value = slot;
  try {
    await brandingService.removeCurrentLogo(slot);
    await brandingStore.initialize();
    toast.add({
      severity: "success",
      summary: t(slot === "watermark" ? "branding.toasts.watermarkRestored" : "branding.toasts.logoDeleted"),
      life: 5000,
    });
  } catch (error: unknown) {
    toast.add({ severity: "error", summary: errorMessage(error), life: 5000 });
  } finally {
    processingSlot.value = null;
  }
};

onMounted(async () => {
  appStore.setMenuItem({
    icon: PrimeIcons.PALETTE,
    backButtonVisible: false,
    title: t("branding.pageTitle"),
  });
  await brandingStore.initialize();
  branding.value = brandingFromStore();
  watermarkEnabled.value = brandingStore.watermarkEnabled;
  loading.value = false;
});
</script>

<style scoped>
.branding-logos {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.branding-logo-card {
  display: grid;
  gap: 0.75rem;
  align-content: start;
}

.branding-logo-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.branding-preview {
  width: 100%;
  height: 7rem;
  object-fit: contain;
  border: 1px solid var(--p-surface-300);
  border-radius: 0.5rem;
  padding: 0.75rem;
}

.branding-preview--empty {
  display: grid;
  place-items: center;
  color: var(--p-text-muted-color);
  text-align: center;
}

.branding-preview--dark {
  background: var(--p-primary-900);
}

.branding-preview--watermark {
  max-width: 14rem;
  height: 12rem;
  background: #fff;
}

.branding-preview--muted {
  opacity: 0.35;
}

.branding-section-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.branding-hint {
  display: block;
  margin: 0;
  color: var(--p-text-muted-color);
}

@media (max-width: 768px) {
  .branding-logos {
    grid-template-columns: 1fr;
  }
}
</style>
