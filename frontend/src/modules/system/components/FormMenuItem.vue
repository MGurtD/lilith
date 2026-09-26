<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormFieldConfig,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  finiteNumberValue,
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import IconPicker from "@/components/IconPicker.vue";
import Message from "primevue/message";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type {
  MenuItemFlat,
  MenuItemNode,
  MenuItemTranslation,
} from "@/modules/system/types/menuitem";
import { getMenuItemsHierarchy } from "@/modules/system/services/menuitem.service";
import LanguageService from "@/services/language.service";
import type { Language } from "@/types";

const props = withDefaults(
  defineProps<{
    menuItem: MenuItemFlat;
    submitting?: boolean;
  }>(),
  { submitting: false },
);

const emit = defineEmits<{
  (event: "submit", menuItem: MenuItemFlat): void;
}>();

const { t } = useI18n();

const languageService = new LanguageService();
const hierarchy = ref<MenuItemNode[]>([]);
const languages = ref<Language[]>([]);
// The title fields are generated from the active languages, so the form is
// rendered only once that list is loaded and stays stable while it is open.
const languagesLoaded = ref(false);

interface TitleField {
  name: string;
  languageCode: string;
  languageName: string;
}

const titleFields = computed<TitleField[]>(() =>
  languages.value.map((language) => {
    const languageCode = language.code.toLowerCase();
    return {
      name: `title_${languageCode.replace(/[^a-z0-9_-]/g, "_")}`,
      languageCode,
      languageName: language.name || language.code,
    };
  }),
);

const translationTitle = (languageCode: string): string =>
  props.menuItem.translations?.find(
    (translation) => translation.languageCode.toLowerCase() === languageCode,
  )?.title ?? "";

const initialValues = computed<FormValues>(() => ({
  ...props.menuItem,
  ...Object.fromEntries(
    titleFields.value.map((field) => [
      field.name,
      translationTitle(field.languageCode),
    ]),
  ),
}));

const parentOptions = computed<MenuItemFlat[]>(() => {
  const list: MenuItemFlat[] = [];
  const walk = (nodes: MenuItemNode[], depth = 0) => {
    nodes.forEach((node) => {
      list.push({ ...node, title: `${" > ".repeat(depth)}${node.title}` });
      if (node.children?.length) walk(node.children, depth + 1);
    });
  };
  walk(hierarchy.value);
  return list.filter(
    (item) => !props.menuItem.id || item.id !== props.menuItem.id,
  );
});

// One title field per active language, named deterministically from its code
// and mapped back to the translation collection at submit.
const titleRows = computed<FormRowConfig[]>(() =>
  titleFields.value.length
    ? [
        {
          columns: { mobile: 1, desktop: 3 },
          fields: titleFields.value.map(
            (field): FormFieldConfig => ({
              name: field.name,
              label: t("menuItems.form.titleForLanguage", {
                language: field.languageName,
              }),
              type: FormFieldType.Text,
              defaultValue: "",
              validation: Yup.string().test(
                "title-not-blank",
                t("menuItems.form.validation.titleRequired"),
                (value) =>
                  typeof value === "string" && value.trim().length > 0,
              ),
            }),
          ),
        },
      ]
    : [],
);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "key",
        label: t("menuItems.form.key"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("menuItems.form.validation.keyRequired"),
        ),
      },
      {
        name: "route",
        label: t("menuItems.form.route"),
        type: FormFieldType.Text,
        props: { placeholder: "/path" },
      },
      {
        name: "sortOrder",
        label: t("menuItems.form.sortOrder"),
        type: FormFieldType.Number,
        defaultValue: 0,
        props: { locale: "en-US", min: 0 },
        validation: Yup.number()
          .typeError(t("menuItems.form.validation.orderRequired"))
          .required(t("menuItems.form.validation.orderRequired"))
          .min(0),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "parentId",
        label: t("menuItems.form.parent"),
        type: FormFieldType.Select,
        props: {
          options: parentOptions.value,
          optionLabel: "title",
          optionValue: "id",
          placeholder: t("menuItems.form.parent"),
          showClear: true,
        },
      },
      {
        name: "icon",
        label: t("menuItems.form.icon"),
        type: FormFieldType.Custom,
      },
    ],
  },
  ...titleRows.value,
]);

const submit = (values: FormValues): void => {
  if (!titleFields.value.length) return;

  const translations: MenuItemTranslation[] = titleFields.value.map(
    (field) => ({
      languageCode: field.languageCode,
      title: stringValue(values[field.name], ""),
    }),
  );

  emit("submit", {
    ...props.menuItem,
    key: stringValue(values.key, props.menuItem.key),
    route: nullableStringValue(values.route, props.menuItem.route ?? null),
    sortOrder: finiteNumberValue(values.sortOrder, props.menuItem.sortOrder),
    parentId: nullableStringValue(
      values.parentId,
      props.menuItem.parentId ?? null,
    ),
    icon: nullableStringValue(values.icon, props.menuItem.icon ?? null),
    translations,
  });
};

const loadHierarchy = async () => {
  hierarchy.value = await getMenuItemsHierarchy();
};

const loadLanguages = async () => {
  languages.value = ((await languageService.GetAll()) ?? []).sort(
    (a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0),
  );
  languagesLoaded.value = true;
};

onMounted(async () => {
  await Promise.all([loadHierarchy(), loadLanguages()]);
});
</script>

<template>
  <div class="form-menu-item">
    <Message
      v-if="languagesLoaded && !titleFields.length"
      severity="error"
      class="mb-3"
    >
      {{ t("menuItems.form.validation.languagesRequired") }}
    </Message>
    <Form
      v-if="languagesLoaded"
      page-actions
      :rows="rows"
      :initial-values="initialValues"
      :loading="submitting"
      :disabled="!titleFields.length"
      @submit="submit"
    >
      <template #field-icon="{ value, setValue, disabled }">
        <IconPicker
          :model-value="typeof value === 'string' ? value : null"
          :class="{ 'pointer-events-none opacity-60': disabled }"
          @update:model-value="setValue"
        />
      </template>
    </Form>
  </div>
</template>
