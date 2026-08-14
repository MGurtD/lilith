<template>
  <div>
    <Button
      :label="t('purchase.receipt.actions.save')"
      size="small"
      class="grid_add_row_button"
      @click="submitForm"
    />
    <br />
    <form v-if="receipt">
      <section class="three-columns">
        <div class="mt-1">
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('purchase.receipt.fields.number')"
            id="number"
            v-model="receipt.number"
            disabled
          />
        </div>
        <div class="mt-1">
          <label class="block text-900 mb-2">{{ t("purchase.receipt.fields.exercise") }}</label>
          <Select
            v-model="receipt.exerciseId"
            :options="sharedDataStore.exercises"
            optionValue="id"
            optionLabel="name"
            class="w-full"
            :class="{
              'p-invalid': validation.errors.exerciseId,
            }"
          />
        </div>
        <div class="mt-1">
          <label class="block text-900 mb-2">{{ t("purchase.receipt.fields.date") }}</label>
          <DatePicker v-model="receipt.date" dateFormat="dd/mm/yy" />
        </div>
      </section>
      <section class="three-columns">
        <div class="mt-1">
          <DropdownLifecycleStatusTransitions
            :label="t('purchase.receipt.fields.status')"
            :statusId="receipt.statusId"
            v-model="receipt.statusId"
            :class="{
              'p-invalid': validation.errors.statusId,
            }"
          />
        </div>
        <div class="mt-1">
          <label class="block text-900 mb-2">{{ t("purchase.receipt.fields.supplier") }}</label>
          <Select
            v-model="receipt.supplierId"
            :options="suppliersStore.suppliers"
            optionValue="id"
            optionLabel="comercialName"
            class="w-full"
            :class="{
              'p-invalid': validation.errors.customerId,
            }"
          />
        </div>
        <div class="mt-1">
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('purchase.receipt.fields.supplierNumber')"
            id="supplierNumber"
            v-model="receipt.supplierNumber"
          />
        </div>
      </section>
    </form>
  </div>
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { useReceiptsStore } from "../store/receipt";
import { useSuppliersStore } from "../store/suppliers";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";

import { Receipt } from "../types";
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
import { useSharedDataStore } from "../../shared/store/masterData";

const emit = defineEmits<{
  (e: "submit", receipt: Receipt): void;
  (e: "cancel"): void;
}>();

const receiptStore = useReceiptsStore();
const suppliersStore = useSuppliersStore();
const sharedDataStore = useSharedDataStore();
const toast = useToast();
const { t } = useI18n();

const { receipt } = storeToRefs(receiptStore);

onMounted(async () => {
  await sharedDataStore.fetchMasterData();
  await suppliersStore.fetchSuppliers();
});

const schema = Yup.object().shape({
  supplierId: Yup.string().required(t("purchase.receipt.validation.supplierRequired")),
  statusId: Yup.string().required(t("purchase.receipt.validation.statusRequired")),
  exerciseId: Yup.string().required(t("purchase.receipt.validation.exerciseRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(receipt.value);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    receipt.value!.date = convertDateTimeToJSON(receipt.value!.date);
    emit("submit", receipt.value!);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("purchase.receipt.messages.invalidForm"),
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
