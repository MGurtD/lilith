<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { nullableStringValue } from "@/components/forms/value-utils";
import {
  BRANDING_PALETTE_OPTIONS,
  isBrandingPalette,
  normalizeBrandingPalette,
  type BrandingUpdateRequest,
} from "@/services/branding.service";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";

const props = withDefaults(
  defineProps<{
    branding: BrandingUpdateRequest;
    canEdit: boolean;
    saving?: boolean;
  }>(),
  { saving: false },
);

const emit = defineEmits<{
  (event: "submit", branding: BrandingUpdateRequest): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "brandName",
        label: t("branding.form.brandName"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .nullable()
          .max(60, t("branding.validation.brandNameMax")),
      },
      {
        name: "primaryColor",
        label: t("branding.form.palette"),
        type: FormFieldType.Custom,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  if (!props.canEdit) return;

  const brandName = nullableStringValue(
    values.brandName,
    props.branding.brandName,
  );

  emit("submit", {
    ...props.branding,
    brandName: brandName?.trim() || null,
    primaryColor: isBrandingPalette(values.primaryColor)
      ? normalizeBrandingPalette(values.primaryColor)
      : props.branding.primaryColor,
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="branding"
    :loading="saving"
    :disabled="!canEdit || saving"
    :show-submit="canEdit"
    :show-cancel="false"
    @submit="submit"
  >
    <template #field-primaryColor="{ value, setValue, disabled, inputId, config }">
      <div
        class="branding-palette-options"
        role="radiogroup"
        :aria-label="config.label"
      >
        <label
          v-for="option in BRANDING_PALETTE_OPTIONS"
          :key="option.value"
          class="branding-palette-option"
          :class="{ 'branding-palette-option--disabled': disabled }"
          :for="`${inputId}-${option.value}`"
        >
          <RadioButton
            :model-value="value"
            :name="inputId"
            :input-id="`${inputId}-${option.value}`"
            :value="option.value"
            :disabled="disabled"
            @update:model-value="setValue"
          />
          <span
            class="branding-palette-swatch"
            :style="{ backgroundColor: option.swatch }"
            aria-hidden="true"
          ></span>
          <span>{{ t(option.translationKey) }}</span>
        </label>
      </div>
    </template>
  </Form>
</template>

<style scoped>
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

.branding-palette-option--disabled {
  cursor: default;
  opacity: 0.6;
}

.branding-palette-option--disabled:hover {
  border-color: var(--p-surface-300);
  background: transparent;
}

.branding-palette-swatch {
  width: 1.25rem;
  height: 1.25rem;
  border-radius: 50%;
  border: 1px solid rgb(15 23 42 / 0.2);
  flex: 0 0 auto;
}
</style>
