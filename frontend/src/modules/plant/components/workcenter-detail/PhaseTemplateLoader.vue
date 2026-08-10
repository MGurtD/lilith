<template>
  <div class="loader-body">
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
      <Column field="name" :header='$t("plant.nom")' :sortable="true" style="width: 30%" />
      <Column field="description" :header='$t("plant.descripcio")' style="width: 50%" />
      <Column :header='$t("plant.detalls")' style="width: 20%">
        <template #body="slotProps">
          <Tag
            :value="t('plant.messages.activityCount', { count: slotProps.data.details?.length || 0 })"
            severity="info"
            rounded
          />
        </template>
      </Column>
      <template #empty>
        <div class="no-data">
          <i :class="PrimeIcons.INBOX" style="font-size: 2rem"></i>
          <p>{{ $t("plant.no-s-han-trobat-plantilles-de-fase-actives") }}</p>
        </div>
      </template>
    </DataTable>

    <!-- Template details preview -->
    <div v-if="selectedTemplate" class="template-preview">
      <span class="font-semibold text-sm text-500 uppercase">{{ $t("plant.activitats-de-la-plantilla") }}</span>
      <DataTable
        :value="selectedTemplate.details"
        class="p-datatable-sm"
        responsiveLayout="scroll"
        stripedRows
        sortField="order"
        :sortOrder="1"
      >
        <Column field="order" :header='$t("plant.ordre")' style="width: 15%" />
        <Column :header='$t("plant.estat-de-maquina")' style="width: 45%">
          <template #body="slotProps">
            {{ getMachineStatusName(slotProps.data.machineStatusId) }}
          </template>
        </Column>
        <Column field="comment" :header='$t("plant.comentari")' style="width: 40%" />
      </DataTable>
    </div>

    <!-- Form inputs -->
    <div v-if="selectedTemplate" class="form-section">
      <div class="form-fields">
        <div class="form-field">
          <label class="form-label">{{ $t("plant.codi-de-la-fase") }}</label>
          <BaseInput v-model="phaseCode" class="w-full" />
        </div>
        <div class="form-field">
          <label class="form-label">{{ $t("plant.descripcio-de-la-fase") }}</label>
          <BaseInput v-model="phaseDescription" class="w-full" />
        </div>
        <div class="form-field">
          <DropdownWorkcenter :label='$t("plant.centre-de-treball")' v-model="selectedWorkcenterId" />
        </div>
      </div>
    </div>

    <!-- Action button -->
    <div v-if="selectedTemplate" class="action-section">
      <Button
        :icon="PrimeIcons.PLUS"
        :label='$t("plant.crear-fase")'
        severity="success"
        :loading="creating"
        :disabled="!phaseCode.trim() || !phaseDescription.trim()"
        @click="onCreatePhase"
        class="create-button"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, computed, onMounted } from "vue";
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

const { t } = useI18n();

interface Props {
  workOrderId: string;
  workcenterTypeId: string;
  preferredWorkcenterId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
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
  const status = dataStore.machineStatuses.find((s) => s.id === machineStatusId);
  return status?.name || machineStatusId;
};

const load = async () => {
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
      templates.value = templateResult.filter((t) => !t.disabled);
      selectedTemplate.value = templates.value[0] ?? undefined;
    } else {
      templates.value = [];
    }
  } catch (error) {
    console.error("Error loading phase templates:", error);
    toast.add({
      severity: "error",
      summary: t("plant.error-al-carregar-les-plantilles-de-fase"),
      life: 4000,
    });
    templates.value = [];
  } finally {
    loading.value = false;
  }
};

const onCreatePhase = async () => {
  if (!selectedTemplate.value || !phaseCode.value.trim() || !phaseDescription.value.trim()) return;

  if (!/^\d+$/.test(phaseCode.value.trim())) {
    toast.add({
      severity: "warn",
      summary: t("plant.el-codi-de-la-fase-ha-de-ser-numeric"),
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
        summary: t("plant.fase-creada-correctament-des-de-la-plantilla"),
        life: 4000,
      });
      phaseCode.value = "";
      phaseDescription.value = "";
      emit("phase-created");
    } else {
      toast.add({
        severity: "error",
        summary: response.errors?.join("\n") || t("plant.messages.phaseCreationError"),
        life: 4000,
      });
    }
  } catch (error) {
    console.error("Error creating phase from template:", error);
    toast.add({
      severity: "error",
      summary: t("plant.error-al-crear-la-fase-des-de-la-plantilla"),
      life: 4000,
    });
  } finally {
    creating.value = false;
  }
};

// Expose reload so WorkOrderLoader can call it when the tab is activated
defineExpose({ load });

onMounted(() => load());
</script>

<style scoped>
.loader-body {
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

@media (max-width: 1024px) and (orientation: portrait) {
  .form-fields {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .form-fields {
    grid-template-columns: 1fr;
  }
}

.form-field {
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
