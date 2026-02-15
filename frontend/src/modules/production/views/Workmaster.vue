<template>
  <header>
    <FormWorkmaster
      v-if="workmaster"
      :workmaster="workmaster"
      @calculateCost="calculateCostSubmit"
      @submit="onWorkmasterSubmit"
    ></FormWorkmaster>
  </header>
  <main class="main">
    <TableWorkmasterPhases
      v-if="workmaster && workmaster.phases"
      :workmaster="workmaster"
      :workmasterPhases="workmaster.phases"
      @add="addWorkMasterPhase"
      @edit="editWorkMasterPhase"
      @delete="deleteWorkMasterPhase"
    ></TableWorkmasterPhases>
  </main>
</template>
<script setup lang="ts">
import FormWorkmaster from "../components/FormWorkmaster.vue";
import TableWorkmasterPhases from "../components/TableWorkmasterPhases.vue";

import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useReferenceStore } from "../../shared/store/reference";
import { useWorkMasterStore } from "../store/workmaster";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { WorkMaster, WorkMasterPhase } from "../types";
import { usePlantModelStore } from "../store/plantmodel";
import { useToast } from "primevue/usetoast";
import { formatCurrency } from "../../../utils/functions";

const route = useRoute();
const router = useRouter();
const store = useStore();
const toast = useToast();
const referenceStore = useReferenceStore();
const workmasterStore = useWorkMasterStore();
const plantModelStore = usePlantModelStore();
const { workmaster } = storeToRefs(workmasterStore);
const id = ref("");

onMounted(async () => {
  id.value = route.params.id as string;
  await loadViewData();

  let pageTitle = "";
  pageTitle = `Ruta de fabricació`;
  if (workmaster.value) {
    pageTitle = `${pageTitle} ${referenceStore.getFullName(
      workmaster.value.reference!,
    )}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });
});

const loadViewData = async () => {
  await referenceStore.fetchReferencesByModule("sales");
  await workmasterStore.fetchOne(id.value);
  plantModelStore.fetchActiveModel();
};

const onWorkmasterSubmit = async (workmaster: WorkMaster) => {
  const updated = await workmasterStore.update(id.value, workmaster);
  if (updated) await loadViewData();
};

const calculateCostSubmit = async (workmaster: WorkMaster) => {
  const updated = await workmasterStore.update(id.value, workmaster);

  const response = await workmasterStore.calculate(workmaster.id);
  if (updated && response.result) {
    toast.add({
      severity: "success",
      summary: "Cálcul de cost",
      detail: `${formatCurrency(response.content!)}`,
      life: 10000,
    });

    await workmasterStore.fetchOne(id.value);
  } else {
    toast.add({
      severity: "warn",
      summary: "Cálcul de cost",
      detail:
        response.errors.length > 0
          ? response.errors[0]
          : "Errors detectats durant el cálcul",
      life: 10000,
    });
  }
};

// Phases
const addWorkMasterPhase = async (phase: WorkMasterPhase) => {
  const created = await workmasterStore.createPhase(phase);
  if (created)
    router.push({ path: `/workmaster/${id.value}/phase/${phase.id}` });
};
const editWorkMasterPhase = (phase: WorkMasterPhase) => {
  router.push({ path: `/workmaster/${id.value}/phase/${phase.id}` });
};
const deleteWorkMasterPhase = async (phase: WorkMasterPhase) => {
  await workmasterStore.deletePhase(phase.id);
};
</script>
<style scoped>
.main {
  margin-top: 1rem;
}
</style>
