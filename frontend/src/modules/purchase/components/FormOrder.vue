<template>
  <form v-if="order">
    <section class="three-columns">
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.TEXT"
          :label="t('purchase.order.fields.number')"
          id="number"
          v-model="order.number"
          disabled
        />
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">{{ t("purchase.order.fields.exercise") }}</label>
        <Select
          v-model="order.exerciseId"
          :options="exerciseStore.exercises"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.exerciseId,
          }"
        />
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">{{ t("purchase.order.fields.orderDate") }}</label>
        <DatePicker v-model="order.date" dateFormat="dd/mm/yy" />
      </div>
    </section>
    <section class="three-columns">
      <div class="mt-1">
        <DropdownLifecycleStatusTransitions
          :label="t('purchase.order.fields.status')"
          :statusId="order.statusId"
          v-model="order.statusId"
        />
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">{{ t("purchase.order.fields.supplier") }}</label>
        <Select
          v-model="order.supplierId"
          :options="suppliersStore.suppliers"
          optionValue="id"
          optionLabel="comercialName"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.customerId,
          }"
        />
      </div>
    </section>
  </form>
</template>
<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { useOrderStore } from "../store/order";
import { useSuppliersStore } from "../store/suppliers";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { PurchaseOrder, Receipt } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";
import { storeToRefs } from "pinia";
import { BaseInputType } from "../../../types/component";
import { convertDateTimeToJSON, formatDate } from "../../../utils/functions";
import { useExerciseStore } from "../../shared/store/exercise";

const emit = defineEmits<{
  (e: "submit", order: PurchaseOrder): void;
  (e: "cancel"): void;
}>();

const orderStore = useOrderStore();
const suppliersStore = useSuppliersStore();
const exerciseStore = useExerciseStore();
const toast = useToast();
const { t } = useI18n();

const { order } = storeToRefs(orderStore);

const schema = Yup.object().shape({
  supplierId: Yup.string().required(t("purchase.order.validation.supplierRequired")),
  statusId: Yup.string().required(t("purchase.order.validation.statusRequired")),
  exerciseId: Yup.string().required(t("purchase.order.validation.exerciseRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(order.value);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    order.value!.date = convertDateTimeToJSON(order.value!.date);
    emit("submit", order.value!);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("purchase.messages.invalidForm"),
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
