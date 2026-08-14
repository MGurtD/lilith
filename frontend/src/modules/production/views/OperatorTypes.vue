<template>
  <DataTable
    :value="plantmodelStore.operatorTypes"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ t("production.operatorTypes.title") }}</span>
        <Button
          :icon="PrimeIcons.PLUS"
          :aria-label="t('production.actions.create')"
          :title="t('production.actions.create')"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" :header="t('production.fields.name')" style="width: 25%"></Column>
    <Column field="description" :header="t('common.description')" style="width: 50%"></Column>
    <Column :header="t('production.fields.disabled')" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          :aria-label="t('production.actions.delete')"
          :title="t('production.actions.delete')"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { getNewUuid } from "../../../utils/functions";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { usePlantModelStore } from "../store/plantmodel";
import { onMounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { OperatorType } from "../types";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const plantmodelStore = usePlantModelStore();
const confirm = useConfirm();
const toast = useToast();
const { t } = useI18n();

onMounted(async () => {
  await plantmodelStore.fetchOperatorTypes();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.operatorTypes.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/operatortype/${getNewUuid()}` });
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/operatortype/${row.data.id}` });
  }
};

const deleteButton = (event: any, operatorType: OperatorType) => {
  confirm.require({
    target: event.currentTarget,
    message: t("production.messages.confirmDeleteOperatorType", { name: operatorType.name }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteOperatorType(operatorType.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("production.messages.deleted"),
          life: 3000,
        });
        await plantmodelStore.fetchOperatorTypes();
      }
    },
  });
};
</script>
