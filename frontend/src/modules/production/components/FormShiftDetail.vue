<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { booleanValue, dateValue } from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { extractTime } from "../../../utils/functions";
import type { ShiftDetail } from "../types";

const props = defineProps<{
  shiftdetail: ShiftDetail;
}>();

const emit = defineEmits<{
  (event: "submit", shiftdetail: ShiftDetail): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const timePattern = /^(\d{1,2}):(\d{2})(?::(\d{2}))?/;

// The entity stores times as "HH:mm:ss"; the time-only DatePicker needs a
// native Date, so the time is placed on today's date.
const timeToDate = (value: unknown): Date | null => {
  if (value instanceof Date) return value;
  if (typeof value !== "string") return null;

  const match = timePattern.exec(value);
  if (!match) return null;

  const date = new Date();
  date.setHours(
    Number(match[1]),
    Number(match[2]),
    match[3] === undefined ? 0 : Number(match[3]),
    0,
  );
  return date;
};

const initialValues = computed(() => ({
  ...props.shiftdetail,
  startTime: timeToDate(props.shiftdetail.startTime),
  endTime: timeToDate(props.shiftdetail.endTime),
}));

const timeProps = { timeOnly: true, hourFormat: "24" } as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "startTime",
        label: t("production.fields.shiftStartTime"),
        type: FormFieldType.Date,
        props: timeProps,
      },
      {
        name: "endTime",
        label: t("production.fields.shiftEndTime"),
        type: FormFieldType.Date,
        props: timeProps,
      },
      {
        name: "isProductiveTime",
        label: t("production.fields.productiveTime"),
        type: FormFieldType.Checkbox,
        defaultValue: true,
      },
    ],
  },
]);

// Converts the picked time back to the "HH:mm:ss" entity format, as the
// legacy form did; a cleared picker yields an empty string.
const timeValue = (value: unknown): string =>
  extractTime(dateValue(value, null)?.toISOString() ?? null);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.shiftdetail,
    startTime: timeValue(values.startTime),
    endTime: timeValue(values.endTime),
    isProductiveTime: booleanValue(
      values.isProductiveTime,
      props.shiftdetail.isProductiveTime,
    ),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>
