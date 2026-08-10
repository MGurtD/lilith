<template>
  <TableVerifactuInvoices
    :invoices="verifactuStore.invoices"
    :loading="verifactuStore.loading"
  >
    <template #filter>
      <TableFilter
        :config="filterConfig"
        :body-width="filterBodyWidth"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :show-create="false"
        embedded
        @filter="searchInvoices"
        @clear="cleanFilter"
      />
    </template>
  </TableVerifactuInvoices>
</template>

<script setup lang="ts">
import TableVerifactuInvoices from "../components/TableVerifactuInvoices.vue";
import TableFilter, {
  type FilterBodyWidth,
  type FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useVerifactuStore } from "../store/verifactu";
import { computed, onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useUserFilterStore } from "../../../store/userfilter";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const toast = useToast();
const store = useStore();
const userFilterStore = useUserFilterStore();
const verifactuStore = useVerifactuStore();

const filter = ref({
  year: undefined as number | undefined,
  month: undefined as number | undefined,
});

const filterBodyWidth: FilterBodyWidth = {
  desktop: "36rem",
  tablet: "46rem",
};

// Generate year options from 2024 to current year
const currentYear = new Date().getFullYear();
const yearOptions = Array.from({ length: currentYear - 2023 }, (_, i) => ({
  value: 2024 + i,
  label: (2024 + i).toString(),
}));

// Month options
const monthOptions = computed(() =>
  [
    "january", "february", "march", "april", "may", "june",
    "july", "august", "september", "october", "november", "december",
  ].map((month, index) => ({
    value: index + 1,
    label: t(`verifactu.findInvoices.months.${month}`),
  })),
);

const filterConfig = computed<Array<FilterConfig>>(() => [
  {
    key: "year",
    label: t("verifactu.findInvoices.filters.year"),
    type: "select",
    options: yearOptions,
    placeholder: t("verifactu.findInvoices.filters.yearPlaceholder"),
    size: "md",
  },
  {
    key: "month",
    label: t("verifactu.findInvoices.filters.month"),
    type: "select",
    options: monthOptions.value,
    placeholder: t("verifactu.findInvoices.filters.monthPlaceholder"),
    size: "md",
  },
]);

onMounted(async () => {
  setCurrentMonth();
  getUserFilter();

  store.setMenuItem({
    icon: PrimeIcons.SHIELD,
    title: t("verifactu.findInvoices.title"),
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
      summary: t("verifactu.findInvoices.messages.incompleteFilter"),
      detail: t("verifactu.findInvoices.messages.selectYearAndMonth"),
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
        summary: t("verifactu.findInvoices.messages.noResults"),
        detail: t("verifactu.findInvoices.table.empty"),
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
      summary: t("common.error"),
      detail: t("verifactu.findInvoices.messages.searchError"),
      life: 5000,
    });
  }
};
</script>
