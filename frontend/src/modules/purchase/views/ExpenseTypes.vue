<template>
  <Table
    preset="crud-list"
    :columns="columns"
    :items="expenseStore.expenseTypes ?? []"
    :filter-config="[]"
    :show-filter-actions="false"
    delete-column-width="5%"
    show-delete-column
    tableStyle="min-width: 100%"
    @row-click="editExpenseType"
    @create="createButtonClick"
    @delete="deleteExpenseType"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ t("purchase.expenseTypes.title") }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type Column,
} from "../../../components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useExpenseStore } from "../store/expense";
import { computed, onMounted, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";
import { ExpenseType } from "../types";

const router = useRouter();
const store = useStore();
const expenseStore = useExpenseStore();
const { t, locale } = useI18n();

const columns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("purchase.fields.name"),
    style: "width: 20%",
  },
  {
    field: "description",
    header: t("purchase.fields.description"),
    style: "width: 50%",
  },
  {
    field: "disabled",
    header: t("purchase.fields.disabled"),
    columnType: ColumnType.Boolean,
    showColor: false,
    style: "width: 20%",
  },
]);

const setMenuTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.FLAG,
    title: t("purchase.expenseTypes.managementTitle"),
  });
};

onMounted(async () => {
  await expenseStore.fetchExpenseTypes();

  setMenuTitle();
});

watch(locale, setMenuTitle);

const createButtonClick = () => {
  router.push({ path: `/expensetype/${getNewUuid()}` });
};

const editExpenseType = (row: DataTableRowClickEvent) => {
  router.push({ path: `/expensetype/${row.data.id}` });
};

const confirm = useConfirm();
const toast = useToast();
const deleteExpenseType = (expenseType: ExpenseType) => {
  confirm.require({
    message: t("purchase.messages.confirmDeleteExpenseType"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await expenseStore.deleteExpenseType(expenseType.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.messages.deleted"),
          life: 3000,
        });
        await expenseStore.fetchExpenseTypes();
      }
    },
  });
};
</script>
