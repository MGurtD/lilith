<template>
  <div>
    <form v-if="purchaseInvoice">
      <section class="four-columns">
        <BaseInput
          label="Num. Fra. Intern"
          id="number"
          v-model="purchaseInvoice.number"
          disabled
        />
        <div>
          <label class="block text-900 mb-2">Exercici</label>
          <Select
            v-model="purchaseInvoice.exerciceId"
            :options="purchaseMasterData.masterData.exercises"
            optionValue="id"
            optionLabel="name"
            class="w-full"
            :class="{
              'p-invalid': validation.errors.exerciseId,
            }"
          />
        </div>
        <div>
          <label class="block text-900 mb-2">Sèrie</label>
          <Select
            v-model="purchaseInvoice.purchaseInvoiceSerieId"
            :options="purchaseMasterData.masterData.series"
            optionValue="id"
            optionLabel="name"
            class="w-full"
            :class="{
              'p-invalid': validation.errors.purchaseInvoiceSerieId,
            }"
          />
        </div>
        <div>
          <DropdownLifecycleStatusTransitions
            label="Estat"
            :statusId="purchaseInvoice.statusId"
            v-model="purchaseInvoice.statusId"
            :class="{
              'p-invalid': validation.errors.statusId,
            }"
          />
        </div>
      </section>

      <section class="four-columns">
        <div class="mt-1">
          <label class="block text-900 mb-2">Proveïdor</label>
          <Select
            v-model="purchaseInvoice.supplierId"
            :options="purchaseMasterData.masterData.suppliers"
            optionValue="id"
            optionLabel="comercialName"
            @change="setSupplierPaymentMethod"
            class="w-full"
            :class="{
              'p-invalid': validation.errors.supplierId,
            }"
          />
        </div>
        <div class="mt-1">
          <BaseInput
            label="Num. Fra. Proveïdor"
            id="supplierNumber"
            v-model="purchaseInvoice.supplierNumber"
          />
        </div>
        <div class="mt-1">
          <label class="block text-900 mb-2">Data Factura</label>
          <DatePicker
            id="purchaseInvoiceDate"
            v-model="purchaseInvoice.purchaseInvoiceDate"
            @date-select="calcAmounts()"
          />
        </div>
        <div class="mt-1">
          <label class="block text-900 mb-2">Forma de pagament</label>
          <Select
            v-model="purchaseInvoice.paymentMethodId"
            :options="purchaseMasterData.masterData.paymentMethods"
            optionValue="id"
            optionLabel="name"
            class="w-full"
            :class="{
              'p-invalid': validation.errors.paymentmethod,
            }"
            @update:modelValue="calcAmounts()"
          />
        </div>
      </section>

      <section class="four-columns">
        <div class="mt-1">
          <BaseInput
            :type="BaseInputType.CURRENCY"
            label="Ports"
            id="transportAmount"
            v-model="purchaseInvoice.transportAmount"
            @update:modelValue="calcAmounts()"
          />
        </div>
        <div class="mt-1">
          <BaseInput
            :type="BaseInputType.NUMERIC"
            label="% IRPF"
            id="extraTaxPercentatge"
            v-model="purchaseInvoice.extraTaxPercentatge"
            @update:modelValue="calcAmounts()"
          />
        </div>
        <div class="mt-1">
          <BaseInput
            :type="BaseInputType.NUMERIC"
            label="% Dto."
            id="discountPercentage"
            v-model="purchaseInvoice.discountPercentage"
            @update:modelValue="calcAmounts()"
          />
        </div>
      </section>

      <section class="four-columns">
        <div class="mt-1">
          <label class="block text-900 mb-2">Base</label>
          <span class="summary-field">{{ purchaseInvoice.baseAmount }} €</span>
        </div>

        <div class="mt-1">
          <label class="block text-900 mb-2">Impostos</label>
          <span class="summary-field">{{ purchaseInvoice.taxAmount }} €</span>
        </div>

        <div class="mt-1">
          <label class="block text-900 mb-2">Total</label>
          <span class="summary-field">{{ purchaseInvoice.netAmount }} €</span>
        </div>
      </section>
    </form>
  </div>
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "@/components/BaseInput.vue";
import DropdownLifecycleStatusTransitions from "@/modules/shared/components/DropdownLifecycleStatusTransitions.vue";
import { usePurchaseInvoiceStore } from "../store/purchaseInvoices";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { PurchaseInvoice, PurchaseInvoiceDueDate } from "../types";
import * as Yup from "yup";
import { FormValidation, FormValidationResult } from "@/utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { BaseInputType } from "@/types/component";
import { convertDateTimeToJSON } from "@/utils/functions";
const emit = defineEmits<{
  (e: "submit", purchaseInvoice: PurchaseInvoice): void;
  (e: "cancel"): void;
}>();

const purchaseStore = usePurchaseInvoiceStore();
const purchaseMasterData = usePurchaseMasterDataStore();
const { purchaseInvoice } = storeToRefs(purchaseStore);

