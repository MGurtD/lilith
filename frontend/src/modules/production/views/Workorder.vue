<template>
  <header>
    <FormWorkorder
      v-if="workorder"
      ref="workorderForm"
      :workorder="workorder"
      @submit="onWorkorderSubmit"
      @download="printReport"
      @download-pdf="printPdf"
    ></FormWorkorder>
  </header>
  <main class="main">
    <div v-if="workorder !== undefined">
      <Tabs v-model:value="activeTab" :key="workorder.id">
        <TabList>
          <Tab value="0">{{ pt("Fases") }}</Tab>
          <Tab value="1">{{ pt("Hores") }}</Tab>
          <Tab value="2">{{ pt("Costs") }}</Tab>
          <Tab value="3">{{ pt("Moviments") }}</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <TableWorkorderPhases
              v-if="workorder.phases"
              :workorder="workorder"
              :workorderPhases="workorder.phases"
              @add="addWorkOrderPhase"
              @edit="editWorkOrderPhase"
              @delete="deleteWorkOrderPhase"
            ></TableWorkorderPhases>
          </TabPanel>
          <TabPanel value="1">
            <TableProductionParts
              v-if="productionPartStore.productionParts"
              :productionParts="productionPartStore.productionParts"
              @delete="deleteProductionPart"
            >
              <template #header>
                <div
                  class="flex flex-wrap align-items-center justify-content-between gap-2"
                >
                  <span class="text-900 font-bold">{{ pt("Hores") }}</span>
                  <Button
                    :icon="PrimeIcons.PLUS"
                    rounded
                    raised
                    @click="onProductionPartAddClick"
                  />
                </div>
              </template>
            </TableProductionParts>
          </TabPanel>
          <TabPanel value="2" v-if="workorder">
            <div class="costs-container">
              <div class="costs-section">
                <h4 class="costs-section-title">
                  <i class="pi pi-euro" />
                  {{ pt("Costos") }}
                </h4>
                <div class="costs-grid">
                  <div class="cost-card">
                    <div class="cost-card-icon">
                      <i class="pi pi-user" />
                    </div>
                    <div class="cost-card-content">
                      <span class="cost-card-label">{{ pt("Cost Operari") }}</span>
                      <span class="cost-card-value">{{
                        formatCurrency(workorder.operatorCost)
                      }}</span>
                    </div>
                  </div>
                  <div class="cost-card">
                    <div class="cost-card-icon">
                      <i class="pi pi-cog" />
                    </div>
                    <div class="cost-card-content">
                      <span class="cost-card-label">{{ pt("Cost Màquina") }}</span>
                      <span class="cost-card-value">{{
                        formatCurrency(workorder.machineCost)
                      }}</span>
                    </div>
                  </div>
                  <div class="cost-card">
                    <div class="cost-card-icon">
                      <i class="pi pi-box" />
                    </div>
                    <div class="cost-card-content">
                      <span class="cost-card-label">{{ pt("Cost Material") }}</span>
                      <span class="cost-card-value">{{
                        formatCurrency(workorder.materialCost)
                      }}</span>
                    </div>
                  </div>
                  <div class="cost-card cost-card-total">
                    <div class="cost-card-icon">
                      <i class="pi pi-calculator" />
                    </div>
                    <div class="cost-card-content">
                      <span class="cost-card-label">{{ pt("Cost Total") }}</span>
                      <span class="cost-card-value">{{
                        formatCurrency(
                          workorder.machineCost +
                            workorder.materialCost +
                            workorder.operatorCost,
                        )
                      }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="costs-section">
                <h4 class="costs-section-title">
                  <i class="pi pi-clock" />
                  {{ pt("Temps") }}
                </h4>
                <div class="time-grid">
                  <div class="cost-card">
                    <div class="cost-card-icon">
                      <i class="pi pi-user" />
                    </div>
                    <div class="cost-card-content">
                      <span class="cost-card-label">{{ pt("Temps Operari") }}</span>
                      <span class="cost-card-value"
                        >{{ workorder.operatorTime }} min</span
                      >
                    </div>
                  </div>
                  <div class="cost-card">
                    <div class="cost-card-icon">
                      <i class="pi pi-cog" />
                    </div>
                    <div class="cost-card-content">
                      <span class="cost-card-label">{{ pt("Temps Màquina") }}</span>
                      <span class="cost-card-value"
                        >{{ workorder.machineTime }} min</span
                      >
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </TabPanel>
          <TabPanel value="3">
            <TableWorkorderStockMovements
              v-if="stockMovementStore.stockMovements"
              :stockMovements="stockMovementStore.stockMovements"
            />
          </TabPanel>
        </TabPanels>
      </Tabs>
    </div>
  </main>
  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <FormWorkOrderProductionPart
      :productionPart="productionPartRequest"
      :avoid-work-order-refresh="true"
      @submit="createProductionPart"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import FormWorkOrderProductionPart from "../components/FormWorkOrderProductionPart.vue";
import FormWorkorder from "../components/FormWorkorder.vue";
import TableWorkorderPhases from "../components/TableWorkorderPhases.vue";
import TableProductionParts from "../components/TableProductionParts.vue";
import TableWorkorderStockMovements from "../components/TableWorkorderStockMovements.vue";

import { onMounted, reactive, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useReferenceStore } from "../../shared/store/reference";
import { useWorkOrderStore } from "../store/workorder";
import { useProductionPartStore } from "../store/productionpart";
import { useStockMovementStore } from "../../warehouse/store/stockMovement";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { ProductionPart, WorkOrder, WorkOrderPhase } from "../types";
import { usePlantModelStore } from "../store/plantmodel";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import {
  convertDateTimeToJSON,
  formatCurrency,
  getNewUuid,
  createBlobAndDownloadFile,
} from "../../../utils/functions";
import { useToast } from "primevue/usetoast";
import { DialogOptions } from "../../../types/component";
import Services from "../services";
import { REPORTS, ReportService } from "../../../services/report.service";

const route = useRoute();
const router = useRouter();
const store = useStore();
const toast = useToast();
const lifecycleStore = useLifecyclesStore();
const referenceStore = useReferenceStore();
const workorderStore = useWorkOrderStore();
const plantModelStore = usePlantModelStore();
const productionPartStore = useProductionPartStore();
const stockMovementStore = useStockMovementStore();
const { workorder } = storeToRefs(workorderStore);
const id = ref("");
const activeTab = ref("0");
const stockMovementsLoaded = ref(false);
const workorderForm = ref<InstanceType<typeof FormWorkorder> | null>(null);

watch(activeTab, async (newTab) => {
  if (newTab === "3" && !stockMovementsLoaded.value && id.value) {
    stockMovementsLoaded.value = true;
    await stockMovementStore.getByWorkOrderId(id.value);
  }
});

const dialogOptions = reactive({
  visible: false,
  title: pt("Crear tíquet de producció"),
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  id.value = route.params.id as string;
  workorderStore.detailedWorkOrders = undefined;
  await loadViewData();

  let pageTitle = "";
  pageTitle = `Ordre de fabricació`;
  if (workorder.value) {
    pageTitle = `${pageTitle} ${workorder.value.code}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });
});

const fetchWorkOrder = async () => {
  await workorderStore.fetchOne(id.value);
};

const loadViewData = async () => {
  referenceStore.fetchReferencesByModule("sales");
  plantModelStore.fetchActiveModel();
  lifecycleStore.fetchOneByName("WorkOrder");
  productionPartStore.fetchByWorkOrderId(id.value);

  await fetchWorkOrder();
};

const onWorkorderSubmit = async (workorder: WorkOrder) => {
  // Convert dates from display format to API format
  const workorderToSubmit = {
    ...workorder,
    plannedDate: workorder.plannedDate
      ? convertDateTimeToJSON(workorder.plannedDate)
      : null,
    startTime: workorder.startTime
      ? convertDateTimeToJSON(workorder.startTime)
      : null,
    endTime: workorder.endTime
      ? convertDateTimeToJSON(workorder.endTime)
      : null,
  };

  const updated = await workorderStore.update(id.value, workorderToSubmit);
  if (updated) {
    toast.add({
      severity: "success",
      summary: pt("Ordre de fabricació actualitzada"),
      life: 3000,
    });

    await loadViewData();
    await workorderForm.value?.reloadLifecycleTransitions();
  } else {
    toast.add({
      severity: "error",
      summary: "Error al actualitzar l'ordre de fabricació",
      detail: pt("Revisi el log per a més informació"),
      life: 10000,
    });
  }
};

// Phases
const addWorkOrderPhase = async (phase: WorkOrderPhase) => {
  const created = await workorderStore.createPhase(phase);
  if (created) {
    router.push({ path: `/workorder/${id.value}/phase/${phase.id}` });
  } else {
    toast.add({
      severity: "error",
      summary: pt("Error al crear la fase"),
      detail: pt("Revisi el log per a més informació"),
      life: 10000,
    });
  }
};
const editWorkOrderPhase = (phase: WorkOrderPhase) => {
  router.push({ path: `/workorder/${id.value}/phase/${phase.id}` });
};
const deleteWorkOrderPhase = async (phase: WorkOrderPhase) => {
  const result = await workorderStore.deletePhase(phase.id);
  if (result) {
    toast.add({
      severity: "success",
      summary: pt("Fase eliminada"),
      detail: `La fase ${phase.code} - ${phase.description} s'ha eliminat correctament`,
      life: 5000,
    });
  }
};

