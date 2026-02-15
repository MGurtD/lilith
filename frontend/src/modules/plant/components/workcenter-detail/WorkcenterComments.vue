<template>
  <div class="workcenter-comments">
    <!-- Empty state when no comments exist -->
    <div v-if="!hasAnyComments" class="empty-state">
      <i :class="PrimeIcons.COMMENTS" class="empty-icon"></i>
      <p class="empty-text">Sense comentaris</p>
      <span class="empty-subtext">
        No hi ha comentaris a l'ordre de treball, fase o detalls de fase.
      </span>
    </div>

    <!-- Comments content when at least one comment exists -->
    <div v-else class="comments-content">
      <!-- Work Order Level Comment -->
      <Card v-if="workOrderComment" class="comment-card work-order-card">
        <template #title>
          <div class="card-header">
            <i :class="PrimeIcons.BOX" class="header-icon"></i>
            <span>Ordre de Treball</span>
            <Tag severity="info" :value="workOrderCode" class="code-tag" />
          </div>
        </template>
        <template #content>
          <p class="comment-text">{{ workOrderComment }}</p>
        </template>
      </Card>

      <!-- Phase Level Comment -->
      <Card v-if="phaseComment || currentPhase" class="comment-card phase-card">
        <template #title>
          <div class="card-header">
            <i :class="PrimeIcons.COG" class="header-icon"></i>
            <span>Fase</span>
            <Tag severity="success" :value="phaseDisplay" class="code-tag" />
            <Button
              v-if="currentPhase"
              :icon="PrimeIcons.PENCIL"
              label="Editar"
              size="small"
              text
              class="edit-button"
              @click="openEditDialog"
            />
          </div>
        </template>
        <template #content>
          <p v-if="phaseComment" class="comment-text">{{ phaseComment }}</p>
          <p v-else class="empty-comment-text">
            Aquesta fase no té cap comentari. Fes clic a "Editar" per afegir-ne un.
          </p>
        </template>
      </Card>

      <!-- Phase Details Comments -->
      <Card v-if="detailComments.length > 0" class="comment-card details-card">
        <template #title>
          <div class="card-header">
            <i :class="PrimeIcons.LIST" class="header-icon"></i>
            <span>Detalls de Fase</span>
            <Badge :value="detailComments.length" severity="secondary" />
          </div>
        </template>
        <template #content>
          <div class="detail-comments-list">
            <div
              v-for="(detail, index) in detailComments"
              :key="index"
              class="detail-comment-item"
            >
              <div class="detail-header">
                <Tag
                  :value="detail.machineStatusName"
                  :style="{
                    backgroundColor: normalizeColor(detail.machineStatusColor),
                    color: getContrastColor(detail.machineStatusColor),
                  }"
                  class="status-tag"
                />
              </div>
              <p class="comment-text">{{ detail.comment }}</p>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- Comment Editor Dialog -->
    <WorkcenterCommentEditor
      v-model:visible="showEditDialog"
      :phaseId="currentPhase?.phaseId"
      :phaseCode="currentPhase?.phaseCode"
      :initialComment="phaseComment"
      @comment-saved="handleCommentSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import type { WorkOrderWithPhases } from "../../../production/types";
import { normalizeColor, isColorLight } from "@/utils/functions";
import WorkcenterCommentEditor from "./WorkcenterCommentEditor.vue";

const props = defineProps<{
  loadedWorkOrders: WorkOrderWithPhases[];
}>();

// Dialog state
const showEditDialog = ref(false);

// Get the first loaded work order (if any)
const currentWorkOrder = computed(() => props.loadedWorkOrders?.[0]);

// Get the first phase of the work order (if any)
const currentPhase = computed(() => currentWorkOrder.value?.phases?.[0]);

// Work order comment
const workOrderComment = computed(
  () => currentWorkOrder.value?.comment?.trim() || "",
);

// Work order code for display
const workOrderCode = computed(
  () => currentWorkOrder.value?.workOrderCode || "",
);

// Phase comment
const phaseComment = computed(() => currentPhase.value?.comment?.trim() || "");

// Phase display (code + description)
const phaseDisplay = computed(() => currentPhase.value?.phaseDisplay || "");

// Detail comments (only those with non-empty comments)
const detailComments = computed(() => {
  if (!currentPhase.value?.details) return [];
  return currentPhase.value.details.filter(
    (d) => d.comment && d.comment.trim() !== "",
  );
});

// Check if there are any comments at any level (or if phase exists for editing)
const hasAnyComments = computed(() => {
  return (
    workOrderComment.value !== "" ||
    phaseComment.value !== "" ||
    detailComments.value.length > 0 ||
    !!currentPhase.value // Show phase card even if empty so edit button is visible
  );
});

// Open edit dialog
const openEditDialog = () => {
  showEditDialog.value = true;
};

// Handle comment saved
const handleCommentSaved = () => {
  // Dialog will close automatically
  // Store refresh is handled in the dialog component via store action
};

// Utility for contrast color
const getContrastColor = (hexColor: string): string => {
  const normalized = normalizeColor(hexColor);
  return isColorLight(normalized) ? "#000000" : "#ffffff";
};
</script>

<style scoped lang="scss">
.workcenter-comments {
  height: 100%;
  display: flex;
  flex-direction: column;
  padding: 1rem;
  overflow-y: auto;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 200px;
  text-align: center;
  color: var(--text-color-secondary);

  .empty-icon {
    font-size: 3rem;
    margin-bottom: 1rem;
    opacity: 0.5;
  }

  .empty-text {
    font-size: 1.25rem;
    font-weight: 600;
    margin: 0 0 0.5rem 0;
    color: var(--text-color);
  }

  .empty-subtext {
    font-size: 0.9rem;
    max-width: 300px;
  }
}

/* Comments Content */
.comments-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.comment-card {
  :deep(.p-card-title) {
    font-size: 1rem;
    padding-bottom: 0.5rem;
    border-bottom: 1px solid var(--surface-border);
    margin-bottom: 0.75rem;
  }

  :deep(.p-card-body) {
    padding: 1rem;
  }

  :deep(.p-card-content) {
    padding: 0;
  }
}

.card-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;

  .header-icon {
    font-size: 1.1rem;
    color: var(--primary-color);
  }

  .code-tag {
    margin-left: auto;
  }

  .edit-button {
    margin-left: 0.5rem;
  }
}

.comment-text {
  margin: 0;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
}

.empty-comment-text {
  margin: 0;
  line-height: 1.6;
  color: var(--text-color-secondary);
  font-style: italic;
}

/* Work Order Card */
.work-order-card {
  :deep(.p-card) {
    border-left: 4px solid var(--blue-500);
  }
}

/* Phase Card */
.phase-card {
  :deep(.p-card) {
    border-left: 4px solid var(--green-500);
  }
}

/* Details Card */
.details-card {
  :deep(.p-card) {
    border-left: 4px solid var(--orange-500);
  }
}

.detail-comments-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.detail-comment-item {
  padding: 0.75rem;
  background-color: var(--surface-ground);
  border-radius: var(--border-radius);

  .detail-header {
    margin-bottom: 0.5rem;
  }

  .status-tag {
    font-size: 0.8rem;
  }

  .comment-text {
    font-size: 0.95rem;
  }
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .workcenter-comments {
    padding: 0.75rem;
  }

  .card-header {
    .code-tag {
      margin-left: 0;
      margin-top: 0.25rem;
    }
  }
}
</style>
