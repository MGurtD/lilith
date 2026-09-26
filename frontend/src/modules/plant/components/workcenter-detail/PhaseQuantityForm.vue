<template>
  <div class="qty-form">
    <div
      v-for="counter in counters"
      :key="counter.key"
      class="qty-counter"
      role="group"
      :aria-labelledby="`qty-${counter.key}-${uid}`"
    >
      <div class="qty-counter__head">
        <span :id="`qty-${counter.key}-${uid}`" class="qty-counter__label">
          <span class="qty-counter__dot" :class="`qty-counter__dot--${counter.key}`"></span>
          {{ counter.label }}
        </span>
        <span class="qty-counter__declared">{{
          t("plant.declare.declared", { count: counter.declared })
        }}</span>
      </div>
      <div class="qty-counter__row">
        <button
          type="button"
          class="qty-key"
          :aria-label="t('plant.declare.minus', { kind: counter.kind })"
          :disabled="counter.value === 0"
          @click="counter.set(counter.value - 1)"
        >
          <i class="pi pi-minus" aria-hidden="true"></i>
        </button>
        <input
          class="qty-counter__value"
          :class="{ 'qty-counter__value--zero': counter.value === 0 }"
          type="number"
          inputmode="numeric"
          min="0"
          :value="counter.value"
          :aria-label="counter.label"
          @input="counter.set(Number(($event.target as HTMLInputElement).value))"
          @focus="($event.target as HTMLInputElement).select()"
        />
        <button
          type="button"
          class="qty-key"
          :aria-label="t('plant.declare.plus', { kind: counter.kind })"
          @click="counter.set(counter.value + 1)"
        >
          <i class="pi pi-plus" aria-hidden="true"></i>
        </button>
      </div>
      <div class="qty-counter__quick">
        <button type="button" class="qty-quick" @click="counter.set(counter.value + 5)">+5</button>
        <button type="button" class="qty-quick" @click="counter.set(counter.value + 10)">+10</button>
        <button
          type="button"
          class="qty-quick qty-quick--reset"
          :disabled="counter.value === 0"
          @click="counter.set(0)"
        >
          {{ t("plant.declare.reset") }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, useId } from "vue";
import { useI18n } from "vue-i18n";

// Good and bad piece counters for the shop floor: 72px keys, +5/+10 and a
// field that accepts typing (plant-mes-redesign.md, task 5.1). Shared by
// "Declarar peces" and the phase close dialog.
interface Props {
  quantityOk: number;
  quantityKo: number;
  counterOk: number;
  counterKo: number;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  (event: "update:counterOk", value: number): void;
  (event: "update:counterKo", value: number): void;
}>();

const { t } = useI18n();
const uid = useId();

const clamp = (value: number) =>
  Number.isFinite(value) ? Math.max(0, Math.floor(value)) : 0;

const counters = computed(() => [
  {
    key: "ok",
    label: t("plant.declare.good"),
    kind: t("plant.declare.goodKind"),
    declared: props.quantityOk,
    value: props.counterOk,
    set: (value: number) => emit("update:counterOk", clamp(value)),
  },
  {
    key: "ko",
    label: t("plant.declare.bad"),
    kind: t("plant.declare.badKind"),
    declared: props.quantityKo,
    value: props.counterKo,
    set: (value: number) => emit("update:counterKo", clamp(value)),
  },
]);
</script>

<style scoped>
.qty-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1.5rem;
}

.qty-counter {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

.qty-counter__head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.5rem;
}

.qty-counter__label {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-family: var(--font-condensed);
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--p-steel-900);
}

.qty-counter__dot {
  width: 12px;
  height: 12px;
  border-radius: 3px;
}

.qty-counter__dot--ok {
  background: var(--p-steel-900);
}

.qty-counter__dot--ko {
  background: var(--p-red-700);
}

.qty-counter__declared {
  font-size: 0.875rem;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-600);
}

.qty-counter__row {
  display: flex;
  align-items: center;
  gap: 0.625rem;
}

.qty-key {
  flex-shrink: 0;
  width: 72px;
  height: 72px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: 6px;
  background: var(--p-surface-0);
  box-shadow:
    inset 0 0 0 1px var(--p-steel-300),
    0 1px 0 var(--p-steel-300);
  color: var(--p-steel-900);
  font-size: 1.5rem;
  cursor: pointer;
}

.qty-key i {
  font-size: 1.5rem;
}

.qty-key:active:not(:disabled) {
  background: var(--p-steel-100);
}

.qty-key:disabled {
  color: var(--p-steel-400);
  cursor: default;
}

.qty-counter__value {
  flex: 1;
  min-width: 0;
  height: 72px;
  box-sizing: border-box;
  border: none;
  border-radius: 6px;
  background: var(--p-steel-50);
  text-align: center;
  font-family: var(--font-condensed);
  font-size: 3rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-900);
  appearance: textfield;
  -moz-appearance: textfield;
}

.qty-counter__value::-webkit-outer-spin-button,
.qty-counter__value::-webkit-inner-spin-button {
  appearance: none;
  margin: 0;
}

.qty-counter__value--zero {
  color: var(--p-steel-500);
}

.qty-key:focus-visible,
.qty-quick:focus-visible,
.qty-counter__value:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: 2px;
}

.qty-counter__quick {
  display: flex;
  gap: 0.5rem;
}

.qty-quick {
  min-height: 44px;
  padding: 0 0.875rem;
  border: none;
  border-radius: 4px;
  background: var(--p-steel-50);
  font-family: var(--font-condensed);
  font-size: 1.0625rem;
  font-weight: 600;
  color: var(--p-steel-700);
  cursor: pointer;
}

.qty-quick--reset {
  margin-left: auto;
  font-family: inherit;
  font-size: 0.9375rem;
  font-weight: 500;
}

.qty-quick:disabled {
  color: var(--p-steel-400);
  cursor: default;
}

@media (max-width: 767.98px) {
  .qty-form {
    grid-template-columns: minmax(0, 1fr);
    gap: 1.25rem;
  }
}
</style>
