<template>
  <FormWorkcenterCost
    v-if="workcentercost"
    :workcentercost="workcentercost"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { WorkcenterCost } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormWorkcenterCost from "../components/FormWorkcenterCost.vue";
import { usePlantModelStore } from "../store/plantmodel";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const { workcentercost } = storeToRefs(plantmodelStore);

const loadView = async () => {
  await plantmodelStore.fetchWorkcenterCost(route.params.id as string);
  let pageTitle = "";
  if (!workcentercost.value) {
    formMode.value = FormActionMode.CREATE;
    plantmodelStore.setNewWorkcenterCost(route.params.id as string);
    pageTitle = "Alta de cost per màquina";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Editar cost per màquina`;
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
  const data = workcentercost.value as WorkcenterCost;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await plantmodelStore.createWorkcenterCost(data);
    message = "Cost per màquina creat correctament";
  } else {
    result = await plantmodelStore.updateWorkcenterCost(data.id, data);
    message = "Cost per màquina actualizat correctament";
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
