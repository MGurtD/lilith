<template>
  <form v-if="transport" @submit.prevent>
    <section class="mt-2 mb-4">
      <div class="flex align-items-center gap-2">
        <Checkbox
          v-model="isFinalCustomer"
          :binary="true"
          @change="onCustomerToggleChange"
          inputId="isFinalCustomer"
        />
        <label for="isFinalCustomer" class="text-900 font-bold"
          >Enviament a client final ({{ customerName }})</label
        >
      </div>
    </section>

    <!-- Logistics / Transport Selection -->
    <section class="two-columns">
      <div class="mb-2">
        <label class="block text-900 mb-2"
          >Proveïdor Logístic (Transportista)</label
        >
        <Select
          v-model="localSupplierId"
          :options="logisticSuppliers"
          optionLabel="comercialName"
          optionValue="id"
          placeholder="Selecciona transportista"
          class="w-full"
          @change="onTransportSupplierChange"
        />
      </div>
      <div class="mb-2">
        <label class="block text-900 mb-2">Tarifa de Transport</label>
        <Select
          v-model="transport.transportRateDetailId"
          :options="compatibleTransportRates"
          optionLabel="label"
          optionValue="id"
          placeholder="Selecciona tarifa"
          class="w-full"
          :disabled="!localSupplierId"
          @change="onRateChange"
          :class="{ 'p-invalid': validation.errors.transportRateDetailId }"
        />
      </div>
    </section>

    <!-- Destination Selection -->
    <section class="mt-3" v-if="!isFinalCustomer">
      <div class="mb-2">
        <label class="block text-900 mb-2"
          >Proveïdor de Destinació (Serveis externs, magatzem...)</label
        >
        <Select
          v-model="destinationSupplierId"
          :options="allSuppliers"
          optionLabel="comercialName"
          optionValue="id"
          placeholder="Selecciona proveïdor de destinació"
          class="w-full"
          @change="onDestinationSupplierChange"
        />
      </div>
    </section>

    <section class="four-columns mt-3">
      <div>
        <BaseInput
          class="mb-2"
          label="Pes (kg)"
          v-model="header.totalWeight"
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :class="{ 'p-invalid': validation.errors.weight }"
        ></BaseInput>
      </div>
      <div>
        <BaseInput
          disabled
          class="mb-2"
          label="Volum (m³)"
          v-model="transport.volume"
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :class="{ 'p-invalid': validation.errors.volume }"
        ></BaseInput>
      </div>
      <div>
        <BaseInput
          disabled
          class="mb-2"
          label="Distància (km)"
          :modelValue="distance"
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :class="{ 'p-invalid': validation.errors.distance }"
        ></BaseInput>
      </div>
      <div>
        <BaseInput
          class="mb-2"
          label="Preu"
          v-model="transport.price"
          :type="BaseInputType.CURRENCY"
          :class="{ 'p-invalid': validation.errors.price }"
        ></BaseInput>
      </div>
    </section>

    <section class="mt-3">
      <div class="mb-2">
        <label class="block text-900 mb-2">Descripció</label>
        <BaseInput
          class="w-full"
          v-model="transport.description"
          :type="BaseInputType.TEXT"
        ></BaseInput>
      </div>
    </section>

    <Button
      :disabled="readonly"
      :label="textActionButton"
      @click="submitForm"
      style="float: right"
      class="mt-4"
    />
  </form>
</template>

<script setup lang="ts">
import { computed, ref, toRefs, onMounted } from "vue";
import { SalesOrderHeader, SalesOrderTransport } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { BaseInputType, FormActionMode } from "../../../types/component";
import { useSuppliersStore } from "../../purchase/store/suppliers";
import { useTransportRateStore } from "../../purchase/store/transportRate";
import { useCustomersStore } from "../store/customers";

const toast = useToast();
const props = defineProps<{
  formAction: FormActionMode;
  header: SalesOrderHeader;
  transport: SalesOrderTransport;
  customerId: string;
  readonly?: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", transport: SalesOrderTransport): void;
}>();

const { transport } = toRefs(props);
const supplierStore = useSuppliersStore();
const transportRateStore = useTransportRateStore();
const customerStore = useCustomersStore();

const localSupplierId = ref<string | null>(null);
const destinationSupplierId = ref<string | null>(null);
const isFinalCustomer = ref(true);

const textActionButton = computed(() => {
  return props.formAction === FormActionMode.CREATE ? "Afegir" : "Modificar";
});

const allSuppliers = computed(() => {
  return supplierStore.suppliers || [];
});

const logisticSuppliers = computed(() => {
  return supplierStore.logisticSuppliers || [];
});

const customerName = computed(() => {
  const cust = customerStore.customers?.find(
    (c) => c.id === props.header.customerId,
  );
  return cust ? cust.comercialName : "Client";
});

const distance = computed(() => {
  if (isFinalCustomer.value) {
    // Usem el customer carregat individualment (té les adreces incloses)
    const cust =
      customerStore.customer?.id === props.header.customerId
        ? customerStore.customer
        : customerStore.customers?.find(
            (c) => c.id === props.header.customerId,
          );
    if (cust) {
      return cust.address?.find((a) => a.main)?.distanceFromSite ?? 0;
    }
  } else {
    const destSup = supplierStore.suppliers?.find(
      (s) => s.id === destinationSupplierId.value,
    );
    if (destSup) {
      return destSup.distanceFromSite ?? 0;
    }
  }
  return 0;
});

