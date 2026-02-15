<template>
  <DataTable
    :value="filteredData"
    tableStyle="min-width: 100%"
    class="p-datatable-sm"
    scrollable
    scrollHeight="flex"
    paginator
    :rows="20"
    @row-click="editRow"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter-3">
          <div class="filter-field">
            <label>Categoria</label>
            <DropdownReferenceCategory
              label=""
              v-model="filter.referenceCategory"
            />
          </div>
          <div class="filter-field">
            <label>Codi</label>
            <BaseInput v-model="filter.code" />
          </div>
          <div class="filter-field">
            <label>Tipus</label>
            <DropdownReferenceTypes
              label=""
              v-model="filter.referenceTypeId"
              :disabled="
                filter.referenceCategory !== ReferenceCategoryEnum.MATERIAL
              "
              @change="cleanFilter"
            />
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
    </template>
    <Column field="code" header="Codi" style="width: 15%"></Column>
    <Column field="description" header="Descripció" style="width: 25%"></Column>
    <!-- Service columns -->
    <Column v-if="isService" header="Preu" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.price) }}
      </template>
    </Column>
    <Column v-if="isService" header="Transport" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.transportAmount) }}
      </template>
    </Column>
    <!-- Material columns -->
    <Column
      v-if="isMaterial"
      field="referenceTypeId"
      header="Tipus"
      style="width: 30%"
    >
      <template #body="slotProps">
        <span>{{ getTypeDescription(slotProps.data.referenceTypeId) }}</span>
      </template>
    </Column>
    <Column
      v-if="isMaterial"
      field="referenceFormatId"
      header="Format"
      style="width: 10%"
    >
      <template #body="slotProps">
        <span>{{
          getFormatDescription(slotProps.data.referenceFormatId)
        }}</span>
      </template>
    </Column>
    <Column v-if="isMaterial" header="Densitat (mm)" style="width: 10%">
      <template #body="slotProps">
        {{ getReferenceTypeDensity(slotProps.data.referenceTypeId) }}
      </template>
    </Column>
    <Column v-if="isTool" header="Àrea" style="width: 10%">
      <template #body="slotProps">
        {{ getAreaName(slotProps.data.areaId) }}
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
</template>

<script setup lang="ts">
import DropdownReferenceTypes from "../../../modules/shared/components/DropdownReferenceType.vue";
import BaseInput from "../../../components/BaseInput.vue";
import { computed, onMounted, onUnmounted, Ref, ref } from "vue";
import { useReferenceStore } from "../../shared/store/reference";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Reference, ReferenceCategoryEnum } from "../../shared/types";
import { useReferenceTypeStore } from "../../shared/store/referenceType";
import { useUserFilterStore } from "../../../store/userfilter";
import DropdownReferenceCategory from "../../shared/components/DropdownReferenceCategory.vue";
import { formatCurrency } from "../../../utils/functions";
import { usePlantModelStore } from "../../production/store/plantmodel";

const userFilterStore = useUserFilterStore();
const referenceTypeStore = useReferenceTypeStore();
const referenceStore = useReferenceStore();
const plantModelStore = usePlantModelStore();

const props = defineProps<{
  references: Array<Reference> | undefined;
  filter: any;
}>();

const emit = defineEmits<{
  (e: "add"): void;
  (e: "edit", reference: Reference): void;
  (e: "delete", reference: Reference): void;
}>();

onMounted(() => {
  const userFilter = userFilterStore.getFilter("Materials", "");
  if (userFilter) {
    props.filter.code = userFilter.code;
    props.filter.referenceTypeId = userFilter.referenceTypeId;
    props.filter.referenceCategory = userFilter.referenceCategory;
  }

  if (!plantModelStore.areas) {
    plantModelStore.fetchAreas();
  }
});
onUnmounted(() => {
  userFilterStore.addFilter("Materials", "", props.filter);
});

const cleanFilter = () => {
  props.filter.code = "";
  props.filter.referenceTypeId = "";
};

const selectedCategoryReferences = computed(() => {
  if (!props.references) return [];
  if (props.filter.referenceCategory === "") {
    return [];
  }

  return referenceStore.references!.filter(
    (r) => r.categoryName === props.filter.referenceCategory,
  );
});

const filteredData = computed(() => {
  if (!selectedCategoryReferences.value) return [];

  let filteredReferences = selectedCategoryReferences.value;

  // Type filter
  if (
    props.filter.referenceTypeId &&
    props.filter.referenceTypeId!.length > 0
  ) {
    filteredReferences = filteredReferences.filter(
      (r) => r.referenceTypeId === props.filter.referenceTypeId,
    );
  }
  // Code filter
  if (props.filter.code.length > 0) {
    filteredReferences = filteredReferences.filter((r) =>
      r.code.toLowerCase().includes(props.filter.code.toLowerCase()),
    );
  }

  return filteredReferences;
});

const isMaterial = computed(() => {
  return props.filter.referenceCategory === ReferenceCategoryEnum.MATERIAL;
});
const isTool = computed(() => {
  return props.filter.referenceCategory === ReferenceCategoryEnum.TOOL;
});
const isService = computed(() => {
  return props.filter.referenceCategory === ReferenceCategoryEnum.SERVICE;
});

const createButtonClick = () => {
  emit("add");
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    emit("edit", row.data);
  }
};

const onDeleteRow = (event: any, reference: Reference) => {
  emit("delete", reference);
};

const getReferenceTypeDensity = (referenceTypeId: string) => {
  const referenceType =
    referenceTypeStore.getReferenceTypeById(referenceTypeId);
  return referenceType ? referenceType.density : 0;
};

const getFormatDescription = (formatId: string) => {
  const format = referenceStore.referenceFormats?.find(
    (f) => f.id === formatId,
  );
  if (format) return format.description;
  else return "";
};

const getTypeDescription = (referenceTypeId: string) => {
  const referenceType =
    referenceTypeStore.getReferenceTypeById(referenceTypeId);
  return referenceType ? referenceType.description : "";
};

const getAreaName = (areaId: string) => {
  const area = plantModelStore.areas?.find((a) => a.id === areaId);

  if (area) return area.name;
  else return "";
};
</script>
<style scoped>
.references-header {
  display: grid;
  grid-template-columns: 3fr 0.1fr;
}
.references-filter {
  display: grid;
  grid-template-columns: 0.2fr 0.8fr;
  align-items: center;
  width: 25vw;
}
</style>
