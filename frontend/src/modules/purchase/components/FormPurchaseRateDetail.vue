<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  finiteNumberValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useReferenceStore } from "../../shared/store/reference";
import { CalculationType, type PurchaseRateDetail } from "../types";

const props = defineProps<{
  detail: PurchaseRateDetail;
}>();

const emit = defineEmits<{
  (event: "submit", detail: PurchaseRateDetail): void;
}>();

const referenceStore = useReferenceStore();
const { t } = useI18n();

const numberProps = {
  minFractionDigits: 2,
  maxFractionDigits: 4,
};

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "referenceId",
        label: t("purchase.purchaseRateDetail.fields.reference"),
        type: FormFieldType.Custom,
      },
    ],
  },
  {
    fields: [
      {
        name: "calculationType",
        label: t("purchase.purchaseRateDetail.fields.calculationType"),
        type: FormFieldType.Select,
        props: {
          options: [
            {
              label: t(
                "purchase.purchaseRateDetail.calculationTypes.units",
              ),
              value: CalculationType.Units,
            },
            {
              label: t(
                "purchase.purchaseRateDetail.calculationTypes.volume",
              ),
              value: CalculationType.Volume,
            },
            {
              label: t(
                "purchase.purchaseRateDetail.calculationTypes.weight",
              ),
              value: CalculationType.Weight,
            },
          ],
          optionLabel: "label",
          optionValue: "value",
        },
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "from",
        label: t("purchase.purchaseRateDetail.fields.from"),
        type: FormFieldType.Number,
        props: numberProps,
      },
      {
        name: "to",
        label: t("purchase.purchaseRateDetail.fields.to"),
        type: FormFieldType.Number,
        props: numberProps,
      },
    ],
  },
  {
    fields: [
      {
        name: "price",
        label: t("purchase.purchaseRateDetail.fields.price"),
        type: FormFieldType.Number,
        props: numberProps,
      },
    ],
  },
]);

onMounted(async () => {
  if (!referenceStore.references) {
    await referenceStore.fetchReferencesByModule("purchase");
  }
});

const calculationTypeValue = (value: unknown): CalculationType => {
  if (
    value === CalculationType.Units ||
    value === CalculationType.Volume ||
    value === CalculationType.Weight
  ) {
    return value;
  }

  return props.detail.calculationType;
};

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.detail,
    referenceId: stringValue(values.referenceId, ""),
    calculationType: calculationTypeValue(values.calculationType),
    from: finiteNumberValue(values.from, props.detail.from),
    to: finiteNumberValue(values.to, props.detail.to),
    price: finiteNumberValue(values.price, props.detail.price),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="detail"
    :show-cancel="false"
    @submit="submit"
  >
    <template #field-referenceId="{ value, setValue, disabled }">
      <Select
        :model-value="typeof value === 'string' ? value : undefined"
        :options="referenceStore.references"
        optionLabel="code"
        optionValue="id"
        :placeholder="
          t('purchase.purchaseRateDetail.placeholders.selectReference')
        "
        filter
        class="w-full"
        :disabled="disabled"
        :virtualScrollerOptions="{ itemSize: 38 }"
        @update:model-value="setValue"
      >
        <template #option="slotProps">
          <div class="flex flex-column">
            <span class="font-bold">{{ slotProps.option.code }}</span>
            <small class="text-600">{{ slotProps.option.description }}</small>
          </div>
        </template>
      </Select>
    </template>
  </Form>
</template>
