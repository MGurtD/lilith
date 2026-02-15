<template>
  <div>
    <DataTable
      :value="workcenterProfitPercentages"
      class="p-datatable-sm"
      tableStyle="min-width: 100%"
      scrollable
      scrollHeight="flex"
      stripedRows
      :rowHover="true"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-900 font-bold">Percentatges de benefici</span>
          <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAddClick" />
        </div>
      </template>
      <template #empty>No s'han trobat percentatges.</template>
      <template #loading>Carregant percentatges. Si us plau espera.</template>
      <Column
        field="profitPercentage"
        header="Percentatge de benefici (%)"
        sortable
        style="width: 85%"
      >
        <template #body="slotProps">
          {{ slotProps.data.profitPercentage }}%
        </template>
      </Column>
      <Column style="width: 15%">
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
      :style="{ width: '450px' }"
    >
      <div class="flex flex-column gap-3 mt-3">
        <div class="flex flex-column gap-2">
          <label for="profitPercentage">Percentatge de profit (%)</label>
          <InputNumber
            id="profitPercentage"
            v-model="newPercentage.profitPercentage"
            :min="0"
            :max="100"
            :minFractionDigits="2"
            :maxFractionDigits="2"
            suffix="%"
            :class="{
              'p-invalid': submitted && !newPercentage.profitPercentage,
            }"
          />
          <small
            v-if="submitted && !newPercentage.profitPercentage"
            class="p-error"
          >
            El percentatge és obligatori.
          </small>
        </div>
      </div>

      <template #footer>
        <Button
          label="Cancel·lar"
          :icon="PrimeIcons.TIMES"
          text
          @click="dialogOptions.visible = false"
        />
        <Button
          label="Guardar"
          :icon="PrimeIcons.CHECK"
          @click="onSaveHandler"
        />
      </template>
    </Dialog>
  </div>
</template>
<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { WorkcenterProfitPercentage } from "../types";
import { reactive, ref } from "vue";
import { DialogOptions } from "../../../types/component";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { getNewUuid } from "../../../utils/functions";

const props = defineProps<{
  workcenterProfitPercentages?: Array<WorkcenterProfitPercentage>;
  workcenterId: string;
}>();

const emits = defineEmits<{
  (e: "delete", percentage: WorkcenterProfitPercentage): void;
  (e: "add", percentage: WorkcenterProfitPercentage): void;
}>();

const dialogOptions = reactive({
  visible: false,
  title: "Nou percentatge de profit",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const toast = useToast();
const confirm = useConfirm();
const submitted = ref(false);

const newPercentage = ref({} as WorkcenterProfitPercentage);

const onAddClick = () => {
  submitted.value = false;
  newPercentage.value = {
    id: getNewUuid(),
    workcenterId: props.workcenterId,
    profitPercentage: 0,
    disabled: false,
  } as WorkcenterProfitPercentage;

  dialogOptions.visible = true;
};

const onSaveHandler = () => {
  submitted.value = true;

  if (
    !newPercentage.value.profitPercentage ||
    newPercentage.value.profitPercentage <= 0
  ) {
    toast.add({
      severity: "warn",
      summary: "Percentatge invàlid",
      detail: "El percentatge ha de ser major que 0",
      life: 5000,
    });
    return;
  }

  // Comprovar si ja existeix aquest percentatge
  const exists = props.workcenterProfitPercentages?.find(
    (p) => p.profitPercentage === newPercentage.value.profitPercentage,
  );
  if (exists) {
    toast.add({
      severity: "warn",
      summary: "Percentatge duplicat",
      detail: `El percentatge ${newPercentage.value.profitPercentage}% ja existeix`,
      life: 5000,
    });
    return;
  }

  dialogOptions.visible = false;
  emits("add", newPercentage.value);
};

const onDeleteRow = (event: Event, percentage: WorkcenterProfitPercentage) => {
  if (!event.currentTarget) return;

  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: `Estàs segur que vols eliminar el percentatge ${percentage.profitPercentage}%?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emits("delete", percentage);
    },
  });
};
</script>
