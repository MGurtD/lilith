<template>
  <div class="grid_add_row_button">
    <SplitButton
      label="Guardar"
      :model="splitButtonItems"
      :size="'small'"
      @click="submitForm"
    />
  </div>
  <Tabs value="0">
    <TabList>
      <Tab value="0">Factura</Tab>
      <Tab value="1">Fitxers</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0" v-if="purchaseInvoice">
        <FormPurchaseInvoice
          ref="purchaseInvoiceForm"
          :purchaseInvoice="purchaseInvoice"
          @submit="onInvoiceSubmit"
        />
        <Tabs value="0">
          <TabList>
            <Tab value="0">Imports</Tab>
            <Tab value="1">Venciments</Tab>
            <Tab value="2">Albarans</Tab>
          </TabList>
          <TabPanels>
            <TabPanel value="0">
              <TablePurchaseInvoiceImports
                :purchase-invoice-imports="
                  purchaseInvoice.purchaseInvoiceImports
                "
                @add="
                  (imp: PurchaseInvoiceImport) =>
                    openInvoiceImportForm(FormActionMode.CREATE, imp)
                "
                @edit="
                  (imp: PurchaseInvoiceImport) =>
                    openInvoiceImportForm(FormActionMode.EDIT, imp)
                "
                @delete="deleteInvoiceImport"
              />
            </TabPanel>
            <TabPanel value="1">
              <DataTable
                :value="
                  isDueDateEditing
                    ? editingDueDates
                    : purchaseInvoice.purchaseInvoiceDueDates
                "
                class="p-datatable-sm"
                tableStyle="min-width: 100%"
                scrollable
                scrollHeight="flex"
                sortField="dueDate"
                :sortOrder="1"
              >
                <Column
                  field="dueDate"
                  header="Venciment"
                  style="width: 50%"
                  sortable
                >
                  <template #body="slotProps">
                    {{ formatDate(slotProps.data.dueDate) }}
                  </template>
                </Column>
                <Column field="amount" header="Import" style="width: 50%">
                  <template #body="slotProps">
                    <template v-if="isDueDateEditing">
                      <InputNumber
                        v-model="slotProps.data.amount"
                        :minFractionDigits="2"
                        :maxFractionDigits="2"
                        inputClass="w-full"
                        :disabled="loadingDueDates"
                      />
                    </template>
                    <template v-else> {{ slotProps.data.amount }} € </template>
                  </template>
                  <template v-if="isDueDateEditing" #footer>
                    <div
                      class="flex justify-content-between align-items-center w-full"
                    >
                      <div class="flex flex-column gap-1 text-left">
                        <div class="font-semibold">
                          Total venciments:
                          {{ formatCurrency(editedDueDatesTotal) }}
                        </div>
                        <div
                          v-if="purchaseInvoice"
                          :class="[
                            'text-sm',
                            dueDatesDifference === 0
                              ? 'text-green-500'
                              : 'text-red-500',
                          ]"
                        >
                          Diferència: {{ formatCurrency(dueDatesDifference) }}
                        </div>
                      </div>
                      <div class="flex gap-2">
                        <Button
                          label="Cancel·lar"
                          icon="pi pi-times"
                          severity="danger"
                          size="small"
                          :disabled="loadingDueDates"
                          @click="cancelEditDueDates"
                        />
                        <Button
                          label="Guardar"
                          icon="pi pi-check"
                          severity="success"
                          size="small"
                          :loading="loadingDueDates"
                          @click="confirmEditDueDates"
                        />
                      </div>
                    </div>
                  </template>
                </Column>
              </DataTable>
            </TabPanel>
            <TabPanel value="2">
              <TableReceipts
                :receipts="receiptsStore.receipts"
                @add="onReceiptAdd"
                @delete="onReceiptDelete"
              />
            </TabPanel>
          </TabPanels>
        </Tabs>
      </TabPanel>
      <TabPanel value="1">
        <FileEntityPicker
          v-if="purchaseInvoice"
          title="Factures"
          entity="PurchaseInvoice"
          :id="route.params.id as string"
        />
      </TabPanel>
    </TabPanels>
  </Tabs>

  <Dialog
    :closable="true"
    v-model:visible="isDialogVisible"
    :header="dialogTitle"
    position="bottom"
    :style="{ width: '40rem' }"
    :breakpoints="{ '1199px': '75vw', '575px': '90vw' }"
    @after-hide="afterDialogHide"
  >
    <FormPurchaseInvoiceImport
      v-if="selectedInvoiceImport"
      :formAction="formInvoiceMode"
      :invoiceImport="selectedInvoiceImport"
      @submit="onInvoiceImportSubmit"
    />
    <SelectorReceipts
      v-if="receiptsStore.selectorReceipts"
      :receipts="receiptsStore.selectorReceipts"
      @selected="onReceiptsSelected"
    />
  </Dialog>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { usePurchaseInvoiceStore } from "../store/purchaseInvoices";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import SplitButton from "primevue/splitbutton";
