<template>
  <div class="file-viewer-container">
    <!-- PDF Mode: Use existing PdfViewer -->
    <PdfViewer v-if="fileMode === 'pdf'" :file="file" />

    <!-- Image Mode: Custom image viewer with zoom -->
    <div v-else-if="fileMode === 'image'" class="image-viewer-container">
      <!-- Toolbar -->
      <Toolbar class="image-toolbar">
        <template #start>
          <div class="toolbar-controls">
            <Button
              :icon="PrimeIcons.MINUS"
              size="small"
              outlined
              :disabled="loading || zoomScale <= MIN_SCALE"
              @click="zoomOut"
              v-tooltip.bottom="t('fileViewer.toolbar.zoomOut')"
              class="zoom-button"
            />

            <span class="zoom-indicator"> {{ displayScale }}% </span>

            <Button
              :icon="PrimeIcons.PLUS"
              size="small"
              outlined
              :disabled="loading || zoomScale >= MAX_SCALE"
              @click="zoomIn"
              v-tooltip.bottom="t('fileViewer.toolbar.zoomIn')"
              class="zoom-button"
            />

            <Button
              :icon="PrimeIcons.REFRESH"
              size="small"
              outlined
              :disabled="loading"
              @click="resetZoom"
              v-tooltip.bottom="t('fileViewer.toolbar.resetZoom')"
              class="zoom-button"
            />
          </div>
        </template>

        <template #end>
          <div class="toolbar-actions">
            <!-- Fullscreen Toggle -->
            <Button
              :icon="
                isFullscreen
                  ? PrimeIcons.WINDOW_MINIMIZE
                  : PrimeIcons.WINDOW_MAXIMIZE
              "
              size="small"
              outlined
              :disabled="loading"
              @click="toggleFullscreen"
              v-tooltip.bottom="
                isFullscreen
                  ? t('fileViewer.toolbar.exitFullscreen')
                  : t('fileViewer.toolbar.fullscreen')
              "
            />

            <!-- Download -->
            <Button
              :icon="PrimeIcons.DOWNLOAD"
              size="small"
              outlined
              :disabled="loading"
              @click="downloadFile"
              v-tooltip.bottom="t('fileViewer.toolbar.downloadImage')"
            />
          </div>
        </template>
      </Toolbar>

      <!-- Image Content Area -->
      <div
        ref="containerRef"
        class="image-content-wrapper"
        :class="{ fullscreen: isFullscreen }"
      >
        <!-- Fullscreen Exit Button -->
        <Button
          v-if="isFullscreen"
          :icon="PrimeIcons.TIMES"
          rounded
          severity="secondary"
          class="fullscreen-exit-button"
          @click="toggleFullscreen"
          v-tooltip.left="t('fileViewer.toolbar.exitFullscreen')"
          :aria-label="t('fileViewer.toolbar.exitFullscreen')"
        />

        <!-- Fullscreen Zoom Controls -->
        <div v-if="isFullscreen" class="fullscreen-zoom-controls">
          <Button
            :icon="PrimeIcons.MINUS"
            rounded
            severity="secondary"
            :disabled="zoomScale <= MIN_SCALE"
            @click="zoomOut"
            v-tooltip.left="t('fileViewer.toolbar.zoomOut')"
            :aria-label="t('fileViewer.toolbar.zoomOut')"
          />
          <span class="fullscreen-zoom-indicator">{{ displayScale }}%</span>
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            severity="secondary"
            :disabled="zoomScale >= MAX_SCALE"
            @click="zoomIn"
            v-tooltip.left="t('fileViewer.toolbar.zoomIn')"
            :aria-label="t('fileViewer.toolbar.zoomIn')"
          />
          <Button
            :icon="PrimeIcons.REFRESH"
            rounded
            severity="secondary"
            @click="resetZoom"
            v-tooltip.left="t('fileViewer.toolbar.resetZoom')"
            :aria-label="t('fileViewer.toolbar.resetZoom')"
          />
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="image-loading-state">
          <ProgressSpinner
            style="width: 50px; height: 50px"
            strokeWidth="4"
            animationDuration="1s"
          />
          <p class="loading-text">{{ t("fileViewer.loading.image") }}</p>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="image-error-state">
          <i
            :class="PrimeIcons.EXCLAMATION_CIRCLE"
            style="font-size: 3rem; color: var(--p-red-500)"
          ></i>
          <p class="error-text">{{ error }}</p>
          <Button
            :label="t('fileViewer.error.retry')"
            outlined
            size="small"
            @click="loadImage"
          />
        </div>

        <!-- Image Viewer -->
        <div v-else-if="imageUrl" class="image-viewer-content">
          <div class="image-scroll-container">
            <img
              ref="imageRef"
              :src="imageUrl"
              :alt="file?.originalName || 'Imatge'"
              class="viewer-image"
              :style="imageStyle"
              @load="onImageLoaded"
              @error="onImageError"
            />
          </div>
        </div>

        <!-- No File State -->
        <div v-else class="image-no-file-state">
          <i
            :class="PrimeIcons.IMAGE"
            style="font-size: 4rem; color: var(--p-surface-400)"
          ></i>
          <p class="no-file-text">{{ t("fileViewer.noFile.selectFile") }}</p>
        </div>
      </div>
    </div>

    <!-- Unsupported Mode: Download panel -->
    <div v-else-if="fileMode === 'unsupported'" class="unsupported-container">
      <div class="unsupported-content">
        <div class="file-icon-wrapper">
          <i :class="getFileIcon()" class="file-icon"></i>
        </div>

        <div class="file-info">
          <h3 class="file-name">
            {{ file?.originalName || t("fileViewer.unsupported.title") }}
          </h3>
          <div class="file-details">
            <Tag
              :value="
                getFileExtension() || t('fileViewer.unsupported.noExtension')
              "
              severity="secondary"
            />
            <span class="file-size">{{ formatFileSize(file?.size || 0) }}</span>
          </div>
        </div>

        <Message severity="info" :closable="false" class="info-message">
          {{ t("fileViewer.unsupported.message") }}
        </Message>

        <Button
          :icon="PrimeIcons.DOWNLOAD"
          :label="t('fileViewer.unsupported.downloadButton')"
          size="large"
          :loading="downloading"
          @click="downloadFile"
          class="download-button"
        />
      </div>
    </div>

    <!-- No File Selected -->
    <div v-else class="no-file-container">
      <i
        :class="PrimeIcons.FILE"
        style="font-size: 4rem; color: var(--p-surface-400)"
      ></i>
      <p class="no-file-text">{{ t("fileViewer.noFile.selectDocument") }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import PdfViewer from "./PdfViewer.vue";
import { FileService } from "../services/file.service";
import { createBlobAndDownloadFile } from "../utils/functions";
import type { File } from "../types";

// PrimeVue components
import Toolbar from "primevue/toolbar";
import Button from "primevue/button";
import ProgressSpinner from "primevue/progressspinner";
import Tag from "primevue/tag";
import Message from "primevue/message";

interface Props {
  file: File | null;
}

const props = defineProps<Props>();
const { t } = useI18n();
const toast = useToast();

// Services
const fileService = new FileService();

// Refs
const containerRef = ref<HTMLElement | null>(null);
const imageRef = ref<HTMLImageElement | null>(null);

// Zoom constants
const MIN_SCALE = 0.5;
const MAX_SCALE = 3.0;
const SCALE_STEP = 0.25;
const DEFAULT_SCALE = 1.0;

// File type extensions
const PDF_EXTENSIONS = [".pdf"];
const IMAGE_EXTENSIONS = [
  ".jpg",
  ".jpeg",
  ".png",
  ".gif",
  ".webp",
  ".bmp",
  ".svg",
];

// State
const imageUrl = ref<string | null>(null);
const imageBlob = ref<Blob | null>(null);
const loading = ref(false);
const downloading = ref(false);
const error = ref<string | null>(null);
const isFullscreen = ref(false);
const zoomScale = ref(DEFAULT_SCALE);
const imageNaturalWidth = ref(0);
const imageNaturalHeight = ref(0);

// Computed: Determine file mode based on extension
const fileMode = computed((): "pdf" | "image" | "unsupported" | null => {
  if (!props.file) return null;

  const extension = getFileExtension().toLowerCase();

  if (PDF_EXTENSIONS.includes(extension)) {
    return "pdf";
  }

  if (IMAGE_EXTENSIONS.includes(extension)) {
    return "image";
  }

  return "unsupported";
});

// Computed: Display scale percentage
const displayScale = computed(() => Math.round(zoomScale.value * 100));

// Computed: Image transform style
const imageStyle = computed(() => ({
  transform: `scale(${zoomScale.value})`,
  transformOrigin: "center center",
}));

// Get file extension with dot
const getFileExtension = (): string => {
  if (!props.file?.originalName) return "";
  const name = props.file.originalName.trim();
  const lastDot = name.lastIndexOf(".");
  if (lastDot === -1 || lastDot === name.length - 1) return "";
  return "." + name.substring(lastDot + 1).toLowerCase();
};

// Get appropriate icon for file type
const getFileIcon = (): string => {
  const ext = getFileExtension().toLowerCase();

  // Document types
  if ([".doc", ".docx"].includes(ext)) return PrimeIcons.FILE_WORD;
  if ([".xls", ".xlsx"].includes(ext)) return PrimeIcons.FILE_EXCEL;
  if ([".ppt", ".pptx"].includes(ext)) return "pi pi-file";
  if ([".txt", ".rtf"].includes(ext)) return "pi pi-file-edit";
  if ([".zip", ".rar", ".7z", ".tar", ".gz"].includes(ext))
    return "pi pi-folder-open";
  if ([".mp4", ".avi", ".mov", ".mkv", ".webm"].includes(ext))
    return "pi pi-video";
  if ([".mp3", ".wav", ".ogg", ".flac"].includes(ext)) return "pi pi-volume-up";

  return PrimeIcons.FILE;
};

// Format file size
const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
};

