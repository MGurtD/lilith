<template>
  <FormExpense v-if="expense" :expense="expense" @submit="submitForm" />
</template>
<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { useI18n } from "vue-i18n";

import { storeToRefs } from "pinia";
import { Expense } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormExpense from "../components/FormExpense.vue";
import { useExpenseStore } from "../store/expense";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const expenseStore = useExpenseStore();
const { expense } = storeToRefs(expenseStore);
const { t, locale } = useI18n();

const setMenuTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.WALLET,
    backButtonVisible: true,
    title:
      formMode.value === FormActionMode.CREATE
        ? t("purchase.expense.createTitle")
        : t("purchase.expense.editTitle"),
  });
};

const loadView = async () => {
  await expenseStore.fetchExpense(route.params.id as string);
  if (!expense.value) {
    formMode.value = FormActionMode.CREATE;
    expenseStore.setNewExpense(route.params.id as string);
  } else {
    formMode.value = FormActionMode.EDIT;

    expense.value.creationDate = new Date(expense.value.creationDate);
    expense.value.endDate = new Date(expense.value.endDate);
    expense.value.paymentDate = new Date(expense.value.paymentDate);
  }

  setMenuTitle();
};

watch(locale, setMenuTitle);

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = expense.value as Expense;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await expenseStore.createExpense(data);
    message = t("purchase.messages.expenseCreated");
  } else {
    result = await expenseStore.updateExpense(data.id, data);
    message = t("purchase.messages.expenseUpdated");
  }

  if (result) {
    toast.add({
      severity: "success",
      summary: message,
      life: 5000,
    });
    router.back();
  }
};
</script>
