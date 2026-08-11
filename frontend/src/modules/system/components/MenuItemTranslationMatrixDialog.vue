<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";
import {
  getMenuItemTranslationMatrix,
  updateMenuItemTranslations,
} from "@/modules/system/services/menuitem.service";
import type {
  MenuItemTranslationMatrixLanguage,
  MenuItemTranslationMatrixRow,
  UpdateMenuItemTranslationRowRequest,
} from "@/modules/system/types/menuitem";

const props = defineProps<{ visible: boolean }>();
const emit = defineEmits<{
  (event: "update:visible", value: boolean): void;
  (event: "saved"): void;
}>();

const { t } = useI18n();
const confirm = useConfirm();
const toast = useToast();
const loading = ref(false);
const saving = ref(false);
const search = ref("");
const languages = ref<MenuItemTranslationMatrixLanguage[]>([]);
const rows = ref<MenuItemTranslationMatrixRow[]>([]);
const originalTitles = ref(new Map<string, string>());
const dirtyCells = ref(new Set<string>());

const cellKey = (menuItemId: string, languageCode: string) =>
  `${menuItemId}:${languageCode.toLowerCase()}`;

const titleFor = (row: MenuItemTranslationMatrixRow, languageCode: string) =>
  row.translations.find(
    (translation) =>
      translation.languageCode.toLowerCase() === languageCode.toLowerCase(),
  )?.title ?? "";

const filteredRows = computed(() => {
  const value = search.value.trim().toLowerCase();
  if (!value) return rows.value;
  return rows.value.filter(
    (row) =>
      row.key.toLowerCase().includes(value) ||
      row.route?.toLowerCase().includes(value) ||
      row.translations.some((translation) =>
        translation.title.toLowerCase().includes(value),
      ),
  );
});

const invalidDirtyCells = computed(
  () =>
    [...dirtyCells.value].filter((key) => {
      const [menuItemId, languageCode] = key.split(":");
      const row = rows.value.find((item) => item.id === menuItemId);
      return !row || !titleFor(row, languageCode).trim();
    }).length,
);

const emptyCells = computed(() =>
  rows.value.reduce(
    (count, row) =>
      count +
      languages.value.filter((language) => !titleFor(row, language.code).trim())
        .length,
    0,
  ),
);

const updateTitle = (
  row: MenuItemTranslationMatrixRow,
  languageCode: string,
  value: string | undefined,
) => {
  const translation = row.translations.find(
    (item) => item.languageCode.toLowerCase() === languageCode.toLowerCase(),
  );
  if (!translation) return;
  translation.title = value ?? "";

  const key = cellKey(row.id, languageCode);
  const next = new Set(dirtyCells.value);
  if (translation.title === originalTitles.value.get(key)) next.delete(key);
  else next.add(key);
  dirtyCells.value = next;
};

const load = async () => {
  loading.value = true;
  search.value = "";
  dirtyCells.value = new Set();
  try {
    const matrix = await getMenuItemTranslationMatrix();
    languages.value = matrix.languages;
    rows.value = matrix.items.map((row) => ({
      ...row,
      translations: row.translations.map((translation) => ({ ...translation })),
    }));
    originalTitles.value = new Map(
      rows.value.flatMap((row) =>
        row.translations.map((translation) => [
          cellKey(row.id, translation.languageCode),
          translation.title,
        ]),
      ),
    );
  } catch {
    toast.add({
      severity: "error",
      summary: t("menuItems.matrix.loadError"),
      life: 4000,
    });
    emit("update:visible", false);
  } finally {
    loading.value = false;
  }
};

const buildRequest = (): UpdateMenuItemTranslationRowRequest[] => {
  const updates = new Map<string, UpdateMenuItemTranslationRowRequest>();
  for (const key of dirtyCells.value) {
    const [menuItemId, languageCode] = key.split(":");
    const row = rows.value.find((item) => item.id === menuItemId);
    if (!row) continue;
    const update = updates.get(menuItemId) ?? {
      menuItemId,
      translations: [],
    };
    update.translations.push({
      languageCode,
      title: titleFor(row, languageCode).trim(),
    });
    updates.set(menuItemId, update);
  }
  return [...updates.values()];
};

const save = async () => {
  if (!dirtyCells.value.size || invalidDirtyCells.value) return;
  saving.value = true;
  try {
    const result = await updateMenuItemTranslations({ items: buildRequest() });
    toast.add({
      severity: "success",
      summary: t("menuItems.matrix.saved", {
        count: result.updatedTranslations,
      }),
      life: 3000,
    });
    dirtyCells.value = new Set();
    emit("saved");
    emit("update:visible", false);
  } catch {
    toast.add({
      severity: "error",
      summary: t("menuItems.matrix.saveError"),
      life: 4000,
    });
  } finally {
    saving.value = false;
  }
};

