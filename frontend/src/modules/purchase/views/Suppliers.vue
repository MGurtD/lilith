<template>
  <Tabs v-model:value="selectedTabIndex">
    <TabList>
      <Tab value="0">
        <i :class="PrimeIcons.LINK" class="mr-2"></i>
        <span>{{ $t("purchase.suppliers.title") }}</span>
      </Tab>
      <Tab value="1">
        <i :class="PrimeIcons.HASHTAG" class="mr-2"></i>
        <span>{{ $t("purchase.supplierTypes.title") }}</span>
      </Tab>
      </TabList>
    <TabPanels>
      <TabPanel value="0">
        <Table
          preset="crud-list"
          :columns="supplierColumns"
          :items="supplierStore.suppliers ?? []"
          :filter-config="[]"
          :show-filter-actions="false"
          delete-column-width="5%"
          show-delete-column
          tableStyle="min-width: 100%"
          @row-click="editSupplier"
          @create="createButtonClick"
          @delete="deleteSupplier"
        >
          <template #prepend>
            <span class="text-900 font-bold">{{ t("purchase.suppliers.title") }}</span>
          </template>
        </Table>
      </TabPanel>
      <TabPanel value="1">
        <Table
          preset="crud-list"
          :columns="supplierTypeColumns"
          :items="supplierStore.supplierTypes ?? []"
          :filter-config="[]"
          :show-filter-actions="false"
          delete-column-width="5%"
          show-delete-column
          tableStyle="min-width: 100%"
          @row-click="editSupplierType"
          @create="createButtonClick"
          @delete="deleteSupplierType"
        >
          <template #prepend>
            <span class="text-900 font-bold">{{ t("purchase.supplierTypes.title") }}</span>
          </template>
        </Table>
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type Column,
} from "../../../components/tables/types";
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useSuppliersStore } from "../store/suppliers";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { Supplier, SupplierType } from "../types";
import { useStore } from "../../../store";
import { useI18n } from "vue-i18n";

const selectedTabIndex = ref("0");
const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const supplierStore = useSuppliersStore();
const { t } = useI18n();

const supplierColumns = computed<Column[]>(() => [
  {
    field: "comercialName",
    header: t("purchase.fields.commercialName"),
    style: "width: 19%",
  },
  {
    field: "taxName",
    header: t("purchase.fields.taxName"),
    style: "width: 19%",
  },
  { field: "vatNumber", header: "CIF", style: "width: 19%" },
  {
    field: "phone",
    header: t("purchase.fields.phone"),
    style: "width: 19%",
  },
  {
    field: "supplierTypeId",
    header: t("purchase.fields.type"),
    columnType: ColumnType.Lookup,
    resolver: getSupplierTypeName,
    style: "width: 19%",
  },
]);

const supplierTypeColumns = computed<Column[]>(() => [
  {
    field: "name",
    header: t("purchase.fields.name"),
    style: "width: 47.5%",
  },
  {
    field: "description",
    header: t("purchase.fields.description"),
    style: "width: 47.5%",
  },
]);

onMounted(async () => {
  await supplierStore.fetchSuppliers();
  await supplierStore.fetchSupplierTypes();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: t("purchase.suppliers.title"),
  });
});

function getSupplierTypeName(id: string): string {
  const supplierType = supplierStore.supplierTypes?.find((st) => st.id === id);
  return supplierType?.name ?? "";
}

const createButtonClick = () => {
  if (selectedTabIndex.value === "0") {
    router.push({ path: `/suppliers/${getNewUuid()}` });
  } else {
    router.push({ path: `/supplier-types/${getNewUuid()}` });
  }
};

const editSupplier = (row: DataTableRowClickEvent) => {
  router.push({ path: `/suppliers/${row.data.id}` });
};

const editSupplierType = (row: DataTableRowClickEvent) => {
  router.push({ path: `/supplier-types/${row.data.id}` });
};

const deleteSupplier = (supplier: Supplier) => {
  confirm.require({
    message: t("purchase.messages.confirmDeleteSupplier", { name: supplier.comercialName }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await supplierStore.deleteSupplier(supplier.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.messages.deleted"),
          life: 3000,
        });
        await supplierStore.fetchSuppliers();
      }
    },
  });
};

const deleteSupplierType = (supplierType: SupplierType) => {
  confirm.require({
    message: t("purchase.messages.confirmDeleteSupplierType", { name: supplierType.name }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await supplierStore.deleteSupplierType(supplierType.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: t("purchase.messages.deleted"),
          life: 3000,
        });
        await supplierStore.fetchSupplierTypes();
      }
    },
  });
};
</script>
