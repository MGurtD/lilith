<template>
  <div class="wc-detail">
    <template v-if="workcenter">
      <WorkcenterPlaca
        :workcenter="workcenter"
        :compact="isPhone"
        @clock-in="handleOperatorClockIn"
        @clock-out="handleOperatorClockOut"
      />

      <section class="wc-detail__tabs">
        <Tabs v-model:value="activeTab" scrollable>
          <TabList>
            <Tab v-if="hasLoadedPhase" value="current">{{
              t("plant.detail.tabs.current")
            }}</Tab>
            <Tab value="queue">{{ t("plant.detail.tabs.queue") }}</Tab>
            <Tab v-if="hasLoadedPhase" value="docs">{{
              t("plant.detail.tabs.docs")
            }}</Tab>
            <Tab v-if="hasLoadedPhase" value="notes">{{
              t("plant.detail.tabs.notes")
            }}</Tab>
            <Tab
              v-if="hasLoadedPhase && activePhaseStore.hasBillOfMaterials"
              value="bom"
              >{{ t("plant.detail.tabs.bom") }}</Tab
            >
          </TabList>
          <TabPanels>
            <TabPanel v-if="hasLoadedPhase" value="current">
              <PhaseTimeSummary />
            </TabPanel>
            <TabPanel value="queue">
              <WorkcenterWorkOrderSelector
                :workcenterTypeId="workcenter.config.workcenterTypeId"
                @workorder-selected="handleWorkOrderSelected"
              />
            </TabPanel>
            <TabPanel v-if="hasLoadedPhase" value="docs">
              <WorkcenterDocumentation :workcenter="workcenter" />
            </TabPanel>
            <TabPanel v-if="hasLoadedPhase" value="notes">
              <WorkcenterComments
                :loadedWorkOrders="workcenterStore.loadedWorkOrdersPhases"
              />
            </TabPanel>
            <TabPanel
              v-if="hasLoadedPhase && activePhaseStore.hasBillOfMaterials"
              value="bom"
            >
              <WorkcenterMaterials />
            </TabPanel>
          </TabPanels>
        </Tabs>
      </section>
    </template>

    <!-- Action dock: where the thumb reaches. -->
    <footer class="wc-dock" :class="{ 'wc-dock--phone': isPhone }">
      <template v-if="!isPhone">
        <div class="wc-dock__group" role="group" aria-labelledby="wc-dock-status">
          <span id="wc-dock-status" class="wc-dock__label">{{
            t("plant.detail.statusGroup")
          }}</span>
          <div class="wc-dock__buttons">
            <button
              v-for="key in statusKeys"
              :key="key.id"
              type="button"
              class="wc-key"
              :class="{ 'wc-key--current': key.current }"
              :style="key.current ? key.style : undefined"
              :aria-pressed="key.current"
              :disabled="key.current"
              @click="key.action()"
            >
              <span class="wc-key__swatch" :style="{ background: key.color }"></span>
              {{ key.name }}
            </button>
            <Button
              icon="pi pi-ellipsis-h"
              :label="t('plant.detail.otherStatuses')"
              outlined
              severity="secondary"
              class="wc-dock__button"
              @click="handleMachineStatusChange"
            />
          </div>
        </div>
        <span class="wc-dock__spacer"></span>
        <div
          v-if="hasLoadedPhase"
          class="wc-dock__group"
          role="group"
          aria-labelledby="wc-dock-phase"
        >
          <span id="wc-dock-phase" class="wc-dock__label">{{
            t("plant.detail.phaseGroup")
          }}</span>
          <div class="wc-dock__buttons">
            <Button
              icon="pi pi-plus"
              :label="t('plant.detail.declare')"
              class="wc-dock__button wc-dock__button--primary"
              @click="phaseQuantitiesVisible = true"
            />
            <Button
              icon="pi pi-check"
              :label="t('plant.detail.finish')"
              outlined
              class="wc-dock__button wc-dock__button--finish"
              @click="handleWorkOrderPhaseClose"
            />
          </div>
        </div>
        <p v-else class="wc-dock__hint">{{ t("plant.detail.noPhaseActions") }}</p>
      </template>

      <template v-else>
        <Button
          outlined
          severity="secondary"
          class="wc-dock__button wc-dock__button--stack"
          aria-haspopup="dialog"
          @click="sheet = 'status'"
        >
          <span class="wc-key__swatch" :style="{ background: currentStatusColor }"></span>
          {{ t("plant.detail.status") }}
        </Button>
        <Button
          v-if="hasLoadedPhase"
          icon="pi pi-plus"
          :label="t('plant.detail.declareShort')"
          :aria-label="t('plant.detail.declare')"
          class="wc-dock__button wc-dock__button--primary"
          @click="phaseQuantitiesVisible = true"
        />
        <Button
          icon="pi pi-ellipsis-h"
          :label="t('plant.detail.more')"
          outlined
          severity="secondary"
          class="wc-dock__button"
          aria-haspopup="dialog"
          @click="sheet = 'more'"
        />
      </template>
    </footer>

    <!-- Phone sheets -->
    <Drawer
      :visible="sheet !== null"
      position="bottom"
      class="plant-sheet"
      :header="sheetTitle"
      @update:visible="onSheetVisible"
    >
      <div v-if="sheet === 'status'" class="plant-sheet__list">
        <button
          v-for="key in statusKeys"
          :key="key.id"
          type="button"
          class="plant-sheet__row"
          :aria-pressed="key.current"
          :disabled="key.current"
          @click="runFromSheet(key.action)"
        >
          <span
            class="wc-key__swatch wc-key__swatch--large"
            :style="{ background: key.color }"
          ></span>
          <span class="plant-sheet__name">{{ key.name }}</span>
          <span v-if="key.current" class="plant-sheet__note">{{
            t("plant.detail.current")
          }}</span>
        </button>
        <button
          type="button"
          class="plant-sheet__row"
          @click="runFromSheet(handleMachineStatusChange)"
        >
          <i class="pi pi-ellipsis-h plant-sheet__icon" aria-hidden="true"></i>
          <span class="plant-sheet__name">{{ t("plant.detail.otherStatuses") }}</span>
        </button>
      </div>
      <div v-else-if="sheet === 'more'" class="plant-sheet__list">
        <button
          v-if="hasLoadedPhase"
          type="button"
          class="plant-sheet__row"
          @click="runFromSheet(handleWorkOrderPhaseClose)"
        >
          <i class="pi pi-check plant-sheet__icon" aria-hidden="true"></i>
          <span class="plant-sheet__name">{{ t("plant.detail.finish") }}</span>
        </button>
        <button
          type="button"
          class="plant-sheet__row"
          :disabled="!canManageOperators"
          @click="runFromSheet(toggleClock)"
        >
          <i
            class="pi plant-sheet__icon"
            :class="isOperatorClockedIn ? 'pi-sign-out' : 'pi-sign-in'"
            aria-hidden="true"
          ></i>
          <span class="plant-sheet__name">{{
            isOperatorClockedIn ? t("plant.placa.leave") : t("plant.placa.enter")
          }}</span>
        </button>
      </div>
    </Drawer>

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
      :workcenterId="id"
      @phase-detail-selected="handlePhaseDetailSelected"
      @phase-created="handlePhaseCreated"
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
    <WorkOrderPhaseQuantities v-model:visible="phaseQuantitiesVisible" />
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, computed, onMounted, onUnmounted, watch } from "vue";
import { useRoute } from "vue-router";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "../../../store";
import {
  usePlantWorkcenterStore,
  usePlantOperatorStore,
  usePlantDataStore,
  usePlantActivePhaseStore,
} from "../store";
import Drawer from "primevue/drawer";
import { useIsPhone } from "@/composables/useIsPhone";
import WorkcenterPlaca from "../components/workcenter-detail/WorkcenterPlaca.vue";
import PhaseTimeSummary from "../components/workcenter-detail/PhaseTimeSummary.vue";
import WorkcenterDocumentation from "../components/workcenter-detail/WorkcenterDocumentation.vue";
import WorkcenterComments from "../components/workcenter-detail/WorkcenterComments.vue";
import WorkcenterMaterials from "../components/workcenter-detail/WorkcenterMaterials.vue";
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
import { WorkOrderPhaseRejectionRequest } from "../../production/types";
import actionsService from "../services/actions.service";
import { normalizeColor } from "@/utils/functions";
import { statusSignal } from "../utils/statusSignal";

