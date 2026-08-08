<template>
  <div>
    <form v-if="salesOrder">
      <section class="four-columns mt-2">
        <div>
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('sales.components.numComanda')"
            id="salesOrderNumber"
            v-model="salesOrder.number"
            disabled
          />
        </div>
        <div>
          <label class="block text-900 mb-2">{{ t('sales.components.client') }}</label>
          <div style="display: flex; align-items: center; gap: 0.5rem">
            <Select
              v-model="salesOrder.customerId"
              :options="customerStore.customers"
              optionValue="id"
              optionLabel="comercialName"
              class="w-full"
              :class="{
                'p-invalid': validation.errors.customerId,
              }"
              @update:modelValue="updateCustomer()"
            />
            <router-link
              v-if="salesOrder.customerId"
              :to="`/customers/${salesOrder.customerId}`"
              style="color: inherit"
            >
              <i class="pi pi-search"></i>
            </router-link>
          </div>
        </div>
        <div>
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('sales.components.comandaClient')"
            id="customerSalesOrderNumber"
            v-model="salesOrder.customerNumber"
          />
        </div>
        <div>
          <DropdownLifecycleStatusTransitions
            :label="t('sales.components.estat')"
            :statusId="salesOrder.statusId"
            v-model="salesOrder.statusId"
            :class="{
              'p-invalid': validation.errors.statusId,
            }"
          />
        </div>
      </section>
      <section class="four-columns mt-2">
        <div>
          <label class="block text-900 mb-2">{{ t('sales.components.dataAlta') }}</label>
          <DatePicker v-model="salesOrder.date" dateFormat="dd/mm/yy" />
        </div>
        <div>
          <label class="block text-900 mb-2">{{ t('sales.components.dataEntrega') }}</label>
          <DatePicker v-model="salesOrder.expectedDate" dateFormat="dd/mm/yy" />
        </div>
        <div>
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('sales.components.numPressupost')"
            id="budgetNumber"
            disabled
            :modelValue="budgetStore.budget?.number ?? ''"
          />
        </div>

        <div>
          <BaseInput
            :type="BaseInputType.TEXT"
            :disabled="true"
            :label="t('sales.components.albaraEntrega')"
            id="deliveryNote"
            :modelValue="deliveryNoteNumber"
          />
        </div>
      </section>
    </form>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { computed, ref } from "vue";
import { storeToRefs } from "pinia";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useSalesOrderStore } from "../store/order";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useCustomersStore } from "../store/customers";
import { useDeliveryNoteStore } from "../store/deliveryNote";
import { SalesOrderHeader } from "../types";
import { BaseInputType } from "../../../types/component";
import { convertDateTimeToJSON } from "../../../utils/functions";
import { useBudgetStore } from "../store/budget";

const { t } = useI18n();
const emit = defineEmits<{
  (e: "submit", salesOrder: SalesOrderHeader): void;
  (e: "cancel"): void;
}>();

const salesOrderStore = useSalesOrderStore();
const customerStore = useCustomersStore();
const deliveryNoteStore = useDeliveryNoteStore();
const budgetStore = useBudgetStore();
const toast = useToast();

const { salesOrder } = storeToRefs(salesOrderStore);

const deliveryNoteNumber = computed(() => {
  return deliveryNoteStore.deliveryNote
    ? deliveryNoteStore.deliveryNote.number
    : "";
});

const schema = Yup.object().shape({
  siteId: Yup.string().required(t("sales.validation.siteRequired")),
  customerId: Yup.string().required(t("sales.validation.customerRequired")),
  statusId: Yup.string().required(t("sales.validation.statusRequired")),
  exerciseId: Yup.string().required(t("sales.validation.exerciseRequired")),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(salesOrder.value);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    parseEntityDates();
    emit("submit", salesOrder.value!);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t('sales.components.formulariInvalid'),
      detail: errors,
      life: 5000,
    });
  }
};

defineExpose({
  submitForm,
});

const updateCustomer = () => {
  const customer = customerStore.customers?.find(
    (c) => c.id === salesOrder.value?.customerId,
  );
  if (customer && salesOrder.value) {
    salesOrder.value.customerCode = customer.code;
    salesOrder.value.customerComercialName = customer.comercialName;
    salesOrder.value.customerTaxName = customer.taxName;
    salesOrder.value.customerVatNumber = customer.vatNumber;
    salesOrder.value.customerAccountNumber = customer.accountNumber;
  }
};

const parseEntityDates = () => {
  if (!salesOrder.value) return;

  salesOrder.value.date = convertDateTimeToJSON(salesOrder.value.date);

  if (salesOrder.value.expectedDate) {
    salesOrder.value.expectedDate = convertDateTimeToJSON(
      salesOrder.value.expectedDate,
    );
  }
};
</script>
<style scoped>
.save_button {
  position: absolute;
  top: 0;
  right: 1rem;
}

.summary-field {
  font-weight: bold;
  border-bottom: 1px solid black;
}
</style>
