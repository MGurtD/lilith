<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  finiteNumberValue,
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import { useToast } from "primevue/usetoast";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { ReferenceService } from "../../shared/services/reference.service";
import { useReferenceStore } from "../../shared/store/reference";
import type { Reference } from "../../shared/types";
import SelectorLot from "../../warehouse/components/SelectorLot.vue";
import PurchaseServices from "../services";
import { useReceiptsStore } from "../store/receipt";
import type { Receipt, ReceiptDetail } from "../types";

const props = defineProps<{
  detail: ReceiptDetail;
  receipt: Receipt;
}>();

const emit = defineEmits<{
  (event: "submit", detail: ReceiptDetail): void;
}>();

const receiptStore = useReceiptsStore();
const referenceStore = useReferenceStore();
const referenceService = new ReferenceService("/reference");
const toast = useToast();
const { t } = useI18n();
const form = ref<{
  setFieldValue: (name: string, value: unknown) => void;
  setValues: (values: FormValues) => void;
} | null>(null);
const format = ref("");
const unitWeight = ref(props.detail.unitWeight);
const currentDescription = ref(props.detail.description);
let referenceRequestSequence = 0;
let calculationRequestSequence = 0;
let suppressCalculations = false;

const decimalProps = {
  locale: "en-US",
  minFractionDigits: 2,
  highlightOnFocus: true,
} as const;
const currencyProps = {
  currency: "EUR",
  locale: "en-US",
  minFractionDigits: 2,
  highlightOnFocus: true,
} as const;

const setFormValues = (values: FormValues): void => {
  suppressCalculations = true;
  try {
    form.value?.setValues(values);
  } finally {
    suppressCalculations = false;
  }
};

const setFormField = (name: string, value: unknown): void => {
  setFormValues({ [name]: value });
};

const referenceRequiresLot = (referenceId: string): boolean =>
  referenceStore.references?.find((item) => item.id === referenceId)
    ?.requiresLot ?? false;

const receiptDetailValues = (values: Readonly<FormValues>): ReceiptDetail => ({
  ...props.detail,
  referenceId: stringValue(values.referenceId, props.detail.referenceId),
  description: stringValue(values.description, props.detail.description),
  quantity: finiteNumberValue(values.quantity, props.detail.quantity),
  width: finiteNumberValue(values.width, props.detail.width),
  lenght: finiteNumberValue(values.lenght, props.detail.lenght),
  height: finiteNumberValue(values.height, props.detail.height),
  diameter: finiteNumberValue(values.diameter, props.detail.diameter),
  thickness: finiteNumberValue(values.thickness, props.detail.thickness),
  totalWeight: finiteNumberValue(
    values.totalWeight,
    props.detail.totalWeight,
  ),
  unitWeight: finiteNumberValue(unitWeight.value, props.detail.unitWeight),
  kilogramPrice: finiteNumberValue(
    values.kilogramPrice,
    props.detail.kilogramPrice,
  ),
  unitPrice: finiteNumberValue(values.unitPrice, props.detail.unitPrice),
  amount: finiteNumberValue(values.amount, props.detail.amount),
  lotId: nullableStringValue(values.lotId, props.detail.lotId ?? null),
  lotCode: stringValue(values.lotCode, props.detail.lotCode ?? ""),
});

const applyCalculationResult = (detail: ReceiptDetail): void => {
  unitWeight.value = detail.unitWeight;
  setFormValues({
    totalWeight: detail.totalWeight,
    unitPrice: detail.unitPrice,
    amount: detail.amount,
  });
};

const calculate = async (values: Readonly<FormValues>): Promise<void> => {
  if (suppressCalculations) return;

  const requestSequence = ++calculationRequestSequence;
  const detail = receiptDetailValues(values);
  const requiresRemoteCalculation =
    (format.value === "RODO" &&
      detail.diameter !== 0 &&
      detail.lenght !== 0) ||
    (format.value === "TUB" &&
      detail.diameter !== 0 &&
      detail.lenght !== 0 &&
      detail.thickness !== 0) ||
    (format.value === "PLACA" &&
      detail.lenght !== 0 &&
      detail.width !== 0 &&
      detail.height !== 0);

  if (requiresRemoteCalculation) {
    const response = await receiptStore.calculateDetailWeightAndPrice(detail);
    if (requestSequence !== calculationRequestSequence) return;

    if (response.result && response.content) {
      applyCalculationResult(response.content);
    } else {
      toast.add({
        summary: t("purchase.receiptDetail.messages.calculator"),
        detail: response.errors[0],
        severity: "warn",
        life: 6000,
      });
    }
  } else if (format.value === "UNITATS") {
    setFormField("amount", detail.unitPrice * detail.quantity);
  }
};

