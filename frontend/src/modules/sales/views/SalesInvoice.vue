<template>
  <main v-if="invoice">
    <FormSalesInvoice
      class="mt-3 mr-3"
      :invoice="invoice"
      :saving="saving"
      @submit="updateInvoice"
      @download="printInvoice"
      @download-pdf="printInvoicePdf"
      @rectificative="requestRectificativeQuantity"
    />

    <section class="invoice-totals-section mt-3">
      <div class="invoice-totals-grid">
        <article class="total-card">
          <div class="total-card-icon">
            <i class="pi pi-wallet" />
          </div>
          <div class="total-card-content">
            <span class="total-card-label">{{ t("sales.detail.labels.taxableBase") }}</span>
            <span class="total-card-value">{{
              formatCurrency(invoice.baseAmount)
            }}</span>
          </div>
        </article>

        <article class="total-card">
          <div class="total-card-icon">
            <i class="pi pi-percentage" />
          </div>
          <div class="total-card-content">
            <span class="total-card-label">{{ t("sales.detail.labels.taxes") }}</span>
            <span class="total-card-value">{{
              formatCurrency(invoice.taxAmount)
            }}</span>
          </div>
        </article>

        <article class="total-card total-card-total">
          <div class="total-card-icon">
            <i class="pi pi-calculator" />
          </div>
          <div class="total-card-content">
            <span class="total-card-label">{{ t("sales.detail.labels.invoiceTotal") }}</span>
            <span class="total-card-value">{{
              formatCurrency(invoice.netAmount)
            }}</span>
          </div>
        </article>

        <article
          v-if="invoice.integrationStatusId !== null"
          class="total-card"
        >
          <div class="total-card-icon">
            <i class="pi pi-verified" />
          </div>
          <div class="total-card-content">
            <span class="total-card-label">{{ t("sales.detail.labels.verifactu") }}</span>
            <span
              class="total-card-value"
              :class="getVerifactuStatusClass()"
              >{{
                invoiceStore.getVerifactuStatusById(
                  invoice.integrationStatusId,
                )
              }}</span
            >
          </div>
        </article>
      </div>
    </section>

    <Tabs v-model:value="activeTab" class="mt-3">
      <TabList>
        <Tab value="0">
          <i :class="PrimeIcons.LIST" class="mr-2"></i>
          <span>{{ t("sales.detail.labels.invoiceDetails") }}</span>
        </Tab>
        <Tab v-if="canEditCustomerData" value="1">
          <i :class="PrimeIcons.ID_CARD" class="mr-2"></i>
          <span>{{ t("sales.detail.labels.fiscalData") }}</span>
        </Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <TableInvoiceDetails
            class="mt-3"
            :canDelete="isEditable"
            :details="invoice.salesInvoiceDetails"
            :deliveryNotes="deliveryNoteStore.deliveryNotes"
            @deleteDeliveryNote="deleteDeliveryNote"
            @delete="deleteInvoiceDetail"
          >
            <template #header>
              <div
                class="flex flex-wrap align-items-center justify-content-between gap-2"
              >
                <span class="text-900 font-bold">{{ t("sales.detail.labels.invoiceDetails") }}</span>
                <div>
                  <Button
                    :size="'small'"
                    :label="t('sales.detail.actions.createDeliveryNote')"
                    @click="openDeliveryNoteSelector"
                    :disabled="!isEditable"
                  />
                  <Button
                    :size="'small'"
                    :label="t('sales.detail.actions.addLine')"
                    class="ml-2"
                    @click="openAddDetail"
                    :disabled="!isEditable"
                  />
                </div>
              </div>
            </template>
          </TableInvoiceDetails>
        </TabPanel>

        <TabPanel v-if="canEditCustomerData" value="1">
          <section class="customer-fiscal-section mt-3">
            <div class="customer-fiscal-card">
              <div class="customer-fiscal-header">
                <i class="pi pi-id-card"></i>
                <span>{{ $t("salesInvoice.customerData.title") }}</span>
                <small class="customer-fiscal-hint">
                  {{ $t("salesInvoice.customerData.hint") }}
                </small>
              </div>
              <FormSalesInvoiceCustomerData
                :invoice="invoice"
                :saving="savingCustomerData"
                @submit="saveCustomerFiscalData"
              />
            </div>
          </section>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </main>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{
      width:
        currentDialogType === dialogType.Rectificative
          ? '20vw'
          : currentDialogType === dialogType.Free
            ? '50vw'
            : '60vw',
    }"
    :maximizable="currentDialogType === dialogType.FromDeliveryNote"
  >
    <FormSalesInvoiceDetail
      v-if="currentDialogType === dialogType.Free && currentInvoiceDetail"
      :invoice-detail="currentInvoiceDetail"
      @submit="createInvoiceDetail"
      @cancel="closeDialog"
    />
    <SelectorDeliveryNotes
      v-if="currentDialogType === dialogType.FromDeliveryNote"
      :headerVisible="true"
      :deliveryNotes="deliveryNoteStore.invoiceableDeliveryNotes"
      @selected="addDeliveryNotes"
    >
      <template #header> </template>
    </SelectorDeliveryNotes>
    <FormRectificativeInvoice
      v-if="
        invoice &&
        currentDialogType === dialogType.Rectificative &&
        rectificativeRequest
      "
      :rectificative-invoice="rectificativeRequest"
      :maximum-quantity="invoice.baseAmount"
      @submit="createRectificativeInvoice"
      @cancel="closeDialog"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { storeToRefs } from "pinia";
