<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  finiteNumberValue,
  nullableStringValue,
  optionalStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { useReferenceStore } from "../../shared/store/reference";
import { useReferenceTypeStore } from "../../shared/store/referenceType";
import { useTaxesStore } from "../../shared/store/tax";
import {
  type Reference,
  ReferenceCategoryEnum,
  type ReferenceType,
} from "../../shared/types";
import Services from "../services";

const props = defineProps<{
  reference: Reference;
}>();

const emit = defineEmits<{
  (event: "submit", reference: Reference): void;
}>();

const referenceStore = useReferenceStore();
const referenceTypeStore = useReferenceTypeStore();
const taxesStore = useTaxesStore();
const plantModelStore = usePlantModelStore();
const { t } = useI18n();
const form = ref<{ submit: () => void } | null>(null);
const lastCostDisabled = ref(false);

const currencyProps = {
  currency: "EUR",
  locale: "en-US",
  minFractionDigits: 2,
} as const;

const referenceTypeLabel = (referenceType: ReferenceType): string =>
  `${referenceType.name} - ${referenceType.description}`;

const commonRows = (): FormRowConfig[] => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "code",
        label: t("purchase.materials.fields.code"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("purchase.materials.validation.codeRequired"))
          .max(50, t("purchase.materials.validation.codeMaxLength")),
      },
      {
        name: "description",
        label: t("purchase.materials.fields.description"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("purchase.materials.validation.descriptionRequired"))
          .max(250, t("purchase.materials.validation.descriptionMaxLength")),
      },
      {
        name: "categoryName",
        label: t("purchase.materials.fields.category"),
        type: FormFieldType.Select,
        disabled: true,
        props: {
          options: referenceStore.referenceCategories,
          optionValue: "code",
          optionLabel: "description",
          placeholder: t("shared.dropdowns.selectPlaceholder"),
        },
      },
    ],
  },
];

const materialRows = (): FormRowConfig[] => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "referenceTypeId",
        label: t("purchase.materials.fields.materialType"),
        type: FormFieldType.Select,
        props: {
          options: referenceTypeStore.referenceTypes ?? [],
          optionValue: "id",
          optionLabel: referenceTypeLabel,
          placeholder: t("shared.dropdowns.selectPlaceholder"),
          showClear: true,
          filter: true,
          filterFields: ["name", "description"],
        },
        validation: Yup.string().required(
          t("purchase.materials.validation.referenceTypeRequired"),
        ),
      },
      {
        name: "referenceFormatId",
        label: t("purchase.materials.fields.format"),
        type: FormFieldType.Select,
        props: {
          options: referenceStore.referenceFormats ?? [],
          optionValue: "id",
          optionLabel: "description",
        },
        validation: Yup.string().required(
          t("purchase.materials.validation.formatRequired"),
        ),
      },
      {
        name: "taxId",
        label: t("purchase.materials.fields.tax"),
        type: FormFieldType.Select,
        props: {
          options: taxesStore.taxes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.materials.validation.taxRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "lastCost",
        label: t("purchase.materials.fields.lastCost"),
        type: FormFieldType.Currency,
        props: currencyProps,
        disabled: lastCostDisabled.value,
      },
      {
        name: "disabled",
        label: t("purchase.materials.fields.disabled"),
        type: FormFieldType.Checkbox,
      },
    ],
  },
];

const toolRows = (): FormRowConfig[] => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "taxId",
        label: t("purchase.materials.fields.tax"),
        type: FormFieldType.Select,
        props: {
          options: taxesStore.taxes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.materials.validation.taxRequired"),
        ),
      },
      {
        name: "areaId",
        label: t("purchase.materials.fields.productionArea"),
        type: FormFieldType.Select,
        props: {
          options: plantModelStore.areas ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
        validation: Yup.string().required(
          t("purchase.materials.validation.areaRequired"),
        ),
      },
    ],
  },
];

