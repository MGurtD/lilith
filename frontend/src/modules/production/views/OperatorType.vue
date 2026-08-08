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
import { useI18n } from "vue-i18n";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const { operatorType } = storeToRefs(plantmodelStore);
const { t } = useI18n();

const loadView = async () => {
  await plantmodelStore.fetchOperatorType(route.params.id as string);
  let pageTitle = "";
  if (!operatorType.value) {
    formMode.value = FormActionMode.CREATE;
    plantmodelStore.setNewOperatorType(route.params.id as string);
    pageTitle = t("production.detail.createOperatorType");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = t("production.detail.operatorTypeTitle", { name: operatorType.value.name });
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
    message = t("production.detail.createdOperatorType");
  } else {
    result = await plantmodelStore.updateOperatorType(data.id, data);
    message = t("production.detail.updatedOperatorType");
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
