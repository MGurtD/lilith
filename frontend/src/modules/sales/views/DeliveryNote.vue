<template>
  <SplitButton
    :label="t('sales.detail.actions.save')"
    @click="submitForm"
    :model="items"
    :size="'small'"
    class="grid_add_row_button"
  />

  <FormDeliveryNote
    v-if="deliveryNote"
    class="mt-3 mb-3"
    ref="deliveryNoteForm"
    :deliveryNote="deliveryNote"
    @submit="onDeliveryNoteSubmit"
  />

  <TableDeliveryNoteDetails
    v-if="deliveryNote"
    :orders="salesOrderStore.salesOrders"
    :canDelete="canModifyDetails"
    @delete="deleteSalesOrder"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-l text-900 font-bold">{{ t("sales.detail.labels.deliveryNoteLines") }}</span>
        <div>
          <Button
            :disabled="!canModifyDetails || isAddingOrders"
            :size="'small'"
            :label="t('sales.detail.actions.createOrder')"
            @click="openSalesOrderSelector()"
          />
        </div>
      </div>
    </template>
  </TableDeliveryNoteDetails>

  <Dialog
    :closable="true"
    v-model:visible="isDialogVisible"
    :header="dialogTitle"
    :modal="true"
    :style="{ width: '50rem' }"
    :breakpoints="{ '1199px': '75vw', '575px': '90vw' }"
  >
    <SelectorOrders
      :orders="salesOrderStore.salesOrdersToDeliver"
      @selected="addSalesOrdersToDeliveryNote"
    />
  </Dialog>
</template>
<script setup lang="ts">
import SelectorOrders from "../components/SelectorOrders.vue";
import FormDeliveryNote from "../components/FormDeliveryNote.vue";
import TableDeliveryNoteDetails from "../components/TableDeliveryNoteDetails.vue";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import { useReferenceStore } from "../../shared/store/reference";
import { useCustomersStore } from "../store/customers";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { useDeliveryNoteStore } from "../store/deliveryNote";
import { DeliveryNote, SalesOrderHeader } from "../types";
import {
  createBlobAndDownloadFile,
  formatDate,
} from "../../../utils/functions";
import { useSalesOrderStore } from "../store/order";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import Services from "../services";
import { REPORTS, ReportService } from "../../../services/report.service";
import { useI18n } from "vue-i18n";

const deliveryNoteForm = ref();

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const router = useRouter();

const store = useStore();
const deliveryNoteStore = useDeliveryNoteStore();
const salesOrderStore = useSalesOrderStore();
const customerStore = useCustomersStore();
const plantModelStore = usePlantModelStore();
const referenceStore = useReferenceStore();
const lifecycleStore = useLifecyclesStore();
const { deliveryNote } = storeToRefs(deliveryNoteStore);
const { t } = useI18n();

const items = computed(() => [
  {
    label: t("sales.detail.actions.download"),
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(true),
  },
  {
    label: t("sales.detail.actions.printPdf"),
    icon: PrimeIcons.FILE_PDF,
    command: () => printPdf(),
  },
  {
    label: t("sales.detail.actions.downloadWithoutPrice"),
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(false),
  },
]);

const dialogTitle = computed(() => t("sales.detail.dialogs.orderSelector"));
const isDialogVisible = ref(false);
const initialStatusId = ref("");

const canModifyDetails = computed(() => {
  if (!lifecycleStore.lifecycle) return false;
  if (!lifecycleStore.lifecycle.statuses) return false;

  // When delivery note is associated to an invoice, could not modify it's detail
  if (
    deliveryNote.value &&
    deliveryNote.value.salesInvoiceId &&
    deliveryNote.value.salesInvoiceId !== null
  )
    return false;

  const deliveredStatus = lifecycleStore.lifecycle.statuses.find(
    (s) => s.name === "Entregat"
  );
  if (!deliveredStatus) return false;

  return deliveredStatus.id !== initialStatusId.value;
});