import { useStore } from "../../../store";
import { useSalesInvoiceStore } from "../store/invoice";
import { useCustomersStore } from "../store/customers";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useTaxesStore } from "../../shared/store/tax";
import { useDeliveryNoteStore } from "../store/deliveryNote";
import { useReferenceStore } from "../../shared/store/reference";
import { PrimeIcons } from "@primevue/core/api";
import {
  convertDateTimeToJSON,
  createBlobAndDownloadFile,
  formatDate,
  formatCurrency,
} from "../../../utils/functions";
import type {
  CreateRectificativeInvoiceRequest,
  DeliveryNote,
  SalesInvoice,
  SalesInvoiceCustomerDataUpdate,
  SalesInvoiceDetail,
} from "../types";
import { DialogOptions } from "../../../types/component";
import FormSalesInvoice from "../components/FormSalesInvoice.vue";
import FormSalesInvoiceCustomerData from "../components/FormSalesInvoiceCustomerData.vue";
import TableInvoiceDetails from "../components/TableInvoiceDetails.vue";
import FormSalesInvoiceDetail from "../components/FormSalesInvoiceDetail.vue";
import FormRectificativeInvoice from "../components/FormRectificativeInvoice.vue";
import SelectorDeliveryNotes from "../components/SelectorDeliveryNotes.vue";
import Services from "../services";
import { REPORTS, ReportService } from "../../../services/report.service";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useVerifactuStore } from "../../verifactu/store/verifactu";
import { useSharedDataStore } from "../../shared/store/masterData";

const route = useRoute();
const router = useRouter();
const { t } = useI18n();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const taxesStore = useTaxesStore();
const customersStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();
const invoiceStore = useSalesInvoiceStore();
const deliveryNoteStore = useDeliveryNoteStore();
const referenceStore = useReferenceStore();
const verifactuStore = useVerifactuStore();
const sharedDataStore = useSharedDataStore();
const { invoice } = storeToRefs(invoiceStore);

