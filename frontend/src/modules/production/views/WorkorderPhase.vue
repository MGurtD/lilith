<template>
  <header>
    <FormWorkOrderPhase
      v-if="workorder && workorderPhase"
      :workorder="workorder"
      :phase="workorderPhase"
      @submit="onWorkOrderPhaseSubmit"
    ></FormWorkOrderPhase>
  </header>
  <main class="main" v-if="workorderPhase">
    <Tabs value="0">
      <TabList>
        <Tab value="0">Pasos</Tab>
        <Tab value="1">Materials</Tab>
        <Tab value="2">Comentaris</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <TableWorkOrderPhaseDetails
            :workorderPhase="workorderPhase"
            :details="workorderPhase.details!"
            @add="onAddDetail"
            @edit="onEditDetail"
            @delete="onDeleteDetail"
          ></TableWorkOrderPhaseDetails>
        </TabPanel>
        <TabPanel value="1">
          <TableWorkOrderPhaseBillOfMaterials
            :workorderPhase="workorderPhase"
            :billOfMaterials="workorderPhase.billOfMaterials!"
            @add="onAddBomItem"
            @edit="onEditBomItem"
            @delete="onDeleteBomItem"
          ></TableWorkOrderPhaseBillOfMaterials>
        </TabPanel>
        <TabPanel value="2">
          <div class="phase-comment-panel">
            <Card>
              <template #title>
                <div class="comment-header">
                  <i :class="PrimeIcons.COMMENT" class="comment-icon"></i>
                  <span>Comentari de Fase</span>
                </div>
              </template>
              <template #content>
                <div v-if="workorderPhase.comment && workorderPhase.comment.trim()" class="comment-content">
                  <p class="comment-text">{{ workorderPhase.comment }}</p>
                </div>
                <div v-else class="empty-comment">
                  <i :class="PrimeIcons.INFO_CIRCLE" class="empty-icon"></i>
                  <p class="empty-text">Aquesta fase no té cap comentari</p>
                  <small class="empty-subtext">
                    Els comentaris es poden afegir des del mòdul de planta durant la fabricació
                  </small>
                </div>
              </template>
            </Card>
          </div>
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
      <FormWorkOrderPhaseDetail
        v-if="selectedDetail"
        :detail="selectedDetail"
        @submit="onWorkOrderPhaseDetailSubmit"
      ></FormWorkOrderPhaseDetail>
      <FormWorkOrderPhaseBomItem
        v-if="selectedBomItem"
        :bomItem="selectedBomItem"
        @submit="onWorkmasterPhasBomItemSubmit"
      ></FormWorkOrderPhaseBomItem>
    </Dialog>
  </main>
</template>
<script setup lang="ts">
import FormWorkOrderPhase from "../components/FormWorkorderPhase.vue";
import TableWorkOrderPhaseDetails from "../components/TableWorkorderPhaseDetails.vue";
import FormWorkOrderPhaseDetail from "../components/FormWorkorderPhaseDetail.vue";
import TableWorkOrderPhaseBillOfMaterials from "../components/TableWorkorderPhaseBillOfMaterials.vue";
import FormWorkOrderPhaseBomItem from "../components/FormWorkorderPhaseBomItem.vue";

import { onMounted, reactive, ref } from "vue";
import { useRoute } from "vue-router";
import { useStore } from "../../../store";
import { useReferenceStore } from "../../shared/store/reference";
import { usePlantModelStore } from "../store/plantmodel";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import {
  WorkOrderPhase,
  WorkOrderPhaseBillOfMaterials,
  WorkOrderPhaseDetail,
} from "../types";
import { DialogOptions, FormActionMode } from "../../../types/component";
import { useWorkOrderStore } from "../store/workorder";
import { useToast } from "primevue/usetoast";

const route = useRoute();
const toast = useToast();
const store = useStore();
const referenceStore = useReferenceStore();
const plantModelStore = usePlantModelStore();
const workorderStore = useWorkOrderStore();
const { workorder, workorderPhase } = storeToRefs(workorderStore);
const id = ref("");
const phaseId = ref("");

