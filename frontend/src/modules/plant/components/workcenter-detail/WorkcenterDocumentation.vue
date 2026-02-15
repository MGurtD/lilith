<template>
  <div class="documentation-content">
    <!-- Mobile Header Bar (visible < 768px) -->
    <div class="mobile-header">
      <Button
        :icon="PrimeIcons.LIST"
        size="small"
        outlined
        @click="drawerVisible = true"
        v-tooltip.bottom="'Veure llista de documents'"
        class="drawer-toggle-btn"
        :badge="documentCount > 0 ? documentCount.toString() : undefined"
        badgeSeverity="info"
      />
      <div class="selected-document-info">
        <span v-if="selectedDocument" class="selected-name">
          {{ selectedDocument.originalName }}
        </span>
        <span v-else class="no-selection"> Selecciona un document </span>
      </div>
    </div>

    <div class="master-detail-layout">
      <!-- Master: Document List (visible >= 768px) -->
      <div class="documents-panel desktop-only">
        <DataTable
          :value="workcenterStore.workOrderReferenceDocuments"
          v-model:selection="selectedDocument"
          selectionMode="single"
          @row-select="onDocumentSelect"
          scrollable
          scrollHeight="flex"
          class="documents-table clickable-rows"
          :rowHover="true"
          stripedRows
        >
          <template #empty>
            <div class="no-data">
              No hi ha instruccions de treball disponibles
            </div>
          </template>
          <Column field="originalName" header="Nom">
            <template #body="slotProps">
              <div class="text-overflow-ellipsis">
                {{ slotProps.data.originalName }}
              </div>
            </template>
          </Column>
          <Column field="type" header="Tipus" style="width: 60px">
            <template #body="slotProps">
              <Tag :value="getFileType(slotProps.data)" severity="info" />
            </template>
          </Column>
        </DataTable>
      </div>

      <!-- Detail: File Viewer (supports PDF, images, and other formats) -->
      <div class="viewer-panel">
        <FileViewer :file="selectedDocument" />
      </div>
    </div>

    <!-- Mobile Drawer for Document List -->
    <Drawer
      v-model:visible="drawerVisible"
      position="left"
      :style="{ width: '280px' }"
      class="documents-drawer"
    >
      <template #header>
        <span class="drawer-header">Documents</span>
      </template>
      <DataTable
        :value="workcenterStore.workOrderReferenceDocuments"
        v-model:selection="selectedDocument"
        selectionMode="single"
        @row-select="onMobileDocumentSelect"
        scrollable
        scrollHeight="flex"
        class="drawer-documents-table clickable-rows"
        :rowHover="true"
        stripedRows
      >
        <template #empty>
          <div class="no-data">
            No hi ha instruccions de treball disponibles
          </div>
        </template>
        <Column field="originalName" header="Nom">
          <template #body="slotProps">
            <div class="drawer-document-item">
              <div class="document-name">
                {{ slotProps.data.originalName }}
              </div>
              <Tag
                :value="getFileType(slotProps.data)"
                severity="info"
                class="document-type-tag"
              />
            </div>
          </template>
        </Column>
      </DataTable>
    </Drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import Button from "primevue/button";
import Drawer from "primevue/drawer";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Tag from "primevue/tag";
import { File } from "../../../../types";
import FileViewer from "../../../../components/FileViewer.vue";
import { usePlantWorkcenterStore } from "../../store";

// Store
const workcenterStore = usePlantWorkcenterStore();

// State
const selectedDocument = ref<File | null>(null);
const drawerVisible = ref(false);

// Computed
const documentCount = computed(
  () => workcenterStore.workOrderReferenceDocuments?.length || 0,
);

// Document selection handler (desktop)
const onDocumentSelect = (event: any) => {
  selectedDocument.value = event.data;
};

// Document selection handler (mobile) - closes drawer after selection
const onMobileDocumentSelect = (event: any) => {
  selectedDocument.value = event.data;
  drawerVisible.value = false;
};

// Helper functions
const getFileType = (file: File): string => {
  if (!file?.originalName) return "Desconegut";
  const name = file.originalName.trim();
  const lastDot = name.lastIndexOf(".");
  if (lastDot === -1 || lastDot === name.length - 1) return "Sense extensió";
  const ext = name.substring(lastDot + 1);
  return ext.toUpperCase();
};

