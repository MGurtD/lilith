<template>
  <Dialog
    :visible="visible"
    modal
    :closable="true"
    :style="{ width: '90vw' }"
    @update:visible="$emit('update:visible', $event)"
  >
    <template #header>
      <div class="w-full flex align-items-center justify-content-between pr-4">
        <div class="flex align-items-center gap-3">
          <div
            class="flex align-items-center justify-content-center bg-primary-100 border-circle p-2"
            style="width: 3rem; height: 3rem"
          >
            <i :class="PrimeIcons.COG" class="text-primary text-xl"></i>
          </div>
          <div class="flex flex-column">
            <span class="font-bold text-lg text-900">Gestió de fases</span>
            <span class="text-sm text-500">Carregar o crear una nova fase</span>
          </div>
        </div>
        <div class="flex gap-4 flex-wrap">
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold">Ordre</span>
            <span class="font-medium text-900 text-lg">{{
              workOrderCode
            }}</span>
          </div>
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold"
              >Referència</span
            >
            <span class="font-medium text-900 text-lg">{{
              referenceCode
            }}</span>
          </div>
          <div class="flex flex-column align-items-end">
            <span class="text-xs text-500 uppercase font-semibold"
              >Quantitat</span
            >
            <span class="font-medium text-900 text-lg">{{ quantity }}</span>
          </div>
        </div>
      </div>
    </template>

    <Tabs
      v-model:value="activeTab"
      class="loader-tabs"
      @update:value="onTabChange"
    >
      <TabList>
        <Tab value="load">
          <i :class="PrimeIcons.COG" class="mr-2" />
          Carregar fase
        </Tab>
        <Tab value="create">
          <i :class="PrimeIcons.COPY" class="mr-2" />
          Nova des de plantilla
        </Tab>
      </TabList>

      <TabPanels>
        <!-- ── TAB 1: CARREGAR FASE ──────────────────────────────────── -->
        <TabPanel value="load">
          <div class="tab-content">
            <DataTable
              :value="phases"
              :loading="loading"
              responsiveLayout="scroll"
              stripedRows
              :rowHover="true"
              class="p-datatable-sm clickable-rows"
              selectionMode="single"
              v-model:selection="selectedPhaseRow"
              sortField="phaseCode"
              :sortOrder="1"
              :isRowSelectable="
                (row: any) =>
                  row.data.workcenterTypeId === props.workcenterTypeId
              "
              :rowClass="getRowClass"
              @row-click="handleRowClick"
            >
              <Column header="" style="width: 2.5rem; text-align: center">
                <template #body="slotProps">
                  <i
                    v-if="slotProps.data.workcenterTypeId === workcenterTypeId"
                    :class="PrimeIcons.CHECK_CIRCLE"
                    class="phase-compatible-icon"
                    title="Compatible amb aquesta màquina"
                  />
                  <i
                    v-else
                    :class="PrimeIcons.LOCK"
                    class="phase-incompatible-icon"
                    title="No compatible amb aquesta màquina"
                  />
                </template>
              </Column>
              <Column
                field="phaseCode"
                header="Codi"
                :sortable="true"
                style="max-width: 50px"
              />
              <Column
                field="phaseDescription"
                header="Descripció"
                style="min-width: 200px"
              />
              <Column header="Estat" style="min-width: 150px">
                <template #body="slotProps">
                  <Tag
                    :value="slotProps.data.phaseStatus"
                    severity="info"
                    rounded
                  />
                </template>
              </Column>
              <Column header="Inici" style="min-width: 150px">
                <template #body="slotProps">
                  <span v-if="slotProps.data.startTime">
                    {{ formatDateTime(slotProps.data.startTime) }}
                  </span>
                </template>
              </Column>
              <Column header="Fi" style="min-width: 150px">
                <template #body="slotProps">
                  <span v-if="slotProps.data.endTime">
                    {{ formatDateTime(slotProps.data.endTime) }}
                  </span>
                </template>
              </Column>
              <Column
                field="preferredWorkcenterName"
                header="Màquina Preferida"
                style="min-width: 180px"
              />
              <Column header="Quant.">
                <template #body="slotProps">
                  <span class="quantity-ok">{{
                    slotProps.data.quantityOk
                  }}</span>
                  /
                  <span class="quantity-ko">{{
                    slotProps.data.quantityKo
                  }}</span>
                </template>
              </Column>
              <template #empty>
                <div class="no-data">
                  <i :class="PrimeIcons.INBOX" style="font-size: 2rem"></i>
                  <p>No s'han trobat fases per aquesta ordre de fabricació</p>
                </div>
              </template>
            </DataTable>

            <InfoPanel
              v-if="hasLoadedWorkOrders && phases.length > 0"
              severity="warn"
              text="No es pot carregar una nova ordre mentre hi hagi fases en procés a la màquina. Finalitza les fases carregades abans de carregar-ne una de nova."
            />
            <InfoPanel
              v-else-if="phases.length === 0"
              severity="warn"
              text="No hi ha fases disponibles per carregar en aquest centre de treball"
            />

            <div class="bottom-panel" v-if="!hasLoadedWorkOrders">
              <div class="panel-content">
                <div class="dropdown-container">
                  <label class="dropdown-label">Activitat a carregar</label>
                  <SelectWorkOrderPhaseDetail
                    v-model="selectedDetailId"
                    :details="selectedPhase?.details || []"
                    class="activity-dropdown"
                  />
                </div>
                <Button
                  :icon="PrimeIcons.COG"
                  label="Carregar"
                  severity="success"
                  :disabled="!selectedDetailId"
                  @click="onLoadActivity"
                  class="action-button"
                />
              </div>
            </div>
          </div>
        </TabPanel>

        <!-- ── TAB 2: NOVA FASE DES DE PLANTILLA ────────────────────── -->
        <TabPanel value="create">
          <div class="tab-content">
            <PhaseTemplateLoader
              ref="phaseTemplateLoaderRef"
              :workOrderId="workOrderId"
              :workcenterTypeId="workcenterTypeId"
              :preferredWorkcenterId="workcenterId"
              @phase-created="onPhaseCreated"
            />
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { WorkOrderPhaseDetailed } from "../../../production/types";
import { WorkOrderPhaseService } from "../../../production/services/workorder.service";
import { useToast } from "primevue/usetoast";
import { formatDateTime } from "../../../../utils/functions";
import { usePlantWorkcenterStore } from "../../store/workcenter.store";
import SelectWorkOrderPhaseDetail from "./SelectWorkOrderPhaseDetail.vue";
import PhaseTemplateLoader from "./PhaseTemplateLoader.vue";

