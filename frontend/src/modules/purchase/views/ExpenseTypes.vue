<template>
  <DataTable
    :value="expenseStore.expenseTypes"
    tableStyle="min-width: 100%"
    @row-click="editExpenseType"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ t("purchase.expenseTypes.title") }}</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" :header="t('purchase.fields.name')" style="width: 20%"></Column>
    <Column field="description" :header="t('purchase.fields.description')" style="width: 50%"></Column>
    <Column :header="t('purchase.fields.disabled')" style="width: 20%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteExpenseType($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useExpenseStore } from "../store/expense";
import { onMounted, watch } from "vue";
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
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/expensetype/${row.data.id}` });
  }
};

const confirm = useConfirm();
const toast = useToast();
const deleteExpenseType = (event: any, expenseType: ExpenseType) => {
  confirm.require({
    target: event.currentTarget,
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
