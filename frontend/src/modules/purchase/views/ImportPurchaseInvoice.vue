<template>
  <PageActions v-if="draft && purchaseInvoice">
    <Button
      icon="pi pi-check"
      :label="t('purchase.invoiceImport.actions.create')"
      :loading="isSaving"
      :disabled="isSaving"
      @click="formRef?.submitForm()"
    />
  </PageActions>

  <input
    ref="fileInput"
    type="file"
    accept="application/pdf,.pdf"
    class="invoice-import__file-input"
    @change="onFileInputChange"
  />

  <div
    v-if="!pdfFile"
    class="invoice-import__dropzone"
    :class="{ 'invoice-import__dropzone--dragging': isDragging }"
    @dragenter.prevent="isDragging = true"
    @dragover.prevent="isDragging = true"
    @dragleave.prevent="onDragLeave"
    @drop.prevent="onDrop"
  >
    <i class="pi pi-file-pdf invoice-import__dropzone-icon" aria-hidden="true" />
    <h2 class="invoice-import__dropzone-title">
      {{
        isDragging
          ? t("purchase.invoiceImport.dropzone.release")
          : t("purchase.invoiceImport.dropzone.title")
      }}
    </h2>
    <p class="invoice-import__dropzone-hint">
      {{ t("purchase.invoiceImport.dropzone.hint") }}
    </p>
    <Button
      icon="pi pi-upload"
      :label="t('purchase.invoiceImport.dropzone.choose')"
      @click="openFilePicker"
    />
    <Message
      v-if="fileError"
      severity="error"
      :closable="false"
      class="invoice-import__dropzone-error"
    >
      {{ fileError }}
    </Message>
  </div>

  <div v-else class="invoice-import">
    <section class="invoice-import__document">
      <div class="invoice-import__document-header">
        <i class="pi pi-file-pdf" aria-hidden="true" />
        <span class="invoice-import__document-name">{{ pdfFile.name }}</span>
        <span class="invoice-import__document-size">
          {{ formatBytes(pdfFile.size) }}
        </span>
        <Button
          icon="pi pi-refresh"
          :label="t('purchase.invoiceImport.document.change')"
          severity="secondary"
          size="small"
          text
          :disabled="isExtracting || isSaving"
          @click="openFilePicker"
        />
      </div>
      <Message v-if="fileError" severity="error" :closable="false">
        {{ fileError }}
      </Message>
      <div class="invoice-import__viewer">
        <PdfViewer :file="null" :source="pdfFile" :source-name="pdfFile.name" />
      </div>
    </section>

    <section class="invoice-import__review">
      <div v-if="isExtracting" class="invoice-import__state">
        <ProgressSpinner style="width: 3rem; height: 3rem" stroke-width="4" />
        <strong>{{ t("purchase.invoiceImport.extracting.title") }}</strong>
        <span>{{ t("purchase.invoiceImport.extracting.detail") }}</span>
      </div>

      <Message
        v-else-if="extractionError"
        severity="error"
        :closable="false"
      >
        <div class="invoice-import__error">
          <strong>{{ t("purchase.invoiceImport.errors.title") }}</strong>
          <span>{{ extractionError }}</span>
          <Button
            icon="pi pi-replay"
            :label="t('purchase.invoiceImport.errors.retry')"
            severity="danger"
            size="small"
            outlined
            @click="extract(pdfFile)"
          />
        </div>
      </Message>

      <template v-else-if="draft && purchaseInvoice">
        <Message
          :severity="draft.issues.length ? 'warn' : 'success'"
          :closable="false"
        >
          <div class="invoice-import__issues">
            <strong>
              {{
                t("purchase.invoiceImport.review.issues", draft.issues.length)
              }}
            </strong>
            <ul v-if="draft.issues.length">
              <li v-for="(issue, index) in draft.issues" :key="index">
                <span class="invoice-import__issue-field">
                  {{ issueLabel(issue) }}:
                </span>
                {{ issue.message }}
                <Button
                  v-if="
                    issue.code === 'SupplierNotFound' &&
                    !dismissedFields.has('supplierId')
                  "
                  icon="pi pi-plus"
                  :label="t('purchase.invoiceImport.actions.createSupplier')"
                  size="small"
                  text
                  class="invoice-import__issue-action"
                  @click="openSupplierDialog"
                />
                <a
                  v-else-if="issue.code === 'DuplicateInvoice' && issue.relatedId"
                  :href="invoiceHref(issue.relatedId)"
                  target="_blank"
                  rel="noopener"
                  class="invoice-import__issue-link"
                >
                  {{ t("purchase.invoiceImport.actions.openInvoice") }}
                  <i class="pi pi-external-link" aria-hidden="true" />
                </a>
              </li>
            </ul>
          </div>
        </Message>

        <Message v-if="duplicateOf" severity="error" :closable="false">
          <div class="invoice-import__error">
            <strong>{{ t("purchase.invoiceImport.duplicate.title") }}</strong>
            <span>{{ duplicateOf.message }}</span>
            <a
              :href="invoiceHref(duplicateOf.id)"
              target="_blank"
              rel="noopener"
              class="invoice-import__issue-link"
            >
              {{ t("purchase.invoiceImport.actions.openInvoice") }}
              <i class="pi pi-external-link" aria-hidden="true" />
            </a>
          </div>
        </Message>

        <div
          v-if="draft.totalAmount != null"
          class="invoice-import__totals"
          :class="
            totalMatches
              ? 'invoice-import__totals--ok'
              : 'invoice-import__totals--mismatch'
          "
        >
          <span>
            {{ t("purchase.invoiceImport.totals.document") }}:
            <strong>{{ formatCurrency(draft.totalAmount) }}</strong>
          </span>
          <span>
            {{ t("purchase.invoiceImport.totals.computed") }}:
            <strong>{{ formatCurrency(computedTotal) }}</strong>
          </span>
          <span class="invoice-import__totals-status">
            <i
              :class="
                totalMatches ? 'pi pi-check-circle' : 'pi pi-exclamation-triangle'
              "
              aria-hidden="true"
            />
            {{
              totalMatches
                ? t("purchase.invoiceImport.totals.matches")
                : t("purchase.invoiceImport.totals.difference", {
                    amount: formatCurrency(computedTotal - draft.totalAmount),
                  })
            }}
          </span>
        </div>

        <FormPurchaseInvoice
          :key="purchaseInvoice.id"
          ref="formRef"
          :purchase-invoice="purchaseInvoice"
          :field-warnings="fieldWarnings"
          @submit="onSubmit"
          @calculated="onCalculated"
          @due-dates-change="onDueDatesChange"
        />

        <TablePurchaseInvoiceImports
          :purchase-invoice-imports="purchaseInvoice.purchaseInvoiceImports"
          :row-warnings="rowWarnings"
          :pending-tax-rates="pendingTaxRates"
          @add="(row: PurchaseInvoiceImport) => openImportForm(FormActionMode.CREATE, row)"
          @edit="(row: PurchaseInvoiceImport) => openImportForm(FormActionMode.EDIT, row)"
          @delete="deleteImport"
        />
      </template>
    </section>
  </div>

  <Dialog
    v-model:visible="isImportDialogVisible"
    :header="
      importFormMode === FormActionMode.CREATE
        ? t('purchase.purchaseInvoice.dialogs.createAmount')
        : t('purchase.purchaseInvoice.dialogs.editAmount')
    "
    modal
    :style="{ width: '90vw', maxWidth: '40rem' }"
    @after-hide="selectedImport = undefined"
  >
    <FormPurchaseInvoiceImport
      v-if="selectedImport"
      :form-action="importFormMode"
      :invoice-import="selectedImport"
      @submit="onImportSubmit"
    />
  </Dialog>

  <Dialog
    v-model:visible="isSupplierDialogVisible"
    :header="t('purchase.invoiceImport.supplierDialog.title')"
    :closable="!isCreatingSupplier"
    modal
    :style="{ width: '90vw', maxWidth: '64rem' }"
    @after-hide="supplierDraft = undefined"
  >
    <FormSupplier
      v-if="supplierDraft"
      in-dialog
      :supplier="supplierDraft"
      @submit="onSupplierSubmit"
      @cancel="isSupplierDialogVisible = false"
    />
  </Dialog>
