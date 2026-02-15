<template>
  <Dialog
    v-model:visible="dialogVisible"
    :header="dialogTitle"
    :modal="true"
    :closable="true"
    :draggable="false"
    :style="{ width: '90vw', maxWidth: '600px' }"
    @hide="handleHide"
  >
    <div class="comment-editor">
      <div class="field">
        <label for="comment" class="block text-900 mb-2">Comentari</label>
        <Textarea
          id="comment"
          v-model="localComment"
          rows="5"
          class="w-full"
          :maxlength="maxLength"
          placeholder="Escriu un comentari sobre aquesta fase..."
        />
        <small class="character-count">
          {{ characterCount }} / {{ maxLength }} caràcters
        </small>
      </div>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <Button
          label="Cancel·lar"
          severity="secondary"
          :icon="PrimeIcons.TIMES"
          @click="handleCancel"
          :disabled="isSaving"
        />
        <Button
          label="Guardar"
          severity="success"
          :icon="PrimeIcons.CHECK"
          @click="handleSave"
          :loading="isSaving"
          :disabled="!hasChanges"
        />
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { usePlantWorkcenterStore } from "../../store/workcenter.store";

const props = defineProps<{
  visible: boolean;
  phaseId?: string;
  phaseCode?: string;
  initialComment?: string;
}>();

const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "comment-saved"): void;
}>();

const toast = useToast();
const workcenterStore = usePlantWorkcenterStore();

const maxLength = 1000;
const localComment = ref("");
const isSaving = ref(false);

// Sync visibility with parent
const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit("update:visible", value),
});

// Dialog title
const dialogTitle = computed(() => {
  if (props.phaseCode) {
    return `Editar Comentari - Fase ${props.phaseCode}`;
  }
  return "Editar Comentari";
});

// Character count
const characterCount = computed(() => localComment.value.length);

// Check if there are changes
const hasChanges = computed(() => {
  const current = localComment.value.trim();
  const initial = (props.initialComment || "").trim();
  return current !== initial;
});

// Watch for prop changes to reset local state
watch(
  () => props.visible,
  (newVisible) => {
    if (newVisible) {
      localComment.value = props.initialComment || "";
    }
  },
);

// Handle cancel
const handleCancel = () => {
  dialogVisible.value = false;
};

// Handle hide (dialog closed by X or overlay)
const handleHide = () => {
  localComment.value = "";
  isSaving.value = false;
};

// Handle save
const handleSave = async () => {
  if (!props.phaseId) {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "No s'ha pogut identificar la fase",
      life: 3000,
    });
    return;
  }

  if (!hasChanges.value) {
    dialogVisible.value = false;
    return;
  }

  isSaving.value = true;

  try {
    const success = await workcenterStore.updatePhaseComment(
      props.phaseId,
      localComment.value.trim(),
    );

    if (success) {
      toast.add({
        severity: "success",
        summary: "Comentari actualitzat",
        detail: "El comentari s'ha guardat correctament",
        life: 3000,
      });

      emit("comment-saved");
      dialogVisible.value = false;
    } else {
      toast.add({
        severity: "error",
        summary: "Error",
        detail: "No s'ha pogut guardar el comentari",
        life: 5000,
      });
    }
  } catch (error) {
    console.error("Error saving comment:", error);
    toast.add({
      severity: "error",
      summary: "Error",
      detail:
        error instanceof Error
          ? `Error al guardar el comentari: ${error.message}`
          : "Error al guardar el comentari",
      life: 5000,
    });
  } finally {
    isSaving.value = false;
  }
};
</script>

<style scoped lang="scss">
.comment-editor {
  padding: 0.5rem 0;

  .field {
    margin-bottom: 0;
  }

  .character-count {
    display: block;
    text-align: right;
    margin-top: 0.5rem;
    color: var(--text-color-secondary);
    font-size: 0.875rem;
  }
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}

:deep(.p-dialog-content) {
  padding-bottom: 1rem;
}
</style>
