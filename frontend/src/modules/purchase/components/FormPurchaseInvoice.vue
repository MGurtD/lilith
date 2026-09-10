<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  dateValue,
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import DropdownLifecycleStatusTransitions from "@/modules/shared/components/DropdownLifecycleStatusTransitions.vue";
import { convertDateTimeToJSON, formatCurrency } from "@/utils/functions";
import { computed, onMounted, onUnmounted, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useToast } from "primevue/usetoast";
import * as Yup from "yup";
import { usePurchaseMasterDataStore } from "../store/purchase";
import { usePurchaseInvoiceStore } from "../store/purchaseInvoices";
import type {
  PurchaseInvoice,
  PurchaseInvoiceCalculatedValues,
  PurchaseInvoiceDueDate,
} from "../types";

type PurchaseInvoiceCalculationFormValues = PurchaseInvoiceCalculatedValues &
  Pick<
    PurchaseInvoice,
    "transportAmount" | "discountPercentage" | "extraTaxPercentatge"
  >;

const props = defineProps<{
  purchaseInvoice: PurchaseInvoice;
}>();

const emit = defineEmits<{
  (event: "submit", purchaseInvoice: PurchaseInvoice): void;
  (
    event: "calculated",
    values: Partial<PurchaseInvoiceCalculatedValues>,
  ): void;
  (event: "due-dates-change", dueDates: PurchaseInvoiceDueDate[]): void;
}>();

const purchaseStore = usePurchaseInvoiceStore();
const purchaseMasterData = usePurchaseMasterDataStore();
const toast = useToast();
const { t } = useI18n();
const form = shallowRef<{
  submit: () => void;
  setFieldValue: (name: string, value: unknown) => void;
  setValues: (values: FormValues) => void;
} | null>(null);
let calculationsReady = false;
let suppressCalculations = false;
let dueDateRequestSequence = 0;
let mountTimer: ReturnType<typeof setTimeout> | undefined;

const createInitialValues = (invoice: PurchaseInvoice): FormValues => ({
  id: invoice.id,
  number: invoice.number,
  exerciceId: invoice.exerciceId,
  purchaseInvoiceSerieId: invoice.purchaseInvoiceSerieId,
  statusId: invoice.statusId,
  supplierId: invoice.supplierId,
  supplierNumber: invoice.supplierNumber,
  purchaseInvoiceDate: invoice.purchaseInvoiceDate,
  paymentMethodId: invoice.paymentMethodId,
  transportAmount: invoice.transportAmount,
  extraTaxPercentatge: invoice.extraTaxPercentatge,
  discountPercentage: invoice.discountPercentage,
  baseAmount: invoice.baseAmount,
  subtotal: invoice.subtotal,
  taxAmount: invoice.taxAmount,
  grossAmount: invoice.grossAmount,
  netAmount: invoice.netAmount,
  discountAmount: invoice.discountAmount,
  extraTaxAmount: invoice.extraTaxAmount,
});

const initialValues = shallowRef<FormValues>(
  createInitialValues(props.purchaseInvoice),
);
const latestCalculationValues = shallowRef<FormValues>({
  ...initialValues.value,
});

watch(
  () => props.purchaseInvoice,
  (invoice) => {
    dueDateRequestSequence += 1;
    initialValues.value = createInitialValues(invoice);
    latestCalculationValues.value = { ...initialValues.value };
  },
);

const numberProps = {
  locale: "en-US",
  minFractionDigits: 0,
} as const;
const currencyProps = {
  currency: "EUR",
  locale: "en-US",
  minFractionDigits: 2,
} as const;

const getBaseAmountFromImports = (): number =>
  props.purchaseInvoice.purchaseInvoiceImports.reduce(
    (total, item) => total + (item.baseAmount ?? 0),
    0,
  );

const getTaxAmountFromImports = (): number =>
  props.purchaseInvoice.purchaseInvoiceImports.reduce(
    (total, item) => total + item.taxAmount,
    0,
  );