const compatibleTransportRates = computed(() => {
  if (!transportRateStore.transportRates) return [];
  const now = new Date();

  const validRates = transportRateStore.transportRates.filter((r) => {
    if (r.disabled) return false;
    const from = new Date(r.validFrom);
    const to = new Date(r.validTo);
    return now >= from && now <= to;
  });

  const weight = props.header.totalWeight || 0;
  const volume = props.transport.volume || 0;
  const dist = distance.value || 0;

  let compatibleDetails: any[] = [];

  validRates.forEach((rate) => {
    if (!rate.details || rate.details.length === 0) return;

    rate.details.forEach((detail) => {
      const weightOk =
        weight === 0 ||
        (detail.minWeight <= weight &&
          (detail.maxWeight === 0 || detail.maxWeight >= weight));
      const volumeOk =
        volume === 0 ||
        (detail.minVolume <= volume &&
          (detail.maxVolume === 0 || detail.maxVolume >= volume));
      const distOk =
        dist === 0 ||
        (detail.minDistance <= dist &&
          (detail.maxDistance === 0 || detail.maxDistance >= dist));

      if (weightOk && volumeOk && distOk) {
        compatibleDetails.push({
          id: detail.id,
          rateName: rate.name,
          rateDescription: rate.description,
          price: detail.price,
          minWeight: detail.minWeight,
          maxWeight: detail.maxWeight,
          minVolume: detail.minVolume,
          maxVolume: detail.maxVolume,
          minDistance: detail.minDistance,
          maxDistance: detail.maxDistance,
        });
      }
    });
  });

  // Ordenar per preu (ascendent)
  compatibleDetails.sort((a, b) => a.price - b.price);

  // Formatejar el label
  return compatibleDetails.map((detail, index) => {
    const isCheapest = index === 0;
    let label = `${detail.rateName}`;
    if (detail.rateDescription) label += ` (${detail.rateDescription})`;

    // Limits
    const limits = [];
    if (detail.maxWeight > 0)
      limits.push(`${detail.minWeight}-${detail.maxWeight} kg`);
    if (detail.maxDistance > 0)
      limits.push(`${detail.minDistance}-${detail.maxDistance} km`);
    if (detail.maxVolume > 0)
      limits.push(`${detail.minVolume}-${detail.maxVolume} m3`);

    if (limits.length > 0) label += ` | ${limits.join(", ")}`;
    label += ` — ${detail.price} €`;
    if (isCheapest) label += " ⭐️ (Millor preu)";

    return {
      id: detail.id,
      label: label,
      price: detail.price,
    };
  });
});

onMounted(async () => {
  await supplierStore.fetchLogisticSuppliers();
  await supplierStore.fetchSuppliers();

  // Carreguem el client concret per tenir les adreces amb distanceFromSite
  // (fetchCustomers no inclou adreces al GetAll del backend)
  if (props.header.customerId) {
    await customerStore.fetchCustomer(props.header.customerId);
  }

  if (
    props.formAction === FormActionMode.CREATE &&
    !transport.value.destination
  ) {
    onCustomerToggleChange();
  } else {
    const cust = customerStore.customers?.find(
      (c) => c.id === props.header.customerId,
    );
    if (cust && transport.value.destination === cust.comercialName) {
      isFinalCustomer.value = true;
    } else {
      isFinalCustomer.value = false;
      const destSup = supplierStore.suppliers?.find(
        (s) => s.comercialName === transport.value.destination,
      );
      if (destSup) {
        destinationSupplierId.value = destSup.id;
      }
    }
  }
});

const onCustomerToggleChange = () => {
  if (isFinalCustomer.value) {
    const cust = customerStore.customers?.find(
      (c) => c.id === props.header.customerId,
    );
    if (cust) {
      transport.value.destination = cust.comercialName;
      const mainAddr = cust.address?.find((a) => a.main);
      transport.value.distance = mainAddr?.distanceFromSite || 0;
    }
    destinationSupplierId.value = null;
  } else {
    if (destinationSupplierId.value) {
      onDestinationSupplierChange();
    } else {
      transport.value.destination = "";
      transport.value.distance = 0;
    }
  }
};

const onTransportSupplierChange = async () => {
  transport.value.transportRateDetailId = "";
  if (localSupplierId.value) {
    await transportRateStore.fetchTransportRatesBySupplierId(
      localSupplierId.value,
    );
  } else {
    transportRateStore.transportRates = [];
  }
};

const onRateChange = () => {
  const selectedRate = compatibleTransportRates.value.find(
    (r) => r.id === transport.value.transportRateDetailId,
  );
  if (selectedRate) {
    transport.value.price = selectedRate.price;
  }
};

const onDestinationSupplierChange = () => {
  const sup = supplierStore.suppliers?.find(
    (s) => s.id === destinationSupplierId.value,
  );
  if (sup) {
    transport.value.destination = sup.comercialName;
    transport.value.distance = sup.distanceFromSite || 0;
  } else {
    transport.value.destination = "";
    transport.value.distance = 0;
  }
};

const schema = Yup.object().shape({
  transportRateDetailId: Yup.string().required("La tarifa és obligatòria"),
  weight: Yup.number().min(0, "El pes no pot ser negatiu"),
  volume: Yup.number().min(0, "El volum no pot ser negatiu"),
  distance: Yup.number().min(0, "La distància no pot ser negativa"),
  price: Yup.number()
    .min(0, "El preu no pot ser negatiu")
    .required("El preu és obligatori"),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.transport);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.transport);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: "Formulari invàlid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>