// Zoom functions
const zoomIn = () => {
  if (zoomScale.value < MAX_SCALE) {
    zoomScale.value = Math.min(zoomScale.value + SCALE_STEP, MAX_SCALE);
  }
};

const zoomOut = () => {
  if (zoomScale.value > MIN_SCALE) {
    zoomScale.value = Math.max(zoomScale.value - SCALE_STEP, MIN_SCALE);
  }
};

const resetZoom = () => {
  zoomScale.value = DEFAULT_SCALE;
};

// Keyboard shortcuts for zoom
const handleKeyboard = (e: KeyboardEvent) => {
  // Only handle if in image mode
  if (fileMode.value !== "image") return;

  if (e.ctrlKey || e.metaKey) {
    if (e.key === "+" || e.key === "=") {
      e.preventDefault();
      zoomIn();
    } else if (e.key === "-") {
      e.preventDefault();
      zoomOut();
    } else if (e.key === "0") {
      e.preventDefault();
      resetZoom();
    }
  }

  // ESC for fullscreen
  if (e.key === "Escape" && isFullscreen.value) {
    toggleFullscreen();
  }
};

// Fullscreen
const toggleFullscreen = async () => {
  if (!containerRef.value) return;

  try {
    if (!isFullscreen.value) {
      if (containerRef.value.requestFullscreen) {
        await containerRef.value.requestFullscreen();
      }
    } else {
      if (document.exitFullscreen) {
        await document.exitFullscreen();
      }
    }
  } catch (err) {
    toast.add({
      severity: "error",
      summary: t("fileViewer.toast.fullscreenError"),
      detail: t("fileViewer.error.fullscreen"),
      life: 4000,
    });
  }
};

