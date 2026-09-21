<template>
  <form @submit.prevent="submitForm">
    <div class="flex flex-column gap-3">
      <div>
        <BaseInput
          :label="$t('shared.supportRequest.form.summary')"
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
          {{ $t('shared.supportRequest.form.description') }}
          <i
            class="pi pi-question-circle md-help-icon"
            role="button"
            tabindex="0"
            :aria-label="$t('shared.supportRequest.form.markdownHelpAria')"
            @click="markdownHelp?.toggle($event)"
            @keydown.enter="markdownHelp?.toggle($event)"
          ></i>
        </label>

        <div
          class="md-toolbar"
          role="toolbar"
          :aria-label="$t('shared.supportRequest.form.toolbarAria')"
        >
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool md-tool--bold"
            :aria-label="$t('shared.supportRequest.form.bold')"
            v-tooltip.bottom="$t('shared.supportRequest.form.bold')"
            @click="applyMarkdown('bold')"
            >B</Button
          >
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool md-tool--italic"
            :aria-label="$t('shared.supportRequest.form.italic')"
            v-tooltip.bottom="$t('shared.supportRequest.form.italic')"
            @click="applyMarkdown('italic')"
            >I</Button
          >
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool md-tool--strike"
            :aria-label="$t('shared.supportRequest.form.strikethrough')"
            v-tooltip.bottom="$t('shared.supportRequest.form.strikethrough')"
            @click="applyMarkdown('strike')"
            >S</Button
          >
          <span class="md-toolbar__sep" aria-hidden="true"></span>
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool"
            :aria-label="$t('shared.supportRequest.form.heading')"
            v-tooltip.bottom="$t('shared.supportRequest.form.heading')"
            @click="applyMarkdown('heading')"
            >H</Button
          >
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool"
            :aria-label="$t('shared.supportRequest.form.quote')"
            v-tooltip.bottom="$t('shared.supportRequest.form.quote')"
            @click="applyMarkdown('quote')"
            >&#10078;</Button
          >
          <span class="md-toolbar__sep" aria-hidden="true"></span>
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool"
            icon="pi pi-list"
            :aria-label="$t('shared.supportRequest.form.bulletList')"
            v-tooltip.bottom="$t('shared.supportRequest.form.bulletList')"
            @click="applyMarkdown('ulist')"
          />
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool"
            :aria-label="$t('shared.supportRequest.form.numberedList')"
            v-tooltip.bottom="$t('shared.supportRequest.form.numberedList')"
            @click="applyMarkdown('olist')"
            >1.</Button
          >
          <span class="md-toolbar__sep" aria-hidden="true"></span>
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool"
            icon="pi pi-link"
            :aria-label="$t('shared.supportRequest.form.link')"
            v-tooltip.bottom="$t('shared.supportRequest.form.link')"
            @click="applyMarkdown('link')"
          />
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool"
            icon="pi pi-code"
            :aria-label="$t('shared.supportRequest.form.inlineCode')"
            v-tooltip.bottom="$t('shared.supportRequest.form.inlineCode')"
            @click="applyMarkdown('code')"
          />
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            class="md-tool md-tool--codeblock"
            :aria-label="$t('shared.supportRequest.form.codeBlock')"
            v-tooltip.bottom="$t('shared.supportRequest.form.codeBlock')"
            @click="applyMarkdown('codeblock')"
            >&#96;&#96;&#96;</Button
          >
        </div>

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
            <p class="md-help__title">{{ $t('shared.supportRequest.markdownHelp.title') }}</p>
            <table class="md-help__table">
              <tbody>
                <tr>
                  <td><code># Títol</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.heading') }}</td>
                </tr>
                <tr>
                  <td><code>**negreta**</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.boldExample') }} <strong>{{ $t('shared.supportRequest.markdownHelp.boldWord') }}</strong></td>
                </tr>
                <tr>
                  <td><code>*cursiva*</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.italicExample') }} <em>{{ $t('shared.supportRequest.markdownHelp.italicWord') }}</em></td>
                </tr>
                <tr>
                  <td><code>~~ratllat~~</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.strikeExample') }}</td>
                </tr>
                <tr>
                  <td><code>- element</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.bulletList') }}</td>
                </tr>
                <tr>
                  <td><code>1. element</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.numberedList') }}</td>
                </tr>
                <tr>
                  <td><code>[text](url)</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.link') }}</td>
                </tr>
                <tr>
                  <td><code>`codi`</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.inlineCode') }}</td>
                </tr>
                <tr>
                  <td><code>```codi```</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.codeBlock') }}</td>
                </tr>
                <tr>
                  <td><code>&gt; cita</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.quote') }}</td>
                </tr>
                <tr>
                  <td><code>- [ ] tasca</code></td>
                  <td>{{ $t('shared.supportRequest.markdownHelp.task') }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </Popover>

        <small v-if="validation.errors.descripcio" class="p-error">
          {{ validation.errors.descripcio?.[0] }}
        </small>

        <div class="mt-3">
          <label class="block text-600 text-sm mb-2">{{ $t('shared.supportRequest.form.preview') }}</label>
          <MarkdownRenderer
            v-if="model.descripcio.trim()"
            :markdown="model.descripcio"
            class="support-preview"
          />
          <p v-else class="text-500 m-0 support-preview-empty">
            {{ $t('shared.supportRequest.form.noPreview') }}
          </p>
        </div>
      </div>

      <div class="flex justify-content-end gap-2 mt-2">
        <Button
          :label="$t('shared.supportRequest.form.cancel')"
          severity="secondary"
          type="button"
          @click="emit('close')"
          :disabled="store.isSubmitting"
        />
        <Button
          :label="$t('shared.supportRequest.form.send')"
          type="submit"
          :loading="store.isSubmitting"
        />
      </div>
    </div>
  </form>
</template>

<script setup lang="ts">
import { nextTick, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";
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

const { t } = useI18n();
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

type MarkdownAction =
  | "bold"
  | "italic"
  | "strike"
  | "heading"
  | "quote"
  | "ulist"
  | "olist"
  | "link"
  | "code"
  | "codeblock";

const applyMarkdown = (action: MarkdownAction) => {
  const textarea = document.getElementById(
    "descripcio",
  ) as HTMLTextAreaElement | null;
  if (!textarea) return;

  const value = model.descripcio;
  const start = textarea.selectionStart ?? value.length;
  const end = textarea.selectionEnd ?? value.length;
  const selected = value.slice(start, end);

  let replacement = selected;
  let cursorStart = start;
  let cursorEnd = end;

  const wrap = (marker: string) => {
    replacement = `${marker}${selected}${marker}`;
    cursorStart = start + marker.length;
    cursorEnd = cursorStart + selected.length;
  };

  const prefixLines = (prefix: string) => {
    replacement = selected
      .split("\n")
      .map((line) => `${prefix}${line}`)
      .join("\n");
    cursorStart = start;
    cursorEnd = start + replacement.length;
  };

  switch (action) {
    case "bold":
      wrap("**");
      break;
    case "italic":
      wrap("*");
      break;
    case "strike":
      wrap("~~");
      break;
    case "code":
      wrap("`");
      break;
    case "heading":
      prefixLines("# ");
      break;
    case "quote":
      prefixLines("> ");
      break;
    case "ulist":
      prefixLines("- ");
      break;
    case "olist":
      replacement = selected
        .split("\n")
        .map((line, index) => `${index + 1}. ${line}`)
        .join("\n");
      cursorStart = start;
      cursorEnd = start + replacement.length;
      break;
    case "link": {
      const text = selected || "text";
      replacement = `[${text}](url)`;
      cursorStart = start + 1;
      cursorEnd = start + 1 + text.length;
      break;
    }
    case "codeblock":
      replacement = `\`\`\`\n${selected}\n\`\`\``;
      cursorStart = start + 4;
      cursorEnd = cursorStart + selected.length;
      break;
  }

  model.descripcio = value.slice(0, start) + replacement + value.slice(end);

  nextTick(() => {
    textarea.focus();
    textarea.setSelectionRange(cursorStart, cursorEnd);
  });
};

const submitForm = async () => {
  validate();
  if (!validation.value.result) {
    const errors = Object.values(validation.value.errors)
      .map((msgs) => msgs.join(". "))
      .join("   ");
    toast.add({
      severity: "warn",
      summary: t("shared.supportRequest.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
    return;
  }

  const result = await store.submit(model.resum, model.descripcio);

  if (result.ok) {
    toast.add({
      severity: "success",
      summary: t("shared.supportRequest.messages.sent"),
      detail: t("shared.supportRequest.messages.sentDetail"),
      life: 5000,
    });
    emit("close");
  } else {
    toast.add({
      severity: "error",
      summary: t("shared.supportRequest.messages.error"),
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
