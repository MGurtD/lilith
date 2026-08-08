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
import { useI18n } from "vue-i18n";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const siteStore = usePlantModelStore();
const { site } = storeToRefs(siteStore);
const { t } = useI18n();

const loadView = async () => {
    await siteStore.fetchSite(route.params.id as string);
    let pageTitle ="";
    if(!site.value){
        formMode.value = FormActionMode.CREATE;
        siteStore.setNewSite(route.params.id as string);
        pageTitle = t("production.detail.createSite");
    } else {
        formMode.value = FormActionMode.EDIT;
        pageTitle = t("production.detail.editSite");

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
    message = t("production.detail.createdSite");
  } else {
    result = await siteStore.updateSite(data.id, data);
    message = t("production.detail.updatedSite");
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
