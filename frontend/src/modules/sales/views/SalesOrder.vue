<template>
  <SplitButton
    label="Guardar"
    @click="submitForm"
    :model="items"
    :size="'small'"
    class="grid_add_row_button"
  />

  <FormSalesOrder
    class="mt-3 mb-3"
    ref="salesOrderForm"
    salesOrder="salesOrder"
    @submit="onOrderSubmit"
  />

  <Tabs value="0">
    <TabList>
      <Tab value="0">Detall</Tab>
      <Tab value="1">Fitxers</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <TableSalesOrderDetails
          v-if="salesOrder"
          :salesOrder="salesOrder"
          :salesOrderDetails="salesOrder.salesOrderDetails"
          :secondaryLifecycle="lifeCycleStore.secondaryLifecycle"
          :workorders="workOrderStore.workorders"
          @edit="
            (det: SalesOrderDetail) =>
              openOrderDetailDialog(FormActionMode.EDIT, det)
          "
          @delete="deleteOrderDetail"
          @createWorkOrder="createWorkOrder"
          @openWorkOrder="openWorkOrder"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-l text-900 font-bold"
                >Linies de la comanda</span
              >
              <section v-if="!deliveryNoteStore.deliveryNote">
                <Button
                  :size="'small'"
                  label="Afegir línea"
                  @click="
                    openOrderDetailDialog(FormActionMode.CREATE, {} as any)
                  "
                  class="mr-2"
                />
              </section>
            </div>
          </template>
        </TableSalesOrderDetails>
      </TabPanel>
      <TabPanel value="1">
        <FileEntityPicker
          v-if="salesOrder"
          entity="SalesOrder"
          :id="salesOrder.id"
          title=""
        />
      </TabPanel>
    </TabPanels>
  </Tabs>

  <Dialog
    v-if="salesOrder"
    :style="{ width: '100%' }"
    :maximizable="true"
    :closable="true"
    v-model:visible="isDetailDialogVisible"
    :header="detailDialogTitle"
    :modal="true"
  >
    <FormBudgetOrderDetail
      v-if="selectedSalesOrderDetail"
      :formAction="formDetailMode"
      :header="salesOrder"
      :detail="selectedSalesOrderDetail"
      @submit="onOrderDetailSubmit"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import {
  BudgetDetail,
  CreateWorkOrderFromSalesOrderDto,
  SalesOrderDetail,
  SalesOrderHeader,
} from "../types";
import { useStore } from "../../../store";
import {
  createBlobAndDownloadFile,
  formatDate,
  getNewUuid,
} from "../../../utils/functions";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import { useSalesOrderStore } from "../store/order";
import { useReferenceStore } from "../../shared/store/reference";
import { useCustomersStore } from "../store/customers";
import { useExerciseStore } from "../../shared/store/exercise";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useTaxesStore } from "../../shared/store/tax";
import FormSalesOrder from "../components/FormSalesOrder.vue";
import FormBudgetOrderDetail from "../components/FormBudgetOrderDetail.vue";
import TableSalesOrderDetails from "../components/TableSalesOrderDetails.vue";
import FileEntityPicker from "../../../components/FileEntityPicker.vue";
import { useDeliveryNoteStore } from "../store/deliveryNote";
import { REPORTS, ReportService } from "../../../services/report.service";
import services from "../services";
import { useWorkOrderStore } from "../../production/store/workorder";
import { useWorkMasterStore } from "../../production/store/workmaster";
import { useBudgetStore } from "../store/budget";

const salesOrderForm = ref();

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const router = useRouter();
const store = useStore();
const toast = useToast();
const salesOrderStore = useSalesOrderStore();
const customerStore = useCustomersStore();
const plantModelStore = usePlantModelStore();
const exerciseStore = useExerciseStore();
const lifeCycleStore = useLifecyclesStore();
const referenceStore = useReferenceStore();
const deliveryNoteStore = useDeliveryNoteStore();
const workMasterStore = useWorkMasterStore();
const workOrderStore = useWorkOrderStore();
const taxesStore = useTaxesStore();
const budgetStore = useBudgetStore();
const { salesOrder } = storeToRefs(salesOrderStore);

const items = [
  {
    label: "Descarregar",
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(true),
  },
  {
    label: "Descarregar sense preu",
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(false),
  },
];

const detailDialogTitle = "Línia de comanda";
const isDetailDialogVisible = ref(false);
const formDetailMode = ref(FormActionMode.EDIT);
const selectedSalesOrderDetail = ref(undefined as undefined | SalesOrderDetail);

