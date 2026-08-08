<template>
  <DataTable
    :value="taxStore.taxes"
    tableStyle="min-width: 100%"
    @row-click="edit"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ $t('shared.taxes.title') }}</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" :header="$t('shared.taxes.columns.name')" style="width: 30%"></Column>
    <Column
      field="percentatge"
      :header="$t('shared.taxes.columns.percentage')"
      style="width: 25%"
    ></Column>
    <Column :header="$t('shared.taxes.columns.reverseCharge')" style="width: 25%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.isReverseCharge" :showColor="false" />
      </template>
    </Column>
    <Column :header="$t('shared.taxes.columns.disabled')" style="width: 15%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteTax($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { onMounted, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useStore } from "../../../store";
import { useTaxesStore } from "../store/tax";
import { Tax } from "../types";

const router = useRouter();
const route = useRoute();
const store = useStore();
const taxStore = useTaxesStore();
const confirm = useConfirm();
const toast = useToast();
const { t } = useI18n();

const refreshMenu = () => {
  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: t("shared.taxes.menuTitle"),
  });
};

onMounted(async () => {
  await taxStore.fetchAll();
  refreshMenu();
});

// Re-fetch quan es torna a la ruta /taxes des d'una sub-ruta (ex: /tax/:id).
// El RouterView no manté la vista muntada (no hi ha KeepAlive), però Vue
// reutilitza la instància del component quan només canvien els params, de
// manera que onMounted no es torna a executar. Sense aquest watch, els canvis
// fets al formulari (ex: marcar inversió subjecte passiu) no es reflecteixen
// al llistat fins a recarregar la pàgina.
watch(
  () => route.fullPath,
  async (newPath, oldPath) => {
    const cameBackToList =
      newPath === "/taxes" && oldPath?.startsWith("/tax/") === true;
    if (cameBackToList) {
      await taxStore.fetchAll();
    }
  },
);

const createButtonClick = () => {
  router.push({ path: `/tax/${getNewUuid()}` });
};

const edit = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    router.push({ path: `/tax/${row.data.id}` });
  }
};

const deleteTax = (event: any, tax: Tax) => {
  confirm.require({
    target: event.currentTarget,
    message: t("shared.taxes.messages.confirmDelete"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await taxStore.delete(tax.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("shared.taxes.messages.deleted"),
          life: 3000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: t("shared.taxes.messages.deleteError"),
          life: 4000,
        });
      }
    },
  });
};
</script>
