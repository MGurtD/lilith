<template>
  <div class="verifactu-integration-requests">
    <DataTable
      :value="filteredRequests"
      :loading="loading"
      dataKey="rowKey"
      responsiveLayout="scroll"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-1"
        >
          <div class="datatable-filter flex gap-3 flex-wrap">
            <div class="flex align-items-center gap-2">
              <label class="auto-width-label">{{
                $t("verifactu.integrationRequests.filters.fromDate")
              }}</label>
              <DatePicker
                v-model="filters.fromDate"
                dateFormat="dd/mm/yy"
                showIcon
                @date-select="onDateSelect"
                inputId="fromDate"
              />
            </div>
            <div class="flex align-items-center gap-2">
              <label class="auto-width-label">{{
                $t("verifactu.integrationRequests.filters.toDate")
              }}</label>
              <DatePicker
                v-model="filters.toDate"
                dateFormat="dd/mm/yy"
                showIcon
                @date-select="onDateSelect"
                inputId="toDate"
              />
            </div>
          </div>
          <div class="datatable-buttons">
            <span class="p-input-icon-left">
              <InputText
                v-model="searchQuery"
                :placeholder="$t('common.search') + ' …'"
                class="w-18rem"
              />
            </span>
          </div>
        </div>
      </template>

      <Column
        field="invoiceNumber"
        :header="
          $t('verifactu.integrationRequests.table.columns.invoiceNumber')
        "
        sortable
      >
        <template #body="slotProps">
          <span class="font-semibold">{{ slotProps.data.invoiceNumber }}</span>
        </template>
      </Column>

      <Column
        field="customerComercialName"
        :header="$t('verifactu.integrationRequests.table.columns.customer')"
        sortable
      >
        <template #body="slotProps">
          {{ slotProps.data.customerComercialName }}
        </template>
      </Column>

      <Column
        field="timestampResponse"
        :header="$t('verifactu.integrationRequests.table.columns.date')"
        sortable
      >
        <template #body="slotProps">
          {{ formatDate(slotProps.data.timestampResponse) }}
        </template>
      </Column>

      <Column
        field="request"
        :header="$t('verifactu.integrationRequests.table.columns.request')"
      >
        <template #body="slotProps">
          <div class="flex items-center gap-2">
            <span
              class="font-mono text-sm truncate max-w-72"
              :title="slotProps.data.request"
            >
              {{ (slotProps.data.request ?? "").substring(0, 60)
              }}{{ (slotProps.data.request?.length || 0) > 60 ? "..." : "" }}
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
              @click="copyToClipboard(slotProps.data.request)"
            />
          </div>
        </template>
      </Column>

      <Column
        field="success"
        :header="$t('verifactu.integrationRequests.table.columns.success')"
      >
        <template #body="slotProps">
          <Tag
            :value="slotProps.data.success ? 'OK' : 'ERR'"
            :severity="slotProps.data.success ? 'success' : 'danger'"
          />
        </template>
      </Column>

      <Column
        field="status"
        :header="$t('verifactu.integrationRequests.table.columns.statusCode')"
      >
        <template #body="slotProps">
          {{ slotProps.data.status }}
        </template>
      </Column>

      <Column
        field="response"
        :header="$t('verifactu.integrationRequests.table.columns.response')"
      >
        <template #body="slotProps">
          <div class="flex items-center gap-2">
            <span
              class="font-mono text-sm truncate max-w-72"
              :title="slotProps.data.response"
            >
              {{ (slotProps.data.response ?? "").substring(0, 60)
              }}{{ (slotProps.data.response?.length || 0) > 60 ? "..." : "" }}
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
              @click="copyToClipboard(slotProps.data.response)"
            />
          </div>
        </template>
      </Column>

      <Column
        :header="$t('verifactu.integrationRequests.table.columns.qrCode')"
      >
        <template #body="slotProps">
          <img
            class="cursor-pointer"
            v-if="slotProps.data.qrCodeBase64"
            :src="slotProps.data.qrCodeBase64"
            alt="QR"
            style="height: 45px"
            @click="openQr(slotProps.data.qrCodeUrl)"
          />
          <span v-else>-</span>
        </template>
      </Column>

      <template #empty>
        <div class="text-center p-4">
          <i class="pi pi-inbox text-4xl text-gray-400 mb-3"></i>
          <p class="text-gray-500">
            {{ $t("verifactu.integrationRequests.table.empty") }}
          </p>
        </div>
      </template>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import DatePicker from "primevue/datepicker";
import Tag from "primevue/tag";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import { useVerifactuStore } from "../store/verifactu";
import { useStore } from "../../../store";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import { formatDate } from "../../../utils/functions";

const { t } = useI18n();
const toast = useToast();
const store = useStore();
const verifactuStore = useVerifactuStore();

// Page title
store.setMenuItem({
  title: t("verifactu.integrationRequests.title"),
  icon: PrimeIcons.HISTORY,
});

const { integrationsBetweenDates, loading } = storeToRefs(verifactuStore);

// Filters
const filters = ref<{ fromDate: Date | null; toDate: Date | null }>({
  fromDate: null,
  toDate: null,
});

// Search box state
const searchQuery = ref("");

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
  const q = (searchQuery.value || "").toLowerCase().trim();
  if (!q) return flattenedRequests.value;
  return flattenedRequests.value.filter((row: any) => {
    const inv = (row.invoiceNumber || "").toString().toLowerCase();
    const cust = (row.customerComercialName || "").toString().toLowerCase();
    return inv.includes(q) || cust.includes(q);
  });
});

const isRangeValid = () => {
  const { fromDate, toDate } = filters.value;
  return !!fromDate && !!toDate && fromDate <= toDate;
};

const loadRequests = async () => {
  if (!isRangeValid()) return;
  try {
    await verifactuStore.GetIntegrationsBetweenDates(
      filters.value.fromDate as Date,
      filters.value.toDate as Date,
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

const onDateSelect = () => {
  // Auto-refresh after both dates selected
  if (isRangeValid()) {
    loadRequests();
  }
};

// Also react on model changes (manual typing)
watch(
  () => [filters.value.fromDate, filters.value.toDate],
  () => {
    if (isRangeValid()) {
      loadRequests();
    }
  },
);

onMounted(() => {
  // Default to last week
  const to = new Date();
  const from = new Date();
  from.setDate(to.getDate() - 7);
  filters.value.fromDate = from;
  filters.value.toDate = to;
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
</script>

<style scoped>
.auto-width-label {
  display: inline-flex;
  width: auto;
  white-space: nowrap;
  align-items: center;
}

.truncate-text {
  max-width: 320px;
  display: inline-block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