const productionPartRequest = ref({} as ProductionPart);
const onProductionPartAddClick = () => {
  productionPartRequest.value = {
    id: getNewUuid(),
    workOrderId: id.value,
    workOrderPhaseId: "",
    workOrderPhaseDetailId: "",
    operatorId: "",
    workcenterId: "",
    operatorTime: 0,
    workcenterTime: 0,
    quantity: 0,
    date: new Date(),
    machineHourCost: 0,
    operatorHourCost: 0,
  };
  if (workorder.value) workorderStore.fetchByWorkOrderId(workorder.value.id);

  dialogOptions.visible = true;
};

const createProductionPart = async () => {
  dialogOptions.visible = false;
  const created = await productionPartStore.create(productionPartRequest.value);
  if (created) {
    productionPartStore.fetchByWorkOrderId(id.value);
    fetchWorkOrder();
  }
};

const deleteProductionPart = async (productionPart: ProductionPart) => {
  await productionPartStore.delete(productionPart.id);
  productionPartStore.fetchByWorkOrderId(id.value);
  fetchWorkOrder();
};

const printReport = async () => {
  const workOrderReport = await Services.WorkOrder.GetReportDataById(
    workorder.value!.id,
  );

  if (workOrderReport) {
    const fileName = `OrdreFabricacio_${workorder.value?.code}.xlsx`;

    const reportService = new ReportService();
    const report = await reportService.Download(
      workOrderReport,
      REPORTS.WorkOrder,
      fileName,
    );

    if (report) {
      createBlobAndDownloadFile(fileName, report);
    } else {
      toast.add({
        severity: "warn",
        summary: pt("Error"),
        detail: "No s'ha pugut generar l'informe de l'ordre de fabricació",
      });
    }
  }
};
const printPdf = async () => {
  const report = await Services.WorkOrder.DownloadPdf(workorder.value!.id);
  if (report) {
    createBlobAndDownloadFile(`OrdreFabricacio_${workorder.value?.code}.pdf`, report);
  }
};