const close = () => {
  if (!dirtyCells.value.size) {
    emit("update:visible", false);
    return;
  }
  confirm.require({
    message: t("menuItems.matrix.confirmDiscard"),
    header: t("common.confirm"),
    icon: "pi pi-exclamation-triangle",
    accept: () => emit("update:visible", false),
  });
};

watch(
  () => props.visible,
  (visible) => {
    if (visible) void load();
  },
);
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    maximizable
    :closable="false"
    :closeOnEscape="false"
    :header="t('menuItems.matrix.title')"
    :style="{ width: '96vw' }"
    :breakpoints="{ '768px': '100vw' }"
    class="menu-translation-matrix-dialog"
  >
    <div class="flex flex-column gap-3 translation-matrix">
      <div class="flex justify-content-end">
        <IconField iconPosition="left" class="matrix-search">
          <InputIcon><i class="pi pi-search" /></InputIcon>
          <InputText
            v-model="search"
            :placeholder="t('menuItems.matrix.search')"
            size="small"
            class="w-full"
          />
        </IconField>
      </div>

      <Message v-if="invalidDirtyCells" severity="error" :closable="false">
        {{ t("menuItems.matrix.emptyChanged") }}
      </Message>

      <DataTable
        :value="filteredRows"
        :loading="loading"
        dataKey="id"
        scrollable
        scrollHeight="62vh"
        stripedRows
        class="translation-matrix-table"
      >
        <Column frozen :header="t('menuItems.matrix.menuItem')" style="min-width: 20rem">
          <template #body="slotProps">
            <div :style="{ paddingLeft: `${slotProps.data.depth * 1.1}rem` }">
              <div class="font-medium">{{ slotProps.data.key }}</div>
              <small class="text-color-secondary">{{ slotProps.data.route || "-" }}</small>
            </div>
          </template>
        </Column>
        <Column
          v-for="language in languages"
          :key="language.code"
          :header="language.name"
          style="min-width: 17rem"
        >
          <template #body="slotProps">
            <InputText
              :model-value="titleFor(slotProps.data, language.code)"
              :invalid="
                dirtyCells.has(cellKey(slotProps.data.id, language.code)) &&
                !titleFor(slotProps.data, language.code).trim()
              "
              class="w-full"
              maxlength="250"
              size="small"
              @update:model-value="updateTitle(slotProps.data, language.code, $event)"
            />
          </template>
        </Column>
      </DataTable>
    </div>

    <template #footer>
      <div class="flex align-items-center justify-content-between w-full gap-3">
        <div class="flex flex-wrap gap-3 text-color-secondary matrix-status">
          <span>{{ t("menuItems.matrix.changed", { count: dirtyCells.size }) }}</span>
          <span :class="{ 'text-orange-500': emptyCells > 0 }">
            {{ t("menuItems.matrix.empty", { count: emptyCells }) }}
          </span>
        </div>
        <div class="flex gap-2">
          <Button
            :label="t('common.cancel')"
            icon="pi pi-times"
            severity="secondary"
            outlined
            size="small"
            :disabled="saving"
            @click="close"
          />
          <Button
            :label="t('common.save')"
            icon="pi pi-save"
            size="small"
            :loading="saving"
            :disabled="!dirtyCells.size || invalidDirtyCells > 0"
            @click="save"
          />
        </div>
      </div>
    </template>
  </Dialog>
</template>

<style scoped>
.translation-matrix {
  min-height: 68vh;
  font-size: 0.8125rem;
}

.matrix-search {
  width: min(28rem, 100%);
}

.matrix-status {
  font-size: 0.75rem;
}

:deep(.translation-matrix-table) {
  font-size: 0.8125rem;
}

:deep(.translation-matrix-table .p-datatable-frozen-column) {
  background: var(--p-content-background);
}

@media (max-width: 768px) {
  .translation-matrix {
    min-height: 76vh;
  }

  :deep(.menu-translation-matrix-dialog) {
    width: 100vw !important;
    height: 100vh !important;
    max-height: 100vh !important;
    margin: 0;
    border-radius: 0;
  }

  :deep(.menu-translation-matrix-dialog .p-dialog-content) {
    flex: 1 1 auto;
  }
}
</style>
