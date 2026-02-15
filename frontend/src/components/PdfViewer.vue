<template>
  <div class="pdf-viewer-container">
    <!-- Toolbar -->
    <Toolbar class="pdf-toolbar">
      <template #start>
        <div class="toolbar-controls">
          <Button
            :icon="PrimeIcons.MINUS"
            size="small"
            outlined
            :disabled="loading || zoomScale <= MIN_SCALE"
            @click="zoomOut"
            v-tooltip.bottom="'Allunyar (Ctrl + -)'"
            class="zoom-button"
          />

          <span class="zoom-indicator"> {{ displayScale }}% </span>

          <Button
            :icon="PrimeIcons.PLUS"
            size="small"
            outlined
            :disabled="loading || zoomScale >= MAX_SCALE"
            @click="zoomIn"
            v-tooltip.bottom="'Apropar (Ctrl + +)'"
            class="zoom-button"
          />

          <Button
            :icon="PrimeIcons.REFRESH"
            size="small"
            outlined
            :disabled="loading"
            @click="resetZoom"
            v-tooltip.bottom="'Restablir zoom (Ctrl + 0)'"
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
              isFullscreen ? 'Sortir de pantalla completa' : 'Pantalla completa'
            "
          />

          <!-- Download -->
          <Button
            :icon="PrimeIcons.DOWNLOAD"
            size="small"
            outlined
            :disabled="loading"
            @click="downloadFile"
            v-tooltip.bottom="'Descarregar document'"
          />
        </div>
      </template>
    </Toolbar>

    <!-- PDF Content Area -->
    <div
      ref="containerRef"
      class="pdf-content-wrapper"
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
        v-tooltip.left="'Sortir de pantalla completa (ESC)'"
        aria-label="Sortir de pantalla completa"
      />

      <!-- Fullscreen Zoom Controls -->
      <div v-if="isFullscreen" class="fullscreen-zoom-controls">
        <Button
          :icon="PrimeIcons.MINUS"
          rounded
          severity="secondary"
          :disabled="zoomScale <= MIN_SCALE"
          @click="zoomOut"
          v-tooltip.left="'Allunyar (Ctrl + -)'"
          aria-label="Allunyar"
        />
        <span class="fullscreen-zoom-indicator">{{ displayScale }}%</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          severity="secondary"
          :disabled="zoomScale >= MAX_SCALE"
          @click="zoomIn"
          v-tooltip.left="'Apropar (Ctrl + +)'"
          aria-label="Apropar"
        />
        <Button
          :icon="PrimeIcons.REFRESH"
          rounded
          severity="secondary"
          @click="resetZoom"
          v-tooltip.left="'Restablir zoom (Ctrl + 0)'"
          aria-label="Restablir zoom"
        />
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="pdf-loading-state">
        <ProgressSpinner
          style="width: 50px; height: 50px"
          strokeWidth="4"
          animationDuration="1s"
        />
        <p class="loading-text">Carregant document...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="pdf-error-state">
        <i
          :class="PrimeIcons.EXCLAMATION_CIRCLE"
          style="font-size: 3rem; color: var(--p-red-500)"
        ></i>
        <p class="error-text">{{ error }}</p>
        <Button
          label="Tornar a intentar"
          outlined
          size="small"
          @click="loadPdf"
        />
      </div>

      <!-- PDF Viewer -->
      <div v-else-if="pdfSource" class="pdf-viewer-content">
        <VuePdfEmbed
          :source="pdfSource"
          :width="pdfWidth"
          @rendered="onRendered"
          @loading-failed="onLoadingFailed"
          @loaded="onLoaded"
        />
      </div>

      <!-- No File State -->
      <div v-else class="pdf-no-file-state">
        <i
          :class="PrimeIcons.FILE_PDF"
          style="font-size: 4rem; color: var(--p-surface-400)"
        ></i>
        <p class="no-file-text">Selecciona un document per visualitzar</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from "vue";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import VuePdfEmbed from "vue-pdf-embed";
import { FileService } from "../services/file.service";
import { createBlobAndDownloadFile } from "../utils/functions";
import type { File } from "../types";

// PrimeVue components
import Toolbar from "primevue/toolbar";
import Button from "primevue/button";
import ProgressSpinner from "primevue/progressspinner";

interface Props {
  file: File | null;
}

const props = defineProps<Props>();
const toast = useToast();

// Services
const fileService = new FileService();

// Refs
const containerRef = ref<HTMLElement | null>(null);

// Zoom constants - usar percentatge del contenidor
const MIN_SCALE = 0.5;
const MAX_SCALE = 3.0;
const SCALE_STEP = 0.25;
const DEFAULT_SCALE = 1.0;

// State
const pdfSource = ref<ArrayBuffer | null>(null);
const pdfBlob = ref<Blob | null>(null); // Store original blob
const loading = ref(false);
const error = ref<string | null>(null);
const totalPages = ref(0);
const isFullscreen = ref(false);
const zoomScale = ref(DEFAULT_SCALE);
const containerWidth = ref(0);

// Computed width based on container and zoom
const pdfWidth = computed(() => {
  if (containerWidth.value === 0) return undefined;
  return Math.floor(containerWidth.value * zoomScale.value);
});

// Computed scale for display
const displayScale = computed(() => {
  return Math.round(zoomScale.value * 100);
});

// Update container width
const updateContainerWidth = () => {
  if (containerRef.value) {
    const contentWrapper = containerRef.value.querySelector(
      ".pdf-viewer-content"
    ) as HTMLElement;
    if (contentWrapper) {
      // Get available width minus padding
      containerWidth.value = contentWrapper.clientWidth - 32; // 16px padding on each side
    }
  }
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
      summary: "Error de pantalla completa",
      detail: "No s'ha pogut canviar el mode de pantalla",
      life: 4000,
    });
  }
};

