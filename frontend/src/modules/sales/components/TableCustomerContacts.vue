<template>
  <CustomerContactForm
    v-if="selectedContact"
    :contact="selectedContact"
    @submit="submitForm"
    @cancel="() => (selectedContact = undefined)"
  />
  <Table
    v-else-if="customer?.contacts"
    :items="customer.contacts"
    :columns="columns"
    :filter-config="[]"
    :filter-values="noFilters"
    :show-filter-actions="false"
    :card-layout="cardLayout"
    phone-layout="cards"
    preset="read-only"
    tableStyle="min-width: 100%"
    show-delete-column
    @create="createButtonClick"
    @delete="deleteContact"
    @row-click="rowContactClick"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ t("sales.components.contactes") }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { computed, ref } from "vue";
import Table from "@/components/tables/Table.vue";
import type { CardLayout, Column } from "@/components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import CustomerContactForm from "./FormCustomerContact.vue";
import { CustomerContact } from "../types";
import { storeToRefs } from "pinia";
import { useConfirm } from "primevue/useconfirm";
import { DataTableRowClickEvent } from "primevue/datatable";
import { FormActionMode } from "../../../types/component";
import { useCustomersStore } from "../store/customers";

const { t } = useI18n();
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

const noFilters = {};

const columns = computed<Column[]>(() => [
  {
    field: "fullName",
    header: t("sales.components.nom"),
    resolver: (_value, row) => {
      const contact = row as CustomerContact;
      return [contact.firstName, contact.lastName].filter(Boolean).join(" ");
    },
    style: "width: 25%",
  },
  { field: "charge", header: t("sales.components.carrec"), style: "width: 25%" },
  { field: "email", header: t("sales.components.correu"), style: "width: 25%" },
  { field: "extension", header: t("sales.components.ext"), style: "width: 5%" },
  { field: "phoneNumber", header: t("sales.components.telefon"), style: "width: 20%" },
]);

// Phone card: the default for this table.
const cardLayout: CardLayout = {
  title: "fullName",
  subtitle: "charge",
  meta: ["email", "phoneNumber", "extension"],
};

const createButtonClick = () => {
  selectedContact.value = {
    customerId: customer.value?.id,
    id: getNewUuid(),
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
  selectedContact.value = { ...(row.data as CustomerContact) };
  formMode.value = FormActionMode.EDIT;
};

const submitForm = (contact: CustomerContact) => {
  if (formMode.value === FormActionMode.CREATE) {
    emit("create", contact);
  } else {
    emit("update", contact);
  }
  // Amb aquesta assignació es torna a pintar la grid
  selectedContact.value = undefined;
};

const deleteContact = (contact: CustomerContact) => {
  confirm.require({
    message: t("sales.componentMessages.deleteContact"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", contact);
    },
  });
};
</script>
