<template>
  <main>
    <DataTable
      @row-click="onEditRow"
      :value="filteredLocations"
      tableStyle="min-width: 100%"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-900 font-bold">{{ t("warehouse.locations.title") }}</span>
          <div class="flex align-items-center gap-2">
            <Select
              v-model="selectedTypeFilter"
              :options="typeFilterOptions"
              optionLabel="label"
              optionValue="value"
              :placeholder="t('warehouse.placeholders.allLocationTypes')"
              style="width: 14rem"
            />
            <Button :icon="PrimeIcons.PLUS" rounded raised @click="onAddClick" />
          </div>
        </div>
      </template>
      <Column field="name" :header="t('warehouse.fields.name')" style="width: 20%"></Column>
      <Column
        field="description"
        :header="t('common.description')"
        style="width: 40%"
      ></Column>
      <Column :header="t('common.type')" style="width: 15%">
        <template #body="slotProps">
          {{ getLocationTypeLabel(slotProps.data.locationType, t) }}
        </template>
      </Column>
      <Column :header="t('warehouse.fields.disabled')" style="width: 10%">
        <template #body="slotProps">
          <BooleanColumn :value="slotProps.data.disabled"></BooleanColumn>
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
  </main>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    @hide="dialogClosed"
  >
    <FormLocation
      v-if="selectedLocation"
      :location="selectedLocation"
      @submit="onLocationSubmit"
    ></FormLocation>
  </Dialog>
</template>

<script setup lang="ts">
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import {
  Warehouse,
  Location,
  getLocationTypeOptions,
  getLocationTypeLabel,
} from "../types";
import { getNewUuid } from "../../../utils/functions";
import { useConfirm } from "primevue/useconfirm";
import { computed, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";
import { DialogOptions, FormActionMode } from "../../../types/component";
import FormLocation from "./FormLocation.vue";

const props = defineProps<{
  warehouse: Warehouse;
  locations: Array<Location>;
}>();
const { t } = useI18n();

const emit = defineEmits<{
  (e: "add", location: Location): void;
  (e: "edit", location: Location): void;
  (e: "delete", location: Location): void;
}>();

// ── Filtre per tipus ─────────────────────────────────────────────────────────
const selectedTypeFilter = ref<string | null>(null);

interface TypeFilterOption {
  value: string | null;
  label: string;
}

const typeFilterOptions = computed<TypeFilterOption[]>(() => [
  { value: null, label: t("warehouse.actions.all") },
  ...getLocationTypeOptions(t),
]);

const filteredLocations = computed(() => {
  if (!selectedTypeFilter.value) return props.locations;
  return props.locations.filter(
    (l) => l.locationType === selectedTypeFilter.value
  );
});

// ── Diàleg ───────────────────────────────────────────────────────────────────
const dialogOptions = reactive({
  visible: false,
  title: "",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);
const confirm = useConfirm();

const selectedLocation = ref(undefined as Location | undefined);
const formAction = ref(FormActionMode.CREATE);

const onAddClick = () => {
  openDialog(FormActionMode.CREATE, {
    id: getNewUuid(),
    warehouseId: props.warehouse.id,
    disabled: false,
    name: "",
    description: "",
  } as Location);
};
const onEditRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    openDialog(FormActionMode.EDIT, row.data);
  }
};
const openDialog = (action: FormActionMode, location: Location) => {
  formAction.value = action;
  selectedLocation.value = location;
  dialogOptions.visible = true;
  dialogOptions.title =
    action === FormActionMode.CREATE
      ? t("warehouse.locations.createTitle")
      : t("warehouse.locations.updateTitle");
};
const onLocationSubmit = (location: Location) => {
  if (formAction.value === FormActionMode.CREATE) {
    emit("add", location);
  } else if (formAction.value === FormActionMode.EDIT) {
    emit("edit", location);
  }
  dialogOptions.visible = false;
};

const dialogClosed = () => {
  selectedLocation.value = undefined;
};

const onDeleteRow = (event: any, location: Location) => {
  confirm.require({
    target: event.currentTarget,
    message: t("warehouse.messages.confirmDeleteLocation", { name: location.name }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", location);
    },
  });
};
</script>
