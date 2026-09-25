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
      <Form
        class="mt-3"
        :rows="rows"
        :initial-values="newPercentage"
        @submit="onSubmit"
        @cancel="dialogOptions.visible = false"
      />
    </Dialog>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import { PrimeIcons } from "@primevue/core/api";
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { finiteNumberValue } from "@/components/forms/value-utils";
import { WorkcenterProfitPercentage } from "../types";
import { computed, reactive, ref } from "vue";
import { DialogOptions } from "../../../types/component";
import { useConfirm } from "primevue/useconfirm";
import * as Yup from "yup";
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

const confirm = useConfirm();

const newPercentage = ref({} as WorkcenterProfitPercentage);

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "profitPercentage",
        label: t("production.components.percentatgeDeProfit"),
        type: FormFieldType.Number,
        props: {
          locale: "en-US",
          min: 0,
          max: 100,
          minFractionDigits: 2,
          maxFractionDigits: 2,
          suffix: "%",
        },
        validation: Yup.number()
          .typeError(t("production.components.elPercentatgeEsObligatori"))
          .required(t("production.components.elPercentatgeEsObligatori"))
          .moreThan(0, t("production.components.elPercentatgeHaDeSerMajorQue0"))
          .test("unique-percentage", function (value) {
            const exists = props.workcenterProfitPercentages?.some(
              (p) => p.profitPercentage === value,
            );
            return exists
              ? this.createError({
                  message: t("production.components.duplicatePercentage", {
                    percentage: value,
                  }),
                })
              : true;
          }),
      },
    ],
  },
]);

const onAddClick = () => {
  newPercentage.value = {
    id: getNewUuid(),
    workcenterId: props.workcenterId,
    profitPercentage: 0,
    disabled: false,
  };

  dialogOptions.visible = true;
};

const onSubmit = (values: FormValues) => {
  dialogOptions.visible = false;
  emits("add", {
    ...newPercentage.value,
    profitPercentage: finiteNumberValue(
      values.profitPercentage,
      newPercentage.value.profitPercentage,
    ),
  });
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