onMounted(() => {
  setTimeout(() => {
    if (
      purchaseInvoice.value &&
      purchaseInvoice.value.purchaseInvoiceImports.length > 0
    ) {
      purchaseInvoice.value.taxAmount = getTaxAmountFromImports();
    }
  }, 500);
});

const schema = Yup.object().shape({
  exerciceId: Yup.string().required("L'exercici es obligatori"),
  supplierId: Yup.string().required("El proveïdor es obligatori"),
  statusId: Yup.string().required("L'estat és obligatori"),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(purchaseInvoice.value);
};

const validateImports = () => {
  if (purchaseInvoice.value?.purchaseInvoiceImports.length === 0) {
    toast.add({
      severity: "warn",
      summary: "Formulari inválid",
      detail: "Ha d'introduir els imports de factura",
      life: 5000,
    });
    return false;
  }
  return true;
};

let hasBeenMounted = false;
onMounted(() => {
  setTimeout(() => {
    hasBeenMounted = true;
  }, 500);
});

const toast = useToast();
const submitForm = async () => {
  validate();
  if (validation.value.result) {
    if (!validateImports()) return;

    emit("submit", purchaseInvoice.value!);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};

const setSupplierPaymentMethod = () => {
  const supplier = purchaseMasterData.masterData.suppliers?.find(
    (s) => s.id === purchaseInvoice.value?.supplierId,
  );
  if (supplier) {
    purchaseInvoice.value!.paymentMethodId = supplier.paymentMethodId;
    calcAmounts();
  }
};

const calcAmounts = async () => {
  if (!hasBeenMounted) return;

  if (purchaseInvoice.value) {
    let baseAmount: number = 0;
    let transportAmount: number = 0;
    let subtotal: number = 0;
    let taxAmount: number = 0;
    let netAmount: number = 0;
    let discountAmount: number = 0;
    let discountPercentage: number = 0;
    let grossAmount: number = 0;
    let extraTaxPercentage: number = 0;
    let extraTaxAmount: number = 0;

    baseAmount = getBaseAmountFromImports();
    taxAmount = getTaxAmountFromImports();

    if (purchaseInvoice.value.transportAmount) {
      transportAmount = parseFloat(
        purchaseInvoice.value.transportAmount.toFixed(2),
      );
    }
    if (purchaseInvoice.value.discountPercentage) {
      discountPercentage = parseFloat(
        purchaseInvoice.value.discountPercentage.toFixed(2),
      );
    }
    if (purchaseInvoice.value.extraTaxPercentatge) {
      extraTaxPercentage = purchaseInvoice.value.extraTaxPercentatge;
    }

    subtotal = (baseAmount + transportAmount) * 1;
    extraTaxAmount = subtotal * (extraTaxPercentage / 100);
    grossAmount = subtotal + taxAmount * 1 - extraTaxAmount * 1;
    discountAmount = (grossAmount * (1 * discountPercentage)) / 100;
    netAmount = parseFloat((grossAmount - discountAmount).toFixed(2));

    purchaseInvoice.value.baseAmount = baseAmount;
    purchaseInvoice.value.transportAmount = transportAmount;
    purchaseInvoice.value.subtotal = subtotal;
    purchaseInvoice.value.taxAmount = taxAmount;
    purchaseInvoice.value.grossAmount = grossAmount;
    purchaseInvoice.value.netAmount = netAmount;
    purchaseInvoice.value.discountPercentage = discountPercentage;
    purchaseInvoice.value.discountAmount = discountAmount;
    purchaseInvoice.value.extraTaxPercentatge = extraTaxPercentage;
    purchaseInvoice.value.extraTaxAmount = extraTaxAmount;

    const invoice = Object.assign({}, purchaseInvoice.value);
    invoice.purchaseInvoiceDate = convertDateTimeToJSON(
      invoice.purchaseInvoiceDate,
    );

    // Calcular venciments
    purchaseInvoice.value.purchaseInvoiceDueDates =
      (await purchaseStore.GetDueDates(
        invoice,
      )) as Array<PurchaseInvoiceDueDate>;
  }
};

const getBaseAmountFromImports = (): number => {
  let baseAmount = 0;
  if (purchaseInvoice.value) {
    purchaseInvoice.value.purchaseInvoiceImports.forEach(
      (i) => (baseAmount += i.baseAmount!),
    );
  }
  return baseAmount;
};

const getTaxAmountFromImports = (): number => {
  let taxAmount = 0;
  if (purchaseInvoice.value) {
    purchaseInvoice.value.purchaseInvoiceImports.forEach(
      (i) => (taxAmount += i.taxAmount),
    );
  }
  return taxAmount;
};

// Methods exposed for the parent components
defineExpose({
  submitForm,
  calcAmounts,
});
</script>
<style scoped>
.save_button {
  position: absolute;
  top: 0;
  right: 1rem;
}
</style>
