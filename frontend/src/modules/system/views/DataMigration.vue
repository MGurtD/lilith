<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import Services from "@/modules/system/services";
import type {
  MigrationEntityInfo,
  ImportReport,
} from "@/modules/system/services/datamigration.service";
import { createBlobAndDownloadFile } from "@/utils/functions";

const { t } = useI18n();
const toast = useToast();

const entities = ref<MigrationEntityInfo[]>([]);
const selectedKeys = ref<string[]>([]);
const loading = ref(false);
const report = ref<ImportReport | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);
const fileInputKey = ref(0);

const hasSelection = computed(() => selectedKeys.value.length > 0);

const reportStatus = computed(() => {
  if (!report.value) return null;
  if (report.value.errors.length === 0)
    return { severity: "success", icon: "pi pi-check-circle", label: t("dataMigration.report.statusSuccess") };
  if (report.value.inserted === 0)
    return { severity: "danger", icon: "pi pi-times-circle", label: t("dataMigration.report.statusFailed") };
  return { severity: "warn", icon: "pi pi-exclamation-triangle", label: t("dataMigration.report.statusPartial") };
});

const allSelected = computed(
  () =>
    entities.value.length > 0 &&
    selectedKeys.value.length === entities.value.length,
);

onMounted(async () => {
  entities.value = await Services.DataMigration.getEntities();
});

const toggleAll = (checked: boolean) => {
  selectedKeys.value = checked ? entities.value.map((e) => e.key) : [];
};

const warnNoSelection = () => {
  toast.add({
    severity: "warn",
    summary: t("dataMigration.title"),
    detail: t("dataMigration.messages.noSelection"),
    life: 3000,
  });
};

const downloadTemplate = async () => {
  if (!hasSelection.value) return warnNoSelection();
  loading.value = true;
  const blob = await Services.DataMigration.downloadTemplate(selectedKeys.value);
  loading.value = false;

  if (blob) {
    createBlobAndDownloadFile("migration-template.xlsx", blob);
  } else {
    toast.add({
      severity: "error",
      summary: t("common.error"),
      detail: t("dataMigration.messages.templateError"),
      life: 4000,
    });
  }
};

const exportData = async () => {
  if (!hasSelection.value) return warnNoSelection();
  loading.value = true;
  const blob = await Services.DataMigration.exportData(selectedKeys.value);
  loading.value = false;

  if (blob) {
    createBlobAndDownloadFile("migration-export.xlsx", blob);
  } else {
    toast.add({
      severity: "error",
      summary: t("common.error"),
      detail: t("dataMigration.messages.exportError"),
      life: 4000,
    });
  }
};

const triggerImport = () => {
  if (!hasSelection.value) return warnNoSelection();
  if (fileInput.value) fileInput.value.value = "";
  fileInput.value?.click();
};

const onFileSelected = async (event: Event) => {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;

  loading.value = true;
  report.value = null;
  let result;
  try {
    result = await Services.DataMigration.import(file, selectedKeys.value);
  } finally {
    loading.value = false;
    input.value = "";
    fileInputKey.value++; // recreate the input so the same file can be picked again
  }

  if (!result) {
    toast.add({
      severity: "error",
      summary: t("common.error"),
      detail: t("dataMigration.messages.importError"),
      life: 4000,
    });
    return;
  }

  report.value = result;
  const severity = result.errors.length > 0 ? "warn" : "success";
  toast.add({
    severity,
    summary: t("dataMigration.title"),
    detail: t("dataMigration.messages.importDone", {
      inserted: result.inserted,
      skipped: result.skipped,
    }),
    life: 4000,
  });
};
</script>

<template>
  <div class="data-migration p-4">
    <div class="flex justify-content-between align-items-center mb-3">
      <div>
        <h2 class="m-0">{{ t("dataMigration.title") }}</h2>
        <p class="text-color-secondary mt-1 mb-0">
          {{ t("dataMigration.description") }}
        </p>
      </div>
    </div>

    <Card class="mb-3">
      <template #title>
        <div class="flex justify-content-between align-items-center">
          <span>{{ t("dataMigration.entitiesTitle") }}</span>
          <div class="flex align-items-center gap-2">
            <Checkbox
              :modelValue="allSelected"
              :binary="true"
              inputId="select-all"
              @update:modelValue="toggleAll"
            />
            <label for="select-all">{{ t("dataMigration.selectAll") }}</label>
          </div>
        </div>
      </template>
      <template #content>
        <div class="flex flex-column gap-2">
          <div
            v-for="entity in entities"
            :key="entity.key"
            class="flex align-items-center gap-2"
          >
            <Checkbox
              v-model="selectedKeys"
              :value="entity.key"
              :inputId="`entity-${entity.key}`"
            />
            <label :for="`entity-${entity.key}`">
              {{ t(entity.displayNameKey) }}
            </label>
          </div>
          <p v-if="entities.length === 0" class="text-color-secondary">
            {{ t("dataMigration.noEntities") }}
          </p>
        </div>
      </template>
    </Card>

    <div class="flex gap-2 flex-wrap mb-3">
      <Button
        :icon="PrimeIcons.DOWNLOAD"
        :label="t('dataMigration.downloadTemplate')"
        severity="secondary"
        :disabled="loading"
        @click="downloadTemplate"
      />
      <Button
        :icon="PrimeIcons.UPLOAD"
        :label="t('dataMigration.import')"
        :disabled="loading"
        @click="triggerImport"
      />
      <Button
        :icon="PrimeIcons.FILE_EXCEL"
        :label="t('dataMigration.export')"
        severity="success"
        :disabled="loading"
        @click="exportData"
      />
      <input
        :key="fileInputKey"
        ref="fileInput"
        type="file"
        accept=".xlsx"
        class="hidden"
        @change="onFileSelected"
      />
    </div>

    <Card v-if="report">
      <template #title>
        <div class="flex align-items-center gap-2">
          <Tag
            v-if="reportStatus"
            :severity="reportStatus.severity"
            :icon="reportStatus.icon"
            :value="reportStatus.label"
          />
          <span>{{ t("dataMigration.report.title") }}</span>
        </div>
      </template>
      <template #content>
        <div class="flex gap-4 mb-3 flex-wrap">
          <Tag severity="info" :value="`${t('dataMigration.report.total')}: ${report.total}`" />
          <Tag severity="success" :value="`${t('dataMigration.report.inserted')}: ${report.inserted}`" />
          <Tag severity="warn" :value="`${t('dataMigration.report.skipped')}: ${report.skipped}`" />
        </div>

        <DataTable
          v-if="report.errors.length > 0"
          :value="report.errors"
          stripedRows
          scrollable
          scrollHeight="40vh"
        >
          <Column field="sheet" :header="t('dataMigration.report.sheet')" />
          <Column field="row" :header="t('dataMigration.report.row')" />
          <Column field="code" :header="t('dataMigration.report.code')" />
          <Column field="reason" :header="t('dataMigration.report.reason')" />
        </DataTable>
        <p v-else class="text-color-secondary m-0">
          {{ t("dataMigration.report.noErrors") }}
        </p>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.hidden {
  display: none;
}
</style>
