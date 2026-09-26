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
  stringValue,
} from "@/components/forms/value-utils";
import { computed, onMounted, onUnmounted, shallowRef, useId, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { FormActionMode } from "../../../types/component";
import { useSuppliersStore } from "../../purchase/store/suppliers";
import { useTransportRateStore } from "../../purchase/store/transportRate";
import { useCustomersStore } from "../store/customers";
import type { SalesOrderHeader, SalesOrderTransport } from "../types";

const props = defineProps<{
  formAction: FormActionMode;
  header: SalesOrderHeader;
  transport: SalesOrderTransport;
  customerId: string;
  readonly?: boolean;
}>();

const emit = defineEmits<{
  (event: "submit", transport: SalesOrderTransport): void;
  (event: "cancel"): void;
}>();

interface TransportRateOption {
  id: string;
  label: string;
  price: number;
}

const { t } = useI18n();
const supplierStore = useSuppliersStore();
const transportRateStore = useTransportRateStore();
const customerStore = useCustomersStore();
const form = shallowRef<{
  setValues: (values: FormValues) => void;
} | null>(null);
const destinationInputId = `sales-order-transport-destination-${useId()}`;
let suppressChanges = false;
let initializeSequence = 0;

// Form-only values: the final-customer toggle, the logistic and destination
// suppliers and the order weight used to filter rates are not part of the
// transport; `destination` and `distance` are hidden values set from them.
const createInitialValues = (transport: SalesOrderTransport): FormValues => ({
  ...transport,
  totalWeight: props.header.totalWeight,
  isFinalCustomer: true,
  logisticSupplierId: null,
  destinationSupplierId: null,
});

const initialValues = shallowRef<FormValues>(
  createInitialValues(props.transport),
);
// Latest values the rate options and the displayed distance depend on.
const currentValues = shallowRef<FormValues>({ ...initialValues.value });

const numberProps = { locale: "en-US", minFractionDigits: 2 } as const;

const listCustomer = () =>
  customerStore.customers?.find((item) => item.id === props.header.customerId);

const customerName = computed(
  () => listCustomer()?.comercialName ?? t("sales.components.client"),
);

// The customer loaded individually carries its addresses; the customer list
// does not, so its distance would always be 0. Undefined when the final
// customer is not available.
const finalCustomerDistance = (): number | undefined => {
  const customer =
    customerStore.customer?.id === props.header.customerId
      ? customerStore.customer
      : listCustomer();
  if (!customer) return undefined;
  return customer.address?.find((item) => item.main)?.distanceFromSite || 0;
};

// Distance shown to the user, used to filter rates and saved.
const displayDistance = (values: Readonly<FormValues>): number => {
  if (booleanValue(values.isFinalCustomer, true)) {
    const distance = finalCustomerDistance();
    if (distance !== undefined) return distance;
  } else {
    const supplier = supplierStore.suppliers?.find(
      (item) =>
        item.id === nullableStringValue(values.destinationSupplierId, null),
    );
    if (supplier) return supplier.distanceFromSite ?? 0;
  }
  return 0;
};

const compatibleTransportRates = (
  values: Readonly<FormValues>,
): TransportRateOption[] => {
  if (!transportRateStore.transportRates) return [];
  const now = new Date();

  const validRates = transportRateStore.transportRates.filter((rate) => {
    if (rate.disabled) return false;
    const from = new Date(rate.validFrom);
    const to = new Date(rate.validTo);
    return now >= from && now <= to;
  });

  const weight = finiteNumberValue(values.totalWeight, 0);
  const volume = finiteNumberValue(values.volume, 0);
  const distance = displayDistance(values);

  const compatibleDetails = validRates.flatMap((rate) =>
    (rate.details ?? [])
      .filter((detail) => {
        const weightOk =
          weight === 0 ||
          (detail.minWeight <= weight &&
            (detail.maxWeight === 0 || detail.maxWeight >= weight));
        const volumeOk =
          volume === 0 ||
          (detail.minVolume <= volume &&
            (detail.maxVolume === 0 || detail.maxVolume >= volume));
        const distanceOk =
          distance === 0 ||
          (detail.minDistance <= distance &&
            (detail.maxDistance === 0 || detail.maxDistance >= distance));
        return weightOk && volumeOk && distanceOk;
      })
      .map((detail) => ({ rate, detail })),
  );

  compatibleDetails.sort((a, b) => a.detail.price - b.detail.price);

  return compatibleDetails.map(({ rate, detail }, index) => {
    let label = `${rate.name}`;
    if (rate.description) label += ` (${rate.description})`;

    const limits: string[] = [];
    if (detail.maxWeight > 0)
      limits.push(`${detail.minWeight}-${detail.maxWeight} kg`);
    if (detail.maxDistance > 0)
      limits.push(`${detail.minDistance}-${detail.maxDistance} km`);
    if (detail.maxVolume > 0)
      limits.push(`${detail.minVolume}-${detail.maxVolume} m3`);

    if (limits.length > 0) label += ` | ${limits.join(", ")}`;
    label += ` — ${detail.price} €`;
    if (index === 0) label += ` ⭐️ (${t("sales.customers.bestPrice")})`;

    return { id: detail.id, label, price: detail.price };
  });
};

const rateOptions = computed(() =>
  compatibleTransportRates(currentValues.value),
);

const trackValues = (values: Readonly<FormValues>): void => {
  currentValues.value = { ...values };
};

// Programmatic updates must not re-enter the change handlers.
const applyValues = (values: FormValues): void => {
  suppressChanges = true;
  try {
    form.value?.setValues(values);
  } finally {
    suppressChanges = false;
  }
  currentValues.value = { ...currentValues.value, ...values };
};

const destinationValues = (supplierId: string | null): FormValues => {
  const supplier = supplierStore.suppliers?.find(
    (item) => item.id === supplierId,
  );
  return supplier
    ? {
        destination: supplier.comercialName,
        distance: supplier.distanceFromSite || 0,
      }
    : { destination: "", distance: 0 };
};

const finalCustomerValues = (values: Readonly<FormValues>): FormValues => {
  if (booleanValue(values.isFinalCustomer, true)) {
    const customer = listCustomer();
    return {
      ...(customer
        ? {
            destination: customer.comercialName,
            distance: finalCustomerDistance() ?? 0,
          }
        : {}),
      destinationSupplierId: null,
    };
  }
  const destinationSupplierId = nullableStringValue(
    values.destinationSupplierId,
    null,
  );
  return destinationSupplierId
    ? destinationValues(destinationSupplierId)
    : { destination: "", distance: 0 };
};

const onFinalCustomerChange = (
  _value: unknown,
  values: Readonly<FormValues>,
): void => {
  if (suppressChanges) return;
  trackValues(values);
  applyValues(finalCustomerValues(values));
};

const onDestinationSupplierChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  if (suppressChanges) return;
  trackValues(values);
  applyValues(destinationValues(nullableStringValue(value, null)));
};

