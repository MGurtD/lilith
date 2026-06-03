<template>
  <TableInvoices
    :invoices="invoiceStore.invoices"
    :customers="customersStore.customers"
    @edit="editSalesInvoice"
    @delete="deleteSalesInvoice"
  >
    <template #filter>
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :body-width="filterBodyWidth"
        embedded
        @filter="filterInvoices"
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
              >Client</label
            >
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
        </template>
      </TableFilter>
    </template>
  </TableInvoices>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '80vw', maxWidth: '425px' }"
  >
    <FormCreateOrderOrInvoice
      :create-request="createRequest"
      @submit="createInvoice"
    />
  </Dialog>
</template>
<script setup lang="ts">
import TableInvoices from "../components/TableInvoices.vue";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useSalesInvoiceStore } from "../store/invoice";
import { useCustomersStore } from "../store/customers";
import { onMounted, onUnmounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import {
  formatDateForQueryParameter,
  getNewUuid,
} from "../../../utils/functions";
import { CreateSalesHeaderRequest, SalesInvoice } from "../types";
import { DialogOptions } from "../../../types/component";
import FormCreateOrderOrInvoice from "../components/FormCreateOrderOrInvoice.vue";
import { useUserFilterStore } from "../../../store/userfilter";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const customersStore = useCustomersStore();
const invoiceStore = useSalesInvoiceStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "50%", tablet: "75%" };

const filter = ref({
  dates: undefined as Array<Date> | undefined,
  customerId: undefined as string | undefined,
});
const dialogOptions = reactive({
  visible: false,
  title: "Crear factura",
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
  customersStore.fetchCustomers();

  setCurrentYear();
  getUserFilter();
  await filterInvoices();

  store.setMenuItem({
    icon: PrimeIcons.MONEY_BILL,
    title: "Factures de venta",
  });
});

onUnmounted(() => {
  userFilterStore.addFilter("SalesInvoices", "", filter.value);
  invoiceStore.invoices = undefined;
});

const getUserFilter = () => {
  const userFilter = userFilterStore.getFilter("SalesInvoices", "");
  if (userFilter) {
    filter.value.customerId = userFilter.customerId;
    if (userFilter.dates) {
      filter.value.dates = [
        new Date(userFilter.dates[0]),
        new Date(userFilter.dates[1]),
      ];
    }
  }
};

const cleanFilter = () => {
  filter.value.customerId = undefined;
  setCurrentYear();
};

const filterInvoices = async () => {
  let startTime = "";
  let endTime = "";

  if (
    filter.value.dates &&
    filter.value.dates.length === 2 &&
    filter.value.dates[1]
  ) {
    startTime = formatDateForQueryParameter(filter.value.dates[0]);
    endTime = formatDateForQueryParameter(filter.value.dates[1]);
  }

  await invoiceStore.GetFiltered(
    startTime,
    endTime,
    undefined,
    filter.value.customerId,
    undefined,
  );
};

const createRequest = ref({} as CreateSalesHeaderRequest);
const generateNewRequest = (): CreateSalesHeaderRequest => {
  return {
    id: getNewUuid(),
    customerId: "",
    exerciseId: "",
    date: new Date(),
  };
};

const createButtonClick = () => {
  createRequest.value = generateNewRequest();
  dialogOptions.visible = true;
};

const createInvoice = async () => {
  const response = await invoiceStore.Create(createRequest.value);
  if (response && !response?.result) {
    const errorMessage =
      response.errors.length > 0
        ? response.errors[0]
        : "Error desconegut, contacte amb l'administrador.";

    toast.add({
      severity: "warn",
      summary: "Error al crear la factura",
      detail: errorMessage,
      life: 10000,
    });
    return;
  }

  if (response)
    router.push({ path: `/sales-invoice/${createRequest.value.id}` });
};

const editSalesInvoice = (invoice: SalesInvoice) => {
  router.push({ path: `/sales-invoice/${invoice.id}` });
};

const deleteSalesInvoice = (invoice: SalesInvoice) => {
  confirm.require({
    message: `Està segur que vol eliminar la factura?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await invoiceStore.Delete(invoice.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });

        await filterInvoices();
      }
    },
  });
};
</script>