import type { MenuItem } from "primevue/menuitem";
import { PurchaseInvoice, PurchaseInvoiceImport, Receipt } from "../types";
import { FormActionMode } from "../../../types/component";
import {
  convertDateTimeToJSON,
  formatCurrency,
  formatDate,
  getNewUuid,
} from "../../../utils/functions";
import FileEntityPicker from "../../../components/FileEntityPicker.vue";
import FormPurchaseInvoice from "../components/FormPurchaseInvoice.vue";
import FormPurchaseInvoiceImport from "../components/FormPurchaseInvoiceImport.vue";
import TablePurchaseInvoiceImports from "../components/TablePurchaseInvoiceImports.vue";
import TableReceipts from "../components/TableReceipts.vue";
import SelectorReceipts from "../components/SelectorReceipts.vue";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { useToast } from "primevue/usetoast";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useReceiptsStore } from "../store/receipt";
import { cloneDeep, round } from "lodash";

const purchaseInvoiceForm = ref();

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const router = useRouter();
const store = useStore();
const toast = useToast();
const lifecycleStore = useLifecyclesStore();
const purchaseMasterDataStore = usePurchaseMasterDataStore();
const purchaseInvoiceStore = usePurchaseInvoiceStore();
const receiptsStore = useReceiptsStore();
const { purchaseInvoice } = storeToRefs(purchaseInvoiceStore);

const dialogTitle = computed(() => {
  if (receiptsStore.selectorReceipts) {
    return "Selecció albarans";
  } else {
    return formInvoiceMode.value === FormActionMode.CREATE
      ? "Introducció import"
      : "Modificació import";
  }
});
const isDialogVisible = ref(false);
const formInvoiceMode = ref(FormActionMode.EDIT);
const selectedInvoiceImport = ref(
  undefined as undefined | PurchaseInvoiceImport,
);

// Due dates edit mode
const isDueDateEditing = ref(false);
const editingDueDates = ref<any[]>([]);
const loadingDueDates = ref(false);
const editedDueDatesTotal = computed(() =>
  round(
    editingDueDates.value.reduce((acc, d) => acc + (d.amount || 0), 0),
    2,
  ),
);
const dueDatesDifference = computed(() => {
  if (!purchaseInvoice.value) return 0;
  return round(purchaseInvoice.value.netAmount, 2) - editedDueDatesTotal.value;
});

// SplitButton items (dynamic visibility for edit due dates)
const splitButtonItems = computed<MenuItem[]>(() => {
  const items: MenuItem[] = [];
  if (
    purchaseInvoice.value &&
    purchaseInvoice.value.purchaseInvoiceDueDates?.length > 1 &&
    !isDueDateEditing.value
  ) {
    items.push({
      label: "Editar venciments",
      icon: "pi pi-pencil",
      command: () => startEditDueDates(),
    });
  }
  return items;
});

