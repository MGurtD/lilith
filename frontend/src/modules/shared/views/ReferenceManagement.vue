<template>
  <div v-if="!loading && reference">
    <Tabs value="general">
      <TabList>
        <Tab value="general">
          <i :class="PrimeIcons.INFO_CIRCLE" class="mr-2" />General
        </Tab>
        <Tab
          value="sales"
          v-if="formMode === FormActionMode.EDIT && reference.sales"
        >
          <i :class="PrimeIcons.SHOPPING_CART" class="mr-2" />Ventes
        </Tab>
        <Tab
          value="purchase"
          v-if="formMode === FormActionMode.EDIT && reference.purchase"
        >
          <i :class="PrimeIcons.TRUCK" class="mr-2" />Compres
        </Tab>
        <Tab
          value="production"
          v-if="formMode === FormActionMode.EDIT && reference.production"
        >
          <i :class="PrimeIcons.COG" class="mr-2" />Producció
        </Tab>
        <Tab value="warehouse" v-if="formMode === FormActionMode.EDIT">
          <i :class="PrimeIcons.BUILDING" class="mr-2" />Magatzem
        </Tab>
      </TabList>

      <TabPanels>
        <!-- ============ GENERAL ============ -->
        <TabPanel value="general">
          <div class="flex justify-content-end mb-2">
            <Button
              label="Guardar"
              size="small"
              :icon="PrimeIcons.SAVE"
              @click="submitGeneral"
            />
          </div>
          <form>
            <section class="five-columns">
              <div class="mt-1">
                <BaseInput label="Codi" id="code" v-model="reference.code" />
              </div>
              <div class="mt-1">
                <BaseInput
                  label="Descripció"
                  id="description"
                  v-model="reference.description"
                />
              </div>
              <div class="mt-1">
                <BaseInput
                  label="Versió"
                  id="version"
                  v-model="reference.version"
                />
              </div>
              <div class="mt-1">
                <DropdownReferenceType
                  label="Tipus de material"
                  v-model="reference.referenceTypeId"
                />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">Format</label>
                <Select
                  v-model="reference.referenceFormatId"
                  :options="referenceStore.referenceFormats"
                  optionValue="id"
                  optionLabel="description"
                  class="w-full"
                  showClear
                />
              </div>
            </section>
            <section class="five-columns">
              <div class="mt-1">
                <DropdownCustomers
                  label="Client"
                  v-model="reference.customerId"
                />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">Impost</label>
                <Select
                  v-model="reference.taxId"
                  :options="taxesStore.taxes"
                  optionValue="id"
                  optionLabel="name"
                  class="w-full"
                />
              </div>
              <div class="mt-1">
                <BaseInput
                  :type="BaseInputType.CURRENCY"
                  label="Cost Teóric Fabricació"
                  id="workMasterCost"
                  v-model="reference.workMasterCost"
                  disabled
                />
              </div>
              <div class="mt-1">
                <BaseInput
                  :type="BaseInputType.CURRENCY"
                  label="Cost Última Fabricació / Compra"
                  id="lastCost"
                  v-model="reference.lastCost"
                  disabled
                />
              </div>
              <div class="mt-1">
                <BaseInput
                  :type="BaseInputType.CURRENCY"
                  label="PVP"
                  id="price"
                  v-model="reference.price"
                />
              </div>
            </section>
            <section class="five-columns">
              <div class="mt-1">
                <label class="block text-900 mb-2">Activa</label>
                <Checkbox v-model="isActive" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">Ventes</label>
                <Checkbox v-model="reference.sales" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">Compres</label>
                <Checkbox v-model="reference.purchase" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">Producció</label>
                <Checkbox v-model="reference.production" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">Servei</label>
                <Checkbox v-model="reference.isService" :binary="true" />
              </div>
            </section>
          </form>

          <div v-if="formMode === FormActionMode.EDIT" class="mt-4">
            <FileEntityPicker
              title="Documentació"
              entity="referenceMaps"
              :id="reference.id"
              :key="reference.id"
            />
          </div>
        </TabPanel>

        <!-- ============ VENTES ============ -->
        <TabPanel value="sales" v-if="reference.sales">
          <section class="five-columns mb-3">
            <div class="mt-1">
              <BaseInput
                :type="BaseInputType.CURRENCY"
                label="PVP"
                id="pvp"
                v-model="reference.price"
              />
            </div>
            <div class="mt-1 flex align-items-end">
              <Button
                label="Guardar PVP"
                size="small"
                :icon="PrimeIcons.SAVE"
                @click="submitGeneral"
              />
            </div>
          </section>

          <DataTable
            :value="salesHistoryRows"
            tableStyle="min-width: 100%"
            scrollable
            scrollHeight="flex"
            :loading="salesLoading"
            paginator
            :rows="10"
          >
            <template #header>
              <label class="block text-900">Històric de ventes (albarans)</label>
            </template>
            <template #empty>Sense ventes registrades.</template>
            <Column field="date" header="Data">
              <template #body="{ data }">{{ formatDate(data.date) }}</template>
            </Column>
            <Column field="number" header="Albarà" />
            <Column field="customerName" header="Client" />
            <Column field="quantity" header="Quantitat" />
            <Column field="unitPrice" header="Preu unitari">
              <template #body="{ data }">{{
                formatCurrency(data.unitPrice)
              }}</template>
            </Column>
            <Column field="amount" header="Import">
              <template #body="{ data }">{{
                formatCurrency(data.amount)
              }}</template>
            </Column>
          </DataTable>
        </TabPanel>

        <!-- ============ COMPRES ============ -->
        <TabPanel value="purchase" v-if="reference.purchase">
          <section class="five-columns mb-3">
            <div class="mt-1">
              <BaseInput
                :type="BaseInputType.CURRENCY"
                label="PUC (últim cost de compra)"
                id="puc"
                v-model="reference.lastCost"
                disabled
              />
            </div>
          </section>

          <Tabs value="suppliers">
            <TabList>
              <Tab value="suppliers">Proveïdors i tarifes</Tab>
              <Tab value="externalServices">Tarifes de serveis externs</Tab>
              <Tab value="transport">Tarifes de transport</Tab>
              <Tab value="purchaseHistory">Històric de compres</Tab>
            </TabList>
            <TabPanels>
              <TabPanel value="suppliers">
                <DataTable
                  :value="referenceStore.referenceSuppliers"
                  tableStyle="min-width: 100%"
                  scrollable
                  scrollHeight="flex"
                  :loading="suppliersLoading"
                >
                  <template #header>
                    <div
                      class="flex flex-wrap align-items-center justify-content-between gap-2"
                    >
                      <label class="block text-900">Proveïdors i tarifes</label>
                      <Button
                        :icon="PrimeIcons.PLUS"
                        rounded
                        raised
                        @click="newSupplier"
                      />
                    </div>
                  </template>
                  <template #empty>Sense proveïdors assignats.</template>
                  <Column header="Proveïdor">
                    <template #body="{ data }">{{
                      suppliersStore.getName(data.supplierId)
                    }}</template>
                  </Column>
                  <Column field="supplierCode" header="Codi proveïdor" />
                  <Column
                    field="supplierDescription"
                    header="Descripció proveïdor"
                  />
                  <Column field="supplierPrice" header="Preu">
                    <template #body="{ data }">{{
                      formatCurrency(data.supplierPrice)
                    }}</template>
                  </Column>
                  <Column field="supplyDays" header="Dies entrega" />
                  <Column>
                    <template #body="{ data }">
                      <i
                        :class="PrimeIcons.PENCIL"
                        class="cursor-pointer mr-3"
                        @click="editSupplier(data)"
                      />
                      <i
                        :class="PrimeIcons.TIMES"
                        class="grid_delete_column_button"
                        @click="removeSupplier(data)"
                      />
                    </template>
                  </Column>
                </DataTable>
              </TabPanel>

              <TabPanel value="externalServices">
                <DataTable
                  :value="externalServiceRateRows"
                  tableStyle="min-width: 100%"
                  scrollable
                  scrollHeight="flex"
                  :loading="ratesLoading"
                >
                  <template #empty>Sense tarifes de serveis externs.</template>
                  <Column field="supplierName" header="Proveïdor" />
                  <Column field="rateName" header="Tarifa" />
                  <Column header="Vàlid des de">
                    <template #body="{ data }">{{
                      formatDate(data.validFrom)
                    }}</template>
                  </Column>
                  <Column header="Vàlid fins a">
                    <template #body="{ data }">{{
                      formatDate(data.validTo)
                    }}</template>
                  </Column>
                  <Column header="Càlcul">
                    <template #body="{ data }">{{
                      getCalculationTypeLabel(data.calculationType)
                    }}</template>
                  </Column>
                  <Column field="from" header="Des de" />
                  <Column field="to" header="Fins a" />
                  <Column field="price" header="Preu">
                    <template #body="{ data }">{{
                      formatCurrency(data.price)
                    }}</template>
                  </Column>
                </DataTable>
              </TabPanel>

              <TabPanel value="transport">
                <DataTable
                  :value="transportRateRows"
                  tableStyle="min-width: 100%"
                  scrollable
                  scrollHeight="flex"
                  :loading="ratesLoading"
                >
                  <template #empty>Sense tarifes de transport.</template>
                  <Column field="supplierName" header="Proveïdor" />
                  <Column field="rateName" header="Tarifa" />
                  <Column field="description" header="Descripció" />
                  <Column header="Vàlid des de">
                    <template #body="{ data }">{{
                      formatDate(data.validFrom)
                    }}</template>
                  </Column>
                  <Column header="Vàlid fins a">
                    <template #body="{ data }">{{
                      formatDate(data.validTo)
                    }}</template>
                  </Column>
                </DataTable>
              </TabPanel>

              <TabPanel value="purchaseHistory">
                <DataTable
                  :value="purchaseHistoryRows"
                  tableStyle="min-width: 100%"
                  scrollable
                  scrollHeight="flex"
                  :loading="purchaseLoading"
                  paginator
                  :rows="10"
                >
                  <template #empty>Sense compres registrades.</template>
                  <Column field="date" header="Data">
                    <template #body="{ data }">{{
                      formatDate(data.date)
                    }}</template>
                  </Column>
                  <Column field="number" header="Albarà" />
                  <Column field="supplierName" header="Proveïdor" />
                  <Column field="quantity" header="Quantitat" />
                  <Column field="unitPrice" header="Preu unitari">
                    <template #body="{ data }">{{
                      formatCurrency(data.unitPrice)
                    }}</template>
                  </Column>
                  <Column field="amount" header="Import">
                    <template #body="{ data }">{{
                      formatCurrency(data.amount)
                    }}</template>
                  </Column>
                </DataTable>
              </TabPanel>
            </TabPanels>
          </Tabs>
        </TabPanel>

        <!-- ============ PRODUCCIÓ ============ -->
        <TabPanel value="production" v-if="reference.production">
          <DataTable
            :value="workMasters"
            tableStyle="min-width: 100%"
            scrollable
            scrollHeight="flex"
            :loading="productionLoading"
            @row-click="openWorkMaster"
          >
            <template #header>
              <label class="block text-900">Rutes de fabricació</label>
            </template>
            <template #empty>Sense rutes de fabricació.</template>
            <Column field="baseQuantity" header="Quantitat Base" />
            <Column field="machineCost" header="Cost màquina">
              <template #body="{ data }">{{
                formatCurrency(data.machineCost)
              }}</template>
            </Column>
            <Column field="operatorCost" header="Cost operari">
              <template #body="{ data }">{{
                formatCurrency(data.operatorCost)
              }}</template>
            </Column>
            <Column field="materialCost" header="Cost material">
              <template #body="{ data }">{{
                formatCurrency(data.materialCost)
              }}</template>
            </Column>
            <Column field="externalCost" header="Cost extern">
              <template #body="{ data }">{{
                formatCurrency(data.externalCost)
              }}</template>
            </Column>
            <Column header="Cost total">
              <template #body="{ data }">{{
                formatCurrency(workMasterTotal(data))
              }}</template>
            </Column>
          </DataTable>

          <DataTable
            :value="workOrders"
            tableStyle="min-width: 100%"
            scrollable
            scrollHeight="flex"
            :loading="productionLoading"
            paginator
            :rows="10"
            class="mt-4"
            @row-click="openWorkOrder"
          >
            <template #header>
              <label class="block text-900">Ordres de fabricació (OFs)</label>
            </template>
            <template #empty>Sense ordres de fabricació.</template>
            <Column field="code" header="Codi" />
            <Column field="plannedDate" header="Data planificada">
              <template #body="{ data }">{{
                formatDate(data.plannedDate)
              }}</template>
            </Column>
            <Column field="plannedQuantity" header="Qty. planificada" />
            <Column field="totalQuantity" header="Qty. fabricada" />
            <Column field="operatorCost" header="Cost operari">
              <template #body="{ data }">{{
                formatCurrency(data.operatorCost)
              }}</template>
            </Column>
            <Column field="machineCost" header="Cost màquina">
              <template #body="{ data }">{{
                formatCurrency(data.machineCost)
              }}</template>
            </Column>
            <Column field="materialCost" header="Cost material">
              <template #body="{ data }">{{
                formatCurrency(data.materialCost)
              }}</template>
            </Column>
            <Column header="Cost total OF">
              <template #body="{ data }">{{
                formatCurrency(workOrderTotal(data))
              }}</template>
            </Column>
          </DataTable>
        </TabPanel>

        <!-- ============ MAGATZEM ============ -->
        <TabPanel value="warehouse">
          <DataTable
            :value="stock"
            tableStyle="min-width: 100%"
            scrollable
            scrollHeight="flex"
            :loading="stockLoading"
            paginator
            :rows="10"
          >
            <template #header>
              <label class="block text-900">Estoc per ubicació</label>
            </template>
            <template #empty>Sense estoc.</template>
            <Column field="warehouseName" header="Magatzem" />
            <Column field="locationName" header="Ubicació" />
            <Column field="quantity" header="Quantitat" />
            <Column field="width" header="Ample" />
            <Column field="length" header="Llarg" />
            <Column field="height" header="Alt" />
            <Column field="diameter" header="Diàmetre" />
            <Column field="thickness" header="Gruix" />
          </DataTable>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <!-- Supplier dialog -->
    <Dialog
      v-model:visible="supplierDialogVisible"
      modal
      header="Proveïdor"
      :style="{ width: '32rem' }"
    >
      <div v-if="editingSupplier" class="flex flex-column gap-3">
        <div>
          <label class="block text-900 mb-2">Proveïdor</label>
          <Select
            v-model="editingSupplier.supplierId"
            :options="suppliersStore.suppliers"
            optionValue="id"
            optionLabel="comercialName"
            class="w-full"
            filter
          />
        </div>
        <BaseInput
          label="Codi proveïdor"
          id="supplierCode"
          v-model="editingSupplier.supplierCode"
        />
        <BaseInput
          label="Descripció proveïdor"
          id="supplierDescription"
          v-model="editingSupplier.supplierDescription"
        />
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Preu"
          id="supplierPrice"
          v-model="editingSupplier.supplierPrice"
        />
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Dies d'entrega"
          id="supplyDays"
          v-model="editingSupplier.supplyDays"
        />
      </div>
      <template #footer>
        <Button
          label="Cancel·lar"
          text
          @click="supplierDialogVisible = false"
        />
        <Button label="Guardar" :icon="PrimeIcons.SAVE" @click="saveSupplier" />
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";

