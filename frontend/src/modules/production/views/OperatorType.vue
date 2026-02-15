<template>
  <FormOperatorType
    v-if="operatorType"
    :operatortype="operatorType"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { OperatorType } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormOperatorType from "../components/FormOperatorType.vue";
import { usePlantModelStore } from "../store/plantmodel";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const { operatorType } = storeToRefs(plantmodelStore);

const loadView = async () => {
  await plantmodelStore.fetchOperatorType(route.params.id as string);
  let pageTitle = "";
  if (!operatorType.value) {
    formMode.value = FormActionMode.CREATE;
    plantmodelStore.setNewOperatorType(route.params.id as string);
    pageTitle = "Alta de tipus d' operari";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Tipus d'operari: ${operatorType.value.name}`;
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
  const data = operatorType.value as OperatorType;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await plantmodelStore.createOperatorType(data);
    message = "Tipus d'operari creat correctament";
  } else {
    result = await plantmodelStore.updateOperatorType(data.id, data);
    message = "Tipus d'operari actualizat correctament";
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
