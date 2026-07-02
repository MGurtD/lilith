<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    paginator
    :rows="20"
    :value="receiptsStore.receipts"
    @row-click="editReceipt"
  >
    <template #header>
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="filterReceipts"
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Període</label
            >
            <DatePicker
              v-model="filter.dates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              placeholder="Selecciona període"
              showIcon
              class="w-full"
              size="small"
            />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Proveïdor</label
            >
            <DropdownSupplier label="" v-model="filter.supplierId" />
          </div>
        </template>
      </TableFilter>
    </template>
    <Column
      field="number"
      header="Número"
      :sortable="true"
      style="width: 10%"
    ></Column>
    <Column header="Data" field="date" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
    <Column header="Proveïdor" style="width: 15%">
      <template #body="slotProps">
        {{ getSupplierNameById(slotProps.data.supplierId) }}
      </template>
    </Column>
    <Column
      header="Número Albarà"
      style="width: 15%"
      field="supplierNumber"
    ></Column>
    <Column header="Estat" style="width: 15%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column style="width: 5%">
      <template #body="slotProps">
        <i
          v-if="
            lifecycleStore.lifecycle?.initialStatusId ===
            slotProps.data.statusId
          "
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteReceipt($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreatePurchaseDocument
      :create-request="createRequest"
      @submit="createReceipt"
    />
  </Dialog>
</template>
<script setup lang="ts">
import FormCreatePurchaseDocument from "../components/FormCreatePurchaseDocument.vue";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useReceiptsStore } from "../store/receipt";
import { useSuppliersStore } from "../store/suppliers";
import { DataTableRowClickEvent } from "primevue/datatable";
import { onMounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DialogOptions } from "../../../types/component";
import {
  formatDateForQueryParameter,
  formatDate,
  getNewUuid,
} from "../../../utils/functions";
import { CreatePurchaseDocumentRequest, PurchaseInvoice } from "../types";
import { useLifecyclesStore } from "../../shared/store/lifecycle";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const lifecycleStore = useLifecyclesStore();
const receiptsStore = useReceiptsStore();
const suppliersStore = useSuppliersStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  supplierId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear albarà",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const setCurrentYear = () => {
  const now = new Date();
  filter.value.dates = [
    new Date(now.getFullYear(), 0, 1),
    new Date(now.getFullYear(), 11, 31),
  ];
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Albarans de compra",
  });

  suppliersStore.fetchSuppliers();
  lifecycleStore.fetchOneByName("Receipts");
  setCurrentYear();

  await filterReceipts();
});

const cleanFilter = () => {
  filter.value.supplierId = undefined;
  setCurrentYear();
};

const filterReceipts = async () => {
  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    const startTime = formatDateForQueryParameter(filter.value.dates[0]);
    const endTime = formatDateForQueryParameter(filter.value.dates[1]);

    await receiptsStore.fetchFiltered(
      startTime,
      endTime,
      filter.value.supplierId,
    );
  } else {
    toast.add({
      severity: "info",
      summary: "Filtre invàlid",
      detail: "Seleccioni un període",
      life: 5000,
    });
  }
};
const getSupplierNameById = (id: string) => {
  const supplier = suppliersStore.suppliers?.find((s) => s.id === id);
  if (supplier) return supplier.comercialName;
  else return "";
};
const getStatusNameById = (id: string) => {
  if (lifecycleStore.lifecycle) {
    const status = lifecycleStore.lifecycle.statuses.find((s) => s.id === id);
    if (status) return status.name;
  }
  return "";
};
const createButtonClick = () => {
  createRequest.value = generateNewRequest();
  dialogOptions.visible = true;
};
const createRequest = ref({} as CreatePurchaseDocumentRequest);
const generateNewRequest = (): CreatePurchaseDocumentRequest => {
  return {
    id: getNewUuid(),
    supplierId: "",
    exerciseId: "",
    date: new Date(),
  };
};
const createReceipt = async () => {
  const created = await receiptsStore.createReceipt(createRequest.value);
  dialogOptions.visible = false;
  if (created) router.push({ path: `/receipts/${createRequest.value.id}` });
};

const editReceipt = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/receipts/${row.data.id}` });
  }
};

const deleteReceipt = (event: any, invoice: PurchaseInvoice) => {
  confirm.require({
    target: event.currentTarget,
    message: `Està segur que vol eliminar l'albarà ${invoice.number}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await receiptsStore.deleteReceipt(invoice.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
        await filterReceipts();
      }
    },
  });
};
</script>