</template>

<script setup lang="ts">
import PageActions from "@/components/PageActions.vue";
import PdfViewer from "@/components/PdfViewer.vue";
import { FileService } from "@/services/file.service";
import { useStore } from "@/store";
import { FormActionMode } from "@/types/component";
import {
  convertDateTimeToJSON,
  formatCurrency,
  getNewUuid,
} from "@/utils/functions";
import { useLifecyclesStore } from "@/modules/shared/store/lifecycle";
import { PrimeIcons } from "@primevue/core/api";
import Message from "primevue/message";
import ProgressSpinner from "primevue/progressspinner";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { computed, nextTick, onMounted, ref, shallowRef } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import FormPurchaseInvoice from "../components/FormPurchaseInvoice.vue";
import FormPurchaseInvoiceImport from "../components/FormPurchaseInvoiceImport.vue";
import FormSupplier from "../components/FormSupplier.vue";
import TablePurchaseInvoiceImports from "../components/TablePurchaseInvoiceImports.vue";
import PurchaseService from "../services";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { usePurchaseInvoiceStore } from "../store/purchaseInvoices";
import { buildNewSupplier, useSuppliersStore } from "../store/suppliers";
import type {
  IngestionIssue,
  IngestPurchaseInvoiceResponse,
  PurchaseInvoice,
  PurchaseInvoiceCalculatedValues,
  PurchaseInvoiceDueDate,
  PurchaseInvoiceImport,
  Supplier,
} from "../types";

