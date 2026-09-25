<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  finiteNumberValue,
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { useReferenceStore } from "../../shared/store/reference";
import type { Inventory } from "../types";
import DropdownWarehousesWithLocations from "./DropdownWarehousesWithLocations.vue";
import SelectorLot from "./SelectorLot.vue";

const props = defineProps<{
  newMovement: Inventory;
}>();

const emit = defineEmits<{
  (event: "submit", newMovement: Inventory): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const referenceStore = useReferenceStore();
const form = ref<InstanceType<typeof Form> | null>(null);

onMounted(async () => {
  if (!referenceStore.references || referenceStore.references.length === 0) {
    await referenceStore.fetchReferences();
  }
});

const dimensionProps = { locale: "en-US", minFractionDigits: 2 } as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "referenceId",
        label: t("warehouse.fields.material"),
        type: FormFieldType.Custom,
        validation: Yup.string()
          .nullable()
          .required(t("warehouse.validation.referenceRequired")),
        // A lot belongs to one reference; drop the previous selection.
        onChange: () => {
          form.value?.setFieldValue("lotId", null);
          form.value?.setFieldValue("lotCode", "");
        },
      },
    ],
  },
  {
    fields: [
      {
        name: "locationId",
        label: t("warehouse.fields.location"),
        type: FormFieldType.Custom,
        validation: Yup.string()
          .nullable()
          .required(
            t("warehouse.validation.locationRequired"),
          ),
      },
    ],
  },
  {
    section: "lot",
    fields: [
      { name: "lotId", label: "", type: FormFieldType.Custom },
      { name: "lotCode", label: "", type: FormFieldType.Custom },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "newQuantity",
        label: t("warehouse.fields.quantity"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 0 },
        validation: Yup.number()
          .typeError(t("warehouse.validation.quantityMinimum"))
          .min(1, t("warehouse.validation.quantityMinimum"))
          .required(t("warehouse.validation.quantityMinimum")),
      },
      {
        name: "width",
        label: t("warehouse.fields.widthMm"),
        type: FormFieldType.Number,
        props: dimensionProps,
      },
      {
        name: "height",
        label: t("warehouse.fields.heightMm"),
        type: FormFieldType.Number,
        props: dimensionProps,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "length",
        label: t("warehouse.fields.lengthMm"),
        type: FormFieldType.Number,
        props: dimensionProps,
      },
      {
        name: "diameter",
        label: t("warehouse.fields.diameterMm"),
        type: FormFieldType.Number,
        props: dimensionProps,
      },
      {
        name: "thickness",
        label: t("warehouse.fields.thicknessMm"),
        type: FormFieldType.Number,
        props: dimensionProps,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.newMovement,
    referenceId: stringValue(values.referenceId, props.newMovement.referenceId),
    locationId: nullableStringValue(
      values.locationId,
      props.newMovement.locationId,
    ),
    lotId: nullableStringValue(values.lotId, props.newMovement.lotId ?? null),
    lotCode: stringValue(values.lotCode, props.newMovement.lotCode ?? ""),
    newQuantity: finiteNumberValue(
      values.newQuantity,
      props.newMovement.newQuantity,
    ),
    width: finiteNumberValue(values.width, props.newMovement.width),
    height: finiteNumberValue(values.height, props.newMovement.height),
    length: finiteNumberValue(values.length, props.newMovement.length),
    diameter: finiteNumberValue(values.diameter, props.newMovement.diameter),
    thickness: finiteNumberValue(values.thickness, props.newMovement.thickness),
  });
};
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="newMovement"
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

    <template #field-locationId="{ value, setValue, disabled, inputId }">
      <DropdownWarehousesWithLocations
        :input-id="inputId"
        label=""
        :model-value="typeof value === 'string' ? value : null"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #section-lot="{ values, setFieldValue }">
      <SelectorLot
        :reference-id="stringValue(values.referenceId, '')"
        :model-value="nullableStringValue(values.lotId, null)"
        @update:model-value="setFieldValue('lotId', $event)"
        @update:lot-code="setFieldValue('lotCode', $event)"
      />
    </template>
  </Form>
</template>
