<template>
  <div v-if="!loading && reference">
    <Tabs value="general">
      <TabList>
        <Tab value="general">
          <i :class="PrimeIcons.INFO_CIRCLE" class="mr-2" />{{ $t('shared.referenceManagement.tabs.general') }}
        </Tab>
        <Tab
          value="sales"
          v-if="formMode === FormActionMode.EDIT && reference.sales"
        >
          <i :class="PrimeIcons.SHOPPING_CART" class="mr-2" />{{ $t('shared.referenceManagement.tabs.sales') }}
        </Tab>
        <Tab
          value="purchase"
          v-if="formMode === FormActionMode.EDIT && reference.purchase"
        >
          <i :class="PrimeIcons.TRUCK" class="mr-2" />{{ $t('shared.referenceManagement.tabs.purchase') }}
        </Tab>
        <Tab
          value="production"
          v-if="formMode === FormActionMode.EDIT && reference.production"
        >
          <i :class="PrimeIcons.COG" class="mr-2" />{{ $t('shared.referenceManagement.tabs.production') }}
        </Tab>
        <Tab value="warehouse" v-if="formMode === FormActionMode.EDIT">
          <i :class="PrimeIcons.BUILDING" class="mr-2" />{{ $t('shared.referenceManagement.tabs.warehouse') }}
        </Tab>
      </TabList>

      <TabPanels>
        <!-- ============ GENERAL ============ -->
        <TabPanel value="general">
          <div class="flex justify-content-end mb-2">
            <Button
              :label="$t('shared.referenceManagement.general.save')"
              size="small"
              :icon="PrimeIcons.SAVE"
              @click="submitGeneral"
            />
          </div>
          <form>
            <section class="five-columns">
              <div class="mt-1">
                <BaseInput :label="$t('shared.referenceManagement.general.code')" id="code" v-model="reference.code" />
              </div>
              <div class="mt-1">
                <BaseInput
                  :label="$t('shared.referenceManagement.general.description')"
                  id="description"
                  v-model="reference.description"
                />
              </div>
              <div class="mt-1">
                <BaseInput
                  :label="$t('shared.referenceManagement.general.version')"
                  id="version"
                  v-model="reference.version"
                />
              </div>
              <div class="mt-1">
                <DropdownReferenceType
                  :label="$t('shared.referenceManagement.general.materialType')"
                  v-model="reference.referenceTypeId"
                />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.format') }}</label>
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
                  :label="$t('shared.referenceManagement.general.client')"
                  v-model="reference.customerId"
                />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.tax') }}</label>
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
                  :label="$t('shared.referenceManagement.general.theoreticalCost')"
                  id="workMasterCost"
                  v-model="reference.workMasterCost"
                  disabled
                />
              </div>
              <div class="mt-1">
                <BaseInput
                  :type="BaseInputType.CURRENCY"
                  :label="$t('shared.referenceManagement.general.lastCost')"
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
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.active') }}</label>
                <Checkbox v-model="isActive" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.sales') }}</label>
                <Checkbox v-model="reference.sales" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.purchase') }}</label>
                <Checkbox v-model="reference.purchase" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.production') }}</label>
                <Checkbox v-model="reference.production" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.service') }}</label>
                <Checkbox v-model="reference.isService" :binary="true" />
              </div>
              <div class="mt-1">
                <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.general.requiresLot') }}</label>
                <Checkbox v-model="reference.requiresLot" :binary="true" />
              </div>
            </section>
          </form>

          <div v-if="formMode === FormActionMode.EDIT" class="mt-4">
            <FileEntityPicker
              :title="$t('shared.referenceManagement.general.documentation')"
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
                :label="$t('shared.referenceManagement.sales.savePvp')"
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
              <label class="block text-900">{{ $t('shared.referenceManagement.sales.salesHistory') }}</label>
            </template>
            <template #empty>{{ $t('shared.referenceManagement.sales.noSales') }}</template>
            <Column field="date" :header="$t('shared.referenceManagement.sales.columns.date')">
              <template #body="{ data }">{{ formatDate(data.date) }}</template>
            </Column>
            <Column field="number" :header="$t('shared.referenceManagement.sales.columns.deliveryNote')" />
            <Column field="customerName" :header="$t('shared.referenceManagement.sales.columns.client')" />
            <Column field="quantity" :header="$t('shared.referenceManagement.sales.columns.quantity')" />
            <Column field="unitPrice" :header="$t('shared.referenceManagement.sales.columns.unitPrice')">
              <template #body="{ data }">{{
                formatCurrency(data.unitPrice)
              }}</template>
            </Column>
                  <Column field="amount" :header="$t('shared.referenceManagement.purchase.purchaseHistoryTable.columns.amount')">
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
                :label="$t('shared.referenceManagement.sales.pucLabel')"
                id="puc"
                v-model="reference.lastCost"
                disabled
              />
            </div>
          </section>

          <Tabs value="suppliers">
            <TabList>
              <Tab value="suppliers">{{ $t('shared.referenceManagement.purchase.tabs.suppliers') }}</Tab>
              <Tab value="externalServices">{{ $t('shared.referenceManagement.purchase.tabs.externalServices') }}</Tab>
              <Tab value="transport">{{ $t('shared.referenceManagement.purchase.tabs.transport') }}</Tab>
              <Tab value="purchaseHistory">{{ $t('shared.referenceManagement.purchase.tabs.purchaseHistory') }}</Tab>
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
                      <label class="block text-900">{{ $t('shared.referenceManagement.purchase.suppliersTable.title') }}</label>
                      <Button
                        :icon="PrimeIcons.PLUS"
                        rounded
                        raised
                        @click="newSupplier"
                      />
                    </div>
                  </template>
                  <template #empty>{{ $t('shared.referenceManagement.purchase.suppliersTable.empty') }}</template>
                  <Column :header="$t('shared.referenceManagement.purchase.suppliersTable.columns.supplier')">
                    <template #body="{ data }">{{
                      suppliersStore.getName(data.supplierId)
                    }}</template>
                  </Column>
                  <Column field="supplierCode" :header="$t('shared.referenceManagement.purchase.suppliersTable.columns.supplierCode')" />
                  <Column
                    field="supplierDescription"
                    :header="$t('shared.referenceManagement.purchase.suppliersTable.columns.supplierDescription')"
                  />
                  <Column field="supplierPrice" :header="$t('shared.referenceManagement.purchase.suppliersTable.columns.price')">
                    <template #body="{ data }">{{
                      formatCurrency(data.supplierPrice)
                    }}</template>
                  </Column>
                  <Column field="supplyDays" :header="$t('shared.referenceManagement.purchase.suppliersTable.columns.deliveryDays')" />
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
                  <template #empty>{{ $t('shared.referenceManagement.purchase.externalServicesTable.empty') }}</template>
                  <Column field="supplierName" :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.supplier')" />
                  <Column field="rateName" :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.rate')" />
                  <Column :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.validFrom')">
                    <template #body="{ data }">{{
                      formatDate(data.validFrom)
                    }}</template>
                  </Column>
                  <Column :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.validTo')">
                    <template #body="{ data }">{{
                      formatDate(data.validTo)
                    }}</template>
                  </Column>
                  <Column :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.calculation')">
                    <template #body="{ data }">{{
                      getCalculationTypeLabel(data.calculationType)
                    }}</template>
                  </Column>
                  <Column field="from" :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.from')" />
                  <Column field="to" :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.to')" />
                  <Column field="price" :header="$t('shared.referenceManagement.purchase.externalServicesTable.columns.price')">
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
                  <template #empty>{{ $t('shared.referenceManagement.purchase.transportTable.empty') }}</template>
                  <Column field="supplierName" :header="$t('shared.referenceManagement.purchase.transportTable.columns.supplier')" />
                  <Column field="rateName" :header="$t('shared.referenceManagement.purchase.transportTable.columns.rate')" />
                  <Column field="description" :header="$t('shared.referenceManagement.purchase.transportTable.columns.description')" />
                  <Column :header="$t('shared.referenceManagement.purchase.transportTable.columns.validFrom')">
                    <template #body="{ data }">{{
                      formatDate(data.validFrom)
                    }}</template>
                  </Column>
                  <Column :header="$t('shared.referenceManagement.purchase.transportTable.columns.validTo')">
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
                  <template #empty>{{ $t('shared.referenceManagement.purchase.purchaseHistoryTable.empty') }}</template>
                  <Column field="date" :header="$t('shared.referenceManagement.purchase.purchaseHistoryTable.columns.date')">
                    <template #body="{ data }">{{
                      formatDate(data.date)
                    }}</template>
                  </Column>
                  <Column field="number" :header="$t('shared.referenceManagement.purchase.purchaseHistoryTable.columns.deliveryNote')" />
                  <Column field="supplierName" :header="$t('shared.referenceManagement.purchase.purchaseHistoryTable.columns.supplier')" />
                  <Column field="quantity" :header="$t('shared.referenceManagement.purchase.purchaseHistoryTable.columns.quantity')" />
                  <Column field="unitPrice" :header="$t('shared.referenceManagement.purchase.purchaseHistoryTable.columns.unitPrice')">
                    <template #body="{ data }">{{
                      formatCurrency(data.unitPrice)
                    }}</template>
                  </Column>
            <Column field="amount" :header="$t('shared.referenceManagement.sales.columns.amount')">
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
              <label class="block text-900">{{ $t('shared.referenceManagement.production.workMastersTitle') }}</label>
            </template>
            <template #empty>{{ $t('shared.referenceManagement.production.workMastersEmpty') }}</template>
            <Column field="baseQuantity" :header="$t('shared.referenceManagement.production.workMastersColumns.baseQuantity')" />
            <Column field="machineCost" :header="$t('shared.referenceManagement.production.workMastersColumns.machineCost')">
              <template #body="{ data }">{{
                formatCurrency(data.machineCost)
              }}</template>
            </Column>
            <Column field="operatorCost" :header="$t('shared.referenceManagement.production.workMastersColumns.operatorCost')">
              <template #body="{ data }">{{
                formatCurrency(data.operatorCost)
              }}</template>
            </Column>
            <Column field="materialCost" :header="$t('shared.referenceManagement.production.workMastersColumns.materialCost')">
              <template #body="{ data }">{{
                formatCurrency(data.materialCost)
              }}</template>
            </Column>
            <Column field="externalCost" :header="$t('shared.referenceManagement.production.workMastersColumns.externalCost')">
              <template #body="{ data }">{{
                formatCurrency(data.externalCost)
              }}</template>
            </Column>
            <Column :header="$t('shared.referenceManagement.production.workMastersColumns.totalCost')">
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
              <label class="block text-900">{{ $t('shared.referenceManagement.production.workOrdersTitle') }}</label>
            </template>
            <template #empty>{{ $t('shared.referenceManagement.production.workOrdersEmpty') }}</template>
            <Column field="code" :header="$t('shared.referenceManagement.production.workOrdersColumns.code')" />
            <Column field="plannedDate" :header="$t('shared.referenceManagement.production.workOrdersColumns.plannedDate')">
              <template #body="{ data }">{{
                formatDate(data.plannedDate)
              }}</template>
            </Column>
            <Column field="plannedQuantity" :header="$t('shared.referenceManagement.production.workOrdersColumns.plannedQuantity')" />
            <Column field="totalQuantity" :header="$t('shared.referenceManagement.production.workOrdersColumns.producedQuantity')" />
            <Column field="operatorCost" :header="$t('shared.referenceManagement.production.workOrdersColumns.operatorCost')">
              <template #body="{ data }">{{
                formatCurrency(data.operatorCost)
              }}</template>
            </Column>
            <Column field="machineCost" :header="$t('shared.referenceManagement.production.workOrdersColumns.machineCost')">
              <template #body="{ data }">{{
                formatCurrency(data.machineCost)
              }}</template>
            </Column>
            <Column field="materialCost" :header="$t('shared.referenceManagement.production.workOrdersColumns.materialCost')">
              <template #body="{ data }">{{
                formatCurrency(data.materialCost)
              }}</template>
            </Column>
            <Column :header="$t('shared.referenceManagement.production.workOrdersColumns.totalCost')">
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
              <label class="block text-900">{{ $t('shared.referenceManagement.warehouse.stockTitle') }}</label>
            </template>
            <template #empty>{{ $t('shared.referenceManagement.warehouse.stockEmpty') }}</template>
            <Column field="warehouseName" :header="$t('shared.referenceManagement.warehouse.stockColumns.warehouse')" />
            <Column field="locationName" :header="$t('shared.referenceManagement.warehouse.stockColumns.location')" />
            <Column field="quantity" :header="$t('shared.referenceManagement.warehouse.stockColumns.quantity')" />
            <Column field="width" :header="$t('shared.referenceManagement.warehouse.stockColumns.width')" />
            <Column field="length" :header="$t('shared.referenceManagement.warehouse.stockColumns.length')" />
            <Column field="height" :header="$t('shared.referenceManagement.warehouse.stockColumns.height')" />
            <Column field="diameter" :header="$t('shared.referenceManagement.warehouse.stockColumns.diameter')" />
            <Column field="thickness" :header="$t('shared.referenceManagement.warehouse.stockColumns.thickness')" />
          </DataTable>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <!-- Supplier dialog -->
    <Dialog
      v-model:visible="supplierDialogVisible"
      modal
      :header="$t('shared.referenceManagement.supplierDialog.title')"
      :style="{ width: '32rem' }"
    >
      <div v-if="editingSupplier" class="flex flex-column gap-3">
        <div>
          <label class="block text-900 mb-2">{{ $t('shared.referenceManagement.supplierDialog.supplierLabel') }}</label>
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
          :label="$t('shared.referenceManagement.supplierDialog.supplierCode')"
          id="supplierCode"
          v-model="editingSupplier.supplierCode"
        />
        <BaseInput
          :label="$t('shared.referenceManagement.supplierDialog.supplierDescription')"
          id="supplierDescription"
          v-model="editingSupplier.supplierDescription"
        />
        <BaseInput
          :type="BaseInputType.CURRENCY"
          :label="$t('shared.referenceManagement.supplierDialog.price')"
          id="supplierPrice"
          v-model="editingSupplier.supplierPrice"
        />
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="$t('shared.referenceManagement.supplierDialog.deliveryDays')"
          id="supplyDays"
          v-model="editingSupplier.supplyDays"
        />
      </div>
      <template #footer>
        <Button
          :label="$t('shared.referenceManagement.supplierDialog.cancel')"
          text
          @click="supplierDialogVisible = false"
        />
        <Button :label="$t('shared.referenceManagement.supplierDialog.save')" :icon="PrimeIcons.SAVE" @click="saveSupplier" />
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
import { useI18n } from "vue-i18n";
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
const { t } = useI18n();

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
      pageTitle = t("shared.referenceManagement.messages.newTitle");
    } else {
      formMode.value = FormActionMode.EDIT;
      pageTitle = t("shared.referenceManagement.messages.editTitle", {
        code: reference.value.code,
        description: reference.value.description,
      });
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
      summary: t("shared.referenceManagement.messages.loadError"),
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
      ? t("shared.referenceManagement.messages.referenceCreated")
      : t("shared.referenceManagement.messages.referenceExists");
  } else {
    result = await referenceStore.updateReference(data.id, data);
    message = result
      ? t("shared.referenceManagement.messages.referenceSaved")
      : t("shared.referenceManagement.messages.referenceSaveError");
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
      summary: t("shared.referenceManagement.messages.selectSupplier"),
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
    summary: ok ? t("shared.referenceManagement.messages.supplierSaved") : t("shared.referenceManagement.messages.supplierSaveError"),
    life: 4000,
  });
  if (ok) supplierDialogVisible.value = false;
};

const removeSupplier = (data: SupplierReference) => {
  confirm.require({
    message: t("shared.referenceManagement.messages.confirmDeleteSupplier"),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const ok = await referenceStore.deleteSupplier(data);
      toast.add({
        severity: ok ? "success" : "warn",
        summary: ok ? t("shared.referenceManagement.messages.supplierDeleted") : t("shared.referenceManagement.messages.supplierDeleteError"),
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