// Matches the backend request size limit of the ingest endpoint.
const MAX_PDF_BYTES = 20 * 1024 * 1024;
// Same tolerance as the backend total check.
const TOTAL_TOLERANCE = 0.05;

const { t } = useI18n();
const router = useRouter();
const toast = useToast();
const appStore = useStore();
const invoiceStore = usePurchaseInvoiceStore();
const masterDataStore = usePurchaseMasterDataStore();
const lifecycleStore = useLifecyclesStore();
const suppliersStore = useSuppliersStore();
const { purchaseInvoice } = storeToRefs(invoiceStore);
const fileService = new FileService();

const formRef = ref<InstanceType<typeof FormPurchaseInvoice> | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);

const pdfFile = shallowRef<File | null>(null);
const fileError = ref<string | null>(null);
const isDragging = ref(false);
const isExtracting = ref(false);
const extractionError = ref<string | null>(null);
const isSaving = ref(false);

const draft = ref<IngestPurchaseInvoiceResponse | null>(null);
// Import ids in the order of draft.taxBreakdown, to follow rows after edits.
const importIdsByRow = ref<string[]>([]);
const reviewedImportIds = ref(new Set<string>());
const computedTotal = ref(0);

// Fields whose review hint was resolved here, e.g. a supplier created from the draft.
const dismissedFields = ref(new Set<string>());
const duplicateOf = ref<{ id: string; message: string } | null>(null);

const isSupplierDialogVisible = ref(false);
const isCreatingSupplier = ref(false);
const supplierDraft = ref<Supplier>();

const isImportDialogVisible = ref(false);
const importFormMode = ref(FormActionMode.EDIT);
const selectedImport = ref<PurchaseInvoiceImport>();

onMounted(async () => {
  appStore.setMenuItem({
    icon: PrimeIcons.FILE_PDF,
    backButtonVisible: true,
    title: t("purchase.invoiceImport.title"),
  });
  await Promise.all([
    masterDataStore.fetchMasterData(),
    lifecycleStore.fetchOneByName("PurchaseInvoice"),
  ]);
});

// ---- File selection ----

const openFilePicker = () => fileInput.value?.click();

const onFileInputChange = (event: Event) => {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  input.value = "";
  if (file) selectFile(file);
};

