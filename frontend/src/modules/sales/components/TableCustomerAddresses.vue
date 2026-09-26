<template>
  <FormCustomerAddress
    v-if="selectedAddress"
    :address="selectedAddress"
    @submit="submitForm"
    @cancel="() => (selectedAddress = undefined)"
  />
  <Table
    v-else-if="customer?.address"
    :items="customer.address"
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
    @delete="deleteAddress"
    @row-click="rowClick"
  >
    <template #prepend>
      <span class="text-900 font-bold">{{ t("sales.components.adreces") }}</span>
    </template>
  </Table>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { computed, ref } from "vue";
import Table from "@/components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "@/components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import FormCustomerAddress from "./FormCustomerAddress.vue";
import { CustomerAddress } from "../types";
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
  (e: "create", contact: CustomerAddress): void;
  (e: "update", contact: CustomerAddress): void;
  (e: "delete", contact: CustomerAddress): void;
}>();

const selectedAddress = ref(undefined as CustomerAddress | undefined);

const noFilters = {};

const columns = computed<Column[]>(() => [
  { field: "name", header: t("sales.components.nom"), style: "width: 25%" },
  { field: "region", header: t("sales.components.provincia"), style: "width: 25%" },
  { field: "city", header: t("sales.components.municipi"), style: "width: 25%" },
  { field: "postalCode", header: t("sales.components.codiPostal"), style: "width: 25%" },
  {
    field: "main",
    header: t("sales.components.principal"),
    columnType: ColumnType.Boolean,
    showColor: false,
  },
]);

// Phone card: the default for this table.
const cardLayout: CardLayout = {
  title: "name",
  subtitle: "city",
  meta: ["region", "postalCode", "main"],
};

const createButtonClick = () => {
  selectedAddress.value = {
    customerId: customer.value?.id,
    id: getNewUuid(),
    name: "",
    country: "",
    region: "",
    city: "",
    postalCode: "",
    address: "",
    main: false,
    disabled: false,
    observations: "",
    latitude: 0,
    longitude: 0,
  } as CustomerAddress;
  formMode.value = FormActionMode.CREATE;
};

const rowClick = (row: DataTableRowClickEvent) => {
  selectedAddress.value = { ...(row.data as CustomerAddress) };
  formMode.value = FormActionMode.EDIT;
};

const submitForm = (address: CustomerAddress) => {
  if (formMode.value === FormActionMode.CREATE) {
    emit("create", address);
  } else {
    emit("update", address);
  }
  // Amb aquesta assignació es torna a pintar la grid
  selectedAddress.value = undefined;
};

const deleteAddress = (contact: CustomerAddress) => {
  confirm.require({
    message: t("sales.componentMessages.deleteAddress"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", contact);
    },
  });
};
</script>