const onLogisticSupplierChange = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  if (suppressChanges) return;
  trackValues(values);
  applyValues({ transportRateDetailId: "" });

  const supplierId = nullableStringValue(value, null);
  if (supplierId) {
    void transportRateStore.fetchTransportRatesBySupplierId(supplierId);
  } else {
    transportRateStore.transportRates = [];
  }
};

const onRateChange = (value: unknown, values: Readonly<FormValues>): void => {
  if (suppressChanges) return;
  trackValues(values);
  const rate = compatibleTransportRates(values).find(
    (item) => item.id === nullableStringValue(value, null),
  );
  if (rate) applyValues({ price: rate.price });
};

const onWeightChange = (_value: unknown, values: Readonly<FormValues>): void => {
  trackValues(values);
};

const initialize = async (transport: SalesOrderTransport): Promise<void> => {
  const sequence = ++initializeSequence;
  await supplierStore.fetchLogisticSuppliers();
  await supplierStore.fetchSuppliers();

  // The customer loaded individually has its addresses with distanceFromSite
  // (the customer list does not include them).
  if (props.header.customerId) {
    await customerStore.fetchCustomer(props.header.customerId);
  }
  if (sequence !== initializeSequence) return;

  if (props.formAction === FormActionMode.CREATE && !transport.destination) {
    applyValues(finalCustomerValues(currentValues.value));
    return;
  }

  const customer = listCustomer();
  if (customer && transport.destination === customer.comercialName) {
    applyValues({ isFinalCustomer: true });
    return;
  }

  const destinationSupplier = supplierStore.suppliers?.find(
    (item) => item.comercialName === transport.destination,
  );
  applyValues({
    isFinalCustomer: false,
    ...(destinationSupplier
      ? { destinationSupplierId: destinationSupplier.id }
      : {}),
  });
};

watch(
  () => props.transport,
  (transport) => {
    initialValues.value = createInitialValues(transport);
    currentValues.value = { ...initialValues.value };
    void initialize(transport);
  },
);

onMounted(() => {
  void initialize(props.transport);
});

