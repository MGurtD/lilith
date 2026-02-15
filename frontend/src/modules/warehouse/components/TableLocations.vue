<template>
  <main>
    <DataTable
      @row-click="onEditRow"
      :value="locations"
      tableStyle="min-width: 100%"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-900 font-bold">Ubicacions</span>
          <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAddClick" />
        </div>
      </template>
      <Column field="name" header="Nom" style="width: 25%"></Column>
      <Column
        field="description"
        header="Descripció"
        style="width: 60%"
      ></Column>
      <Column header="Desactivada" style="width: 10%">
        <template #body="slotProps">
          <BooleanColumn :value="slotProps.data.disabled"></BooleanColumn>
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
  </main>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    @hide="dialogClosed"
  >
    <FormLocation
      v-if="selectedLocation"
      :location="selectedLocation"
      @submit="onLocationSubmit"
    ></FormLocation>
  </Dialog>
</template>

<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Warehouse, Location } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { useConfirm } from "primevue/useconfirm";
import { reactive, ref } from "vue";
import { DialogOptions, FormActionMode } from "../../../types/component";
import FormLocation from "./FormLocation.vue";

const props = defineProps<{
  warehouse: Warehouse;
  locations: Array<Location>;
}>();

const emit = defineEmits<{
  (e: "add", location: Location): void;
  (e: "edit", location: Location): void;
  (e: "delete", location: Location): void;
}>();

const dialogOptions = reactive({
  visible: false,
  title: "Ubicació",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);
const confirm = useConfirm();

const selectedLocation = ref(undefined as Location | undefined);
const formAction = ref(FormActionMode.CREATE);

const onAddClick = () => {
  openDialog(FormActionMode.CREATE, {
    id: getNewUuid(),
    warehouseId: props.warehouse.id,
    disabled: false,
    name: "",
    description: "",
  } as Location);
};
const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    openDialog(FormActionMode.EDIT, row.data);
  }
};
const openDialog = (action: FormActionMode, location: Location) => {
  formAction.value = action;
  selectedLocation.value = location;
  dialogOptions.visible = true;
  dialogOptions.title =
    action === FormActionMode.CREATE ? "Crear ubicació" : "Actualitzar";
};
const onLocationSubmit = (location: Location) => {
  if (formAction.value === FormActionMode.CREATE) {
    emit("add", location);
  } else if (formAction.value === FormActionMode.EDIT) {
    emit("edit", location);
  }
  dialogOptions.visible = false;
};

const dialogClosed = () => {
  selectedLocation.value = undefined;
};

const onDeleteRow = (event: any, location: Location) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la ubicació '${location.name}'?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", location);
    },
  });
};
</script>
