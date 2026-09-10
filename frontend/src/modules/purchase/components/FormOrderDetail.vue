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
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import SharedServices from "../../shared/services";
import { useReferenceStore } from "../../shared/store/reference";
import { ReferenceCategoryEnum } from "../../shared/types";
import PurchaseServices from "../services";
import type { PurchaseOrder, PurchaseOrderDetail } from "../types";

const props = defineProps<{
  detail: PurchaseOrderDetail;
  order: PurchaseOrder;
}>();

const emit = defineEmits<{
  (event: "submit", detail: PurchaseOrderDetail): void;
}>();

const referenceStore = useReferenceStore();
const { t } = useI18n();
const form = ref<{
  setFieldValue: (name: string, value: unknown) => void;
} | null>(null);
const unitPrice = ref(props.detail.unitPrice);
const latestQuantity = ref(props.detail.quantity);
let referenceRequestSequence = 0;

watch(
  () => props.detail,
  (detail) => {
    referenceRequestSequence += 1;
    unitPrice.value = detail.unitPrice;
    latestQuantity.value = detail.quantity;
  },
  { immediate: true },
);

const calculateAmount = (
  referenceId: string,
  quantity: number,
): number | undefined => {
  const reference = referenceStore.references?.find(
    (item) => item.id === referenceId,
  );
  if (!reference) return undefined;

  return reference.categoryName === ReferenceCategoryEnum.SERVICE
    ? unitPrice.value
    : quantity * unitPrice.value;
};

const updateCalculatedAmount = (
  referenceId: string,
  quantity: number,
): void => {
  const amount = calculateAmount(referenceId, quantity);
  if (amount !== undefined) form.value?.setFieldValue("amount", amount);
};

const updateQuantity = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  latestQuantity.value = finiteNumberValue(value, latestQuantity.value);
  updateCalculatedAmount(
    stringValue(values.referenceId, props.detail.referenceId),
    latestQuantity.value,
  );
};

const addDays = (days: number): Date => {
  const date = new Date();
  date.setDate(date.getDate() + days);
  return date;
};

const loadReferenceInfo = async (referenceId: string | null): Promise<void> => {
  const requestSequence = ++referenceRequestSequence;
  if (referenceId === null || props.order.supplierId === "") return;

  const supplierReference =
    await PurchaseServices.Supplier.getSupplierReferenceBySupplierIdAndReferenceId(
      props.order.supplierId,
      referenceId,
    );
  if (requestSequence !== referenceRequestSequence) return;

  if (supplierReference) {
    unitPrice.value = supplierReference.supplierPrice;
    form.value?.setFieldValue(
      "expectedReceiptDate",
      addDays(supplierReference.supplyDays),
    );
    form.value?.setFieldValue(
      "description",
      supplierReference.supplierDescription,
    );
  } else {
    const reference = await SharedServices.Reference.getById(referenceId);
    if (requestSequence !== referenceRequestSequence) return;

    if (reference) {
      unitPrice.value = reference.price;
      form.value?.setFieldValue("description", reference.description);
    }
  }

  updateCalculatedAmount(referenceId, latestQuantity.value);
};

const updateReference = (
  value: string | null,
  setValue: (value: unknown) => void,
): void => {
  setValue(value);
  void loadReferenceInfo(value);
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "referenceId",
        label: t("purchase.orderDetail.fields.purchaseReference"),
        type: FormFieldType.Custom,
        disabled: props.detail.receivedQuantity > 0,
        validation: Yup.string().required(
          t("purchase.orderDetail.validation.referenceRequired"),
        ),
      },
      {
        name: "statusId",
        label: t("purchase.order.fields.status"),
        type: FormFieldType.Custom,
      },
      {
        name: "expectedReceiptDate",
        label: t("purchase.orderDetail.fields.expectedReceiptDate"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
      },
    ],
  },
  {
    fields: [
      {
        name: "description",
        label: t("purchase.orderDetail.fields.description"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("purchase.orderDetail.validation.descriptionRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "quantity",
        label: t("purchase.orderDetail.fields.quantity"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
        disabled: props.detail.receivedQuantity > 0,
        onChange: updateQuantity,
        validation: Yup.number()
          .min(1, t("purchase.orderDetail.validation.quantityMinimum"))
          .required(t("purchase.orderDetail.validation.quantityMinimum")),
      },
      {
        name: "amount",
        label: t("purchase.orderDetail.fields.price"),
        type: FormFieldType.Currency,
        props: {
          currency: "EUR",
          locale: "en-US",
          minFractionDigits: 2,
        },
        disabled: props.detail.receivedQuantity > 0,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.detail,
    referenceId: stringValue(values.referenceId, props.detail.referenceId),
    statusId: stringValue(values.statusId, props.detail.statusId),
    expectedReceiptDate: dateValue(
      values.expectedReceiptDate,
      props.detail.expectedReceiptDate,
    ),
    description: stringValue(values.description, props.detail.description),
    quantity: finiteNumberValue(values.quantity, props.detail.quantity),
    unitPrice: finiteNumberValue(unitPrice.value, props.detail.unitPrice),
    amount: finiteNumberValue(values.amount, props.detail.amount),
  });
};
</script>

<template>
  <Form ref="form" :rows="rows" :initial-values="detail" @submit="submit">
    <template #field-referenceId="{ value, setValue, disabled }">
      <DropdownReference
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :full-name="true"
        :disabled="disabled"
        @update:model-value="updateReference($event, setValue)"
      />
    </template>

    <template #field-statusId="{ value, setValue, disabled }">
      <DropdownLifecycleStatusTransitions
        label=""
        :status-id="detail.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #actions="{ submit: submitForm, loading, disabled }">
      <Button
        type="button"
        :label="t('purchase.order.actions.create')"
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
