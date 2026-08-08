<template>
  <Tabs value="0">
    <TabList>
      <Tab value="0">
        <i :class="PrimeIcons.BUILDING" class="mr-2"></i>
        <span>{{ t("purchase.supplier.tabs.supplier") }}</span>
      </Tab>
      <Tab value="1" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.TAG" class="mr-2"></i>
        <span>{{ t("purchase.supplier.tabs.references") }}</span>
      </Tab>
      <Tab value="2" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.USERS" class="mr-2"></i>
        <span>{{ t("purchase.supplier.tabs.contacts") }}</span>
      </Tab>
      <Tab value="3" v-if="formMode === FormActionMode.EDIT && isLogisticSupplier">
        <i :class="PrimeIcons.TRUCK" class="mr-2"></i>
        <span>{{ t("purchase.supplier.tabs.transportRates") }}</span>
      </Tab>
      <Tab value="4" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.MONEY_BILL" class="mr-2"></i>
        <span>{{ t("purchase.supplier.tabs.purchaseRates") }}</span>
      </Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <FormSupplier v-if="supplier" :supplier="supplier" @submit="submitForm" />
      </TabPanel>
      <TabPanel value="1" v-if="formMode === FormActionMode.EDIT">
        <TableSupplierReferences
          v-if="supplier && supplierStore.supplierReferences"
          :title="t('purchase.supplier.tabs.references')"
          :formActionMode="formMode"
          :supplier-id="supplier.id"
          :supplier-references="supplierStore.supplierReferences"
          @create="addReference"
          @update="editReference"
          @delete="removeReference"
        />
      </TabPanel>
      <TabPanel value="2" v-if="formMode === FormActionMode.EDIT">
        <TableSupplierContacts
          :title="t('purchase.supplier.tabs.contacts')"
          :formActionMode="formMode"
          @create="addContact"
          @update="editContact"
          @delete="removeContact"
        />
      </TabPanel>
      <TabPanel value="3" v-if="formMode === FormActionMode.EDIT && isLogisticSupplier">
        <TableTransportRates
          v-if="supplier"
          :supplierId="supplier.id"
        />
      </TabPanel>
      <TabPanel value="4" v-if="formMode === FormActionMode.EDIT">
        <TablePurchaseRates
          v-if="supplier"
          :supplierId="supplier.id"
        />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { useSuppliersStore } from "../store/suppliers";
import { PrimeIcons } from "@primevue/core/api";

import FormSupplier from "../components/FormSupplier.vue";
import { storeToRefs } from "pinia";
import { Supplier, SupplierContact, SupplierReference } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import TableSupplierContacts from "../components/TableSupplierContacts.vue";
import { useReferenceStore } from "../../shared/store/reference";
import TableSupplierReferences from "../components/TableSupplierReferences.vue";
import TableTransportRates from "../components/TableTransportRates.vue";
import TablePurchaseRates from "../components/TablePurchaseRates.vue";
import { useTransportRateStore } from "../store/transportRate";
import { usePurchaseRateStore } from "../store/purchaseRate";
import { useI18n } from "vue-i18n";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const supplierStore = useSuppliersStore();
const referenceStore = useReferenceStore();
const transportRateStore = useTransportRateStore();
const purchaseRateStore = usePurchaseRateStore();
const { t } = useI18n();
const { supplier } = storeToRefs(supplierStore);

const isLogisticSupplier = computed(() => {
  if (!supplier.value || !supplierStore.supplierTypes) return false;
  const type = supplierStore.supplierTypes.find(
    (t) => t.id === supplier.value!.supplierTypeId
  );
  return type?.name === "Logistica";
});

const loadView = async () => {
  const supplierId = route.params.id as string;

  await supplierStore.fetchSupplier(supplierId);
  supplierStore.fetchSupplierTypes();
  supplierStore.fetchSupplierReferences(supplierId);
  referenceStore.fetchReferencesByModule("purchase");

  // Comprovar existencia del proveïdor
  let pageTitle = "";
  if (!supplier.value) {
    formMode.value = FormActionMode.CREATE;
    supplierStore.setNewSupplier(supplierId);
    pageTitle = t("purchase.supplier.createTitle");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = t("purchase.supplier.detailTitle", {
      name: supplier.value.comercialName,
    });
    // Carregar tarifes
    if (isLogisticSupplier.value) {
       await transportRateStore.fetchTransportRatesBySupplierId(supplierId);
    }
    await purchaseRateStore.fetchPurchaseRatesBySupplierId(supplierId);
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
  const data = supplier.value as Supplier;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await supplierStore.createSupplier(data);
    message = t("purchase.supplier.messages.created");
  } else {
    result = await supplierStore.updateSupplier(data.id, data);
    message = t("purchase.supplier.messages.updated");
  }

  if (result) {
    toast.add({
      severity: "success",
      summary: message,
      life: 5000,
    });
    await loadView();
  }
};

const addContact = async (contact: SupplierContact) => {
  const result = await supplierStore.addContactToSupplier(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.supplier.messages.contactAdded"),
      life: 5000,
    });
  }
};

const editContact = async (contact: SupplierContact) => {
  const result = await supplierStore.updateContactFromSupplier(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.supplier.messages.contactUpdated"),
      life: 5000,
    });
  }
};

const removeContact = async (contact: SupplierContact) => {
  const result = await supplierStore.removeContactFromSupplier(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.supplier.messages.contactDeleted"),
      life: 5000,
    });
  }
};

const addReference = async (Reference: SupplierReference) => {
  const result = await supplierStore.addReferenceToSupplier(Reference);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.supplier.messages.referenceAdded"),
      life: 5000,
    });
  }
};

const editReference = async (Reference: SupplierReference) => {
  const result = await supplierStore.updateReferenceFromSupplier(Reference);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.supplier.messages.referenceUpdated"),
      life: 5000,
    });
  }
};

const removeReference = async (Reference: SupplierReference) => {
  const result = await supplierStore.removeReferenceFromSupplier(Reference);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.supplier.messages.referenceDeleted"),
      life: 5000,
    });
  }
};
</script>
