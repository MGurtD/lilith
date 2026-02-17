<template>
  <div class="workcenter-detail">
    <!-- Main content -->
    <main class="workcenter-main" v-if="workcenter">
      <div class="content-layout">
        <!-- Left panel - Realtime data -->
        <aside class="realtime-panel">
          <WorkcenterRealtimePanel :workcenter="workcenter" />
        </aside>

        <!-- Right panel - Tabs -->
        <section class="tabs-panel">
          <Tabs v-model:value="activeTab">
            <TabList>
              <Tab value="0">
                <div class="flex align-items-center gap-2">
                  <i :class="PrimeIcons.CALENDAR"></i>
                  <span class="font-bold">Fases disponibles</span>
                </div>
              </Tab>
              <Tab v-if="hasLoadedPhase" value="1">
                <div class="flex align-items-center gap-2">
                  <i :class="PrimeIcons.FILE"></i>
                  <span class="font-bold">Documentació</span>
                </div>
              </Tab>
              <Tab v-if="hasLoadedPhase" value="2">
                <div class="flex align-items-center gap-2">
                  <i :class="PrimeIcons.COMMENTS"></i>
                  <span class="font-bold">Comentaris</span>
                </div>
              </Tab>
            </TabList>
            <TabPanels>
              <!-- Available Phases Tab -->
              <TabPanel value="0">
                <WorkcenterWorkOrderSelector
                  :workcenterTypeId="workcenter.config.workcenterTypeId"
                  @workorder-selected="handleWorkOrderSelected"
                />
              </TabPanel>

              <!-- Documentation Tab -->
              <TabPanel v-if="hasLoadedPhase" value="1">
                <WorkcenterDocumentation :workcenter="workcenter" />
              </TabPanel>

              <!-- Comments Tab -->
              <TabPanel v-if="hasLoadedPhase" value="2">
                <WorkcenterComments
                  :loadedWorkOrders="workcenterStore.loadedWorkOrdersPhases"
                />
              </TabPanel>
            </TabPanels>
          </Tabs>
        </section>
      </div>
    </main>

    <!-- Bottom panel - Touch device buttons -->
    <footer class="touch-panel">
      <!-- Operator Group -->
      <div class="action-group operator-group">
        <span class="group-label">Operari</span>
        <div class="group-buttons">
          <Button
            v-if="!isOperatorClockedIn"
            :icon="PrimeIcons.SIGN_IN"
            label="Entrar"
            severity="primary"
            class="touch-button"
            @click="handleOperatorClockIn"
          />
          <Button
            v-else
            :icon="PrimeIcons.SIGN_OUT"
            label="Sortir"
            severity="primary"
            class="touch-button"
            @click="handleOperatorClockOut"
          />
        </div>
      </div>

      <!-- Machine Status Group (always visible, dynamic buttons only when phase loaded) -->
      <div class="action-group activity-group">
        <span class="group-label">Estat màquina</span>
        <div class="group-buttons">
          <!-- Dynamic activity buttons from phase details (only when phase loaded) -->
          <template v-if="hasLoadedPhase">
            <Button
              v-for="detail in phaseActivityButtons"
              :key="detail.machineStatusId"
              :label="detail.machineStatusName"
              :icon="detail.machineStatusIcon"
              :style="{
                '--btn-bg': normalizeColor(detail.machineStatusColor),
                '--btn-color': getContrastColor(detail.machineStatusColor),
              }"
              class="touch-button activity-button"
              @click="handleActivityChange(detail.machineStatusId!)"
            />
          </template>
          <!-- Close machine button (Parada) - always visible, disabled when machine is closed -->
          <Button
            v-if="closedStatus"
            :icon="closedStatus.icon"
            :label="closedStatus.name"
            :disabled="isMachineClosed"
            :style="{
              '--btn-bg': normalizeColor(closedStatus.color),
              '--btn-color': getContrastColor(closedStatus.color),
            }"
            class="touch-button activity-button"
            @click="handleCloseMachine"
          />
          <!-- Other statuses - always visible -->
          <Button
            :icon="PrimeIcons.PLUS"
            label="Altres"
            severity="primary"
            class="touch-button"
            @click="handleMachineStatusChange"
          />
        </div>
      </div>

      <!-- Phase Group (only if phase is loaded) -->
      <div class="action-group phase-group">
        <span class="group-label">Fase</span>
        <div class="group-buttons">
          <Button
            :icon="PrimeIcons.PLUS"
            label="Afegir qtt."
            severity="primary"
            class="touch-button"
            @click="phaseQuantitiesVisible = true"
            :disabled="!hasLoadedPhase"
          />
          <Button
            :icon="PrimeIcons.CHECK_CIRCLE"
            label="Finalitzar"
            severity="primary"
            class="touch-button"
            @click="handleWorkOrderPhaseClose"
            :disabled="!hasLoadedPhase"
          />
        </div>
      </div>
    </footer>

    <!-- Machine Status Selector Dialog -->
    <MachineStatusSelector
      v-model:visible="statusSelectorVisible"
      :statuses="dataStore.machineStatuses"
      :excludeIds="excludeStatusIds"
      @status-changed="onStatusChanged"
    />

    <!-- Work Order Loader Dialog -->
    <WorkOrderLoader
      v-model:visible="workOrderLoaderVisible"
      :workOrderId="selectedWorkOrderData.workOrderId"
      :workOrderCode="selectedWorkOrderData.workOrderCode"
      :referenceCode="selectedWorkOrderData.referenceCode"
      :quantity="selectedWorkOrderData.quantity"
      :workcenterTypeId="workcenter?.config.workcenterTypeId || ''"
      @phase-detail-selected="handlePhaseDetailSelected"
    />

    <!-- Work Order Unloader Dialog -->
    <WorkOrderUnloader
      v-model:visible="workOrderUnloaderVisible"
      :workcenterId="id"
      :workOrderId="unloadWorkOrderData.workOrderId"
      :workOrderCode="unloadWorkOrderData.workOrderCode"
      :workOrderPhaseId="unloadWorkOrderData.workOrderPhaseId"
      :currentPhaseStatusId="unloadWorkOrderData.currentPhaseStatusId"
      :referenceCode="unloadWorkOrderData.referenceCode"
      :plannedQuantity="unloadWorkOrderData.plannedQuantity"
      :phaseDescription="unloadWorkOrderData.phaseDescription"
      :workcenterTypeId="workcenter?.config.workcenterTypeId || ''"
      :nextMachineStatusId="unloadNextMachineStatusId"
      :showNextPhaseOption="unloadShowNextPhaseOption"
      @phase-unloaded="handlePhaseUnloaded"
    />

    <!-- Phase Quantities Dialog -->
    <WorkOrderPhaseQuantities
      v-model:visible="phaseQuantitiesVisible"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRoute } from "vue-router";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "../../../store";
