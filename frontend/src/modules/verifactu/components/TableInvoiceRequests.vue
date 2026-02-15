<template>
  <DataTable :value="value" :loading="loading">
    <Column
      field="createdOn"
      :header="
        $t('verifactu.invoiceIntegration.tableInvoiceRequests.columns.date')
      "
      sortable
    >
      <template #body="slotProps">
        {{ formatDateTime(slotProps.data.createdOn) }}
      </template>
    </Column>

    <Column
      field="request"
      :header="
        $t('verifactu.invoiceIntegration.tableInvoiceRequests.columns.request')
      "
      sortable
    >
      <template #body="slotProps">
        <div class="flex items-center gap-2">
          <span class="font-mono text-sm truncate max-w-72"
            >{{ (slotProps.data.request ?? "").substring(0, 60)
            }}{{
              (slotProps.data.request?.length || 0) > 60 ? "..." : ""
            }}</span
          >
          <Button
            icon="pi pi-copy"
            size="small"
            text
            rounded
            :aria-label="
              $t(
                'verifactu.invoiceIntegration.tableInvoiceRequests.actions.copy'
              )
            "
            @click="copyToClipboard(slotProps.data.request)"
          />
        </div>
      </template>
    </Column>

    <Column
      field="status"
      :header="
        $t(
          'verifactu.invoiceIntegration.tableInvoiceRequests.columns.statusCode'
        )
      "
      sortable
    />

    <Column
      field="response"
      :header="
        $t('verifactu.invoiceIntegration.tableInvoiceRequests.columns.response')
      "
      sortable
    >
      <template #body="slotProps">
        <div class="flex items-center gap-2">
          <span class="font-mono text-sm truncate max-w-72"
            >{{ (slotProps.data.response ?? "").substring(0, 60)
            }}{{
              (slotProps.data.response?.length || 0) > 60 ? "..." : ""
            }}</span
          >
          <Button
            icon="pi pi-copy"
            size="small"
            text
            rounded
            :aria-label="
              $t(
                'verifactu.invoiceIntegration.tableInvoiceRequests.actions.copy'
              )
            "
            @click="copyToClipboard(slotProps.data.response)"
          />
        </div>
      </template>
    </Column>

    <Column
      field="qrCodeBase64"
      :header="
        $t('verifactu.invoiceIntegration.tableInvoiceRequests.columns.qrImage')
      "
      headerStyle="width: 4rem"
    >
      <template #body="slotProps">
        <img
          v-if="slotProps.data.qrCodeBase64"
          :src="`${slotProps.data.qrCodeBase64}`"
          width="60"
          height="60"
          alt="QR"
        />
      </template>
    </Column>

    <Column field="qrCodeUrl" headerStyle="width: 6rem">
      <template #body="slotProps">
        <Button
          v-if="slotProps.data.qrCodeUrl"
          icon="pi pi-external-link"
          size="small"
          text
          rounded
          :aria-label="
            $t('verifactu.invoiceIntegration.tableInvoiceRequests.actions.open')
          "
          @click="openInNewTab(slotProps.data.qrCodeUrl)"
        />
      </template>
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import { formatDateTime } from "../../../utils/functions";

function copyToClipboard(text?: string) {
  if (!text) return;
  navigator.clipboard?.writeText(text).catch(() => {
    // Silently ignore if clipboard not available
  });
}

function openInNewTab(url?: string) {
  if (!url) return;
  window.open(url, "_blank");
}

// Props
withDefaults(
  defineProps<{
    value: any[];
    loading?: boolean;
  }>(),
  {
    value: () => [],
    loading: false,
  }
);
</script>
