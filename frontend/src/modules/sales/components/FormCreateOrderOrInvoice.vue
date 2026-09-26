<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { dateValue, stringValue } from "@/components/forms/value-utils";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { useExerciseStore } from "../../shared/store/exercise";
import DropdownCustomers from "./DropdownCustomers.vue";
import type { CreateSalesHeaderRequest } from "../types";

// Shared create dialog of the sales order, delivery note, budget and invoice
// lists. Each list persists the emitted request with its own store.
const props = withDefaults(
  defineProps<{
    createRequest: CreateSalesHeaderRequest;
    loading?: boolean;
  }>(),
  { loading: false },
);

const emit = defineEmits<{
  (event: "submit", createRequest: CreateSalesHeaderRequest): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const exerciseStore = useExerciseStore();
const currentExerciseId = ref("");

const initialValues = computed(() => ({
  ...props.createRequest,
  exerciseId: props.createRequest.exerciseId || currentExerciseId.value,
}));

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "customerId",
        label: t("sales.components.client"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.customerRequired"),
        ),
      },
    ],
  },
  {
    fields: [
      {
        name: "exerciseId",
        label: t("sales.components.exercici"),
        type: FormFieldType.Select,
        props: {
          options: exerciseStore.exercises,
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("sales.validation.exerciseRequired"),
        ),
      },
    ],
  },
  {
    fields: [
      {
        name: "date",
        label: t("sales.components.data"),
        type: FormFieldType.Date,
        validation: Yup.date()
          .typeError(t("sales.validation.dateRequired"))
          .required(t("sales.validation.dateRequired")),
      },
    ],
  },
]);

onMounted(async () => {
  if (!exerciseStore.exercises?.length) {
    await exerciseStore.fetchActive();
  }

  currentExerciseId.value =
    exerciseStore.exercises?.find(
      (exercise) => exercise.name === String(new Date().getFullYear()),
    )?.id ?? "";
});

// The date stays a native Date: Date.prototype.toJSON (utils/functions.ts)
// applies the local-time conversion when the store serializes the request.
const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.createRequest,
    customerId: stringValue(values.customerId, ""),
    exerciseId: stringValue(values.exerciseId, ""),
    date: dateValue(values.date, props.createRequest.date),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="initialValues"
    :loading="loading"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-customerId="{ value, setValue, disabled, inputId }">
      <DropdownCustomers
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>
