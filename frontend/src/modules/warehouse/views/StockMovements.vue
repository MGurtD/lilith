<template>
  <div>
    <DataTable
      class="p-datatable-sm"
      tableStyle="min-width:100%"
      scrollable
      scrollHeight="flex"
      sortField="movementDate"
      :sortOrder="1"
      :value="stockMovementStore.stockMovements"
      :paginator="(stockMovementStore.stockMovements?.length ?? 0) > 20"
      :rows="20"
    >
      <template #header>
        <TableFilter
          :config="[]"
          v-model="filter"
          :show-title="false"
          :show-action-labels="false"
          :show-create="false"
          :body-width="filterBodyWidth"
          embedded
          @filter="filterMovements"
          @clear="cleanFilter"
        >
          <template #prepend>
            <div class="table-filter-prepend-field table-filter-prepend-field--lg">
              <label class="filter-label table-filter-prepend-label">{{ t("common.period") }}</label>
              <DatePicker
                v-model="filter.dates"
                selectionMode="range"
                dateFormat="dd/mm/yy"
                showIcon
                class="w-full"
                size="small"
                :placeholder="t('warehouse.placeholders.selectPeriod')"
              />
            </div>
            <div class="table-filter-prepend-field table-filter-prepend-field--md">
              <label class="filter-label table-filter-prepend-label">{{ t("warehouse.fields.location") }}</label>
              <DropdownWarehousesWithLocations
                label=""
                v-model="filter.locationId"
              />
            </div>
          </template>
        </TableFilter>
      </template>
      <Column :header="t('common.date')" field="movementDate" sortable style="width: 10%">
        <template #body="slotProps">
          {{ formatDateTime(slotProps.data.movementDate) }}
        </template>
      </Column>
      <Column :header="t('warehouse.fields.reference')" style="width: 15%">
        <template #body="slotProps">
          {{ referenceStore.getFullNameById(slotProps.data.referenceId) }}
        </template></Column
      >
      <Column :header="t('warehouse.fields.location')" style="width: 10%">
      <Column header="Lot" style="width: 8%">
        <template #body="slotProps">
          {{ getLotCode(slotProps.data.lotId) }}
        </template>
      </Column>
      <Column header="Ubicació" style="width: 10%">
        <template #body="slotProps">
          {{ slotProps.data.location?.name }}
        </template>
      </Column>
      <Column field="width" :header="t('warehouse.fields.widthMmAxis')" style="width: 5%"></Column>
      <Column field="length" :header="t('warehouse.fields.lengthMmAxis')" style="width: 5%"></Column>
      <Column field="height" :header="t('warehouse.fields.heightMmAxis')" style="width: 5%"></Column>
      <Column field="diameter" :header="t('warehouse.fields.diameterMm')" style="width: 5%"></Column>
      <Column field="thickness" :header="t('warehouse.fields.thicknessMm')" style="width: 5%"></Column>
      <Column
        field="description"
        :header="t('common.description')"
        style="width: 25%"
      ></Column>
      <Column
        :header="t('warehouse.fields.movementType')"
        field="movementType"
        style="width: 10%"
      >
        <template #body="slotProps">
          <TagMovementType :movementType="slotProps.data.movementType" />
        </template>
      </Column>
      <Column field="quantity" :header="t('warehouse.fields.quantity')" style="width: 8%"></Column>
      <Column header="" style="width: 4%">
        <template #body="slotProps">
          <Button
            icon="pi pi-sitemap"
            text
            rounded
            size="small"
            :disabled="!slotProps.data.lotId"
            v-tooltip.top="'Veure traçabilitat del lot'"
            @click="
              goToLotTraceability(
                slotProps.data.referenceId,
                slotProps.data.lotId,
              )
            "
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import TableFilter, {
  type FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";
import DropdownWarehousesWithLocations from "../components/DropdownWarehousesWithLocations.vue";
import TagMovementType from "../../../components/TagMovementType.vue";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useStockMovementStore } from "../store/stockMovement";
import { useReferenceStore } from "../../shared/store/reference";
import { useExerciseStore } from "../../shared/store/exercise";
import Services from "../services";
import { Lot } from "../types";
import { onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";
import {
  formatDateForQueryParameter,
  formatDateTime,
} from "../../../utils/functions";

const toast = useToast();
const { t } = useI18n();
const router = useRouter();
const store = useStore();
const stockMovementStore = useStockMovementStore();
const referenceStore = useReferenceStore();
const exerciseStore = useExerciseStore();

const lotsById = ref<Record<string, Lot>>({});

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  locationId: undefined as string | undefined,
});

const filterBodyWidth: FilterBodyWidth = {
  desktop: "55%",
  tablet: "70%",
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MAP,
    title: t("warehouse.stockMovements.title"),
  });
  await exerciseStore.fetchAll();
  await referenceStore.fetchReferences();
  setCurrentYear();
});

const setCurrentYear = () => {
  const year = new Date().getFullYear().toString();
  const currentExercise = exerciseStore.exercises?.find((e) => e.name === year);

  if (currentExercise) {
    filter.value.dates = [
      new Date(currentExercise.startDate),
      new Date(currentExercise.endDate),
    ];
  }
};

const cleanFilter = () => {
  filter.value.dates = undefined;
  filter.value.locationId = undefined;
};

const getLotCode = (lotId?: string | null) => {
  if (!lotId) return "—";
  return lotsById.value[lotId]?.code ?? "—";
};

const resolveLotCodes = async () => {
  const lotIds = Array.from(
    new Set(
      (stockMovementStore.stockMovements ?? [])
        .map((movement) => movement.lotId)
        .filter((id): id is string => !!id && !lotsById.value[id]),
    ),
  );

  await Promise.all(
    lotIds.map(async (lotId) => {
      const lot = await Services.Lot.getById(lotId);
      if (lot) lotsById.value[lotId] = lot;
    }),
  );
};

const goToLotTraceability = (referenceId: string, lotId?: string | null) => {
  if (!lotId) return;
  router.push({
    path: "/lot-traceability",
    query: { referenceId, lotId },
  });
};

watch(
  () => stockMovementStore.stockMovements,
  () => resolveLotCodes(),
);

const filterMovements = async () => {
  if (filter.value.dates && filter.value.dates[1]) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await stockMovementStore.getBetweenDates(
      startTime,
      endTime,
      filter.value.locationId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: t("warehouse.messages.invalidFilter"),
      detail: t("warehouse.messages.selectPeriod"),
      life: 5000,
    });
  }
};
</script>
