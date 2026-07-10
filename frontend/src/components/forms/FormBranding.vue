<script setup lang="ts">
import { computed, ref, watch } from "vue";
import InputText from "primevue/inputtext";
import Select from "primevue/select";
import ColorPicker from "primevue/colorpicker";
import Button from "primevue/button";
import type { Branding } from "@/types/branding";

interface ThemeOption {
  label: string;
  value: string | null;
}

const THEME_OPTIONS: Array<ThemeOption> = [
  { label: "Lara (per defecte)", value: "lara" },
  { label: "Aura", value: "aura" },
  { label: "Material", value: "material" },
  { label: "Nora", value: "nora" },
];

const HEX_COLOR_REGEX = /^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$/;

const props = defineProps<{
  modelValue: Branding;
  saving: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", value: Branding): void;
  (e: "submit"): void;
  (e: "reset"): void;
}>();

const colorPickerValue = ref("#0ea5e9");

const draft = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value),
});

const colorError = computed(() => {
  if (!draft.value.primaryColor) return null;
  if (!HEX_COLOR_REGEX.test(draft.value.primaryColor)) {
    return "El color ha de ser un codi hexadecimal (#RRGGBB o #RRGGBBAA)";
  }
  return null;
});

watch(
  () => draft.value.primaryColor,
  (val) => {
    if (val && HEX_COLOR_REGEX.test(val)) {
      colorPickerValue.value = val;
    }
  },
  { immediate: true },
);

function onColorPickerChange(value: any) {
  if (typeof value === "string") {
    draft.value.primaryColor = value;
  } else if (value && typeof value === "object" && "hex" in value) {
    draft.value.primaryColor = String((value as any).hex);
  }
}

function submit() {
  emit("submit");
}
</script>

<template>
  <div class="form-branding">
    <div class="form-branding__field">
      <label for="branding-theme">Tema</label>
      <Select
        input-id="branding-theme"
        v-model="draft.theme"
        :options="THEME_OPTIONS"
        option-label="label"
        option-value="value"
        placeholder="Lara (per defecte)"
        class="w-full"
      />
    </div>

    <div class="form-branding__field">
      <label for="branding-color">Color primari</label>
      <div class="form-branding__color">
        <ColorPicker
          input-id="branding-color"
          v-model="colorPickerValue"
          @change="onColorPickerChange(colorPickerValue)"
          format="hex"
        />
        <InputText
          v-model="draft.primaryColor"
          placeholder="#0ea5e9"
          class="form-branding__color-input"
          :class="{ 'p-invalid': !!colorError }"
        />
      </div>
      <small v-if="colorError" class="form-branding__error">
        {{ colorError }}
      </small>
      <small v-else class="form-branding__hint">
        Hexadecimal: #RRGGBB o #RRGGBBAA.
      </small>
    </div>

    <div class="form-branding__field">
      <label for="branding-title">Títol del sidebar</label>
      <InputText
        input-id="branding-title"
        v-model="draft.titleSidebar"
        placeholder="Lilith"
        class="w-full"
      />
    </div>

    <div class="form-branding__field">
      <label for="branding-logo-main">URL logo principal (capçalera)</label>
      <InputText
        input-id="branding-logo-main"
        v-model="draft.logoMain"
        placeholder="/assets/branding/logo-main.png o URL externa"
        class="w-full"
      />
    </div>

    <div class="form-branding__field">
      <label for="branding-logo-sidebar">URL logo del sidebar</label>
      <InputText
        input-id="branding-logo-sidebar"
        v-model="draft.logoSidebar"
        placeholder="/assets/branding/logo-sidebar.png o URL externa"
        class="w-full"
      />
    </div>

    <div class="form-branding__actions">
      <Button
        label="Desar"
        icon="pi pi-save"
        :loading="saving"
        :disabled="!!colorError"
        @click="submit"
      />
      <Button
        label="Descartar canvis"
        icon="pi pi-times"
        severity="secondary"
        text
        :disabled="saving"
        @click="emit('reset')"
      />
    </div>
  </div>
</template>

<style scoped>
.form-branding__field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}
.form-branding__color {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}
.form-branding__color-input {
  flex: 1;
}
.form-branding__hint {
  color: var(--p-text-muted-color, #6b7280);
  font-size: 0.85rem;
}
.form-branding__error {
  color: var(--p-red-500, #ef4444);
  font-size: 0.85rem;
}
.form-branding__actions {
  display: flex;
  gap: 0.75rem;
  margin-top: 1rem;
}
</style>