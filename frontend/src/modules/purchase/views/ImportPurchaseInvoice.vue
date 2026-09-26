<template>
  <div class="import-purchase-invoice">
    <div class="import-toolbar">
      <Button
        icon="pi pi-arrow-left"
        label="Tornar"
        severity="secondary"
        text
        size="small"
        @click="onCancel"
      />
      <h2 class="import-title">Importar factura (PDF)</h2>
    </div>

    <!-- Slim upload banner — collapses to a thin status row when there's a result -->
    <div class="ingest-banner" :class="{ 'ingest-banner--has-result': result }">
      <!-- Initial state: no file selected -->
      <div v-if="!selectedFile && !isUploading" class="ingest-banner__initial">
        <i class="pi pi-file-pdf"></i>
        <span>Selecciona o arrossega un PDF</span>
        <FileUpload
          mode="basic"
          :auto="true"
          accept="application/pdf"
          :chooseLabel="'Selecciona PDF'"
          :showCancelButton="false"
          :showUploadButton="false"
          :customUpload="true"
          @uploader="onUploader"
          @select="onFileSelected"
        />
      </div>

      <!-- Uploading state: progress bar -->
      <div v-else-if="isUploading" class="ingest-banner__uploading">
        <i class="pi pi-spin pi-cloud-upload"></i>
        <span class="ingest-banner__filename">{{ selectedFile?.name }}</span>
        <span class="ingest-banner__size">{{ formatBytes(selectedFile?.size ?? 0) }}</span>
        <ProgressBar
          mode="indeterminate"
          style="flex: 1; height: 6px; max-width: 240px"
        />
      </div>

      <!-- Completed state: change file link -->
      <div v-else-if="result" class="ingest-banner__completed">
        <i class="pi pi-check-circle" style="color: var(--p-green-500)"></i>
        <span class="ingest-banner__filename">{{ selectedFile?.name }}</span>
        <span class="ingest-banner__size">{{ formatBytes(selectedFile?.size ?? 0) }}</span>
        <Button
          icon="pi pi-refresh"
          label="Canviar PDF"
          severity="secondary"
          text
          size="small"
          @click="onReset"
        />
      </div>

      <!-- Error state -->
      <Message
        v-if="errorMessage"
        severity="error"
        class="ingest-banner__error"
        :closable="true"
        @close="errorMessage = null"
      >
        {{ errorMessage }}
      </Message>
    </div>

    <!-- Review form (unchanged shape, but no wrapping Card) -->
    <div v-if="result" class="ingest-review">
      <FormPurchaseInvoice
        ref="formRef"
        :purchaseInvoice="store.purchaseInvoice!"
        @submit="onAccept"
        @cancel="onCancel"
      />
      <div class="ingest-actions">
        <Button
          label="Acceptar i crear"
          icon="pi pi-check"
          :loading="isSaving"
          :disabled="isSaving"
          @click="onAcceptClick"
        />
        <Button
          label="Cancel·lar"
          icon="pi pi-times"
          severity="secondary"
          :disabled="isSaving"
          @click="onCancel"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import Button from "primevue/button";
import Card from "primevue/card";
import Message from "primevue/message";
import {
  convertDateTimeToJSON,
  getNewUuid,
} from "@/utils/functions";
import {
  usePurchaseInvoiceStore,
} from "../store/purchaseInvoices";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { useSuppliersStore } from "../store/suppliers";
import PurchaseService from "../services";
import FormPurchaseInvoice from "../components/FormPurchaseInvoice.vue";
import { useStore } from "@/store";
import type { PurchaseInvoice } from "../types";

const router = useRouter();
const toast = useToast();
const appStore = useStore();
const store = usePurchaseInvoiceStore();
const masterData = usePurchaseMasterDataStore();
const supplierStore = useSuppliersStore();

const formRef = ref<InstanceType<typeof FormPurchaseInvoice> | null>(null);

const selectedFile = ref<File | null>(null);
const isUploading = ref(false);
const isSaving = ref(false);
const errorMessage = ref<string | null>(null);
const result = ref(false);

const formatBytes = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
};

onMounted(async () => {
  appStore.setMenuItem({
    icon: PrimeIcons.FILE_PDF,
    backButtonVisible: true,
    title: "Importar factura (PDF)",
  });
  // Seed a fresh draft so the form has something to bind to before upload.
  store.setNewPurchaseInvoice(getNewUuid());
  await masterData.fetchMasterData();
  // Preload the entity-specific supplier store so setFromIngestion can
  // auto-resolve SupplierId by VatNumber at upload time.
  await supplierStore.fetchSuppliers();
});

