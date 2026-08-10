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
import { useI18n } from "vue-i18n";
  
  const formMode = ref(FormActionMode.EDIT);
  const route = useRoute();
  const store = useStore();
  const plantmodelStore = usePlantModelStore();
const { area } = storeToRefs(plantmodelStore);
const { t } = useI18n();
  
  const loadView = async () => {
    await plantmodelStore.fetchArea(route.params.id as string);    
    let pageTitle = "";
    if (!area.value) {
      formMode.value = FormActionMode.CREATE;
      plantmodelStore.setNewArea(route.params.id as string);
      pageTitle = t("production.detail.createArea");
    } else {
      formMode.value = FormActionMode.EDIT;
      pageTitle = t("production.detail.areaTitle", { name: area.value.name });
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
      message = t("production.detail.createdArea");
    } else {
      result = await plantmodelStore.updateArea(data.id, data);
      message = t("production.detail.updatedArea");
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
