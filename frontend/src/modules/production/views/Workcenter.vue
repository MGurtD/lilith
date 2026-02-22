<template>
  <FormWorkcenter
    v-if="workcenter"
    :workcenter="workcenter"
    @submit="submitForm"
  />
  <section class="mt-4">
    <!-- Afegir tab -->
    <Tabs value="0">
      <TabList>
        <Tab value="0">Imatge</Tab>
        <Tab value="1">Percentatges</Tab>
        <Tab value="2">Ubicacions</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <FileEntityPicker
            title="Imatge de la màquina"
            entity="WorkcenterPicture"
            :max-files="1"
            :id="route.params.id as string"
          />
        </TabPanel>
        <TabPanel value="1">
          <TableWorkcenterProfitPercentage
            v-if="workcenter"
            :workcenterProfitPercentages="workcenterProfitPercentages"
            :workcenterId="workcenter.id"
            @delete="deleteWorkcenterProfitPercentage"
            @add="addWorkcenterProfitPercentage"
          />
        </TabPanel>
        <TabPanel value="2">
          <TableWorkcenterLocations
            v-if="workcenter"
            :workcenterLocations="workcenterLocations"
            :workcenterId="workcenter.id"
            @delete="deleteWorkcenterLocation"
            @add="addWorkcenterLocation"
          />
        </TabPanel>
      </TabPanels>
    </Tabs>
  </section>
</template>
<script setup lang="ts">
import FileEntityPicker from "../../../components/FileEntityPicker.vue";
import FormWorkcenter from "../components/FormWorkcenter.vue";
import TableWorkcenterProfitPercentage from "../components/TableWorkcenterProfitPercentage.vue";
import TableWorkcenterLocations from "../components/TableWorkcenterLocations.vue";
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { Workcenter, WorkcenterProfitPercentage, WorkcenterLocation } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import { usePlantModelStore } from "../store/plantmodel";
import { getNewUuid } from "../../../utils/functions";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const { workcenter } = storeToRefs(plantmodelStore);
const { workcenterProfitPercentages } = storeToRefs(plantmodelStore);
const { workcenterLocations } = storeToRefs(plantmodelStore);

const loadView = async () => {
  await plantmodelStore.fetchWorkcenter(route.params.id as string);
  let pageTitle = "";
  if (!workcenter.value) {
    formMode.value = FormActionMode.CREATE;
    plantmodelStore.setNewWorkcenter(route.params.id as string);
    pageTitle = "Alta de màquina";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Màquina: ${workcenter.value.name}`;
  }
  await plantmodelStore.fetchWorkcenterProfitPercentagesByWorkcenterId(
    route.params.id as string,
  );
  workcenterProfitPercentages.value =
    plantmodelStore.workcenterProfitPercentages;

  await plantmodelStore.fetchWorkcenterLocationsByWorkcenterId(
    route.params.id as string,
  );

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
  const data = workcenter.value as Workcenter;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await plantmodelStore.createWorkcenter(data);
    message = "Màquina creada correctament";
  } else {
    result = await plantmodelStore.updateWorkcenter(data.id, data);
    message = "Màquina actualizada correctament";
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

const deleteWorkcenterProfitPercentage = async (
  workcenterProfitPercentage: WorkcenterProfitPercentage,
) => {
  await plantmodelStore.deleteWorkcenterProfitPercentage(
    workcenterProfitPercentage.id,
  );
  await loadView();
};

const addWorkcenterProfitPercentage = async (
  workcenterProfitPercentage: WorkcenterProfitPercentage,
) => {
  const result = await plantmodelStore.createWorkcenterProfitPercentage(
    workcenterProfitPercentage,
  );
  if (result) {
    toast.add({
      severity: "success",
      summary: "Percentatge creat",
      detail: "El percentatge de profit s'ha creat correctament",
      life: 5000,
    });
    await loadView();
  }
};

const deleteWorkcenterLocation = async (entity: WorkcenterLocation) => {
  await plantmodelStore.deleteWorkcenterLocation(
    entity.id,
    route.params.id as string,
  );
  await loadView();
};

const addWorkcenterLocation = async (locationId: string) => {
  const entity: WorkcenterLocation = {
    id: getNewUuid(),
    workcenterId: route.params.id as string,
    locationId,
  };
  const result = await plantmodelStore.createWorkcenterLocation(entity);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Ubicació afegida",
      detail: "La ubicació s'ha assignat correctament a la màquina.",
      life: 5000,
    });
    await loadView();
  }
};
</script>