// Handle fullscreen change events
const handleFullscreenChange = () => {
  isFullscreen.value = !!document.fullscreenElement;
  // Recalculate container width on fullscreen change
  setTimeout(() => updateContainerWidth(), 100);
};

// Load PDF
const loadPdf = async () => {
  if (!props.file) {
    pdfSource.value = null;
    pdfBlob.value = null;
    return;
  }

  loading.value = true;
  error.value = null;
  pdfSource.value = null;

  try {
    const { blob } = await fileService.Download(props.file);

    if (!blob) {
      throw new Error("No s'ha pogut descarregar el document");
    }

    // Store blob and convert to ArrayBuffer
    pdfBlob.value = blob;
    const arrayBuffer = await blob.arrayBuffer();
    pdfSource.value = arrayBuffer;
  } catch (err: any) {
    console.error("Error loading PDF:", err);
    error.value = err?.message || "Error en carregar el document PDF";
    toast.add({
      severity: "error",
      summary: "Error de càrrega",
      detail: error.value,
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
};

// Download file
const downloadFile = async () => {
  if (!props.file) return;

  try {
    const { blob, contentType } = await fileService.Download(props.file);
    if (blob) {
      createBlobAndDownloadFile(props.file.originalName, blob, contentType);
      toast.add({
        severity: "success",
        summary: "Descàrrega completada",
        detail: `${props.file.originalName}`,
        life: 3000,
      });
    }
  } catch (err) {
    toast.add({
      severity: "error",
      summary: "Error de descàrrega",
      detail: "No s'ha pogut descarregar el document",
      life: 4000,
    });
  }
};

// PDF Events
const onRendered = () => {
  loading.value = false;
  // Update container width after first render
  if (containerWidth.value === 0) {
    setTimeout(() => updateContainerWidth(), 100);
  }
};

const onLoaded = ({ numPages }: { numPages: number }) => {
  totalPages.value = numPages;
};

const onLoadingFailed = (err: any) => {
  console.error("PDF loading failed:", err);
  error.value = "No s'ha pogut carregar el document PDF";
  loading.value = false;
  toast.add({
    severity: "error",
    summary: "Error de càrrega",
    detail: error.value,
    life: 5000,
  });
};

// Watch for file changes
watch(
  () => props.file,
  (newFile) => {
    if (newFile) {
      // Reset zoom when loading new file
      zoomScale.value = DEFAULT_SCALE;
      loadPdf();
    } else {
      pdfSource.value = null;
      pdfBlob.value = null;
      error.value = null;
      totalPages.value = 0;
      zoomScale.value = DEFAULT_SCALE;
    }
  },
  { immediate: true }
);

// Lifecycle
onMounted(() => {
  document.addEventListener("fullscreenchange", handleFullscreenChange);
  document.addEventListener("keydown", handleKeyboard);

  // Initial container width calculation
  setTimeout(() => updateContainerWidth(), 100);

  // Recalculate on window resize
  window.addEventListener("resize", updateContainerWidth);
});

onUnmounted(() => {
  document.removeEventListener("fullscreenchange", handleFullscreenChange);
  document.removeEventListener("keydown", handleKeyboard);
  window.removeEventListener("resize", updateContainerWidth);
});
</script>

<style scoped>
.pdf-viewer-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

/* Toolbar */
.pdf-toolbar {
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

.page-indicator,
.zoom-indicator {
  font-size: 0.875rem;
  font-weight: 600;
  color: #475569;
  padding: 0 0.5rem;
}

.zoom-indicator {
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
.pdf-content-wrapper {
  flex: 1;
  overflow: auto;
  background: #f8fafc;
  position: relative;
}

.pdf-content-wrapper.fullscreen {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 9999;
  border-radius: 0;
  background: #f8fafc;
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

/* PDF Content */
.pdf-viewer-content {
  padding: 16px;
  width: 100%;
  box-sizing: border-box;
}

/* Make PDF responsive - critical for proper display */
:deep(.vue-pdf-embed) {
  margin: 0 auto;
  display: block;
  max-width: 100%;
}

:deep(.vue-pdf-embed > div) {
  max-width: 100%;
}

.pdf-content-wrapper.fullscreen :deep(.vue-pdf-embed) {
  display: flex;
  flex-direction: column;
  align-items: center;
}

:deep(.vue-pdf-embed__page) {
  margin-bottom: 8px;
  box-shadow: 0 2px 8px 4px rgba(0, 0, 0, 0.1);
}

:deep(.vue-pdf-embed__page canvas) {
  margin: 0 auto;
}

/* Loading State */
.pdf-loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  padding: 3rem;
  height: 100%;
}

.loading-text {
  font-size: 1rem;
  color: #64748b;
  margin: 0;
  font-weight: 500;
}

/* Error State */
.pdf-error-state {
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
.pdf-no-file-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  padding: 3rem;
  opacity: 0.6;
}

.no-file-text {
  font-size: 1.125rem;
  color: #64748b;
  margin: 0;
  font-weight: 500;
}

/* Responsive */
@media (max-width: 768px) {
  .pdf-toolbar {
    padding: 0.5rem;
  }

  .toolbar-controls,
  .toolbar-actions {
    gap: 0.25rem;
  }

  .page-indicator,
  .zoom-indicator {
    font-size: 0.75rem;
    padding: 0 0.25rem;
  }

  .pdf-viewer-content {
    padding: 12px 8px;
  }
}

/* Scrollbar styling */
.pdf-content-wrapper::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}

.pdf-content-wrapper::-webkit-scrollbar-track {
  background: #f1f5f9;
}

.pdf-content-wrapper::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 5px;
}

.pdf-content-wrapper::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