import {
  usePlantWorkcenterStore,
  usePlantOperatorStore,
  usePlantDataStore,
} from "../store";
import WorkcenterRealtimePanel from "../components/workcenter-detail/WorkcenterRealtimePanel.vue";
import WorkcenterDocumentation from "../components/workcenter-detail/WorkcenterDocumentation.vue";
import WorkcenterComments from "../components/workcenter-detail/WorkcenterComments.vue";
import WorkcenterWorkOrderSelector from "../components/workcenter-detail/WorkcenterWorkOrderSelector.vue";
import WorkOrderLoader from "../components/workcenter-detail/WorkOrderLoader.vue";
import WorkOrderUnloader from "../components/workcenter-detail/WorkOrderUnloader.vue";
import WorkOrderPhaseQuantities from "../components/workcenter-detail/WorkOrderPhaseQuantities.vue";
import MachineStatusSelector from "../components/MachineStatusSelector.vue";
import { WorkOrderWithPhases } from "../../production/types";
import {
  useWebSocketConnection,
  WS_ENDPOINTS,
} from "../composables/useWebSocketConnection";
import {
  ChangeMachineStatusRequest,
  LoadWorkOrderPhaseRequest,
  UnloadWorkOrderPhaseRequest,
} from "../types";
import actionsService from "../services/actions.service";
import { normalizeColor, isColorLight } from "@/utils/functions";

const route = useRoute();
const toast = useToast();
const appStore = useStore();
const workcenterStore = usePlantWorkcenterStore();
const operatorStore = usePlantOperatorStore();
const dataStore = usePlantDataStore();
const { connect } = useWebSocketConnection();

const id = route.params.id as string;
const activeTab = ref("0");
const statusSelectorVisible = ref(false);
const workOrderLoaderVisible = ref(false);
const workOrderUnloaderVisible = ref(false);
const phaseQuantitiesVisible = ref(false);
const selectedWorkOrderData = ref({
  workOrderId: "",
  workOrderCode: "",
  referenceCode: "",
  quantity: 0,
});
const unloadWorkOrderData = ref({
  workOrderId: "",
  workOrderCode: "",
  workOrderPhaseId: "",
  currentPhaseStatusId: "",
  referenceCode: "",
  plannedQuantity: 0,
  phaseDescription: "",
});
const unloadNextMachineStatusId = ref<string | undefined>(undefined);
const unloadShowNextPhaseOption = ref(true);