</script>
<style scoped>
.main {
  margin-top: 1rem;
}

/* Costs tab */
.costs-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  padding: 0.5rem 0;
}

.costs-section-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0 0 0.75rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--p-text-color);
}

.costs-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.75rem;
}

.time-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.75rem;
}

.cost-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.85rem 1rem;
  background: var(--p-content-background, #fff);
  transition: box-shadow 0.15s ease;
}

.cost-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.cost-card-total {
  background: var(--p-primary-50, #eef2ff);
  border-color: var(--p-primary-200, #c7d2fe);
}

.cost-card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 8px;
  background: var(--p-surface-100, #f1f5f9);
  color: var(--p-primary-color, #3b82f6);
  font-size: 1rem;
  flex-shrink: 0;
}

.cost-card-total .cost-card-icon {
  background: var(--p-primary-100, #dbeafe);
  color: var(--p-primary-700, #1d4ed8);
}

.cost-card-content {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  min-width: 0;
}

.cost-card-label {
  font-size: 0.8rem;
  color: var(--p-text-muted-color);
  white-space: nowrap;
}

.cost-card-value {
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.cost-card-total .cost-card-value {
  color: var(--p-primary-700, #1d4ed8);
}

@media (max-width: 1200px) {
  .costs-grid,
  .time-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .costs-grid,
  .time-grid {
    grid-template-columns: 1fr;
  }
}
</style>
