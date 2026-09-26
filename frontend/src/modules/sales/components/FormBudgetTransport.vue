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
import {
  computed,
  onMounted,
  onUnmounted,
  ref,
  shallowRef,
  useId,
  watch,
} from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { FormActionMode } from "../../../types/component";
import { useSuppliersStore } from "../../purchase/store/suppliers";
import { useTransportRateStore } from "../../purchase/store/transportRate";
import type { TransportRate } from "../../purchase/types";
import { useCustomersStore } from "../store/customers";
import type { Budget, BudgetTransport } from "../types";

const props = defineProps<{
  formAction: FormActionMode;
  header: Budget;
  transport: BudgetTransport;
  customerId: string;
  readonly?: boolean;
}>();

const emit = defineEmits<{
  (event: "submit", transport: BudgetTransport): void;
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
const form = shallowRef<{ setValues: (values: FormValues) => void } | null>(
  null,
);
const destinationInputId = `budget-transport-destination-${useId()}`;

const finalCustomer = computed(() =>
  customerStore.customers?.find((c) => c.id === props.header.customerId),
);

const customerName = computed(
  () => finalCustomer.value?.comercialName ?? t("sales.components.client"),
);

const initialIsFinalCustomer = (transport: BudgetTransport): boolean => {
  if (transport.destinationSupplierId) return false;
  if (!transport.destination) return true;
  return transport.destination === finalCustomer.value?.comercialName;
};

// The hidden `destination` travels with the form values (section row) and
// `isFinalCustomer` is UI state only; it is not part of the payload.
const createInitialValues = (transport: BudgetTransport): FormValues => ({
  ...transport,
  isFinalCustomer: initialIsFinalCustomer(transport),
});

const initialValues = shallowRef<FormValues>(
  createInitialValues(props.transport),
);

// Feature-owned mirrors of the values the rate filter depends on, and the
// rates of the selected carrier (kept locally so stale responses are ignored).
const isFinalCustomer = ref(
  booleanValue(initialValues.value.isFinalCustomer, true),
);
const destinationSupplierId = ref<string | null>(
  props.transport.destinationSupplierId,
);
const transportRates = ref<TransportRate[]>([]);
let rateRequestSequence = 0;
let suppressCallbacks = false;

// The individually loaded customer includes its addresses; the customer list
// does not, so its distance would always be 0.
const finalCustomerDistance = computed<number>(() => {
  const customer =
    customerStore.customer?.id === props.header.customerId
      ? customerStore.customer
      : finalCustomer.value;
  return customer?.address?.find((a) => a.main)?.distanceFromSite || 0;
});

const rateDistance = computed<number>(() => {
  if (isFinalCustomer.value) return finalCustomerDistance.value;
  return (
    supplierStore.suppliers?.find((s) => s.id === destinationSupplierId.value)
      ?.distanceFromSite ?? 0
  );
});

const compatibleTransportRates = computed<TransportRateOption[]>(() => {
  const now = new Date();
  const weight = props.header.totalWeight || 0;
  const volume = props.transport.volume || 0;
  const distance = rateDistance.value || 0;

  const compatibleDetails = transportRates.value
    .filter((rate) => {
      if (rate.disabled) return false;
      return now >= new Date(rate.validFrom) && now <= new Date(rate.validTo);
    })
    .flatMap((rate) =>
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
    )
    .sort((a, b) => a.detail.price - b.detail.price);

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
});

const setFormValues = (values: FormValues): void => {
  suppressCallbacks = true;
  try {
    form.value?.setValues(values);
  } finally {
    suppressCallbacks = false;
  }
};

const finalCustomerDestination = (): FormValues => {
  const customer = finalCustomer.value;
  return {
    ...(customer
      ? {
          destination: customer.comercialName,
          distance: finalCustomerDistance.value,
        }
      : {}),
    destinationSupplierId: null,
  };
};

const supplierDestination = (supplierId: string | null): FormValues => {
  const supplier = supplierStore.suppliers?.find((s) => s.id === supplierId);
  return supplier
    ? {
        destination: supplier.comercialName,
        distance: supplier.distanceFromSite || 0,
        destinationSupplierId: supplier.id,
      }
    : { destination: "", distance: 0, destinationSupplierId: null };
};

const applyDestination = (
  finalCustomerSelected: boolean,
  supplierId: string | null,
): void => {
  setFormValues(
    finalCustomerSelected
      ? finalCustomerDestination()
      : supplierDestination(supplierId),
  );
};

const loadTransportRates = async (supplierId: string | null): Promise<void> => {
  const requestSequence = ++rateRequestSequence;
  if (!supplierId) {
    transportRates.value = [];
    return;
  }
  await transportRateStore.fetchTransportRatesBySupplierId(supplierId);
  if (requestSequence !== rateRequestSequence) return;
  transportRates.value = transportRateStore.transportRates ?? [];
};

const updateFinalCustomer = (
  value: unknown,
  values: Readonly<FormValues>,
): void => {
  isFinalCustomer.value = booleanValue(value, true);
  if (suppressCallbacks) return;
  applyDestination(
    isFinalCustomer.value,
    nullableStringValue(values.destinationSupplierId, null),
  );
};

const updateDestinationSupplier = (value: unknown): void => {
  destinationSupplierId.value = nullableStringValue(value, null);
  if (suppressCallbacks) return;
  setFormValues(supplierDestination(destinationSupplierId.value));
};

const updateLogisticSupplier = (value: unknown): void => {
  if (suppressCallbacks) return;
  setFormValues({ transportRateDetailId: "" });
  void loadTransportRates(nullableStringValue(value, null));
};

const updateRate = (value: unknown): void => {
  if (suppressCallbacks) return;
  const rate = compatibleTransportRates.value.find((r) => r.id === value);
  if (rate) setFormValues({ price: rate.price });
};

const numberProps = { locale: "en-US", minFractionDigits: 2 } as const;
const currencyProps = {
  locale: "en-US",
  minFractionDigits: 2,
  suffix: " €",
} as const;

const rows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "isFinalCustomer",
        label: t("sales.customers.shipToFinalCustomer", { customer: customerName.value }),
        type: FormFieldType.Checkbox,
        onChange: updateFinalCustomer,
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
        onChange: updateLogisticSupplier,
      },
      {
        name: "transportRateDetailId",
        label: t("sales.components.tarifaDeTransport"),
        type: FormFieldType.Select,
        props: {
          options: compatibleTransportRates.value,
          optionLabel: "label",
          optionValue: "id",
          placeholder: t("sales.components.seleccionaTarifa"),
        },
        disabled: (values) => !values.logisticSupplierId,
        onChange: updateRate,
        validation: Yup.string().required(
          t("sales.validation.transportRateRequired"),
        ),
      },
    ],
  },
  {
    // Shown only when the transport goes to a supplier instead of the final
    // customer; `destination` is derived hidden state kept registered here.
    section: "destination",
    fields: [
      {
        name: "destinationSupplierId",
        label: "",
        type: FormFieldType.Custom,
        onChange: updateDestinationSupplier,
      },
      { name: "destination", label: "", type: FormFieldType.Custom },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "weight",
        label: t("sales.components.pesKg"),
        type: FormFieldType.Number,
        props: numberProps,
        validation: Yup.number().min(0, t("sales.validation.weightNotNegative")),
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
        type: FormFieldType.Number,
        props: numberProps,
        disabled: true,
        validation: Yup.number().min(0, t("sales.validation.distanceNotNegative")),
      },
      {
        name: "price",
        label: t("sales.components.preu"),
        type: FormFieldType.Number,
        props: currencyProps,
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

const restoreTransport = async (transport: BudgetTransport): Promise<void> => {
  const startSequence = rateRequestSequence;
  await supplierStore.fetchLogisticSuppliers();
  await supplierStore.fetchSuppliers();
  // The single customer includes the addresses with distanceFromSite
  // (the customer list does not).
  if (props.header.customerId) {
    await customerStore.fetchCustomer(props.header.customerId);
  }
  if (transport !== props.transport) return;

  if (props.formAction === FormActionMode.CREATE && !transport.destination) {
    applyDestination(isFinalCustomer.value, destinationSupplierId.value);
  } else if (
    transport.logisticSupplierId &&
    // Skip when the user already chose another carrier meanwhile.
    startSequence === rateRequestSequence
  ) {
    await loadTransportRates(transport.logisticSupplierId);
  }
};

watch(
  () => props.transport,
  (transport) => {
    rateRequestSequence += 1;
    transportRates.value = [];
    initialValues.value = createInitialValues(transport);
    isFinalCustomer.value = booleanValue(
      initialValues.value.isFinalCustomer,
      true,
    );
    destinationSupplierId.value = transport.destinationSupplierId;
    void restoreTransport(transport);
  },
);

onMounted(() => {
  void restoreTransport(props.transport);
});

onUnmounted(() => {
  rateRequestSequence += 1;
});

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.transport,
    logisticSupplierId: stringValue(
      values.logisticSupplierId,
      props.transport.logisticSupplierId,
    ),
    transportRateDetailId: stringValue(values.transportRateDetailId, ""),
    destinationSupplierId: nullableStringValue(
      values.destinationSupplierId,
      null,
    ),
    destination: stringValue(values.destination, props.transport.destination),
    weight: finiteNumberValue(values.weight, props.transport.weight),
    volume: finiteNumberValue(values.volume, props.transport.volume),
    distance: finiteNumberValue(values.distance, props.transport.distance),
    price: finiteNumberValue(values.price, props.transport.price),
    description: stringValue(values.description, props.transport.description),
  });
};
</script>

<template>
  <Form
    ref="form"
    :rows="rows"
    :initial-values="initialValues"
    :disabled="readonly"
    @submit="submit"
    @cancel="emit('cancel')"
  >
    <template #section-destination="{ values, setFieldValue, disabled }">
      <div v-if="!booleanValue(values.isFinalCustomer, true)">
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
  </Form>
</template>
