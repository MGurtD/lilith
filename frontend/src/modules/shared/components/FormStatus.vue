<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { booleanValue, stringValue } from "@/components/forms/value-utils";
import { computed, ref, useId, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { FormActionMode } from "../../../types/component";
import SharedServices from "../services";
import type { LifecycleTag, Status } from "../types";

interface StatusTagChanges {
  assign: string[];
  remove: string[];
}

const props = defineProps<{
  formAction: FormActionMode;
  status: Status;
}>();

const emit = defineEmits<{
  (event: "submit", status: Status, tagChanges: StatusTagChanges): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const form = ref<{
  setFieldValue: (name: string, value: unknown) => void;
} | null>(null);
const tagsInputId = `status-tags-${useId()}`;

// Tags are loaded after the form opens. The assigned tags seed the tagIds
// field and are the baseline for the assign/remove diff emitted on submit.
const availableTags = ref<LifecycleTag[]>([]);
const assignedTags = ref<LifecycleTag[] | undefined>(undefined);
const initialTagIds = ref<string[]>([]);
let tagRequestSequence = 0;

const loadTags = async (status: Status): Promise<void> => {
  const requestSequence = ++tagRequestSequence;
  availableTags.value = [];
  assignedTags.value = status.lifecycleTags;
  initialTagIds.value = [];

  const statusTags = await SharedServices.Lifecycle.getTagsByStatus(status.id);
  const lifecycleTags = status.lifecycleId
    ? await SharedServices.Lifecycle.getTagsByLifecycle(status.lifecycleId)
    : undefined;
  if (requestSequence !== tagRequestSequence) return;

  if (statusTags) assignedTags.value = statusTags;
  initialTagIds.value = (assignedTags.value ?? []).map((tag) => tag.id);
  form.value?.setFieldValue("tagIds", [...initialTagIds.value]);
  availableTags.value = lifecycleTags ?? [];
};

watch(
  () => props.status,
  (status) => {
    void loadTags(status);
  },
  { immediate: true },
);

interface ColorOption {
  id: string;
  value: string;
}

// Stored values stay PrimeVue severities. "help" is no longer offered: Aura has
// no such severity, so it showed the tenant colour; old values render neutral.
const colors = computed<ColorOption[]>(() => [
  { id: "", value: t("shared.statuses.form.colors.none") },
  { id: "secondary", value: t("shared.statuses.form.colors.secondary") },
  { id: "info", value: t("shared.statuses.form.colors.info") },
  { id: "warn", value: t("shared.statuses.form.colors.warn") },
  { id: "success", value: t("shared.statuses.form.colors.success") },
  { id: "danger", value: t("shared.statuses.form.colors.danger") },
  { id: "contrast", value: t("shared.statuses.form.colors.contrast") },
]);

const colorLabel = (id: unknown): string =>
  colors.value.find((color) => color.id === (id ?? ""))?.value ??
  t("shared.statuses.form.colors.none");

const colorSeverity = (id: unknown): string =>
  typeof id === "string" && id !== "" ? id : "secondary";

const tagIdsValue = (value: unknown): string[] =>
  Array.isArray(value)
    ? value.filter((id): id is string => typeof id === "string")
    : [];

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "name",
        label: t("shared.statuses.form.name"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("shared.lifecycle.validation.nameRequired"),
        ),
      },
      {
        name: "color",
        label: t("shared.statuses.form.color"),
        type: FormFieldType.Custom,
        defaultValue: "",
      },
      {
        name: "disabled",
        label: t("shared.statuses.form.disabled"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
      },
    ],
  },
  {
    section: "tags",
    fields: [
      {
        name: "tagIds",
        label: t("shared.statuses.form.tags"),
        type: FormFieldType.Custom,
        defaultValue: [],
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  const selectedTagIds = tagIdsValue(values.tagIds);

  emit(
    "submit",
    {
      ...props.status,
      name: stringValue(values.name, ""),
      color: stringValue(values.color, props.status.color),
      disabled: booleanValue(values.disabled, false),
      lifecycleTags: assignedTags.value,
    },
    {
      assign: selectedTagIds.filter((id) => !initialTagIds.value.includes(id)),
      remove: initialTagIds.value.filter((id) => !selectedTagIds.includes(id)),
    },
  );
};
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="status"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-color="{ value, setValue, disabled, inputId }">
      <!-- Colours by meaning; each option previews the tag lists will show. -->
      <Select
        :input-id="inputId"
        :model-value="typeof value === 'string' ? value : ''"
        :options="colors"
        option-value="id"
        option-label="value"
        class="w-full"
        :disabled="disabled"
        @update:model-value="setValue"
      >
        <template #value="{ value: selected }">
          <Tag
            :value="colorLabel(selected)"
            :severity="colorSeverity(selected)"
          />
        </template>
        <template #option="{ option }">
          <Tag :value="option.value" :severity="colorSeverity(option.id)" />
        </template>
      </Select>
    </template>

    <template #section-tags="{ values, setFieldValue, disabled }">
      <div v-if="availableTags.length > 0">
        <label class="block text-900 mb-2" :for="tagsInputId">
          {{ t("shared.statuses.form.tags") }}
        </label>
        <MultiSelect
          :input-id="tagsInputId"
          :model-value="tagIdsValue(values.tagIds)"
          :options="availableTags"
          option-value="id"
          option-label="name"
          :placeholder="t('shared.statuses.form.tagsPlaceholder')"
          class="w-full"
          display="chip"
          :disabled="disabled"
          @update:model-value="setFieldValue('tagIds', $event)"
        >
          <template #option="{ option }">
            <div class="flex align-items-center">
              <i v-if="option.icon" :class="option.icon" class="mr-2"></i>
              <Tag v-if="option.color" :severity="option.color" class="mr-2">
                {{ option.name }}
              </Tag>
              <span v-else>{{ option.name }}</span>
            </div>
          </template>
        </MultiSelect>

        <div v-if="assignedTags && assignedTags.length > 0" class="mt-3">
          <span class="block text-900 mb-2">
            {{ t("shared.statuses.form.assignedTags") }}
          </span>
          <div class="flex gap-2 flex-wrap">
            <Tag
              v-for="tag in assignedTags"
              :key="tag.id"
              :severity="tag.color"
            >
              <i v-if="tag.icon" :class="tag.icon" class="mr-1"></i>
              {{ tag.name }}
            </Tag>
          </div>
        </div>
      </div>
    </template>
  </Form>
</template>
