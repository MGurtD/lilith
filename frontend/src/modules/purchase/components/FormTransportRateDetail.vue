<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { finiteNumberValue } from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import type { TransportRateDetail } from "../types";

const props = defineProps<{
  detail: TransportRateDetail;
}>();

const emit = defineEmits<{
  (event: "submit", detail: TransportRateDetail): void;
}>();

const { t } = useI18n();

const numberProps = {
  minFractionDigits: 2,
  maxFractionDigits: 4,
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "minWeight",
        label: t("purchase.fields.minimumWeight"),
        type: FormFieldType.Number,
        props: numberProps,
      },
      {
        name: "maxWeight",
        label: t("purchase.fields.maximumWeight"),
        type: FormFieldType.Number,
        props: numberProps,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "minVolume",
        label: t("purchase.fields.minimumVolume"),
        type: FormFieldType.Number,
        props: numberProps,
      },
      {
        name: "maxVolume",
        label: t("purchase.fields.maximumVolume"),
        type: FormFieldType.Number,
        props: numberProps,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "minDistance",
        label: t("purchase.fields.minimumDistance"),
        type: FormFieldType.Number,
        props: numberProps,
      },
      {
        name: "maxDistance",
        label: t("purchase.fields.maximumDistance"),
        type: FormFieldType.Number,
        props: numberProps,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "price",
        label: t("purchase.fields.price"),
        type: FormFieldType.Number,
        props: numberProps,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.detail,
    minWeight: finiteNumberValue(values.minWeight, props.detail.minWeight),
    maxWeight: finiteNumberValue(values.maxWeight, props.detail.maxWeight),
    minVolume: finiteNumberValue(values.minVolume, props.detail.minVolume),
    maxVolume: finiteNumberValue(values.maxVolume, props.detail.maxVolume),
    minDistance: finiteNumberValue(
      values.minDistance,
      props.detail.minDistance,
    ),
    maxDistance: finiteNumberValue(
      values.maxDistance,
      props.detail.maxDistance,
    ),
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
  />
</template>
