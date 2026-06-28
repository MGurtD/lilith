<template>
  <div class="verifactu-invoice-integration">
    <DataTable
      :value="invoices"
      :loading="loading"
      dataKey="id"
      responsiveLayout="scroll"
    >
      <template #header>
        <TableFilter
          :config="[]"
          :body-width="filterBodyWidth"
          v-model="filters"
          :show-title="false"
          :show-action-labels="false"
          :show-filter-action="false"
          :show-create="false"
          embedded
          @clear="clearFilters"
        >
          <template #prepend>
            <div
              class="table-filter-prepend-field table-filter-prepend-field--md"
            >
              <label class="filter-label table-filter-prepend-label">{{
                $t("verifactu.invoiceIntegration.filters.toDate")
              }}</label>
              <DatePicker
                v-model="filters.limitDate"
                dateFormat="dd/mm/yy"
                :placeholder="
                  $t('verifactu.invoiceIntegration.filters.selectToDate')
                "
                showIcon
                class="w-full"
                size="small"
              />
            </div>
          </template>
          <template #append>
            <Button
              :label="
                $t('verifactu.invoiceIntegration.actions.integrateSelected')
              "
              :size="'small'"
              icon="pi pi-upload"
              @click="integrateVisibleInvoices"
              :disabled="!invoices.length || integrating"
              :loading="integrating"
            />
          </template>
        </TableFilter>
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
            class="result-row py-2 px-3 border-round border-1 mb-2"
            :class="r.status === 'success' ? 'result-row--ok' : 'result-row--error'"
          >
            <div class="flex align-items-center justify-content-between gap-2">
              <div class="flex align-items-center gap-2 min-w-0">
                <i
                  v-if="r.status === 'success'"
                  class="pi pi-check-circle text-green-600"
                  style="font-size: 1.1rem"
                ></i>
                <i
                  v-else
                  class="pi pi-times-circle text-red-600"
                  style="font-size: 1.1rem"
                ></i>
                <span class="font-semibold">{{ r.invoiceNumber }}</span>
                <Tag
                  v-if="r.status === 'error' && r.statusRegister"
                  :value="r.statusRegister"
                  severity="danger"
                  class="ml-1"
                />
              </div>
              <small
                v-if="r.message"
                class="text-color-secondary text-right ml-2"
                style="max-width: 60%"
              >
                {{ r.message }}
              </small>
            </div>
            <div
              v-if="r.status === 'error' && r.responseXml"
              class="mt-2"
            >
              <Button
                :label="
                  expandedResponses.has(r.id)
                    ? $t('common.hideDetails') || 'Amagar detalls'
                    : $t('common.showDetails') || 'Mostrar detalls'
                "
                :icon="
                  expandedResponses.has(r.id)
                    ? 'pi pi-chevron-up'
                    : 'pi pi-chevron-down'
                "
                text
                size="small"
                severity="secondary"
                @click="toggleResponseDetails(r.id)"
              />
              <pre
                v-if="expandedResponses.has(r.id)"
                class="response-xml mt-2 p-2 border-round border-1 surface-border text-xs"
              >{{ r.responseXml }}</pre>
            </div>
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
import Tag from "primevue/tag";
import TableFilter, {
  type FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";
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
    statusRegister?: string;
    responseXml?: string;
  }>
>([]);
const expandedResponses = ref<Set<number | string>>(new Set());

const toggleResponseDetails = (id: number | string) => {
  const next = new Set(expandedResponses.value);
  if (next.has(id)) {
    next.delete(id);
  } else {
    next.add(id);
  }
  expandedResponses.value = next;
};
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
const filterBodyWidth: FilterBodyWidth = {
  desktop: "25%",
  tablet: "33%",
};

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

const clearFilters = () => {
  filters.value.limitDate = new Date();
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
        const content = (response as any)?.content ?? {};
        const integrationSucceeded =
          response?.result === true && content?.success !== false;

        if (integrationSucceeded) {
          batchResults.value.push({
            id: inv.id,
            invoiceNumber: String(inv.invoiceNumber ?? ""),
            status: "success",
            statusRegister: content?.status,
          });

          progress.value.current += 1;
        } else {
          const errMsg =
            response?.errors?.[0] ??
            content?.errorMessage ??
            "Integration failed";
          batchResults.value.push({
            id: inv.id,
            invoiceNumber: String(inv.invoiceNumber ?? ""),
            status: "error",
            message: errMsg,
            statusRegister: content?.status,
            responseXml: content?.response,
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
.result-row {
  transition: background-color 0.15s ease;
}

.result-row--ok {
  background-color: var(--p-green-50, #ecfdf5);
  border-color: var(--p-green-200, #a7f3d0);
}

.result-row--error {
  background-color: var(--p-red-50, #fef2f2);
  border-color: var(--p-red-300, #fca5a5);
}

.response-xml {
  white-space: pre-wrap;
  word-break: break-word;
  max-height: 18rem;
  overflow: auto;
  background: var(--p-surface-50, #f8fafc);
  font-family:
    ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, monospace;
}
</style>
