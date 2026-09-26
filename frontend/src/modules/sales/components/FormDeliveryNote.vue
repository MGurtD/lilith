<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { dateValue, stringValue } from "@/components/forms/value-utils";
import { formatDate } from "@/utils/functions";
import { PrimeIcons } from "@primevue/core/api";
import { isEqual } from "lodash";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useCustomersStore } from "../store/customers";
import type { DeliveryNote } from "../types";

const props = defineProps<{
  deliveryNote: DeliveryNote;
  lockHeader?: boolean;
  lockStatus?: boolean;
}>();

const emit = defineEmits<{
  (event: "submit", deliveryNote: DeliveryNote): void;
  (event: "download", showPrices: boolean): void;
  (event: "downloadPdf"): void;
}>();

const { t } = useI18n();
const customerStore = useCustomersStore();

const items = computed(() => [
  {
    label: t("sales.detail.actions.download"),
    icon: PrimeIcons.FILE_WORD,
    command: () => emit("download", true),
  },
  {
    label: t("sales.detail.actions.printPdf"),
    icon: PrimeIcons.FILE_PDF,
    command: () => emit("downloadPdf"),
  },
  {
    label: t("sales.detail.actions.downloadWithoutPrice"),
    icon: PrimeIcons.FILE_WORD,
    command: () => emit("download", false),
  },
]);

// The screen reloads the delivery note after adding or removing orders, so
// the form receives a stable scalar snapshot and only resets when one of its
// values changes. Details and the linked invoice are merged from the latest
// prop at submit.
interface DeliveryNoteScalars {
  number: string;
  customerId: string;
  createdOnText: string;
  statusId: string;
  deliveryDate: Date | null;
  salesInvoiceNumber: string;
  siteId: string;
  exerciseId: string;
}

const scalarSnapshot = (model: DeliveryNote): DeliveryNoteScalars => ({
  number: model.number,
  customerId: model.customerId,
  createdOnText: model.createdOn ? formatDate(model.createdOn) : "",
  statusId: model.statusId,
  deliveryDate: model.deliveryDate instanceof Date ? model.deliveryDate : null,
  salesInvoiceNumber: model.salesInvoice?.invoiceNumber ?? "",
  siteId: model.siteId,
  exerciseId: model.exerciseId,
});

const initialValues = ref(scalarSnapshot(props.deliveryNote));

watch(
  () => scalarSnapshot(props.deliveryNote),
  (next) => {
    if (!isEqual(next, initialValues.value)) initialValues.value = next;
  },
);

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "number",
        label: t("sales.components.numeroAlbara"),
        type: FormFieldType.Text,
        disabled: true,
      },
      {
        name: "customerId",
        label: t("sales.components.client"),
        type: FormFieldType.Custom,
        disabled: props.lockHeader,
        validation: Yup.string().required(
          t("sales.validation.customerRequired"),
        ),
      },
      {
        name: "createdOnText",
        label: t("sales.components.dataCreacio"),
        type: FormFieldType.Text,
        disabled: true,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "statusId",
        label: t("sales.components.estat"),
        type: FormFieldType.Custom,
        disabled: props.lockStatus,
        validation: Yup.string().required(t("sales.validation.statusRequired")),
      },
      {
        name: "deliveryDate",
        label: t("sales.components.dataEntrega"),
        type: FormFieldType.Date,
        disabled: props.lockHeader,
        props: { dateFormat: "dd/mm/yy" },
      },
      {
        name: "salesInvoiceNumber",
        label: t("sales.components.numeroDeFactura"),
        type: FormFieldType.Text,
        disabled: true,
      },
    ],
  },
  {
    // Not editable here, but the legacy form refused to save without them.
    // Registered so their rules still run; the slot only shows the errors.
    section: "header",
    fields: [
      {
        name: "siteId",
        label: "",
        type: FormFieldType.Custom,
        validation: Yup.string().required(t("sales.validation.siteRequired")),
      },
      {
        name: "exerciseId",
        label: "",
        type: FormFieldType.Custom,
        validation: Yup.string().required(
          t("sales.validation.exerciseRequired"),
        ),
      },
    ],
  },
]);

// A delivered note only saves a status change; an invoiced one never saves.
// Only the main Save button is disabled: the download menu stays available.
const saveDisabled = (values: Readonly<FormValues>): boolean =>
  Boolean(props.lockStatus) ||
  (Boolean(props.lockHeader) &&
    values.statusId === initialValues.value.statusId);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.deliveryNote,
    customerId: stringValue(values.customerId, props.deliveryNote.customerId),
    statusId: stringValue(values.statusId, props.deliveryNote.statusId),
    deliveryDate: dateValue(values.deliveryDate, null),
  });
};
</script>

<template>
  <Form
    page-actions
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
  >
    <template #field-customerId="{ value, setValue, disabled, inputId }">
      <div class="customer-field">
        <Select
          :input-id="inputId"
          :model-value="typeof value === 'string' ? value : undefined"
          :options="customerStore.customers ?? []"
          option-value="id"
          option-label="comercialName"
          class="w-full"
          :disabled="disabled"
          @update:model-value="setValue"
        />
        <router-link
          v-if="typeof value === 'string' && value"
          :to="`/customers/${value}`"
          class="customer-field__link"
        >
          <i class="pi pi-search"></i>
        </router-link>
      </div>
    </template>

    <template #field-statusId="{ value, setValue, disabled, inputId }">
      <DropdownLifecycleStatusTransitions
        :input-id="inputId"
        label=""
        :status-id="deliveryNote.statusId"
        :model-value="typeof value === 'string' ? value : undefined"
        :disabled="disabled"
        @update:model-value="setValue"
      />
    </template>

    <template #section-header="{ errors }">
      <small
        v-for="(message, name) in errors"
        :key="name"
        class="header-error"
        role="alert"
      >
        <i class="pi pi-exclamation-circle" aria-hidden="true" />
        <span>{{ message }}</span>
      </small>
    </template>

    <template #actions="{ submit: submitDeliveryNote, values }">
      <SplitButton
        icon="pi pi-save"
        :label="t('sales.detail.actions.save')"
        :model="items"
        :button-props="{ disabled: saveDisabled(values) }"
        @click="submitDeliveryNote"
      />
    </template>
  </Form>
</template>

<style scoped>
.customer-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-field__link {
  color: inherit;
}

.header-error {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--p-orange-600);
}
</style>
