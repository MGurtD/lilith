<script setup lang="ts">
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import FileViewer from "@/components/FileViewer.vue";
import { FileService } from "@/services/file.service";
import type { File } from "@/types";
import type { AttachmentConfig } from "./types";

const props = defineProps<{
  config: AttachmentConfig;
}>();

const { t } = useI18n();
const toast = useToast();
const fileService = new FileService();

const visible = ref(false);
const loading = ref(false);
const files = ref<File[]>([]);
const selectedFile = ref<File | null>(null);
const selectedItem = ref<unknown>(null);

let latestRequestId = 0;

const dialogHeader = computed(() => {
  const title = props.config.title ?? t("table.attachments.dialogTitle");
  const item = selectedItem.value;
  const field = props.config.titleField;
  const value = field && item && typeof item === "object"
    ? (item as Record<string, unknown>)[field]
    : null;
  const identifier = typeof value === "string" || typeof value === "number"
    ? String(value)
    : null;

  return identifier ? `${title} - ${identifier}` : title;
});

function getItemId(item: unknown): string | null {
  if (!item || typeof item !== "object") return null;
  const id = (item as { id?: unknown }).id;
  return typeof id === "string" && id.length > 0 ? id : null;
}

async function open(item: unknown): Promise<void> {
  const id = getItemId(item);
  if (!props.config.entity || !id) return;

  const requestId = ++latestRequestId;

  // This request is deliberately initiated only by the table action click.
  // The component never preloads files or counts for table rows.
  selectedItem.value = item;
  files.value = [];
  selectedFile.value = null;
  visible.value = true;
  loading.value = true;

  try {
    const entityFiles = (await fileService.GetEntityFiles(props.config.entity, id)) ?? [];
    if (requestId !== latestRequestId) return;

    files.value = entityFiles;
    selectedFile.value = entityFiles[0] ?? null;
  } catch (error: unknown) {
    if (requestId !== latestRequestId) return;

    console.error("[TableAttachmentViewer] attachment load failed", error);
    toast.add({
      severity: "error",
      summary: t("table.attachments.tooltip"),
      detail: t("table.attachments.loadError"),
      life: 5000,
    });
  } finally {
    if (requestId === latestRequestId) {
      loading.value = false;
    }
  }
}

defineExpose({ open });
</script>

<template>
  <Dialog
    v-model:visible="visible"
    :header="dialogHeader"
    modal
    :style="{ width: '90vw', height: '85vh' }"
  >
    <div class="attachments-dialog-content">
      <ProgressSpinner
        v-if="loading"
        style="width: 50px; height: 50px"
        strokeWidth="4"
      />
      <template v-else-if="files.length > 0">
        <div v-if="files.length > 1" class="attachment-file-selector">
          <Button
            v-for="file in files"
            :key="file.id"
            :label="file.originalName"
            size="small"
            :severity="selectedFile?.id === file.id ? 'primary' : 'secondary'"
            :outlined="selectedFile?.id !== file.id"
            @click="selectedFile = file"
          />
        </div>
        <FileViewer
          :key="selectedFile?.id"
          :file="selectedFile"
          class="attachment-file-viewer"
        />
      </template>
      <div v-else class="attachments-empty-state">
        <i class="pi pi-paperclip" aria-hidden="true"></i>
        <p>{{ t("table.attachments.empty") }}</p>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.attachments-dialog-content {
  display: flex;
  flex-direction: column;
  height: calc(85vh - 7rem);
  min-height: 20rem;
  gap: 1rem;
}

.attachment-file-selector {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.attachment-file-selector :deep(.p-button) {
  cursor: pointer;
}

.attachment-file-viewer {
  flex: 1;
  min-height: 0;
}

.attachments-empty-state {
  display: flex;
  flex: 1;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  color: var(--p-text-muted-color);
  text-align: center;
}

.attachments-empty-state i {
  font-size: 2.5rem;
}
</style>