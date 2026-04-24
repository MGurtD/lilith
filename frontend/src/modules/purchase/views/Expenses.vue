<template>
  <DataTable
    :value="filteredExpenses"
    class="p-datatable-sm small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    paginator
    :rows="20"
    @row-click="editExpense"
  >
    <template #header>
      <div class="filter-toolbar">
        <div class="filter-toolbar__field filter-toolbar__field--exercise">
          <label class="filter-label">Periode</label>
          <DatePicker
            v-model="filter.dates"
            selectionMode="range"
            dateFormat="dd/mm/yy"
            :showIcon="true"
            class="w-full"
            placeholder="Selecciona periode"
            @update:model-value="filterExpense"
          />
        </div>
        <div class="filter-toolbar__field">
          <label class="filter-label">Tipus</label>
          <Select
            v-model="filter.expenseTypeId"
            :options="expenseStore.expenseTypes"
            optionValue="id"
            optionLabel="name"
            class="w-full"
            placeholder="Tots els tipus"
            showClear
          />
        </div>
        <div class="filter-toolbar__field">
          <label class="filter-label">Freqüència</label>
          <Select
            v-model="filter.frecuency"
            :options="frequencyOptions"
            optionValue="id"
            optionLabel="name"
            class="w-full"
            placeholder="Totes"
            showClear
          />
        </div>
        <div class="filter-toolbar__actions">
          <Button
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="clearFilter"
          />
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
    </template>
    <Column header="Tipus" style="width: 15%">
      <template #body="slotProps">
        {{ getExpenseTypeNameById(slotProps.data.expenseTypeId) }}
      </template>
    </Column>
    <Column
      field="description"
      header="Descripció"
      style="width: 40%"
      sortable
    ></Column>
    <Column
      field="paymentDate"
      header="Data pagament"
      style="width: 20%"
      sortable
    >
      <template #body="slotProps">
        {{ formatDate(slotProps.data.paymentDate) }}
      </template>
    </Column>
    <Column field="amount" header="Import" style="width: 15%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.amount) }}
      </template>
    </Column>
    <Column header="Freqüència" style="width: 12%">
      <template #body="slotProps">
        {{ getFrequencyName(slotProps.data.frecuency, slotProps.data.recurring) }}
      </template>
    </Column>
    <Column header="Recurrent" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.recurring" :showColor="false" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteExpense($event, slotProps.data)"
        />
      </template>
    </Column>
    <template #footer
      ><div class="expenses-footer-total">
        Total visible {{ formatCurrency(totalAmount) }}
      </div></template
    >
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useExpenseStore } from "../store/expense";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  formatDate,
  formatDateForQueryParameter,
  formatCurrency,
} from "../../../utils/functions";
import { Expense } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useExerciseStore } from "../../shared/store/exercise";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const store = useStore();
const exerciseStore = useExerciseStore();
const expenseStore = useExpenseStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();

const filter = ref({
  expenseTypeId: undefined as string | undefined,
  frecuency: undefined as number | undefined,
  dates: undefined as Array<Date> | undefined,
});

const frequencyOptions = [
  { id: 0, name: "No recurrent" },
  { id: 1, name: "Mensual" },
  { id: 2, name: "Bimensual" },
  { id: 3, name: "Trimestral" },
  { id: 6, name: "Semestral" },
  { id: 12, name: "Anual" },
];

const filteredExpenses = computed(() => {
  if (!expenseStore.expenses) return [];

  let expenses = expenseStore.expenses;

  if (filter.value.expenseTypeId) {
    expenses = expenses.filter(
      (expense) => expense.expenseTypeId === filter.value.expenseTypeId,
    );
  }

  if (filter.value.frecuency !== undefined) {
    expenses = expenses.filter((expense) => {
      if (filter.value.frecuency === 0) {
        return !expense.recurring || expense.frecuency === 0;
      }

      return expense.frecuency === filter.value.frecuency;
    });
  }

  return expenses;
});

const totalAmount = computed(() => {
  let total = 0;
  filteredExpenses.value.forEach((expense) => (total += expense.amount));
  return total;
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
  store.setMenuItem({
    icon: PrimeIcons.WALLET,
    title: "Gestió de despeses",
  });

  await expenseStore.fetchExpenseTypes();
  if (!exerciseStore.exercises?.length) {
    await exerciseStore.fetchActive();
  }
  setCurrentYear();
  getUserFilter();
  filterExpense();
});
onUnmounted(() => {
  const savedFilter = {
    expenseTypeId: filter.value.expenseTypeId,
    frecuency: filter.value.frecuency,
    dates: filter.value.dates,
  };

  userFilterStore.addFilter("Expenses", "", savedFilter);
});

const filterExpense = async () => {
  if (filter.value.dates && filter.value.dates.length > 1 && filter.value.dates[1]) {
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
  const year = new Date().getFullYear().toString();
  const currentExercise = exerciseStore.exercises?.find((e) => e.name === year);

  if (currentExercise) {
    filter.value.dates = [
      new Date(currentExercise.startDate),
      new Date(currentExercise.endDate),
    ];
  }
};

const clearFilter = async () => {
  filter.value.expenseTypeId = undefined;
  filter.value.frecuency = undefined;
  setCurrentYear();
  await filterExpense();
  userFilterStore.removeFilter("Expenses", "");
};

const createButtonClick = () => {
  router.push({ path: `/expense/${uuidv4()}` });
};

const editExpense = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/expense/${row.data.id}` });
  }
};

const deleteExpense = (event: any, expense: Expense) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar la despesa?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await expenseStore.deleteExpense(expense.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
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
    return "No recurrent";
  }

  return frequencyOptions.find((option) => option.id === frequency)?.name ?? "-";
};
</script>

<style scoped>
.filter-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: end;
}

.filter-toolbar__field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  flex: 0 0 auto;
}

.filter-toolbar__field--exercise {
  width: 22rem;
}

.filter-toolbar__field:not(.filter-toolbar__field--exercise) {
  width: 16rem;
}

.filter-toolbar__exercise-picker {
  width: 100%;
}

.filter-toolbar__actions {
  display: flex;
  gap: 0.5rem;
  align-self: end;
}

.expenses-footer-total {
  display: flex;
  justify-content: flex-end;
  font-weight: 600;
}

:deep(.filter-toolbar__exercise-picker > *) {
  width: 100%;
}

@media (max-width: 1200px) {
  .filter-toolbar {
    align-items: stretch;
  }

  .filter-toolbar__actions {
    min-width: 0;
  }
}

@media (max-width: 768px) {
  .filter-toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-toolbar__field,
  .filter-toolbar__field--exercise,
  .filter-toolbar__field:not(.filter-toolbar__field--exercise) {
    width: 100%;
  }

  .filter-toolbar__actions {
    justify-content: flex-end;
  }
}
</style>
