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
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import type { WorkOrderPhaseBillOfMaterials } from "../types";

const props = defineProps<{
  bomItem: WorkOrderPhaseBillOfMaterials;
}>();

const emit = defineEmits<{
  (event: "submit", bomItem: WorkOrderPhaseBillOfMaterials): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const decimalProps = { locale: "en-US", minFractionDigits: 2 } as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "referenceId",
        label: t("production.components.material"),
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("production.validation.elMaterialDeConsumEsObligatori"),
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
        props: { locale: "en-US", minFractionDigits: 0 },
        validation: Yup.number()
          .typeError(t("production.validation.laQuantitatAConsumirEsObligatoria"))
          .min(1, t("production.validation.laQuantitatAConsumirHaDeSerPositiva"))
          .required(t("production.validation.laQuantitatAConsumirEsObligatoria")),
      },
      {
        name: "width",
        label: t("production.components.ampladaMm"),
        type: FormFieldType.Number,
        props: decimalProps,
      },
      {
        name: "height",
        label: t("production.components.alcadaMm"),
        type: FormFieldType.Number,
        props: decimalProps,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "length",
        label: t("production.components.longitudMm"),
        type: FormFieldType.Number,
        props: decimalProps,
      },
      {
        name: "diameter",
        label: t("production.components.diametreMm"),
        type: FormFieldType.Number,
        props: decimalProps,
      },
      {
        name: "thickness",
        label: t("production.components.gruixMm"),
        type: FormFieldType.Number,
        props: decimalProps,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.bomItem,
    referenceId: stringValue(values.referenceId, props.bomItem.referenceId),
    quantity: finiteNumberValue(values.quantity, props.bomItem.quantity),
    width: finiteNumberValue(values.width, props.bomItem.width),
    height: finiteNumberValue(values.height, props.bomItem.height),
    length: finiteNumberValue(values.length, props.bomItem.length),
    diameter: finiteNumberValue(values.diameter, props.bomItem.diameter),
    thickness: finiteNumberValue(values.thickness, props.bomItem.thickness),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="bomItem"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #field-referenceId="{ value, setValue, disabled, inputId }">
      <DropdownReference
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :full-name="true"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>
  </Form>
</template>