const dialogType = {
  Free: 0,
  FromDeliveryNote: 1,
  Rectificative: 2,
};
const currentDialogType = ref(0);
const dialogOptions = reactive({
  visible: false,
  title: "",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);
const invoiceId = ref("");
const activeTab = ref("0");

onMounted(async () => {
  invoiceId.value = route.params.id as string;

  if (!taxesStore.taxes) await taxesStore.fetchAll();
  if (!customersStore.customers) await customersStore.fetchCustomers();
  await sharedDataStore.fetchMasterData();
  await referenceStore.fetchReferences();
  referenceStore.module = "sales";
  await lifecycleStore.fetchOneByName("SalesInvoice");
  await invoiceStore.GetById(invoiceId.value);
  await deliveryNoteStore.GetByInvoiceId(invoiceId.value);

  store.setMenuItem({
    icon: PrimeIcons.WALLET,
    title: `${t("sales.invoices.title")} ${invoice.value!.invoiceNumber} - ${
      invoice.value!.customerComercialName
    }`,
    backButtonVisible: true,
  });
});


onUnmounted(() => {
  invoiceStore.invoice = undefined;
  deliveryNoteStore.deliveryNotes = undefined;
});

const isEditable = computed(() => {
  return (
    invoice.value !== undefined && invoice.value.parentSalesInvoiceId === null
  );
});

const integrationStatusName = computed(() => {
  if (!invoice.value?.integrationStatusId) return "";
  return invoiceStore.getVerifactuStatusById(
    invoice.value.integrationStatusId,
  );
});

const canEditCustomerData = computed(() => {
  if (!invoice.value) return false;
  const status = integrationStatusName.value;
  return status === "Pendent" || status === "Error";
});

const saving = ref(false);
const savingCustomerData = ref(false);

const confirmCustomerDataPropagation = (count: number): Promise<boolean> =>
  new Promise((resolve) => {
    confirm.require({
      header: t("salesInvoice.customerData.messages.propagationHeader"),
      message: t("salesInvoice.customerData.messages.propagationMessage", {
        count,
      }),
      icon: "pi pi-exclamation-triangle",
      acceptLabel: t("salesInvoice.customerData.messages.acceptLabel"),
      rejectLabel: t("salesInvoice.customerData.messages.rejectLabel"),
      acceptClass: "p-button-warning",
      accept: () => resolve(true),
      // A true cancel saves nothing, so the admin can fix the data and
      // trigger the dialog again intentionally (issue #69 follow-up).
      reject: () => resolve(false),
      onHide: () => resolve(false),
    });
  });

// The fiscal tab's own Save: persists the fiscal data through its own endpoint
// and stays on the screen. When sibling invoices (same customer, Verifactu
// Pendent | Error) exist, the user decides whether they are updated too; a
// cancel saves nothing. The store reloads the invoice after a successful save.
const saveCustomerFiscalData = async (
  customerData: SalesInvoiceCustomerDataUpdate,
): Promise<void> => {
  if (!invoice.value || savingCustomerData.value) return;
  const invoiceId = invoice.value.id;

  savingCustomerData.value = true;
  try {
    const propagation =
      await invoiceStore.GetCustomerDataPropagation(invoiceId);
    const pendingCount = propagation?.pendingInvoicesCount ?? 0;
    let propagateToAll = false;
    if (pendingCount > 0) {
      if (!(await confirmCustomerDataPropagation(pendingCount))) return;
      propagateToAll = true;
    }

    const response = await invoiceStore.UpdateCustomerData(invoiceId, {
      ...customerData,
      propagateToAll,
    });
    if (response?.result) {
      const propagatedCount =
        (response.content as { propagatedInvoiceCount?: number } | undefined)
          ?.propagatedInvoiceCount ?? 0;
      const detail =
        propagatedCount > 0
          ? t("salesInvoice.customerData.messages.successPropagated", {
              count: propagatedCount,
            })
          : t("salesInvoice.customerData.messages.successSingle");
      toast.add({
        severity: "success",
        summary: t("salesInvoice.customerData.title"),
        detail,
        life: 5000,
      });
      return;
    }

    const errorMessage =
      response?.errors && response.errors.length > 0
        ? response.errors.join(", ")
        : t("salesInvoice.customerData.messages.error");
    toast.add({
      severity: "error",
      summary: t("salesInvoice.customerData.title"),
      detail: errorMessage,
      life: 7000,
    });
  } finally {
    savingCustomerData.value = false;
  }
};

const getVerifactuStatusClass = () => {
  if (!invoice.value?.integrationStatusId) return "";

  const status = invoiceStore.getVerifactuStatusById(
    invoice.value.integrationStatusId,
  );

  if (status === "OK") {
    return "status-ok";
  }

  if (status === "Error") {
    return "status-error";
  }

  if (status === "Pendent") {
    return "status-pending";
  }

  return "";
};

// The header Save persists the invoice only; the fiscal data has its own Save
// in the fiscal tab.
const updateInvoice = async (updatedInvoice: SalesInvoice) => {
  if (saving.value) return;
  saving.value = true;
  try {
    const updated = await invoiceStore.Update(updatedInvoice);
    if (updated) {
      router.back();
    }
  } finally {
    saving.value = false;
  }
};

const printInvoice = async () => {
  if (!invoice.value) return;

  const invoiceReport = await Services.SalesInvoice.GetReportDataById(
    invoice.value.id,
  );

  if (invoiceReport) {
    const fileName = `Factura_${invoice.value.invoiceNumber}.docx`;
    const reportService = new ReportService();
    const report = await reportService.Download(
      invoiceReport,
      REPORTS.Invoice,
      fileName,
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

const printInvoicePdf = async () => {
  if (!invoice.value) return;

  try {
    const report = await Services.SalesInvoice.DownloadPdf(invoice.value.id);
    if (!report) throw new Error("No s'ha pogut generar el PDF");

    createBlobAndDownloadFile(`Factura_${invoice.value.invoiceNumber}.pdf`, report);
  } catch {
    toast.add({
      severity: "error",
      summary: t("sales.detail.messages.error"),
      detail: t("sales.detail.messages.reportError"),
    });
  }
};
// Invoice details
const openDeliveryNoteSelector = async () => {
  if (invoice.value) {
    await deliveryNoteStore.GetToInvoice(invoice.value.customerId);

    currentDialogType.value = dialogType.FromDeliveryNote;
    dialogOptions.title = t("sales.detail.dialogs.deliveryNoteSelector");
    dialogOptions.visible = true;
  }
};
const addDeliveryNotes = async (deliveryNotes: Array<DeliveryNote>) => {
  for (let index = 0; index < deliveryNotes.length; index++) {
    const deliveryNote = deliveryNotes[index];
    await invoiceStore.AddDeliveryNote(invoice.value!.id, deliveryNote);
  }

  dialogOptions.visible = false;
  await loadDetails();
};
const deleteDeliveryNote = async (deliveryNote: DeliveryNote) => {
  await invoiceStore.RemoveDeliveryNote(invoice.value!.id, deliveryNote);
  await loadDetails();
  toast.add({
    severity: "success",
    summary: "Albarà desassignat",
    detail: "L'albarà s'ha desassignat correctament de la factura",
    life: 4000,
  });
};
const loadDetails = async () => {
  await invoiceStore.GetById(invoiceId.value);
  await deliveryNoteStore.GetByInvoiceId(invoiceId.value);
};

const closeDialog = () => {
  dialogOptions.visible = false;
};

const currentInvoiceDetail = ref<SalesInvoiceDetail>();
const openAddDetail = () => {
  currentDialogType.value = dialogType.Free;
  if (invoice.value) {
    const tax = taxesStore.taxes?.find((t) => t.percentatge === 21);
    currentInvoiceDetail.value = {
      salesInvoiceId: invoice.value.id,
      quantity: 1,
      description: "",
      unitPrice: 0,
      amount: 0,
      unitCost: 0,
      totalCost: 0,
      ...(tax ? { taxId: tax.id } : {}),
    } as SalesInvoiceDetail;

    dialogOptions.title = t("sales.detail.dialogs.freeLine");
    dialogOptions.visible = true;
  }
};
const createInvoiceDetail = async (detail: SalesInvoiceDetail) => {
  await invoiceStore.CreateInvoiceDetail(detail);
  dialogOptions.visible = false;
};
const deleteInvoiceDetail = async (detail: SalesInvoiceDetail) => {
  await invoiceStore.DeleteInvoiceDetail(detail);
};

// Create rectificative invoice
const rectificativeRequest = ref(
  undefined as undefined | CreateRectificativeInvoiceRequest
);
const requestRectificativeQuantity = async () => {
  rectificativeRequest.value = {
    id: invoice.value!.id,
    createCorrectionInvoice: false,
    quantity: 0,
  };

  dialogOptions.visible = true;
  dialogOptions.title = t("sales.detail.dialogs.rectificativeInvoice");
  currentDialogType.value = dialogType.Rectificative;
};
const createRectificativeInvoice = async (
  request: CreateRectificativeInvoiceRequest,
) => {
  const response = await invoiceStore.CreateRectificative(request);
  if (response && response.result && response.content) {
    toast.add({
      summary: t("sales.detail.messages.rectificativeInvoice"),
      detail: t("sales.detail.messages.rectificativeCreated", { number: response.content.invoiceNumber }),
      severity: "success",
      life: 10000,
    });

    router.back();
  } else {
    toast.add({
      summary: t("sales.detail.messages.rectificativeInvoice"),
      detail: t("sales.detail.messages.invoiceCreationError"),
      severity: "error",
      life: 10000,
    });
  }
};

// Send to Verifactu
const sendToVerifactu = async () => {
  if (invoice.value) {
    const response = await verifactuStore.SendToVerifactu(invoice.value.id);
    if (response && response.result) {
      toast.add({
        severity: "success",
        summary: t("sales.detail.labels.verifactu"),
        detail: t("sales.detail.messages.verifactuSent"),
        life: 5000,
      });
      await invoiceStore.GetById(invoiceId.value);
    } else {
      toast.add({
        severity: "error",
        summary: t("sales.detail.messages.error"),
        detail:
          t("sales.detail.messages.sendError") +
          (response?.errors && response?.errors.length > 0
            ? `: ${response.errors[0]}`
            : ""),
        life: 5000,
      });
    }
  }
};
</script>

<style scoped>

.invoice-totals-section {
  padding-right: 0.75rem;
}

.invoice-totals-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 0.75rem;
}

.total-card {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.65rem 0.85rem;
  background: var(--p-content-background, #fff);
  transition: box-shadow 0.15s ease;
}

.total-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.total-card-total {
  background: var(--p-primary-50, #eef2ff);
  border-color: var(--p-primary-200, #c7d2fe);
}

.total-card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: 8px;
  background: var(--p-surface-100, #f1f5f9);
  color: var(--p-primary-color, #3b82f6);
  font-size: 0.95rem;
  flex-shrink: 0;
}

.total-card-total .total-card-icon {
  background: var(--p-primary-100, #dbeafe);
  color: var(--p-primary-700, #1d4ed8);
}

.total-card-content {
  display: flex;
  flex-direction: column;
  gap: 0.05rem;
  min-width: 0;
}

.total-card-label {
  font-size: 0.8571rem;
  color: var(--p-text-muted-color);
  white-space: nowrap;
  line-height: 1.1;
}

.total-card-value {
  font-size: 1rem;
  font-weight: 700;
  color: var(--p-text-color);
  line-height: 1.15;
}

.total-card-total .total-card-value {
  color: var(--p-primary-700, #1d4ed8);
}

.status-ok {
  color: #28a745;
}

.status-error {
  color: #dc3545;
}

.status-pending {
  color: #6c757d;
}

.customer-fiscal-section {
  padding-right: 0.75rem;
}

.customer-fiscal-card {
  border: 1px solid var(--p-content-border-color);
  border-left: 4px solid var(--p-primary-500, #6366f1);
  border-radius: 8px;
  padding: 1rem 1.25rem;
  background: var(--p-content-background, #fff);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.customer-fiscal-header {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  margin-bottom: 0.85rem;
  color: var(--p-text-color);
}

.customer-fiscal-header i {
  color: var(--p-primary-color, #6366f1);
  font-size: 1.1rem;
}

.customer-fiscal-header span {
  font-weight: 700;
  font-size: 1rem;
}

.customer-fiscal-hint {
  margin-left: auto;
  color: var(--p-text-muted-color);
  font-size: 0.8rem;
}

@media (max-width: 640px) {
  .customer-fiscal-section {
    padding-right: 0;
  }
}

@media (max-width: 1200px) {
  .invoice-totals-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .invoice-totals-grid {
    grid-template-columns: 1fr;
  }

  .invoice-totals-section {
    padding-right: 0;
  }
}
</style>
