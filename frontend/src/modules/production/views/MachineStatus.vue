<template>
  <FormMachineStatus
    v-if="machineStatus"
    :machineStatus="machineStatus"
    @submit="submitForm"
  />

  <section
    v-if="machineStatus && formMode === FormActionMode.EDIT"
    class="mt-4"
  >
    <TableMachineStatusReasons
      :reasons="machineStatus.reasons || []"
      :machineStatusId="machineStatus.id"
      @add="onAddReason"
      @edit="onEditReason"
      @delete="onDeleteReason"
    />
  </section>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :modal="true"
    :closable="true"
    :style="{ width: '50vw' }"
    @hide="cleanSelecteds"
  >
    <FormMachineStatusReason
      v-if="selectedReason"
      :reason="selectedReason"
      :existingReasons="machineStatus?.reasons || []"
      @submit="onReasonSubmit"
      @cancel="dialogOptions.visible = false"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { onMounted, ref, reactive } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { MachineStatus, MachineStatusReason } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormMachineStatus from "../components/FormMachineStatus.vue";
import TableMachineStatusReasons from "../components/TableMachineStatusReasons.vue";
import FormMachineStatusReason from "../components/FormMachineStatusReason.vue";
import { usePlantModelStore } from "../store/plantmodel";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const { machineStatus } = storeToRefs(plantmodelStore);

const loadView = async () => {
  await plantmodelStore.fetchMachineStatus(route.params.id as string);
  let pageTitle = "";

  if (!machineStatus.value) {
    formMode.value = FormActionMode.CREATE;
    plantmodelStore.setNewMachineStatus(route.params.id as string);
    pageTitle = "Alta d'estat de màquina";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Estat de màquina ${machineStatus.value.name}`;
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
  const data = machineStatus.value as MachineStatus;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await plantmodelStore.createMachineStatus(data);
    message = "Estat de màquina creat correctament";
  } else {
    result = await plantmodelStore.updateMachineStatus(data.id, data);
    message = "Estat de màquina actualizat correctament";
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

// Machine Status Reason Dialog Management
const selectedReason = ref<MachineStatusReason | undefined>();
const reasonFormMode = ref(FormActionMode.CREATE);
const dialogOptions = reactive({
  visible: false,
  title: "",
});

const onAddReason = (reason: MachineStatusReason) => {
  selectedReason.value = reason;
  reasonFormMode.value = FormActionMode.CREATE;
  dialogOptions.title = "Afegir motiu d'estat";
  dialogOptions.visible = true;
};

const onEditReason = (reason: MachineStatusReason) => {
  selectedReason.value = { ...reason }; // Clone to avoid direct mutation
  reasonFormMode.value = FormActionMode.EDIT;
  dialogOptions.title = "Editar motiu d'estat";
  dialogOptions.visible = true;
};

const onDeleteReason = async (id: string) => {
  const result = await plantmodelStore.deleteReason(id);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Motiu eliminat correctament",
      life: 4000,
    });
  }
};

const onReasonSubmit = async (reason: MachineStatusReason) => {
  const result =
    reasonFormMode.value === FormActionMode.CREATE
      ? await plantmodelStore.createReason(reason)
      : await plantmodelStore.updateReason(reason.id, reason);

  if (result) {
    dialogOptions.visible = false;
    toast.add({
      severity: "success",
      summary:
        reasonFormMode.value === FormActionMode.CREATE
          ? "Motiu creat correctament"
          : "Motiu actualitzat correctament",
      life: 4000,
    });
  }
};

const cleanSelecteds = () => {
  selectedReason.value = undefined;
};
</script>
