<template>
  <FormExpenseType
    v-if="expenseType"
    :expenseType="expenseType"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { ExpenseType } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import { useExpenseStore } from "../store/expense";
import FormExpenseType from "../components/FormExpenseType.vue";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const expenseStore = useExpenseStore();
const { expenseType } = storeToRefs(expenseStore);

const loadView = async () => {
  await expenseStore.fetchExpenseType(route.params.id as string);
  let pageTitle = "";
  if (!expenseType.value) {
    formMode.value = FormActionMode.CREATE;
    expenseStore.setNewExpenseType(route.params.id as string);
    pageTitle = "Alta de tipus de despesa";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Tipus de despesa ${expenseType.value.name}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.FLAG,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = expenseType.value as ExpenseType;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await expenseStore.createExpenseType(data);
    message = "Tipus de despesa creada correctament";
  } else {
    result = await expenseStore.updateExpenseType(data.id, data);
    message = "Tipus de despesa actualitzada correctament";
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
