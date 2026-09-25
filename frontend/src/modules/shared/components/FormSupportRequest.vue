<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { stringValue } from "@/components/forms/value-utils";
import MarkdownRenderer from "@/components/help/MarkdownRenderer.vue";
import type Popover from "primevue/popover";
import { computed, nextTick, ref, type ComponentPublicInstance } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";

defineProps<{
  loading?: boolean;
}>();

const emit = defineEmits<{
  (event: "submit", request: { resum: string; descripcio: string }): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const markdownHelp = ref<InstanceType<typeof Popover> | null>(null);
const descriptionInput = ref<ComponentPublicInstance | null>(null);

// The validation messages are the legacy ones; they have no i18n keys yet.
const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "resum",
        label: t("shared.supportRequest.form.summary"),
        type: FormFieldType.Text,
        defaultValue: "",
        validation: Yup.string()
          .required(t("shared.supportRequest.validation.summaryRequired"))
          .max(255, t("shared.supportRequest.validation.summaryMax")),
      },
    ],
  },
  {
    fields: [
      {
        // The slot renders its own label, with the Markdown help next to it.
        name: "descripcio",
        label: "",
        type: FormFieldType.Custom,
        defaultValue: "",
        validation: Yup.string().required(
          t("shared.supportRequest.validation.descriptionRequired"),
        ),
      },
    ],
  },
]);

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

interface ToolbarButton {
  action: MarkdownAction;
  label: string;
  text?: string;
  icon?: string;
  modifier?: string;
  separatorBefore?: boolean;
}

const toolbarButtons = computed<ToolbarButton[]>(() => [
  {
    action: "bold",
    label: t("shared.supportRequest.form.bold"),
    text: "B",
    modifier: "md-tool--bold",
  },
  {
    action: "italic",
    label: t("shared.supportRequest.form.italic"),
    text: "I",
    modifier: "md-tool--italic",
  },
  {
    action: "strike",
    label: t("shared.supportRequest.form.strikethrough"),
    text: "S",
    modifier: "md-tool--strike",
  },
  {
    action: "heading",
    label: t("shared.supportRequest.form.heading"),
    text: "H",
    separatorBefore: true,
  },
  {
    action: "quote",
    label: t("shared.supportRequest.form.quote"),
    text: "❞",
  },
  {
    action: "ulist",
    label: t("shared.supportRequest.form.bulletList"),
    icon: "pi pi-list",
    separatorBefore: true,
  },
  {
    action: "olist",
    label: t("shared.supportRequest.form.numberedList"),
    text: "1.",
  },
  {
    action: "link",
    label: t("shared.supportRequest.form.link"),
    icon: "pi pi-link",
    separatorBefore: true,
  },
  {
    action: "code",
    label: t("shared.supportRequest.form.inlineCode"),
    icon: "pi pi-code",
  },
  {
    action: "codeblock",
    label: t("shared.supportRequest.form.codeBlock"),
    text: "```",
    modifier: "md-tool--codeblock",
  },
]);

// Rewrites the field value around the textarea selection; the textarea is
// only read for the selection and refocused afterwards.
const applyMarkdown = (
  action: MarkdownAction,
  current: unknown,
  setValue: (value: unknown) => void,
): void => {
  const textarea = descriptionInput.value?.$el as
    | HTMLTextAreaElement
    | undefined;
  if (!textarea) return;

  const value = stringValue(current, "");
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
      replacement = "```\n" + selected + "\n```";
      cursorStart = start + 4;
      cursorEnd = cursorStart + selected.length;
      break;
  }

  setValue(value.slice(0, start) + replacement + value.slice(end));

  void nextTick(() => {
    textarea.focus();
    textarea.setSelectionRange(cursorStart, cursorEnd);
  });
};

