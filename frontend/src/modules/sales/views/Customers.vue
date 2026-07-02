<template>
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
        <Table
          :columns="customerColumns"
          :items="filteredData"
          :filter-config="customerFilterConfig"
          v-model:filter-values="customerFilter"
          :filter-body-width="customerFilterBodyWidth"
          preset="crud-list"
          page="Customers"
          tableStyle="min-width: 100%"
          sort-field="comercialName"
          :sort-order="1"
          :scroll-height="customersScrollHeight"
          showDeleteColumn
          :canDelete="() => true"
          @clear="cleanCustomerFilter"
          @create="createCustomer"
          @delete="deleteCustomer"
          @row-click="editCustomer"
        />
      </TabPanel>
      <TabPanel value="1">
        <Table
          :columns="customerTypeColumns"
          :items="customerStore.customerTypes ?? []"
          :filter-config="[]"
          v-model:filter-values="emptyFilter"
          :filter-body-width="typesFilterBodyWidth"
          preset="crud-list"
          page="CustomerTypes"
          tableStyle="min-width: 100%"
          :scroll-height="typesScrollHeight"
          showDeleteColumn
          :canDelete="() => true"
          @create="createCustomerType"
          @delete="deleteCustomerType"
          @row-click="editCustomerType"
        />
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
import Table from "../../../components/tables/Table.vue";
import type { Column } from "../../../components/tables/types";
import { ColumnType } from "../../../components/tables/types";
import type { FilterConfig, FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
const selectedTabIndex = ref("0");
const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const customerStore = useCustomersStore();

const customersScrollHeight = "flex";
const typesScrollHeight = "flex";

const customerFilterBodyWidth: FilterBodyWidth = {
  desktop: "33%",
  tablet: "50%",
};

const typesFilterBodyWidth: FilterBodyWidth = {
  desktop: "25%",
  tablet: "50%",
};

const customerFilterConfig: FilterConfig[] = [
  {
    key: "code",
    label: "Nom comercial",
    type: "text",
    placeholder: "Nom comercial",
    size: "md",
  },
];

const customerColumns = ref<Column[]>([
  { field: "comercialName", header: "Nom comercial", sortable: true, style: "width: 20%" },
  { field: "taxName", header: "Nom Fiscal", style: "width: 20%" },
  { field: "vatNumber", header: "CIF", style: "width: 20%" },
  {
    field: "customerTypeId",
    header: "Tipus",
    columnType: ColumnType.Lookup,
    resolver: customerStore.getCustomerTypeNameById,
    style: "width: 20%",
  },
  { field: "disabled", header: "Desactivat", sortable: true, columnType: ColumnType.Boolean, style: "width: 20%" },
]);

const customerTypeColumns = ref<Column[]>([
  { field: "name", header: "Nom", style: "width: 33%" },
  { field: "description", header: "Descripció", style: "width: 33%" },
  { field: "disabled", header: "Desactivat", columnType: ColumnType.Boolean, style: "width: 33%" },
]);

const customerFilter = ref({
  code: "",
});

const emptyFilter = ref({});

const filteredData = computed(() => {
  if (!customerStore.customers) return [];

  if (customerFilter.value.code.length > 0) {
    return customerStore.customers.filter((r: Customer) =>
      r.comercialName.toLowerCase().includes(customerFilter.value.code.toLowerCase()),
    );
  } else {
    return customerStore.customers;
  }
});

const cleanCustomerFilter = () => {
  customerFilter.value.code = "";
};

const createCustomer = () => {
  router.push({ path: `/customers/${uuidv4()}` });
};

const createCustomerType = () => {
  router.push({ path: `/customer-types/${uuidv4()}` });
};

onMounted(async () => {
  await customerStore.fetchCustomers();
  await customerStore.fetchCustomerTypes();

  store.setMenuItem({
    title: "Clients",
    icon: PrimeIcons.HASHTAG,
  });
});

const deleteCustomer = (customer: Customer) => {
  confirm.require({
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

const deleteCustomerType = (customerType: CustomerType) => {
  confirm.require({
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
  router.push({ path: `/customers/${row.data.id}` });
};

const editCustomerType = (row: DataTableRowClickEvent) => {
  router.push({ path: `/customer-types/${row.data.id}` });
};
</script>