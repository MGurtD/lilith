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

    <Card class="import-card">
      <template #content>
        <div class="import-step">
          <label class="block text-900 mb-2 font-medium">
            1. Selecciona el fitxer PDF de la factura
          </label>
          <input
            ref="fileInput"
            type="file"
            accept="application/pdf"
            class="import-file-input"
            :disabled="isUploading"
            @change="onFileSelected"
          />
          <small v-if="selectedFile" class="import-file-name">
            {{ selectedFile.name }} ({{ formatBytes(selectedFile.size) }})
          </small>
        </div>

        <div class="import-step">
          <Button
            label="Processar PDF"
            icon="pi pi-cloud-upload"
            :loading="isUploading"
            :disabled="!selectedFile || isUploading"
            @click="onUpload"
          />
        </div>

        <Message
          v-if="errorMessage"
          severity="error"
          class="import-message"
          :closable="false"
        >
          {{ errorMessage }}
        </Message>
      </template>
    </Card>

    <Card v-if="result" class="import-card">
      <template #content>
        <div class="import-step">
          <label class="block text-900 mb-2 font-medium">
            2. Revisa les dades i accepta la creació
          </label>
          <FormPurchaseInvoice
            ref="formRef"
            :purchaseInvoice="store.purchaseInvoice!"
            @submit="onAccept"
            @cancel="onCancel"
          />
        </div>

        <div class="import-actions">
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
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
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
import PurchaseService from "../services";
import FormPurchaseInvoice from "../components/FormPurchaseInvoice.vue";
import { useStore } from "@/store";
import type { PurchaseInvoice } from "../types";

const router = useRouter();
const toast = useToast();
const appStore = useStore();
const store = usePurchaseInvoiceStore();
const masterData = usePurchaseMasterDataStore();

const fileInput = ref<HTMLInputElement | null>(null);
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
});

const onFileSelected = (event: Event) => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0] ?? null;
  selectedFile.value = file;
  errorMessage.value = null;
  result.value = false;
};

const onUpload = async () => {
  if (!selectedFile.value) return;

  isUploading.value = true;
  errorMessage.value = null;

  try {
    const payload = await PurchaseService.PurchaseInvoiceIngestion.ingest(
      selectedFile.value,
    );

    if (!payload) {
      errorMessage.value =
        "No s'ha pogut obtenir resposta del servidor d'ingesta.";
      return;
    }

    // Prefill the store from the provider response. SupplierId is left empty
    // so the operator can pick it manually after reviewing the form.
    store.setFromIngestion(payload);

    // Auto-match supplier by VatNumber against the in-memory master data.
    if (payload.supplierVatNumber && store.purchaseInvoice) {
      const match = masterData.masterData.suppliers?.find(
        (s) =>
          s.vatNumber &&
          payload.supplierVatNumber &&
          s.vatNumber.replace(/\s/g, "").toLowerCase() ===
            payload.supplierVatNumber.replace(/\s/g, "").toLowerCase(),
      );
      if (match) {
        store.purchaseInvoice.supplierId = match.id;
      }
    }

    // Force header totals to recompute immediately, bypassing the 500ms gate.
    formRef.value?.calcAmountsNow();

    result.value = true;
    toast.add({
      severity: "success",
      summary: "Dades extretes",
      detail: "Revisa els camps i prem 'Acceptar i crear'.",
      life: 4000,
    });
  } finally {
    isUploading.value = false;
  }
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

.import-card {
  width: 100%;
}

.import-step {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.import-file-input {
  font-size: 0.9rem;
}

.import-file-name {
  color: var(--p-text-muted-color);
  font-size: 0.85rem;
}

.import-message {
  margin-top: 0.5rem;
}

.import-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
  margin-top: 1rem;
}
</style>