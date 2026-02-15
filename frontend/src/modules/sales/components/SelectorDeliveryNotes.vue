<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :metaKeySelection="false"
    :value="filteredDeliveryNotes"
    selectionMode="multiple"
    v-model:selection="selectedDeliveryNote"
  >
    <template #header>
      <header class="selector-filter">
        <div class="selector-filter-field">
          <label for="">Buscar</label> &nbsp;
          <InputText
            style="width: 150px; height: 35px"
            v-model="codeToFilter"
            size="small"
          />
        </div>
        <div class="selector-filter-button">
          <Button
            @click="onSelectedClick"
            :size="'small'"
            :icon="PrimeIcons.CHECK_SQUARE"
          ></Button>
        </div>
      </header>
    </template>

    <template #groupheader="slotProps">
      <div class="flex align-items-center gap-2">
        <b
          >Albarà d'entrega {{ slotProps.data.salesOrderNumber }} -
          {{ formatDate(slotProps.data.salesOrderDate) }}</b
        >
      </div>
    </template>

    <Column header="Número" field="number" style="width: 10%"></Column>
    <Column header="Estat" field="status" style="width: 10%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column header="Data Entrega" field="deliveryDate" style="width: 10%">
      <template #body="slotProps">
        {{
          slotProps.data.deliveryDate
            ? formatDate(slotProps.data.deliveryDate)
            : ""
        }}
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { DeliveryNote } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { formatDate } from "../../../utils/functions";
import { useLifecyclesStore } from "../../shared/store/lifecycle";

const props = defineProps<{
  deliveryNotes: Array<DeliveryNote> | undefined;
  headerVisible?: boolean;
}>();
const emits = defineEmits<{
  (e: "selected", deliveryNotes: Array<DeliveryNote>): void;
}>();

onMounted(() => {
  lifecycleStore.fetchOneByName("DeliveryNote");
});

const lifecycleStore = useLifecyclesStore();

const selectedDeliveryNote = ref([] as Array<DeliveryNote>);
const codeToFilter = ref("");
const filteredDeliveryNotes = computed(() => {
  var filtered = [] as Array<DeliveryNote>;

  if (props.deliveryNotes) {
    filtered = props.deliveryNotes.filter((o) =>
      o.number.toString().includes(codeToFilter.value),
    );
  }

  return filtered;
});

const getStatusNameById = (statusId: string) => {
  const status = lifecycleStore.lifecycle?.statuses.find(
    (s) => s.id === statusId,
  );
  return status ? status.name : "";
};

const onSelectedClick = () => {
  if (selectedDeliveryNote.value.length === 0) return;
  emits("selected", selectedDeliveryNote.value);
};
</script>
<style scoped>
.selector-filter {
  display: grid;
  grid-template-columns: 1fr 0.1fr;
}
</style>
