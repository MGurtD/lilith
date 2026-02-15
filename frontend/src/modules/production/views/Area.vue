<template>
    <FormArea
      v-if="area"
      :area="area"
      @submit="submitForm"
    />
  </template>
  <script setup lang="ts">
  import { onMounted, ref } from "vue";
  import { useRoute } from "vue-router";
  import { PrimeIcons } from "@primevue/core/api";
  
  import { storeToRefs } from "pinia";
  import { Area } from "../types";
  import { useStore } from "../../../store";
  
  import { useToast } from "primevue/usetoast";
  import { FormActionMode } from "../../../types/component";
  import router from "../../../router";
  import FormArea from "../components/FormArea.vue";
  import { usePlantModelStore } from "../store/plantmodel";
  
  const formMode = ref(FormActionMode.EDIT);
  const route = useRoute();
  const store = useStore();
  const plantmodelStore = usePlantModelStore();
  const { area } = storeToRefs(plantmodelStore);
  
  const loadView = async () => {
    await plantmodelStore.fetchArea(route.params.id as string);    
    let pageTitle = "";
    if (!area.value) {
      formMode.value = FormActionMode.CREATE;
      plantmodelStore.setNewArea(route.params.id as string);
      pageTitle = "Alta d'area";
    } else {
      formMode.value = FormActionMode.EDIT;
      pageTitle = `Area ${area.value.name}`;
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
    const data = area.value as Area;
    let result = false;
    let message = "";
  
    if (formMode.value === FormActionMode.CREATE) {
      result = await plantmodelStore.createArea(data);
      message = "Area creada correctament";
    } else {
      result = await plantmodelStore.updateArea(data.id, data);
      message = "Area actualizada correctament";
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
  