const loadView = async () => {
  const workOrderId = route.params.id as string;
  await salesOrderStore.GetById(workOrderId);
  referenceStore.fetchReferencesByModule("sales");
  lifeCycleStore.fetchOneByName("SalesOrder");
  lifeCycleStore.fetchSecondaryByName("WorkOrder");
  plantModelStore.fetchSites();
  exerciseStore.fetchAll();
  customerStore.fetchCustomers();
  taxesStore.fetchAll();
  workOrderStore.fetchBySalesOrder(workOrderId);

  if (!workMasterStore.workmasters) {
    workMasterStore.fetchAllActives();
  }

  let pageTitle = "";
  if (salesOrder.value) {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Comanda ${salesOrder.value.number}`;

    // Parse date on the form
    salesOrder.value.date = formatDate(salesOrder.value.date);
    if (salesOrder.value.expectedDate) {
      salesOrder.value.expectedDate = formatDate(salesOrder.value.expectedDate);
    }

    // Get the related DeliveryNote info
    if (salesOrder.value.deliveryNoteId) {
      deliveryNoteStore.GetById(salesOrder.value.deliveryNoteId);
    } else if (deliveryNoteStore.deliveryNote) {
      deliveryNoteStore.deliveryNote = undefined;
    }

    // Get related budget
    if (salesOrder.value.budgetId) {
      await budgetStore.GetById(salesOrder.value.budgetId);
    } else {
      budgetStore.budget = undefined;
    }
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
  salesOrderStore.salesOrder = undefined;
  salesOrderStore.salesOrders = undefined;
  salesOrderStore.salesOrdersToDeliver = undefined;
  deliveryNoteStore.deliveryNote = undefined;
  workOrderStore.workorders = undefined;
});

const submitForm = () => {
  const form = salesOrderForm.value as any;
  form.submitForm();
};

const openOrderDetailDialog = (
  formMode: FormActionMode,
  salesOrderDetail: SalesOrderDetail,
) => {
  if (formMode === FormActionMode.CREATE) {
    const orderExercise = exerciseStore.exercises?.find(
      (e) => e.id === salesOrder.value!.exerciseId,
    );

    salesOrderDetail = {
      id: getNewUuid(),
      referenceId: "",
      quantity: 1,
      profit: 0,
      productionProfit: 0,
      materialProfit: orderExercise?.materialProfit || 30,
      externalProfit: orderExercise?.externalProfit || 30,
      discount: 0,
      unitCost: 0,
      serviceCost: 0,
      transportCost: 0,
      productionCost: 0,
      materialCost: 0,
      unitPrice: 0,
      totalCost: 0,
      amount: 0,
      salesOrderHeaderId: "",
      lastCost: 0,
      workMasterCost: 0,
      description: "",
      isDelivered: false,
      isInvoiced: false,
      workMasterId: null,
      workOrderId: null,
      userNotes: "",
    } as SalesOrderDetail;
  }

  salesOrderDetail.salesOrderHeaderId = salesOrder.value!.id;
  selectedSalesOrderDetail.value = Object.assign({}, salesOrderDetail);
  formDetailMode.value = formMode;
  isDetailDialogVisible.value = true;
};

const onOrderSubmit = async (salesOrder: SalesOrderHeader) => {
  if (!salesOrder.date) {
    toast.add({
      severity: "error",
      summary: "Error al crear la comanda ",
      detail: "La data no pot estar buida",
      life: 5000,
    });
    return false;
  }

  let result = false;
  let message = "";

  result = await salesOrderStore.Update(salesOrder.id, salesOrder);
  message = result
    ? "Comanda actualitzada"
    : "Error a l'actualitzar la comanda";

  toast.add({
    life: 5000,
    severity: result ? "success" : "error",
    summary: message,
  });

  if (result) {
    router.back();
  }
};

const onOrderDetailSubmit = async (detail: BudgetDetail | SalesOrderDetail) => {
  detail = detail as SalesOrderDetail;

  if (formDetailMode.value === FormActionMode.CREATE) {
    await salesOrderStore.CreateDetail(detail);
  } else if (formDetailMode.value === FormActionMode.EDIT) {
    await salesOrderStore.UpdateDetail(detail);
    const index = salesOrder.value!.salesOrderDetails!.findIndex(
      (i) => i.id === detail.id,
    );
    salesOrder.value!.salesOrderDetails![index] = detail;
  }
  isDetailDialogVisible.value = false;
};

const deleteOrderDetail = async (detail: SalesOrderDetail) => {
  if (formMode.value === FormActionMode.EDIT) {
    await salesOrderStore.DeleteDetail(detail);
  }
  const afterDelete = salesOrder.value!.salesOrderDetails!.filter(
    (i) => i.id !== detail.id,
  );
  salesOrder.value!.salesOrderDetails = afterDelete;
  isDetailDialogVisible.value = false;
};

const createWorkOrder = async (dto: CreateWorkOrderFromSalesOrderDto) => {
  const response = await workOrderStore.create(dto.workOrderDto);
  if (response.result) {
    dto.orderDetail.workOrderId = response.content!.id;

    const updated = await salesOrderStore.UpdateDetail(dto.orderDetail);
    if (updated) {
      toast.add({
        severity: "success",
        summary: "Generació OF",
        detail: `Ordre de fabricació ${response.content!.code} generada`,
        life: 5000,
      });

      workOrderStore.fetchBySalesOrder(salesOrder.value!.id);
    }
  } else {
    toast.add({
      severity: "error",
      summary: "Generació OF",
      detail: `Error al generar la ordre de fabricació`,
      life: 5000,
    });
  }
};

const openWorkOrder = (workorderid: string) => {
  router.push({ path: `/workorder/${workorderid}` });
};

const printInvoice = async (showPrices: boolean) => {
  const orderReport = await services.SalesOrder.GetReportDataById(
    salesOrder.value!.id,
    showPrices,
  );

  if (orderReport) {
    const fileName = `Comanda_${salesOrder.value?.number}.docx`;

    const reportService = new ReportService();
    const report = await reportService.Download(
      orderReport,
      REPORTS.Order,
      fileName,
    );

    if (report) {
      createBlobAndDownloadFile(fileName, report);
    } else {
      toast.add({
        severity: "warn",
        summary: "Error",
        detail: "No s'ha pugut generar fulla de la comanda",
      });
    }
  }
};
</script>