const dialogOptions = reactive({
  visible: false,
  title: "Fase",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  id.value = route.params.id as string;
  phaseId.value = route.params.phaseid as string;
  await loadViewData();

  let pageTitle = "";
  pageTitle = `Ordre de fabricació`;
  if (workorder.value && workorderPhase.value) {
    pageTitle = `${pageTitle} ${workorder.value.code} - Fase ${workorderPhase.value.code}`;
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
  await workorderStore.fetchPhaseById(phaseId.value);
};

const onWorkOrderPhaseSubmit = async (phase: WorkOrderPhase) => {
  const updated = await workorderStore.updatePhase(phaseId.value, phase);

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
const selectedDetail = ref(undefined as WorkOrderPhaseDetail | undefined);
const selectedBomItem = ref(
  undefined as WorkOrderPhaseBillOfMaterials | undefined,
);
const cleanSelecteds = () => {
  selectedDetail.value = undefined;
  selectedBomItem.value = undefined;
};

// Details
const onAddDetail = (detail: WorkOrderPhaseDetail) => {
  formAction.value = FormActionMode.CREATE;
  selectedDetail.value = detail;

  dialogOptions.title = "Afegir pas de fabricació";
  dialogOptions.visible = true;
};
const onEditDetail = (detail: WorkOrderPhaseDetail) => {
  formAction.value = FormActionMode.EDIT;
  selectedDetail.value = detail;

  dialogOptions.title = "Modificar pas de fabricació";
  dialogOptions.visible = true;
};
const onDeleteDetail = async (detail: WorkOrderPhaseDetail) => {
  await workorderStore.updatePhase(
    phaseId.value,
    workorderStore.workorderPhase!,
  );
  await workorderStore.deletePhaseDetail(detail.id);
};

const onWorkOrderPhaseDetailSubmit = async (detail: WorkOrderPhaseDetail) => {
  let promise;
  if (formAction.value === FormActionMode.CREATE) {
    promise = workorderStore.createPhaseDetail(detail);
  } else if (formAction.value === FormActionMode.EDIT) {
    promise = workorderStore.updatePhaseDetail(detail.id, detail);
  }
  await workorderStore.updatePhase(
    phaseId.value,
    workorderStore.workorderPhase!,
  );
  const result = await promise;

  if (result) {
    dialogOptions.visible = false;
  }
};

// Bill of materials
const onAddBomItem = (bomItem: WorkOrderPhaseBillOfMaterials) => {
  formAction.value = FormActionMode.CREATE;
  selectedBomItem.value = bomItem;

  dialogOptions.title = "Afegir material";
  dialogOptions.visible = true;
};
const onEditBomItem = (bomItem: WorkOrderPhaseBillOfMaterials) => {
  formAction.value = FormActionMode.EDIT;
  selectedBomItem.value = bomItem;

  dialogOptions.title = "Modificar material";
  dialogOptions.visible = true;
};
const onDeleteBomItem = async (bomItem: WorkOrderPhaseBillOfMaterials) => {
  await workorderStore.updatePhase(
    phaseId.value,
    workorderStore.workorderPhase!,
  );
  await workorderStore.deletePhaseBomItem(bomItem.id);
};

const onWorkmasterPhasBomItemSubmit = async (
  bomItem: WorkOrderPhaseBillOfMaterials,
) => {
  let promise;
  if (formAction.value === FormActionMode.CREATE) {
    promise = workorderStore.createPhaseBomItem(bomItem);
  } else if (formAction.value === FormActionMode.EDIT) {
    promise = workorderStore.updatePhaseBomItem(bomItem.id, bomItem);
  }
  await workorderStore.updatePhase(
    phaseId.value,
    workorderStore.workorderPhase!,
  );
  const result = await promise;

  if (result) {
    dialogOptions.visible = false;
  }
};
</script>

<style scoped lang="scss">
.phase-comment-panel {
  padding: 1rem;

  .comment-header {
    display: flex;
    align-items: center;
    gap: 0.5rem;

    .comment-icon {
      color: var(--primary-color);
      font-size: 1.2rem;
    }
  }

  .comment-content {
    .comment-text {
      margin: 0;
      line-height: 1.6;
      white-space: pre-wrap;
      word-break: break-word;
      font-size: 1rem;
    }
  }

  .empty-comment {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 1rem;
    text-align: center;
    color: var(--text-color-secondary);

    .empty-icon {
      font-size: 3rem;
      margin-bottom: 1rem;
      opacity: 0.5;
    }

    .empty-text {
      font-size: 1.1rem;
      font-weight: 500;
      margin: 0 0 0.5rem 0;
      color: var(--text-color);
    }

    .empty-subtext {
      font-size: 0.9rem;
      max-width: 400px;
      line-height: 1.4;
    }
  }

  :deep(.p-card) {
    border-left: 4px solid var(--primary-color);
  }

  :deep(.p-card-body) {
    padding: 1.5rem;
  }

  :deep(.p-card-title) {
    font-size: 1.1rem;
    padding-bottom: 0.75rem;
    border-bottom: 1px solid var(--surface-border);
    margin-bottom: 1rem;
  }
}
</style>

