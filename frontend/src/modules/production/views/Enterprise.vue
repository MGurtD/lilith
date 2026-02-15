<template>
  <FormEnterprise
    v-if="enterprise"
    :enterprise="enterprise"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { Enterprise } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormEnterprise from "../components/FormEnterprise.vue";
import { usePlantModelStore } from "../store/plantmodel";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const { enterprise } = storeToRefs(plantmodelStore);

const loadView = async () => {
  await plantmodelStore.fetchEnterprise(route.params.id as string);
  let pageTitle = "";
  if (!enterprise.value) {
    formMode.value = FormActionMode.CREATE;
    plantmodelStore.setNewEnterprise(route.params.id as string);
    pageTitle = "Alta d'empresa";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Empresa ${enterprise.value.name}`;
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
  const data = enterprise.value as Enterprise;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await plantmodelStore.createEnterprise(data);
    message = "Empresa creada correctament";
  } else {
    result = await plantmodelStore.updateEnterprise(data.id, data);
    message = "Empresa actualizada correctament";
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
