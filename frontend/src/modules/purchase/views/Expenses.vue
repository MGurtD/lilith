<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="filteredExpenses"
    :filter-config="[]"
    :filter-labels="filterMetadata.filterLabels"
    :filter-value-resolvers="filterMetadata.filterValueResolvers"
    v-model:filter-values="filter"
    :filter-body-width="filterBodyWidth"
    page="Expenses"
    class="p-datatable-sm small-datatable"
    tableStyle="min-width: 100%"
    sortMode="multiple"
    paginator
    :rows="25"
    delete-column-width="5%"
    show-delete-column
    @filter="filterExpense"
    @clear="clearFilter"
    @create="createButtonClick"
    @delete="deleteExpense"
    @row-click="editExpense"
  >
    <template #prepend>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label"
          >{{ t("purchase.fields.period") }}</label
        >
        <DatePicker
          v-model="filter.dates"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          :placeholder="t('purchase.placeholders.selectPeriod')"
          showIcon
          class="w-full"
          size="small"
        />
      </div>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--md"
      >
        <label class="filter-label table-filter-prepend-label">{{ t("purchase.fields.type") }}</label>
        <Select
          v-model="filter.expenseTypeId"
          :options="expenseStore.expenseTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :placeholder="t('purchase.placeholders.allExpenseTypes')"
          showClear
          size="small"
        />
      </div>
      <div
        class="table-filter-prepend-field table-filter-prepend-field--sm"
      >
        <label class="filter-label table-filter-prepend-label"
          >{{ t("purchase.fields.frequency") }}</label
        >
        <Select
          v-model="filter.frecuency"
          :options="frequencyOptions"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :placeholder="t('purchase.placeholders.allFrequencies')"
          showClear
          size="small"
        />
      </div>
    </template>

  </Table>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type Column,
} from "../../../components/tables/types";
import { createTableViewFilterMetadata } from "../../../components/tables/table-view-filter-metadata";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useExpenseStore } from "../store/expense";
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { formatDateForQueryParameter, formatCurrency, getNewUuid } from "../../../utils/functions";
import { Expense } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";
import { useUserFilterStore } from "../../../store/userfilter";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";

const router = useRouter();
const store = useStore();
const expenseStore = useExpenseStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();
const { t, locale } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "expenseTypeId",
    header: t("purchase.fields.type"),
    columnType: ColumnType.Lookup,
    resolver: getExpenseTypeNameById,
    style: "width: 18%",
  },
  {
    field: "description",
    header: t("purchase.fields.description"),
    sortable: true,
    style: "width: 34%",
  },
  {
    field: "paymentDate",
    header: t("purchase.fields.paymentDate"),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 18%",
  },
  {
    field: "frecuency",
    header: t("purchase.fields.frequency"),
    resolver: resolveFrequency,
    style: "width: 15%",
  },
  {
    field: "amount",
    header: t("purchase.fields.amount"),
    columnType: ColumnType.Currency,
    total: "sum",
    totalFormat: formatCurrency,
    style: "width: 10%; text-align: right",
  },
]);

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const filter = ref({
  expenseTypeId: undefined as string | undefined,
  frecuency: undefined as number | null | undefined,
  dates: undefined as Array<Date> | undefined,
});

const frequencyOptions = computed(() => [
  { id: 0, name: t("purchase.frequency.notRecurring") },
  { id: 1, name: t("purchase.frequency.monthly") },
  { id: 2, name: t("purchase.frequency.bimonthly") },
  { id: 3, name: t("purchase.frequency.quarterly") },
  { id: 6, name: t("purchase.frequency.halfYearly") },
  { id: 12, name: t("purchase.frequency.yearly") },
]);

const filterMetadata = computed(() =>
  createTableViewFilterMetadata(columns.value, {
    labels: {
      dates: t("purchase.fields.period"),
      frecuency: t("purchase.fields.frequency"),
    },
    valueResolvers: {
      frecuency: (value) =>
        typeof value === "number" ? getFrequencyName(value, value !== 0) : "",
    },
  }),
);

const setMenuTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.WALLET,
    title: t("purchase.expenses.title"),
  });
};

const filteredExpenses = computed(() => {
  if (!expenseStore.expenses) return [];

  let expenses = expenseStore.expenses;

  if (filter.value.expenseTypeId) {
    expenses = expenses.filter(
      (expense) => expense.expenseTypeId === filter.value.expenseTypeId,
    );
  }

  if (filter.value.frecuency !== undefined && filter.value.frecuency !== null) {
    expenses = expenses.filter((expense) => {
      if (filter.value.frecuency === 0) {
        return !expense.recurring || expense.frecuency === 0;
      }

      return expense.frecuency === filter.value.frecuency;
    });
  }

  return expenses;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("Expenses", "");
  if (userFilter) {
    filter.value.expenseTypeId = userFilter.expenseTypeId;
    filter.value.frecuency = userFilter.frecuency;
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
      ];
    }
  }
};

onMounted(async () => {
  setMenuTitle();

  await expenseStore.fetchExpenseTypes();
  setCurrentYear();
  getUserFilter();
  filterExpense();
});
watch(locale, setMenuTitle);
onUnmounted(() => {
  const savedFilter = {
    expenseTypeId: filter.value.expenseTypeId,
    frecuency: filter.value.frecuency,
    dates: filter.value.dates,
  };

  userFilterStore.addFilter("Expenses", "", savedFilter);
});

const filterExpense = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length > 1 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await expenseStore.fetchExpenses(
      startTime,
      endTime,
      filter.value.expenseTypeId,
    );
  }
};

const setCurrentYear = () => {
  const now = new Date();
  filter.value.dates = [
    new Date(now.getFullYear(), 0, 1),
    new Date(now.getFullYear(), 11, 31),
  ];
};

const clearFilter = async () => {
  filter.value.expenseTypeId = undefined;
  filter.value.frecuency = undefined;
  setCurrentYear();
  await filterExpense();
  userFilterStore.removeFilter("Expenses", "");
};

const createButtonClick = () => {
  router.push({ path: `/expense/${getNewUuid()}` });
};

const editExpense = (row: DataTableRowClickEvent) => {
  router.push({ path: `/expense/${row.data.id}` });
};

const deleteExpense = (expense: Expense) => {
  confirm.require({
    message: t("purchase.messages.confirmDeleteExpense"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await expenseStore.deleteExpense(expense.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.messages.deleted"),
          life: 3000,
        });
        await filterExpense();
      }
    },
  });
};

const getExpenseTypeNameById = (id: string) => {
  const type = expenseStore.expenseTypes?.find((s) => s.id === id);
  if (type) return type.name;
  else return "";
};

const getFrequencyName = (frequency: number, recurring: boolean) => {
  if (!recurring || frequency === 0) {
    return t("purchase.frequency.notRecurring");
  }

  return (
    frequencyOptions.value.find((option) => option.id === frequency)?.name ?? "-"
  );
};

const resolveFrequency = (value: unknown, data: unknown): string => {
  if (typeof value !== "number" || !data || typeof data !== "object") return "";
  const recurring = (data as Expense).recurring;
  return getFrequencyName(value, recurring);
};
</script>
