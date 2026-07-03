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
      <Tab value="1">Transports</Tab>
      <Tab value="2">Serveis Externs</Tab>
      <Tab value="3">Fitxers</Tab>
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
                  label="Ponderar Costos"
                  @click="onDistributeAllCosts(salesOrder.id)"
                  class="mr-2 dark-gray-button"
                />
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
        <TableSalesOrderTransports
          v-if="salesOrder && salesOrder.transports"
          :salesOrder="salesOrder"
          :transports="salesOrder.transports"
          @edit="
            (trans: SalesOrderTransport) =>
              openSalesOrderTransportDialog(FormActionMode.EDIT, trans)
          "
          @delete="deleteSalesOrderTransport"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-l text-900 font-bold"
                >Transports de la comanda</span
              >
              <section v-if="!deliveryNoteStore.deliveryNote">
                <Button
                  :size="'small'"
                  label="Afegir transport"
                  @click="
                    openSalesOrderTransportDialog(FormActionMode.CREATE, {} as any)
                  "
                  class="mr-2"
                />
              </section>
            </div>
          </template>
        </TableSalesOrderTransports>
      </TabPanel>
      <TabPanel value="2">
        <TableSalesOrderExternalServices
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
        </TableSalesOrderExternalServices>
        <p v-else class="mt-3 text-500">Sense serveis externs calculats.</p>
      </TabPanel>
      <TabPanel value="3">
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
  <Dialog
    :closable="true"
    :style="{ width: '50%' }"
    :maximizable="true"
    v-model:visible="isTransportDialogVisible"
    :header="transportDialogTitle"
    :modal="true"
    v-if="salesOrder"
  >
    <FormSalesOrderTransport
      v-if="salesOrder && salesOrderTransport"
      :formAction="formTransportMode"
      :header="salesOrder"
      :transport="salesOrderTransport"
      :customerId="salesOrder.customerId"
      :readonly="false"
      @submit="onSalesOrderTransportSubmit"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { onUnmounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import {
  BudgetDetail,
  CreateWorkOrderFromSalesOrderDto,
  SalesOrderDetail,
  SalesOrderHeader,
  SalesOrderTransport,
} from "../types";
import { useStore } from "../../../store";
import {
  createBlobAndDownloadFile,
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
import { useBudgetStore } from "../store/budget";
import TableSalesOrderTransports from "../components/TableSalesOrderTransports.vue";
import FormSalesOrderTransport from "../components/FormSalesOrderTransport.vue";
import TableSalesOrderExternalServices from "../components/TableSalesOrderExternalServices.vue";
import type { SalesOrderExternalServiceRow } from "../components/TableSalesOrderExternalServices.vue";
import { ReferenceService } from "../../shared/services/reference.service";
import { useSuppliersStore } from "../../purchase/store/suppliers";

const referenceService = new ReferenceService("/reference");
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
const workOrderStore = useWorkOrderStore();
const taxesStore = useTaxesStore();
const budgetStore = useBudgetStore();
const supplierStore = useSuppliersStore();
const { salesOrder } = storeToRefs(salesOrderStore);

export type { SalesOrderExternalServiceRow };
const externalServicesWithSuppliers = ref<SalesOrderExternalServiceRow[]>([]);

const calculatePriceForRow = async (row: SalesOrderExternalServiceRow): Promise<void> => {
  if (!row.supplierId) return;
  const rateInfo = await referenceService.getRateInfo(row.referenceId, row.supplierId);
  if (rateInfo !== null) {
    row.unitPrice = rateInfo.unitPrice;
    // 0 = Volum, 1 = Pes, 2 = Unitats (default)
    const magnitude =
      rateInfo.calculationType === 0 ? row.volume :
      rateInfo.calculationType === 1 ? row.weight :
      row.quantity;
    row.totalPrice = rateInfo.unitPrice * magnitude;
  }
};

const loadExternalServiceSuppliers = async () => {
  if (!salesOrder.value?.externalServices?.length) {
    externalServicesWithSuppliers.value = [];
    return;
  }
  const previousById = new Map<string, SalesOrderExternalServiceRow>(
    externalServicesWithSuppliers.value.map((r) => [r.id, r])
  );

  const rows = await Promise.all(
    salesOrder.value.externalServices.map(async (svc) => {
      const suppliers = await supplierStore.fetchSuppliersByReference(svc.referenceId);
      const prev = previousById.get(svc.id);
      const row: SalesOrderExternalServiceRow = {
        ...svc,
        availableSuppliers: suppliers,
        supplierId: prev?.supplierId ?? svc.supplierId,
      };
      await calculatePriceForRow(row);
      
      // Auto-save if the locally calculated total price is different from the DB
      if (row.supplierId && Math.abs(row.totalPrice - svc.totalPrice) > 0.001) {
        services.SalesOrder.UpdateExternalService({
          id: row.id,
          salesOrderHeaderId: row.salesOrderHeaderId,
          referenceId: row.referenceId,
          description: row.description,
          weight: row.weight,
          volume: row.volume,
          quantity: row.quantity,
          supplierId: row.supplierId,
          unitPrice: row.unitPrice,
          totalPrice: row.totalPrice
        });
      }
      
      return row;
    })
  );
  externalServicesWithSuppliers.value = rows;
};

const onExternalServiceSupplierChange = async (row: SalesOrderExternalServiceRow) => {
  await calculatePriceForRow(row);
  const result = await salesOrderStore.UpdateExternalService({
      id: row.id,
      salesOrderHeaderId: row.salesOrderHeaderId,
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
  () => salesOrder.value?.externalServices,
  () => loadExternalServiceSuppliers(),
  { deep: true }
);

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
  {
    label: "Crear albarà",
    icon: PrimeIcons.TRUCK,
    command: () => createDeliveryNote(),
  },
];

const detailDialogTitle = "Línia de comanda";
const isDetailDialogVisible = ref(false);
const formDetailMode = ref(FormActionMode.EDIT);
const selectedSalesOrderDetail = ref(undefined as undefined | SalesOrderDetail);

const transportDialogTitle = "Transport de comanda";
const isTransportDialogVisible = ref(false);
const formTransportMode = ref(FormActionMode.EDIT);
const salesOrderTransport = ref(undefined as undefined | SalesOrderTransport);

const loadView = async (salesOrderId: string) => {
  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: "Comanda",
  });

  budgetStore.budget = undefined;
  deliveryNoteStore.deliveryNote = undefined;

  await salesOrderStore.GetById(salesOrderId);
  await loadExternalServiceSuppliers();

  await referenceStore.fetchReferences();
  referenceStore.module = "sales";
  lifeCycleStore.fetchOneByName("SalesOrder");
  lifeCycleStore.fetchSecondaryByName("WorkOrder");
  plantModelStore.fetchSites();
  exerciseStore.fetchAll();
  customerStore.fetchCustomers();
  taxesStore.fetchAll();
  workOrderStore.fetchBySalesOrder(salesOrderId);

  let pageTitle = "Comanda";
  if (salesOrder.value) {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Comanda ${salesOrder.value.number}`;

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

watch(
  () => route.params.id,
  async (salesOrderId) => {
    if (typeof salesOrderId === "string" && salesOrderId) {
      await loadView(salesOrderId);
    }
  },
  { immediate: true },
);

onUnmounted(() => {
  salesOrderStore.salesOrder = undefined;
  salesOrderStore.salesOrders = undefined;
  salesOrderStore.salesOrdersToDeliver = undefined;
  deliveryNoteStore.deliveryNote = undefined;
  workOrderStore.workorders = undefined;
  externalServicesWithSuppliers.value = [];
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

const createDeliveryNote = async () => {
  if (!salesOrder.value) return;

  if (salesOrder.value.deliveryNoteId) {
    toast.add({
      severity: "warn",
      summary: "Aquesta comanda ja té un albarà associat",
      life: 5000,
    });
    return;
  }

  const response = await deliveryNoteStore.CreateFromSalesOrder(salesOrder.value);

  if (response.result && response.content?.id) {
    toast.add({
      severity: "success",
      summary: `Albarà ${response.content.number} creat correctament`,
      life: 5000,
    });

    router.push(`/deliverynote/${response.content.id}`);
    return;
  }

  toast.add({
    severity: "error",
    summary: "Error al crear l'albarà",
    detail: response.errors[0],
    life: 5000,
  });
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

const openSalesOrderTransportDialog = (
  formMode: FormActionMode,
  transport: SalesOrderTransport,
) => {
  if (formMode === FormActionMode.CREATE) {
    transport = {
      id: getNewUuid(),
      salesOrderHeaderId: salesOrder.value!.id,
      transportRateDetailId: "",
      weight: 0,
      volume: 0,
      distance: 0,
      price: 0,
    } as SalesOrderTransport;
  }
  salesOrderTransport.value = Object.assign({}, transport);
  formTransportMode.value = formMode;
  isTransportDialogVisible.value = true;
};

const onSalesOrderTransportSubmit = async (transport: SalesOrderTransport) => {
  if (formTransportMode.value === FormActionMode.CREATE) {
    await salesOrderStore.CreateTransport(transport);
  } else if (formTransportMode.value === FormActionMode.EDIT) {
    await salesOrderStore.UpdateTransport(transport);
  }
  isTransportDialogVisible.value = false;
};

const deleteSalesOrderTransport = async (transport: SalesOrderTransport) => {
  if (formMode.value === FormActionMode.EDIT) {
    await salesOrderStore.DeleteTransport(transport.id, salesOrder.value!.id);
  } else {
    const afterDelete = salesOrder.value!.transports!.filter(
      (i) => i.id !== transport.id,
    );
    salesOrder.value!.transports = afterDelete;
  }
};

const onDistributeAllCosts = async (salesOrderId: string) => {
  const result = await salesOrderStore.DistributeAllCosts(salesOrderId);
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
</script>
