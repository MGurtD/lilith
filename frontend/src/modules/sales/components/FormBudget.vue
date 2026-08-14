<template>
  <div>
    <form v-if="budget">
      <section class="four-columns mt-2">
        <div>
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('sales.components.pressupost')"
            id="number"
            v-model="budget.number"
            disabled
          />
        </div>
        <div>
          <label class="block text-900 mb-2">{{ t('sales.components.dataAlta') }}</label>
          <DatePicker v-model="budget.date" dateFormat="dd/mm/yy" />
        </div>
        <div>
          <label class="block text-900 mb-2">{{ t('sales.components.dataAcceptacio') }}</label>
          <DatePicker v-model="budget.acceptanceDate" dateFormat="dd/mm/yy" />
        </div>
        <div>
          <BaseInput
            :type="BaseInputType.TEXT"
            :disabled="true"
            :label="t('sales.components.comanda')"
            :modelValue="budgetStore.order?.number ?? ''"
          />
        </div>
      </section>
      <section class="three-columns mt-2">
        <div>
          <DropdownLifecycleStatusTransitions
            :label="t('sales.components.estat')"
            :statusId="budget.statusId"
            v-model="budget.statusId"
            :class="{
              'p-invalid': validation.errors.statusId,
            }"
          />
        </div>
        <div>
          <label class="block text-900 mb-2">{{ t('sales.components.client') }}</label>
          <div style="display: flex; align-items: center; gap: 0.5rem">
            <Select
              v-model="budget.customerId"
              :options="customerStore.customers"
              optionValue="id"
              optionLabel="comercialName"
              class="w-full"
              :class="{
                'p-invalid': validation.errors.customerId,
              }"
            />
            <router-link
              v-if="budget.customerId"
              :to="`/customers/${budget.customerId}`"
              style="color: inherit"
            >
              <i class="pi pi-search"></i>
            </router-link>
          </div>
        </div>
        <div>
          <BaseInput
            :type="BaseInputType.NUMERIC"
            :label="t('sales.components.diesNaturalsEntrega')"
            id="deliveryDays"
            v-model="budget.deliveryDays"
          />
        </div>
      </section>
      <section v-if="budget.notes && budget.notes.length > 0" class="mt-2">
        <div>
          <BaseInput
            :type="BaseInputType.TEXT"
            :label="t('sales.components.notes')"
            id="notes"
            :modelValue="budget.notes"
            disabled
          />
        </div>
      </section>
    </form>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { computed, ref } from "vue";
import { storeToRefs } from "pinia";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useCustomersStore } from "../store/customers";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { Budget } from "../types";
import { BaseInputType } from "../../../types/component";
import { convertDateTimeToJSON } from "../../../utils/functions";
import { useBudgetStore } from "../store/budget";

const { t } = useI18n();
const emit = defineEmits<{
  (e: "submit", budget: Budget): void;
  (e: "cancel"): void;
}>();

const budgetStore = useBudgetStore();
const customerStore = useCustomersStore();
const toast = useToast();

const { budget } = storeToRefs(budgetStore);

const schema = Yup.object().shape({
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
  validation.value = formValidation.validate(budget.value);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    parseEntityDates();
    emit("submit", budget.value!);
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

const parseEntityDates = () => {
  if (!budget.value) return;

  budget.value.date = convertDateTimeToJSON(budget.value.date);
  if (budget.value.acceptanceDate) {
    budget.value.acceptanceDate = convertDateTimeToJSON(
      budget.value.acceptanceDate,
    );
  }
};
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
