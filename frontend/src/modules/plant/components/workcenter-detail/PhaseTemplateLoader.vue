<template>
  <Dialog
    :visible="visible"
    modal
    :closable="true"
    :style="{ width: '70vw' }"
    @update:visible="$emit('update:visible', $event)"
  >
    <template #header>
      <div class="w-full flex align-items-center gap-3">
        <div
          class="flex align-items-center justify-content-center bg-primary-100 border-circle p-2"
          style="width: 3rem; height: 3rem"
        >
          <i :class="PrimeIcons.COPY" class="text-primary text-xl"></i>
        </div>
        <div class="flex flex-column">
          <span class="font-bold text-lg text-900"
            >Crear fase des de plantilla</span
          >
          <span class="text-sm text-500"
            >Selecciona una plantilla i defineix el codi i la descripció de la
            nova fase</span
          >
        </div>
      </div>
    </template>

    <div class="dialog-content">
      <!-- Template selection table -->
      <DataTable
        :value="templates"
        :loading="loading"
        responsiveLayout="scroll"
        stripedRows
        :rowHover="true"
        class="p-datatable-sm clickable-rows"
        selectionMode="single"
        v-model:selection="selectedTemplate"
        sortField="name"
        :sortOrder="1"
      >
        <Column
          field="name"
          header="Nom"
          :sortable="true"
          style="width: 30%"
        />
        <Column field="description" header="Descripció" style="width: 50%" />
        <Column header="Detalls" style="width: 20%">
          <template #body="slotProps">
            <Tag
              :value="`${slotProps.data.details?.length || 0} activitats`"
              severity="info"
              rounded
            />
          </template>
        </Column>
        <template #empty>
          <div class="no-data">
            <i :class="PrimeIcons.INBOX" style="font-size: 2rem"></i>
            <p>No s'han trobat plantilles de fase actives</p>
          </div>
        </template>
      </DataTable>

      <!-- Template details preview -->
      <div v-if="selectedTemplate" class="template-preview">
        <span class="font-semibold text-sm text-500 uppercase"
          >Activitats de la plantilla</span
        >
        <DataTable
          :value="selectedTemplate.details"
          class="p-datatable-sm"
          responsiveLayout="scroll"
          stripedRows
          sortField="order"
          :sortOrder="1"
        >
          <Column field="order" header="Ordre" style="width: 15%" />
          <Column header="Estat de màquina" style="width: 45%">
            <template #body="slotProps">
              {{ getMachineStatusName(slotProps.data.machineStatusId) }}
            </template>
          </Column>
          <Column field="comment" header="Comentari" style="width: 40%" />
        </DataTable>
      </div>

      <!-- Form inputs for code, description and workcenter -->
      <div v-if="selectedTemplate" class="form-section">
        <div class="form-fields">
          <div class="form-field">
            <label class="form-label">Codi de la fase</label>
            <BaseInput v-model="phaseCode" class="w-full" />
          </div>
          <div class="form-field">
            <label class="form-label">Descripció de la fase</label>
            <BaseInput v-model="phaseDescription" class="w-full" />
          </div>
          <div class="form-field">
            <DropdownWorkcenter
              label="Centre de treball"
              v-model="selectedWorkcenterId"
            />
          </div>
        </div>
      </div>

      <!-- Action button -->
      <div v-if="selectedTemplate" class="action-section">
        <Button
          :icon="PrimeIcons.PLUS"
          label="Crear fase"
          severity="success"
          :loading="creating"
          :disabled="!phaseCode.trim()"
          @click="onCreatePhase"
          class="create-button"
        />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import {
  PhaseTemplate,
  CreatePhaseFromTemplateDto,
} from "../../../production/types";
import { usePlantDataStore } from "../../store";
import { usePlantModelStore } from "../../../production/store/plantmodel";
import Services from "../../../production/services";
import { WorkOrderPhaseService } from "../../../production/services/workorder.service";
import DropdownWorkcenter from "../../../production/components/DropdownWorkcenter.vue";

interface Props {
  visible: boolean;
  workOrderId: string;
  workcenterTypeId: string;
  preferredWorkcenterId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (e: "phase-created"): void;
}>();

const toast = useToast();
const dataStore = usePlantDataStore();
const plantModelStore = usePlantModelStore();
const phaseService = new WorkOrderPhaseService("WorkOrderPhase");

