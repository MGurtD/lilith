<script setup lang="ts">
import { ref, watch, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import * as yup from "yup";
import { FormValidation } from "@/utils/form-validator";
import IconPicker from "@/components/IconPicker.vue";
import type { MenuItemFlat, MenuItemNode } from "@/modules/system/types/menuitem";
import { getMenuItemsHierarchy } from "@/modules/system/services/menuitem.service";
import { BaseInputType } from "@/types/component";
import LanguageService from "@/services/language.service";
import type { Language } from "@/types";

const props = defineProps<{
  modelValue: Partial<MenuItemFlat>;
  submitting?: boolean;
}>();
const emit = defineEmits<{
  (e: "update:modelValue", value: Partial<MenuItemFlat>): void;
  (e: "submit"): void;
}>();

const { t } = useI18n();

const schema = yup.object({
  key: yup.string().required(() => t("menuItems.form.validation.keyRequired")),
  sortOrder: yup
    .number()
    .required(() => t("menuItems.form.validation.orderRequired"))
    .min(0),
});
const validator = new FormValidation(schema as any);

const form = ref<Partial<MenuItemFlat>>({ ...props.modelValue });
const errors = ref<Record<string, string[]>>({});
const hierarchy = ref<MenuItemNode[]>([]);
const languages = ref<Language[]>([]);
const languageService = new LanguageService();
const withActiveLanguages = (value: Partial<MenuItemFlat>) => ({
  ...value,
  translations: languages.value.map((language) => ({
    languageCode: language.code.toLowerCase(),
    title:
      value.translations?.find(
        (translation) =>
          translation.languageCode.toLowerCase() === language.code.toLowerCase(),
      )?.title ?? "",
  })),
});
const parentFlat = computed<MenuItemFlat[]>(() => {
  const list: MenuItemFlat[] = [];
  const walk = (nodes: MenuItemNode[], depth = 0) => {
    nodes.forEach((n) => {
      list.push({ ...n, title: `${" > ".repeat(depth)}${n.title}` });
      if (n.children?.length) walk(n.children, depth + 1);
    });
  };
  walk(hierarchy.value);
  return list.filter((i) => !form.value.id || i.id !== form.value.id);
});

// REMOVED: Bidirectional watcher that was causing infinite loops
// The parent passes initial data via props, child maintains its own state
// Child only syncs back to parent on submit, not on every change
watch(
  () => props.modelValue,
  (v) => {
    form.value = languages.value.length ? withActiveLanguages(v) : { ...v };
  },
  { immediate: true },
);

const validate = () => {
  const r = validator.validate(form.value);
  errors.value = r.errors;
  let valid = r.result;
  if (!languages.value.length) {
    errors.value.translations = [
      t("menuItems.form.validation.languagesRequired"),
    ];
    valid = false;
  }
  for (const translation of form.value.translations ?? []) {
    if (!translation.title.trim()) {
      errors.value[`translation-${translation.languageCode}`] = [
        t("menuItems.form.validation.titleRequired"),
      ];
      valid = false;
    }
  }
  return valid && form.value.translations?.length === languages.value.length;
};

const submit = () => {
  if (!validate()) return;
  // Sync final state to parent before submit
  emit("update:modelValue", form.value);
  emit("submit");
};

const loadHierarchy = async () => {
  hierarchy.value = await getMenuItemsHierarchy();
};

const loadLanguages = async () => {
  languages.value = ((await languageService.GetAll()) ?? []).sort(
    (a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0),
  );
  form.value = withActiveLanguages(form.value);
};

onMounted(async () => {
  await Promise.all([loadHierarchy(), loadLanguages()]);
});
</script>
<template>
  <div class="form-menu-item">
    <div class="formgrid grid">
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("menuItems.form.key") }}</label>
        <BaseInput v-model="form.key" />
        <small class="p-error" v-if="errors.key">{{ errors.key[0] }}</small>
      </div>
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("menuItems.form.route") }}</label>
        <BaseInput v-model="form.route" placeholder="/path" />
      </div>
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("menuItems.form.sortOrder") }}</label>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          v-model="form.sortOrder as any"
          :min="0"
        />
        <small class="p-error" v-if="errors.sortOrder">{{
          errors.sortOrder[0]
        }}</small>
      </div>
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("menuItems.form.parent") }}</label>
        <Select
          v-model="form.parentId"
          :options="parentFlat"
          optionLabel="title"
          optionValue="id"
          :placeholder="t('menuItems.form.parent')"
          showClear
          class="w-full"
        />
      </div>
      <div class="field col-12 md:col-4">
        <label class="block mb-2">{{ t("menuItems.form.icon") }}</label>
        <IconPicker v-model="form.icon" />
      </div>
      <div
        v-for="translation in form.translations"
        :key="translation.languageCode"
        class="field col-12 md:col-4"
      >
        <label class="block mb-2">
          {{
            t("menuItems.form.titleForLanguage", {
              language:
                languages.find(
                  (language) => language.code === translation.languageCode,
                )?.name ?? translation.languageCode,
            })
          }}
        </label>
        <BaseInput v-model="translation.title" />
        <small
          class="p-error"
          v-if="errors[`translation-${translation.languageCode}`]"
        >
          {{ errors[`translation-${translation.languageCode}`][0] }}
        </small>
      </div>
      <small class="p-error col-12" v-if="errors.translations">
        {{ errors.translations[0] }}
      </small>
    </div>
    <div class="flex gap-2 mt-3">
      <Button
        :label="t('menuItems.form.save')"
        icon="pi pi-save"
        :loading="submitting"
        @click="submit"
      />
    </div>
  </div>
</template>