onUnmounted(() => {
  initializeSequence += 1;
});

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "isFinalCustomer",
        label: t("sales.customers.shipToFinalCustomer", { customer: customerName.value }),
        type: FormFieldType.Checkbox,
        onChange: onFinalCustomerChange,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 2 },
    fields: [
      {
        name: "logisticSupplierId",
        label: t("sales.components.proveidorLogisticTransportista"),
        type: FormFieldType.Select,
        props: {
          options: supplierStore.logisticSuppliers ?? [],
          optionLabel: "comercialName",
          optionValue: "id",
          placeholder: t("sales.components.seleccionaTransportista"),
        },
        onChange: onLogisticSupplierChange,
      },
      {
        name: "transportRateDetailId",
        label: t("sales.components.tarifaDeTransport"),
        type: FormFieldType.Select,
        props: {
          options: rateOptions.value,
          optionLabel: "label",
          optionValue: "id",
          placeholder: t("sales.components.seleccionaTarifa"),
        },
        disabled: (values) => !values.logisticSupplierId,
        onChange: onRateChange,
        validation: Yup.string().required(
          t("sales.validation.transportRateRequired"),
        ),
      },
    ],
  },
  {
    // Shown only when the transport does not go to the final customer; the
    // hidden destination name is registered with it.
    section: "destination",
    fields: [
      {
        name: "destinationSupplierId",
        label: "",
        type: FormFieldType.Custom,
        onChange: onDestinationSupplierChange,
      },
      { name: "destination", label: "", type: FormFieldType.Custom },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "totalWeight",
        label: t("sales.components.pesKg"),
        type: FormFieldType.Number,
        props: numberProps,
        onChange: onWeightChange,
      },
      {
        name: "volume",
        label: t("sales.components.volumM"),
        type: FormFieldType.Number,
        props: numberProps,
        disabled: true,
        validation: Yup.number().min(0, t("sales.validation.volumeNotNegative")),
      },
      {
        name: "distance",
        label: t("sales.components.distanciaKm"),
        type: FormFieldType.Custom,
        disabled: true,
        validation: Yup.number().min(0, t("sales.validation.distanceNotNegative")),
      },
      {
        name: "price",
        label: t("sales.components.preu"),
        type: FormFieldType.Number,
        props: { locale: "en-US", minFractionDigits: 2, suffix: " €" },
        validation: Yup.number()
          .min(0, t("sales.validation.priceNotNegative"))
          .required(t("sales.validation.priceRequired")),
      },
    ],
  },
  {
    fields: [
      {
        name: "description",
        label: t("sales.components.descripcio"),
        type: FormFieldType.Text,
      },
    ],
  },
]);

// Saves the distance shown in the dialog. For the final customer it comes from
// the loaded customer (also when editing without changing the destination);
// a destination supplier keeps the hidden value set by its selection.
const submittedDistance = (values: Readonly<FormValues>): number => {
  const hiddenDistance = finiteNumberValue(
    values.distance,
    props.transport.distance,
  );
  if (!booleanValue(values.isFinalCustomer, true)) return hiddenDistance;
  return finalCustomerDistance() ?? hiddenDistance;
};

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.transport,
    transportRateDetailId: stringValue(
      values.transportRateDetailId,
      props.transport.transportRateDetailId,
    ),
    volume: finiteNumberValue(values.volume, props.transport.volume),
    distance: submittedDistance(values),
    price: finiteNumberValue(values.price, props.transport.price),
    description: stringValue(values.description, props.transport.description),
    destination: stringValue(values.destination, props.transport.destination),
  });
};
</script>

<template>
  <Form
    ref="form"
    class="mt-2"
    :rows="rows"
    :initial-values="initialValues"
    :disabled="readonly"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #section-destination="{ values, setFieldValue, disabled }">
      <div v-if="values.isFinalCustomer !== true">
        <label class="block text-900 mb-2" :for="destinationInputId">
          {{ t("sales.components.proveidorDeDestinacioServeisExternsMagatzem") }}
        </label>
        <Select
          :input-id="destinationInputId"
          :model-value="nullableStringValue(values.destinationSupplierId, null)"
          :options="supplierStore.suppliers ?? []"
          option-label="comercialName"
          option-value="id"
          :placeholder="t('sales.components.seleccionaProveidorDeDestinacio')"
          class="w-full"
          :disabled="disabled"
          @update:model-value="setFieldValue('destinationSupplierId', $event)"
        />
      </div>
    </template>

    <!-- Shows the distance used to filter rates, which is also the one saved
         (see submittedDistance). -->
    <template #field-distance="{ inputId }">
      <InputNumber
        :input-id="inputId"
        :model-value="displayDistance(currentValues)"
        locale="en-US"
        :min-fraction-digits="2"
        class="w-full"
        disabled
      />
    </template>
  </Form>
</template>
