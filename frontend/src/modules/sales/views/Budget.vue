<template>
  <SplitButton
    label="Guardar"
    @click="submitForm"
    :model="items"
    :size="'small'"
    class="grid_add_row_button"
  />

  <FormBudget class="mt-3 mb-3" ref="budgetForm" @submit="onBudgetSubmit" />

  <Tabs value="0">
    <TabList>
      <Tab value="0">Detall</Tab>
      <Tab value="1">Transport</Tab>
      <Tab value="2">Serveis externs</Tab>
      <Tab value="3">Notes</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <TableBudgetDetails
          v-if="budget && budget.details"
          :budget="budget"
          :details="budget.details"
          @edit="
            (det: BudgetDetail) =>
              openBudgetDetailDialog(FormActionMode.EDIT, det)
          "
          @delete="deleteSalesOrderDetails"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-l text-900 font-bold"
                >Linies del pressupost</span
              >
              <section v-if="!budgetStore.order">
                <Button
                  :size="'small'"
                  label="Ponderar Costos"
                  @click="onDistributeAllCosts(budget.id)"
                  class="mr-2 dark-gray-button"
                />
                <Button
                  :size="'small'"
                  label="Afegir línea"
                  @click="
                    openBudgetDetailDialog(FormActionMode.CREATE, {} as any)
                  "
                  class="mr-2"
                />
              </section>
            </div>
          </template>
        </TableBudgetDetails>
      </TabPanel>
      <TabPanel value="1">
        <TableBudgetTransports
          v-if="budget && budget.transports"
          :budget="budget"
          :transports="budget.transports"
          @edit="
            (trans: BudgetTransport) =>
              openBudgetTransportDialog(FormActionMode.EDIT, trans)
          "
          @delete="deleteBudgetTransport"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-l text-900 font-bold"
                >Transports del pressupost</span
              >
              <section v-if="!budgetStore.order">
                <Button
                  :size="'small'"
                  label="Afegir transport"
                  @click="
                    openBudgetTransportDialog(FormActionMode.CREATE, {} as any)
                  "
                  class="mr-2"
                />
              </section>
            </div>
          </template>
        </TableBudgetTransports>
      </TabPanel>
      <TabPanel value="2">
        <TableBudgetExternalServices
          v-if="externalServicesWithSuppliers.length > 0"
          :externalServices="externalServicesWithSuppliers"
          @supplierChange="onExternalServiceSupplierChange"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-l text-900 font-bold">Serveis externs</span>
            </div>
          </template>
        </TableBudgetExternalServices>
        <p v-else class="mt-3 text-500">Sense serveis externs calculats.</p>
      </TabPanel>
      <TabPanel value="3">
        <section v-if="budget" class="mt-2">
          <div>
            <label class="block text-900 mb-2">Notes Internes</label>
            <Textarea
              class="w-full"
              rows="3"
              placeholder="Notes internes"
              v-model="budget.userNotes"
            />
          </div>
        </section>
        <section v-if="budget" class="mt-2">
          <div>
            <BaseInput
              :type="BaseInputType.TEXT"
              label="Notes automàtiques"
              id="notes"
              v-model="budget.notes"
              disabled
            />
          </div>
        </section>
      </TabPanel>
    </TabPanels>
  </Tabs>

  <Dialog
    :closable="true"
    :style="{ width: '100%' }"
    :maximizable="true"
    v-model:visible="isDetailDialogVisible"
    :header="detailDialogTitle"
    :modal="true"
    v-if="budget"
  >
    <FormBudgetOrderDetail
      v-if="budget && budgetDetail"
      :formAction="formDetailMode"
      :header="budget"
      :detail="budgetDetail"
      :readonly="false"
      @submit="onBudgetDetailSubmit"
    />
  </Dialog>
  <Dialog
    :closable="true"
    :style="{ width: '50%' }"
    :maximizable="true"
    v-model:visible="isTransportDialogVisible"
    :header="transportDialogTitle"
    :modal="true"
    v-if="budget"
  >
    <FormBudgetTransport
      v-if="budget && budgetTransport"
      :formAction="formTransportMode"
      :header="budget"
      :transport="budgetTransport"
      :customerId="budget.customerId"
      :readonly="false"
      @submit="onBudgetTransportSubmit"
    />
  </Dialog>
  <!--:readonly="budgetStore.order !== null"-->
