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
import { round } from "lodash";
import { computed, onUnmounted, ref, shallowRef, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { FormActionMode } from "../../../types/component";
import DropdownWorkmasters from "../../production/components/DropdownWorkmasters.vue";
import { useWorkMasterStore } from "../../production/store/workmaster";
import type {
  ProductionCosts,
  WorkmastersOptionsLoadedPayload,
} from "../../production/types";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { useReferenceStore } from "../../shared/store/reference";
import type {
  Budget,
  BudgetDetail,
  DetailPhaseProfit,
  SalesOrderDetail,
  SalesOrderHeader,
} from "../types";
import TableWorkmasterProfit from "./TableWorkmasterProfit.vue";

type OrderDetail = BudgetDetail | SalesOrderDetail;

const props = defineProps<{
  formAction: FormActionMode;
  header: Budget | SalesOrderHeader;
  detail: BudgetDetail | SalesOrderDetail;
  readonly?: boolean;
}>();

const emit = defineEmits<{
  (event: "submit", detail: BudgetDetail | SalesOrderDetail): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();
const workmasterStore = useWorkMasterStore();
const referenceStore = useReferenceStore();
const form = shallowRef<{
  setFieldValue: (name: string, value: unknown) => void;
  setValues: (values: FormValues) => void;
} | null>(null);
const activeTab = ref("0");

/** Values read and written by the line calculations. */
interface DetailCalculationValues {
  referenceId: string;
  workMasterId: string | null;
  description: string;
  referencePrice: number;
  quantity: number;
  profit: number;
  productionProfit: number;
  materialProfit: number;
  externalProfit: number;
  discount: number;
  productionCost: number;
  materialCost: number;
  transportCost: number;
  serviceCost: number;
  unitCost: number;
  unitPrice: number;
  totalCost: number;
  amount: number;
}

/** Working copy plus the fields a calculation wrote, applied in one batch. */
interface DetailCalculation {
  values: DetailCalculationValues;
  changes: Partial<DetailCalculationValues>;
}

// `referencePrice` is display-only (not part of the detail) and, as in the
// legacy form, starts at 0 until a reference is chosen.
const createInitialValues = (
  detail: OrderDetail,
  formAction: FormActionMode,
): FormValues => {
  const values = {
    referenceId: detail.referenceId,
    workMasterId: detail.workMasterId,
    description: detail.description,
    referencePrice: 0,
    quantity: detail.quantity,
    profit: detail.profit,
    productionProfit: detail.productionProfit,
    materialProfit: detail.materialProfit,
    externalProfit: detail.externalProfit,
    discount: detail.discount,
    productionCost: detail.productionCost,
    materialCost: detail.materialCost,
    transportCost: detail.transportCost,
    serviceCost: detail.serviceCost,
    unitCost: detail.unitCost,
    unitPrice: detail.unitPrice,
    totalCost: detail.totalCost,
    amount: detail.amount,
    userNotes: detail.userNotes,
  };

  // Initial regularisation of lines saved before production cost and
  // production/external profit were stored separately.
  if (
    formAction === FormActionMode.EDIT &&
    detail.productionCost === 0 &&
    detail.productionProfit === 0 &&
    detail.workMasterId !== null
  ) {
    values.productionCost =
      detail.totalCost - (detail.serviceCost + detail.transportCost);
    if (values.productionCost > 0) values.productionProfit = detail.profit;
    if (detail.serviceCost > 0 && detail.transportCost > 0) {
      values.externalProfit = detail.profit;
    }
  }

  return values;
};

const initialValues = shallowRef<FormValues>(
  createInitialValues(props.detail, props.formAction),
);
// Latest values needed after an await (cost requests).
const latestValues = shallowRef<FormValues>({ ...initialValues.value });

// Feature-owned state for the per-phase margins grid, which stays outside the
// form fields and is merged into the payload at submit.
const phaseProfits = ref<DetailPhaseProfit[]>([
  ...(props.detail.phaseProfits ?? []),
]);
const currentReferenceId = ref(stringValue(initialValues.value.referenceId, ""));
const currentWorkMasterId = ref(
  nullableStringValue(initialValues.value.workMasterId, null),
);
const currentQuantity = ref(finiteNumberValue(initialValues.value.quantity, 0));

/**
 * Set only when the user intentionally changes the reference. Cleared as soon
 * as DropdownWorkmasters reports its fresh options, so rapid changes never
 * apply a stale result.
 */
let autoSelectWorkmaster = false;
let costRequestSequence = 0;
let suppressCallbacks = false;

watch(
  () => props.detail,
  (detail) => {
    costRequestSequence += 1;
    autoSelectWorkmaster = false;
    initialValues.value = createInitialValues(detail, props.formAction);
    latestValues.value = { ...initialValues.value };
    phaseProfits.value = [...(detail.phaseProfits ?? [])];
    currentReferenceId.value = stringValue(initialValues.value.referenceId, "");
    currentWorkMasterId.value = nullableStringValue(
      initialValues.value.workMasterId,
      null,
    );
    currentQuantity.value = finiteNumberValue(initialValues.value.quantity, 0);
  },
);

onUnmounted(() => {
  costRequestSequence += 1;
});

const setFormValues = (values: FormValues): void => {
  suppressCallbacks = true;
  try {
    form.value?.setValues(values);
  } finally {
    suppressCallbacks = false;
  }
};

// Cleared numbers count as 0, as they did in the legacy arithmetic.
const toCalculation = (values: Readonly<FormValues>): DetailCalculation => ({
  values: {
    referenceId: stringValue(values.referenceId, ""),
    workMasterId: nullableStringValue(values.workMasterId, null),
    description: stringValue(values.description, ""),
    referencePrice: finiteNumberValue(values.referencePrice, 0),
    quantity: finiteNumberValue(values.quantity, 0),
    profit: finiteNumberValue(values.profit, 0),
    productionProfit: finiteNumberValue(values.productionProfit, 0),
    materialProfit: finiteNumberValue(values.materialProfit, 0),
    externalProfit: finiteNumberValue(values.externalProfit, 0),
    discount: finiteNumberValue(values.discount, 0),
    productionCost: finiteNumberValue(values.productionCost, 0),
    materialCost: finiteNumberValue(values.materialCost, 0),
    transportCost: finiteNumberValue(values.transportCost, 0),
    serviceCost: finiteNumberValue(values.serviceCost, 0),
    unitCost: finiteNumberValue(values.unitCost, 0),
    unitPrice: finiteNumberValue(values.unitPrice, 0),
    totalCost: finiteNumberValue(values.totalCost, 0),
    amount: finiteNumberValue(values.amount, 0),
  },
  changes: {},
});

const write = <TKey extends keyof DetailCalculationValues>(
  calculation: DetailCalculation,
  key: TKey,
  value: DetailCalculationValues[TKey],
): void => {
  calculation.values[key] = value;
  calculation.changes[key] = value;
};

const applyCalculation = (
  calculation: DetailCalculation,
  base: Readonly<FormValues>,
): void => {
  const changes: FormValues = { ...calculation.changes };
  if (Object.keys(changes).length > 0) setFormValues(changes);
  latestValues.value = { ...base, ...changes };
};

const findReference = (referenceId: string) =>
  referenceStore.references?.find((r) => r.id === referenceId);

const updateImports = (calculation: DetailCalculation): void => {
  const v = calculation.values;

  // starting from unit cost; with no cost, the reference price
  let unitPrice = v.unitCost;
  if (unitPrice === 0) {
    unitPrice = findReference(v.referenceId)?.price || 0;
  }

  // apply profit
  if (v.productionProfit > 0 || v.externalProfit > 0 || v.materialProfit > 0) {
    unitPrice =
      (v.productionCost * (1 + v.productionProfit / 100) +
        v.materialCost * (1 + v.materialProfit / 100) +
        (v.transportCost + v.serviceCost) * (1 + v.externalProfit / 100)) /
      v.quantity;

    if (v.unitCost > 0) {
      write(calculation, "profit", round((unitPrice * 100) / v.unitCost - 100, 2));
    }
  }

  // apply discount
  if (v.discount > 0) {
    unitPrice *= 1 - v.discount / 100;
  }

  // round it & calculate total price (amount)
  write(calculation, "unitPrice", round(unitPrice, 2));
  write(calculation, "amount", round(v.unitPrice * v.quantity, 2));
};

const updateCosts = (calculation: DetailCalculation): void => {
  const v = calculation.values;
  write(
    calculation,
    "totalCost",
    v.productionCost + v.serviceCost + v.materialCost + v.transportCost,
  );
  write(calculation, "unitCost", v.totalCost / v.quantity);
  updateImports(calculation);
};

const updateUnitPrice = (calculation: DetailCalculation): void => {
  const v = calculation.values;
  write(calculation, "amount", round(v.unitPrice * v.quantity, 2));
};

const getReferenceInfo = (calculation: DetailCalculation): void => {
  const reference = findReference(calculation.values.referenceId);
  if (reference) {
    write(calculation, "description", reference.description);
    write(calculation, "referencePrice", reference.price);
    write(calculation, "unitPrice", reference.price);
    write(calculation, "workMasterId", null);
    write(calculation, "unitCost", reference.lastCost);
    updateImports(calculation);
  } else {
    write(calculation, "referencePrice", 0);
    write(calculation, "description", "");
    write(calculation, "unitPrice", 0);
    write(calculation, "workMasterId", null);
    write(calculation, "unitCost", 0);
    write(calculation, "totalCost", 0);
    write(calculation, "amount", 0);
  }
  // The next optionsLoaded event may auto-select the workmaster. Never armed
  // when an existing detail is loaded.
  autoSelectWorkmaster = true;
};

const applyWorkmasterCosts = (
  calculation: DetailCalculation,
  costs: ProductionCosts,
): void => {
  const v = calculation.values;
  write(calculation, "transportCost", costs.externalTransportCost);
  write(calculation, "serviceCost", costs.externalServiceCost);
  write(calculation, "productionCost", costs.machineCost + costs.operatorCost);
  write(calculation, "materialCost", costs.materialCost);
  write(
    calculation,
    "totalCost",
    v.transportCost + v.serviceCost + v.productionCost + v.materialCost,
  );
  write(calculation, "unitCost", v.totalCost / v.quantity);
  updateImports(calculation);
};

const clearWorkmasterCosts = (calculation: DetailCalculation): void => {
  getReferenceInfo(calculation);
  write(calculation, "transportCost", 0);
  write(calculation, "serviceCost", 0);
  write(calculation, "productionCost", 0);
  write(calculation, "materialCost", 0);
  write(calculation, "totalCost", 0);
};

const recalculate = (
  values: Readonly<FormValues>,
  calculate: (calculation: DetailCalculation) => void,
): void => {
  const calculation = toCalculation(values);
  calculate(calculation);
  applyCalculation(calculation, values);
};

const loadWorkmasterCosts = async (
  values: Readonly<FormValues>,
): Promise<void> => {
  const requestSequence = ++costRequestSequence;
  const calculation = toCalculation(values);
  const workMasterId = calculation.values.workMasterId;

  if (!workMasterId) {
    clearWorkmasterCosts(calculation);
    applyCalculation(calculation, values);
    return;
  }

  const response = await workmasterStore.getCosts(
    workMasterId,
    calculation.values.quantity,
  );
  if (requestSequence !== costRequestSequence) return;
  if (!response.result || !response.content) return;

  const latest = latestValues.value;
  const latestCalculation = toCalculation(latest);
  applyWorkmasterCosts(latestCalculation, response.content);
  applyCalculation(latestCalculation, latest);
};

const trackValues = (_value: unknown, values: Readonly<FormValues>): void => {
  latestValues.value = { ...values };
};

const onReferenceChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  trackValues(value, values);
  currentReferenceId.value = stringValue(value, "");
  if (suppressCallbacks) return;
  costRequestSequence += 1;
  recalculate(values, getReferenceInfo);
};

const onWorkMasterChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  trackValues(value, values);
  currentWorkMasterId.value = nullableStringValue(value, null);
  if (suppressCallbacks) return;
  void loadWorkmasterCosts(values);
};

const onImportSourceChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  trackValues(value, values);
  if (suppressCallbacks) return;
  recalculate(values, updateImports);
};

const onExternalCostChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  trackValues(value, values);
  if (suppressCallbacks) return;
  recalculate(values, updateCosts);
};

const onQuantityChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  trackValues(value, values);
  currentQuantity.value = finiteNumberValue(value, 0);
  if (suppressCallbacks) return;

  const calculation = toCalculation(values);
  if (!calculation.values.quantity) write(calculation, "quantity", 1);
  if (calculation.values.workMasterId) {
    void loadWorkmasterCosts({
      ...values,
      quantity: calculation.values.quantity,
    });
  }
  updateImports(calculation);
  applyCalculation(calculation, values);
};

const onUnitPriceChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  trackValues(value, values);
  if (suppressCallbacks) return;
  recalculate(values, updateUnitPrice);
};

const onWorkmasterOptionsLoaded = async (
  payload: WorkmastersOptionsLoadedPayload,
): Promise<void> => {
  // Only react when an intentional user reference change armed the flag, and
  // discard events that belong to a previous (stale) reference fetch.
  if (!autoSelectWorkmaster) return;
  if (payload.referenceId !== currentReferenceId.value) return;
  autoSelectWorkmaster = false;

  // Auto-select only an unambiguous choice (exactly one active workmaster);
  // with 0 or 2+ options the costs are reset so stale values are never saved.
  const workMasterId = payload.options.length === 1 ? payload.options[0].id : null;
  setFormValues({ workMasterId });
  latestValues.value = { ...latestValues.value, workMasterId };
  await loadWorkmasterCosts(latestValues.value);
};

