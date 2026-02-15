<template>
  <FormSupplierType @submit="submitForm" />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useSuppliersStore } from "../store/suppliers";
import { PrimeIcons, ToastSeverity } from "@primevue/core/api";

import FormSupplierType from "../components/FormSupplierType.vue";
import { storeToRefs } from "pinia";
import { SupplierType } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";

const formMode = ref(FormActionMode.EDIT);
const router = useRouter();
const route = useRoute();
const store = useStore();
const supplierStore = useSuppliersStore();
const { supplierType } = storeToRefs(supplierStore);

const loadView = async () => {
  await supplierStore.fetchSupplierType(route.params.id as string);

  // Comprovar existencia del proveïdor
  let pageTitle = "";
  if (!supplierType.value) {
    formMode.value = FormActionMode.CREATE;
    supplierStore.setNewSupplierType(route.params.id as string);
    pageTitle = "Alta de tipus de proveïdor";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Tipus de proveïdor ${supplierType.value.name}`;
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
  const data = supplierType.value as SupplierType;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await supplierStore.createSupplierType(data);
    message = "Tipus de proveïdor creat correctament";
  } else {
    result = await supplierStore.updateSupplierType(data.id, data);
    message = "Tipus de proveïdor actualizat correctament";
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
