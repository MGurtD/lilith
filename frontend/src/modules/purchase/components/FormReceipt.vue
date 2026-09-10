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
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useSharedDataStore } from "../../shared/store/masterData";
import { useSuppliersStore } from "../store/suppliers";
import type { Receipt } from "../types";

const props = defineProps<{
  receipt: Receipt;
}>();

const emit = defineEmits<{
  (event: "submit", receipt: Receipt): void;
}>();

const suppliersStore = useSuppliersStore();
const sharedDataStore = useSharedDataStore();
const { t } = useI18n();
const form = ref<{ submit: () => void } | null>(null);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "number",
        label: t("purchase.receipt.fields.number"),
        type: FormFieldType.Text,
        disabled: true,
      },
      {
        name: "exerciseId",
        label: t("purchase.receipt.fields.exercise"),
        type: FormFieldType.Select,
        props: {
          options: sharedDataStore.exercises,
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.receipt.validation.exerciseRequired"),
        ),
      },
      {
        name: "date",
        label: t("purchase.receipt.fields.date"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "statusId",
        label: t("purchase.receipt.fields.status"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("purchase.receipt.validation.statusRequired"),
        ),
      },
      {
        name: "supplierId",
        label: t("purchase.receipt.fields.supplier"),
        type: FormFieldType.Select,
        props: {
          options: suppliersStore.suppliers ?? [],
          optionValue: "id",
          optionLabel: "comercialName",
        },
        validation: Yup.string().required(
          t("purchase.receipt.validation.supplierRequired"),
        ),
      },
      {
        name: "supplierNumber",
        label: t("purchase.receipt.fields.supplierNumber"),
        type: FormFieldType.Text,
      },
    ],
  },
]);

onMounted(async () => {
  await sharedDataStore.fetchMasterData();
  await suppliersStore.fetchSuppliers();
});

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.receipt,
    number: stringValue(values.number, ""),
    exerciseId: stringValue(values.exerciseId, ""),
    date: dateValue(values.date, props.receipt.date),
    statusId: stringValue(values.statusId, ""),
    supplierId: stringValue(values.supplierId, ""),
    supplierNumber: stringValue(values.supplierNumber, ""),
  });
};

const submitForm = (): void => form.value?.submit();

defineExpose({ submitForm });
</script>

<template>
  <div>
    <Button
      :label="t('purchase.receipt.actions.save')"
      size="small"
      class="grid_add_row_button"
      @click="submitForm"
    />
    <br />
    <Form
      ref="form"
      :rows="rows"
      :initial-values="receipt"
      :show-submit="false"
      :show-cancel="false"
      @submit="submit"
    >
      <template #field-statusId="{ value, setValue, disabled }">
        <DropdownLifecycleStatusTransitions
          label=""
          :status-id="receipt.statusId"
          :model-value="typeof value === 'string' ? value : undefined"
          :disabled="disabled"
          @update:model-value="setValue"
        />
      </template>
    </Form>
  </div>
</template>
