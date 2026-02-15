<template>
    <FormSite 
        v-if="site"
        :site="site"
        @submit="submitForm"
    />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { Site } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormSite from "../components/FormSite.vue";
import { usePlantModelStore } from "../store/plantmodel";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const siteStore = usePlantModelStore();
const { site } = storeToRefs(siteStore);

const loadView = async () => {
    await siteStore.fetchSite(route.params.id as string);
    let pageTitle ="";
    if(!site.value){
        formMode.value = FormActionMode.CREATE;
        siteStore.setNewSite(route.params.id as string);
        pageTitle = "Alta de locals";
    } else {
        formMode.value = FormActionMode.EDIT;
        pageTitle = "Modificació de locals";

    }
    store.setMenuItem({
        icon: PrimeIcons.WALLET,
        backButtonVisible: true,
        title:pageTitle,
    });    
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = site.value as Site;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await siteStore.createSite(data);
    message = "Local registrat correctament";
  } else {
    result = await siteStore.updateSite(data.id, data);
    message = "Local actualitzat correctament";
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