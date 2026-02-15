<template>
  <main class="container">
    <section class="two-columns">
      <div>
        <DataTable
          :value="shiftStore.shifts"
          tableStyle="min-width: 100%"
          @row-click="selectShift"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-900 font-bold">Torns</span>
              <Button
                :icon="PrimeIcons.PLUS"
                rounded
                raised
                @click="openShift"
              />
            </div>
          </template>
          <Column field="name" header="Nom"></Column>
          <Column field="disabled" header="Desactivat">
            <template #body="slotProps">
              <BooleanColumn :value="slotProps.data.disabled" />
            </template>
          </Column>
        </DataTable>
      </div>
      <div>
        <DataTable
          :value="shiftStore.shiftdetails"
          tableStyle="min-width: 100%"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-900 font-bold">Horaris</span>
              <Button
                :icon="PrimeIcons.PLUS"
                rounded
                raised
                @click="openShiftDetail"
              />
            </div>
          </template>
          <Column field="startTime" header="Hora inici"></Column>
          <Column field="endTime" header="Hora fi"></Column>
          <Column field="isProductiveTime" header="Temps Productiu">
            <template #body="slotProps">
              <BooleanColumn :value="slotProps.data.isProductiveTime" />
            </template>
          </Column>
        </DataTable>
      </div>
    </section>
  </main>
  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <FormShift v-if="shift" :shift="shift" @submit="submitShift" />
  </Dialog>
  <Dialog
    v-model:visible="dialogOptionsDetail.visible"
    :header="dialogOptionsDetail.title"
    :closable="dialogOptionsDetail.closable"
    :modal="dialogOptionsDetail.modal"
  >
    <FormShiftDetail
      v-if="shiftdetail"
      :shiftdetail="shiftdetail"
      @submit="submitShiftDetail"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { onMounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "../../../store";
import { Shift, ShiftDetail } from "../types";
import { useToast } from "primevue/usetoast";
import { useShiftStore } from "../store/shift";
import { DataTableRowClickEvent } from "primevue/datatable";
import { FormActionMode, DialogOptions } from "../../../types/component";
import FormShift from "../components/FormShift.vue";
import FormShiftDetail from "../components/FormShiftDetail.vue";
import { storeToRefs } from "pinia";
import { getNewUuid } from "../../../utils/functions";
import BooleanColumn from "../../../components/tables/BooleanColumn.vue";

const store = useStore();
const shiftStore = useShiftStore();
const toast = useToast();

const { shift, shiftdetail } = storeToRefs(shiftStore);

const openShift = () => {
  shiftStore.setNewShift(getNewUuid());
  dialogOptions.visible = true;
  dialogOptions.title = "Alta de torns";
};

const openShiftDetail = () => {
  shiftStore.setNewShiftDetail(getNewUuid());
  shiftdetail.value!.shiftId = selectedShift.value!.id;
  dialogOptionsDetail.visible = true;
  dialogOptionsDetail.title = "Configuració de torns";
};

const dialogOptions = reactive({
  visible: false,
  title: "",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const dialogOptionsDetail = reactive({
  visible: false,
  title: "",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const selectedShift = ref(undefined as Shift | undefined);

const selectShift = async (row: DataTableRowClickEvent) => {
  await shiftStore.fetchShiftDetailsByShiftId(row.data.id);
  selectedShift.value = row.data;
};

onMounted(async () => {
  await shiftStore.fetchAllShifts();

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: false,
    title: "Gestió de torns",
  });
});

const submitShift = async () => {
  const data = shift.value as Shift;
  let result = false;
  result = await shiftStore.createShift(data);
  if (result) {
    dialogOptions.visible = false;
    toast.add({
      severity: "success",
      summary: "Torn creat correctament",
      life: 5000,
    });
  }
};
const submitShiftDetail = async () => {
  const data = shiftdetail.value as ShiftDetail;
  let result = false;
  result = await shiftStore.createDetail(data);
  if (result) {
    dialogOptionsDetail.visible = false;
    toast.add({
      severity: "success",
      summary: "Detall creat correctament",
      life: 5000,
    });
  }
};
</script>