// Handle fullscreen change events
const handleFullscreenChange = () => {
  isFullscreen.value = !!document.fullscreenElement;
};

// Load image
const loadImage = async () => {
  if (!props.file || fileMode.value !== "image") {
    imageUrl.value = null;
    imageBlob.value = null;
    return;
  }

  loading.value = true;
  error.value = null;

  // Clean up previous URL
  if (imageUrl.value) {
    URL.revokeObjectURL(imageUrl.value);
    imageUrl.value = null;
  }

  try {
    const { blob, contentType } = await fileService.Download(props.file);

    if (!blob) {
      throw new Error(t("fileViewer.error.loadImage"));
    }

    // Store blob and create object URL
    imageBlob.value = new Blob([blob], { type: contentType || "image/jpeg" });
    imageUrl.value = URL.createObjectURL(imageBlob.value);
  } catch (err: any) {
    console.error("Error loading image:", err);
    error.value = err?.message || t("fileViewer.error.loadImage");
    toast.add({
      severity: "error",
      summary: t("fileViewer.toast.loadError"),
      detail: error.value,
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
};

// Image events
const onImageLoaded = () => {
  if (imageRef.value) {
    imageNaturalWidth.value = imageRef.value.naturalWidth;
    imageNaturalHeight.value = imageRef.value.naturalHeight;
  }
};

const onImageError = () => {
  error.value = t("fileViewer.error.loadImage");
  loading.value = false;
};

// Download file
const downloadFile = async () => {
  if (!props.file) return;

  downloading.value = true;

  try {
    const { blob, contentType } = await fileService.Download(props.file);
    if (blob) {
      createBlobAndDownloadFile(props.file.originalName, blob, contentType);
      toast.add({
        severity: "success",
        summary: t("fileViewer.toast.downloadSuccess"),
        detail: `${props.file.originalName}`,
        life: 3000,
      });
    }
  } catch (err) {
    toast.add({
      severity: "error",
      summary: t("fileViewer.toast.downloadError"),
      detail: t("fileViewer.error.download"),
      life: 4000,
    });
  } finally {
    downloading.value = false;
  }
};

// Watch for file changes
watch(
  () => props.file,
  (newFile, oldFile) => {
    // Reset zoom when file changes
    zoomScale.value = DEFAULT_SCALE;
    error.value = null;

    // Clean up old image URL
    if (imageUrl.value) {
      URL.revokeObjectURL(imageUrl.value);
      imageUrl.value = null;
    }

    if (newFile && fileMode.value === "image") {
      loadImage();
    }
  },
  { immediate: true },
);

// Lifecycle
onMounted(() => {
  document.addEventListener("fullscreenchange", handleFullscreenChange);
  document.addEventListener("keydown", handleKeyboard);
});

onUnmounted(() => {
  document.removeEventListener("fullscreenchange", handleFullscreenChange);
  document.removeEventListener("keydown", handleKeyboard);

  // Clean up image URL
  if (imageUrl.value) {
    URL.revokeObjectURL(imageUrl.value);
  }
});
</script>

<style scoped>
.file-viewer-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 100%;
}

/* ===== IMAGE VIEWER STYLES ===== */
.image-viewer-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

/* Toolbar */
.image-toolbar {
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border-bottom: 1px solid #e2e8f0;
  padding: 0.75rem 1rem;
  border-radius: 0;
}

.toolbar-controls,
.toolbar-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.zoom-indicator {
  font-size: 0.875rem;
  font-weight: 600;
  color: #475569;
  padding: 0 0.5rem;
  min-width: 48px;
  text-align: center;
}

.zoom-button {
  transition: all 0.2s ease;
}

.zoom-button:hover:not(:disabled) {
  transform: scale(1.05);
}

/* Content Wrapper */
.image-content-wrapper {
  flex: 1;
  overflow: auto;
  background: #f8fafc;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.image-content-wrapper.fullscreen {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 9999;
  border-radius: 0;
  background: #1e293b;
}

/* Image scroll container for zoomed images */
.image-scroll-container {
  width: 100%;
  height: 100%;
  overflow: auto;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Image Content */
.image-viewer-content {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  box-sizing: border-box;
}

.viewer-image {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  transition: transform 0.2s ease;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.image-content-wrapper.fullscreen .viewer-image {
  max-height: 90vh;
  max-width: 95vw;
}

/* Fullscreen Exit Button */
.fullscreen-exit-button {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 10000;
  backdrop-filter: blur(8px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transition: all 0.2s ease;
}

.fullscreen-exit-button:hover {
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.2);
  transform: scale(1.05);
}

/* Fullscreen Zoom Controls */
.fullscreen-zoom-controls {
  position: fixed;
  bottom: 2rem;
  right: 1rem;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  background: rgba(255, 255, 255, 0.95);
  padding: 0.75rem;
  border-radius: 2rem;
  backdrop-filter: blur(8px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.fullscreen-zoom-indicator {
  font-size: 0.875rem;
  font-weight: 600;
  color: #475569;
  min-width: 48px;
  text-align: center;
  padding: 0.25rem 0;
}

/* Loading State */
.image-loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  padding: 3rem;
}

.loading-text {
  font-size: 1rem;
  color: #64748b;
  margin: 0;
  font-weight: 500;
}

/* Error State */
.image-error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  padding: 3rem;
  text-align: center;
}

.error-text {
  font-size: 1rem;
  color: #ef4444;
  margin: 0;
  font-weight: 500;
  max-width: 400px;
}

/* No File State */
.image-no-file-state,
.no-file-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  padding: 3rem;
  opacity: 0.6;
  height: 100%;
}

.no-file-text {
  font-size: 1.125rem;
  color: #64748b;
  margin: 0;
  font-weight: 500;
}

/* ===== UNSUPPORTED FILE STYLES ===== */
.unsupported-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  padding: 2rem;
}

.unsupported-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
  max-width: 400px;
  text-align: center;
}

.file-icon-wrapper {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
  display: flex;
  align-items: center;
  justify-content: center;
}

.file-icon {
  font-size: 3rem;
  color: #64748b;
}

.file-info {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.file-name {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0;
  word-break: break-word;
}

.file-details {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
}

.file-size {
  font-size: 0.875rem;
  color: #64748b;
}

.info-message {
  margin: 0;
}

.download-button {
  margin-top: 0.5rem;
}

/* ===== RESPONSIVE ===== */
@media (max-width: 768px) {
  .image-toolbar {
    padding: 0.5rem;
  }

  .toolbar-controls,
  .toolbar-actions {
    gap: 0.25rem;
  }

  .zoom-indicator {
    font-size: 0.75rem;
    padding: 0 0.25rem;
  }

  .image-viewer-content {
    padding: 12px 8px;
  }

  .unsupported-container {
    padding: 1rem;
  }

  .file-icon-wrapper {
    width: 80px;
    height: 80px;
  }

  .file-icon {
    font-size: 2.5rem;
  }

  .file-name {
    font-size: 1.1rem;
  }
}

/* Scrollbar styling */
.image-content-wrapper::-webkit-scrollbar,
.image-scroll-container::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}

.image-content-wrapper::-webkit-scrollbar-track,
.image-scroll-container::-webkit-scrollbar-track {
  background: #f1f5f9;
}

.image-content-wrapper::-webkit-scrollbar-thumb,
.image-scroll-container::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 5px;
}

.image-content-wrapper::-webkit-scrollbar-thumb:hover,
.image-scroll-container::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