</template>
<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import {
  Budget,
  BudgetDetail,
  BudgetTransport,
  SalesOrderDetail,
} from "../types";
import { useStore } from "../../../store";
import { BaseInputType } from "../../../types/component";
import {
  createBlobAndDownloadFile,
  getNewUuid,
} from "../../../utils/functions";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import { useReferenceStore } from "../../shared/store/reference";
import { useCustomersStore } from "../store/customers";
import { useExerciseStore } from "../../shared/store/exercise";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useTaxesStore } from "../../shared/store/tax";
import { useSuppliersStore } from "../../purchase/store/suppliers";
import { REPORTS, ReportService } from "../../../services/report.service";
import Services from "../services";
import { useWorkMasterStore } from "../../production/store/workmaster";
import { useBudgetStore } from "../store/budget";
import TableBudgetDetails from "../components/TableBudgetDetails.vue";
import FormBudget from "../components/FormBudget.vue";
import FormBudgetOrderDetail from "../components/FormBudgetOrderDetail.vue";
import TableBudgetTransports from "../components/TableBudgetTransports.vue";
import FormBudgetTransport from "../components/FormBudgetTransport.vue";
import TableBudgetExternalServices from "../components/TableBudgetExternalServices.vue";
import type { BudgetExternalServiceRow } from "../components/TableBudgetExternalServices.vue";
import { ReferenceService } from "../../shared/services/reference.service";
import { useSalesOrderStore } from "../store/order";

const referenceService = new ReferenceService("/reference");

const formMode = ref(FormActionMode.EDIT);
const budgetForm = ref();

const route = useRoute();
const router = useRouter();
const store = useStore();
const toast = useToast();
const budgetStore = useBudgetStore();
const customerStore = useCustomersStore();
const plantModelStore = usePlantModelStore();
const exerciseStore = useExerciseStore();
const lifeCycleStore = useLifecyclesStore();
const referenceStore = useReferenceStore();
const workMasterStore = useWorkMasterStore();
const taxesStore = useTaxesStore();
const salesOrderStore = useSalesOrderStore();
const supplierStore = useSuppliersStore();
const { budget } = storeToRefs(budgetStore);

export type { BudgetExternalServiceRow };
const externalServicesWithSuppliers = ref<BudgetExternalServiceRow[]>([]);

const calculatePriceForRow = async (row: BudgetExternalServiceRow): Promise<void> => {
  if (!row.supplierId) return;
  const unitPrice = await referenceService.getPriceBySupplier(row.referenceId, row.supplierId);
  if (unitPrice !== null) {
    row.unitPrice = unitPrice;
    row.totalPrice = unitPrice * row.quantity;
  }
};

const loadExternalServiceSuppliers = async () => {
  if (!budget.value?.externalServices?.length) {
    externalServicesWithSuppliers.value = [];
    return;
  }
  // Preservar les seleccions de supplier anteriors per mantenir-les al reconstruir
  const previousById = new Map<string, BudgetExternalServiceRow>(
    externalServicesWithSuppliers.value.map((r) => [r.id, r])
  );

  const rows = await Promise.all(
    budget.value.externalServices.map(async (svc) => {
      const suppliers = await supplierStore.fetchSuppliersByReference(svc.referenceId);
      const prev = previousById.get(svc.id);
      const row: BudgetExternalServiceRow = {
        ...svc,
        availableSuppliers: suppliers,
        supplierId: prev?.supplierId ?? svc.supplierId,
      };
      // Recalcular preu si ja hi ha un proveïdor seleccionat
      await calculatePriceForRow(row);
      return row;
    })
  );
  externalServicesWithSuppliers.value = rows;
};