const submit = (values: FormValues): void => {
  emit("submit", {
    resum: stringValue(values.resum, ""),
    descripcio: stringValue(values.descripcio, ""),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :loading="loading"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-descripcio="{ value, setValue, disabled, inputId }">
      <div class="md-label">
        <label :for="inputId">
          {{ t("shared.supportRequest.form.description") }}
        </label>
        <i
          class="pi pi-question-circle md-help-icon"
          role="button"
          tabindex="0"
          :aria-label="t('shared.supportRequest.form.markdownHelpAria')"
          @click="markdownHelp?.toggle($event)"
          @keydown.enter="markdownHelp?.toggle($event)"
        ></i>
      </div>

      <div
        class="md-toolbar"
        role="toolbar"
        :aria-label="t('shared.supportRequest.form.toolbarAria')"
      >
        <template v-for="button in toolbarButtons" :key="button.action">
          <span
            v-if="button.separatorBefore"
            class="md-toolbar__sep"
            aria-hidden="true"
          ></span>
          <Button
            type="button"
            text
            severity="secondary"
            size="small"
            :class="['md-tool', button.modifier]"
            :icon="button.icon"
            :disabled="disabled"
            :aria-label="button.label"
            v-tooltip.bottom="button.label"
            @click="applyMarkdown(button.action, value, setValue)"
          >
            <template v-if="button.text" #default>{{ button.text }}</template>
          </Button>
        </template>
      </div>

      <Textarea
        ref="descriptionInput"
        :id="inputId"
        :model-value="stringValue(value, '')"
        rows="6"
        class="w-full"
        :disabled="disabled"
        autoResize
        @update:model-value="setValue"
      />

      <Popover ref="markdownHelp">
        <div class="md-help">
          <p class="md-help__title">
            {{ t("shared.supportRequest.markdownHelp.title") }}
          </p>
          <table class="md-help__table">
            <tbody>
              <tr>
                <td><code># Títol</code></td>
                <td>{{ t("shared.supportRequest.markdownHelp.heading") }}</td>
              </tr>
              <tr>
                <td><code>**negreta**</code></td>
                <td>
                  {{ t("shared.supportRequest.markdownHelp.boldExample") }}
                  <strong>{{
                    t("shared.supportRequest.markdownHelp.boldWord")
                  }}</strong>
                </td>
              </tr>
              <tr>
                <td><code>*cursiva*</code></td>
                <td>
                  {{ t("shared.supportRequest.markdownHelp.italicExample") }}
                  <em>{{ t("shared.supportRequest.markdownHelp.italicWord") }}</em>
                </td>
              </tr>
              <tr>
                <td><code>~~ratllat~~</code></td>
                <td>
                  {{ t("shared.supportRequest.markdownHelp.strikeExample") }}
                </td>
              </tr>
              <tr>
                <td><code>- element</code></td>
                <td>{{ t("shared.supportRequest.markdownHelp.bulletList") }}</td>
              </tr>
              <tr>
                <td><code>1. element</code></td>
                <td>
                  {{ t("shared.supportRequest.markdownHelp.numberedList") }}
                </td>
              </tr>
              <tr>
                <td><code>[text](url)</code></td>
                <td>{{ t("shared.supportRequest.markdownHelp.link") }}</td>
              </tr>
              <tr>
                <td><code>`codi`</code></td>
                <td>{{ t("shared.supportRequest.markdownHelp.inlineCode") }}</td>
              </tr>
              <tr>
                <td><code>```codi```</code></td>
                <td>{{ t("shared.supportRequest.markdownHelp.codeBlock") }}</td>
              </tr>
              <tr>
                <td><code>&gt; cita</code></td>
                <td>{{ t("shared.supportRequest.markdownHelp.quote") }}</td>
              </tr>
              <tr>
                <td><code>- [ ] tasca</code></td>
                <td>{{ t("shared.supportRequest.markdownHelp.task") }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </Popover>

      <div class="mt-3">
        <span class="block text-600 text-sm mb-2">
          {{ t("shared.supportRequest.form.preview") }}
        </span>
        <MarkdownRenderer
          v-if="stringValue(value, '').trim()"
          :markdown="stringValue(value, '')"
          class="support-preview"
        />
        <p v-else class="text-500 m-0 support-preview-empty">
          {{ t("shared.supportRequest.form.noPreview") }}
        </p>
      </div>
    </template>
    <template #actions="{ loading: busy }">
      <Button
        type="button"
        severity="secondary"
        icon="pi pi-times"
        :label="t('shared.supportRequest.form.cancel')"
        :disabled="busy"
        @click="emit('cancel')"
      />
      <Button
        type="submit"
        icon="pi pi-send"
        :label="t('shared.supportRequest.form.send')"
        :loading="busy"
      />
    </template>
  </Form>
</template>

<style scoped>
.support-preview,
.support-preview-empty {
  min-height: 80px;
  padding: 0.65rem 0.85rem;
  border: 1px solid var(--p-surface-300);
  border-radius: var(--p-content-border-radius, 6px);
  background: var(--p-surface-50);
}

.md-label {
  display: flex;
  align-items: center;
  margin-bottom: 0.5rem;
  color: var(--p-text-color);
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
