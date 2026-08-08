<template>
  <FormExpenseType
    v-if="expenseType"
    :expenseType="expenseType"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { useI18n } from "vue-i18n";

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
const { t, locale } = useI18n();

const setMenuTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.FLAG,
    backButtonVisible: true,
    title:
      formMode.value === FormActionMode.CREATE
        ? t("purchase.expenseTypes.createTitle")
        : t("purchase.expenseTypes.detailTitle", {
            name: expenseType.value?.name ?? "",
          }),
  });
};

const loadView = async () => {
  await expenseStore.fetchExpenseType(route.params.id as string);
  if (!expenseType.value) {
    formMode.value = FormActionMode.CREATE;
    expenseStore.setNewExpenseType(route.params.id as string);
  } else {
    formMode.value = FormActionMode.EDIT;
  }

  setMenuTitle();
};

watch(locale, setMenuTitle);

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
    message = t("purchase.messages.expenseTypeCreated");
  } else {
    result = await expenseStore.updateExpenseType(data.id, data);
    message = t("purchase.messages.expenseTypeUpdated");
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
