<template>
  <div class="button-panel">
    <div class="flex align-items-end justify-content-end">
      <SplitButton
        label="Guardar"
        @click="updateInvoice"
        :model="items"
        :size="'small'"
      />
    </div>
  </div>

  <main v-if="invoice">
    <FormSalesInvoice class="mt-3 mr-3" :invoice="invoice" />

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
          <span class="text-900 font-bold">Detall de la factura</span>
          <div>
            <Button
              :size="'small'"
              label="Afegir albarà"
              @click="openDeliveryNoteSelector"
              :disabled="!isEditable"
            />
            &nbsp;&nbsp;
            <Button
              :size="'small'"
              label="Afegir linia lliure"
              @click="openAddDetail"
              :disabled="!isEditable"
            />
          </div>
        </div>
      </template>
    </TableInvoiceDetails>
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
      v-if="currentDialogType === dialogType.Free"
      :invoiceDetail="currentInvoiceDetail"
      @submit="createInvoiceDetail"
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
    />
  </Dialog>
</template>
<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
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
} from "../../../utils/functions";
import {
  CreateRectificativeInvoiceRequest,
  DeliveryNote,
  SalesInvoiceDetail,
} from "../types";
import { DialogOptions } from "../../../types/component";
import FormSalesInvoice from "../components/FormSalesInvoice.vue";
import TableInvoiceDetails from "../components/TableInvoiceDetails.vue";
import FormSalesInvoiceDetail from "../components/FormSalesInvoiceDetail.vue";
import FormRectificativeInvoice from "../components/FormRectificativeInvoice.vue";
import SelectorDeliveryNotes from "../components/SelectorDeliveryNotes.vue";
import Services from "../services";
import { REPORTS, ReportService } from "../../../services/report.service";
import { useToast } from "primevue/usetoast";
import { useVerifactuStore } from "../../verifactu/store/verifactu";

const items = [
  {
    label: "Descarregar",
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(),
  },
  {
    label: "Rectificativa",
    icon: PrimeIcons.FILE_IMPORT,
    command: () => requestRectificativeQuantity(),
  },
];

const route = useRoute();
const router = useRouter();
const store = useStore();
const toast = useToast();
const taxesStore = useTaxesStore();
const customersStore = useCustomersStore();
const lifecycleStore = useLifecyclesStore();
const invoiceStore = useSalesInvoiceStore();
const deliveryNoteStore = useDeliveryNoteStore();
const referenceStore = useReferenceStore();
const verifactuStore = useVerifactuStore();
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

