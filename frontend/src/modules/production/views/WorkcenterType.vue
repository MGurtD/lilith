<template>
    <FormWorkcenterType
      v-if="workcenterType"
      :workcentertype="workcenterType"
      @submit="submitForm"
    />
  </template>
  <script setup lang="ts">
  import { onMounted, ref } from "vue";
  import { useRoute } from "vue-router";
  import { PrimeIcons } from "@primevue/core/api";
  
  import { storeToRefs } from "pinia";
  import { WorkcenterType } from "../types";
  import { useStore } from "../../../store";
  
  import { useToast } from "primevue/usetoast";
  import { FormActionMode } from "../../../types/component";
  import router from "../../../router";
  import FormWorkcenterType from "../components/FormWorkcenterType.vue";
  import { usePlantModelStore } from "../store/plantmodel";
  
  const formMode = ref(FormActionMode.EDIT);
  const route = useRoute();
  const store = useStore();
  const plantmodelStore = usePlantModelStore();
  const { workcenterType } = storeToRefs(plantmodelStore);
  
  const loadView = async () => {
    await plantmodelStore.fetchWorkcenterType(route.params.id as string);    
    let pageTitle = "";
    if (!workcenterType.value) {
      formMode.value = FormActionMode.CREATE;
      plantmodelStore.setNewWorkcenterType(route.params.id as string);
      pageTitle = "Alta de tipus de màquina";
    } else {
      formMode.value = FormActionMode.EDIT;
      pageTitle = `Tipus de màquina: ${workcenterType.value.name}`;
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
    const data = workcenterType.value as WorkcenterType;
    let result = false;
    let message = "";
  
    if (formMode.value === FormActionMode.CREATE) {
      result = await plantmodelStore.createWorkcenterType(data);
      message = "Tipus de màquina creat correctament";
    } else {
      result = await plantmodelStore.updateWorkcenterType(data.id, data);
      message = "Tipus de màquina actualizat correctament";
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
  