const loadView = async () => {
  const invoiceId = route.params.id as string;
  let pageTitle = "";

  await purchaseInvoiceStore.GetById(invoiceId);

  if (!purchaseInvoice.value) {
    formMode.value = FormActionMode.CREATE;
    purchaseInvoiceStore.setNewPurchaseInvoice(invoiceId);
    pageTitle = "Alta de factures de compra";

    setDefaultValues();
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Factura de compra: ${purchaseInvoice.value.number}`;

    purchaseInvoice.value.purchaseInvoiceDate = new Date(
      purchaseInvoice.value.purchaseInvoiceDate,
    );
  }
  // Get associated receipts
  receiptsStore.fetchByInvoice(invoiceId);

  store.setMenuItem({
    icon: PrimeIcons.POUND,
    backButtonVisible: true,
    title: pageTitle,
  });
};

const setDefaultValues = () => {
  if (purchaseInvoice.value) {
    const currentExercice = purchaseMasterDataStore.masterData.exercises?.find(
      (e) => e.name === new Date().getFullYear().toString(),
    );
    if (currentExercice) purchaseInvoice.value.exerciceId = currentExercice.id;

    const serie = purchaseMasterDataStore.masterData.series?.find(
      (s) => s.name === "Nacional",
    );
    if (serie) purchaseInvoice.value.purchaseInvoiceSerieId = serie.id;

    const status = lifecycleStore.lifecycle?.statuses?.find(
      (s) => s.name === "Nova",
    );
    if (status) purchaseInvoice.value.statusId = status.id;
  }
};

onMounted(async () => {
  await loadView();
});

const submitForm = () => {
  const form = purchaseInvoiceForm.value as any;
  form.submitForm();
};

// Invoice imports
const openInvoiceImportForm = (
  formMode: FormActionMode,
  invoiceImport: PurchaseInvoiceImport,
) => {
  if (formMode === FormActionMode.CREATE) {
    invoiceImport.id = getNewUuid();
  }

  invoiceImport.purchaseInvoiceId = purchaseInvoice.value!.id;
  selectedInvoiceImport.value = invoiceImport;
  formInvoiceMode.value = formMode;
  isDialogVisible.value = true;
};

const onInvoiceSubmit = async (invoice: PurchaseInvoice) => {
  let result = false;
  let message = "";

  invoice.purchaseInvoiceDate = convertDateTimeToJSON(
    invoice.purchaseInvoiceDate,
  );

  if (formMode.value === FormActionMode.CREATE) {
    result = await purchaseInvoiceStore.Create(invoice);
    message = result ? "Factura creada" : "Error al crear la factura";
  } else {
    result = await purchaseInvoiceStore.Update(invoice);
    message = result ? "Factura actualizada" : "Error al actualizar la factura";
  }

  toast.add({
    life: 5000,
    severity: result ? "success" : "error",
    summary: message,
  });

  if (result) {
    router.back();
  }
};

const onInvoiceImportSubmit = async (invoiceImport: PurchaseInvoiceImport) => {
  // When is a new record, work in memory to send a composed object to the backend
  if (formInvoiceMode.value === FormActionMode.CREATE) {
    if (formMode.value === FormActionMode.EDIT) {
      await purchaseInvoiceStore.CreateInvoiceImport(invoiceImport);
    }

    purchaseInvoice.value?.purchaseInvoiceImports.push(invoiceImport);
  } else if (formInvoiceMode.value === FormActionMode.EDIT) {
    if (formMode.value === FormActionMode.EDIT) {
      await purchaseInvoiceStore.UpdateInvoiceImport(invoiceImport);
    }
  }

  closeDialogAndCalcAmounts();
};

const deleteInvoiceImport = async (invoiceImport: PurchaseInvoiceImport) => {
  if (formMode.value === FormActionMode.EDIT) {
    await purchaseInvoiceStore.DeleteInvoiceImport(invoiceImport);
  }

  const afterImports = purchaseInvoice.value!.purchaseInvoiceImports.filter(
    (i) => i.id !== invoiceImport.id,
  );
  purchaseInvoice.value!.purchaseInvoiceImports = afterImports;

  closeDialogAndCalcAmounts();
};

const closeDialogAndCalcAmounts = () => {
  const form = purchaseInvoiceForm.value as any;
  form.calcAmounts();
  isDialogVisible.value = false;
};

const onReceiptAdd = async () => {
  if (!purchaseInvoice.value) return;

  await receiptsStore.fetchInvoiceableBySupplier(
    purchaseInvoice.value.supplierId,
  );
  isDialogVisible.value = true;
};

const afterDialogHide = () => {
  selectedInvoiceImport.value = undefined;
  receiptsStore.selectorReceipts = undefined;
};

const onReceiptsSelected = async (receipts: Array<Receipt>) => {
  const promises = [] as Array<Promise<any>>;

  for (let i = 0; i < receipts.length; i++) {
    promises.push(
      receiptsStore.associateInvoice(receipts[i], purchaseInvoice.value!.id),
    );
  }

  await Promise.all(promises);
  await receiptsStore.fetchByInvoice(purchaseInvoice.value!.id);

  isDialogVisible.value = false;
  receiptsStore.selectorReceipts = undefined;
};

const onReceiptDelete = async (receipt: Receipt) => {
  receipt.purchaseInvoiceId = null;
  await receiptsStore.unassociateInvoice(receipt, purchaseInvoice.value!.id);
  await receiptsStore.fetchByInvoice(purchaseInvoice.value!.id);
};

const startEditDueDates = () => {
  if (!purchaseInvoice.value) return;
  // Deep clone to avoid mutating original until confirm
  editingDueDates.value = cloneDeep(
    purchaseInvoice.value.purchaseInvoiceDueDates,
  );
  isDueDateEditing.value = true;
};

const confirmEditDueDates = async () => {
  if (!purchaseInvoice.value) return;
  const invoiceTotal = round(purchaseInvoice.value.netAmount, 2);
  const dueTotal = round(
    editingDueDates.value.reduce((acc, d) => acc + d.amount, 0),
    2,
  );

  if (dueTotal !== 0 && dueTotal !== invoiceTotal) {
    toast.add({
      severity: "error",
      summary: "Error de validació",
      detail: `La suma (${dueTotal} €) no coincideix amb el total de la factura (${invoiceTotal} €)`,
      life: 5000,
    });
    return;
  }

  try {
    loadingDueDates.value = true;

    editingDueDates.value.forEach((d) => {
      if (!d.id) d.id = getNewUuid();
      d.purchaseInvoiceId = purchaseInvoice.value!.id;
    });
    await purchaseInvoiceStore.ReplaceDueDates(editingDueDates.value);

    toast.add({
      severity: "success",
      summary: "Venciments actualitzats",
      life: 4000,
    });
    isDueDateEditing.value = false;
  } catch (e: any) {
    toast.add({
      severity: "error",
      summary: "Error guardant venciments",
      detail: e?.message ?? "",
      life: 6000,
    });
  } finally {
    loadingDueDates.value = false;
  }
};

const cancelEditDueDates = () => {
  // Simply exit edit mode and discard the working copy
  isDueDateEditing.value = false;
  editingDueDates.value = [];
};
</script>