interface Props {
  visible: boolean;
  workOrderId: string;
  workOrderCode: string;
  referenceCode: string;
  quantity: number;
  workcenterTypeId: string;
  workcenterId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (
    e: "phase-detail-selected",
    data: {
      workOrderId: string;
      workOrderPhaseId: string;
      machineStatusId: string;
    },
  ): void;
  (e: "phase-created"): void;
}>();

const toast = useToast();
const phaseService = new WorkOrderPhaseService("WorkOrderPhase");
const workcenterStore = usePlantWorkcenterStore();

const phases = ref<WorkOrderPhaseDetailed[]>([]);
const loading = ref(false);
const selectedDetailId = ref<string>("");
const selectedPhaseId = ref<string>("");
const selectedPhaseRow = ref<WorkOrderPhaseDetailed | undefined>(undefined);
const activeTab = ref<"load" | "create">("load");
const phaseTemplateLoaderRef = ref<InstanceType<
  typeof PhaseTemplateLoader
> | null>(null);

const hasLoadedWorkOrders = computed(() => {
  return workcenterStore.loadedWorkOrdersPhases.length > 0;
});

const autoSelectedPhase = computed(() => {
  const validPhases = phases.value
    .filter(
      (phase) =>
        !phase.endTime && phase.workcenterTypeId === props.workcenterTypeId,
    )
    .sort((a, b) => a.phaseCode.localeCompare(b.phaseCode));
  return validPhases[0];
});

const selectedPhase = computed(() => {
  if (selectedPhaseId.value) {
    return phases.value.find((p) => p.phaseId === selectedPhaseId.value);
  }
  return autoSelectedPhase.value;
});