import { useStore } from "../../../store";
import { FormActionMode, BaseInputType } from "../../../types/component";
import { formatCurrency, formatDate, getNewUuid } from "../../../utils/functions";

import BaseInput from "../../../components/BaseInput.vue";
import FileEntityPicker from "../../../components/FileEntityPicker.vue";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import DropdownReferenceType from "../components/DropdownReferenceType.vue";

import { useReferenceStore } from "../store/reference";
import { useTaxesStore } from "../store/tax";
import { useSuppliersStore } from "../../purchase/store/suppliers";
import { useCustomersStore } from "../../sales/store/customers";

import { Reference, ReferenceCategoryEnum } from "../types";
import { SupplierReference } from "../../purchase/types";
import { DeliveryNote } from "../../sales/types";
import {
  Receipt,
  PurchaseRate,
  TransportRate,
  CalculationType,
} from "../../purchase/types";
import { WorkMaster, WorkOrder } from "../../production/types";
import { StockListItem } from "../../warehouse/types";

import { DeliveryNoteService } from "../../sales/services/deliveryNote.service";
import { ReceiptService } from "../../purchase/services/receipt.service";
import { WorkOrderService } from "../../production/services/workorder.service";
import { WorkMasterService } from "../../production/services/workmaster.service";
import { StockService } from "../../warehouse/services/warehouse.service";
import { PurchaseRateService } from "../../purchase/services/purchaseRate.service";
import { TransportRateService } from "../../purchase/services/transportRate.service";

