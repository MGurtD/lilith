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
          phone-layout="cards"
          :card-layout="supplierCardLayout"
          preset="crud-list"
          :columns="supplierColumns"
          :items="filteredSuppliers"
          :filter-config="supplierFilterConfig"
          v-model:filter-values="supplierFilter"
          :filter-body-width="supplierFilterBodyWidth"
          :show-filter-actions="false"
          delete-column-width="5%"
          show-delete-column
          tableStyle="min-width: 100%"
          @row-click="editSupplier"
          @create="createButtonClick"
          @delete="deleteSupplier"
        />
      </TabPanel>
      <TabPanel value="1">
        <Table
          phone-layout="cards"
          :card-layout="supplierTypeCardLayout"
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

  <Dialog
    v-model:visible="supplierTypeDialogVisible"
    :header="supplierTypeDialogTitle"
    :closable="!supplierTypeSaving"
    modal
    :style="{ width: '80vw', maxWidth: '425px' }"
    @hide="closeSupplierTypeDialog"
  >
    <FormSupplierType
      v-if="selectedSupplierType"
      :key="selectedSupplierType.id"
      :supplier-type="selectedSupplierType"
      :loading="supplierTypeSaving"
      @submit="submitSupplierType"
    />
  </Dialog>
</template>
<script setup lang="ts">
import Table from "../../../components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "../../../components/tables/types";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "../../../components/tables/TableFilter.vue";
import { getNewUuid } from "../../../utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useSuppliersStore } from "../store/suppliers";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import type { DataTableRowClickEvent } from "primevue/datatable";
import type { Supplier, SupplierType } from "../types";
import { useStore } from "../../../store";
import { useI18n } from "vue-i18n";
import { FormActionMode } from "../../../types/component";
import FormSupplierType from "../components/FormSupplierType.vue";

const selectedTabIndex = ref("0");
const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const supplierStore = useSuppliersStore();
const { t } = useI18n();
const supplierTypeDialogVisible = ref(false);
const supplierTypeFormMode = ref(FormActionMode.CREATE);
const selectedSupplierType = ref<SupplierType>();
const supplierTypeSaving = ref(false);

const supplierTypeDialogTitle = computed(() =>
  supplierTypeFormMode.value === FormActionMode.CREATE
    ? t("purchase.supplierTypes.createTitle")
    : t("purchase.supplierTypes.detailTitle", {
        name: selectedSupplierType.value?.name ?? "",
      }),
);

const supplierFilter = ref({
  name: "",
  supplierTypeId: "",
});

const supplierFilterBodyWidth: FilterBodyWidth = {
  desktop: "50%",
  tablet: "75%",
};

const supplierFilterConfig = computed<FilterConfig[]>(() => [
  {
    key: "name",
    label: t("purchase.fields.name"),
    type: "text",
    placeholder: t("purchase.fields.name"),
    size: "lg",
  },
  {
    key: "supplierTypeId",
    label: t("purchase.fields.type"),
    type: "select",
    options: supplierStore.supplierTypes ?? [],
    optionLabel: "name",
    optionValue: "id",
    placeholder: t("purchase.fields.type"),
    size: "md",
  },
]);

const filteredSuppliers = computed(() => {
  const name = supplierFilter.value.name.trim().toLocaleLowerCase();
  const supplierTypeId = supplierFilter.value.supplierTypeId;

  return (supplierStore.suppliers ?? []).filter(
    (supplier) =>
      (!name || supplier.comercialName.toLocaleLowerCase().includes(name)) &&
      (!supplierTypeId || supplier.supplierTypeId === supplierTypeId),
  );
});

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

const supplierCardLayout: CardLayout = {
  title: "comercialName",
  subtitle: "taxName",
  meta: ["vatNumber", "phone", "supplierTypeId"],
};

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

const supplierTypeCardLayout: CardLayout = {
  title: "name",
  subtitle: "description",
};

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
    supplierTypeFormMode.value = FormActionMode.CREATE;
    selectedSupplierType.value = {
      id: getNewUuid(),
      name: "",
      description: "",
      disabled: false,
    };
    supplierTypeDialogVisible.value = true;
  }
};

const editSupplier = (row: DataTableRowClickEvent) => {
  router.push({ path: `/suppliers/${row.data.id}` });
};

const editSupplierType = (row: DataTableRowClickEvent) => {
  supplierTypeFormMode.value = FormActionMode.EDIT;
  selectedSupplierType.value = { ...(row.data as SupplierType) };
  supplierTypeDialogVisible.value = true;
};

const submitSupplierType = async (supplierType: SupplierType) => {
  if (supplierTypeSaving.value) return;

  supplierTypeSaving.value = true;
  try {
    const created = supplierTypeFormMode.value === FormActionMode.CREATE;
    const saved = created
      ? await supplierStore.createSupplierType(supplierType)
      : await supplierStore.updateSupplierType(supplierType.id, supplierType);

    if (!saved) return;

    toast.add({
      severity: "success",
      summary: t(
        created
          ? "purchase.messages.supplierTypeCreated"
          : "purchase.messages.supplierTypeUpdated",
      ),
      life: 5000,
    });
    supplierTypeDialogVisible.value = false;
  } finally {
    supplierTypeSaving.value = false;
  }
};

const closeSupplierTypeDialog = () => {
  selectedSupplierType.value = undefined;
  supplierTypeFormMode.value = FormActionMode.CREATE;
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
