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
        <span class="text-900 font-bold">Impostos</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>
    <Column field="name" header="Nom" style="width: 33%"></Column>
    <Column
      field="percentatge"
      header="% Percentatge"
      style="width: 33%"
    ></Column>
    <Column header="Desactivada" style="width: 33%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { PrimeIcons } from "@primevue/core/api";
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useStore } from "../../../store";
import { useTaxesStore } from "../store/tax";

const router = useRouter();
const store = useStore();
const taxStore = useTaxesStore();

onMounted(async () => {
  await taxStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: "Gestió d'impostos",
  });
});

const createButtonClick = () => {
  router.push({ path: `/tax/${uuidv4()}` });
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
</script>