const route = useRoute();
const router = useRouter();
const toast = useToast();
const confirm = useConfirm();

const store = useStore();
const referenceStore = useReferenceStore();
const taxesStore = useTaxesStore();
const suppliersStore = useSuppliersStore();
const customersStore = useCustomersStore();

const deliveryNoteService = new DeliveryNoteService("/DeliveryNote");
const receiptService = new ReceiptService("/Receipt");
const workOrderService = new WorkOrderService("/WorkOrder");
const workMasterService = new WorkMasterService("/WorkMaster");
const stockService = new StockService("/Stock");
const purchaseRateService = new PurchaseRateService("/PurchaseRate");
const transportRateService = new TransportRateService("/TransportRate");

const { reference } = storeToRefs(referenceStore);
const id = ref("");

const loading = ref(false);
const salesLoading = ref(false);
const purchaseLoading = ref(false);
const productionLoading = ref(false);
const stockLoading = ref(false);
const suppliersLoading = ref(false);
const ratesLoading = ref(false);
const formMode = ref(FormActionMode.EDIT);

const salesHistory = ref<Array<DeliveryNote>>([]);
const purchaseHistory = ref<Array<Receipt>>([]);
const purchaseRates = ref<Array<PurchaseRate>>([]);
const transportRates = ref<Array<TransportRate>>([]);
const workMasters = ref<Array<WorkMaster>>([]);
const workOrders = ref<Array<WorkOrder>>([]);
const stock = ref<Array<StockListItem>>([]);

