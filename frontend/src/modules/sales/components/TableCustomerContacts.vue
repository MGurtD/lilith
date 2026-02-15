<template>
  <Button
    v-if="!selectedContact"
    :icon="PrimeIcons.PLUS"
    class="grid_add_row_button"
    rounded
    @click="createButtonClick"
  />

  <CustomerContactForm
    v-if="selectedContact"
    :contact="selectedContact"
    @submit="submitForm"
    @cancel="() => (selectedContact = undefined)"
  />
  <div v-else>
    <DataTable
      v-if="customer?.contacts"
      :value="customer.contacts"
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
      <Column header="Ext." field="extension" style="width: 5%"></Column>
      <Column header="Telèfon" field="phoneNumber" style="width: 20%"></Column>
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
import CustomerContactForm from "./FormCustomerContact.vue";
import { CustomerContact } from "../types";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { FormActionMode } from "../../../types/component";
import { useCustomersStore } from "../store/customers";

const confirm = useConfirm();
const customerStore = useCustomersStore();
const { customer } = storeToRefs(customerStore);
const formMode = ref(FormActionMode.CREATE);

const emit = defineEmits<{
  (e: "create", contact: CustomerContact): void;
  (e: "update", contact: CustomerContact): void;
  (e: "delete", contact: CustomerContact): void;
}>();

const selectedContact = ref(undefined as CustomerContact | undefined);

const createButtonClick = () => {
  selectedContact.value = {
    customerId: customer.value?.id,
    id: uuidv4(),
    charge: "",
    email: "",
    firstName: "",
    lastName: "",
    disabled: false,
    main: false,
    phoneNumber: "",
  } as CustomerContact;
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
  const contact = selectedContact.value as CustomerContact;
  if (formMode.value === FormActionMode.CREATE) {
    emit("create", contact);
  } else {
    emit("update", contact);
  }
  // Amb aquesta assignació es torna a pintar la grid
  selectedContact.value = undefined;
};

const deleteContact = (event: any, contact: CustomerContact) => {
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
