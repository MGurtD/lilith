<template>
  <div class="verifactu-invoice-integration">
    <DataTable
      :value="invoices"
      :loading="loading"
      dataKey="id"
      responsiveLayout="scroll"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-1"
        >
          <div class="flex gap-2">
            <label class="form-label pr-1 auto-width-label">{{
              $t("verifactu.invoiceIntegration.filters.toDate")
            }}</label>
            <DatePicker
              v-model="filters.limitDate"
              dateFormat="dd/mm/yy"
              :placeholder="
                $t('verifactu.invoiceIntegration.filters.selectToDate')
              "
              showIcon
              class="date-input"
              @date-select="onLimitDateSelect"
            />
          </div>
          <div class="datatable-buttons">
            <Button
              :label="
                $t('verifactu.invoiceIntegration.actions.integrateSelected')
              "
              :size="'small'"
              icon="pi pi-upload"
              @click="integrateVisibleInvoices"
              :disabled="!invoices.length || integrating"
              :loading="integrating"
              class="filter-button"
            />
          </div>
        </div>
      </template>
      <Column
        field="invoiceNumber"
        :header="$t('verifactu.invoiceIntegration.table.columns.number')"
      >
        <template #body="slotProps">
          <span class="font-semibold">{{ slotProps.data.invoiceNumber }}</span>
        </template>
      </Column>

      <Column
        field="invoiceDate"
        :header="$t('verifactu.invoiceIntegration.table.columns.date')"
      >
        <template #body="slotProps">
          {{ formatDate(slotProps.data.invoiceDate) }}
        </template>
      </Column>

      <Column
        field="dueDate"
        :header="$t('verifactu.invoiceIntegration.table.columns.dueDate')"
      >
        <template #body="slotProps">
          {{ getLastDueDateFormatted(slotProps.data) }}
        </template>
      </Column>

      <Column
        field="customer.fiscalName"
        :header="$t('verifactu.invoiceIntegration.table.columns.customer')"
      >
        <template #body="slotProps">
          <div>
            <div class="font-semibold">
              {{
                slotProps.data.customerComercialName ||
                slotProps.data.customerTaxName
              }}
            </div>
            <div class="text-sm text-gray-500">
              {{ slotProps.data.customerVatNumber }}
            </div>
          </div>
        </template>
      </Column>

      <Column
        field="totalAmount"
        :header="$t('verifactu.invoiceIntegration.table.columns.amount')"
      >
        <template #body="slotProps">
          <span class="font-semibold">{{
            formatCurrency(slotProps.data.baseAmount + slotProps.data.taxAmount)
          }}</span>
        </template>
      </Column>

      <template #empty>
        <div class="text-center p-4">
          <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
          <p class="text-gray-500">
            {{ $t("verifactu.invoiceIntegration.table.empty") }}
          </p>
        </div>
      </template>
    </DataTable>

    <!-- Batch progress & results dialog -->
    <Dialog
      v-model:visible="batchDialogVisible"
      :modal="true"
      :closable="!isBatchRunning"
      :draggable="false"
      :style="{ width: '40rem' }"
      :header="t('verifactu.invoiceIntegration.title')"
    >
      <!-- Progress view -->
      <div v-if="isBatchRunning" class="flex flex-column gap-3">
        <div class="flex align-items-center justify-content-between">
          <span class="font-semibold"
            >{{ progress.current }} / {{ progress.total }}</span
          >
          <span class="text-sm text-color-secondary">{{
            progress.currentInvoiceNumber || "-"
          }}</span>
        </div>
        <ProgressBar :value="progressPercent" />
      </div>

      <!-- Results view -->
      <div v-else class="flex flex-column gap-3">
        <div class="flex align-items-center justify-content-between">
          <div>
            <span class="font-semibold">{{ successCount }}</span>
            <span class="ml-1">ok</span>
          </div>
          <div>
            <span class="font-semibold">{{ errorCount }}</span>
            <span class="ml-1">error</span>
          </div>
        </div>
        <div class="results-list">
          <div
            v-for="r in batchResults"
            :key="r.id"
            class="result-row flex align-items-center justify-content-between py-2 px-2 border-round surface-border border-1 mb-2"
          >
            <div class="flex align-items-center gap-2">
              <i
                v-if="r.status === 'success'"
                class="pi pi-check text-green-500"
              ></i>
              <i v-else class="pi pi-times text-red-500"></i>
              <span class="font-semibold">{{ r.invoiceNumber }}</span>
            </div>
            <small class="text-color-secondary">{{ r.message || "" }}</small>
          </div>
        </div>

        <div class="flex justify-content-end">
          <Button
            :label="t('common.close') || 'Close'"
            @click="batchDialogVisible = false"
          />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import DatePicker from "primevue/datepicker";
import Dialog from "primevue/dialog";
import ProgressBar from "primevue/progressbar";
import { useVerifactuStore } from "../store/verifactu";
import { useStore } from "../../../store";
import { formatDate, formatCurrency } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";

