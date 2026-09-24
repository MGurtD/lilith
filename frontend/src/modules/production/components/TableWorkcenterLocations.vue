<template>
  <div>
    <DataTable
      :value="workcenterLocations"
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
          <span class="text-900 font-bold">{{ t("production.components.ubicacionsAssignades") }}</span>
          <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAddClick" />
        </div>
      </template>
      <template #empty>{{ t("production.components.noSHanTrobatUbicacions") }}</template>
      <template #loading>{{ t("production.components.carregantUbicacionsSiUsPlauEspera") }}</template>
      <Column :header="t('production.components.magatzem')" style="width: 40%">
        <template #body="slotProps">
          {{
            slotProps.data.location?.description ||
            slotProps.data.location?.name ||
            "-"
          }}
        </template>
      </Column>
      <Column :header="t('production.components.ubicacio')" style="width: 50%">
        <template #body="slotProps">
          {{ slotProps.data.location?.name || "-" }}
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
      v-model:visible="dialogVisible"
      :header="t('production.components.vincularUbicacio')"
      :closable="true"
      :modal="true"
      :style="{ width: '450px' }"
    >
      <Form
        class="mt-3"
        :rows="rows"
        :initial-values="initialValues"
        @submit="onSubmit"
        @cancel="dialogVisible = false"
      >
        <template #field-locationId="{ value, setValue, disabled, inputId }">
          <DropdownWarehousesWithLocations
            :input-id="inputId"
            :model-value="typeof value === 'string' ? value : null"
            :placeholder="t('production.components.seleccionaUnaUbicacio')"
            :disabled="disabled"
            @update:model-value="setValue"
          />
        </template>
      </Form>
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
import { stringValue } from "@/components/forms/value-utils";
import { computed, ref } from "vue";
import { useConfirm } from "primevue/useconfirm";
import * as Yup from "yup";
import { WorkcenterLocation } from "../types";
import DropdownWarehousesWithLocations from "../../warehouse/components/DropdownWarehousesWithLocations.vue";

const props = defineProps<{
  workcenterLocations?: Array<WorkcenterLocation>;
  workcenterId: string;
}>();

const emits = defineEmits<{
  (e: "delete", entity: WorkcenterLocation): void;
  (e: "add", locationId: string): void;
}>();

const confirm = useConfirm();

const dialogVisible = ref(false);

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "locationId",
        label: t("production.components.ubicacio"),
        type: FormFieldType.Custom,
        validation: Yup.string()
          .nullable()
          .required(
            t("production.components.hasDeSeleccionarUnaUbicacioPerContinuar"),
          )
          .test(
            "location-not-assigned",
            t("production.components.aquestaUbicacioJaEstaAssignadaAAquestaMaquina"),
            (value) =>
              !value ||
              !props.workcenterLocations?.some((wl) => wl.locationId === value),
          ),
      },
    ],
  },
]);

// Stable reference: an inline literal would reset the form on every render.
// The dialog content is unmounted when hidden, so every opening starts from
// an empty selection.
const initialValues = { locationId: null };

const onAddClick = () => {
  dialogVisible.value = true;
};

const onSubmit = (values: FormValues) => {
  dialogVisible.value = false;
  emits("add", stringValue(values.locationId, ""));
};

const onDeleteRow = (event: Event, entity: WorkcenterLocation) => {
  if (!event.currentTarget) return;

  confirm.require({
    target: event.currentTarget as HTMLElement,
    message: t("production.messages.confirmUnlinkLocation", {
      name: entity.location?.name || entity.locationId,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emits("delete", entity);
    },
  });
};
</script>
