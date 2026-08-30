<template>
  <div>
    <form v-if="deliveryNote">
      <section class="three-columns">
        <div class="mt-1">
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('sales.components.numeroAlbara')"
            id="salesOrderNumber"
            v-model="deliveryNote.number"
            disabled
          />
        </div>
        <div class="mt-1">
          <label class="block text-900 mb-2">{{ t('sales.components.client') }}</label>
          <div style="display: flex; align-items: center; gap: 0.5rem">
            <Select
              v-model="deliveryNote.customerId"
              :options="customerStore.customers"
              optionValue="id"
              optionLabel="comercialName"
              class="w-full"
              :disabled="lockHeader"
              :class="{
                'p-invalid': validation.errors.customerId,
              }"
            />
            <router-link
              v-if="deliveryNote.customerId"
              :to="`/customers/${deliveryNote.customerId}`"
              style="color: inherit"
            >
              <i class="pi pi-search"></i>
            </router-link>
          </div>
        </div>
        <div class="mt-2">
          <BaseInput
            v-model="createdOn"
            :label="t('sales.components.dataCreacio')"
            :disabled="true"
          />
        </div>
      </section>
      <section class="three-columns">
        <div class="mt-2">
          <DropdownLifecycleStatusTransitions
            :label="t('sales.components.estat')"
            :statusId="deliveryNote.statusId"
            v-model="deliveryNote.statusId"
            :disabled="lockStatus"
            :class="{
              'p-invalid': validation.errors.statusId,
            }"
          />
        </div>
        <div class="mt-2">
          <label class="block text-900 mb-2">{{ t('sales.components.dataEntrega') }}</label>
          <DatePicker
            v-model="deliveryNote.deliveryDate"
            dateFormat="dd/mm/yy"
            class="mt-2"
            :disabled="lockHeader"
          />
        </div>
        <div class="mt-2">
          <BaseInput
            v-model="salesInvoiceNumber"
            :label="t('sales.components.numeroDeFactura')"
            :disabled="true"
          />
        </div>
      </section>
    </form>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { computed, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { useCustomersStore } from "../store/customers";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { useSharedDataStore } from "../../shared/store/masterData";
import { DeliveryNote } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { BaseInputType } from "../../../types/component";
import { formatDate } from "../../../utils/functions";

const { t } = useI18n();
const props = defineProps<{
  deliveryNote: DeliveryNote;
  lockHeader?: boolean;
  lockStatus?: boolean;
}>();

const emit = defineEmits<{
  (e: "submit", deliveryNote: DeliveryNote): void;
  (e: "cancel"): void;
}>();

const customerStore = useCustomersStore();
const sharedDataStore = useSharedDataStore();
const toast = useToast();

const salesInvoiceNumber = computed(() => {
  if (props.deliveryNote && props.deliveryNote.salesInvoice)
    return props.deliveryNote.salesInvoice.invoiceNumber;
  return "";
});

const createdOn = computed((): string => {
  if (props.deliveryNote && props.deliveryNote.createdOn)
    return formatDate(props.deliveryNote.createdOn);
  return "";
});

const schema = Yup.object().shape({
  siteId: Yup.string().required(t("sales.validation.siteRequired")),
  customerId: Yup.string().required(t("sales.validation.customerRequired")),
  statusId: Yup.string().required(t("sales.validation.statusRequired")),
  exerciseId: Yup.string().required(t("sales.validation.exerciseRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.deliveryNote);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.deliveryNote);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t('sales.components.formulariInvalid'),
      detail: errors,
      life: 5000,
    });
  }
};

defineExpose({
  submitForm,
});
</script>
<style scoped>
.save_button {
  position: absolute;
  top: 0;
  right: 1rem;
}

.summary-field {
  font-weight: bold;
  border-bottom: 1px solid black;
}
</style>