const { t } = useI18n();
const toast = useToast();
const verifactuStore = useVerifactuStore();
const store = useStore();

// Set page title in the store
store.setMenuItem({
  title: t("verifactu.invoiceIntegration.title"),
  icon: PrimeIcons.UPLOAD,
});

// Use store refs for reactivity
const { pendingInvoices, loading } = storeToRefs(verifactuStore);

// State
const integrating = ref(false);
const batchDialogVisible = ref(false);
const isBatchRunning = ref(false);
const batchResults = ref<
  Array<{
    id: number | string;
    invoiceNumber: string;
    status: "success" | "error";
    message?: string;
  }>
>([]);
const progress = ref<{
  total: number;
  current: number;
  currentInvoiceNumber?: string;
}>({
  total: 0,
  current: 0,
  currentInvoiceNumber: undefined,
});

const progressPercent = computed(() =>
  progress.value.total > 0
    ? Math.floor((progress.value.current / progress.value.total) * 100)
    : 0,
);
const successCount = computed(
  () => batchResults.value.filter((r) => r.status === "success").length,
);
const errorCount = computed(
  () => batchResults.value.filter((r) => r.status === "error").length,
);

// Filters
const filters = ref({
  limitDate: new Date(), // Date limit
});

// Computed for date validation
// no extra min/max constraints for single date filter

// Computed
const invoices = computed(() => pendingInvoices.value || []);

// Helpers
const naturalCompare = (a: string, b: string) =>
  (a || "").localeCompare(b || "", undefined, {
    numeric: true,
    sensitivity: "base",
  });

// Methods
// No range validation needed; single date
const validateDate = () => !!filters.value.limitDate;

const onLimitDateSelect = () => {
  if (validateDate()) {
    loadInvoices();
  }
};

const loadInvoices = async () => {
  // Validate date before loading
  if (!validateDate()) {
    return;
  }

  try {
    await verifactuStore.GetPendingIntegration(filters.value.limitDate);
  } catch (error) {
    console.error("Error loading invoices:", error);
    toast.add({
      severity: "error",
      summary: t("common.error"),
      detail: t("verifactu.invoiceIntegration.messages.loadError"),
      life: 5000,
    });
  }
};

const integrateVisibleInvoices = async () => {
  if (!invoices.value.length) return;
  integrating.value = true;
  batchResults.value = [];
  batchDialogVisible.value = true;
  isBatchRunning.value = true;

  // Order invoices by invoiceNumber using natural ordering (numeric-aware)
  const ordered = [...invoices.value].sort((a, b) =>
    naturalCompare(
      String(a.invoiceNumber ?? ""),
      String(b.invoiceNumber ?? ""),
    ),
  );

  progress.value.total = ordered.length;
  progress.value.current = 0;
  progress.value.currentInvoiceNumber = undefined;

  try {
    for (const inv of ordered) {
      progress.value.currentInvoiceNumber = String(inv.invoiceNumber ?? "");

      try {
        const response = await verifactuStore.SendToVerifactu(inv.id);
        if (response?.result) {
          batchResults.value.push({
            id: inv.id,
            invoiceNumber: String(inv.invoiceNumber ?? ""),
            status: "success",
          });

          progress.value.current += 1;
        } else {
          const errMsg = response?.errors?.join(", ") || "Integration failed";
          batchResults.value.push({
            id: inv.id,
            invoiceNumber: String(inv.invoiceNumber ?? ""),
            status: "error",
            message: errMsg,
          });
          // Stop on first error to preserve chain integrity
          break;
        }
      } catch (e: any) {
        const message = e?.message || "Unexpected error";
        batchResults.value.push({
          id: inv.id,
          invoiceNumber: String(inv.invoiceNumber ?? ""),
          status: "error",
          message,
        });
        // Stop on first error to preserve chain integrity
        break;
      }
    }
  } catch (error) {
    console.error("Error integrating invoices:", error);
  } finally {
    isBatchRunning.value = false;
    integrating.value = false;
    // Refresh list after processing
    await loadInvoices();
  }
};

// Removed status/actions related helpers

const getLastDueDateFormatted = (invoice: any) => {
  const dates = invoice?.salesInvoiceDueDates;
  if (Array.isArray(dates) && dates.length > 0) {
    const last = dates[dates.length - 1]?.dueDate;
    if (last) return formatDate(last);
  }
  return "-";
};

// Lifecycle
onMounted(() => {
  loadInvoices();
});

watch(
  () => filters.value.limitDate,
  () => {
    if (validateDate()) {
      loadInvoices();
    }
  },
);
</script>

<style scoped>
.auto-width-label {
  display: inline-flex;
  width: auto;
  white-space: nowrap;
  align-items: center;
}

.date-input {
  width: 100%;
}

.filter-button {
  height: 2.5rem;
  white-space: nowrap;
}
</style>