// Auto-select first document when documents are loaded
watch(
  () => workcenterStore.workOrderReferenceDocuments,
  (documents) => {
    if (documents && documents.length > 0 && !selectedDocument.value) {
      selectedDocument.value = documents[0];
    }
  },
  { immediate: true },
);

// Lifecycle
onMounted(() => {
  // Check if documents are already loaded and select first one
  if (
    workcenterStore.workOrderReferenceDocuments?.length > 0 &&
    !selectedDocument.value
  ) {
    selectedDocument.value = workcenterStore.workOrderReferenceDocuments[0];
  }
});
</script>

<style scoped>
.documentation-content {
  height: 100%;
  display: flex;
  flex-direction: column;
}

/* ===== MOBILE HEADER ===== */
.mobile-header {
  display: none;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border-bottom: 1px solid #e2e8f0;
  border-radius: 8px 8px 0 0;
}

.drawer-toggle-btn {
  flex-shrink: 0;
}

.selected-document-info {
  flex: 1;
  min-width: 0;
  overflow: hidden;
}

.selected-name {
  display: block;
  font-weight: 500;
  color: #1e293b;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.no-selection {
  color: #94a3b8;
  font-style: italic;
}

/* ===== MASTER-DETAIL LAYOUT ===== */
.master-detail-layout {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: 1.5rem;
  flex: 1;
  min-height: 0;
  overflow: hidden;
}

/* ===== DOCUMENTS PANEL (DESKTOP) ===== */
.documents-panel {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  min-width: 0;
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.documents-panel :deep(.p-datatable) {
  height: 100%;
}

.documents-panel :deep(.p-datatable-wrapper) {
  overflow-x: hidden;
  flex: 1;
}

.documents-panel :deep(.p-datatable-header-cell) {
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  font-weight: 600;
}

/* ===== VIEWER PANEL ===== */
.viewer-panel {
  min-height: 0;
  min-width: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  width: 100%;
}

/* ===== COMMON STYLES ===== */
.no-data {
  text-align: center;
  padding: 2rem;
  color: var(--text-color-secondary);
}

.text-overflow-ellipsis {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 100%;
}

/* ===== DRAWER STYLES ===== */
.documents-drawer :deep(.p-drawer-content) {
  padding: 0;
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}

.drawer-header {
  font-weight: 600;
  font-size: 1.125rem;
}

.drawer-documents-table {
  height: 100%;
}

.drawer-documents-table :deep(.p-datatable-wrapper) {
  flex: 1;
}

.drawer-document-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  padding: 0.25rem 0;
}

.document-name {
  font-weight: 500;
  color: #1e293b;
  word-break: break-word;
}

.document-type-tag {
  align-self: flex-start;
}

/* ===== CLICKABLE ROWS ===== */
.clickable-rows :deep(.p-datatable-tbody > tr) {
  cursor: pointer;
}

.clickable-rows :deep(.p-datatable-tbody > tr:hover) {
  background: var(--p-primary-50) !important;
}

.clickable-rows :deep(.p-datatable-tbody > tr.p-highlight) {
  background: var(--p-primary-100) !important;
}

/* ===== RESPONSIVE: TABLET (768px - 991px) ===== */
@media (max-width: 991px) and (min-width: 768px) {
  .master-detail-layout {
    grid-template-columns: 200px 1fr;
    gap: 1rem;
  }

  .documents-panel {
    max-width: 200px;
  }

  /* Hide type column on tablet to save space */
  .documents-panel :deep(.p-datatable-thead th:nth-child(2)),
  .documents-panel :deep(.p-datatable-tbody td:nth-child(2)) {
    display: none;
  }
}

/* ===== RESPONSIVE: MOBILE (< 768px) ===== */
@media (max-width: 767px) {
  .mobile-header {
    display: flex;
  }

  .desktop-only {
    display: none !important;
  }

  .master-detail-layout {
    grid-template-columns: 1fr;
  }

  .viewer-panel {
    min-height: 500px;
    border-radius: 0 0 8px 8px;
  }
}

/* ===== RESPONSIVE: SMALL MOBILE (< 480px) ===== */
@media (max-width: 479px) {
  .mobile-header {
    padding: 0.5rem;
  }

  .viewer-panel {
    min-height: 400px;
  }
}
</style>