const templates = ref<PhaseTemplate[]>([]);
const selectedTemplate = ref<PhaseTemplate | undefined>(undefined);
const loading = ref(false);
const creating = ref(false);
const phaseCode = ref("");
const phaseDescription = ref("");
const selectedWorkcenterId = ref<string>(props.preferredWorkcenterId);

const selectedWorkcenterTypeId = computed(() => {
  if (!selectedWorkcenterId.value || !plantModelStore.workcenters) {
    return props.workcenterTypeId;
  }
  const wc = plantModelStore.workcenters.find(
    (w) => w.id === selectedWorkcenterId.value,
  );
  return wc ? wc.workcenterTypeId : props.workcenterTypeId;
});

const getMachineStatusName = (machineStatusId: string): string => {
  const status = dataStore.machineStatuses.find(
    (s) => s.id === machineStatusId,
  );
  return status?.name || machineStatusId;
};

const loadTemplates = async () => {
  loading.value = true;
  selectedTemplate.value = undefined;
  phaseCode.value = "";
  phaseDescription.value = "";
  selectedWorkcenterId.value = props.preferredWorkcenterId;
  try {
    const [templateResult] = await Promise.all([
      Services.PhaseTemplate.getAll(),
      !plantModelStore.workcenters || plantModelStore.workcenters.length === 0
        ? plantModelStore.fetchActiveWorkcenters()
        : Promise.resolve(),
    ]);
    if (templateResult) {
      // Only show non-disabled templates
      templates.value = templateResult.filter((t) => !t.disabled);
      // Auto-select first template
      selectedTemplate.value = templates.value[0] ?? undefined;
    } else {
      templates.value = [];
    }
  } catch (error) {
    console.error("Error loading phase templates:", error);
    toast.add({
      severity: "error",
      summary: "Error al carregar les plantilles de fase",
      life: 4000,
    });
    templates.value = [];
  } finally {
    loading.value = false;
  }
};

const onCreatePhase = async () => {
  if (!selectedTemplate.value || !phaseCode.value.trim()) return;

  if (!/^\d+$/.test(phaseCode.value.trim())) {
    toast.add({
      severity: "warn",
      summary: "El codi de la fase ha de ser numèric",
      life: 4000,
    });
    return;
  }

  creating.value = true;
  try {
    const dto: CreatePhaseFromTemplateDto = {
      phaseTemplateId: selectedTemplate.value.id,
      workOrderId: props.workOrderId,
      workcenterTypeId: selectedWorkcenterTypeId.value,
      preferredWorkcenterId: selectedWorkcenterId.value,
      code: phaseCode.value.trim(),
      description: phaseDescription.value.trim(),
    };

    const response = await phaseService.CreateFromTemplate(dto);
    if (response.result) {
      toast.add({
        severity: "success",
        summary: "Fase creada correctament des de la plantilla",
        life: 4000,
      });
      emit("phase-created");
      emit("update:visible", false);
    } else {
      toast.add({
        severity: "error",
        summary: response.errors?.join("\n") || "Error al crear la fase",
        life: 4000,
      });
    }
  } catch (error) {
    console.error("Error creating phase from template:", error);
    toast.add({
      severity: "error",
      summary: "Error al crear la fase des de la plantilla",
      life: 4000,
    });
  } finally {
    creating.value = false;
  }
};

watch(
  () => props.visible,
  (newValue) => {
    if (newValue) {
      loadTemplates();
    }
  },
);

onMounted(() => {
  if (props.visible) {
    loadTemplates();
  }
});
</script>

<style scoped>
.dialog-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.no-data {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 3rem 1rem;
  color: var(--text-color-secondary);
}

.no-data i {
  color: var(--text-color-secondary);
  opacity: 0.5;
}

.no-data p {
  margin: 0;
  font-size: 1rem;
}

.template-preview {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.75rem;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-border);
  border-radius: var(--border-radius);
}

.form-section {
  padding: 0.75rem;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-border);
  border-radius: var(--border-radius);
}

.form-fields {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
}

/* Tablet portrait: code+desc on first row, workcenter on second */
@media (max-width: 1024px) and (orientation: portrait) {
  .form-fields {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* Mobile: one field per row */
@media (max-width: 640px) {
  .form-fields {
    grid-template-columns: 1fr;
  }
}

.form-field {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--text-color);
}

.action-section {
  display: flex;
  justify-content: flex-end;
}

.create-button {
  min-width: 180px;
  font-size: 1.05rem;
  padding: 0.75rem 1.5rem;
}
</style>