const onExternalServiceSupplierChange = async (row: BudgetExternalServiceRow) => {
  await calculatePriceForRow(row);
  const result = await budgetStore.UpdateExternalService({
      id: row.id,
      budgetId: row.budgetId,
      referenceId: row.referenceId,
      description: row.description,
      weight: row.weight,
      volume: row.volume,
      quantity: row.quantity,
      supplierId: row.supplierId,
      unitPrice: row.unitPrice,
      totalPrice: row.totalPrice
  });
  
  if (result) {
    toast.add({
      severity: "success",
      summary: "Proveïdor actualitzat",
      detail: "S'ha desat el proveïdor per al servei extern.",
      life: 3000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "No s'ha pogut desar el proveïdor.",
      life: 3000,
    });
  }
};

watch(
  () => budget.value?.externalServices,
  () => loadExternalServiceSuppliers(),
  { deep: true }
);

const items = [
  {
    label: "Descarregar",
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(),
  },
  {
    label: "Crear comanda",
    icon: PrimeIcons.FLAG_FILL,
    command: () => createSalesOrder(),
  },
];

const detailDialogTitle = "Línia del pressupost";
const isDetailDialogVisible = ref(false);
const formDetailMode = ref(FormActionMode.EDIT);
const budgetDetail = ref(undefined as undefined | BudgetDetail);

const transportDialogTitle = "Transport del pressupost";
const isTransportDialogVisible = ref(false);
const formTransportMode = ref(FormActionMode.EDIT);
const budgetTransport = ref(undefined as undefined | BudgetTransport);

