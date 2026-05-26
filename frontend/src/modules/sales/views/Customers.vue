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
        <div ref="customersTableRef">
          <DataTable
            :value="filteredData"
            tableStyle="min-width: 100%"
            sort-field="comercialName"
            :sort-order="1"
            scrollable
            :scrollHeight="customersScrollHeight"
            paginator
            :rows="20"
            @row-click="editCustomer"
          >
            <template #header>
              <TableFilter
                :config="customerFilterConfig"
                v-model="filter"
                :show-title="false"
                :show-action-labels="false"
                :show-filter-action="false"
                :body-width="customerFilterBodyWidth"
                embedded
                @clear="cleanFilter"
                @create="createCustomer"
              />
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
        </div>
      </TabPanel>
      <TabPanel value="1">
        <div ref="typesTableRef">
          <DataTable
            :value="customerStore.customerTypes"
            tableStyle="min-width: 100%"
            scrollable
            :scrollHeight="typesScrollHeight"
            @row-click="editCustomerType"
          >
            <template #header>
              <TableFilter
                :config="[]"
                v-model="emptyFilter"
                :show-title="false"
                :show-action-labels="false"
                :show-filter-action="false"
                :show-create="true"
                embedded
                @create="createCustomerType"
              />
            </template>
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
        </div>
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
import { useScrollHeight } from "@/composables/useScrollHeight";
import TableFilter from "../../../components/tables/TableFilter.vue";
import type {
  FilterConfig,
  FilterBodyWidth,
} from "../../../components/tables/TableFilter.vue";

const selectedTabIndex = ref("0");
const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const customerStore = useCustomersStore();

const { tableRef: customersTableRef, scrollHeight: customersScrollHeight } =
  useScrollHeight(140);
const { tableRef: typesTableRef, scrollHeight: typesScrollHeight } =
  useScrollHeight();

const customerFilterBodyWidth: FilterBodyWidth = {
  desktop: "33%",
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

const filter = ref({
  code: "",
});

const emptyFilter = ref({});

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

const cleanFilter = () => {
  filter.value.code = "";
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
</script>