const loadView = async () => {
  const id = route.params.id as string;
  await deliveryNoteStore.GetById(id);
  initialStatusId.value = deliveryNoteStore.deliveryNote!.statusId;
  await salesOrderStore.GetByDeliveryNote(id);
  await referenceStore.fetchReferences();
  referenceStore.module = "sales";
  plantModelStore.fetchSites();

  if (!customerStore.customers) customerStore.fetchCustomers();
  if (!lifecycleStore.lifecycle) lifecycleStore.fetchOneByName("DeliveryNote");

  let pageTitle = "";
  if (deliveryNote.value) {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `${t("sales.deliveryNotes.title")} ${deliveryNote.value.number}`;

    if (deliveryNote.value.deliveryDate) {
      deliveryNote.value.deliveryDate = formatDate(
        deliveryNote.value.deliveryDate
      );
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
  deliveryNoteStore.deliveryNote = undefined;
  salesOrderStore.salesOrders = undefined;
  salesOrderStore.salesOrdersToDeliver = undefined;
});

const submitForm = () => {
  if (!deliveryNote.value?.createdOn) {
    toast.add({
      severity: "error",
      summary: t("sales.detail.messages.error"),
      detail: t("sales.detail.messages.dateRequired"),
      life: 5000,
    });
    return false;
  }
  const form = deliveryNoteForm.value as any;
  form.submitForm();
};

const toast = useToast();

const onDeliveryNoteSubmit = async (deliveryNote: DeliveryNote) => {
  let result = false;
  let message = "";

  result = await deliveryNoteStore.Update(deliveryNote.id, deliveryNote);
  message = result ? t("sales.detail.messages.updated") : t("sales.deliveryNotes.messages.createError");

  toast.add({
    life: 5000,
    severity: result ? "success" : "error",
    summary: message,
  });

  if (result) {
    router.back();
  }
};

const isAddingOrders = ref(false);

const openSalesOrderSelector = async () => {
  if (deliveryNote.value)
    await salesOrderStore.GetToDeliver(deliveryNote.value.customerId);

  isDialogVisible.value = true;
};

const addSalesOrdersToDeliveryNote = async (
  orders: Array<SalesOrderHeader>
) => {
  isAddingOrders.value = true;
  isDialogVisible.value = false;

  try {
    for (const order of orders) {
      const response = await deliveryNoteStore.AddOrder(
        deliveryNote.value!.id,
        order
      );
      if (!response.result) {
        toast.add({
          severity: "error",
          summary: t("sales.detail.messages.orderAddError"),
          detail:
            typeof response.content === "string"
              ? response.content
              : t("sales.detail.messages.orderAddFailed"),
          life: 6000,
        });
        continue;
      }

      salesOrderStore.salesOrdersToDeliver =
        salesOrderStore.salesOrdersToDeliver?.filter(
          (o) => o.id !== order.id
        );
    }

    await salesOrderStore.GetByDeliveryNote(deliveryNote.value!.id);
  } finally {
    isAddingOrders.value = false;
  }
};

const deleteSalesOrder = async (order: SalesOrderHeader) => {
  await deliveryNoteStore.DeleteOrder(deliveryNote.value!.id, order);
  await salesOrderStore.GetByDeliveryNote(deliveryNote.value!.id);
};

const printInvoice = async (showPrices: boolean) => {
  const deliveryNoteReport = await Services.DeliveryNote.GetReportDataById(
    deliveryNote.value!.id,
    showPrices
  );

  if (deliveryNoteReport) {
    const fileName = `AlbaraEntrega_${deliveryNote.value?.number}.docx`;

    const reportService = new ReportService();
    const report = await reportService.Download(
      deliveryNoteReport,
      REPORTS.DeliveryNote,
      fileName
    );

    if (report) {
      createBlobAndDownloadFile(fileName, report);
    } else {
      toast.add({
        severity: "warn",
        summary: t("sales.detail.messages.error"),
        detail: t("sales.detail.messages.reportError"),
      });
    }
  }
};
const printPdf = async () => {
  const report = await Services.DeliveryNote.DownloadPdf(deliveryNote.value!.id, true);
  if (report) createBlobAndDownloadFile(`Albara_${deliveryNote.value?.number}.pdf`, report);
};

</script>
