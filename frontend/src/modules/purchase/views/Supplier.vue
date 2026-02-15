<template>
  <Tabs value="0">
    <TabList>
      <Tab value="0">
        <i :class="PrimeIcons.BUILDING" class="mr-2"></i>
        <span>Proveïdor</span>
      </Tab>
      <Tab value="1" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.TAG" class="mr-2"></i>
        <span>Referencies</span>
      </Tab>
      <Tab value="2" v-if="formMode === FormActionMode.EDIT">
        <i :class="PrimeIcons.USERS" class="mr-2"></i>
        <span>Contactes</span>
      </Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <FormSupplier @submit="submitForm" />
      </TabPanel>
      <TabPanel value="1" v-if="formMode === FormActionMode.EDIT">
        <TableSupplierReferences
          v-if="supplier && supplierStore.supplierReferences"
          title="Referències"
          :formActionMode="formMode"
          :supplier-id="supplier.id"
          :supplier-references="supplierStore.supplierReferences"
          @create="addReference"
          @update="editReference"
          @delete="removeReference"
        />
      </TabPanel>
      <TabPanel value="2" v-if="formMode === FormActionMode.EDIT">
        <SupplierContacts
          @create="addContact"
          @update="editContact"
          @delete="removeContact"
        />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { useSuppliersStore } from "../store/suppliers";
import { PrimeIcons } from "@primevue/core/api";

import FormSupplier from "../components/FormSupplier.vue";
import { storeToRefs } from "pinia";
import { Supplier, SupplierContact, SupplierReference } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import SupplierContacts from "../components/TableSupplierContacts.vue";
import { useReferenceStore } from "../../shared/store/reference";
import TableSupplierReferences from "../components/TableSupplierReferences.vue";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const supplierStore = useSuppliersStore();
const referenceStore = useReferenceStore();
const { supplier } = storeToRefs(supplierStore);

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
    pageTitle = "Alta de proveïdor";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Proveïdor ${supplier.value.comercialName}`;
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
    message = "Proveïdor creat correctament";
  } else {
    result = await supplierStore.updateSupplier(data.id, data);
    message = "Proveïdor actualizat correctament";
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
      summary: "Contacte afegit",
      life: 5000,
    });
  }
};

const editContact = async (contact: SupplierContact) => {
  const result = await supplierStore.updateContactFromSupplier(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Contacte actualizat",
      life: 5000,
    });
  }
};

const removeContact = async (contact: SupplierContact) => {
  const result = await supplierStore.removeContactFromSupplier(contact);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Contacte eliminat",
      life: 5000,
    });
  }
};

const addReference = async (Reference: SupplierReference) => {
  const result = await supplierStore.addReferenceToSupplier(Reference);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Referència afegida",
      life: 5000,
    });
  }
};

const editReference = async (Reference: SupplierReference) => {
  const result = await supplierStore.updateReferenceFromSupplier(Reference);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Referència actualizada",
      life: 5000,
    });
  }
};

const removeReference = async (Reference: SupplierReference) => {
  const result = await supplierStore.removeReferenceFromSupplier(Reference);
  if (result) {
    toast.add({
      severity: "success",
      summary: "Referència eliminada",
      life: 5000,
    });
  }
};
</script>
