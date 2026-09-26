<template>
  <DataTable
    :value="invoices"
    :paginator="true"
    :rows="20"
    :loading="loading"
    dataKey="numSerieFactura"
    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
    :rowsPerPageOptions="[5, 10, 25]"
    :currentPageReportTemplate="t('verifactu.findInvoices.table.currentPageReport')"
    responsiveLayout="scroll"
    class="p-datatable-sm"
  >
    <template #header>
      <div class="flex justify-content-between align-items-center">
        <slot name="filter"></slot>
      </div>
    </template>

    <Column field="numSerieFactura" :header="t('verifactu.findInvoices.table.columns.number')" sortable>
      <template #body="{ data }">
        <span class="font-semibold">{{ data.numSerieFactura }}</span>
      </template>
    </Column>

    <Column field="fechaExpedicionFactura" :header="t('verifactu.findInvoices.table.columns.issueDate')" sortable />

    <Column field="tipoFactura" :header="t('verifactu.findInvoices.table.columns.type')" sortable />

    <Column field="importeTotal" :header="t('verifactu.findInvoices.table.columns.totalAmount')" sortable>
      <template #body="{ data }">
        <span class="font-semibold">{{
          formatCurrency(data.importeTotal)
        }}</span>
      </template>
    </Column>

    <Column field="cuotaTotal" :header="t('verifactu.findInvoices.table.columns.tax')" sortable>
      <template #body="{ data }">
        {{ formatCurrency(data.cuotaTotal) }}
      </template>
    </Column>

    <Column field="fechaHoraUsoRegistro" :header="t('verifactu.findInvoices.table.columns.registrationDate')" sortable>
      <template #body="{ data }">
        {{ formatDateTime(data.fechaHoraUsoRegistro) }}
      </template>
    </Column>

    <Column :header="t('verifactu.findInvoices.table.columns.hash')" style="width: 250px">
      <template #body="{ data }">
        <div class="text-overflow-ellipsis" :title="data.huella">
          {{ data.huella?.substring(0, 30) }}...
        </div>
      </template>
    </Column>

    <template #empty>
      <div class="text-center py-4">
        <p>{{ t("verifactu.findInvoices.table.empty") }}</p>
      </div>
    </template>
  </DataTable>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { VerifactuInvoice } from "../types";
import { formatCurrency, formatDateTime } from "../../../utils/functions";

const { t } = useI18n();

interface Props {
  invoices: VerifactuInvoice[];
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});
</script>

<style scoped>
.text-overflow-ellipsis {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 250px;
}
</style>