const isActive = computed({
  get: () => (reference.value ? !reference.value.disabled : true),
  set: (value: boolean) => {
    if (reference.value) reference.value.disabled = !value;
  },
});

const salesHistoryRows = computed(() =>
  salesHistory.value.flatMap((dn) =>
    dn.details
      .filter((d) => d.referenceId === id.value)
      .map((d) => ({
        date: dn.deliveryDate ?? dn.createdOn,
        number: dn.number,
        customerName: customersStore.getCustomerNameById(dn.customerId),
        quantity: d.quantity,
        unitPrice: d.unitPrice,
        amount: d.amount,
      }))
  )
);

const purchaseHistoryRows = computed(() =>
  purchaseHistory.value.flatMap((r) =>
    r.details
      .filter((d) => d.referenceId === id.value)
      .map((d) => ({
        date: r.date,
        number: r.number,
        supplierName: suppliersStore.getName(r.supplierId),
        quantity: d.quantity,
        unitPrice: d.unitPrice,
        amount: d.amount,
      }))
  )
);

const externalServiceRateRows = computed(() =>
  purchaseRates.value.flatMap((rate) =>
    (rate.details ?? [])
      .filter((d) => d.referenceId === id.value)
      .map((d) => ({
        supplierName: suppliersStore.getName(rate.supplierId),
        rateName: rate.name,
        validFrom: rate.validFrom,
        validTo: rate.validTo,
        calculationType: d.calculationType,
        from: d.from,
        to: d.to,
        price: d.price,
      }))
  )
);

