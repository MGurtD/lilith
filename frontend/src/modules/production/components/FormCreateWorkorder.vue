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
import { computed, useId } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { useReferenceStore } from "../../shared/store/reference";
import { useWorkMasterStore } from "../store/workmaster";
import type { CreateWorkOrderDto, WorkMaster } from "../types";

const props = defineProps<{
  createWorkOrderDto: CreateWorkOrderDto;
  filteredWorkMasters?: Array<WorkMaster>;
}>();

const emit = defineEmits<{
  (e: "submit", createWorkOrderDto: CreateWorkOrderDto): void;
  (e: "cancel"): void;
}>();

const { t } = useI18n();
const workMasterStore = useWorkMasterStore();
const referenceStore = useReferenceStore();
const lotCodeInputId = `create-workorder-lot-code-${useId()}`;

const referenceRequiresLot = (workMasterId: string): boolean => {
  const workMaster = workMasterStore.workmasters?.find(
    (wm) => wm.id === workMasterId,
  );
  if (!workMaster) return false;
  return (
    referenceStore.references?.find((r) => r.id === workMaster.referenceId)
      ?.requiresLot ?? false
  );
};

const formatWorkMasterLabel = (workMaster: WorkMaster): string => {
  const referenceName = referenceStore.getShortNameById(workMaster.referenceId);
  const modeName = workMasterStore.workmasterModes.find(
    (mode) => mode.id === workMaster.mode,
  )?.value;

  return `${referenceName}  (Base = ${workMaster.baseQuantity} )  ${modeName}`;
};

const initialValues = computed<FormValues>(() => ({
  ...props.createWorkOrderDto,
  plannedDate:
    props.createWorkOrderDto.plannedDate instanceof Date
      ? props.createWorkOrderDto.plannedDate
      : null,
  lotCode: props.createWorkOrderDto.lotCode ?? "",
}));

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "workMasterId",
        label: t("production.components.ruta"),
        type: FormFieldType.Select,
        props: {
          options: props.filteredWorkMasters
            ? props.filteredWorkMasters
            : (workMasterStore.workmasters ?? []),
          optionValue: "id",
          optionLabel: formatWorkMasterLabel,
          virtualScrollerOptions: { itemSize: 38 },
          filter: true,
        },
        validation: Yup.string().required(
          t("production.validation.laRutaDeFabricacioEsObligatoria"),
        ),
      },
    ],
  },
  {
    fields: [
      {
        name: "plannedQuantity",
        label: t("production.components.quantitat"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
        validation: Yup.number()
          .min(1, t("production.validation.laQuantitatHaDeSerSuperiorA0"))
          .required(t("production.validation.laQuanitatEsObligatoria")),
      },
    ],
  },
  {
    fields: [
      {
        name: "plannedDate",
        label: t("production.components.dataPrevista"),
        type: FormFieldType.Date,
        props: { dateFormat: "dd/mm/yy" },
        validation: Yup.date()
          .typeError(t("production.validation.laDataPrevistaEsObligatoria"))
          .required(t("production.validation.laDataPrevistaEsObligatoria")),
      },
    ],
  },
  {
    fields: [
      {
        name: "comment",
        label: t("production.components.comentariFabriacio"),
        type: FormFieldType.Textarea,
      },
    ],
  },
  {
    // Registered while hidden so the lot code keeps its value when the
    // selected route changes; it is only submitted when the route's
    // reference requires a lot.
    section: "lot",
    fields: [{ name: "lotCode", label: "", type: FormFieldType.Custom }],
  },
]);

const submit = (values: FormValues): void => {
  const workMasterId = stringValue(
    values.workMasterId,
    props.createWorkOrderDto.workMasterId,
  );
  const lotCode = stringValue(values.lotCode, "");

  emit("submit", {
    ...props.createWorkOrderDto,
    workMasterId,
    plannedQuantity: finiteNumberValue(
      values.plannedQuantity,
      props.createWorkOrderDto.plannedQuantity,
    ),
    plannedDate: dateValue(values.plannedDate, null),
    comment: stringValue(values.comment, ""),
    lotCode: referenceRequiresLot(workMasterId) ? lotCode : undefined,
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #section-lot="{ values, setFieldValue, disabled }">
      <div v-if="referenceRequiresLot(stringValue(values.workMasterId, ''))">
        <label class="block text-900 mb-2" :for="lotCodeInputId">
          {{ t("production.components.codiLot") }}
        </label>
        <InputText
          :id="lotCodeInputId"
          class="w-full"
          :model-value="stringValue(values.lotCode, '')"
          :disabled="disabled"
          @update:model-value="setFieldValue('lotCode', $event ?? '')"
        />
      </div>
    </template>
  </Form>
</template>