const calculateOnChange = (
  _value: unknown,
  values: Readonly<FormValues>,
): void => {
  void calculate(values);
};

const updateDescription = (value: unknown): void => {
  currentDescription.value = stringValue(value, "");
};

const loadReferenceContext = async (
  referenceId: string,
): Promise<{ reference?: Reference; format?: string }> => {
  await referenceStore.fetchReference(referenceId);
  const reference = referenceStore.reference;
  if (!reference?.referenceFormatId) return { reference };

  const referenceFormat = await referenceService.getReferenceFormatById(
    reference.referenceFormatId,
  );
  return { reference, format: referenceFormat?.code };
};

const resetReferenceValues = (): void => {
  calculationRequestSequence += 1;
  unitWeight.value = 0;
  setFormValues({
    thickness: 0,
    lenght: 0,
    diameter: 0,
    height: 0,
    kilogramPrice: 0,
    quantity: 0,
    totalWeight: 0,
    width: 0,
    unitPrice: 0,
    amount: 0,
    lotId: null,
    lotCode: "",
  });
};

const loadReferenceInfo = async (referenceId: string | null): Promise<void> => {
  const requestSequence = ++referenceRequestSequence;
  calculationRequestSequence += 1;
  if (referenceId === null || props.receipt.supplierId === "") return;

  resetReferenceValues();
  const context = await loadReferenceContext(referenceId);
  if (requestSequence !== referenceRequestSequence) return;

  if (context.format) {
    format.value = context.format;
    setFormField("format", context.format);
  }

  const supplierReference =
    await PurchaseServices.Supplier.getSupplierReferenceBySupplierIdAndReferenceId(
      props.receipt.supplierId,
      referenceId,
    );
  if (requestSequence !== referenceRequestSequence) return;

  const price = supplierReference
    ? supplierReference.supplierPrice
    : context.reference?.price;
  if (price !== undefined) {
    setFormField(
      format.value === "UNITATS" ? "unitPrice" : "kilogramPrice",
      price,
    );
  }

  if (currentDescription.value === "") {
    currentDescription.value = referenceStore.getFullNameById(referenceId);
    setFormField("description", currentDescription.value);
  }
};

const updateReference = (
  value: string | null,
  setValue: (value: unknown) => void,
): void => {
  setValue(value);
  void loadReferenceInfo(value);
};

