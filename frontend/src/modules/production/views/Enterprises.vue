<template>
  <DataTable
    :value="plantmodelStore.enterprises"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    @row-click="editEnterprise"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ t("production.enterprises.title") }}</span>
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
    <Column field="defaultSiteId" :header="t('production.enterprises.defaultSite')" style="width: 15%">
      <template #body="slotProps">
        {{ plantmodelStore.getSiteNameById(slotProps.data.defaultSiteId) }}
      </template>
    </Column>
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
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { Enterprise } from "../types";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const toast = useToast();
const confirm = useConfirm();
const plantmodelStore = usePlantModelStore();
const { t } = useI18n();

onMounted(async () => {
  await plantmodelStore.fetchEnterprises();
  if (!plantmodelStore.sites) await plantmodelStore.fetchSites();

  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: t("production.enterprises.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/enterprise/${getNewUuid()}` });
};

const editEnterprise = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/enterprise/${row.data.id}` });
  }
};
const deleteButton = (event: any, entity: Enterprise) => {
  confirm.require({
    target: event.currentTarget,
    message: t("production.messages.confirmDeleteEnterprise", { name: entity.name }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await plantmodelStore.deleteEnterprise(entity.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("production.messages.deleted"),
          life: 3000,
        });
        await plantmodelStore.fetchEnterprises();
      }
    },
  });
};
</script>
