<template>
  <FormOperator v-if="operator" 
  :operator="operator" @submit="submitForm" />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { Operator } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormOperator from "../components/FormOperator.vue";
import { usePlantModelStore } from "../store/plantmodel";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const { operator } = storeToRefs(plantmodelStore);

const loadView = async () => {
  await plantmodelStore.fetchOperator(route.params.id as string);
  let pageTitle = "";
  if (!operator.value) {
    formMode.value = FormActionMode.CREATE;
    plantmodelStore.setNewOperator(route.params.id as string);
    pageTitle = "Alta d'operari";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Operari: ${operator.value.name}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.USER,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = operator.value as Operator;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await plantmodelStore.createOperator(data);
    message = "Operari creat correctament";
  } else {
    result = await plantmodelStore.updateOperator(data.id, data);
    message = "Operari actualizat correctament";
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
