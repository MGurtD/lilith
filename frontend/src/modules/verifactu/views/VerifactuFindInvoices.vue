<template>
  <TableVerifactuInvoices
    :invoices="verifactuStore.invoices"
    :loading="verifactuStore.loading"
  >
    <template #filter>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-1"
      >
        <div class="datatable-filter">
          <div class="filter-field m-1">
            <label class="block text-900">Any</label>
            <Select
              v-model="filter.year"
              :options="yearOptions"
              optionValue="value"
              optionLabel="label"
              class="w-full ml-2"
              placeholder="Selecciona un any"
            />
          </div>
          <div class="filter-field m-1">
            <label class="block text-900">Mes</label>
            <Select
              v-model="filter.month"
              :options="monthOptions"
              optionValue="value"
              optionLabel="label"
              class="w-full ml-1"
              placeholder="Selecciona un mes"
            />
          </div>
        </div>
      </div>
      <div class="datatable-buttons" style="margin-left: auto">
        <Button
          class="datatable-button mr-2"
          :icon="PrimeIcons.FILTER"
          rounded
          raised
          @click="searchInvoices"
          :disabled="!filter.year || !filter.month"
        />
        <Button
          class="datatable-button mr-2"
          :icon="PrimeIcons.FILTER_SLASH"
          rounded
          raised
          @click="cleanFilter"
        />
      </div>
    </template>
  </TableVerifactuInvoices>
</template>

<script setup lang="ts">
import TableVerifactuInvoices from "../components/TableVerifactuInvoices.vue";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useVerifactuStore } from "../store/verifactu";
import { onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useUserFilterStore } from "../../../store/userfilter";

const toast = useToast();
const store = useStore();
const userFilterStore = useUserFilterStore();
const verifactuStore = useVerifactuStore();

const filter = ref({
  year: undefined as number | undefined,
  month: undefined as number | undefined,
});

// Generate year options from 2024 to current year
const currentYear = new Date().getFullYear();
const yearOptions = Array.from({ length: currentYear - 2023 }, (_, i) => ({
  value: 2024 + i,
  label: (2024 + i).toString(),
}));

// Month options
const monthOptions = [
  { value: 1, label: "Gener" },
  { value: 2, label: "Febrer" },
  { value: 3, label: "Març" },
  { value: 4, label: "Abril" },
  { value: 5, label: "Maig" },
  { value: 6, label: "Juny" },
  { value: 7, label: "Juliol" },
  { value: 8, label: "Agost" },
  { value: 9, label: "Setembre" },
  { value: 10, label: "Octubre" },
  { value: 11, label: "Novembre" },
  { value: 12, label: "Desembre" },
];

onMounted(async () => {
  setCurrentMonth();
  getUserFilter();

  store.setMenuItem({
    icon: PrimeIcons.SHIELD,
    title: "Verifactu - Consulta de factures enviades",
  });
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("VerifactuInvoices", "");
  if (userFilter) {
    filter.value.year = userFilter.year;
    filter.value.month = userFilter.month;
  }
};

const setCurrentMonth = () => {
  const now = new Date();
  filter.value.year = now.getFullYear();
  filter.value.month = now.getMonth() + 1; // JavaScript months are 0-indexed
};

const cleanFilter = () => {
  filter.value.year = undefined;
  filter.value.month = undefined;
  verifactuStore.invoices = [];
};

const searchInvoices = async () => {
  if (!filter.value.year || !filter.value.month) {
    toast.add({
      severity: "warn",
      summary: "Filtre incomplet",
      detail: "Si us plau, selecciona any i mes",
      life: 3000,
    });
    return;
  }

  try {
    const response = await verifactuStore.FindInvoices(
      filter.value.month,
      filter.value.year,
    );

    if (response && response.invoices.length === 0) {
      toast.add({
        severity: "info",
        summary: "Sense resultats",
        detail: "No s'han trobat factures per al període seleccionat",
        life: 3000,
      });
    }

    // Save filter
    const savedFilter = {
      year: filter.value.year,
      month: filter.value.month,
    };
    userFilterStore.addFilter("VerifactuInvoices", "", savedFilter);
  } catch (error) {
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Error en la cerca de factures",
      life: 5000,
    });
  }
};
</script>
