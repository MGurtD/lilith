<template>
  <Button
    :icon="PrimeIcons.PLUS"
    class="grid_add_row_button"
    rounded
    @click="createButtonClick"
  />

  <Tabs v-model:value="selectedTabIndex">
    <TabList>
      <Tab value="0">
        <i :class="PrimeIcons.LINK" class="mr-2"></i>
        <span>Clients</span>
      </Tab>
      <Tab value="1">
        <i :class="PrimeIcons.HASHTAG" class="mr-2"></i>
        <span>Tipus de client</span>
      </Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <DataTable
          :value="filteredData"
          tableStyle="min-width: 100%"
          sort-field="comercialName"
          :sort-order="1"
          scrollable
          scrollHeight="flex"
          paginator
          :rows="20"
          @row-click="editCustomer"
        >
          <template #header>
            <div class="references-header">
              <div class="references-filter">
                <label>Nom comercial</label>
                <BaseInput v-model="filter.code" />
              </div>
            </div>
          </template>
          <Column
            field="comercialName"
            header="Nom comercial"
            sortable
            style="width: 20%"
          ></Column>
          <Column
            field="taxName"
            header="Nom Fiscal"
            style="width: 20%"
          ></Column>
          <Column field="vatNumber" header="CIF" style="width: 20%"></Column>
          <Column header="Tipus" style="width: 20%">
            <template #body="slotProps">
              <span>{{
                getCustomerTypeName(slotProps.data.customerTypeId)
              }}</span>
            </template>
          </Column>
          <Column header="Desactivat" sortable style="width: 20%">
            <template #body="slotProps">
              <BooleanColumn
                :value="slotProps.data.disabled"
                :showColor="false"
              />
            </template>
          </Column>
          <Column>
            <template #body="slotProps">
              <i
                :class="PrimeIcons.TIMES"
                class="grid_delete_column_button"
                @click="deleteCustomer($event, slotProps.data)"
              />
            </template>
          </Column>
        </DataTable>
      </TabPanel>
      <TabPanel value="1">
        <DataTable
          :value="customerStore.customerTypes"
          tableStyle="min-width: 100%"
          scrollable
          scrollHeight="flex"
          @row-click="editCustomerType"
        >
          <Column field="name" header="Nom" style="width: 33%"></Column>
          <Column
            field="description"
            header="Descripció"
            style="width: 33%"
          ></Column>
          <Column header="Desactivat" style="width: 33%">
            <template #body="slotProps">
              <BooleanColumn :value="slotProps.data.disabled" />
            </template>
          </Column>
          <Column>
            <template #body="slotProps">
              <i
                :class="PrimeIcons.TIMES"
                class="grid_delete_column_button"
                @click="deleteCustomerType($event, slotProps.data)"
              />
            </template>
          </Column>
        </DataTable>
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useCustomersStore } from "../store/customers";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Customer, CustomerType } from "../types";
import { useStore } from "../../../store";

const selectedTabIndex = ref("0");
const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const customerStore = useCustomersStore();

const filter = ref({
  code: "",
});

const filteredData = computed(() => {
  if (!customerStore.customers) return [];

  if (filter.value.code.length > 0) {
    return customerStore.customers.filter((r: Customer) =>
      r.comercialName.toLowerCase().includes(filter.value.code.toLowerCase()),
    );
  } else {
    return customerStore.customers;
  }
});

onMounted(async () => {
  await customerStore.fetchCustomers();
  await customerStore.fetchCustomerTypes();

  store.setMenuItem({
    title: "Clients",
    icon: PrimeIcons.HASHTAG,
  });
});

const getCustomerTypeName = (id: string) => {
  const customerType = customerStore.customerTypes?.find((st) => st.id === id);
  if (customerType) {
    return customerType.name;
  }
};

const deleteCustomer = (event: any, customer: Customer) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el client ${customer.comercialName}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await customerStore.deleteCustomer(customer.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await customerStore.fetchCustomers();
      }
    },
  });
};

const deleteCustomerType = (event: any, customerType: CustomerType) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el tipus de client ${customerType.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await customerStore.deleteCustomerType(customerType.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await customerStore.fetchCustomerTypes();
      }
    },
  });
};

const editCustomer = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/customers/${row.data.id}` });
  }
};

const editCustomerType = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/customer-types/${row.data.id}` });
  }
};

const createButtonClick = () => {
  if (selectedTabIndex.value === "0") {
    router.push({ path: `/customers/${uuidv4()}` });
  } else {
    router.push({ path: `/customer-types/${uuidv4()}` });
  }
};
</script>
<style scoped>
.references-header {
  display: grid;
  grid-template-columns: 3fr 0.1fr;
}
.references-filter {
  display: grid;
  grid-template-columns: 0.3fr 0.7fr;
  align-items: center;
  width: 25vw;
}
</style>