const serviceRows = (): FormRowConfig[] => [
  {
    columns: { mobile: 1, desktop: 4 },
    fields: [
      {
        name: "price",
        label: t("purchase.materials.fields.servicePrice"),
        type: FormFieldType.Currency,
        props: currencyProps,
        validation: Yup.number().required(
          t("purchase.materials.validation.priceRequired"),
        ),
      },
      {
        name: "transportAmount",
        label: t("purchase.materials.fields.transportPrice"),
        type: FormFieldType.Currency,
        props: currencyProps,
        validation: Yup.number().required(
          t("purchase.materials.validation.transportPriceRequired"),
        ),
      },
      {
        name: "taxId",
        label: t("purchase.materials.fields.tax"),
        type: FormFieldType.Select,
        props: {
          options: taxesStore.taxes ?? [],
          optionValue: "id",
          optionLabel: "name",
        },
      },
      {
        name: "disabled",
        label: t("purchase.materials.fields.disabled"),
        type: FormFieldType.Checkbox,
      },
    ],
  },
];

const rows = computed<FormRowConfig[]>(() => {
  let categoryRows: FormRowConfig[] = [];
  if (props.reference.categoryName === ReferenceCategoryEnum.MATERIAL) {
    categoryRows = materialRows();
  } else if (props.reference.categoryName === ReferenceCategoryEnum.TOOL) {
    categoryRows = toolRows();
  } else if (props.reference.categoryName === ReferenceCategoryEnum.SERVICE) {
    categoryRows = serviceRows();
  }

  return [...commonRows(), ...categoryRows];
});

onMounted(async () => {
  if (props.reference.categoryName === ReferenceCategoryEnum.MATERIAL) {
    await referenceTypeStore.fetchAll();
    const receipts = await Services.Receipt.GetByReferenceId(props.reference.id);
    lastCostDisabled.value = Boolean(receipts?.length);
  } else if (props.reference.categoryName === ReferenceCategoryEnum.TOOL) {
    if (!taxesStore.taxes) await taxesStore.fetchAll();
    await plantModelStore.fetchAreas();
  } else if (props.reference.categoryName === ReferenceCategoryEnum.SERVICE) {
    if (!taxesStore.taxes) await taxesStore.fetchAll();
  }
});

const submit = (values: FormValues): void => {
  const reference: Reference = {
    ...props.reference,
    code: stringValue(values.code, props.reference.code),
    description: stringValue(values.description, props.reference.description),
    categoryName: stringValue(
      values.categoryName,
      props.reference.categoryName,
    ),
  };

  if (reference.categoryName === ReferenceCategoryEnum.MATERIAL) {
    reference.referenceTypeId = nullableStringValue(
      values.referenceTypeId,
      props.reference.referenceTypeId,
    );
    reference.referenceFormatId = nullableStringValue(
      values.referenceFormatId,
      props.reference.referenceFormatId,
    );
    reference.taxId = optionalStringValue(values.taxId, props.reference.taxId);
    reference.lastCost = finiteNumberValue(
      values.lastCost,
      props.reference.lastCost,
    );
    reference.disabled = booleanValue(
      values.disabled,
      props.reference.disabled,
    );
  } else if (reference.categoryName === ReferenceCategoryEnum.TOOL) {
    reference.taxId = optionalStringValue(values.taxId, props.reference.taxId);
    reference.areaId = nullableStringValue(
      values.areaId,
      props.reference.areaId,
    );
  } else if (reference.categoryName === ReferenceCategoryEnum.SERVICE) {
    reference.price = finiteNumberValue(values.price, props.reference.price);
    reference.transportAmount = finiteNumberValue(
      values.transportAmount,
      props.reference.transportAmount,
    );
    reference.taxId = optionalStringValue(values.taxId, props.reference.taxId);
    reference.disabled = booleanValue(
      values.disabled,
      props.reference.disabled,
    );
  }

  emit("submit", reference);
};

const submitForm = (): void => form.value?.submit();
</script>

<template>
  <div>
    <Button
      :label="t('purchase.materials.actions.save')"
      class="grid_add_row_button"
      size="small"
      @click="submitForm"
    />
    <br />
  </div>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="reference"
    :show-submit="false"
    :show-cancel="false"
    @submit="submit"
  />
</template>