const isDisabled = (field: string): boolean => {
  if (format.value === "UNITATS") {
    return [
      "thickness",
      "width",
      "height",
      "kilogramprice",
      "length",
      "diameter",
      "totalweight",
    ].includes(field);
  }
  if (format.value === "RODO") {
    return ["thickness", "width", "height", "totalweight"].includes(field);
  }
  if (format.value === "TUB") {
    return ["width", "height", "totalweight"].includes(field);
  }
  if (format.value === "PLACA") {
    return ["thickness", "diameter", "totalweight"].includes(field);
  }
  return false;
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "referenceId",
        label: t("purchase.receiptDetail.fields.purchaseReference"),
        type: FormFieldType.Custom,
        span: { desktop: 2 },
        validation: Yup.string().required(
          t("purchase.receiptDetail.validation.referenceRequired"),
        ),
      },
      {
        name: "format",
        label: t("purchase.receiptDetail.fields.format"),
        type: FormFieldType.Text,
        defaultValue: "",
        disabled: true,
      },
      {
        name: "kilogramPrice",
        label: t("purchase.receiptDetail.fields.pricePerKilo"),
        type: FormFieldType.Currency,
        props: currencyProps,
        disabled: isDisabled("kilogramprice"),
        onChange: calculateOnChange,
      },
    ],
  },
  {
    fields: [
      {
        name: "description",
        label: t("purchase.receiptDetail.fields.description"),
        type: FormFieldType.Text,
        onChange: updateDescription,
      },
    ],
  },
  {
    section: "lot",
    fields: [
      {
        name: "lotId",
        label: "",
        type: FormFieldType.Custom,
      },
      {
        name: "lotCode",
        label: "",
        type: FormFieldType.Custom,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "width",
        label: t("purchase.receiptDetail.fields.width"),
        type: FormFieldType.Number,
        props: decimalProps,
        disabled: isDisabled("width"),
        onChange: calculateOnChange,
      },
      {
        name: "height",
        label: t("purchase.receiptDetail.fields.height"),
        type: FormFieldType.Number,
        props: decimalProps,
        disabled: isDisabled("height"),
        onChange: calculateOnChange,
      },
      {
        name: "lenght",
        label: t("purchase.receiptDetail.fields.length"),
        type: FormFieldType.Number,
        props: decimalProps,
        disabled: isDisabled("length"),
        onChange: calculateOnChange,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "diameter",
        label: t("purchase.receiptDetail.fields.diameter"),
        type: FormFieldType.Number,
        props: decimalProps,
        disabled: isDisabled("diameter"),
        onChange: calculateOnChange,
      },
      {
        name: "thickness",
        label: t("purchase.receiptDetail.fields.thickness"),
        type: FormFieldType.Number,
        props: decimalProps,
        disabled: isDisabled("thickness"),
        onChange: calculateOnChange,
      },
      {
        name: "totalWeight",
        label: t("purchase.receiptDetail.fields.weight"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 2 },
        disabled: isDisabled("totalweight"),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "quantity",
        label: t("purchase.receiptDetail.fields.quantity"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0, highlightOnFocus: true },
        onChange: calculateOnChange,
        validation: Yup.number()
          .min(1, t("purchase.receiptDetail.validation.quantityMinimum"))
          .required(t("purchase.receiptDetail.validation.quantityMinimum")),
      },
      {
        name: "unitPrice",
        label: t("purchase.receiptDetail.fields.unitPrice"),
        type: FormFieldType.Currency,
        props: currencyProps,
        onChange: calculateOnChange,
      },
      {
        name: "amount",
        label: t("purchase.receiptDetail.fields.price"),
        type: FormFieldType.Currency,
        props: {
          currency: "EUR",
          locale: "en-US",
          minFractionDigits: 2,
        },
      },
    ],
  },
]);

watch(
  () => props.detail,
  (detail) => {
    referenceRequestSequence += 1;
    calculationRequestSequence += 1;
    format.value = "";
    unitWeight.value = detail.unitWeight;
    currentDescription.value = detail.description;

    if (detail.referenceId) {
      const requestSequence = referenceRequestSequence;
      void loadReferenceContext(detail.referenceId).then((context) => {
        if (
          requestSequence === referenceRequestSequence &&
          context.format !== undefined
        ) {
          format.value = context.format;
          setFormField("format", context.format);
        }
      });
    }
  },
  { immediate: true },
);

const submit = (values: FormValues): void => {
  emit("submit", receiptDetailValues(values));
};
</script>

<template>
  <Form ref="form" :rows="rows" :initial-values="detail" @submit="submit">
    <template #field-referenceId="{ value, setValue, disabled, inputId }">
      <DropdownReference
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :full-name="true"
        :disabled="disabled"
        @update:model-value="updateReference($event, setValue)"
      />
    </template>

    <template #section-lot="{ values, setFieldValue }">
      <div
        v-if="referenceRequiresLot(stringValue(values.referenceId, ''))"
        class="mt-2"
      >
        <SelectorLot
          :reference-id="stringValue(values.referenceId, '')"
          :model-value="nullableStringValue(values.lotId, null)"
          @update:model-value="setFieldValue('lotId', $event)"
          @update:lot-code="setFieldValue('lotCode', $event)"
        />
      </div>
    </template>

    <template #actions="{ submit: submitForm, loading, disabled }">
      <Button
        type="button"
        :label="t('purchase.receiptDetail.actions.create')"
        :loading="loading"
        :disabled="disabled"
        size="small"
        class="mt-2"
        style="float: right"
        @click="submitForm"
      />
    </template>
  </Form>
</template>