const workcenter = computed(() => workcenterStore.workcenterView);

// Computed para determinar si el operario está fichado
const isOperatorClockedIn = computed(() => {
  if (!workcenter.value?.realtime?.operators || !operatorStore.operator) {
    return false;
  }
  return workcenter.value.realtime.operators.some(
    (op) => op.operatorId === operatorStore.operator!.id,
  );
});

// Computed para determinar si hay una fase cargada
const hasLoadedPhase = computed(() => {
  return (
    workcenter.value?.realtime?.workorders &&
    workcenter.value.realtime.workorders.length > 0
  );
});

// Computed para detectar si la máquina está en estado Closed (parada)
const isMachineClosed = computed(() => {
  return workcenter.value?.realtime?.statusClosed === true;
});

// Computed para obtener el estado Closed desde el store de datos
const closedStatus = computed(() => {
  return dataStore.machineStatuses.find((s) => s.closed === true);
});

// Computed para obtener la fase actualmente cargada
const currentLoadedPhase = computed(() => {
  return workcenterStore.loadedWorkOrdersPhases?.[0]?.phases?.[0];
});

// Computed para obtener los botones de actividad (excluyendo estado actual)
const phaseActivityButtons = computed(() => {
  const details = currentLoadedPhase.value?.details ?? [];
  const currentStatusId = workcenter.value?.realtime?.statusId;
  return details
    .filter((d) => d.machineStatusId !== currentStatusId)
    .sort((a, b) => a.order - b.order);
});

// Computed para obtener los IDs de estados a excluir del selector "Altres"
// Excluye: estado actual, estado "Parada" (closed), y estados dinámicos de la fase
const excludeStatusIds = computed(() => {
  const ids: string[] = [];

  // Excluir estado actual de la máquina
  if (workcenter.value?.realtime?.statusId) {
    ids.push(workcenter.value.realtime.statusId);
  }

  // Excluir estado "Parada" (closed) - ya tiene su propio botón
  if (closedStatus.value) {
    ids.push(closedStatus.value.id);
  }

  // Excluir estados dinámicos de la fase (ya tienen sus propios botones)
  const details = currentLoadedPhase.value?.details ?? [];
  for (const detail of details) {
    if (detail.machineStatusId) {
      ids.push(detail.machineStatusId);
    }
  }

  return ids;
});

// Utilidad para calcular color de contraste (texto blanco o negro) - usa isColorLight de functions.ts
const getContrastColor = (hexColor: string): string => {
  const normalized = normalizeColor(hexColor);
  return isColorLight(normalized) ? "#000000" : "#ffffff";
};

onMounted(async () => {
  // 1. Carregar dades del workcenter
  await workcenterStore.fetchWorkcenter(id);

  if (!workcenter.value) {
    toast.add({
      severity: "error",
      summary: "Centre de treball no trobat",
      life: 4000,
    });
    return;
  }

  // 2. Configurar header
  appStore.setMenuItem({
    icon: PrimeIcons.COG,
    backButtonVisible: true,
    title: workcenter.value.config.description,
  });

  // 3. Carregar estats de màquina
  await dataStore.fetchMachineStatuses();

  // 4. Establir pestanya activa segons si hi ha fase carregada
  // Si hi ha fase carregada -> Documentació (tab 1), si no -> Fases disponibles (tab 0)
  activeTab.value = hasLoadedPhase.value ? "1" : "0";

  // 5. Connectar WebSocket específic del workcenter
  workcenterStore.connectToWorkcenter(id);
  connect(WS_ENDPOINTS.WORKCENTER(id), { debug: true });
});

onUnmounted(() => {
  // Netejar informació del workcenter dels stores
  workcenterStore.clearWorkcenter();
});

const handleOperatorClockIn = async () => {
  const result = await workcenterStore.clockInOperator();
  if (result) {
    toast.add({
      severity: "success",
      summary: "Entrada registrada correctament",
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al registrar l'entrada",
      life: 4000,
    });
  }
};

const handleOperatorClockOut = async () => {
  const result = await workcenterStore.clockOutOperator();
  if (result) {
    toast.add({
      severity: "success",
      summary: "Sortida registrada correctament",
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al registrar la sortida",
      life: 4000,
    });
  }
};

const handleMachineStatusChange = async () => {
  statusSelectorVisible.value = true;
};

