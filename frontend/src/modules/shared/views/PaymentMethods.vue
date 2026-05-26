<template>
  <DataTable
    :value="filteredPaymentMethods"
    tableStyle="min-width: 100%"
    scrollHeight="flex"
    @row-click="editPaymentMethod"
  >
    <template #header>
      <TableFilter
        :config="filterConfig"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :show-filter-action="false"
        embedded
        @clear="cleanFilter"
        @create="createButtonClick"
      />
    </template>
    <Column field="name" header="Nom" style="width: 20%"></Column>
    <Column field="description" header="Descripció" style="width: 20%"></Column>
    <Column field="dueDays" header="Dies venciment" style="width: 20%"></Column>
    <Column
      field="paymentDay"
      header="Dia pagament"
      style="width: 20%"
    ></Column>
    <Column header="Desactivada" style="width: 20%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { v4 as uuidv4 } from "uuid";
import { PrimeIcons } from "@primevue/core/api";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { PaymentMethod } from "../types";
import { useStore } from "../../../store";
import { usePaymentMethodStore } from "../store/paymentMethod";
import TableFilter, {
  type FilterConfig,
} from "../../../components/tables/TableFilter.vue";

const toast = useToast();
const confirm = useConfirm();
const router = useRouter();
const store = useStore();
const paymentMethodStore = usePaymentMethodStore();

const filter = ref({
  search: "",
});

const filterConfig: Array<FilterConfig> = [
  {
    key: "search",
    label: "Cercar",
    type: "text",
    placeholder: "Nom o descripció",
    size: "sm",
    row: 0,
  },
];

const filteredPaymentMethods = computed(() => {
  if (!paymentMethodStore.paymentMethods) return [];

  const search = filter.value.search.trim().toLowerCase();
  if (!search) return paymentMethodStore.paymentMethods;

  return paymentMethodStore.paymentMethods.filter((paymentMethod) => {
    return (
      paymentMethod.name.toLowerCase().includes(search) ||
      paymentMethod.description.toLowerCase().includes(search)
    );
  });
});

onMounted(async () => {
  await paymentMethodStore.fetchAll();

  store.setMenuItem({
    icon: PrimeIcons.HASHTAG,
    title: "Gestió de formes de pagament",
  });
});

const createButtonClick = () => {
  router.push({ path: `/payment-methods/${uuidv4()}` });
};

const cleanFilter = () => {
  filter.value.search = "";
};

const editPaymentMethod = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    )
  ) {
    router.push({ path: `/payment-methods/${row.data.id}` });
  }
};

const deletePaymentMethod = (event: any, model: PaymentMethod) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la forma de pagament ${model.name}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await paymentMethodStore.delete(model.id);
      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminat",
          life: 3000,
        });
      }

      await paymentMethodStore.fetchAll();
    },
  });
};
</script>

<style scoped>
:deep(.table-filter__field) {
  max-width: 25%;
}

@media (max-width: 1200px) {
  :deep(.table-filter__field) {
    max-width: 40%;
  }
}

@media (max-width: 768px) {
  :deep(.table-filter__field) {
    max-width: 100%;
  }
}
</style>
