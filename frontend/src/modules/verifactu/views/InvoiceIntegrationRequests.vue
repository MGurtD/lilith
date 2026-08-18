<template>
  <div class="verifactu-integration-requests">
    <Table
      :items="filteredRequests"
      :columns="columns"
      :filter-config="filterConfig"
      v-model:filter-values="filters"
      :filter-body-width="filterBodyWidth"
      :show-filter-action="false"
      :show-create="false"
      :loading="loading"
      dataKey="rowKey"
      responsiveLayout="scroll"
      @clear="clearFilters"
    >
      <template #body-invoiceNumber="{ data }">
        <span class="font-semibold">{{ data.invoiceNumber }}</span>
      </template>
      <template #body-request="{ data }">
        <div class="flex items-center gap-2">
          <span
            class="font-mono text-sm truncate max-w-72"
            :title="data.request"
          >
            {{ (data.request ?? "").substring(0, 60)
            }}{{ (data.request?.length || 0) > 60 ? "..." : "" }}
          </span>
          <Button
            icon="pi pi-copy"
            size="small"
            text
            rounded
            :aria-label="
              $t(
                'verifactu.invoiceIntegration.tableInvoiceRequests.actions.copy',
              )
            "
            @click="copyToClipboard(data.request)"
          />
        </div>
      </template>
      <template #body-success="{ data }">
        <Tag
          :value="
            data.success
              ? t('verifactu.integrationRequests.status.success')
              : t('verifactu.integrationRequests.status.error')
          "
          :severity="data.success ? 'success' : 'danger'"
        />
      </template>
      <template #body-response="{ data }">
        <div class="flex items-center gap-2">
          <span
            class="font-mono text-sm truncate max-w-72"
            :title="data.response"
          >
            {{ (data.response ?? "").substring(0, 60)
            }}{{ (data.response?.length || 0) > 60 ? "..." : "" }}
          </span>
          <Button
            icon="pi pi-copy"
            size="small"
            text
            rounded
            :aria-label="
              $t(
                'verifactu.invoiceIntegration.tableInvoiceRequests.actions.copy',
              )
            "
            @click="copyToClipboard(data.response)"
          />
        </div>
      </template>
      <template #body-qrCode="{ data }">
        <img
          class="cursor-pointer"
          v-if="data.qrCodeBase64"
          :src="data.qrCodeBase64"
          :alt="t('verifactu.integrationRequests.table.qrCodeAlt')"
          style="height: 45px"
          @click="openQr(data.qrCodeUrl)"
        />
        <span v-else>-</span>
      </template>
      <template #body-actions="{ data }">
        <div class="flex items-center gap-2">
          <Button
            icon="pi pi-eye"
            size="small"
            text
            rounded
            :aria-label="$t('verifactu.integrationRequests.actions.viewDetail')"
            :title="$t('verifactu.integrationRequests.actions.viewDetail')"
            @click="openDetail(data)"
          />
          <Button
            v-if="!data.success"
            icon="pi pi-refresh"
            size="small"
            text
            rounded
            severity="warn"
            :loading="resendingId === data.invoiceId"
            :disabled="resendingId === data.invoiceId"
            :aria-label="$t('verifactu.integrationRequests.actions.resend')"
            :title="$t('verifactu.integrationRequests.actions.resend')"
            @click="confirmResend(data)"
          />
        </div>
      </template>
      <template #empty>
        <div class="text-center p-4">
          <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
          <p class="text-gray-500">
            {{ $t("verifactu.integrationRequests.table.empty") }}
          </p>
        </div>
      </template>
    </Table>

    <Dialog
      v-model:visible="detailDialogVisible"
      :modal="true"
      :header="$t('verifactu.integrationRequests.detailDialog.title')"
      :style="{ width: '60vw' }"
      :maximizable="true"
      :draggable="false"
    >
      <div v-if="selectedRequest" class="verifactu-detail">
        <div class="verifactu-detail-grid">
          <div>
            <span class="verifactu-detail-label">{{
              $t("verifactu.integrationRequests.table.columns.invoiceNumber")
            }}</span>
            <span class="verifactu-detail-value">{{
              selectedRequest.invoiceNumber
            }}</span>
          </div>
          <div>
            <span class="verifactu-detail-label">{{
              $t("verifactu.integrationRequests.table.columns.customer")
            }}</span>
            <span class="verifactu-detail-value">{{
              selectedRequest.customerComercialName
            }}</span>
          </div>
          <div>
            <span class="verifactu-detail-label">{{
              $t("verifactu.integrationRequests.table.columns.date")
            }}</span>
            <span class="verifactu-detail-value">{{
              formatDate(selectedRequest.timestampResponse)
            }}</span>
          </div>
          <div>
            <span class="verifactu-detail-label">{{
              $t("verifactu.integrationRequests.table.columns.success")
            }}</span>
            <Tag
              :value="
                selectedRequest.success
                  ? t('verifactu.integrationRequests.status.success')
                  : t('verifactu.integrationRequests.status.error')
              "
              :severity="selectedRequest.success ? 'success' : 'danger'"
            />
          </div>
        </div>

        <div class="verifactu-detail-section">
          <div class="verifactu-detail-header">
            <span>{{
              $t("verifactu.integrationRequests.table.columns.request")
            }}</span>
            <Button
              icon="pi pi-copy"
              size="small"
              text
              rounded
              :aria-label="
                $t(
                  'verifactu.invoiceIntegration.tableInvoiceRequests.actions.copy',
                )
              "
              @click="copyToClipboard(selectedRequest.request)"
            />
          </div>
          <pre class="verifactu-detail-pre">{{
            selectedRequest.request ?? ""
          }}</pre>
        </div>

        <div class="verifactu-detail-section">
          <div class="verifactu-detail-header">
            <span>{{
              $t("verifactu.integrationRequests.table.columns.response")
            }}</span>
            <Button
              icon="pi pi-copy"
              size="small"
              text
              rounded
              :aria-label="
                $t(
                  'verifactu.invoiceIntegration.tableInvoiceRequests.actions.copy',
                )
              "
              @click="copyToClipboard(selectedRequest.response)"
            />
          </div>
          <pre class="verifactu-detail-pre">{{
            selectedRequest.response ?? ""
          }}</pre>
        </div>
      </div>
      <template #footer>
        <Button
          :label="$t('common.close')"
          icon="pi pi-times"
          @click="detailDialogVisible = false"
        />
      </template>
    </Dialog>

    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import Tag from "primevue/tag";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import ConfirmDialog from "primevue/confirmdialog";