onMounted(async () => {
  invoiceId.value = route.params.id as string;

  if (!taxesStore.taxes) await taxesStore.fetchAll();
  if (!customersStore.customers) await customersStore.fetchCustomers();
  await referenceStore.fetchReferencesByModule("sales");
  await lifecycleStore.fetchOneByName("SalesInvoice");
  await invoiceStore.GetById(invoiceId.value);
  await deliveryNoteStore.GetByInvoiceId(invoiceId.value);

  if (invoice.value) {
    invoice.value.invoiceDate = formatDate(invoice.value.invoiceDate);
  }

  store.setMenuItem({
    icon: PrimeIcons.WALLET,
    title: `Factura de venta ${invoice.value!.invoiceNumber} - ${
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

const updateInvoice = async () => {
  if (invoice.value) {
    invoice.value.invoiceDate = convertDateTimeToJSON(
      invoice.value.invoiceDate
    );

    const updated = await invoiceStore.Update(invoice.value);
    if (updated) {
      router.back();
    }
  }
};

// Report print
const printInvoice = async () => {
  const invoiceReport = await Services.SalesInvoice.GetReportDataById(
    invoice.value!.id
  );

  if (invoiceReport) {
    const fileName = `Factura_${invoice.value?.invoiceNumber}.docx`;

    const reportService = new ReportService();
    const report = await reportService.Download(
      invoiceReport,
      REPORTS.Invoice,
      fileName
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

// Invoice details
const openDeliveryNoteSelector = async () => {
  if (invoice.value) {
    await deliveryNoteStore.GetToInvoice(invoice.value.customerId);

    currentDialogType.value = dialogType.FromDeliveryNote;
    dialogOptions.title = "Selector d'albarans d'entrega";
    dialogOptions.visible = true;
  }
};
const addDeliveryNotes = async (deliveryNotes: Array<DeliveryNote>) => {
  for (let index = 0; index < deliveryNotes.length; index++) {
    const deliveryNote = deliveryNotes[index];
    await invoiceStore.AddDeliveryNote(invoice.value!.id, deliveryNote);
  }

  dialogOptions.visible = false;
  loadDetails();
};
const deleteDeliveryNote = async (deliveryNote: DeliveryNote) => {
  await invoiceStore.RemoveDeliveryNote(invoice.value!.id, deliveryNote);
  loadDetails();
};
const loadDetails = async () => {
  lifecycleStore.fetchOneByName("SalesInvoice");
  await deliveryNoteStore.GetByInvoiceId(invoiceId.value);
};

const currentInvoiceDetail = reactive({} as SalesInvoiceDetail);
const openAddDetail = () => {
  currentDialogType.value = dialogType.Free;
  if (invoice.value) {
    currentInvoiceDetail.salesInvoiceId = invoice.value.id;
    currentInvoiceDetail.quantity = 1;
    currentInvoiceDetail.description = "";
    currentInvoiceDetail.unitPrice = 0;
    currentInvoiceDetail.amount = 0;
    currentInvoiceDetail.totalCost = 0;

    const tax = taxesStore.taxes?.find((t) => t.percentatge === 21);
    if (tax) currentInvoiceDetail.taxId = tax.id;

    dialogOptions.title = "Introducció de línea lliure";
    dialogOptions.visible = true;
  }
};
const createInvoiceDetail = async () => {
  await invoiceStore.CreateInvoiceDetail(currentInvoiceDetail);
  dialogOptions.visible = false;
};
const deleteInvoiceDetail = async (detail: SalesInvoiceDetail) => {
  await invoiceStore.DeleteInvoiceDetail(detail);
};

// Create rectificative invoice
const rectificativeRequest = ref(
  undefined as undefined | CreateRectificativeInvoiceRequest
);
const requestRectificativeQuantity = () => {
  rectificativeRequest.value = {
    id: invoice.value!.id,
    quantity: 0,
  };

  dialogOptions.visible = true;
  dialogOptions.title = "Crear factura rectificativa";
  currentDialogType.value = dialogType.Rectificative;
};
const createRectificativeInvoice = async () => {
  if (rectificativeRequest.value) {
    const response = await invoiceStore.CreateRectificative(
      rectificativeRequest.value
    );
    if (response && response.result && response.content) {
      toast.add({
        summary: "Factura rectificativa",
        detail: `Creada correctament amb el número ${response.content.invoiceNumber}`,
        severity: "success",
        life: 10000,
      });

      router.back();
    } else {
      toast.add({
        summary: "Factura rectificativa",
        detail: "Error en la creació de la factura",
        severity: "error",
        life: 10000,
      });
    }
  }
};

// Send to Verifactu
const sendToVerifactu = async () => {
  if (invoice.value) {
    const response = await verifactuStore.SendToVerifactu(invoice.value.id);
    if (response && response.result) {
      toast.add({
        severity: "success",
        summary: "Enviament a Verifactu",
        detail: "Factura enviada correctament a Verifactu",
        life: 5000,
      });
      await invoiceStore.GetById(invoiceId.value);
    } else {
      toast.add({
        severity: "error",
        summary: "Error en l'enviament",
        detail:
          "No s'ha pogut enviar la factura" +
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
.button-panel {
  position: absolute;
  top: 0;
  right: 2rem;
  z-index: 1000;
}
</style>