const onFileSelected = (event: { originalEvent?: Event; files: File[] | File | unknown }) => {
  // PrimeVue v4 fires @select before the upload is attempted; we use it
  // only to display the chosen filename in the banner during upload.
  const files = Array.isArray(event.files)
    ? event.files
    : event.files
      ? [event.files as File]
      : [];
  selectedFile.value = (files[0] as File) ?? null;
  errorMessage.value = null;
  result.value = false;
};

const onUploader = async (event: { files: File[] | File }) => {
  // PrimeVue v4 fires @uploader when auto=true and a file is selected.
  // The payload is { files: File | File[] }; normalize to File[].
  const files = Array.isArray(event.files)
    ? event.files
    : event.files
      ? [event.files]
      : [];
  const file = files[0];
  if (!file) return;

  selectedFile.value = file;
  isUploading.value = true;
  errorMessage.value = null;

  try {
    const payload = await PurchaseService.PurchaseInvoiceIngestion.ingest(file);

    if (!payload) {
      errorMessage.value =
        "No s'ha pogut obtenir resposta del servidor d'ingesta.";
      return;
    }

    // Prefill the store from the provider response. setFromIngestion also
    // auto-resolves SupplierId by VatNumber against useSuppliersStore. If
    // no match, supplierId is left empty so the operator can pick manually.
    store.setFromIngestion(payload);

    // The form only mounts once result is true; wait for it, then recompute
    // header totals without waiting for its mount delay.
    result.value = true;
    await nextTick();
    await formRef.value?.calcAmountsNow();

    toast.add({
      severity: "success",
      summary: "Dades extretes",
      detail: "Revisa els camps i prem 'Acceptar i crear'.",
      life: 4000,
    });
  } catch (err) {
    // Surface network/parse errors without losing the selectedFile so the
    // operator can see which file failed and retry by picking another.
    const detail =
      err instanceof Error ? err.message : "Error desconegut durant la ingesta.";
    errorMessage.value = `Error processant el PDF: ${detail}`;
    result.value = false;
  } finally {
    isUploading.value = false;
  }
};

const onReset = () => {
  selectedFile.value = null;
  result.value = false;
  errorMessage.value = null;
  // Reset the form to a fresh draft so the operator can re-upload a new PDF
  // and review a clean set of fields.
  store.setNewPurchaseInvoice(getNewUuid());
};

const onAcceptClick = () => {
  formRef.value?.submitForm();
};

const onAccept = async (invoice: PurchaseInvoice) => {
  isSaving.value = true;
  try {
    // Normalize the date before the API call (matches existing flow).
    invoice.purchaseInvoiceDate = convertDateTimeToJSON(
      invoice.purchaseInvoiceDate,
    );
    const ok = await store.Create(invoice);
    if (ok) {
      toast.add({
        severity: "success",
        summary: "Factura creada",
        life: 4000,
      });
      router.replace({ name: "PurchaseInvoice", params: { id: invoice.id } });
    } else {
      toast.add({
        severity: "error",
        summary: "Error al crear la factura",
        life: 6000,
      });
    }
  } finally {
    isSaving.value = false;
  }
};

const onCancel = () => {
  router.push({ name: "PurchaseInvoices" });
};
</script>

<style scoped>
.import-purchase-invoice {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1rem;
}

.import-toolbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.import-title {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
}

.ingest-banner {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border: 1px solid var(--p-surface-200, var(--surface-200));
  border-radius: 0.5rem;
  background: var(--p-surface-50, var(--surface-50));
}

.ingest-banner--has-result {
  padding: 0.5rem 1rem;
  background: var(--p-green-50, var(--green-50, #ecfdf5));
  border-color: var(--p-green-200, var(--green-200, #a7f3d0));
}

.ingest-banner__initial,
.ingest-banner__uploading,
.ingest-banner__completed {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.ingest-banner__initial i,
.ingest-banner__uploading i,
.ingest-banner__completed i {
  font-size: 1.1rem;
}

.ingest-banner__filename {
  font-weight: 500;
  flex: 0 1 auto;
}

.ingest-banner__size {
  color: var(--p-text-muted-color);
  font-size: 0.85rem;
  flex: 0 0 auto;
}

.ingest-banner__error {
  margin-top: 0.25rem;
}

.ingest-review {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 0.5rem;
}

.ingest-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}
</style>