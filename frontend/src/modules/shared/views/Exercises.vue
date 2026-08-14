<template>
  <DataTable
    :value="exercises"
    dataKey="id"
    tableStyle="min-width: 100%"
    @row-click="editExercise"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <span class="text-900 font-bold">{{ $t('shared.exercises.title') }}</span>
        <Button
          :icon="PrimeIcons.PLUS"
          rounded
          raised
          @click="createButtonClick"
        />
      </div>
    </template>

    <Column field="name" :header="$t('shared.exercises.columns.name')" style="width: 15%"></Column>
    <Column field="description" :header="$t('shared.exercises.columns.description')" style="width: 25%"></Column>
    <Column :header="$t('shared.exercises.columns.startDate')" style="width: 20%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.startDate) }}
      </template>
    </Column>
    <Column :header="$t('shared.exercises.columns.endDate')" style="width: 20%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.endDate) }}
      </template>
    </Column>
    <Column :header="$t('shared.exercises.columns.disabled')" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { storeToRefs } from "pinia";
import { useStore } from "../../../store";
import { useExerciseStore } from "../store/exercise";
import { formatDate, getNewUuid } from "../../../utils/functions";

const router = useRouter();
const store = useStore();
const exerciseStore = useExerciseStore();
const { exercises } = storeToRefs(exerciseStore);
const { t } = useI18n();

onMounted(async () => {
  await exerciseStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: t("shared.exercises.menuTitle"),
  });
});

const createButtonClick = () => {
  router.push({ path: `/exercise/${getNewUuid()}` });
};

const editExercise = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/exercise/${row.data.id}` });
  }
};
</script>
