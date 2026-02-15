<template>
  <main v-if="order">
    <SplitButton
      label="Guardar"
      @click="submitForm"
      :model="items"
      :size="'small'"
      class="grid_add_row_button"
    />

    <FormOrder class="pt-3" ref="orderForm" @submit="onOrderSubmit" />
    <br />
    <TableOrderDetails
      v-if="order.details"
      :details="order.details"
      @edit="openEditDetailForm"
      @delete="removeDetail"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-900 font-bold">Detall de la comanda</span>
          <div>
            <Button
              :size="'small'"
              :icon="PrimeIcons.PLUS"
              rounded
              @click="openCreateDetailForm"
            />
          </div>
        </div>
      </template>
    </TableOrderDetails>

    <Dialog
      v-model:visible="dialogOptions.visible"
      :header="dialogOptions.title"
      :closable="dialogOptions.closable"
      :modal="dialogOptions.modal"
    >
      <FormOrderDetail
        :detail="selectedDetail"
        :order="order"
        @submit="submitDetailForm"
      />
    </Dialog>
  </main>
  <main v-else>Carregant comanda ...</main>
</template>
<script setup lang="ts">
import FormOrder from "../components/FormOrder.vue";
import TableOrderDetails from "../components/TableOrderDetails.vue";
import FormOrderDetail from "../components/FormOrderDetail.vue";
import { onMounted, reactive, ref } from "vue";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { PurchaseOrderDetail } from "../types";
import { GenericResponse } from "../../../types";
import { ReferenceCategoryEnum } from "../../shared/types";
import {
  createBlobAndDownloadFile,
  formatDate,
  getNewUuid,
} from "../../../utils/functions";
import { DialogOptions, FormActionMode } from "../../../types/component";
import { useRouter, useRoute } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useStore } from "../../../store";
import { useReferenceStore } from "../../shared/store/reference";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useReferenceTypeStore } from "../../shared/store/referenceType";
import { useOrderStore } from "../store/order";
import { useSuppliersStore } from "../store/suppliers";
import { useExerciseStore } from "../../shared/store/exercise";
import Services from "../services";
import { REPORTS, ReportService } from "../../../services/report.service";

const router = useRouter();
const route = useRoute();
const store = useStore();
const orderForm = ref();
const confirm = useConfirm();
const referenceStore = useReferenceStore();
const referenceTypeStore = useReferenceTypeStore();
const orderStore = useOrderStore();
const suppliersStore = useSuppliersStore();
const exerciseStore = useExerciseStore();
const lifecycleStore = useLifecyclesStore();
const { order } = storeToRefs(orderStore);

const items = [
  {
    label: "Descarregar",
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(),
  },
];

const loadView = async () => {
  await orderStore.fetchOne(route.params.id as string);

  if (!lifecycleStore.lifecycle) lifecycleStore.fetchOneByName("PurchaseOrder");
  if (!exerciseStore.exercises?.length) exerciseStore.fetchActive();
  if (!suppliersStore.suppliers) suppliersStore.fetchSuppliers();
  if (!referenceTypeStore.referenceTypes) referenceTypeStore.fetchActive();
  if (!referenceStore.references || referenceStore.module !== "purchase")
    referenceStore.fetchReferencesByModule("purchase");

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    title: `Comanda de compra ${order.value?.number}`,
    backButtonVisible: true,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();

const submitForm = async () => {
  if (!order.value?.date) {
    toast.add({
      severity: "error",
      summary: "Error al crear la comanda ",
      detail: "La data no pot estar buida",
      life: 5000,
    });
    return false;
  }

  const form = orderForm.value as any;
  form.submitForm();
};

const onOrderSubmit = async () => {
  let result = false;
  let message = "";
  if (order.value) {
    result = await orderStore.update(order.value.id, order.value);
    message = "Comanda actualizada correctament";

    if (result) {
      toast.add({
        severity: "success",
        summary: message,
        life: 5000,
      });
      router.back();
    }
  }
};

const dialogOptions = reactive({
  visible: false,
  title: "Linea",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);
const formAction = ref(FormActionMode.CREATE);
const selectedDetail = ref({} as PurchaseOrderDetail);

const openCreateDetailForm = () => {
  formAction.value = FormActionMode.CREATE;
  selectedDetail.value = {
    id: getNewUuid(),
    purchaseOrderId: order.value!.id,
    amount: 0,
    unitPrice: 0,
    quantity: 0,
    receivedQuantity: 0,
    referenceId: "",
    statusId: lifecycleStore.lifecycle?.initialStatusId || "",
    expectedReceiptDate: null,
    workOrderPhaseId: null,
    disabled: false,
  } as PurchaseOrderDetail;

  dialogOptions.title = "Crear línia";
  dialogOptions.visible = true;
};

const openEditDetailForm = (detail: PurchaseOrderDetail) => {
  formAction.value = FormActionMode.EDIT;
  selectedDetail.value = detail;
  referenceStore.setNewReference(getNewUuid(), ReferenceCategoryEnum.MATERIAL);

  dialogOptions.title = "Modificar línia";
  dialogOptions.visible = true;
};

// Order Detail
const submitDetailForm = async (detail: PurchaseOrderDetail) => {
  if (formAction.value === FormActionMode.CREATE) {
    await addDetail(detail);
  } else if (formAction.value === FormActionMode.EDIT) {
    await editDetail(detail);
  }

  dialogOptions.visible = false;
  await orderStore.fetchOne(route.params.id as string);
};

const addDetail = async (detail: PurchaseOrderDetail) => {
  const response = await orderStore.createDetail(detail);
  if (!response.result) showResponseErrorToast(response);
};

const editDetail = async (detail: PurchaseOrderDetail) => {
  const response = await orderStore.updateDetail(detail.id, detail);
  if (!response.result) showResponseErrorToast(response);
};

const removeDetail = async (detail: PurchaseOrderDetail) => {
  confirm.require({
    message: `Está segur que vols la línia?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const response = await orderStore.deleteDetail(detail.id, detail);
      if (!response.result) showResponseErrorToast(response);
    },
  });
};

const showResponseErrorToast = (
  response: GenericResponse<PurchaseOrderDetail>,
) => {
  toast.add({
    severity: "error",
    summary: response.errors[0],
    life: 10000,
    closable: true,
  });
};

const printInvoice = async () => {
  const orderReport = await Services.Order.GetReportDataById(order.value!.id);

  if (orderReport) {
    const fileName = `ComandaCompra_${order.value?.number}.docx`;

    const reportService = new ReportService();
    const report = await reportService.Download(
      orderReport,
      REPORTS.PurchaseOrder,
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
