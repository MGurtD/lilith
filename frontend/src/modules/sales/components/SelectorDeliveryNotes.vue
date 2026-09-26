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
          <label>{{ t('sales.components.buscar') }}</label>
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

    <Column :header="t('sales.components.numero')" field="number" style="width: 10%"></Column>
    <Column :header="t('sales.components.estat')" field="status" style="width: 10%">
      <template #body="slotProps">
        {{ getStatusNameById(slotProps.data.statusId) }}
      </template>
    </Column>
    <Column :header="t('sales.components.dataEntrega')" field="deliveryDate" style="width: 10%">
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
import { useI18n } from "vue-i18n";
import { computed, onMounted, ref } from "vue";
import { DeliveryNote } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { formatDate } from "../../../utils/functions";
import { useLifecyclesStore } from "../../shared/store/lifecycle";

const { t } = useI18n();
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
