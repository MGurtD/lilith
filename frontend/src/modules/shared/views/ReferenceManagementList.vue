<template>
  <div>
    <DataTable
      :value="references"
      tableStyle="min-width: 100%"
      scrollable
      scrollHeight="flex"
      :loading="loading"
      paginator
      :rows="20"
      sortField="code"
      :sortOrder="1"
      removableSort
      filterDisplay="menu"
      v-model:filters="filters"
      :globalFilterFields="['code', 'description']"
      @row-click="openReference"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <IconField>
            <InputIcon class="pi pi-search" />
            <InputText
              v-model="filters['global'].value"
              placeholder="Cercar..."
            />
          </IconField>
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createReference"
          />
        </div>
      </template>
      <template #empty>Sense referències.</template>
      <Column field="code" header="Codi" sortable />
      <Column field="description" header="Descripció" sortable />
      <Column field="version" header="Versió" />
      <Column header="Ventes">
        <template #body="{ data }">
          <i
            v-if="data.sales"
            :class="PrimeIcons.CHECK"
            class="text-green-500"
          />
        </template>
      </Column>
      <Column header="Compres">
        <template #body="{ data }">
          <i
            v-if="data.purchase"
            :class="PrimeIcons.CHECK"
            class="text-green-500"
          />
        </template>
      </Column>
      <Column header="Producció">
        <template #body="{ data }">
          <i
            v-if="data.production"
            :class="PrimeIcons.CHECK"
            class="text-green-500"
          />
        </template>
      </Column>
      <Column header="Activa">
        <template #body="{ data }">
          <i
            v-if="!data.disabled"
            :class="PrimeIcons.CHECK"
            class="text-green-500"
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { FilterMatchMode } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";

import { useStore } from "../../../store";
import { useReferenceStore } from "../store/reference";
import { getNewUuid } from "../../../utils/functions";

const router = useRouter();
const store = useStore();
const referenceStore = useReferenceStore();

const { references } = storeToRefs(referenceStore);
const loading = ref(false);

const filters = ref({
  global: { value: null as string | null, matchMode: FilterMatchMode.CONTAINS },
});

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.BOX,
    backButtonVisible: false,
    title: "Gestió de referències",
  });
  loading.value = true;
  await referenceStore.fetchReferences();
  loading.value = false;
});

const openReference = (row: DataTableRowClickEvent) => {
  router.push({ path: `/reference-management/${row.data.id}` });
};

const createReference = () => {
  router.push({ path: `/reference-management/${getNewUuid()}` });
};
</script>