const copyProfitAverage = (profitAverage: number): void => {
  // Goes through the productionProfit change pipeline (recalculates imports).
  form.value?.setFieldValue("productionProfit", profitAverage);
  activeTab.value = "0";
};

const onPhaseProfitsUpdate = (profits: DetailPhaseProfit[]): void => {
  phaseProfits.value = profits;
};

const integerProps = { locale: "en-US", minFractionDigits: 0 } as const;
const decimalProps = { locale: "en-US", minFractionDigits: 2 } as const;
const currencyProps = {
  locale: "en-US",
  minFractionDigits: 2,
  suffix: " €",
} as const;
const costColumns = { mobile: 1, tablet: 4, desktop: 7 };
const withoutWorkmaster = (values: Readonly<FormValues>): boolean =>
  nullableStringValue(values.workMasterId, null) === null;

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "referenceId",
        label: t("sales.components.referencia"),
        type: FormFieldType.Custom,
        onChange: onReferenceChange,
      },
      {
        name: "referencePrice",
        label: t("sales.components.preu"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: true,
      },
      {
        name: "workMasterId",
        label: t("sales.components.rutaDeFabricacio"),
        type: FormFieldType.Custom,
        onChange: onWorkMasterChange,
      },
    ],
  },
  {
    columns: costColumns,
    fields: [
      {
        name: "productionCost",
        label: t("sales.components.costProduccio"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: true,
      },
      {
        name: "productionProfit",
        label: t("sales.components.beneficiProduccio"),
        type: FormFieldType.Number,
        props: decimalProps,
        onChange: onImportSourceChange,
      },
      {
        name: "materialCost",
        label: t("sales.components.costMaterial"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: true,
      },
      {
        name: "materialProfit",
        label: t("sales.components.beneficiMaterial"),
        type: FormFieldType.Number,
        props: decimalProps,
        onChange: onImportSourceChange,
      },
      {
        name: "serviceCost",
        label: t("sales.components.costServei"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: withoutWorkmaster,
        onChange: onExternalCostChange,
      },
      {
        name: "transportCost",
        label: t("sales.components.costTransport"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: withoutWorkmaster,
        onChange: onExternalCostChange,
      },
      {
        name: "externalProfit",
        label: t("sales.components.beneficiExterns"),
        type: FormFieldType.Number,
        props: decimalProps,
        onChange: onImportSourceChange,
      },
    ],
  },
  {
    columns: costColumns,
    fields: [
      {
        name: "quantity",
        label: t("sales.components.quantitat"),
        type: FormFieldType.Number,
        props: integerProps,
        onChange: onQuantityChange,
        validation: Yup.number()
          .required(t("sales.validation.quantityRequired"))
          .min(1, t("sales.validation.quantityPositive")),
      },
      {
        name: "unitCost",
        label: t("sales.components.costUnitari"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: true,
      },
      {
        name: "totalCost",
        label: t("sales.components.costTotal"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: true,
      },
      {
        name: "profit",
        label: t("sales.components.benefici"),
        type: FormFieldType.Number,
        props: integerProps,
        disabled: true,
        validation: Yup.number().required(t("sales.validation.profitRequired")),
      },
      {
        name: "discount",
        label: t("sales.components.descompte"),
        type: FormFieldType.Number,
        props: decimalProps,
        onChange: onImportSourceChange,
        validation: Yup.number().required(
          t("sales.validation.discountRequired"),
        ),
      },
      {
        name: "unitPrice",
        label: t("sales.components.preuUnitari"),
        type: FormFieldType.Number,
        props: currencyProps,
        onChange: onUnitPriceChange,
        validation: Yup.number().required(
          t("sales.validation.unitPriceRequired"),
        ),
      },
      {
        name: "amount",
        label: t("sales.components.total"),
        type: FormFieldType.Number,
        props: currencyProps,
        disabled: true,
        validation: Yup.number()
          .required(t("sales.validation.totalRequired"))
          .min(0, t("sales.validation.totalNotNegative")),
      },
    ],
  },
  {
    fields: [
      {
        name: "description",
        label: t("sales.components.descripcio"),
        type: FormFieldType.Text,
        onChange: trackValues,
      },
    ],
  },
  {
    fields: [
      {
        name: "userNotes",
        label: t("sales.components.notesInternes"),
        type: FormFieldType.Textarea,
        props: { rows: 3, placeholder: t("sales.components.notesInternes") },
        onChange: trackValues,
      },
    ],
  },
]);

// Numbers fall back to 0, consistent with the calculations above.
const detailValues = (values: Readonly<FormValues>): OrderDetail => ({
  ...props.detail,
  referenceId: stringValue(values.referenceId, ""),
  workMasterId: nullableStringValue(
    values.workMasterId,
    props.detail.workMasterId,
  ),
  description: stringValue(values.description, props.detail.description),
  quantity: finiteNumberValue(values.quantity, 0),
  profit: finiteNumberValue(values.profit, 0),
  productionProfit: finiteNumberValue(values.productionProfit, 0),
  materialProfit: finiteNumberValue(values.materialProfit, 0),
  externalProfit: finiteNumberValue(values.externalProfit, 0),
  discount: finiteNumberValue(values.discount, 0),
  productionCost: finiteNumberValue(values.productionCost, 0),
  materialCost: finiteNumberValue(values.materialCost, 0),
  transportCost: finiteNumberValue(values.transportCost, 0),
  serviceCost: finiteNumberValue(values.serviceCost, 0),
  unitCost: finiteNumberValue(values.unitCost, 0),
  unitPrice: finiteNumberValue(values.unitPrice, 0),
  totalCost: finiteNumberValue(values.totalCost, 0),
  amount: finiteNumberValue(values.amount, 0),
  userNotes: stringValue(values.userNotes, props.detail.userNotes),
  phaseProfits: phaseProfits.value,
});

const submit = (values: FormValues): void => {
  emit("submit", detailValues(values));
};
</script>

<template>
  <Tabs v-model:value="activeTab">
    <TabList>
      <Tab value="0">{{ t("sales.components.referencia") }}</Tab>
      <Tab value="1">{{ t("sales.components.marges") }}</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <Form
          ref="form"
          :rows="rows"
          :initial-values="initialValues"
          :disabled="readonly"
          @submit="submit"
          @cancel="emit('cancel')"
        >
          <template #field-referenceId="{ value, setValue, disabled, inputId }">
            <DropdownReference
              :input-id="inputId"
              label=""
              :model-value="typeof value === 'string' ? value : null"
              :customer-id="header.customerId"
              :full-name="true"
              :disabled="disabled"
              @update:model-value="setValue"
            />
          </template>

          <template #field-workMasterId="{ value, setValue, disabled, inputId }">
            <!-- Only forward `disabled` when set: the component disables
                 itself while loading and $attrs would override that. -->
            <DropdownWorkmasters
              :input-id="inputId"
              label=""
              :model-value="typeof value === 'string' ? value : null"
              :reference-id="currentReferenceId"
              :active-by-reference="true"
              v-bind="disabled ? { disabled: true } : {}"
              @update:model-value="setValue"
              @options-loaded="onWorkmasterOptionsLoaded"
            />
          </template>
        </Form>
      </TabPanel>
      <TabPanel value="1">
        <TableWorkmasterProfit
          :work-master-id="currentWorkMasterId"
          :quantity="currentQuantity"
          :phase-profits="phaseProfits"
          @updateProfitAverage="copyProfitAverage"
          @update:phaseProfits="onPhaseProfitsUpdate"
        />
      </TabPanel>
    </TabPanels>
  </Tabs>
</template>
