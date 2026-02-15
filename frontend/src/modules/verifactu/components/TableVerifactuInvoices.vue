<template>
  <DataTable
    :value="invoices"
    :paginator="true"
    :rows="20"
    :loading="loading"
    dataKey="numSerieFactura"
    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
    :rowsPerPageOptions="[5, 10, 25]"
    currentPageReportTemplate="Mostrant {first} a {last} de {totalRecords} factures"
    responsiveLayout="scroll"
    class="p-datatable-sm"
  >
    <template #header>
      <div class="flex justify-content-between align-items-center">
        <slot name="filter"></slot>
      </div>
    </template>

    <Column field="numSerieFactura" header="Número" sortable>
      <template #body="{ data }">
        <span class="font-semibold">{{ data.numSerieFactura }}</span>
      </template>
    </Column>

    <Column field="fechaExpedicionFactura" header="Data expedició" sortable />

    <Column field="tipoFactura" header="Tipus" sortable />

    <Column field="importeTotal" header="Import total" sortable>
      <template #body="{ data }">
        <span class="font-semibold">{{
          formatCurrency(data.importeTotal)
        }}</span>
      </template>
    </Column>

    <Column field="cuotaTotal" header="IVA" sortable>
      <template #body="{ data }">
        {{ formatCurrency(data.cuotaTotal) }}
      </template>
    </Column>

    <Column field="fechaHoraUsoRegistro" header="Data registre" sortable>
      <template #body="{ data }">
        {{ formatDateTime(data.fechaHoraUsoRegistro) }}
      </template>
    </Column>

    <Column header="Huella" style="width: 250px">
      <template #body="{ data }">
        <div class="text-overflow-ellipsis" :title="data.huella">
          {{ data.huella?.substring(0, 30) }}...
        </div>
      </template>
    </Column>

    <template #empty>
      <div class="text-center py-4">
        <p>No s'han trobat factures per al període seleccionat</p>
      </div>
    </template>
  </DataTable>
</template>

<script setup lang="ts">
import { VerifactuInvoice } from "../types";
import { formatCurrency, formatDateTime } from "../../../utils/functions";

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