const invoiceValues = (values: Readonly<FormValues>): PurchaseInvoice => ({
  ...props.purchaseInvoice,
  number: stringValue(values.number, props.purchaseInvoice.number),
  exerciceId: stringValue(
    values.exerciceId,
    props.purchaseInvoice.exerciceId,
  ),
  purchaseInvoiceSerieId: stringValue(
    values.purchaseInvoiceSerieId,
    props.purchaseInvoice.purchaseInvoiceSerieId,
  ),
  statusId: stringValue(values.statusId, props.purchaseInvoice.statusId),
  supplierId: stringValue(values.supplierId, props.purchaseInvoice.supplierId),
  supplierNumber: stringValue(
    values.supplierNumber,
    props.purchaseInvoice.supplierNumber,
  ),
  purchaseInvoiceDate: dateValue(
    values.purchaseInvoiceDate,
    props.purchaseInvoice.purchaseInvoiceDate,
  ),
  paymentMethodId: stringValue(
    values.paymentMethodId,
    props.purchaseInvoice.paymentMethodId,
  ),
  transportAmount: finiteNumberValue(
    values.transportAmount,
    props.purchaseInvoice.transportAmount,
  ),
  extraTaxPercentatge: finiteNumberValue(
    values.extraTaxPercentatge,
    props.purchaseInvoice.extraTaxPercentatge,
  ),
  discountPercentage: finiteNumberValue(
    values.discountPercentage,
    props.purchaseInvoice.discountPercentage,
  ),
  baseAmount: finiteNumberValue(
    values.baseAmount,
    props.purchaseInvoice.baseAmount,
  ),
  subtotal: finiteNumberValue(values.subtotal, props.purchaseInvoice.subtotal),
  taxAmount: finiteNumberValue(
    values.taxAmount,
    props.purchaseInvoice.taxAmount,
  ),
  grossAmount: finiteNumberValue(
    values.grossAmount,
    props.purchaseInvoice.grossAmount,
  ),
  netAmount: finiteNumberValue(
    values.netAmount,
    props.purchaseInvoice.netAmount,
  ),
  discountAmount: finiteNumberValue(
    values.discountAmount,
    props.purchaseInvoice.discountAmount,
  ),
  extraTaxAmount: finiteNumberValue(
    values.extraTaxAmount,
    props.purchaseInvoice.extraTaxAmount,
  ),
});

const applyCalculatedValues = (
  values: Readonly<FormValues>,
  formValues: PurchaseInvoiceCalculationFormValues,
  calculated: PurchaseInvoiceCalculatedValues,
): FormValues => {
  suppressCalculations = true;
  try {
    form.value?.setValues(formValues);
  } finally {
    suppressCalculations = false;
  }
  const nextValues = { ...values, ...formValues };
  latestCalculationValues.value = nextValues;
  emit("calculated", calculated);
  return nextValues;
};

const calculateAmounts = async (
  values: Readonly<FormValues> = latestCalculationValues.value,
): Promise<void> => {
  latestCalculationValues.value = { ...values };
  if (!calculationsReady) return;

  const requestSequence = ++dueDateRequestSequence;
  const baseAmount = getBaseAmountFromImports();
  const taxAmount = getTaxAmountFromImports();
  const rawTransportAmount = finiteNumberValue(values.transportAmount, 0);
  const rawDiscountPercentage = finiteNumberValue(
    values.discountPercentage,
    0,
  );
  const extraTaxPercentatge = finiteNumberValue(
    values.extraTaxPercentatge,
    0,
  );
  const transportAmount = rawTransportAmount
    ? Number(rawTransportAmount.toFixed(2))
    : 0;
  const discountPercentage = rawDiscountPercentage
    ? Number(rawDiscountPercentage.toFixed(2))
    : 0;
  const subtotal = baseAmount + transportAmount;
  const extraTaxAmount = subtotal * (extraTaxPercentatge / 100);
  const grossAmount = subtotal + taxAmount - extraTaxAmount;
  const discountAmount = (grossAmount * discountPercentage) / 100;
  const netAmount = Number((grossAmount - discountAmount).toFixed(2));
  const calculated: PurchaseInvoiceCalculatedValues = {
    baseAmount,
    subtotal,
    taxAmount,
    grossAmount,
    netAmount,
    discountAmount,
    extraTaxAmount,
  };
  const calculationFormValues: PurchaseInvoiceCalculationFormValues = {
    ...calculated,
    transportAmount,
    discountPercentage,
    extraTaxPercentatge,
  };
  const currentValues = applyCalculatedValues(
    values,
    calculationFormValues,
    calculated,
  );
  const invoice = invoiceValues(currentValues);
  const dueDateInvoice = {
    ...invoice,
    purchaseInvoiceDate: convertDateTimeToJSON(
      new Date(invoice.purchaseInvoiceDate),
    ),
  };
  const dueDates = await purchaseStore.GetDueDates(dueDateInvoice);
  if (requestSequence !== dueDateRequestSequence || !dueDates) return;

  emit("due-dates-change", dueDates);
};

