<template>
  <header>
    <FormPhaseTemplate
      v-if="phaseTemplate"
      :phaseTemplate="phaseTemplate"
      @submit="onPhaseTemplateSubmit"
    ></FormPhaseTemplate>
  </header>
  <main class="main">
    <TablePhaseTemplateDetails
      v-if="phaseTemplate && phaseTemplate.details"
      :phaseTemplate="phaseTemplate"
      :details="phaseTemplate.details"
      @add="addDetail"
      @edit="editDetail"
      @delete="deleteDetail"
    ></TablePhaseTemplateDetails>
  </main>

  <Dialog
    v-model:visible="detailDialogOptions.visible"
    :header="detailDialogOptions.title"
    :closable="detailDialogOptions.closable"
    :modal="detailDialogOptions.modal"
  >
    <FormPhaseTemplateDetail
      v-if="selectedDetail"
      :detail="selectedDetail"
      @submit="onDetailSubmit"
    ></FormPhaseTemplateDetail>
  </Dialog>
</template>

<script setup lang="ts">
import FormPhaseTemplate from "../components/FormPhaseTemplate.vue";
import TablePhaseTemplateDetails from "../components/TablePhaseTemplateDetails.vue";
import FormPhaseTemplateDetail from "../components/FormPhaseTemplateDetail.vue";

import { onMounted, ref, reactive } from "vue";
import { useRoute } from "vue-router";
import { useStore } from "../../../store";
import { usePhaseTemplateStore } from "../store/phasetemplate";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { PhaseTemplate, PhaseTemplateDetail } from "../types";
import { usePlantModelStore } from "../store/plantmodel";
import { useToast } from "primevue/usetoast";
import { DialogOptions, FormActionMode } from "../../../types/component";

const route = useRoute();
const store = useStore();
const toast = useToast();
const phaseTemplateStore = usePhaseTemplateStore();
const plantModelStore = usePlantModelStore();
const { phaseTemplate } = storeToRefs(phaseTemplateStore);
const id = ref("");

const selectedDetail = ref<PhaseTemplateDetail | undefined>(undefined);
const detailActionMode = ref(FormActionMode.CREATE);
const detailDialogOptions = reactive({
  visible: false,
  title: "Detall de la plantilla",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  id.value = route.params.id as string;
  await loadViewData();

  let pageTitle = "Plantilla de fase";
  if (phaseTemplate.value) {
    pageTitle = `${pageTitle} - ${phaseTemplate.value.name}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.LIST,
    backButtonVisible: true,
    title: pageTitle,
  });
});

const loadViewData = async () => {
  await phaseTemplateStore.fetchOne(id.value);
  plantModelStore.fetchActiveModel();
};

const onPhaseTemplateSubmit = async (model: PhaseTemplate) => {
  const updated = await phaseTemplateStore.update(id.value, model);
  if (updated) {
    toast.add({
      severity: "success",
      summary: "Actualitzada",
      life: 3000,
    });
    await loadViewData();
  }
};

// Details
const addDetail = (detail: PhaseTemplateDetail) => {
  selectedDetail.value = { ...detail };
  detailActionMode.value = FormActionMode.CREATE;
  detailDialogOptions.title = "Crear detall";
  detailDialogOptions.visible = true;
};

const editDetail = (detail: PhaseTemplateDetail) => {
  selectedDetail.value = { ...detail };
  detailActionMode.value = FormActionMode.EDIT;
  detailDialogOptions.title = "Editar detall";
  detailDialogOptions.visible = true;
};

const deleteDetail = async (detail: PhaseTemplateDetail) => {
  await phaseTemplateStore.deleteDetail(detail.id);
};

const onDetailSubmit = async (detail: PhaseTemplateDetail) => {
  if (detailActionMode.value === FormActionMode.CREATE) {
    await phaseTemplateStore.createDetail(detail);
  } else {
    await phaseTemplateStore.updateDetail(detail.id, detail);
  }
  detailDialogOptions.visible = false;
  selectedDetail.value = undefined;
};
</script>

<style scoped>
.main {
  margin-top: 1rem;
}
</style>
