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
  import { useI18n } from "vue-i18n";
  
  import { storeToRefs } from "pinia";
  import { WorkcenterType } from "../types";
  import { useStore } from "../../../store";
  
  import { useToast } from "primevue/usetoast";
  import { FormActionMode } from "../../../types/component";
  import router from "../../../router";
  import FormWorkcenterType from "../components/FormWorkcenterType.vue";
  import { usePlantModelStore } from "../store/plantmodel";
  const { t } = useI18n();
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
      pageTitle = t("production.detail.createWorkcenterType");
    } else {
      formMode.value = FormActionMode.EDIT;
      pageTitle = t("production.detail.workcenterTypeTitle", {
        name: workcenterType.value.name,
      });
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
      message = t("production.detail.createdWorkcenterType");
    } else {
      result = await plantmodelStore.updateWorkcenterType(data.id, data);
      message = t("production.detail.updatedWorkcenterType");
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