const calculateOnChange = (
  _value: unknown,
  values: Readonly<FormValues>,
): void => {
  if (suppressCalculations) return;
  void calculateAmounts(values);
};

const updateSupplier = (
  _value: unknown,
  values: Readonly<FormValues>,
): void => {
  latestCalculationValues.value = { ...values };
  const supplierId = stringValue(values.supplierId, "");
  const supplier = purchaseMasterData.masterData.suppliers?.find(
    (item) => item.id === supplierId,
  );
  if (supplier) {
    form.value?.setFieldValue("paymentMethodId", supplier.paymentMethodId);
  }
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "number",
        label: t("purchase.fields.internalInvoiceNumber"),
        type: FormFieldType.Text,
        disabled: true,
      },
      {
        name: "exerciceId",
        label: t("purchase.fields.exercise"),
        type: FormFieldType.Select,
        props: {
          options: purchaseMasterData.masterData.exercises ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.validation.exerciseRequired"),
        ),
      },
      {
        name: "purchaseInvoiceSerieId",
        label: t("purchase.fields.series"),
        type: FormFieldType.Select,
        props: {
          options: purchaseMasterData.masterData.series ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
      },
      {
        name: "statusId",
        label: t("common.status"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("purchase.validation.statusRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "supplierId",
        label: t("purchase.fields.supplier"),
        type: FormFieldType.Select,
        props: {
          options: purchaseMasterData.masterData.suppliers ?? [],
          optionValue: "id",
          optionLabel: "comercialName",
        },
        onChange: updateSupplier,
        validation: Yup.string().required(
          t("purchase.validation.supplierRequired"),
        ),
      },
      {
        name: "supplierNumber",
        label: t("purchase.fields.supplierInvoiceNumber"),
        type: FormFieldType.Text,
      },
      {
        name: "purchaseInvoiceDate",
        label: t("purchase.fields.invoiceDate"),
        type: FormFieldType.Date,
        onChange: calculateOnChange,
      },
      {
        name: "paymentMethodId",
        label: t("purchase.fields.paymentMethod"),
        type: FormFieldType.Select,
        props: {
          options: purchaseMasterData.masterData.paymentMethods ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        onChange: calculateOnChange,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "transportAmount",
        label: t("purchase.fields.transportAmount"),
        type: FormFieldType.Currency,
        props: currencyProps,
        onChange: calculateOnChange,
      },
      {
        name: "extraTaxPercentatge",
        label: t("purchase.fields.withholdingTax"),
        type: FormFieldType.Number,
        props: numberProps,
        onChange: calculateOnChange,
      },
      {
        name: "discountPercentage",
        label: t("purchase.fields.discount"),
        type: FormFieldType.Number,
        props: numberProps,
        onChange: calculateOnChange,
      },
    ],
  },
  {
    section: "summary",
    fields: [
      { name: "baseAmount", label: "", type: FormFieldType.Custom },
      { name: "subtotal", label: "", type: FormFieldType.Custom },
      { name: "taxAmount", label: "", type: FormFieldType.Custom },
      { name: "grossAmount", label: "", type: FormFieldType.Custom },
      { name: "netAmount", label: "", type: FormFieldType.Custom },
      { name: "discountAmount", label: "", type: FormFieldType.Custom },
      { name: "extraTaxAmount", label: "", type: FormFieldType.Custom },
    ],
  },
]);

