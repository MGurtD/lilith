<template>
  <form class="branding-form" @submit.prevent="saveBranding">
    <div class="flex justify-content-end mb-3">
      <Button
        v-if="canEdit"
        type="submit"
        :label="t('branding.saveButton')"
        icon="pi pi-save"
        :loading="saving"
      />
    </div>

    <div class="two-columns">
      <BaseInput
        v-model="brandName"
        :label="t('branding.form.brandName')"
        id="application-brand-name"
        :disabled="!canEdit || saving"
        :class="{ 'p-invalid': validation.errors.brandName }"
      />
      <fieldset class="branding-palette-field" :disabled="!canEdit || saving">
        <legend>{{ t('branding.form.palette') }}</legend>
        <div class="branding-palette-options">
          <label
            v-for="option in BRANDING_PALETTE_OPTIONS"
            :key="option.value"
            class="branding-palette-option"
            :for="`branding-palette-${option.value}`"
          >
            <RadioButton
              v-model="primaryColor"
              name="branding-palette"
              :input-id="`branding-palette-${option.value}`"
              :value="option.value"
            />
            <span
              class="branding-palette-swatch"
              :style="{ backgroundColor: option.swatch }"
              aria-hidden="true"
            ></span>
            <span>{{ t(option.translationKey) }}</span>
          </label>
        </div>
      </fieldset>
    </div>

    <Message v-if="!canEdit" severity="info" class="mt-3">
      {{ t('branding.noPermission') }}
    </Message>

    <div class="branding-logos mt-4">
      <div class="branding-logo-card">
        <label class="block text-900 mb-2">{{ t('branding.logos.main.label') }}</label>
        <img
          v-if="brandingStore.hasMainLogo"
          :src="brandingStore.mainLogoUrl"
          :alt="brandingStore.brandName"
          class="branding-preview"
        />
        <div v-else class="branding-preview branding-preview--empty">
          {{ t('branding.logos.main.empty') }}
        </div>
        <div class="branding-logo-actions">
          <FileUpload
            mode="basic"
            custom-upload
            auto
            :choose-label="t('branding.logos.select')"
            accept="image/png,image/jpeg,image/webp"
            :max-file-size="2 * 1024 * 1024"
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
        <label class="block text-900 mb-2">{{ t('branding.logos.sidebar.label') }}</label>
        <img
          v-if="brandingStore.hasSidebarLogo"
          :src="brandingStore.sidebarLogoUrl"
          :alt="brandingStore.brandName"
          class="branding-preview branding-preview--dark"
        />
        <div v-else class="branding-preview branding-preview--dark branding-preview--empty">
          {{ t('branding.logos.sidebar.empty') }}
        </div>
        <div class="branding-logo-actions">
          <FileUpload
            mode="basic"
            custom-upload
            auto
            :choose-label="t('branding.logos.select')"
            accept="image/png,image/jpeg,image/webp"
            :max-file-size="2 * 1024 * 1024"
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
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { isAxiosError } from "axios";
import * as Yup from "yup";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import FileUpload from "primevue/fileupload";
import type { FileUploadSelectEvent } from "primevue/fileupload";

import BaseInput from "@/components/BaseInput.vue";
import {
  BRANDING_PALETTE_OPTIONS,
  brandingService,
  type BrandingLogoSlot,
  type BrandingPalette,
} from "@/services/branding.service";
import { useBrandingStore } from "@/store/branding";
import { FormValidation, FormValidationResult } from "@/utils/form-validator";

const props = defineProps<{ canEdit: boolean }>();

const { t } = useI18n();
const toast = useToast();
const brandingStore = useBrandingStore();
const brandName = ref(brandingStore.brandName);
const primaryColor = ref<BrandingPalette>(brandingStore.primaryColor);
const saving = ref(false);
const processingSlot = ref<BrandingLogoSlot | null>(null);

const schema = Yup.object({
  brandName: Yup.string()
    .nullable()
    .max(60, () => t("branding.validation.brandNameMax")),
});
const validation = ref({ result: false, errors: {} } as FormValidationResult);

const errorMessage = (error: unknown): string =>
  isAxiosError(error) && error.response?.status === 403
    ? t("branding.toasts.noPermission")
    : t("branding.toasts.error");

const saveBranding = async () => {
  validation.value = new FormValidation(schema).validate({
    brandName: brandName.value,
  });
  if (!validation.value.result || !props.canEdit) return;

  saving.value = true;
  try {
    await brandingService.updateCurrent({
      brandName: brandName.value?.trim() || null,
      primaryColor: primaryColor.value,
    });
    await brandingStore.initialize();
    brandName.value = brandingStore.brandName;
    primaryColor.value = brandingStore.primaryColor;
    toast.add({ severity: "success", summary: t("branding.toasts.updated"), life: 5000 });
  } catch (error: unknown) {
    toast.add({ severity: "error", summary: errorMessage(error), life: 5000 });
  } finally {
    saving.value = false;
  }
};

const uploadLogo = async (slot: BrandingLogoSlot, event: FileUploadSelectEvent) => {
  const file = (event.files as File[])[0];
  if (!file || !props.canEdit) return;

  if (file.size > 2 * 1024 * 1024) {
    toast.add({ severity: "warn", summary: t("branding.logos.fileSizeError"), life: 5000 });
    return;
  }

  processingSlot.value = slot;
  try {
    await brandingService.uploadCurrentLogo(slot, file);
    await brandingStore.initialize();
    toast.add({ severity: "success", summary: t("branding.toasts.logoUpdated"), life: 5000 });
  } catch (error: unknown) {
    toast.add({ severity: "error", summary: errorMessage(error), life: 5000 });
  } finally {
    processingSlot.value = null;
  }
};

const removeLogo = async (slot: BrandingLogoSlot) => {
  if (!props.canEdit) return;

  processingSlot.value = slot;
  try {
    await brandingService.removeCurrentLogo(slot);
    await brandingStore.initialize();
    toast.add({ severity: "success", summary: t("branding.toasts.logoDeleted"), life: 5000 });
  } catch (error: unknown) {
    toast.add({ severity: "error", summary: errorMessage(error), life: 5000 });
  } finally {
    processingSlot.value = null;
  }
};
</script>

<style scoped>
.branding-palette-field {
  border: 0;
  padding: 0;
  margin: 0;
  min-width: 0;
}

.branding-palette-field legend {
  color: var(--p-text-color);
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.branding-palette-options {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.branding-palette-option {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  border: 1px solid var(--p-surface-300);
  border-radius: 0.5rem;
  padding: 0.45rem 0.65rem;
  cursor: pointer;
  transition: border-color 0.15s ease, background-color 0.15s ease;
}

.branding-palette-option:hover {
  border-color: var(--p-primary-400);
  background: var(--p-surface-100);
}

.branding-palette-swatch {
  width: 1.25rem;
  height: 1.25rem;
  border-radius: 50%;
  border: 1px solid rgb(15 23 42 / 0.2);
  flex: 0 0 auto;
}

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

@media (max-width: 768px) {
  .branding-logos {
    grid-template-columns: 1fr;
  }
}
</style>
