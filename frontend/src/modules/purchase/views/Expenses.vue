<template>
  <DataTable
    :value="expenseStore.expenses"
    class="p-datatable-sm small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sortMode="multiple"
    @row-click="editExpense"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter">
          <div class="filter-field">
            <ExerciseDatePicker
              :exercises="exerciseStore.exercises"
              @range-selected="filterExpense"
            />
          </div>
          <div class="filter-field">
            <label class="block text-900 mb-2">Tipus</label>
            <Select
              v-model="filter.expenseTypeId"
              :options="expenseStore.expenseTypes"
              optionValue="id"
              optionLabel="name"
              class="w-full"
            />
          </div>
        </div>

        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER"
            rounded
            raised
            @click="filterExpense"
          />
          <Button
            class="datatable-button mr-2"
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
      ><div class="flex-right">
        Total {{ formatCurrency(totalAmount) }}
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
import ExerciseDatePicker from "../../../components/ExerciseDatePicker.vue";
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
});

const totalAmount = computed(() => {
  let total = 0;
  if (expenseStore.expenses) {
    expenseStore.expenses.forEach((e) => (total += e.amount));
  }
  return total;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("Expenses", "");
  if (userFilter) {
    filter.value.expenseTypeId = userFilter.expenseTypeId;
    if (userFilter.exercisePicker) {
      store.exercisePicker.exercise = userFilter.exercisePicker.exercise;
      store.exercisePicker.dates = [
        new Date(userFilter.exercisePicker.dates[0]),
        new Date(userFilter.exercisePicker.dates[1]),
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
    exercisePicker: store.exercisePicker,
  };

  userFilterStore.addFilter("Expenses", "", savedFilter);
});

const filterExpense = async () => {
  if (store.exercisePicker.dates) {
    const startTime = formatDateForQueryParameter(
      store.exercisePicker.dates[0],
    );
    const endTime = formatDateForQueryParameter(store.exercisePicker.dates[1]);

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
    store.exercisePicker.exercise = currentExercise;
    store.exercisePicker.dates = [
      new Date(store.exercisePicker.exercise.startDate),
      new Date(store.exercisePicker.exercise.endDate),
    ];
  }
};

const clearFilter = async () => {
  filter.value.expenseTypeId = "";
  store.cleanExercisePicker();
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
</script>
