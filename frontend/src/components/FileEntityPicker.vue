<template>
  <div class="base_input">
    <Toolbar>
      <template #start>
        <p class="file_viewer_title">{{ title }}</p>
      </template>
      <template #end>
        <Button
          size="small"
          :disabled="!canLoadMoreFiles() || loading"
          :loading="loading"
          rounded
          :icon="PrimeIcons.UPLOAD"
          @click="uploadFile"
        />
      </template>
    </Toolbar>

    <section v-if="!loading" class="file-viewer">
      <article class="file-viewer-item" v-for="file in files" :key="file.id">
        <div class="file-viewer-item-type">
          <i :class="getFileIcon(file)" style="font-size: 2.5rem" />
          <p>
            {{ file.originalName.substring(0, 20) }}
          </p>
        </div>
        <div
          :class="
            file.type === FileType.IMAGE ||
            file.originalName.toLowerCase().endsWith('.pdf')
              ? 'file-viewer-item-actions-image'
              : 'file-viewer-item-actions-file'
          "
        >
          <div
            v-if="
              file.type === FileType.IMAGE ||
              file.originalName.toLowerCase().endsWith('.pdf')
            "
            class="file-viewer-item-action file-viewer-item-action-show"
            @click="showFile(file)"
          >
            <i :class="PrimeIcons.EYE" style="font-size: 1rem" />
          </div>
          <div
            class="file-viewer-item-action file-viewer-item-action-download"
            @click="downloadFile(file)"
          >
            <i :class="PrimeIcons.DOWNLOAD" style="font-size: 1rem" />
          </div>
          <div
            class="file-viewer-item-action file-viewer-item-action-delete"
            @click="deleteFile(file)"
          >
            <i :class="PrimeIcons.TIMES" />
          </div>
        </div>
      </article>
    </section>
  </div>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: dialogOptions.isPdf ? '90vw' : 'auto' }"
  >
    <div v-if="!dialogOptions.isPdf" id="img-container"></div>
    <PdfViewer v-else :file="dialogOptions.selectedFile" />
  </Dialog>
</template>
<script setup lang="ts">
import { onMounted, reactive, ref, watch } from "vue";
import { FileService } from "../services/file.service";
import { File } from "../types";
import Toolbar from "primevue/toolbar";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { loadImage, createBlobAndDownloadFile } from "../utils/functions";
import { useConfirm } from "primevue/useconfirm";
import { DialogOptions, FileType } from "../types/component";
import PdfViewer from "./PdfViewer.vue";

const props = defineProps<{
  title: string;
  entity: string;
  id: string;
  maxFiles?: number;
}>();

const toast = useToast();
const confirm = useConfirm();

const service = new FileService();
const files = ref(undefined as undefined | Array<File>);
const loading = ref(false);

// Instancia para controlar el diálogo
const dialogOptions = reactive({
  visible: false,
  title: "Detalle del document",
  closable: true,
  position: "center",
  modal: true,
  isPdf: false,
  selectedFile: null as File | null,
} as DialogOptions);

const fetchData = async () => {
  if (!props.id || loading.value) return;

  loading.value = true;
  try {
    files.value = await service.GetEntityFiles(props.entity, props.id);
  } catch (error) {
    console.error("Error fetching files:", error);
    toast.add({
      severity: "error",
      detail: "Error al carregar els arxius",
    });
  } finally {
    loading.value = false;
  }
};

// Watch solo para cambios después del mount inicial
watch(
  () => props.id,
  (newValue, oldValue) => {
    // Solo ejecutar si el componente ya está montado y el valor realmente cambió
    if (newValue && newValue !== oldValue && oldValue !== undefined) {
      fetchData();
    }
  }
);

onMounted(async () => {
  if (props.id) {
    await fetchData();
  }
});

const canLoadMoreFiles = () => {
  if (!props.maxFiles) {
    return true;
  }

  if (files.value) {
    return files.value?.length < props.maxFiles;
  }
};

const uploadFile = async () => {
  const input = document.createElement("input");
  input.type = "file";

  input.onchange = async (e: any) => {
    const file = e.target.files[0];

    const uploaded = await service.Upload(file, props.entity, props.id);
    if (!uploaded) {
      toast.add({
        severity: "error",
        detail: "Error al carregar l'arxiu",
      });
      return;
    }
    await fetchData();
  };

  input.click();
};

const showFile = async (file: File) => {
  const isPdf = file.originalName.toLowerCase().endsWith(".pdf");

  dialogOptions.isPdf = isPdf;
  dialogOptions.selectedFile = file;
  dialogOptions.title = isPdf ? "Visualitzador de PDF" : "Detalle de la imatge";
  dialogOptions.visible = true;

  // Si es una imatge, carregar-la al contenidor
  if (!isPdf) {
    const { blob: downloadedBlob, contentType } = await service.Download(file);
    const blob = new Blob([downloadedBlob], {
      type: contentType || "image/jpeg",
    });
    const imageElement = await loadImage(blob);

    // Netegem el contenidor abans d'afegir la nova imatge
    const container = document.getElementById("img-container");
    if (container) {
      container.innerHTML = "";
      container.appendChild(imageElement);
    }
  }
  // Si es un PDF, el component PdfViewer s'encarregarà de carregar-lo
};

