<template>
  <Button
    v-if="!selectedContact"
    :icon="PrimeIcons.PLUS"
    class="grid_add_row_button"
    rounded
    @click="createButtonClick"
  />

  <SupplierContactForm
    v-if="selectedContact"
    :contact="selectedContact"
    @submit="submitForm"
    @cancel="() => (selectedContact = undefined)"
  />
  <div v-else>
    <DataTable
      v-if="supplier?.contacts"
      :value="supplier.contacts"
      tableStyle="min-width: 100%"
      @row-click="rowContactClick"
    >
      <Column header="Nom" style="width: 25%">
        <template #body="slotProps">
          {{ slotProps.data.firstName }} {{ slotProps.data.lastName }}
        </template>
      </Column>
      <Column header="Càrrec" field="charge" style="width: 25%"></Column>
      <Column header="Correu" field="email" style="width: 25%"></Column>
      <Column header="Telèfon" field="phone" style="width: 25%"></Column>
      <Column>
        <template #body="slotProps">
          <i
            :class="PrimeIcons.TIMES"
            class="grid_delete_column_button"
            @click="deleteContact($event, slotProps.data)"
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
import SupplierContactForm from "./FormSupplierContact.vue";
import { SupplierContact } from "../types";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { FormActionMode } from "../../../types/component";

const confirm = useConfirm();
const supplierStore = useSuppliersStore();
const { supplier } = storeToRefs(supplierStore);
const formMode = ref(FormActionMode.CREATE);

const emit = defineEmits<{
  (e: "create", contact: SupplierContact): void;
  (e: "update", contact: SupplierContact): void;
  (e: "delete", contact: SupplierContact): void;
}>();

const selectedContact = ref(undefined as SupplierContact | undefined);

const createButtonClick = () => {
  selectedContact.value = {
    supplierId: supplier.value?.id,
    id: uuidv4(),
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    phoneExtension: "",
    charge: "",
    disabled: false,
    default: false,
    observations: "",
  } as SupplierContact;
  formMode.value = FormActionMode.CREATE;
};

const rowContactClick = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    selectedContact.value = row.data;
    formMode.value = FormActionMode.EDIT;
  }
};

const submitForm = () => {
  const contact = selectedContact.value as SupplierContact;
  if (formMode.value === FormActionMode.CREATE) {
    emit("create", contact);
  } else {
    emit("update", contact);
  }
  // Amb aquesta assignació es torna a pintar la grid
  selectedContact.value = undefined;
};

const deleteContact = (event: any, contact: SupplierContact) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el contacte?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", contact);
    },
  });
};
</script>
