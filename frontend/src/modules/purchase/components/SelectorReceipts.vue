<template>
  <DataTable
    class="small-datatable"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    dataKey="id"
    :value="filteredOrders"
    v-model:selection="selectedReceipts"
  >
    <template #header>
      <header class="selector-filter">
        <div class="selector-filter-field">
          <label for="" class="mr-2">{{ t("purchase.receiptSelector.search") }}</label>
          <InputText
            style="width: 150px; height: 35px"
            v-model="selectedReceipt"
            size="small"
          />
        </div>
        <div class="selector-filter-button">
          <Button
            @click="onSelectedClick"
            :size="'small'"
            :icon="PrimeIcons.CHECK_SQUARE"
            :aria-label="t('purchase.receiptSelector.actions.select')"
          ></Button>
        </div>
      </header>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
    <Column :header="t('purchase.receiptSelector.columns.number')" field="number" style="width: 30%"></Column>
    <Column
      :header="t('purchase.receiptSelector.columns.supplierNumber')"
      field="supplierNumber"
      style="width: 30%"
    ></Column>
    <Column :header="t('purchase.receiptSelector.columns.date')" field="date" style="width: 30%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.date) }}
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import { computed, ref } from "vue";
import { Receipt } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { useI18n } from "vue-i18n";
import { formatDate } from "../../../utils/functions";

const { t } = useI18n();

const selectedReceipts = ref([] as Array<Receipt>);

const props = defineProps<{
  receipts: Array<Receipt> | undefined;
}>();
const emits = defineEmits<{
  (e: "selected", receipts: Array<Receipt>): void;
}>();

const selectedReceipt = ref("");
const filteredOrders = computed(() => {
  var filtered = [] as Array<Receipt>;

  if (props.receipts) {
    filtered = props.receipts.filter(
      (o) =>
        o.number.toString().includes(selectedReceipt.value) ||
        o.supplierNumber.includes(selectedReceipt.value),
    );
  }

  return filtered;
});

const onSelectedClick = () => {
  if (selectedReceipts.value.length === 0) return;

  emits("selected", selectedReceipts.value);
};
</script>
<style scoped>
.selector-filter {
  display: grid;
  grid-template-columns: 1fr 0.1fr;
}
</style>