const downloadFile = async (file: File) => {
  const { blob, contentType } = await service.Download(file);
  createBlobAndDownloadFile(file.originalName, blob, contentType);
};

const getFileIcon = (file: File): string => {
  if (file.type === FileType.IMAGE) {
    return PrimeIcons.IMAGE;
  }

  const fileName = file.originalName.toLowerCase();
  if (fileName.endsWith(".pdf")) {
    return PrimeIcons.FILE_PDF;
  }
  if (fileName.endsWith(".docx") || fileName.endsWith(".doc")) {
    return PrimeIcons.FILE_WORD;
  }
  if (fileName.endsWith(".xlsx") || fileName.endsWith(".xls")) {
    return PrimeIcons.FILE_EXCEL;
  }

  return PrimeIcons.FILE;
};

const deleteFile = async (file: File) => {
  confirm.require({
    message: "Està segur que vol eliminar l'arxiu seleccionat",
    accept: async () => {
      var deleted = await service.Delete(file.id);
      if (!deleted) {
        toast.add({
          severity: "error",
          detail: "Error al eliminar l'arxiu",
        });
        return;
      }
      await fetchData();
    },
  });
};
</script>
<style scoped>
.base_input {
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  transition: all 0.3s ease;
}

.base_input:hover {
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.12);
}

/* Toolbar moderno */
.file_viewer_title {
  font-weight: 700;
  font-size: 1rem;
  color: #000;
  margin: 0;
}

/* Grid responsive moderno */
.file-viewer {
  padding: 1.5rem;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: 1rem;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  min-height: 150px;
}

@media (max-width: 768px) {
  .file-viewer {
    grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
    padding: 1rem;
    gap: 0.75rem;
  }
}

@media (max-width: 480px) {
  .file-viewer {
    grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
    padding: 0.75rem;
    gap: 0.5rem;
  }
}

/* Cards modernas */
.file-viewer-item {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  transition: all 0.3s ease;
  cursor: pointer;
  aspect-ratio: 0.85;
  max-height: 180px;
}

.file-viewer-item:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
  border-color: #3b82f6;
}

.file-viewer-item-type {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: linear-gradient(135deg, #f8fafc 0%, #ffffff 100%);
  position: relative;
}

.file-viewer-item-type i {
  color: #3b82f6;
  margin-bottom: 0.5rem;
  transition: all 0.3s ease;
  font-size: 2.5rem !important;
}

.file-viewer-item:hover .file-viewer-item-type i {
  transform: scale(1.1);
  color: #1e40af;
}

.file-viewer-item-type p {
  margin: 0;
  font-size: 0.75rem;
  font-weight: 500;
  color: #475569;
  text-align: center;
  line-height: 1.2;
  word-break: break-word;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}

/* Actions bar moderna */
.file-viewer-item-actions-file,
.file-viewer-item-actions-image {
  display: flex;
  height: 40px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
}

.file-viewer-item-action {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  font-size: 0.875rem;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.file-viewer-item-action::before {
  content: "";
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(
    90deg,
    transparent,
    rgba(255, 255, 255, 0.2),
    transparent
  );
  transition: left 0.5s;
}

.file-viewer-item-action:hover::before {
  left: 100%;
}

.file-viewer-item-action:hover {
  transform: translateY(-1px);
  cursor: pointer;
}

.file-viewer-item-action i {
  font-size: 1rem;
  transition: transform 0.2s ease;
}

.file-viewer-item-action:hover i {
  transform: scale(1.1);
}

/* Colores modernos para las acciones */
.file-viewer-item-action-show {
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
}

.file-viewer-item-action-show:hover {
  background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
}

.file-viewer-item-action-download {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
}

.file-viewer-item-action-download:hover {
  background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
}

.file-viewer-item-action-delete {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
}

.file-viewer-item-action-delete:hover {
  background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
}

/* Separadores entre acciones */
.file-viewer-item-actions-file .file-viewer-item-action:not(:last-child),
.file-viewer-item-actions-image .file-viewer-item-action:not(:last-child) {
  border-right: 1px solid rgba(255, 255, 255, 0.2);
}

#img-container {
  display: flex;
  justify-content: center;
  align-items: center;
  max-width: 100%;
  max-height: 70vh;
  overflow: hidden;
  border-radius: 8px;
}

#img-container img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

/* PDF Viewer in dialog */
:deep(.pdf-viewer-container) {
  height: 80vh;
  max-height: 80vh;
}
</style>