import { useConfirm } from "primevue/useconfirm";
import {
  type FilterBodyWidth,
  type FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import Table from "../../../components/tables/Table.vue";
import { ColumnType, type Column } from "../../../components/tables/types";
import { useVerifactuStore } from "../store/verifactu";
import { useStore } from "../../../store";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import { formatDate } from "../../../utils/functions";

const { t } = useI18n();
const toast = useToast();
const confirm = useConfirm();
const store = useStore();
const verifactuStore = useVerifactuStore();

// Page title
store.setMenuItem({
  title: t("verifactu.integrationRequests.title"),
  icon: PrimeIcons.HISTORY,
});

const { integrationsBetweenDates, loading } = storeToRefs(verifactuStore);

// Filters
const filters = ref<{
  dates: Array<Date> | null;
  searchQuery: string;
}>({
  dates: null,
  searchQuery: "",
});
const filterBodyWidth: FilterBodyWidth = {
  desktop: "58%",
  tablet: "76%",
};
const filterConfig = computed<Array<FilterConfig>>(() => [
  {
    key: "dates",
    label: t("common.period"),
    type: "date-range",
    placeholder: t("verifactu.integrationRequests.filters.periodPlaceholder"),
    size: "lg",
  },
  {
    key: "searchQuery",
    label: t("common.search"),
    type: "text",
    placeholder: `${t("common.search")} ...`,
    size: "lg",
  },
]);

const columns = computed<Column[]>(() => [
  {
    field: "invoiceNumber",
    header: t("verifactu.integrationRequests.table.columns.invoiceNumber"),
    sortable: true,
  },
  {
    field: "customerComercialName",
    header: t("verifactu.integrationRequests.table.columns.customer"),
    sortable: true,
  },
  {
    field: "timestampResponse",
    header: t("verifactu.integrationRequests.table.columns.date"),
    columnType: ColumnType.Date,
    sortable: true,
  },
  {
    field: "request",
    header: t("verifactu.integrationRequests.table.columns.request"),
  },
  {
    field: "success",
    header: t("verifactu.integrationRequests.table.columns.success"),
  },
  {
    field: "status",
    header: t("verifactu.integrationRequests.table.columns.statusCode"),
  },
  {
    field: "response",
    header: t("verifactu.integrationRequests.table.columns.response"),
  },
  {
    field: "qrCode",
    header: t("verifactu.integrationRequests.table.columns.qrCode"),
  },
  {
    field: "actions",
    header: t("verifactu.integrationRequests.table.columns.actions"),
    style: "width: 12rem",
  },
]);

// Each sales invoice can have multiple Verifactu requests -> flatten for the table
const flattenedRequests = computed(() => {
  const list = integrationsBetweenDates.value || [];
  return list.flatMap((inv: any) => {
    const reqs =
      inv?.verifactuRequests ||
      inv?.VerifactuRequests ||
      inv?.salesInvoiceVerifactuRequests ||
      inv?.SalesInvoiceVerifactuRequests ||
      [];
    return (reqs as any[]).map((r: any, idx: number) => ({
      rowKey: `${inv?.id || inv?.invoiceId || "inv"}-${r?.id || idx}`,
      invoiceId: inv?.id || inv?.invoiceId,
      invoiceNumber: inv?.invoiceNumber,
      customerComercialName:
        inv?.customerComercialName || inv?.customerTaxName || "",
      timestampResponse: r?.timestampResponse,
      request: r?.request,
      success: r?.success,
      status: r?.status,
      response: r?.response,
      qrCodeUrl: r?.qrCodeUrl,
      qrCodeBase64: r?.qrCodeBase64,
    }));
  });
});

// Client-side filter for invoiceNumber or customerComercialName
const filteredRequests = computed(() => {
  const q = (filters.value.searchQuery || "").toLowerCase().trim();
  if (!q) return flattenedRequests.value;
  return flattenedRequests.value.filter((row: any) => {
    const inv = (row.invoiceNumber || "").toString().toLowerCase();
    const cust = (row.customerComercialName || "").toString().toLowerCase();
    return inv.includes(q) || cust.includes(q);
  });
});

const isRangeValid = () => {
  const dates = filters.value.dates;
  return (
    !!dates &&
    dates.length === 2 &&
    !!dates[0] &&
    !!dates[1] &&
    dates[0] <= dates[1]
  );
};

const loadRequests = async () => {
  if (!isRangeValid()) return;
  try {
    await verifactuStore.GetIntegrationsBetweenDates(
      filters.value.dates?.[0] as Date,
      filters.value.dates?.[1] as Date,
    );
  } catch (err) {
    console.error("Error loading integration requests:", err);
    toast.add({
      severity: "error",
      summary: t("common.error"),
      detail: t("verifactu.integrationRequests.messages.loadError"),
      life: 5000,
    });
  }
};

const clearFilters = () => {
  filters.value.dates = null;
  filters.value.searchQuery = "";
};

// Also react on model changes (manual typing)
watch(
  () => filters.value.dates,
  () => {
    if (isRangeValid()) {
      loadRequests();
    }
  },
  { deep: true },
);

onMounted(() => {
  // Default to last week
  const to = new Date();
  const from = new Date();
  from.setDate(to.getDate() - 7);
  filters.value.dates = [from, to];
  loadRequests();
});

const openQr = (url?: string) => {
  if (!url) return;
  window.open(url, "_blank", "noopener,noreferrer");
};

function copyToClipboard(text?: string) {
  if (!text) return;
  navigator.clipboard?.writeText(text).catch(() => {
    // ignore if clipboard not available
  });
}

// Detail dialog state
const detailDialogVisible = ref(false);
const selectedRequest = ref<any | null>(null);

const openDetail = (row: any) => {
  selectedRequest.value = row;
  detailDialogVisible.value = true;
};

// Resend state
const resendingId = ref<string | null>(null);

const performResend = async (invoiceId: string) => {
  resendingId.value = invoiceId;
  try {
    const response = await verifactuStore.ResendToVerifactu(invoiceId);
    if (response?.result) {
      toast.add({
        severity: "success",
        summary: t("verifactu.integrationRequests.messages.resendSuccess"),
        detail: t("verifactu.integrationRequests.messages.resendSuccessDetail"),
        life: 5000,
      });
      await loadRequests();
    } else {
      const errorMessage =
        response?.errors && response.errors.length > 0
          ? response.errors.join(", ")
          : t("verifactu.integrationRequests.messages.resendError");
      toast.add({
        severity: "error",
        summary: t("common.error"),
        detail: errorMessage,
        life: 7000,
      });
    }
  } catch (err: any) {
    console.error("Error resending invoice to Verifactu:", err);
    toast.add({
      severity: "error",
      summary: t("common.error"),
      detail:
        err?.message || t("verifactu.integrationRequests.messages.resendError"),
      life: 7000,
    });
  } finally {
    resendingId.value = null;
  }
};

const confirmResend = (row: any) => {
  if (!row?.invoiceId) return;
  confirm.require({
    message: t("verifactu.integrationRequests.messages.resendConfirm", {
      number: row.invoiceNumber,
    }),
    header: t("verifactu.integrationRequests.messages.resendHeader"),
    icon: "pi pi-exclamation-triangle",
    acceptLabel: t("common.accept"),
    rejectLabel: t("common.cancel"),
    accept: () => performResend(row.invoiceId),
  });
};
</script>

<style scoped>
.truncate-text {
  max-width: 320px;
  display: inline-block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.verifactu-detail {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.verifactu-detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem 1rem;
}

.verifactu-detail-grid > div {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.verifactu-detail-label {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

.verifactu-detail-value {
  font-weight: 600;
}

.verifactu-detail-section {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.verifactu-detail-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 600;
  color: var(--p-text-color);
}

.verifactu-detail-pre {
  background: var(--p-surface-100, #f1f5f9);
  border: 1px solid var(--p-content-border-color);
  border-radius: 6px;
  padding: 0.75rem;
  font-family: var(--p-font-mono, monospace);
  font-size: 0.8rem;
  max-height: 280px;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-word;
  margin: 0;
}

@media (max-width: 640px) {
  .verifactu-detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>