const onDragLeave = (event: DragEvent) => {
  const target = event.currentTarget as HTMLElement;
  if (!target.contains(event.relatedTarget as Node | null)) {
    isDragging.value = false;
  }
};

const onDrop = (event: DragEvent) => {
  isDragging.value = false;
  const file = event.dataTransfer?.files?.[0];
  if (file) selectFile(file);
};

const selectFile = (file: File) => {
  const isPdf =
    file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf");
  if (!isPdf) {
    fileError.value = t("purchase.invoiceImport.errors.notPdf");
    return;
  }
  if (file.size > MAX_PDF_BYTES) {
    fileError.value = t("purchase.invoiceImport.errors.tooLarge");
    return;
  }
  fileError.value = null;
  pdfFile.value = file;
  void extract(file);
};

const formatBytes = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
};

// ---- Extraction ----

const extract = async (file: File) => {
  isExtracting.value = true;
  extractionError.value = null;
  draft.value = null;

  const result = await PurchaseService.PurchaseInvoiceIngestion.ingest(file);
  // A newer file may have been chosen while this one was being read.
  if (pdfFile.value !== file) return;
  isExtracting.value = false;

  if (!result.ok) {
    extractionError.value = result.error;
    return;
  }
  void applyDraft(result.draft);
};

const applyDraft = async (response: IngestPurchaseInvoiceResponse) => {
  const invoice = invoiceStore.setFromIngestion(response);
  if (!invoice) return;
  invoiceStore.applyNewInvoiceDefaults();

  const supplier = masterDataStore.masterData.suppliers?.find(
    (s) => s.id === invoice.supplierId,
  );
  if (supplier?.paymentMethodId) {
    invoice.paymentMethodId = supplier.paymentMethodId;
  }

  importIdsByRow.value = invoice.purchaseInvoiceImports.map((row) => row.id);
  reviewedImportIds.value = new Set();
  dismissedFields.value = new Set();
  duplicateOf.value = null;
  computedTotal.value = 0;
  draft.value = response;

  // The form mounts with the draft; compute totals and due dates right away.
  await nextTick();
  await formRef.value?.calcAmountsNow();
};

// ---- Review hints ----

const fieldWarnings = computed<Record<string, string[]>>(() => {
  const warnings: Record<string, string[]> = {};
  for (const issue of draft.value?.issues ?? []) {
    // Tax rows are flagged in the table; the total is compared live above the form.
    if (issue.field === "taxBreakdown" || issue.field === "netAmount") continue;
    if (dismissedFields.value.has(issue.field)) continue;
    (warnings[issue.field] ??= []).push(issue.message);
  }
  return warnings;
});

const rowWarnings = computed<Record<string, string[]>>(() => {
  const warnings: Record<string, string[]> = {};
  for (const issue of draft.value?.issues ?? []) {
    if (issue.field !== "taxBreakdown" || issue.rowIndex == null) continue;
    const id = importIdsByRow.value[issue.rowIndex];
    if (!id || reviewedImportIds.value.has(id)) continue;
    (warnings[id] ??= []).push(issue.message);
  }
  return warnings;
});

// The rate read from the PDF, shown while a line still has no tax.
const pendingTaxRates = computed<Record<string, number>>(() =>
  Object.fromEntries(
    (draft.value?.taxBreakdown ?? []).flatMap((row, index) => {
      const id = importIdsByRow.value[index];
      return id && !row.taxId ? [[id, row.taxRate]] : [];
    }),
  ),
);

const issueLabels = computed<Record<string, string>>(() => ({
  supplierId: t("purchase.fields.supplier"),
  supplierNumber: t("purchase.fields.supplierInvoiceNumber"),
  purchaseInvoiceDate: t("purchase.fields.invoiceDate"),
  extraTaxPercentatge: t("purchase.fields.withholdingTax"),
  netAmount: t("purchase.invoiceImport.totals.document"),
  taxBreakdown: t("purchase.invoiceImport.review.taxBreakdown"),
}));

