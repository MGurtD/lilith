<template>
  <DataTable
    :value="plantmodelStore.operators"
    tableStyle="min-width: 100%"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ pt("Operaris") }}</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="code" :header="pt('Codi')" style="width: 15%"></Column>
    <Column :header="pt('Nom complet')" style="width: 35%">
      <template #body="slotProps">
        {{ slotProps.data.name }} {{ slotProps.data.surname }}
      </template>
    </Column>
    <Column field="vatNumber" header="NIF" style="width: 15%"></Column>
    <Column :header="pt('Tipus')" style="width: 15%">
      <template #body="slotProps">
        {{
          operatorTypeStore.getOperatorTypeNameById(
            slotProps.data.operatorTypeId
          )
        }}
      </template>
    </Column>
    <Column :header="pt('Desactivat')" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { usePlantModelStore } from "../store/plantmodel";
import { useOperatorTypeStore } from "../store/operatortype";
import { onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Operator } from "../types";

const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const operatorTypeStore = useOperatorTypeStore();
const toast = useToast();
const confirm = useConfirm();

onMounted(async () => {
  await plantmodelStore.fetchOperators();
  if (!operatorTypeStore.operatorTypes) {
    await operatorTypeStore.fetchOperatorTypes();
  }

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Gestió d'operaris",
  });
});

const createButtonClick = () => {
  router.push({ path: `/operator/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/operator/${row.data.id}` });
  }
};

const deleteButton = (event: any, operator: Operator) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar l'operari ${operator.surname}, ${operator.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteOperator(operator.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminat"),
          life: 3000,
        });
        await plantmodelStore.fetchOperators();
      }
    },
  });
};
</script>