const transportRateRows = computed(() =>
  transportRates.value.map((rate) => ({
    supplierName: suppliersStore.getName(rate.supplierId),
    rateName: rate.name,
    description: rate.description,
    validFrom: rate.validFrom,
    validTo: rate.validTo,
  }))
);

const getCalculationTypeLabel = (type: CalculationType) => {
  switch (type) {
    case CalculationType.Volume:
      return "Volum";
    case CalculationType.Weight:
      return "Pes";
    case CalculationType.Units:
      return "Unitats";
    default:
      return "";
  }
};

const workMasterTotal = (wm: WorkMaster) =>
  (wm.machineCost ?? 0) +
  (wm.operatorCost ?? 0) +
  (wm.materialCost ?? 0) +
  (wm.externalCost ?? 0);

const workOrderTotal = (wo: WorkOrder) =>
  (wo.operatorCost ?? 0) + (wo.machineCost ?? 0) + (wo.materialCost ?? 0);

const loadGeneral = async () => {
  await Promise.all([
    taxesStore.fetchAll(),
    referenceStore.fetchReference(id.value),
    referenceStore.referenceFormats
      ? Promise.resolve()
      : referenceStore.fetchReferences(),
  ]);
};

const loadRelated = async () => {
  if (!reference.value) return;
  const refId = reference.value.id;

  // Lookups for name resolution
  if (!customersStore.customers) await customersStore.fetchCustomers();
  if (!suppliersStore.suppliers) await suppliersStore.fetchSuppliers();

  if (reference.value.sales) {
    salesLoading.value = true;
    salesHistory.value =
      (await deliveryNoteService.GetByReferenceId(refId)) ?? [];
    salesLoading.value = false;
  }

  if (reference.value.purchase) {
    suppliersLoading.value = true;
    purchaseLoading.value = true;
    ratesLoading.value = true;
    await referenceStore.fetchReferenceSuppliers(refId);
    suppliersLoading.value = false;
    purchaseHistory.value =
      (await receiptService.GetByReferenceId(refId)) ?? [];
    purchaseLoading.value = false;

    purchaseRates.value =
      (await purchaseRateService.getByReferenceId(refId)) ?? [];

    const supplierIds = [
      ...new Set(
        (referenceStore.referenceSuppliers ?? []).map((s) => s.supplierId)
      ),
    ];
    const transportResults = await Promise.all(
      supplierIds.map((sid) => transportRateService.getBySupplierId(sid))
    );
    transportRates.value = transportResults.flatMap((r) => r ?? []);
    ratesLoading.value = false;
  }

  if (reference.value.production) {
    productionLoading.value = true;
    const [wm, wo] = await Promise.all([
      workMasterService.getByReferenceId(refId),
      workOrderService.GetBetweenDatesAndStatus(
        "2000-01-01",
        "2999-12-31",
        undefined,
        refId
      ),
    ]);
    workMasters.value = wm ?? [];
    workOrders.value = wo ?? [];
    productionLoading.value = false;
  }

  stockLoading.value = true;
  stock.value = await stockService.getByReference(refId);
  stockLoading.value = false;
};