const issueLabel = (issue: IngestionIssue): string =>
  issue.field === "taxBreakdown" && issue.rowIndex != null
    ? t("purchase.invoiceImport.review.taxRow", { row: issue.rowIndex + 1 })
    : (issueLabels.value[issue.field] ?? issue.field);

const totalMatches = computed(
  () =>
    draft.value?.totalAmount != null &&
    Math.abs(computedTotal.value - draft.value.totalAmount) <= TOTAL_TOLERANCE,
);

const onCalculated = (values: Partial<PurchaseInvoiceCalculatedValues>) => {
  if (values.netAmount !== undefined) computedTotal.value = values.netAmount;
};

const onDueDatesChange = (dueDates: PurchaseInvoiceDueDate[]) => {
  if (purchaseInvoice.value) {
    purchaseInvoice.value.purchaseInvoiceDueDates = dueDates;
  }
};

// Opens in a new tab so the draft and the dropped PDF are kept.
const invoiceHref = (id: string): string =>
  router.resolve({ name: "PurchaseInvoice", params: { id } }).href;

// ---- Supplier quick creation ----

const openSupplierDialog = async () => {
  await suppliersStore.fetchSupplierTypes();
  const name = draft.value?.supplierName ?? "";
  supplierDraft.value = {
    ...buildNewSupplier(getNewUuid()),
    comercialName: name,
    taxName: name,
    vatNumber: draft.value?.supplierVatNumber ?? "",
  };
  isSupplierDialogVisible.value = true;
};

const onSupplierSubmit = async (supplier: Supplier) => {
  if (isCreatingSupplier.value) return;
  isCreatingSupplier.value = true;
  try {
    // A rejected creation (e.g. an existing name, 409) is reported by the API client.
    const created = await suppliersStore
      .createSupplier(supplier)
      .catch(() => false);
    if (!created) return;

    await masterDataStore.fetchMasterData();
    formRef.value?.setSupplier(supplier.id);
    dismissedFields.value = new Set(dismissedFields.value).add("supplierId");
    isSupplierDialogVisible.value = false;
    toast.add({
      severity: "success",
      summary: t("purchase.invoiceImport.messages.supplierCreated"),
      life: 4000,
    });
  } finally {
    isCreatingSupplier.value = false;
  }
};

// ---- Tax breakdown rows (kept locally until the invoice is created) ----

const openImportForm = (mode: FormActionMode, row: PurchaseInvoiceImport) => {
  if (!purchaseInvoice.value) return;
  selectedImport.value = {
    ...row,
    id: mode === FormActionMode.CREATE ? getNewUuid() : row.id,
    purchaseInvoiceId: purchaseInvoice.value.id,
  };
  importFormMode.value = mode;
  isImportDialogVisible.value = true;
};

const onImportSubmit = (row: PurchaseInvoiceImport) => {
  if (!purchaseInvoice.value) return;
  const rows = purchaseInvoice.value.purchaseInvoiceImports;
  const index = rows.findIndex((item) => item.id === row.id);
  if (index >= 0) rows.splice(index, 1, row);
  else rows.push(row);

  reviewedImportIds.value = new Set(reviewedImportIds.value).add(row.id);
  isImportDialogVisible.value = false;
  formRef.value?.calcAmounts();
};

const deleteImport = (row: PurchaseInvoiceImport) => {
  if (!purchaseInvoice.value) return;
  purchaseInvoice.value.purchaseInvoiceImports =
    purchaseInvoice.value.purchaseInvoiceImports.filter(
      (item) => item.id !== row.id,
    );
  formRef.value?.calcAmounts();
};

// ---- Create ----