const loadView = async () => {
  const budgetId = route.params.id as string;
  await budgetStore.GetById(budgetId);
  await budgetStore.GetAssociatedSalesOrders(budgetId);
  await loadExternalServiceSuppliers();

  referenceStore.fetchReferencesByModule("sales");
  workMasterStore.fetchAllActives();
  lifeCycleStore.fetchOneByName("Budget");
  plantModelStore.fetchSites();
  exerciseStore.fetchAll();
  customerStore.fetchCustomers();
  taxesStore.fetchAll();

  let pageTitle = "";
  if (budget.value) {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Pressupost ${budget.value.number}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

onUnmounted(() => {
  budgetStore.budget = undefined;
  budgetStore.order = undefined;
});

const submitForm = () => {
  if (!budget.value?.date) {
    toast.add({
      severity: "error",
      summary: "Error al crear la comanda ",
      detail: "La data no pot estar buida",
      life: 5000,
    });
    return false;
  }
  const form = budgetForm.value as any;
  form.submitForm();
};

const openBudgetDetailDialog = (
  formMode: FormActionMode,
  detail: BudgetDetail,
) => {
  if (formMode === FormActionMode.CREATE) {
    const budgetExercise = exerciseStore.exercises?.find(
      (e) => e.id === budget.value!.exerciseId,
    );

    detail = {
      id: getNewUuid(),
      referenceId: "",
      workMasterId: null,
      profit: 0,
      productionProfit: 0,
      materialProfit: budgetExercise?.materialProfit || 30,
      externalProfit: budgetExercise?.externalProfit || 30,
      discount: 0,
      quantity: 1,
      unitCost: 0,
      productionCost: 0,
      materialCost: 0,
      unitPrice: 0,
      serviceCost: 0,
      transportCost: 0,
      totalCost: 0,
      amount: 0,
      budgetId: budget.value!.id,
      description: "",
    } as BudgetDetail;
  }

  budgetDetail.value = Object.assign({}, detail);
  formDetailMode.value = formMode;
  isDetailDialogVisible.value = true;
};

const onBudgetSubmit = async (budget: Budget) => {
  let result = false;
  let message = "";

  result = await budgetStore.Update(budget.id, budget);
  message = result
    ? "Pressupost actualitzat correctament"
    : "Error a l'actualitzar el pressupost";

  toast.add({
    life: 5000,
    severity: result ? "success" : "error",
    summary: message,
  });

  if (result) {
    router.back();
  }
};

const onBudgetDetailSubmit = async (
  detail: BudgetDetail | SalesOrderDetail,
) => {
  detail = detail as BudgetDetail;

  if (formDetailMode.value === FormActionMode.CREATE) {
    await budgetStore.CreateDetail(detail);
    //budget.value!.details!.push(detail);
  } else if (formDetailMode.value === FormActionMode.EDIT) {
    await budgetStore.UpdateDetail(detail);
    const index = budget.value!.details!.findIndex((i) => i.id === detail.id);
    budget.value!.details![index] = detail;
  }

  isDetailDialogVisible.value = false;
};

const deleteSalesOrderDetails = async (detail: BudgetDetail) => {
  if (formMode.value === FormActionMode.EDIT) {
    await budgetStore.DeleteDetail(detail);
  }
  const afterDelete = budget.value!.details!.filter((i) => i.id !== detail.id);
  budget.value!.details = afterDelete;
  isDetailDialogVisible.value = false;
};

const openBudgetTransportDialog = (
  formMode: FormActionMode,
  transport: BudgetTransport,
) => {
  if (formMode === FormActionMode.CREATE) {
    transport = {
      id: getNewUuid(),
      budgetId: budget.value!.id,
      transportRateDetailId: "",
      weight: 0,
      volume: 0,
      distance: 0,
      price: 0,
    } as BudgetTransport;
  }
  budgetTransport.value = Object.assign({}, transport);
  formTransportMode.value = formMode;
  isTransportDialogVisible.value = true;
};

const onBudgetTransportSubmit = async (transport: BudgetTransport) => {
  if (formTransportMode.value === FormActionMode.CREATE) {
    await budgetStore.CreateTransport(transport);
  } else if (formTransportMode.value === FormActionMode.EDIT) {
    await budgetStore.UpdateTransport(transport);
  }
  isTransportDialogVisible.value = false;
};

const deleteBudgetTransport = async (transport: BudgetTransport) => {
  if (formMode.value === FormActionMode.EDIT) {
    await budgetStore.DeleteTransport(transport);
  } else {
    const afterDelete = budget.value!.transports!.filter(
      (i) => i.id !== transport.id,
    );
    budget.value!.transports = afterDelete;
  }
};

const createSalesOrder = async () => {
  if (budgetStore.order) {
    toast.add({
      severity: "warn",
      summary: "Aquest pressupost ja té una comanda associada",
      life: 5000,
    });
    return;
  }

  if (budget.value) {
    const response = await salesOrderStore.CreateFromBudget(budget.value);

    if (response.result) {
      const budgetId = response.content?.budgetId;
      const createdSalesOrder = budgetId
        ? await salesOrderStore.GetFromBudgetId(budgetId)
        : undefined;
      const salesOrderId = createdSalesOrder?.id ?? response.content?.id;

      toast.add({
        severity: "success",
        summary: `Comanda ${response.content?.number} creada correctament`,
        life: 5000,
      });

      if (!salesOrderId) {
        toast.add({
          severity: "error",
          summary: "Error al obrir la comanda",
          detail: "No s'ha pogut resoldre la comanda creada",
          life: 5000,
        });
        return;
      }

      router.push(`/salesorder/${salesOrderId}`);
    } else {
      toast.add({
        severity: "error",
        summary: "Error al crear la comanda ",
        detail: response.errors[0],
        life: 5000,
      });
    }
  }
};

const onDistributeAllCosts = async (budgetId: string) => {
  const result = await budgetStore.DistributeAllCosts(budgetId);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Costos ponderats",
      detail:
        "S'han ponderat els costos de transport i serveis externs correctament entre els detalls.",
      life: 5000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: "Error al ponderar",
      detail:
        "No s'han pogut ponderar els costos (és possible que hi hagi un error al servidor).",
      life: 5000,
    });
  }
};

const printInvoice = async () => {
  const budgetReport = await Services.Budget.GetReportDataById(
    budget.value!.id,
  );

  if (budgetReport) {
    const fileName = `Pressupost_${budget.value?.number}.docx`;

    const reportService = new ReportService();
    const report = await reportService.Download(
      budgetReport,
      REPORTS.Budget,
      fileName,
    );

    if (report) {
      createBlobAndDownloadFile(fileName, report);
    } else {
      toast.add({
        severity: "warn",
        summary: "Error",
        detail: "No s'ha pugut generar fulla del pressupost",
      });
    }
  }
};
</script>
