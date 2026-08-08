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
          <span class="text-900 font-bold">{{ t("production.components.percentatgesDeBenefici") }}</span>
          <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAddClick" />
        </div>
      </template>
      <template #empty>{{ t("production.components.noSHanTrobatPercentatges") }}</template>
      <template #loading>{{ t("production.components.carregantPercentatgesSiUsPlauEspera") }}</template>
      <Column
        field="profitPercentage"
        :header="t('production.components.percentatgeDeBenefici')"
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
          <label for="profitPercentage">{{ t("production.components.percentatgeDeProfit") }}</label>
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
            {{ t("production.components.elPercentatgeEsObligatori") }}
          </small>
        </div>
      </div>

      <template #footer>
        <Button
          :label="t('production.components.cancellar')"
          :icon="PrimeIcons.TIMES"
          text
          @click="dialogOptions.visible = false"
        />
        <Button
          :label="t('production.components.guardar')"
          :icon="PrimeIcons.CHECK"
          @click="onSaveHandler"
        />
      </template>
    </Dialog>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
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
  title: t("production.components.nouPercentatgeDeProfit"),
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
      summary: t("production.components.percentatgeInvalid"),
      detail: t("production.components.elPercentatgeHaDeSerMajorQue0"),
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
      summary: t("production.components.percentatgeDuplicat"),
      detail: t("production.components.duplicatePercentage", { percentage: newPercentage.value.profitPercentage }),
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
    message: t("production.components.confirmDeletePercentage", { percentage: percentage.profitPercentage }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emits("delete", percentage);
    },
  });
};
</script>
