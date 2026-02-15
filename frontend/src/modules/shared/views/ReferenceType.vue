<template>
  <FormReferenceType
    v-if="referenceType"
    :reference-type="referenceType"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import { ReferenceType } from "../types";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import FormReferenceType from "../components/FormReferenceType.vue";
import { useReferenceTypeStore } from "../store/referenceType";

const formMode = ref(FormActionMode.EDIT);
const router = useRouter();
const route = useRoute();
const store = useStore();
const referenceTypeStore = useReferenceTypeStore();
const { referenceType } = storeToRefs(referenceTypeStore);

const loadView = async () => {
  await referenceTypeStore.fetchReferenceType(route.params.id as string);
  let pageTitle = "";
  if (!referenceType.value) {
    formMode.value = FormActionMode.CREATE;
    referenceTypeStore.setNewReferenceType(route.params.id as string);
    pageTitle = "Alta de tipus de materials";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Tipus de material ${referenceType.value.name}`;
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
  const data = referenceType.value as ReferenceType;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await referenceTypeStore.createReferenceType(data);
    message = "Tipus de material creat correctament";
  } else {
    result = await referenceTypeStore.updateReferenceType(data.id, data);
    message = "Tipus de material actualizat correctament";
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
