<template>
  <div class="phase-quantity-form">
    <div class="info-section">
      <div class="produced-units-row">
        <div class="produced-column">
          <h4 class="section-title">
            <i :class="PrimeIcons.CHECK_CIRCLE" class="mr-2"></i>{{ $t("plant.quantitat-produida") }}</h4>
          <div class="produced-unit-card ok">
            <span class="produced-value">{{ quantityOk }}</span>
          </div>
        </div>
        <div class="produced-column">
          <h4 class="section-title">
            <i :class="PrimeIcons.EXCLAMATION_TRIANGLE" class="mr-2"></i>{{ $t("plant.quantitat-defectuosa") }}</h4>
          <div class="produced-unit-card ko">
            <span class="produced-value">{{ quantityKo }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="input-section">
      <h4 class="section-title">
        <i :class="PrimeIcons.PLUS_CIRCLE" class="mr-2"></i>{{ $t("plant.afegir-mes-quantitat") }}</h4>
      <p class="section-hint">{{ $t("plant.introdueix-la-quantitat-addicional-produida-en-aquesta-sessio") }}</p>
      <div class="counters-row">
        <div class="counter-field">
          <InputNumber
            :model-value="counterOk"
            :min="0"
            :useGrouping="false"
            class="w-full"
            showButtons
            buttonLayout="horizontal"
            :step="1"
            decrementButtonClass="p-button-secondary"
            incrementButtonClass="p-button-secondary"
            incrementButtonIcon="pi pi-plus"
            decrementButtonIcon="pi pi-minus"
            @update:model-value="emit('update:counterOk', $event ?? 0)"
          />
        </div>
        <div class="counter-field">
          <InputNumber
            :model-value="counterKo"
            :min="0"
            :useGrouping="false"
            class="w-full"
            showButtons
            buttonLayout="horizontal"
            :step="1"
            decrementButtonClass="p-button-secondary"
            incrementButtonClass="p-button-secondary"
            incrementButtonIcon="pi pi-plus"
            decrementButtonIcon="pi pi-minus"
            @update:model-value="emit('update:counterKo', $event ?? 0)"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";

interface Props {
  quantityOk: number;
  quantityKo: number;
  counterOk: number;
  counterKo: number;
}

defineProps<Props>();

const emit = defineEmits<{
  (event: "update:counterOk", value: number): void;
  (event: "update:counterKo", value: number): void;
}>();
</script>

<style scoped>
.phase-quantity-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.section-title {
  margin: 0 0 0.75rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text-color);
  display: flex;
  align-items: center;
}

.section-hint {
  margin: 0 0 1rem 0;
  font-size: 0.85rem;
  color: var(--text-color-secondary);
}

.info-section {
  background: var(--p-surface-50);
  border-radius: 8px;
  padding: 1rem;
}

.produced-units-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.produced-column {
  display: flex;
  flex-direction: column;
}

.produced-unit-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  border-radius: 8px;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
}

.produced-unit-card.ok {
  border-left: 4px solid var(--p-green-500);
}

.produced-unit-card.ko {
  border-left: 4px solid var(--p-red-500);
}

.produced-value {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--text-color);
}

.input-section {
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
  border-radius: 8px;
  padding: 1rem;
}

.counters-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.counter-field {
  display: flex;
  flex-direction: column;
}

@media (max-width: 768px) {
  .produced-units-row,
  .counters-row {
    grid-template-columns: 1fr;
  }
}
</style>
