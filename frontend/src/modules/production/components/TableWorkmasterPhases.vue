<template>
  <DataTable
    @row-click="onEditRow"
    class="p-datatable-sm clickable-rows"
    :value="workmasterPhases"
    tableStyle="min-width: 100%"
    sort-field="code"
    :sort-order="1"
    scrollable
    scrollHeight="flex"
    stripedRows
    :rowHover="true"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">Fases de la ruta</span>
        <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAddClick" />
      </div>
    </template>
    <Column field="code" header="Codi" sortable style="width: 10%"></Column>
    <Column field="description" header="Descripció" style="width: 20%"></Column>
    <Column header="Tipus de màquina" style="width: 15%">
      <template #body="slotProps">
        {{ getWorkcenterType(slotProps.data.workcenterTypeId) }}
      </template>
    </Column>
    <Column header="Màquina preferida" style="width: 15%">
      <template #body="slotProps">
        {{ getWorkcenter(slotProps.data.preferredWorkcenterId) }}
      </template>
    </Column>
    <Column header="Tipus d'operari" style="width: 15%">
      <template #body="slotProps">
        {{ getOperatorType(slotProps.data.operatorTypeId) }}
      </template>
    </Column>
    <Column header="Extern" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isExternalWork"></BooleanColumn>
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <FormWorkmasterPhase
      v-if="newPhase"
      :workmaster="workmaster"
      :phase="newPhase"
      @submit="onAddHandler"
    ></FormWorkmasterPhase>
  </Dialog>
</template>

<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { WorkMaster, WorkMasterPhase } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import FormWorkmasterPhase from "./FormWorkmasterPhase.vue";
import { DialogOptions } from "../../../types/component";
import { reactive, ref } from "vue";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  workmaster: WorkMaster;
  workmasterPhases: Array<WorkMasterPhase>;
}>();

const emit = defineEmits<{
  (e: "add", phase: WorkMasterPhase): void;
  (e: "edit", phase: WorkMasterPhase): void;
  (e: "delete", phase: WorkMasterPhase): void;
}>();

const dialogOptions = reactive({
  visible: false,
  title: "Nova fase",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const toast = useToast();
const confirm = useConfirm();
const plantModelStore = usePlantModelStore();

const getWorkcenterType = (id: string) => {
  if (!plantModelStore.workcenterTypes) return "";
  const entity = plantModelStore.workcenterTypes?.find((e) => id === e.id);
  if (!entity) return "";
  return entity.name;
};
const getWorkcenter = (id: string) => {
  if (!plantModelStore.workcenters) return "";
  const entity = plantModelStore.workcenters?.find((e) => id === e.id);
  if (!entity) return "";
  return entity.name;
};
const getOperatorType = (id: string) => {
  if (!plantModelStore.operatorTypes) return "";
  const entity = plantModelStore.operatorTypes?.find((e) => id === e.id);
  if (!entity) return "";
  return entity.name;
};

const getNextPhaseCode = () => {
  // Obtener el array de phases
  const phases = props.workmaster.phases;

  // Calcular el máximo valor de code
  const maxCode = phases.reduce((max, phase) => {
    const codeValue = parseInt(phase.code, 10) || 0; // Asegúrate de convertir a número
    return Math.max(max, codeValue);
  }, 0); // Valor inicial 0

  // Calcular el próximo número que termine en 0
  const nextCode = Math.ceil((maxCode + 1) / 10) * 10;

  // Retornar el resultado como string
  return nextCode.toString();
};

const newPhase = ref({} as WorkMasterPhase);
const onAddClick = () => {
  newPhase.value = {
    id: getNewUuid(),
    workMasterId: props.workmaster.id,
    disabled: false,
    code: getNextPhaseCode(),
    description: "",
    operatorTypeId: null,
    workcenterTypeId: null,
    preferredWorkcenterId: null,
    isExternalWork: false,
    externalWorkCost: 0,
    transportCost: 0,
    serviceReferenceId: null,
    profitPercentage: 0,
    workmasterPhaseDetails: [],
    workmasterPhaseBillOfMaterials: [],
  } as WorkMasterPhase;

  dialogOptions.visible = true;
};

const onAddHandler = (phase: WorkMasterPhase) => {
  const existsPhase = props.workmaster.phases.find(
    (p) => p.code === phase.code,
  );
  if (existsPhase) {
    toast.add({
      severity: "warn",
      summary: `Fase invàlida`,
      detail: `La fase ${phase.code} ja existeix`,
      life: 5000,
    });
    return;
  }

  dialogOptions.visible = false;
  emit("add", phase);
};

const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, phase: WorkMasterPhase) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la fase?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", phase);
    },
  });
};
</script>