// Handler para cerrar la máquina (cambiar a estado Closed directamente)
const handleCloseMachine = async () => {
  if (!closedStatus.value) {
    toast.add({
      severity: "error",
      summary: "No s'ha trobat l'estat de màquina tancada",
      life: 4000,
    });
    return;
  }

  // If there's a loaded phase, open unloader dialog
  if (hasLoadedPhase.value) {
    // Populate unload dialog data (same as handleWorkOrderPhaseClose)
    const currentActivePhase = workcenter.value!.realtime!.workorders[0];
    const loadedWorkOrder = workcenterStore.loadedWorkOrdersPhases?.[0];

    if (!loadedWorkOrder) {
      toast.add({
        severity: "error",
        summary: "No s'han pogut carregar les dades de l'ordre",
        life: 4000,
      });
      return;
    }

    const currentPhase = loadedWorkOrder.phases.find(
      (p) => p.phaseId === currentActivePhase.workOrderPhaseId,
    );

    if (!currentPhase) {
      toast.add({
        severity: "error",
        summary: "No s'ha pogut trobar la fase actual",
        life: 4000,
      });
      return;
    }

    unloadWorkOrderData.value = {
      workOrderId: loadedWorkOrder.workOrderId,
      workOrderCode: loadedWorkOrder.workOrderCode,
      workOrderPhaseId: currentActivePhase.workOrderPhaseId,
      currentPhaseStatusId: currentPhase.phaseStatusId,
      referenceCode: loadedWorkOrder.salesReferenceDisplay,
      plannedQuantity: loadedWorkOrder.plannedQuantity,
      phaseDescription: currentPhase.phaseDescription,
    };

    // Set next machine status to CLOSED and hide next phase option
    unloadNextMachineStatusId.value = closedStatus.value.id;
    unloadShowNextPhaseOption.value = false;
    workOrderUnloaderVisible.value = true;
    return;
  }

  // No phase loaded - just change machine status directly
  const result = await workcenterStore.changeMachineStatus(
    closedStatus.value.id,
  );
  if (result) {
    toast.add({
      severity: "success",
      summary: "Màquina tancada correctament",
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al tancar la màquina",
      life: 4000,
    });
  }
};

// Handler para cambiar actividad (botones dinámicos de fase)
const handleActivityChange = async (statusId: string) => {
  const result = await workcenterStore.changeMachineStatus(statusId);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Activitat canviada correctament",
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al canviar l'activitat",
      life: 4000,
    });
  }
};

const onStatusChanged = async (request: ChangeMachineStatusRequest) => {
  const result = await workcenterStore.changeMachineStatus(
    request.statusId,
    request.statusReasonId,
  );

  if (result) {
    toast.add({
      severity: "success",
      summary: "Estat canviat correctament",
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al canviar l'estat",
      life: 4000,
    });
  }
};

const handleWorkOrderSelected = (workOrder: WorkOrderWithPhases) => {
  selectedWorkOrderData.value = {
    workOrderId: workOrder.workOrderId,
    workOrderCode: workOrder.workOrderCode,
    referenceCode: workOrder.salesReferenceDisplay,
    quantity: workOrder.plannedQuantity,
  };
  workOrderLoaderVisible.value = true;
};

const handlePhaseDetailSelected = async (data: {
  workOrderId: string;
  workOrderPhaseId: string;
  machineStatusId: string;
}) => {
  const request: LoadWorkOrderPhaseRequest = {
    workcenterId: id,
    workOrderPhaseId: data.workOrderPhaseId,
    machineStatusId: data.machineStatusId,
  };

  const result =
    await actionsService.client.loadWorkOrderPhaseAndMachineStatus(request);

  if (result) {
    workOrderLoaderVisible.value = false;
    // Refresh available work orders list
    if (workcenter.value?.config.workcenterTypeId) {
      await workcenterStore.fetchAvailableWorkOrders(
        workcenter.value.config.workcenterTypeId,
      );
    }
    toast.add({
      severity: "success",
      summary: "Fase de fabricació carregada",
      detail: "L'activitat s'ha carregat correctament al centre de treball",
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al carregar la fase",
      detail: "No s'ha pogut carregar l'activitat",
      life: 4000,
    });
  }
};

