<template>
  <main class="container">
    <section class="two-columns">
      <div>
        <Table
          :items="shiftStore.shifts ?? []"
          :columns="shiftColumns"
          :filter-config="[]"
          :show-filter-actions="false"
          tableStyle="min-width: 100%"
          @create="openShift"
          @row-click="selectShift"
        >
          <template #prepend>
            <span class="text-900 font-bold">{{ pt("Torns") }}</span>
          </template>
        </Table>
      </div>
      <div>
        <Table
          :items="shiftStore.shiftdetails ?? []"
          :columns="shiftDetailColumns"
          :filter-config="[]"
          :show-filter-actions="false"
          :show-create="Boolean(selectedShift)"
          tableStyle="min-width: 100%"
          @create="openShiftDetail"
        >
          <template #prepend>
            <span class="text-900 font-bold">{{ pt("Horaris") }}</span>
          </template>
        </Table>
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
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import { computed, onMounted, reactive, ref } from "vue";
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

const store = useStore();
const shiftStore = useShiftStore();
const toast = useToast();

const { shift, shiftdetail } = storeToRefs(shiftStore);

const shiftColumns = computed<Column[]>(() => [
  { field: "name", header: pt("Nom") },
  {
    field: "disabled",
    header: pt("Desactivat"),
    columnType: ColumnType.Boolean,
  },
]);

const shiftDetailColumns = computed<Column[]>(() => [
  { field: "startTime", header: pt("Hora inici") },
  { field: "endTime", header: pt("Hora fi") },
  {
    field: "isProductiveTime",
    header: pt("Temps Productiu"),
    columnType: ColumnType.Boolean,
  },
]);

const openShift = () => {
  shiftStore.setNewShift(getNewUuid());
  dialogOptions.visible = true;
  dialogOptions.title = t("production.detail.createShift");
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
    title: pt("Gestió de torns"),
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
      summary: pt("Torn creat correctament"),
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
      summary: pt("Detall creat correctament"),
      life: 5000,
    });
  }
};
</script>
