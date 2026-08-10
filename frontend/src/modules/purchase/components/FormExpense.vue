<template>
  <form v-if="expense">
    <section class="four-columns">
      <div class="mt-2">
        <label class="block text-900 mb-2">{{ $t("purchase.fields.type") }}</label>
        <Select
          v-model="expense.expenseTypeId"
          :options="expenseStore.expenseTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.expenseTypeId,
          }"
        />
      </div>
      <div class="mt-2">
        <label class="block text-900 mb-2">{{ $t("purchase.fields.creationDate") }}</label>
        <DatePicker
          id="creationDate"
          v-model="expense.creationDate"
          :class="{
            'p-invalid': validation.errors.creationDate,
          }"
        />
      </div>
      <div class="mt-2">
        <label class="block text-900 mb-2">{{ $t("purchase.fields.paymentDate") }}</label>
        <DatePicker
          id="paymentDate"
          v-model="expense.paymentDate"
          :class="{
            'p-invalid': validation.errors.paymentDate,
          }"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :label="$t('purchase.fields.amount')"
          id="amount"
          v-model="expense.amount"
          :type="BaseInputType.CURRENCY"
          :class="{
            'p-invalid': validation.errors.amount,
          }"
        />
      </div>
    </section>
    <section class="four-columns">
      <div class="mt-2">
        <label class="block text-900 mb-2">{{ $t("purchase.fields.recurring") }}</label>
        <Checkbox v-model="expense.recurring" class="w-full" :binary="true" />
      </div>
      <div class="mt-2">
        <label class="block text-900 mb-2">{{ $t("purchase.fields.frequency") }}</label>
        <Select
          v-model="expense.frecuency"
          :disabled="!expense.recurring"
          :options="frecuencies"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.expenseTypeId,
          }"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :label="$t('purchase.fields.paymentDay')"
          id="paymentDay"
          v-model="expense.paymentDay"
          :disabled="!expense.recurring"
        />
      </div>
      <div class="mt-2">
        <label class="block text-900 mb-2">{{ $t("purchase.fields.endDate") }}</label>
        <DatePicker
          id="endDate"
          v-model="expense.endDate"
          :disabled="!expense.recurring"
        />
      </div>
    </section>
    <div>
      <label class="block text-900 mb-2">{{ $t("purchase.fields.description") }}</label>
      <Textarea v-model="expense.description" class="w-full" />
    </div>
    <div class="mt-2">
      <Button :label="$t('common.save')" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { useExpenseStore } from "../store/expense";
import { Expense } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { BaseInputType } from "../../../types/component";
import { convertDateTimeToJSON } from "../../../utils/functions";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  expense: Expense;
}>();

const emit = defineEmits<{
  (e: "submit", expense: Expense): void;
  (e: "cancel"): void;
}>();

onMounted(() => {});

const expenseStore = useExpenseStore();
const { t } = useI18n();
const { expense } = storeToRefs(expenseStore);

const frecuencies = computed(() => [
  { id: 1, name: t("purchase.frequency.monthly") },
  { id: 2, name: t("purchase.frequency.bimonthly") },
  { id: 3, name: t("purchase.frequency.quarterly") },
  { id: 6, name: t("purchase.frequency.halfYearly") },
  { id: 12, name: t("purchase.frequency.yearly") },
]);

const schema = Yup.object().shape({
  expenseTypeId: Yup.string().required(t("purchase.validation.expenseTypeRequired")),
  creationDate: Yup.string().required(t("purchase.validation.creationDateRequired")),
  paymentDate: Yup.string().required(t("purchase.validation.paymentDateRequired")),
  amount: Yup.string().required(t("purchase.validation.amountRequired")),
});

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.expense);
};

const toast = useToast();
const submitForm = async () => {
  validate();
  if (validation.value.result) {
    props.expense.creationDate = convertDateTimeToJSON(
      props.expense.creationDate,
    );
    props.expense.endDate = convertDateTimeToJSON(props.expense.endDate);
    props.expense.paymentDate = convertDateTimeToJSON(
      props.expense.paymentDate,
    );

    emit("submit", expense.value as Expense);
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
</script>