const handleWorkOrderPhaseClose = async () => {
  if (
    !workcenter.value?.realtime?.workorders ||
    workcenter.value.realtime.workorders.length === 0
  ) {
    toast.add({
      severity: "warn",
      summary: "No hi ha cap fase carregada",
      life: 4000,
    });
    return;
  }

  // Extract current phase data from realtime and loaded work orders
  const currentActivePhase = workcenter.value.realtime.workorders[0];
  const loadedWorkOrder = workcenterStore.loadedWorkOrdersPhases?.[0];

  if (!loadedWorkOrder) {
    toast.add({
      severity: "error",
      summary: "No s'han pogut carregar les dades de l'ordre",
      life: 4000,
    });
    return;
  }

  // Find the matching phase in the loaded work order
  const currentPhase = loadedWorkOrder.phases.find(
    (p) => p.phaseId === currentActivePhase.workOrderPhaseId,
  );

  if (!currentPhase) {
    toast.add({
      severity: "error",
      summary: "No s'ha pogut trobar la fase actual",
      life: 4000,
    });
    return;
  }

  // Populate unload dialog data
  unloadWorkOrderData.value = {
    workOrderId: loadedWorkOrder.workOrderId,
    workOrderCode: loadedWorkOrder.workOrderCode,
    workOrderPhaseId: currentActivePhase.workOrderPhaseId,
    currentPhaseStatusId: currentPhase.phaseStatusId,
    referenceCode: loadedWorkOrder.salesReferenceDisplay,
    plannedQuantity: loadedWorkOrder.plannedQuantity,
    phaseDescription: currentPhase.phaseDescription,
  };

  // Set next machine status to PAUSA (stopped) and show next phase option
  const stoppedStatus = dataStore.machineStatuses.find((s) => s.stopped === true);
  unloadNextMachineStatusId.value = stoppedStatus?.id;
  unloadShowNextPhaseOption.value = true;

  workOrderUnloaderVisible.value = true;
};

const handlePhaseUnloaded = async (data: UnloadWorkOrderPhaseRequest) => {
  const result = await actionsService.client.unloadWorkOrderPhase(data);

  if (result) {
    workOrderUnloaderVisible.value = false;
    // Refresh available work orders list
    if (workcenter.value?.config.workcenterTypeId) {
      await workcenterStore.fetchAvailableWorkOrders(
        workcenter.value.config.workcenterTypeId,
      );
    }
    toast.add({
      severity: "success",
      summary: "Fase finalitzada correctament",
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al finalitzar la fase",
      life: 4000,
    });
  }
};
</script>

<style scoped>
.workcenter-detail {
  display: flex;
  flex-direction: column;
  height: calc(100vh - var(--top-panel-height) - 1rem);
  overflow: hidden;
}

.workcenter-main {
  flex: 1;
  overflow: hidden;
  padding-bottom: 1rem;
}

.content-layout {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 1rem;
  height: 100%;
}

.realtime-panel {
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
  border-radius: var(--border-radius);
  overflow-y: auto;
  height: 100%;
}

.tabs-panel {
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-border);
  border-radius: var(--border-radius);
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.tabs-panel :deep(.p-tabs) {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.tabs-panel :deep(.p-tabpanels) {
  flex: 1;
  overflow-y: auto;
}

.touch-panel {
  background: var(--p-surface-50);
  border-top: 1px solid var(--p-surface-border);
  padding: 0.75rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1.5rem;
  flex-wrap: wrap;
}

.action-group {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.action-group.operator-group {
  align-items: flex-start;
}

.action-group.phase-group {
  align-items: flex-end;
}

.group-label {
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--p-text-muted-color);
}

.group-buttons {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  justify-content: center;
}

.group-separator {
  width: 1px;
  align-self: stretch;
  background: var(--p-surface-border);
  margin: 0.5rem 0;
}

.touch-button {
  min-width: 150px;
  font-size: 1.1rem;
  padding: 0.75rem 1.5rem;
}

.touch-button :deep(.p-button-icon) {
  font-size: 1.3rem;
}

.activity-button {
  background-color: var(--btn-bg) !important;
  color: var(--btn-color) !important;
  border-color: var(--btn-bg) !important;
}

.activity-button:hover:not(:disabled) {
  filter: brightness(0.9);
}

@media (max-width: 1200px) {
  .content-layout {
    grid-template-columns: 300px 1fr;
  }

  .touch-panel {
    gap: 1rem;
    padding: 0.75rem 1rem;
  }

  .touch-button {
    min-width: 130px;
    font-size: 1rem;
    padding: 0.6rem 1rem;
  }
}

@media (max-width: 768px) {
  .content-layout {
    grid-template-columns: 1fr;
  }

  .realtime-panel {
    display: none;
  }

  .touch-panel {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
    padding: 0.75rem;
  }

  .group-separator {
    width: 100%;
    height: 1px;
    margin: 0;
  }

  .action-group {
    width: 100%;
  }

  .group-buttons {
    width: 100%;
    flex-direction: column;
  }

  .touch-button {
    width: 100%;
    min-width: unset;
    justify-content: center;
  }
}
</style>
