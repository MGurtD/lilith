<template>
  <Button
    v-if="!selectedAddress"
    :icon="PrimeIcons.PLUS"
    class="grid_add_row_button"
    rounded
    @click="createButtonClick"
  />

  <FormCustomerAddress
    v-if="selectedAddress"
    :address="selectedAddress"
    @submit="submitForm"
    @cancel="() => (selectedAddress = undefined)"
  />
  <div v-else>
    <DataTable
      v-if="customer?.address"
      :value="customer.address"
      tableStyle="min-width: 100%"
      @row-click="rowClick"
    >
      <Column header="Nom" field="name" style="width: 25%"></Column>
      <Column header="Província" field="region" style="width: 25%"></Column>
      <Column header="Municipi" field="city" style="width: 25%"></Column>
      <Column
        header="Codi Postal"
        field="postalCode"
        style="width: 25%"
      ></Column>
      <Column header="Principal">
        <template #body="slotProps">
          <BooleanColumn :value="slotProps.data.main" :show-color="false" />
        </template>
      </Column>
      <Column>
        <template #body="slotProps">
          <i
            :class="PrimeIcons.TIMES"
            class="grid_delete_column_button"
            @click="deleteAddress($event, slotProps.data)"
          />
        </template>
      </Column>
    </DataTable>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { v4 as uuidv4 } from "uuid";
import FormCustomerAddress from "./FormCustomerAddress.vue";
import { CustomerAddress } from "../types";
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
  (e: "create", contact: CustomerAddress): void;
  (e: "update", contact: CustomerAddress): void;
  (e: "delete", contact: CustomerAddress): void;
}>();

const selectedAddress = ref(undefined as CustomerAddress | undefined);

const createButtonClick = () => {
  selectedAddress.value = {
    customerId: customer.value?.id,
    id: uuidv4(),
    name: "",
    country: "",
    region: "",
    city: "",
    postalCode: "",
    address: "",
    main: false,
    disabled: false,
    observations: "",
  } as CustomerAddress;
  formMode.value = FormActionMode.CREATE;
};

const rowClick = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button"
    )
  ) {
    selectedAddress.value = row.data;
    formMode.value = FormActionMode.EDIT;
  }
};

const submitForm = () => {
  const contact = selectedAddress.value as CustomerAddress;
  if (formMode.value === FormActionMode.CREATE) {
    emit("create", contact);
  } else {
    emit("update", contact);
  }
  // Amb aquesta assignació es torna a pintar la grid
  selectedAddress.value = undefined;
};

const deleteAddress = (event: any, contact: CustomerAddress) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar l'adreça?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: () => {
      emit("delete", contact);
    },
  });
};
</script>
