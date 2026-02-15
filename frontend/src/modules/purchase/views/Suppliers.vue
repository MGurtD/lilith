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
        <span>Proveïdors</span>
      </Tab>
      <Tab value="1">
        <i :class="PrimeIcons.HASHTAG" class="mr-2"></i>
        <span>Tipus de Proveïdor</span>
      </Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <DataTable
          :value="supplierStore.suppliers"
          tableStyle="min-width: 100%"
          scrollable
          scrollHeight="flex"
          @row-click="editSupplier"
        >
          <Column
            field="comercialName"
            header="Nom Comercial"
            style="width: 20%"
          ></Column>
          <Column
            field="taxName"
            header="Nom Fiscal"
            style="width: 20%"
          ></Column>
          <Column field="vatNumber" header="CIF" style="width: 20%"></Column>
          <Column field="phone" header="Telèfon" style="width: 20%"></Column>
          <Column header="Tipus" style="width: 20%">
            <template #body="slotProps">
              <span>{{
                getSupplierTypeName(slotProps.data.supplierTypeId)
              }}</span>
            </template>
          </Column>
          <Column>
            <template #body="slotProps">
              <i
                :class="PrimeIcons.TIMES"
                class="grid_delete_column_button"
                @click="deleteSupplier($event, slotProps.data)"
              />
            </template>
          </Column>
        </DataTable>
      </TabPanel>
      <TabPanel value="1">
        <DataTable
          :value="supplierStore.supplierTypes"
          tableStyle="min-width: 100%"
          scrollable
          scrollHeight="flex"
          @row-click="editSupplierType"
        >
          <Column field="name" header="Nom" style="width: 50%"></Column>
          <Column
            field="description"
            header="Descripció"
            style="width: 50%"
          ></Column>
          <Column>
            <template #body="slotProps">
              <i
                :class="PrimeIcons.TIMES"
                class="grid_delete_column_button"
                @click="deleteSupplierType($event, slotProps.data)"
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
import { useSuppliersStore } from "../store/suppliers";
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Supplier, SupplierType } from "../types";
import { useStore } from "../../../store";

const selectedTabIndex = ref("0");
const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const supplierStore = useSuppliersStore();

onMounted(async () => {
  await supplierStore.fetchSuppliers();
  await supplierStore.fetchSupplierTypes();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: "Proveïdors",
  });
});

const getSupplierTypeName = (id: string) => {
  const supplierType = supplierStore.supplierTypes?.find((st) => st.id === id);
  if (supplierType) {
    return supplierType.name;
  }
};

const createButtonClick = () => {
  if (selectedTabIndex.value === "0") {
    router.push({ path: `/suppliers/${uuidv4()}` });
  } else {
    router.push({ path: `/supplier-types/${uuidv4()}` });
  }
};

const editSupplier = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/suppliers/${row.data.id}` });
  }
};

const editSupplierType = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/supplier-types/${row.data.id}` });
  }
};

const deleteSupplier = (event: any, supplier: Supplier) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el proveïdor ${supplier.comercialName}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await supplierStore.deleteSupplier(supplier.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await supplierStore.fetchSuppliers();
      }
    },
  });
};

const deleteSupplierType = (event: any, supplierType: SupplierType) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar el tipus de proveïdor ${supplierType.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await supplierStore.deleteSupplierType(supplierType.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await supplierStore.fetchSupplierTypes();
      }
    },
  });
};
</script>
