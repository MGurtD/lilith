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
import { computed, onUnmounted, shallowRef, useId, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import Services from "../services";
import { usePlantModelStore } from "../store/plantmodel";
import { useWorkOrderStore } from "../store/workorder";
import type { DetailedWorkOrder, ProductionPart } from "../types";

const props = defineProps<{
  productionPart: ProductionPart;
  avoidWorkOrderRefresh: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", productionPart: ProductionPart): void;
  (e: "cancel"): void;
}>();

const { t } = useI18n();
const plantModelStore = usePlantModelStore();
const workOrderStore = useWorkOrderStore();
const form = shallowRef<{
  setValues: (values: FormValues) => void;
} | null>(null);
const detailInputId = `production-part-detail-${useId()}`;
const numberProps = { locale: "en-US", minFractionDigits: 0 } as const;
let workOrderRequestSequence = 0;

watch(
  () => props.productionPart,
  () => {
    workOrderRequestSequence += 1;
  },
);

onUnmounted(() => {
  workOrderRequestSequence += 1;
});

const detailKey = (
  workOrderId: string,
  workOrderPhaseId: string,
  workOrderPhaseDetailId: string,
): string => `${workOrderId}|${workOrderPhaseId}|${workOrderPhaseDetailId}`;

const detailedWorkOrderOptions = computed(() =>
  [...(workOrderStore.detailedWorkOrders ?? [])]
    .sort((a, b) => a.workOrderCode.localeCompare(b.workOrderCode))
    .map((workorder) => ({
      label:
        workorder.workOrderCode +
        "  (" +
        workorder.referenceDescription +
        ") - " +
        workorder.workOrderPhaseCode +
        "  (" +
        workorder.workOrderPhaseDescription +
        ") | " +
        workorder.machineStatusDescription,
      value: detailKey(
        workorder.workOrderId,
        workorder.workOrderPhaseId,
        workorder.workOrderPhaseDetailId,
      ),
      workorder,
    })),
);

const selectedDetailKey = (values: Readonly<FormValues>): string | null => {
  const workOrderId = stringValue(values.workOrderId, "");
  const workOrderPhaseId = stringValue(values.workOrderPhaseId, "");
  const workOrderPhaseDetailId = stringValue(values.workOrderPhaseDetailId, "");
  if (!workOrderId || !workOrderPhaseId || !workOrderPhaseDetailId) return null;
  return detailKey(workOrderId, workOrderPhaseId, workOrderPhaseDetailId);
};

const detailError = (errors: Record<string, string>): string | undefined =>
  errors.workOrderId ?? errors.workOrderPhaseId ?? errors.workOrderPhaseDetailId;

const setDetailedWorkOrder = (
  key: unknown,
  setValues: (values: FormValues) => void,
): void => {
  const detailedWorkOrder: DetailedWorkOrder | undefined =
    detailedWorkOrderOptions.value.find((option) => option.value === key)
      ?.workorder;
  if (!detailedWorkOrder) return;

  setValues({
    workOrderId: detailedWorkOrder.workOrderId,
    workOrderPhaseId: detailedWorkOrder.workOrderPhaseId,
    workOrderPhaseDetailId: detailedWorkOrder.workOrderPhaseDetailId,
  });
};

const workcenterOptions = computed(() =>
  [...(plantModelStore.workcenters ?? [])].sort((a, b) =>
    a.description.localeCompare(b.description),
  ),
);

const operatorOptions = computed(() =>
  [...(plantModelStore.operators ?? [])]
    .sort((a, b) => a.surname.localeCompare(b.surname))
    .map((operator) => ({
      value: operator.id,
      label: operator.name + " " + operator.surname,
    })),
);

// The detailed work order list depends on the workcenter: a new workcenter
// clears the previous selection and only the latest response is applied.
const getWorkOrders = async (workcenterId: string): Promise<void> => {
  if (props.avoidWorkOrderRefresh) return;

  const requestSequence = ++workOrderRequestSequence;
  form.value?.setValues({
    workOrderId: "",
    workOrderPhaseId: "",
    workOrderPhaseDetailId: "",
  });
  const detailedWorkOrders =
    await Services.DetailedWorkOrder.getByWorkcenterId(workcenterId);
  if (requestSequence !== workOrderRequestSequence) return;

  workOrderStore.detailedWorkOrders = detailedWorkOrders;
};

const updateWorkcenter = (value: unknown): void => {
  void getWorkOrders(stringValue(value, ""));
};

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "workcenterId",
        label: t("production.components.maquina"),
        type: FormFieldType.Select,
        props: {
          options: workcenterOptions.value,
          optionValue: "id",
          optionLabel: "description",
          filter: true,
        },
        onChange: updateWorkcenter,
        validation: Yup.string().required(
          t("production.validation.escullUnaMaquina"),
        ),
      },
      {
        name: "operatorId",
        label: t("production.components.operari"),
        type: FormFieldType.Select,
        props: {
          options: operatorOptions.value,
          optionValue: "value",
          optionLabel: "label",
          filter: true,
        },
        validation: Yup.string().required(
          t("production.validation.escullUnOperari"),
        ),
      },
      {
        name: "date",
        label: t("production.components.dataTiquet"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
      },
    ],
  },
  {
    section: "detailedWorkOrder",
    fields: [
      {
        name: "workOrderId",
        label: "",
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("production.validation.escullUnaOrdreDeFabricacio"),
        ),
      },
      {
        name: "workOrderPhaseId",
        label: "",
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("production.validation.escullUnaFase"),
        ),
      },
      {
        name: "workOrderPhaseDetailId",
        label: "",
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("production.validation.escullUnaActivitat"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "quantity",
        label: t("production.components.quantitat"),
        type: FormFieldType.Number,
        props: numberProps,
        validation: Yup.number()
          .required(
            t("production.validation.hasDIntroduirUnaQuantitatEnteraPotSer0"),
          )
          .integer(
            t("production.validation.hasDIntroduirUnaQuantitatEnteraPotSer0"),
          ),
      },
      {
        name: "workcenterTime",
        label: t("production.components.tempsCentreDeTreballMinuts"),
        type: FormFieldType.Number,
        props: numberProps,
        validation: Yup.number()
          .required(
            t("production.validation.hasDIntroduirElTempsIHaDeSerMajorQue0"),
          )
          .integer(
            t("production.validation.hasDIntroduirElTempsIHaDeSerMajorQue0"),
          ),
      },
      {
        name: "operatorTime",
        label: t("production.components.tempsOperariMinuts"),
        type: FormFieldType.Number,
        props: numberProps,
        validation: Yup.number()
          .required(
            t("production.validation.hasDIntroduirElTempsIHaDeSerMajorQue0"),
          )
          .integer(
            t("production.validation.hasDIntroduirElTempsIHaDeSerMajorQue0"),
          ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.productionPart,
    workcenterId: stringValue(values.workcenterId, ""),
    operatorId: stringValue(values.operatorId, ""),
    date: dateValue(values.date, props.productionPart.date ?? null),
    workOrderId: stringValue(values.workOrderId, ""),
    workOrderPhaseId: stringValue(values.workOrderPhaseId, ""),
    workOrderPhaseDetailId: stringValue(values.workOrderPhaseDetailId, ""),
    quantity: finiteNumberValue(values.quantity, props.productionPart.quantity),
    workcenterTime: finiteNumberValue(
      values.workcenterTime,
      props.productionPart.workcenterTime,
    ),
    operatorTime: finiteNumberValue(
      values.operatorTime,
      props.productionPart.operatorTime,
    ),
  });
};
</script>

<template>
  <Form
    ref="form"
    class="mt-2"
    :rows="rows"
    :initial-values="productionPart"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #section-detailedWorkOrder="{ values, errors, setValues, disabled }">
      <div>
        <label class="block text-900 mb-2" :for="detailInputId">
          {{ t("production.components.ordreFabricacioFaseActivitat") }}
        </label>
        <Select
          :label-id="detailInputId"
          :model-value="selectedDetailKey(values)"
          :options="detailedWorkOrderOptions"
          option-label="label"
          option-value="value"
          filter
          class="w-full"
          :class="{ 'p-invalid': detailError(errors) }"
          :disabled="disabled"
          :aria-invalid="detailError(errors) ? 'true' : undefined"
          @update:model-value="setDetailedWorkOrder($event, setValues)"
        />
        <small
          v-if="detailError(errors)"
          class="detail-error"
          role="alert"
        >
          <i class="pi pi-exclamation-circle" aria-hidden="true" />
          <span>{{ detailError(errors) }}</span>
        </small>
      </div>
    </template>
  </Form>
</template>

<style scoped>
.detail-error {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  margin-top: 0.35rem;
  color: var(--p-orange-600);
  line-height: 1.25;
}
</style>
