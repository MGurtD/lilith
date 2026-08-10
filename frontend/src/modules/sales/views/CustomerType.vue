<template>
  <FormCustomerType
    v-if="customerStore.customerType"
    :customerType="customerStore.customerType"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useCustomersStore } from "../store/customers";
import { PrimeIcons } from "@primevue/core/api";

import FormCustomerType from "../components/FormCustomerType.vue";
import { storeToRefs } from "pinia";
import { CustomerType } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import { useI18n } from "vue-i18n";

const formMode = ref(FormActionMode.EDIT);
const router = useRouter();
const route = useRoute();
const store = useStore();
const customerStore = useCustomersStore();
const { customerType } = storeToRefs(customerStore);
const { t } = useI18n();

const loadView = async () => {
  await customerStore.fetchCustomerType(route.params.id as string);

  // Comprovar existencia del client
  let pageTitle = "";
  if (!customerType.value) {
    formMode.value = FormActionMode.CREATE;
    customerStore.setNewCustomerType(route.params.id as string);
    pageTitle = t("sales.customers.createCustomerType");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `${t("sales.customers.customerTypes")} ${customerType.value.name}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = customerType.value as CustomerType;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await customerStore.createCustomerType(data);
    message = t("sales.customers.customerTypeCreated");
  } else {
    result = await customerStore.updateCustomerType(data.id, data);
    message = t("sales.customers.customerTypeUpdated");
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
