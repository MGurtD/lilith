<template>
  <FormExercise v-if="exercise" :exercise="exercise" @submit="submitForm" />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { Exercise } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import { useRouter } from "vue-router";
import FormExercise from "../components/FormExercise.vue";

import { useExerciseStore } from "../store/exercise";

const formMode = ref(FormActionMode.EDIT);
const router = useRouter();
const route = useRoute();
const store = useStore();
const exerciseStore = useExerciseStore();
const { exercise } = storeToRefs(exerciseStore);

const loadView = async () => {
  await exerciseStore.fetchOne(route.params.id as string);

  // Comprovar existencia del proveïdor
  let pageTitle = "";
  if (!exercise.value) {
    formMode.value = FormActionMode.CREATE;
    exerciseStore.setNew(route.params.id as string);
    pageTitle = "Alta d'exercici";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Exercici ${exercise.value.name}`;

    exercise.value.startDate = new Date(exercise.value.startDate);
    exercise.value.endDate = new Date(exercise.value.endDate);
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
  const data = exercise.value as Exercise;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await exerciseStore.create(data);
    message = "Exercici creat correctament";
  } else {
    result = await exerciseStore.update(data.id, data);
    message = "Exercici actualizat correctament";
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
