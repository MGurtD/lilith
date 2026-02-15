<template>
  <main class="clockin">
    <div class="clockin__container">
      <!-- Logo Section -->
      <div class="clockin__header">
        <img
          src="../../../assets/images/logo.jpg"
          alt="Logo Temges"
          class="clockin__logo"
        />
        <h1 class="clockin__title">{{ $t("shopfloor.clockin.title") }}</h1>
      </div>

      <!-- Input Section -->
      <div class="clockin__form">
        <InputGroup class="clockin__input-group">
          <InputGroupAddon>
            <i :class="PrimeIcons.USER"></i>
          </InputGroupAddon>
          <Password
            v-model="operatorCode"
            :placeholder="$t('shopfloor.clockin.placeholder')"
            class="clockin__input"
            autofocus
            @keyup.enter="onSubmit"
            :feedback="false"
            inputClass="clockin__password-field"
          />
        </InputGroup>

        <Button
          :label="$t('shopfloor.clockin.buttonLabel')"
          :icon="PrimeIcons.ARROW_RIGHT"
          icon-pos="right"
          size="large"
          class="clockin__button"
          :disabled="!operatorCode.trim()"
          @click="onSubmit"
        />
      </div>

      <!-- Visual Feedback -->
      <div class="clockin__hint" v-if="!operatorCode.trim()">
        <i :class="PrimeIcons.INFO_CIRCLE"></i>
        <span>{{ $t("shopfloor.clockin.instructions") }}</span>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import Password from "primevue/password";
import InputGroup from "primevue/inputgroup";
import InputGroupAddon from "primevue/inputgroupaddon";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { usePlantOperatorStore } from "../store";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";
import { useStore } from "../../../store";

const store = useStore();
const router = useRouter();
const plantModelStore = usePlantModelStore();
const plantOperatorStore = usePlantOperatorStore();
const toast = useToast();
const { t } = useI18n();
const operatorCode = ref("");

onMounted(async () => {
  // Pre-cargar operadores
  await plantModelStore.fetchOperators();

  // El guard se encargará de la navegación
  await plantOperatorStore.getOperator();

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    title: "Fitxatge Operador",
  });
});

const onSubmit = async () => {
  if (!operatorCode.value.trim()) return;

  const operator = plantModelStore.operators?.find(
    (op) => op.code === operatorCode.value.trim()
  );

  if (operator) {
    // Guardar operador en el store
    await plantOperatorStore.setOperator(operator);

    // Navegar a la vista de áreas
    router.push({ name: "SiteAreas" });
  } else {
    toast.add({
      severity: "error",
      summary: t("shopfloor.clockin.errorTitle"),
      detail: t("shopfloor.clockin.errorDetail"),
      life: 4000,
    });
    // Clear input after error for quick retry
    operatorCode.value = "";
  }
};
</script>

<style scoped>
.clockin {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: calc(100vh - var(--top-panel-height) - 2rem);
}

.clockin__container {
  width: 100%;
  max-width: 500px;
  background: white;
  border-radius: 24px;
  padding: 2.5rem 2rem;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.4s ease-out;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.clockin__header {
  text-align: center;
  margin-bottom: 3rem;
}

.clockin__logo {
  height: 80px;
  width: auto;
  margin-bottom: 1.5rem;
  object-fit: contain;
}

.clockin__title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #2d3748;
  margin-bottom: 0.5rem;
  letter-spacing: -0.025em;
}

.clockin__instructions {
  font-size: 1rem;
  color: #718096;
  margin: 0;
  line-height: 1.5;
}

.clockin__form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.clockin__input-group {
  width: 100%;
}

.clockin__input-group :deep(.p-inputgroup-addon) {
  background: #f7fafc;
  border: 2px solid #e2e8f0;
  border-right: none;
  border-radius: 12px 0 0 12px;
  padding: 0 1rem;
  min-width: 3.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.clockin__input-group :deep(.p-inputgroup-addon i) {
  color: #a0aec0;
  font-size: 1.5rem;
}

.clockin__input {
  flex: 1;
}

.clockin__input :deep(.p-password) {
  width: 100%;
}

.clockin__input :deep(.p-password-input),
.clockin__password-field {
  width: 100%;
  padding: 1.25rem 1.25rem !important;
  font-size: 1.125rem !important;
  text-align: left !important;
  border: 2px solid #e2e8f0 !important;
  border-left: none !important;
  border-radius: 0 12px 12px 0 !important;
  transition: all 0.3s ease !important;
  background: #f7fafc !important;
}

.clockin__input :deep(.p-password-input:focus),
.clockin__password-field:focus {
  border-color: #667eea !important;
  background: white !important;
  box-shadow: none !important;
  outline: none !important;
}

.clockin__input-group:focus-within :deep(.p-inputgroup-addon) {
  border-color: #667eea;
  background: white;
}

.clockin__button {
  width: 100%;
  padding: 1.25rem !important;
  font-size: 1.25rem !important;
  font-weight: 600 !important;
  border-radius: 12px !important;
  background: #667eea !important;
  border: none !important;
  transition: all 0.3s ease !important;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.clockin__button:not(:disabled):hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.4) !important;
}

.clockin__button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  background: #cbd5e0 !important;
}

.clockin__hint {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1.5rem;
  padding: 1rem;
  background: #edf2f7;
  border-radius: 8px;
  color: #4a5568;
  font-size: 0.875rem;
  text-align: center;
}

.clockin__hint i {
  font-size: 1rem;
  color: #667eea;
}

/* Tablet and larger screens */
@media (min-width: 640px) {
  .clockin__container {
    padding: 3rem 2.5rem;
  }

  .clockin__logo {
    height: 100px;
  }

  .clockin__title {
    font-size: 2rem;
  }

  .clockin__instructions {
    font-size: 1.125rem;
  }

  .clockin__input :deep(.p-password-input),
  .clockin__password-field {
    font-size: 1.5rem !important;
  }

  .clockin__input-group :deep(.p-inputgroup-addon) {
    min-width: 4rem;
  }

  .clockin__input-group :deep(.p-inputgroup-addon i) {
    font-size: 1.75rem;
  }

  .clockin__button {
    font-size: 1.5rem !important;
  }
}

/* Large screens */
@media (min-width: 1024px) {
  .clockin__container {
    max-width: 600px;
    padding: 4rem 3rem;
  }

  .clockin__logo {
    height: 120px;
  }

  .clockin__title {
    font-size: 2.25rem;
  }
}

/* Touch-friendly adjustments */
@media (hover: none) and (pointer: coarse) {
  .clockin__input :deep(.p-password-input),
  .clockin__password-field {
    padding: 1.5rem 1.25rem !important;
  }

  .clockin__input-group :deep(.p-inputgroup-addon) {
    padding: 1.5rem 1rem;
  }

  .clockin__button {
    padding: 1.75rem !important;
  }
}
</style>