const loadView = async () => {
  loading.value = true;
  try {
    await loadGeneral();

    let pageTitle = "";
    if (!reference.value) {
      formMode.value = FormActionMode.CREATE;
      referenceStore.setNewReference(id.value, ReferenceCategoryEnum.PRODUCT);
      pageTitle = "Alta de referència";
    } else {
      formMode.value = FormActionMode.EDIT;
      pageTitle = `Referència ${reference.value.code} - ${reference.value.description}`;
      await loadRelated();
    }

    store.setMenuItem({
      icon: PrimeIcons.BOX,
      backButtonVisible: true,
      title: pageTitle,
    });
  } catch (error) {
    console.error("Error loading view:", error);
    toast.add({
      severity: "error",
      summary: "Error al carregar la vista",
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  id.value = route.params.id as string;
  await loadView();
});

onUnmounted(() => {
  referenceStore.reference = undefined;
  referenceStore.referenceSuppliers = undefined;
});

const submitGeneral = async () => {
  const data = reference.value as Reference;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await referenceStore.createReference(data);
    message = result
      ? "Referència creada correctament"
      : "La referència + versió introduïda ja existeix";
  } else {
    result = await referenceStore.updateReference(data.id, data);
    message = result
      ? "Referència actualitzada correctament"
      : "No s'ha pogut actualitzar la referència";
  }

  toast.add({
    severity: result ? "success" : "warn",
    summary: message,
    life: 5000,
  });

  if (result && formMode.value === FormActionMode.CREATE) {
    router.replace({ path: `/reference-management/${data.id}` });
    await loadView();
  }
};

// Suppliers
const supplierDialogVisible = ref(false);
const editingSupplier = ref<SupplierReference | null>(null);

const newSupplier = () => {
  editingSupplier.value = {
    id: getNewUuid(),
    referenceId: id.value,
    supplierId: "",
    supplierCode: reference.value?.code ?? "",
    supplierDescription: reference.value?.description ?? "",
    supplierPrice: 0,
    supplyDays: 0,
    disabled: false,
  } as SupplierReference;
  supplierDialogVisible.value = true;
};

const editSupplier = (data: SupplierReference) => {
  editingSupplier.value = { ...data };
  supplierDialogVisible.value = true;
};

const saveSupplier = async () => {
  if (!editingSupplier.value) return;
  if (!editingSupplier.value.supplierId) {
    toast.add({
      severity: "warn",
      summary: "Selecciona un proveïdor",
      life: 4000,
    });
    return;
  }

  const exists = referenceStore.referenceSuppliers?.some(
    (s) => s.id === editingSupplier.value!.id
  );
  const ok = exists
    ? await referenceStore.updateSupplier(editingSupplier.value)
    : await referenceStore.addSupplier(editingSupplier.value);

  toast.add({
    severity: ok ? "success" : "warn",
    summary: ok ? "Proveïdor guardat" : "No s'ha pogut guardar el proveïdor",
    life: 4000,
  });
  if (ok) supplierDialogVisible.value = false;
};

const removeSupplier = (data: SupplierReference) => {
  confirm.require({
    message: "Està segur que vol eliminar el proveïdor seleccionat?",
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const ok = await referenceStore.deleteSupplier(data);
      toast.add({
        severity: ok ? "success" : "warn",
        summary: ok ? "Proveïdor eliminat" : "No s'ha pogut eliminar",
        life: 4000,
      });
    },
  });
};

const openWorkMaster = (row: DataTableRowClickEvent) => {
  router.push({ path: `/workmaster/${row.data.id}` });
};

const openWorkOrder = (row: DataTableRowClickEvent) => {
  router.push({ path: `/workorder/${row.data.id}` });
};
</script>