const onSubmit = async (invoice: PurchaseInvoice) => {
  if (!pdfFile.value || isSaving.value) return;
  isSaving.value = true;
  try {
    duplicateOf.value = null;
    const result = await invoiceStore.CreateChecked({
      ...invoice,
      purchaseInvoiceDate: convertDateTimeToJSON(invoice.purchaseInvoiceDate),
    });
    if (!result.ok) {
      if (result.duplicateOfId) {
        duplicateOf.value = { id: result.duplicateOfId, message: result.error };
      } else {
        toast.add({
          severity: "error",
          summary: t("purchase.purchaseInvoice.messages.createError"),
          detail: result.error || undefined,
          life: 6000,
        });
      }
      return;
    }

    const attached = await attachPdf(invoice.id, pdfFile.value);
    toast.add(
      attached
        ? {
            severity: "success",
            summary: t("purchase.invoiceImport.messages.created"),
            life: 4000,
          }
        : {
            severity: "warn",
            summary: t("purchase.purchaseInvoice.messages.created"),
            detail: t("purchase.invoiceImport.messages.attachError"),
            life: 8000,
          },
    );
    await router.replace({ name: "PurchaseInvoice", params: { id: invoice.id } });
  } finally {
    isSaving.value = false;
  }
};

const attachPdf = async (invoiceId: string, file: File): Promise<boolean> => {
  try {
    return (
      (await fileService.Upload(file, "PurchaseInvoice", invoiceId)) !==
      undefined
    );
  } catch {
    return false;
  }
};
</script>

<style scoped>
.invoice-import__file-input {
  display: none;
}

.invoice-import__dropzone {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  min-height: 22rem;
  padding: 2rem 1rem;
  border: 2px dashed var(--p-content-border-color);
  border-radius: 12px;
  background: var(--p-content-background);
  text-align: center;
  transition:
    border-color 0.15s ease,
    background 0.15s ease;
}

.invoice-import__dropzone--dragging {
  border-color: var(--p-primary-color);
  background: color-mix(in srgb, var(--p-primary-color) 6%, transparent);
}

.invoice-import__dropzone-icon {
  font-size: 3rem;
  color: var(--p-primary-color);
}

.invoice-import__dropzone-title {
  margin: 0;
  font-size: 1.25rem;
}

.invoice-import__dropzone-hint {
  margin: 0;
  max-width: 32rem;
  color: var(--p-text-muted-color);
}

.invoice-import__dropzone-error {
  margin-top: 0.5rem;
}

.invoice-import {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 1.25rem;
}

.invoice-import__document,
.invoice-import__review {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  min-width: 0;
}

.invoice-import__document-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: 0;
}

.invoice-import__document-header .pi-file-pdf {
  color: var(--p-primary-color);
}

.invoice-import__document-name {
  overflow: hidden;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.invoice-import__document-size {
  flex: 0 0 auto;
  margin-right: auto;
  color: var(--p-text-muted-color);
  font-size: 0.85rem;
}

.invoice-import__viewer {
  height: 60vh;
  min-height: 24rem;
}

.invoice-import__state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  min-height: 16rem;
  color: var(--p-text-muted-color);
  text-align: center;
}

.invoice-import__state strong {
  color: var(--p-text-color);
}

.invoice-import__error,
.invoice-import__issues {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.5rem;
}

.invoice-import__issues ul {
  margin: 0;
  padding-left: 1.1rem;
}

.invoice-import__issue-field {
  font-weight: 600;
}

.invoice-import__issue-action {
  margin-left: 0.25rem;
  padding-block: 0;
}

.invoice-import__issue-link {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  margin-left: 0.5rem;
  color: var(--p-primary-color);
  font-weight: 600;
}

.invoice-import__totals {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem 1.5rem;
  padding: 0.65rem 1rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
}

.invoice-import__totals--ok .invoice-import__totals-status {
  color: var(--p-green-600);
}

.invoice-import__totals--mismatch {
  border-color: var(--p-yellow-500);
}

.invoice-import__totals--mismatch .invoice-import__totals-status {
  color: var(--p-yellow-700);
}

.invoice-import__totals-status {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  margin-left: auto;
  font-weight: 600;
}

@media (min-width: 1200px) {
  .invoice-import {
    grid-template-columns: minmax(0, 5fr) minmax(0, 7fr);
    align-items: start;
  }

  .invoice-import__document {
    position: sticky;
    top: 0;
  }

  .invoice-import__viewer {
    height: calc(100vh - 12rem);
  }
}
</style>
