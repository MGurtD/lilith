<template>
  <Dialog
    v-model:visible="dialogVisible"
    :header="title"
    :closable="true"
    :modal="true"
  >
    <FormSupplierReference
      v-if="selectedReference"
      :supplierId="supplierId"
      :referenceId="referenceId"
      :referenceSupplier="selectedReference"
      @submit="submitForm"
    />
  </Dialog>

  <div>
    <DataTable
      v-if="supplierReferences"
      :value="supplierReferences"
      tableStyle="min-width: 100%"
      :scroll-height="'calc(100vh - 260px)'"
      sortField="supplierCode"
      :sortOrder="1"
      @row-click="rowContactClick"
    >
      <template #header>
        <slot name="header">
          <div
            class="flex flex-wrap align-items-center justify-content-between gap-2"
          >
            <span class="text-l text-900 font-bold">{{ title }}</span>
            <div>
              <Button
                v-if="formActionMode === FormActionMode.EDIT"
                :icon="PrimeIcons.PLUS"
                rounded
                @click="createButtonClick"
              />
            </div>
          </div>
        </slot>
      </template>
      <Column
        v-if="supplierId"
        header="Referència"
        field="referenceId"
        style="width: 25%"
      >
        <template #body="slotProps">
          {{ referenceStore.getFullNameById(slotProps.data.referenceId) }}
        </template>
      </Column>
      <Column
        v-if="referenceId"
        header="Proveïdor"
        field="supplierId"
        style="width: 25%"
        sortable
      >
        <template #body="slotProps">
          {{ supplierStore.getName(slotProps.data.supplierId) }}
        </template>
      </Column>
      <Column
        header="Codi proveïdor"
        field="supplierCode"
        style="width: 20%"
        sortable
      >
      </Column>
      <Column
        header="Descripció"
        field="supplierDescription"
        style="width: 25%"
        sortable
      ></Column>
      <Column header="Preu" field="supplierPrice" style="width: 20%" sortable>
        <template #body="slotProps">
          {{ formatCurrency(slotProps.data.supplierPrice) }}
        </template>
      </Column>
      <Column
        header="Dies submin."
        field="supplyDays"
        style="width: 20%"
        sortable
      ></Column>
      <Column>
        <template #body="slotProps">
          <i
            :class="PrimeIcons.TIMES"
            class="grid_delete_column_button"
            @click="deleteReferences($event, slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { v4 as uuidv4 } from "uuid";
import { useSuppliersStore } from "../store/suppliers";
import FormSupplierReference from "./FormSupplierReference.vue";
import { SupplierReference } from "../types";
import { formatCurrency } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { FormActionMode } from "../../../types/component";
import { useToast } from "primevue/usetoast";
import { useReferenceStore } from "../../shared/store/reference";

const toast = useToast();
const confirm = useConfirm();
const referenceStore = useReferenceStore();
const supplierStore = useSuppliersStore();
const formMode = ref(FormActionMode.CREATE);

const props = defineProps<{
  title: string;
  formActionMode: FormActionMode;
  referenceId?: string;
  supplierId?: string;
  supplierReferences: SupplierReference[];
}>();

const emit = defineEmits<{
  (e: "create", references: SupplierReference): void;
  (e: "update", references: SupplierReference): void;
  (e: "delete", references: SupplierReference): void;
}>();

const selectedReference = ref(undefined as SupplierReference | undefined);
const dialogVisible = ref(false);

const createButtonClick = () => {
  selectedReference.value = {
    id: uuidv4(),
    disabled: false,
    supplierId: props.supplierId ? props.supplierId : "",
    referenceId: props.referenceId ? props.referenceId : "",
    supplierCode: "",
    supplierDescription: "",
    supplierPrice: 0,
    supplyDays: 0,
  } as SupplierReference;
  formMode.value = FormActionMode.CREATE;
  dialogVisible.value = true;
};

const rowContactClick = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    selectedReference.value = row.data;
    formMode.value = FormActionMode.EDIT;
    dialogVisible.value = true;
  }
};

const submitForm = () => {
  const reference = selectedReference.value as SupplierReference;
  if (formMode.value === FormActionMode.CREATE) {
    var exists = supplierStore.supplierReferences?.find(
      (r) => r.referenceId === reference.referenceId
    );
    if (exists) {
      toast.add({
        severity: "warn",
        summary: "Creació de referència",
        detail: "La referència ja existeix",
      });
      return;
    }

    emit("create", reference);
  } else {
    emit("update", reference);
  }

  dialogVisible.value = false;
  selectedReference.value = undefined;
};

const deleteReferences = (event: any, contact: SupplierReference) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la referència?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", contact);
    },
  });
};
</script>