onMounted(() => {
  mountTimer = setTimeout(() => {
    calculationsReady = true;
    if (props.purchaseInvoice.purchaseInvoiceImports.length > 0) {
      const taxAmount = getTaxAmountFromImports();
      form.value?.setFieldValue("taxAmount", taxAmount);
      latestCalculationValues.value = {
        ...latestCalculationValues.value,
        taxAmount,
      };
      emit("calculated", { taxAmount });
    }
  }, 500);
});

onUnmounted(() => {
  if (mountTimer !== undefined) clearTimeout(mountTimer);
  dueDateRequestSequence += 1;
});

const validateImports = (): boolean => {
  if (props.purchaseInvoice.purchaseInvoiceImports.length > 0) return true;

  toast.add({
    severity: "warn",
    summary: t("purchase.messages.invalidForm"),
    detail: t("purchase.validation.invoiceImportsRequired"),
    life: 5000,
  });
  return false;
};

const submit = (values: FormValues): void => {
  if (!validateImports()) return;
  emit("submit", invoiceValues(values));
};

const submitForm = (): void => form.value?.submit();
const calcAmounts = (): void => {
  void calculateAmounts();
};
const getSupplierId = (): string =>
  stringValue(
    latestCalculationValues.value.supplierId,
    props.purchaseInvoice.supplierId,
  );

defineExpose({ submitForm, calcAmounts, getSupplierId });
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="initialValues"
    :show-submit="false"
    :show-cancel="false"
    @submit="submit"
  >
    <template #field-statusId="{ value, setValue, disabled }">
      <DropdownLifecycleStatusTransitions
        label=""
        :status-id="purchaseInvoice.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #section-summary="{ values }">
      <section class="summary-grid mb-2">
        <div class="cost-card">
          <div class="cost-card-icon"><i class="pi pi-file" /></div>
          <div class="cost-card-content">
            <span class="cost-card-label">{{ t("purchase.fields.base") }}</span>
            <span class="cost-card-value">
              {{
                formatCurrency(
                  finiteNumberValue(
                    values.baseAmount,
                    purchaseInvoice.baseAmount,
                  ),
                )
              }}
            </span>
          </div>
        </div>
        <div class="cost-card">
          <div class="cost-card-icon"><i class="pi pi-receipt" /></div>
          <div class="cost-card-content">
            <span class="cost-card-label">{{ t("purchase.fields.taxes") }}</span>
            <span class="cost-card-value">
              {{
                formatCurrency(
                  finiteNumberValue(
                    values.taxAmount,
                    purchaseInvoice.taxAmount,
                  ),
                )
              }}
            </span>
          </div>
        </div>
        <div class="cost-card cost-card-total">
          <div class="cost-card-icon"><i class="pi pi-calculator" /></div>
          <div class="cost-card-content">
            <span class="cost-card-label">{{ t("common.total") }}</span>
            <span class="cost-card-value">
              {{
                formatCurrency(
                  finiteNumberValue(
                    values.netAmount,
                    purchaseInvoice.netAmount,
                  ),
                )
              }}
            </span>
          </div>
        </div>
      </section>
    </template>
  </Form>
</template>

<style scoped>
.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
}

.cost-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: 8px;
  padding: 0.85rem 1rem;
  background: var(--p-content-background, #fff);
  transition: box-shadow 0.15s ease;
}

.cost-card:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.cost-card-total {
  background: var(--p-primary-50, #eef2ff);
  border-color: var(--p-primary-200, #c7d2fe);
}

.cost-card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 8px;
  background: var(--p-surface-100, #f1f5f9);
  color: var(--p-primary-color, #3b82f6);
  font-size: 1rem;
  flex-shrink: 0;
}

.cost-card-total .cost-card-icon {
  background: var(--p-primary-100, #dbeafe);
  color: var(--p-primary-700, #1d4ed8);
}

.cost-card-content {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  min-width: 0;
}

.cost-card-label {
  font-size: 0.8rem;
  color: var(--p-text-muted-color);
  white-space: nowrap;
}

.cost-card-value {
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.cost-card-total .cost-card-value {
  color: var(--p-primary-700, #1d4ed8);
}

@media (max-width: 767px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }
}
</style>
