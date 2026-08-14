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
      <div class="flex flex-column gap-3 mt-3">
        <DropdownWarehousesWithLocations
          :label="t('production.components.ubicacio')"
          v-model="selectedLocationId"
          :placeholder="t('production.components.seleccionaUnaUbicacio')"
        />
      </div>

      <template #footer>
        <Button
          :label="t('production.components.cancellar')"
          :icon="PrimeIcons.TIMES"
          text
          @click="dialogVisible = false"
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
import { ref } from "vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
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
const toast = useToast();

const dialogVisible = ref(false);
const selectedLocationId = ref<string | null>(null);

const onAddClick = () => {
  selectedLocationId.value = null;
  dialogVisible.value = true;
};

const onSaveHandler = () => {
  if (!selectedLocationId.value) {
    toast.add({
      severity: "warn",
      summary: t("production.components.ubicacioNoSeleccionada"),
      detail: t("production.components.hasDeSeleccionarUnaUbicacioPerContinuar"),
      life: 5000,
    });
    return;
  }

  const alreadyAssigned = props.workcenterLocations?.find(
    (wl) => wl.locationId === selectedLocationId.value,
  );
  if (alreadyAssigned) {
    toast.add({
      severity: "warn",
      summary: t("production.components.ubicacioDuplicada"),
      detail: t("production.components.aquestaUbicacioJaEstaAssignadaAAquestaMaquina"),
      life: 5000,
    });
    return;
  }

  dialogVisible.value = false;
  emits("add", selectedLocationId.value);
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
