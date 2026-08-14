<template>
  <Tabs value="0">
    <TabList>
      <Tab value="0">
        <i :class="PrimeIcons.WALLET" class="mr-2"></i>
        <span>{{ t("sales.customers.title") }}</span>
      </Tab>
      <Tab value="1" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.USERS" class="mr-2"></i>
        <span>{{ t("sales.customers.contacts") }}</span>
      </Tab>
      <Tab value="2" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.ENVELOPE" class="mr-2"></i>
        <span>{{ t("sales.customers.addresses") }}</span>
      </Tab>
      <Tab value="3" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.CHART_PIE" class="mr-2"></i>
        <span>{{ t("sales.customers.statistics") }}</span>
      </Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <FormCustomer @submit="submitForm" />
      </TabPanel>
      <TabPanel value="1" v-if="formMode === FormActionMode.EDIT">
        <CustomerContacts
          @create="addContact"
          @update="editContact"
          @delete="removeContact"
        />
      </TabPanel>
      <TabPanel value="2" v-if="formMode === FormActionMode.EDIT">
        <CustomerAddresses
          @create="addAddress"
          @update="editAddress"
          @delete="removeAddress"
        />
      </TabPanel>
      <TabPanel value="3" v-if="formMode === FormActionMode.EDIT">
        <FormCustomerStatistics
          :budgets="budgets ?? []"
          :salesInvoices="invoices ?? []"
        />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { Customer, CustomerAddress, CustomerContact } from "../types";
import { storeToRefs } from "pinia";
import { useStore } from "../../../store";
import { useCustomersStore } from "../store/customers";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import FormCustomer from "../components/FormCustomer.vue";
import CustomerAddresses from "../components/TableCustomerAddresses.vue";
import CustomerContacts from "../components/TableCustomerContacts.vue";
import FormCustomerStatistics from "../components/FormCustomerStatistic.vue";
import { useSharedDataStore } from "../../shared/store/masterData";
import { useBudgetStore } from "../store/budget";
import { formatDateForQueryParameter } from "../../../utils/functions";
import { useSalesInvoiceStore } from "../store/invoice";
import { useI18n } from "vue-i18n";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const sharedData = useSharedDataStore();
const customerStore = useCustomersStore();
const budgetStore = useBudgetStore();
const salesInvoiceStore = useSalesInvoiceStore();
const { customer } = storeToRefs(customerStore);
const { budgets } = storeToRefs(budgetStore);
const { invoices } = storeToRefs(salesInvoiceStore);
const { t } = useI18n();

const loadView = async () => {
  await customerStore.fetchCustomer(route.params.id as string);
  customerStore.fetchCustomerTypes();
  sharedData.fetchMasterData();

  const now = new Date();
  const starttime = new Date(now.getFullYear(), 0, 1); // 1 gener 00:00
  const endtime = new Date(now.getFullYear(), 11, 31, 23, 59, 59, 999); // 31 desembre 23:59:59.999
  if (route.params.id) {
    await budgetStore.GetFiltered(
      formatDateForQueryParameter(starttime),
      formatDateForQueryParameter(endtime),
      route.params.id as string,
    );

    await salesInvoiceStore.GetFiltered(
      formatDateForQueryParameter(starttime),
      formatDateForQueryParameter(endtime),
      "",
      route.params.id as string,
    );
  }

  // Comprovar existencia del proveïdor
  let pageTitle = "";
  if (!customer.value) {
    formMode.value = FormActionMode.CREATE;
    customerStore.setNewCustomer(route.params.id as string);
    pageTitle = t("sales.customers.createCustomer");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `${t("sales.customers.title")} ${customer.value.comercialName}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    title: pageTitle,
    backButtonVisible: true,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = customer.value as Customer;
  let result;
  let successMessage = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await customerStore.createCustomer(data);
    successMessage = t("sales.customers.customerCreated");
  } else {
    result = await customerStore.updateCustomer(data.id, data);
    successMessage = t("sales.customers.customerUpdated");
  }

  if (result.result) {
    toast.add({
      severity: "success",
      summary: successMessage,
      life: 5000,
    });
    await loadView();
  } else {
    toast.add({
      severity: "error",
      summary: t("sales.detail.messages.error"),
      detail: result.errors?.[0] ?? t("sales.customers.customerSaveError"),
      life: 6000,
    });
  }
};

const addContact = async (contact: CustomerContact) => {
  const result = await customerStore.addContactToCustomer(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("sales.customers.contactAdded"),
      life: 5000,
    });
  }
};

const editContact = async (contact: CustomerContact) => {
  const result = await customerStore.updateContactFromCustomer(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("sales.customers.contactUpdated"),
      life: 5000,
    });
  }
};

const removeContact = async (contact: CustomerContact) => {
  const result = await customerStore.removeContactFromCustomer(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("sales.customers.contactDeleted"),
      life: 5000,
    });
  }
};

const addAddress = async (address: CustomerAddress) => {
  const result = await customerStore.addAddressToCustomer(address);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("sales.customers.addressAdded"),
      life: 5000,
    });
  }
};

const editAddress = async (address: CustomerAddress) => {
  const result = await customerStore.updateAddressFromCustomer(address);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("sales.customers.addressUpdated"),
      life: 5000,
    });
  }
};

const removeAddress = async (address: CustomerAddress) => {
  const result = await customerStore.removeAddressFromCustomer(address);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("sales.customers.addressDeleted"),
      life: 5000,
    });
  }
};
</script>
