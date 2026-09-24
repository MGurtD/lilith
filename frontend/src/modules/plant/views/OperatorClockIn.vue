<template>
  <main class="clockin">
    <form class="clockin__entry" @submit.prevent="onSubmit">
      <!-- One question, one field, one line under it: the line says how to answer
           and turns into the error in place, so the keypad never moves. -->
      <div class="clockin__field">
        <label for="operator-code" class="clockin__label">
          {{ t("shopfloor.clockin.codeLabel") }}
        </label>
        <!-- Scanners type into the focused field, so the keypad never takes focus.
             The on-screen keyboard only opens when the operator asks for letters. -->
        <InputText
          id="operator-code"
          ref="codeInput"
          v-model="operatorCode"
          type="password"
          autocomplete="off"
          :inputmode="letters ? 'text' : 'none'"
          :invalid="notFound"
          aria-describedby="operator-code-help"
          autofocus
          class="clockin__code"
          @input="notFound = false"
        />
        <p
          id="operator-code-help"
          class="clockin__help"
          :class="{ 'clockin__help--error': notFound }"
          :role="notFound ? 'alert' : undefined"
        >
          <i
            :class="notFound ? 'pi pi-exclamation-circle' : 'pi pi-barcode'"
            aria-hidden="true"
          ></i>
          <span>{{
            notFound ? t("shopfloor.clockin.notFound") : t("shopfloor.clockin.instructions")
          }}</span>
        </p>
      </div>

      <div class="clockin__keypad" role="group" :aria-label="t('shopfloor.clockin.keypad')">
        <button
          v-for="digit in DIGITS"
          :key="digit"
          type="button"
          class="clockin__key"
          @pointerdown.prevent
          @click="press(digit)"
        >
          {{ digit }}
        </button>
        <button
          type="button"
          class="clockin__key clockin__key--soft"
          :aria-label="letters ? t('shopfloor.clockin.keypad') : t('shopfloor.clockin.letters')"
          :aria-pressed="letters"
          @pointerdown.prevent
          @click="toggleLetters"
        >
          {{ letters ? "123" : "ABC" }}
        </button>
        <button type="button" class="clockin__key" @pointerdown.prevent @click="press('0')">0</button>
        <button
          type="button"
          class="clockin__key clockin__key--soft"
          :aria-label="t('shopfloor.clockin.backspace')"
          @pointerdown.prevent
          @click="backspace"
        >
          <i class="pi pi-delete-left" aria-hidden="true"></i>
        </button>
      </div>

      <Button
        type="submit"
        :label="t('shopfloor.clockin.buttonLabel')"
        icon="pi pi-arrow-right"
        iconPos="right"
        :disabled="!operatorCode.trim()"
        class="clockin__submit"
      />
    </form>

    <!-- Brand panel: the time, readable from across the shop floor, and the title block. -->
    <aside class="clockin__panel">
      <div class="clockin__time">{{ time }}</div>
      <div class="clockin__date">{{ date }}</div>
      <TitleBlock
        class="clockin__title-block"
        :top="{ label: t('ui.titleBlock.system'), value: 'Zenith ERP' }"
        :cells="[
          { label: t('ui.titleBlock.company'), value: brandingStore.brandName, strong: true },
          { label: t('ui.titleBlock.screen'), value: t('shopfloor.clockin.title') },
          { label: t('ui.titleBlock.scale'), value: '1:1' },
        ]"
      />
    </aside>
  </main>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref } from "vue";
import { useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import InputText from "primevue/inputtext";
import { useI18n } from "vue-i18n";
import TitleBlock from "@/components/brand/TitleBlock.vue";
import { useBrandingStore } from "@/store/branding";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { usePlantOperatorStore } from "../store";
import { useStore } from "../../../store";

const DIGITS = ["1", "2", "3", "4", "5", "6", "7", "8", "9"];
const MAX_CODE_LENGTH = 32;

const store = useStore();
const brandingStore = useBrandingStore();
const router = useRouter();
const plantModelStore = usePlantModelStore();
const plantOperatorStore = usePlantOperatorStore();
const { t } = useI18n();

const operatorCode = ref("");
const notFound = ref(false);
const letters = ref(false);
const codeInput = ref<{ $el: HTMLInputElement }>();

const focusCode = () => codeInput.value?.$el.focus();

const press = (key: string) => {
  notFound.value = false;
  operatorCode.value = (operatorCode.value + key).slice(0, MAX_CODE_LENGTH);
  focusCode();
};

const backspace = () => {
  notFound.value = false;
  operatorCode.value = operatorCode.value.slice(0, -1);
  focusCode();
};

// inputmode changes only apply on the next focus, so refocus after toggling.
const toggleLetters = async () => {
  letters.value = !letters.value;
  codeInput.value?.$el.blur();
  await nextTick();
  focusCode();
};

// Clock for the brand panel, refreshed often enough to never show a stale minute.
const now = ref(new Date());
let clockTimer: ReturnType<typeof setInterval> | undefined;
const time = computed(() =>
  new Intl.DateTimeFormat(store.language.current, { hour: "2-digit", minute: "2-digit" }).format(
    now.value,
  ),
);
const date = computed(() =>
  new Intl.DateTimeFormat(store.language.current, {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(now.value),
);

onMounted(async () => {
  clockTimer = setInterval(() => (now.value = new Date()), 15000);

  // Pre-cargar operadores
  await plantModelStore.fetchOperators();

  // El guard se encargará de la navegación
  await plantOperatorStore.getOperator();

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    title: t("plant.fitxatge-operador"),
  });
});

onUnmounted(() => clearInterval(clockTimer));

const onSubmit = async () => {
  const code = operatorCode.value.trim();
  if (!code) return;

  const operator = plantModelStore.operators?.find((op) => op.code === code);

  if (operator) {
    await plantOperatorStore.setOperator(operator);
    router.push({ name: "SiteAreas" });
  } else {
    // The message stays next to the field; clear it for a quick retry or rescan.
    notFound.value = true;
    operatorCode.value = "";
    focusCode();
  }
};
</script>

<style scoped>
.clockin {
  display: grid;
  grid-template-columns: minmax(0, 29rem) minmax(0, 1fr);
  min-height: calc(100vh - var(--top-panel-height) - 2rem);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius-md);
  overflow: hidden;
}

.clockin__entry {
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 1.25rem;
  padding: 1.75rem 2.25rem;
  background: var(--p-surface-0);
}

.clockin__field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.clockin__label {
  font-family: var(--font-condensed);
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--p-text-color);
}

/* Masked code: large dots, centred in the field (no placeholder to misalign). */
.clockin__code {
  width: 100%;
  height: 4rem;
  padding: 0 1.125rem;
  font-family: var(--font-condensed);
  font-size: 2.125rem;
  line-height: 1;
  letter-spacing: 0.5rem;
}

/* The field keeps focus after a miss, so the error border must win over focus. */
.clockin__code.p-invalid,
.clockin__code.p-invalid:focus {
  border-color: var(--p-red-600);
}

/* Fixed height: switching between help and error never moves the keypad. */
.clockin__help {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  min-height: 3rem;
  margin: 0;
  font-size: 1rem;
  line-height: 1.5rem;
  color: var(--p-text-muted-color);
}

.clockin__help .pi {
  flex-shrink: 0;
  font-size: 1.125rem;
  line-height: 1.5rem;
}

.clockin__help--error {
  font-weight: 500;
  color: var(--p-red-700);
}

.clockin__keypad {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.625rem;
}

.clockin__key {
  all: unset;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 4.5rem;
  border-radius: var(--p-border-radius-lg);
  background: var(--p-surface-0);
  box-shadow:
    inset 0 0 0 1px var(--p-surface-300),
    0 1px 0 var(--p-surface-300);
  font-family: var(--font-condensed);
  font-size: 1.875rem;
  font-weight: 600;
  color: var(--p-text-color);
  cursor: pointer;
  user-select: none;
  touch-action: manipulation;
}

.clockin__key:active {
  background: var(--p-surface-100);
}

.clockin__key:focus-visible {
  outline: 3px solid var(--p-primary-color);
  outline-offset: 2px;
}

.clockin__key--soft {
  font-size: 1.0625rem;
  font-weight: 500;
  color: var(--p-surface-700);
  background: var(--p-surface-50);
}

.clockin__key--soft .pi {
  font-size: 1.5rem;
}

.clockin__submit {
  height: 4rem;
  font-size: 1.25rem;
}

.clockin__panel {
  position: relative;
  display: flex;
  flex-direction: column;
  padding: 2.5rem 2rem 12rem;
  background: var(--p-surface-50);
  border-left: 1px solid var(--p-surface-200);
}

.clockin__time {
  font-family: var(--font-condensed);
  font-size: 6rem;
  line-height: 1;
  font-weight: 600;
  letter-spacing: -0.0625rem;
  font-variant-numeric: tabular-nums;
  color: var(--p-primary-color);
}

.clockin__date {
  margin-top: 0.5rem;
  font-size: 1.25rem;
  color: var(--p-surface-700);
}

.clockin__date::first-letter {
  text-transform: uppercase;
}

.clockin__title-block {
  position: absolute;
  left: 2rem;
  right: 2rem;
  bottom: 2rem;
}

/* Portrait tablets and phones: the entry alone. */
@media (max-width: 899.98px) {
  .clockin {
    grid-template-columns: minmax(0, 1fr);
  }

  .clockin__entry {
    width: 100%;
    max-width: 29rem;
    box-sizing: border-box;
    justify-self: center;
    padding: 1.5rem 1rem;
  }

  .clockin__panel {
    display: none;
  }
}
</style>
