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
    :header="detailDialogTitle"
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

import { computed, onMounted, ref, reactive, watch } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import { useStore } from "../../../store";
import { usePhaseTemplateStore } from "../store/phasetemplate";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { PhaseTemplate, PhaseTemplateDetail } from "../types";
import { usePlantModelStore } from "../store/plantmodel";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";

const route = useRoute();
const store = useStore();
const toast = useToast();
const { t, locale } = useI18n();
const phaseTemplateStore = usePhaseTemplateStore();
const plantModelStore = usePlantModelStore();
const { phaseTemplate } = storeToRefs(phaseTemplateStore);
const id = ref("");

const selectedDetail = ref<PhaseTemplateDetail | undefined>(undefined);
const detailActionMode = ref(FormActionMode.CREATE);
const detailDialogOptions = reactive({
  visible: false,
  closable: true,
  position: "center",
  modal: true,
});
const detailDialogTitle = computed(() =>
  detailActionMode.value === FormActionMode.CREATE
    ? t("phaseTemplates.details.dialogs.createTitle")
    : t("phaseTemplates.details.dialogs.editTitle"),
);

const setMenuTitle = () => {
  let pageTitle = t("phaseTemplates.pageTitle");
  if (phaseTemplate.value) {
    pageTitle = `${pageTitle} - ${phaseTemplate.value.name}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.LIST,
    backButtonVisible: true,
    title: pageTitle,
  });
};

watch(locale, setMenuTitle);

onMounted(async () => {
  id.value = route.params.id as string;
  await loadViewData();

  setMenuTitle();
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
      summary: t("phaseTemplates.messages.updated"),
      life: 3000,
    });
    await loadViewData();
    setMenuTitle();
  }
};

// Details
const addDetail = (detail: PhaseTemplateDetail) => {
  selectedDetail.value = { ...detail };
  detailActionMode.value = FormActionMode.CREATE;
  detailDialogOptions.visible = true;
};

const editDetail = (detail: PhaseTemplateDetail) => {
  selectedDetail.value = { ...detail };
  detailActionMode.value = FormActionMode.EDIT;
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
