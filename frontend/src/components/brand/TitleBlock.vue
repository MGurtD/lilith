<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { DEFAULT_MAIN_LOGO } from "@/config/branding";
import { useBrandingStore } from "@/store/branding";

export interface TitleBlockCell {
  label: string;
  value: string;
  strong?: boolean;
}

// A drawing's title block ("caixetí"): the tenant's logo and one cell on top,
// three cells below. The top rule carries the tenant's colour.
defineProps<{
  top: TitleBlockCell;
  cells: [TitleBlockCell, TitleBlockCell, TitleBlockCell];
}>();

const { t } = useI18n();
const brandingStore = useBrandingStore();
const logoLoadFailed = ref(false);
</script>

<template>
  <section class="title-block" :aria-label="t('ui.titleBlock.label')">
    <div class="title-block__logo">
      <img
        :src="logoLoadFailed ? DEFAULT_MAIN_LOGO : brandingStore.mainLogoUrl"
        :alt="brandingStore.brandName"
        draggable="false"
        @error="logoLoadFailed = true"
      />
    </div>
    <div class="title-block__cell title-block__cell--top">
      <span class="title-block__label">{{ top.label }}</span>
      <span class="title-block__value" :class="{ 'title-block__value--strong': top.strong }">{{
        top.value
      }}</span>
    </div>
    <div v-for="cell in cells" :key="cell.label" class="title-block__cell">
      <span class="title-block__label">{{ cell.label }}</span>
      <span class="title-block__value" :class="{ 'title-block__value--strong': cell.strong }">{{
        cell.value
      }}</span>
    </div>
  </section>
</template>

<style scoped>
.title-block {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-700);
  border-top: 6px solid var(--p-primary-color);
  border-radius: 2px;
}

.title-block__logo {
  grid-column: span 2;
  display: flex;
  align-items: center;
  min-height: 4.25rem;
  padding: 0.75rem 1rem;
  border-right: 1px solid var(--p-surface-200);
  border-bottom: 1px solid var(--p-surface-200);
}

.title-block__logo img {
  max-height: 2.75rem;
  max-width: 100%;
  object-fit: contain;
}

.title-block__cell {
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 0.125rem;
  padding: 0.625rem 0.875rem;
  min-width: 0;
}

.title-block__cell--top {
  border-bottom: 1px solid var(--p-surface-200);
}

.title-block__cell:not(.title-block__cell--top):not(:last-child) {
  border-right: 1px solid var(--p-surface-200);
}

.title-block__label {
  font-family: var(--font-condensed);
  font-size: 0.8571rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.title-block__value {
  font-weight: 500;
  color: var(--p-text-color);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.title-block__value--strong {
  font-family: var(--font-condensed);
  font-size: 1.2143rem;
  font-weight: 600;
}
</style>
