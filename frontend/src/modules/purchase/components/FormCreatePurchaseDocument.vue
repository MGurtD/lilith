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
import { useExerciseStore } from "../../shared/store/exercise";
import { useSuppliersStore } from "../store/suppliers";
import type { CreatePurchaseDocumentRequest } from "../types";

const props = withDefaults(
  defineProps<{
    createRequest: CreatePurchaseDocumentRequest;
    loading?: boolean;
  }>(),
  { loading: false },
);

const emit = defineEmits<{
  (event: "submit", request: CreatePurchaseDocumentRequest): void;
}>();

const { t } = useI18n();
const exerciseStore = useExerciseStore();
const suppliersStore = useSuppliersStore();
const currentExerciseId = ref("");

const initialValues = computed(() => ({
  ...props.createRequest,
  exerciseId: props.createRequest.exerciseId || currentExerciseId.value,
}));

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
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
  {
    fields: [
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
    ],
  },
  {
    fields: [
      {
        name: "date",
        label: t("purchase.order.fields.date"),
        type: FormFieldType.Date,
        validation: Yup.date()
          .typeError(t("purchase.order.validation.dateRequired"))
          .required(t("purchase.order.validation.dateRequired")),
      },
    ],
  },
]);

onMounted(async () => {
  if (!exerciseStore.exercises.length) {
    await exerciseStore.fetchActive();
  }

  currentExerciseId.value =
    exerciseStore.exercises.find(
      (exercise) => exercise.name === String(new Date().getFullYear()),
    )?.id ?? "";
});

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.createRequest,
    supplierId: stringValue(values.supplierId, ""),
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
    :disabled="loading"
    @submit="submit"
  >
    <template #actions="{ submit: submitForm, loading: formLoading, disabled }">
      <Button
        type="button"
        :label="t('purchase.order.actions.create')"
        :loading="formLoading"
        :disabled="disabled"
        @click="submitForm"
      />
    </template>
  </Form>
</template>
