<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  dateValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useExerciseStore } from "../../shared/store/exercise";
import { useSuppliersStore } from "../store/suppliers";
import type { PurchaseOrder } from "../types";

const props = defineProps<{
  order: PurchaseOrder;
}>();

const emit = defineEmits<{
  (event: "submit", order: PurchaseOrder): void;
}>();

const exerciseStore = useExerciseStore();
const suppliersStore = useSuppliersStore();
const { t } = useI18n();
const form = ref<{ submit: () => void } | null>(null);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "number",
        label: t("purchase.order.fields.number"),
        type: FormFieldType.Text,
        disabled: true,
      },
      {
        name: "exerciseId",
        label: t("purchase.order.fields.exercise"),
        type: FormFieldType.Select,
        props: {
          options: exerciseStore.exercises,
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.order.validation.exerciseRequired"),
        ),
      },
      {
        name: "date",
        label: t("purchase.order.fields.orderDate"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("purchase.order.validation.dateRequired"))
          .required(t("purchase.order.validation.dateRequired")),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "statusId",
        label: t("purchase.order.fields.status"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("purchase.order.validation.statusRequired"),
        ),
      },
      {
        name: "supplierId",
        label: t("purchase.order.fields.supplier"),
        type: FormFieldType.Select,
        props: {
          options: suppliersStore.suppliers ?? [],
          optionValue: "id",
          optionLabel: "comercialName",
        },
        validation: Yup.string().required(
          t("purchase.order.validation.supplierRequired"),
        ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.order,
    number: stringValue(values.number, ""),
    exerciseId: stringValue(values.exerciseId, ""),
    date: dateValue(values.date, props.order.date),
    statusId: stringValue(values.statusId, ""),
    supplierId: stringValue(values.supplierId, ""),
  });
};

const submitForm = (): void => form.value?.submit();

defineExpose({ submitForm });
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="order"
    :show-submit="false"
    :show-cancel="false"
    @submit="submit"
  >
    <template #field-statusId="{ value, setValue, disabled, inputId }">
      <DropdownLifecycleStatusTransitions
        :input-id="inputId"
        label=""
        :status-id="order.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>
