<template>
  <header>
    <FormWorkmasterPhase
      v-if="workmaster && workmasterPhase"
      :workmaster="workmaster"
      :phase="workmasterPhase"
      @submit="onWorkmasterPhaseSubmit"
    ></FormWorkmasterPhase>
  </header>
  <main class="main" v-if="workmasterPhase">
    <Tabs value="0">
      <TabList>
        <Tab value="0">Pasos</Tab>
        <Tab value="1">Materials</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <TableWorkmasterPhaseDetails
            :workmasterPhase="workmasterPhase"
            :details="workmasterPhase.details!"
            @add="onAddDetail"
            @edit="onEditDetail"
            @delete="onDeleteDetail"
          ></TableWorkmasterPhaseDetails>
        </TabPanel>
        <TabPanel value="1">
          <TableWorkmasterPhaseBillOfMaterials
            :workmasterPhase="workmasterPhase"
            :billOfMaterials="workmasterPhase.billOfMaterials!"
            @add="onAddBomItem"
            @edit="onEditBomItem"
            @delete="onDeleteBomItem"
          ></TableWorkmasterPhaseBillOfMaterials>
        </TabPanel>
      </TabPanels>
    </Tabs>
    <Dialog
      v-model:visible="dialogOptions.visible"
      :header="dialogOptions.title"
      :closable="dialogOptions.closable"
      :modal="dialogOptions.modal"
      @hide="cleanSelecteds"
    >
      <FormWorkmasterPhaseDetail
        v-if="selectedDetail"
        :detail="selectedDetail"
        @submit="onWorkmasterPhaseDetailSubmit"
      ></FormWorkmasterPhaseDetail>
      <FormWorkmasterPhaseBomItem
        v-if="selectedBomItem"
        :bomItem="selectedBomItem"
        @submit="onWorkmasterPhasBomItemSubmit"
      ></FormWorkmasterPhaseBomItem>
    </Dialog>
  </main>
</template>
<script setup lang="ts">
import FormWorkmasterPhase from "../components/FormWorkmasterPhase.vue";
import TableWorkmasterPhaseDetails from "../components/TableWorkmasterPhaseDetails.vue";
import FormWorkmasterPhaseDetail from "../components/FormWorkmasterPhaseDetail.vue";
import TableWorkmasterPhaseBillOfMaterials from "../components/TableWorkmasterPhaseBillOfMaterials.vue";
import FormWorkmasterPhaseBomItem from "../components/FormWorkmasterPhaseBomItem.vue";

import { onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useReferenceStore } from "../../shared/store/reference";
import { useWorkMasterStore } from "../store/workmaster";
import { usePlantModelStore } from "../store/plantmodel";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import {
  WorkMasterPhase,
  WorkMasterPhaseBillOfMaterials,
  WorkMasterPhaseDetail,
} from "../types";
import { DialogOptions, FormActionMode } from "../../../types/component";
import { useToast } from "primevue/usetoast";

const route = useRoute();
const toast = useToast();
const store = useStore();
const referenceStore = useReferenceStore();
const plantModelStore = usePlantModelStore();
const workmasterStore = useWorkMasterStore();
const { workmaster, workmasterPhase } = storeToRefs(workmasterStore);
const id = ref("");
const phaseId = ref("");

const dialogOptions = reactive({
  visible: false,
  title: "Linea",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  id.value = route.params.id as string;
  phaseId.value = route.params.phaseid as string;
  await loadViewData();

  let pageTitle = "";
  pageTitle = `Ruta de fabricació`;
  if (workmaster.value && workmasterPhase.value) {
    pageTitle = `${pageTitle} ${referenceStore.getFullName(
      workmaster.value.reference!,
    )} - Fase ${workmasterPhase.value.code}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });

  referenceStore.fetchReferencesByModule("purchase");
  plantModelStore.fetchActiveModel();
});

const loadViewData = async () => {
  await workmasterStore.fetchPhaseById(phaseId.value);
};

const onWorkmasterPhaseSubmit = async (phase: WorkMasterPhase) => {
  const updated = await workmasterStore.updatePhase(phaseId.value, phase);
  if (updated) {
    toast.add({
      severity: "success",
      summary: "Fase actualitzada",
      detail: `La fase ${phase.code} ha estat actualitzada correctament`,
      life: 10000,
    });
  }
};
const formAction = ref(FormActionMode.CREATE);
const selectedDetail = ref(undefined as WorkMasterPhaseDetail | undefined);
const selectedBomItem = ref(
  undefined as WorkMasterPhaseBillOfMaterials | undefined,
);
const cleanSelecteds = () => {
  selectedDetail.value = undefined;
  selectedBomItem.value = undefined;
};

// Details
const onAddDetail = (detail: WorkMasterPhaseDetail) => {
  formAction.value = FormActionMode.CREATE;
  selectedDetail.value = detail;

  dialogOptions.title = "Afegir pas de fabricació";
  dialogOptions.visible = true;
};
const onEditDetail = (detail: WorkMasterPhaseDetail) => {
  formAction.value = FormActionMode.EDIT;
  selectedDetail.value = detail;

  dialogOptions.title = "Modificar pas de fabricació";
  dialogOptions.visible = true;
};
const onDeleteDetail = async (detail: WorkMasterPhaseDetail) => {
  await workmasterStore.updatePhase(
    workmasterStore.workmasterPhase!.id,
    workmasterStore.workmasterPhase!,
  );
  await workmasterStore.deletePhaseDetail(detail.id);
};

const onWorkmasterPhaseDetailSubmit = async (detail: WorkMasterPhaseDetail) => {
  let promise;
  if (formAction.value === FormActionMode.CREATE) {
    promise = workmasterStore.createPhaseDetail(detail);
  } else if (formAction.value === FormActionMode.EDIT) {
    promise = workmasterStore.updatePhaseDetail(detail.id, detail);
  }
  await workmasterStore.updatePhase(
    workmasterStore.workmasterPhase!.id,
    workmasterStore.workmasterPhase!,
  );
  const result = await promise;

  if (result) {
    dialogOptions.visible = false;
  }
};

// Bill of materials
const onAddBomItem = (bomItem: WorkMasterPhaseBillOfMaterials) => {
  formAction.value = FormActionMode.CREATE;
  selectedBomItem.value = bomItem;

  dialogOptions.title = "Afegir material";
  dialogOptions.visible = true;
};
const onEditBomItem = (bomItem: WorkMasterPhaseBillOfMaterials) => {
  formAction.value = FormActionMode.EDIT;
  selectedBomItem.value = bomItem;

  dialogOptions.title = "Modificar material";
  dialogOptions.visible = true;
};
const onDeleteBomItem = async (bomItem: WorkMasterPhaseBillOfMaterials) => {
  await workmasterStore.updatePhase(
    workmasterStore.workmasterPhase!.id,
    workmasterStore.workmasterPhase!,
  );
  await workmasterStore.deletePhaseBomItem(bomItem.id);
};

const onWorkmasterPhasBomItemSubmit = async (
  bomItem: WorkMasterPhaseBillOfMaterials,
) => {
  let promise;
  if (formAction.value === FormActionMode.CREATE) {
    promise = workmasterStore.createPhaseBomItem(bomItem);
  } else if (formAction.value === FormActionMode.EDIT) {
    promise = workmasterStore.updatePhaseBomItem(bomItem.id, bomItem);
  }
  await workmasterStore.updatePhase(
    workmasterStore.workmasterPhase!.id,
    workmasterStore.workmasterPhase!,
  );
  const result = await promise;

  if (result) {
    dialogOptions.visible = false;
  }
};
</script>