const selectPhase = (phase: WorkOrderPhaseDetailed) => {
  selectedPhaseId.value = phase.phaseId;
  selectedPhaseRow.value = phase;
  if (phase.details && phase.details.length > 0) {
    selectedDetailId.value = phase.details[0].machineStatusId || "";
  } else {
    selectedDetailId.value = "";
  }
};

const getRowClass = (data: WorkOrderPhaseDetailed) => {
  return data.workcenterTypeId === props.workcenterTypeId
    ? "phase-row-compatible"
    : "phase-row-incompatible";
};

const handleRowClick = (event: any) => {
  const phase = event.data as WorkOrderPhaseDetailed;
  if (phase.workcenterTypeId === props.workcenterTypeId) {
    selectPhase(phase);
  }
};

const onLoadActivity = () => {
  if (!selectedPhase.value || !selectedDetailId.value) {
    toast.add({
      severity: "warn",
      summary: "Selecciona una fase i una activitat",
      life: 4000,
    });
    return;
  }
  emit("phase-detail-selected", {
    workOrderId: props.workOrderId,
    workOrderPhaseId: selectedPhase.value.phaseId,
    machineStatusId: selectedDetailId.value,
  });
};

const loadPhases = async () => {
  if (!props.workOrderId) return;
  loading.value = true;
  selectedDetailId.value = "";
  selectedPhaseId.value = "";
  selectedPhaseRow.value = undefined;
  try {
    const result = await phaseService.GetWorkOrderPhasesDetailed(
      props.workOrderId,
    );
    if (result) {
      phases.value = result;
      if (autoSelectedPhase.value) {
        selectPhase(autoSelectedPhase.value);
      }
    } else {
      phases.value = [];
    }
  } catch (error) {
    console.error("Error loading work order phases:", error);
    toast.add({
      severity: "error",
      summary: "Error al carregar les fases de l'ordre de fabricació",
      life: 4000,
    });
    phases.value = [];
  } finally {
    loading.value = false;
  }
};

const onPhaseCreated = async () => {
  await loadPhases();
  activeTab.value = "load";
  emit("phase-created");
};

// Reload the template tab content when the user switches to it
const onTabChange = (tab: string | number) => {
  if (tab === "create") {
    phaseTemplateLoaderRef.value?.load();
  }
};

watch(
  () => props.workOrderId,
  () => {
    if (props.visible && props.workOrderId) loadPhases();
  },
);

watch(
  () => props.visible,
  (newValue) => {
    if (newValue && props.workOrderId) {
      activeTab.value = "load";
      loadPhases();
    }
  },
);

onMounted(() => {
  if (props.visible && props.workOrderId) loadPhases();
});
</script>

<style scoped>
.loader-tabs {
  display: flex;
  flex-direction: column;
}

.tab-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding-top: 1rem;
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

.bottom-panel {
  background: var(--p-surface-50);
  border-top: 2px solid var(--p-surface-border);
  padding: 1.25rem 1.5rem;
  border-radius: 0 0 var(--border-radius) var(--border-radius);
}

.panel-content {
  display: flex;
  align-items: flex-end;
  gap: 1.5rem;
}

.dropdown-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.dropdown-label {
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--text-color);
}

.activity-dropdown {
  width: 100%;
}

.action-button {
  min-width: 150px;
  font-size: 1.05rem;
  padding: 0.75rem 1.5rem;
}

.action-button :deep(.p-button-icon) {
  font-size: 1.2rem;
}

:deep(.p-tabpanels) {
  padding: 0;
}

:deep(.p-tabpanel) {
  padding: 0;
}

:deep(.phase-row-incompatible) {
  opacity: 0.6;
  cursor: not-allowed !important;
  color: var(--text-color-secondary);
}

:deep(.phase-row-incompatible td) {
  pointer-events: none;
}

:deep(.phase-row-compatible) {
  cursor: pointer;
}

.phase-compatible-icon {
  color: var(--p-green-500);
  font-size: 1rem;
}

.phase-incompatible-icon {
  color: var(--p-surface-400);
  font-size: 0.9rem;
}
</style>
