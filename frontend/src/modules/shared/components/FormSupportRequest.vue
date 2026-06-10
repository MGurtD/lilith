<template>
  <form @submit.prevent="submitForm">
    <div class="flex flex-column gap-3">
      <div>
        <BaseInput
          label="Resum"
          id="resum"
          v-model="model.resum"
          :class="{ 'p-invalid': validation.errors.resum }"
        />
        <small v-if="validation.errors.resum" class="p-error">
          {{ validation.errors.resum?.[0] }}
        </small>
      </div>

      <div>
        <label class="block text-900 mb-2">
          Descripció (Markdown)
          <i
            class="pi pi-question-circle md-help-icon"
            role="button"
            tabindex="0"
            aria-label="Ajuda de sintaxi Markdown"
            @click="markdownHelp?.toggle($event)"
            @keydown.enter="markdownHelp?.toggle($event)"
          ></i>
        </label>
        <Textarea
          id="descripcio"
          v-model="model.descripcio"
          rows="6"
          class="w-full"
          :class="{ 'p-invalid': validation.errors.descripcio }"
          autoResize
        />

        <Popover ref="markdownHelp">
          <div class="md-help">
            <p class="md-help__title">Sintaxi Markdown bàsica</p>
            <table class="md-help__table">
              <tbody>
                <tr>
                  <td><code># Títol</code></td>
                  <td>Encapçalament (## , ###...)</td>
                </tr>
                <tr>
                  <td><code>**negreta**</code></td>
                  <td>Text en <strong>negreta</strong></td>
                </tr>
                <tr>
                  <td><code>*cursiva*</code></td>
                  <td>Text en <em>cursiva</em></td>
                </tr>
                <tr>
                  <td><code>~~ratllat~~</code></td>
                  <td>Text ratllat</td>
                </tr>
                <tr>
                  <td><code>- element</code></td>
                  <td>Llista de punts</td>
                </tr>
                <tr>
                  <td><code>1. element</code></td>
                  <td>Llista numerada</td>
                </tr>
                <tr>
                  <td><code>[text](url)</code></td>
                  <td>Enllaç</td>
                </tr>
                <tr>
                  <td><code>`codi`</code></td>
                  <td>Codi en línia</td>
                </tr>
                <tr>
                  <td><code>```codi```</code></td>
                  <td>Bloc de codi</td>
                </tr>
                <tr>
                  <td><code>&gt; cita</code></td>
                  <td>Cita</td>
                </tr>
                <tr>
                  <td><code>- [ ] tasca</code></td>
                  <td>Casella de verificació</td>
                </tr>
              </tbody>
            </table>
          </div>
        </Popover>

        <small v-if="validation.errors.descripcio" class="p-error">
          {{ validation.errors.descripcio?.[0] }}
        </small>

        <div class="mt-3">
          <label class="block text-600 text-sm mb-2">Vista prèvia</label>
          <MarkdownRenderer
            v-if="model.descripcio.trim()"
            :markdown="model.descripcio"
            class="support-preview"
          />
          <p v-else class="text-500 m-0 support-preview-empty">
            No hi ha res a previsualitzar.
          </p>
        </div>
      </div>

      <div class="flex justify-content-end gap-2 mt-2">
        <Button
          label="Cancel·lar"
          severity="secondary"
          type="button"
          @click="emit('close')"
          :disabled="store.isSubmitting"
        />
        <Button
          label="Enviar"
          type="submit"
          :loading="store.isSubmitting"
        />
      </div>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useToast } from "primevue/usetoast";
import type Popover from "primevue/popover";
import * as Yup from "yup";
import BaseInput from "../../../components/BaseInput.vue";
import MarkdownRenderer from "../../../components/help/MarkdownRenderer.vue";
import { FormValidation, FormValidationResult } from "../../../utils/form-validator";
import { useSupportStore } from "../store/support";

const emit = defineEmits<{
  (e: "close"): void;
}>();

const store = useSupportStore();
const toast = useToast();

const markdownHelp = ref<InstanceType<typeof Popover> | null>(null);

const model = reactive({
  resum: "",
  descripcio: "",
});

const schema = Yup.object().shape({
  resum: Yup.string()
    .required("El resum és obligatori")
    .max(255, "El resum no pot superar els 255 caràcters"),
  descripcio: Yup.string().required("La descripció és obligatòria"),
});

const validation = ref<FormValidationResult>({ result: false, errors: {} });

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(model);
};

const submitForm = async () => {
  validate();
  if (!validation.value.result) {
    const errors = Object.values(validation.value.errors)
      .map((msgs) => msgs.join(". "))
      .join("   ");
    toast.add({
      severity: "warn",
      summary: "Formulari invàlid",
      detail: errors,
      life: 5000,
    });
    return;
  }

  const result = await store.submit(model.resum, model.descripcio);

  if (result.ok) {
    toast.add({
      severity: "success",
      summary: "Sol·licitud enviada",
      detail: "La teva petició de suport s'ha registrat correctament.",
      life: 5000,
    });
    emit("close");
  } else {
    toast.add({
      severity: "error",
      summary: "Error en enviar la sol·licitud",
      detail: result.error,
      life: 8000,
    });
  }
};
</script>

<style scoped>
.support-preview,
.support-preview-empty {
  min-height: 80px;
  padding: 0.65rem 0.85rem;
  border: 1px solid var(--p-surface-300);
  border-radius: var(--p-content-border-radius, 6px);
  background: var(--p-surface-50);
}

.md-help-icon {
  margin-left: 0.35rem;
  color: var(--p-primary-color);
  cursor: pointer;
  font-size: 0.95rem;
}

.md-help-icon:hover {
  color: var(--p-primary-600);
}

.md-help {
  max-width: 320px;
}

.md-help__title {
  margin: 0 0 0.5rem;
  font-weight: 600;
}

.md-help__table {
  border-collapse: collapse;
  width: 100%;
  font-size: 0.85rem;
}

.md-help__table td {
  padding: 0.2rem 0.5rem 0.2rem 0;
  vertical-align: top;
}

.md-help__table code {
  background: var(--p-surface-100);
  padding: 0.1rem 0.35rem;
  border-radius: 4px;
  white-space: nowrap;
}
</style>