const { t } = useI18n();

const route = useRoute();
const toast = useToast();
const appStore = useStore();
const workcenterStore = usePlantWorkcenterStore();
const operatorStore = usePlantOperatorStore();
const dataStore = usePlantDataStore();
const activePhaseStore = usePlantActivePhaseStore();
const { connect } = useWebSocketConnection();

const id = route.params.id as string;
const isPhone = useIsPhone();
const activeTab = ref("queue");
const sheet = ref<"status" | "more" | null>(null);
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

// Computed para determinar si la actividad actual permite gestionar operarios
const canManageOperators = computed(() => {
  return workcenter.value?.realtime?.statusOperatorsAllowed === true;
});

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

// Status keys of the dock: the phase's activities (when a phase is loaded)
// and the closed status; the current one is filled with its colour.
const statusKeys = computed(() => {
  const currentStatusId = workcenter.value?.realtime?.statusId;
  const keys: {
    id: string;
    name: string;
    color: string;
    current: boolean;
    style: Record<string, string>;
    action: () => void;
  }[] = [];
  const add = (id: string, name: string, color: string, action: () => void) => {
    if (keys.some((key) => key.id === id)) return;
    keys.push({
      id,
      name,
      color: normalizeColor(color),
      current: id === currentStatusId,
      style: statusSignal(color).style,
      action,
    });
  };
  if (hasLoadedPhase.value) {
    [...(currentLoadedPhase.value?.details ?? [])]
      .sort((a, b) => a.order - b.order)
      .forEach((detail) => {
        if (detail.machineStatusId) {
          const statusId = detail.machineStatusId;
          add(statusId, detail.machineStatusName, detail.machineStatusColor, () =>
            handleActivityChange(statusId),
          );
        }
      });
  }
  if (closedStatus.value) {
    add(closedStatus.value.id, closedStatus.value.name, closedStatus.value.color, () =>
      handleCloseMachine(),
    );
  }
  return keys;
});

const currentStatusColor = computed(() => {
  const statusId = workcenter.value?.realtime?.statusId;
  const color = statusId ? dataStore.getMachineStatusById(statusId)?.color : undefined;
  return statusSignal(color).band;
});

const sheetTitle = computed(() =>
  sheet.value === "status"
    ? t("plant.detail.changeStatus")
    : t("plant.detail.moreActions"),
);

const onSheetVisible = (visible: boolean) => {
  if (!visible) sheet.value = null;
};

const runFromSheet = (action: () => unknown) => {
  sheet.value = null;
  void action();
};

const toggleClock = () =>
  isOperatorClockedIn.value ? handleOperatorClockOut() : handleOperatorClockIn();

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

const loadMaterialsProvisioningIfNeeded = async () => {
  if (
    activeTab.value !== "bom" ||
    !hasLoadedPhase.value ||
    !activePhaseStore.hasBillOfMaterials
  ) {
    return;
  }

  await activePhaseStore.ensureMaterialsProvisioningLoaded();
};

onMounted(async () => {
  // 1. Carregar dades del workcenter
  await workcenterStore.fetchWorkcenter(id);

  if (!workcenter.value) {
    toast.add({
      severity: "error",
      summary: t("plant.centre-de-treball-no-trobat"),
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

  // 4. Carregar ubicacions associades al workcenter sense bloquejar la resta del flux
  void workcenterStore.fetchWorkcenterLocations(id);

  // 5. With a phase loaded the screen opens on it, else on the phase list.
  activeTab.value = hasLoadedPhase.value ? "current" : "queue";

  // 6. Connectar WebSocket específic del workcenter
  workcenterStore.connectToWorkcenter(id);
  connect(WS_ENDPOINTS.WORKCENTER(id), { debug: true });
});

// Loading a phase opens it; unloading returns to the phase list (the
// phase tabs no longer exist).
watch(hasLoadedPhase, (loaded) => {
  activeTab.value = loaded ? "current" : "queue";
});

watch(
  [
    activeTab,
    () => activePhaseStore.activePhase?.phaseId,
    () => activePhaseStore.billOfMaterials.length,
  ],
  () => {
    void loadMaterialsProvisioningIfNeeded();
  },
);

onUnmounted(() => {
  // Netejar informació del workcenter dels stores
  workcenterStore.clearWorkcenter();
});

const handleOperatorClockIn = async () => {
  const result = await workcenterStore.clockInOperator();
  if (result) {
    toast.add({
      severity: "success",
      summary: t("plant.entrada-registrada-correctament"),
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("plant.messages.operatorClockInError"),
      life: 4000,
    });
  }
};

const handleOperatorClockOut = async () => {
  const result = await workcenterStore.clockOutOperator();
  if (result) {
    toast.add({
      severity: "success",
      summary: t("plant.sortida-registrada-correctament"),
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("plant.error-al-registrar-la-sortida"),
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
      summary: t("plant.messages.closedMachineStatusNotFound"),
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
        summary: t("plant.messages.workOrderDataLoadError"),
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
        summary: t("plant.messages.currentPhaseNotFound"),
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
      summary: t("plant.maquina-tancada-correctament"),
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("plant.error-al-tancar-la-maquina"),
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
      summary: t("plant.activitat-canviada-correctament"),
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("plant.messages.activityChangeError"),
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
      summary: t("plant.estat-canviat-correctament"),
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("plant.messages.statusChangeError"),
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
      summary: t("plant.fase-de-fabricacio-carregada"),
      detail: t("plant.messages.activityLoaded"),
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("plant.error-al-carregar-la-fase"),
      detail: t("plant.messages.activityLoadError"),
      life: 4000,
    });
  }
};

const handlePhaseCreated = async () => {
  // Refresh available work orders list after a phase was created from template
  if (workcenter.value?.config.workcenterTypeId) {
    await workcenterStore.fetchAvailableWorkOrders(
      workcenter.value.config.workcenterTypeId,
    );
  }
};

const handleWorkOrderPhaseClose = async () => {
  if (
    !workcenter.value?.realtime?.workorders ||
    workcenter.value.realtime.workorders.length === 0
  ) {
    toast.add({
      severity: "warn",
      summary: t("plant.no-hi-ha-cap-fase-carregada"),
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
      summary: t("plant.messages.workOrderDataLoadError"),
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
      summary: t("plant.messages.currentPhaseNotFound"),
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

const handlePhaseUnloaded = async (
  data: UnloadWorkOrderPhaseRequest,
  rejections: WorkOrderPhaseRejectionRequest[],
) => {
  const result = await actionsService.client.unloadWorkOrderPhase(data);

  if (result) {
    workOrderUnloaderVisible.value = false;

    // The unload endpoint owns the quantities; the reasons behind the KO units
    // are recorded separately once the unload has succeeded.
    const rejectionsRegistered = await activePhaseStore.registerPhaseRejections(
      data.workOrderPhaseId,
      data.quantityKo,
      rejections,
    );
    if (!rejectionsRegistered) {
      toast.add({
        severity: "warn",
        summary: t("plant.rejections.registerError"),
        life: 6000,
      });
    }

    // Refresh available work orders list
    if (workcenter.value?.config.workcenterTypeId) {
      await workcenterStore.fetchAvailableWorkOrders(
        workcenter.value.config.workcenterTypeId,
      );
    }
    toast.add({
      severity: "success",
      summary: t("plant.fase-finalitzada-correctament"),
      life: 4000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("plant.error-al-finalitzar-la-fase"),
      life: 4000,
    });
  }
};
</script>

<style scoped>
.wc-detail {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  height: calc(100dvh - var(--top-panel-height) - 2.5rem);
  overflow: hidden;
}

.wc-detail__tabs {
  flex: 1;
  min-height: 0;
  display: flex;
  flex-direction: column;
  background: var(--p-surface-0);
  border: 1px solid var(--p-steel-200);
  border-radius: 4px;
  overflow: hidden;
}

.wc-detail__tabs :deep(.p-tabs) {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.wc-detail__tabs :deep(.p-tab) {
  min-height: 52px;
}

.wc-detail__tabs :deep(.p-tabpanels) {
  flex: 1;
  overflow-y: auto;
  padding: 0;
}

/* Dock */
.wc-dock {
  flex-shrink: 0;
  display: flex;
  align-items: flex-end;
  gap: 1.5rem;
  padding: 0.625rem 0.875rem 0.75rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-steel-200);
  border-radius: 4px;
}

.wc-dock__group {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.wc-dock__label {
  font-family: var(--font-condensed);
  font-size: 0.8125rem;
  font-weight: 500;
  color: var(--p-steel-600);
}

.wc-dock__buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.wc-dock__spacer {
  flex: 1;
}

.wc-dock__button {
  min-height: 56px;
  padding-inline: 1rem;
  font-size: 1rem;
}

.wc-dock__button--primary {
  padding-inline: 1.25rem;
  font-size: 1.0625rem;
}

.wc-dock__button--finish {
  color: var(--p-steel-900);
  border: 2px solid var(--p-steel-900);
  font-size: 1.0625rem;
}

.wc-dock__hint {
  align-self: center;
  max-width: 20rem;
  margin: 1.125rem 0 0;
  text-align: right;
  font-size: 0.9375rem;
  color: var(--p-steel-600);
}

.wc-key {
  min-height: 56px;
  display: inline-flex;
  align-items: center;
  gap: 0.625rem;
  padding: 0 1rem;
  border: none;
  border-radius: 4px;
  box-shadow: inset 0 0 0 1px var(--p-steel-300);
  background: var(--p-surface-0);
  font: inherit;
  font-size: 1rem;
  font-weight: 500;
  color: var(--p-steel-900);
  cursor: pointer;
}

.wc-key:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: 2px;
}

.wc-key:hover:not(:disabled) {
  box-shadow: inset 0 0 0 1px var(--p-steel-500);
}

.wc-key--current {
  box-shadow: none;
  font-weight: 600;
  cursor: default;
}

.wc-key__swatch {
  flex-shrink: 0;
  width: 16px;
  height: 16px;
  border-radius: 3px;
}

.wc-key--current .wc-key__swatch {
  box-shadow: 0 0 0 2px currentColor;
}

.wc-key__swatch--large {
  width: 20px;
  height: 20px;
  border-radius: 4px;
}

/* Phones: three buttons; status and more open sheets. */
.wc-dock--phone {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(0, 1fr));
  gap: 0.5rem;
  padding: 0.5rem 0.5rem calc(0.5rem + env(safe-area-inset-bottom, 0px));
}

.wc-dock--phone .wc-dock__button {
  justify-content: center;
  padding-inline: 0.5rem;
}

.wc-dock__button--stack {
  gap: 0.5rem;
}

.wc-dock :deep(.p-button-outlined.p-button-secondary) {
  color: var(--p-steel-900);
  border-color: var(--p-steel-300);
}

@media (max-width: 767.98px) {
  .wc-detail {
    height: calc(100dvh - var(--top-panel-height) - 2rem);
    gap: 0.625rem;
  }
}
</style>

<style>
/* Phone sheets are teleported to <body>. Scoped under the position class so
   they beat PrimeVue's default drawer height (see TableFilter sheets). */
.p-drawer-bottom .p-drawer.plant-sheet,
.p-drawer.p-drawer-bottom.plant-sheet {
  height: auto;
  max-height: 80dvh;
  border-radius: 12px 12px 0 0;
}

.plant-sheet .p-drawer-content {
  padding: 0 0 env(safe-area-inset-bottom, 0px);
}

.plant-sheet__list {
  display: flex;
  flex-direction: column;
  border-top: 1px solid var(--p-steel-100);
}

.plant-sheet__row {
  min-height: 64px;
  display: flex;
  align-items: center;
  gap: 0.875rem;
  padding: 0 1.25rem;
  border: none;
  border-bottom: 1px solid var(--p-steel-100);
  background: var(--p-surface-0);
  font: inherit;
  font-size: 1.0625rem;
  color: var(--p-steel-900);
  text-align: left;
  cursor: pointer;
}

.plant-sheet__row:disabled {
  cursor: default;
}

.plant-sheet__row:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: -3px;
}

.plant-sheet__name {
  flex: 1;
}

.plant-sheet__note {
  font-size: 0.875rem;
  color: var(--p-steel-600);
}

.plant-sheet__icon {
  width: 20px;
  text-align: center;
  color: var(--p-steel-700);
}
</style>
