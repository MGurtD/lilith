<template>
  <form v-if="supplier">
    <section class="four-columns mb-2">
      <BaseInput
        name="comercialName"
        :label="t('purchase.supplier.fields.commercialName')"
        id="comercialName"
        v-model="supplier.comercialName"
        :class="{
          'p-invalid': validation.errors.comercialName,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('purchase.supplier.fields.taxName')"
        id="taxName"
        v-model="supplier.taxName"
        :class="{
          'p-invalid': validation.errors.taxName,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('purchase.supplier.fields.vatNumber')"
        id="vatNumber"
        v-model="supplier.vatNumber"
        :class="{
          'p-invalid': validation.errors.vatNumber,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.supplier.fields.supplierType") }}</label>
        <Select
          v-model="supplier.supplierTypeId"
          :options="supplierStore.supplierTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.supplierTypeId,
          }"
        />
      </div>
    </section>

    <LocationFields
      :model-value="supplier"
      :show-distance="true"
      :validation-errors="validation.errors"
    />

    <section class="three-columns mb-2">
      <BaseInput
        :label="t('purchase.supplier.fields.phone')"
        id="phone"
        v-model="supplier.phone"
        :class="{
          'p-invalid': validation.errors.phone,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.supplier.fields.paymentMethod") }}</label>
        <Select
          v-model="supplier.paymentMethodId"
          :options="paymentMethodStore.paymentMethods"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.paymentMethodId,
          }"
        />
      </div>
      <BaseInput
        :label="t('purchase.supplier.fields.accountNumber')"
        id="accountNumber"
        v-model="supplier.accountNumber"
        :class="{
          'p-invalid': validation.errors.accountNumber,
        }"
      ></BaseInput>
    </section>

    <div class="mt-2">
      <label class="block text-900 mb-2">{{ t("purchase.supplier.fields.observations") }}</label>
      <Textarea v-model="supplier.observations" class="w-full" />
    </div>

    <div class="mt-2">
      <label class="block text-900 mb-2"
        >{{ t("purchase.supplier.fields.purchaseOrderNotes") }}</label
      >
      <Textarea v-model="supplier.notes" class="w-full" />
    </div>

    <div class="mt-2 flex justify-content-end gap-2">
      <Button :label="t('purchase.supplier.actions.save')" @click="submitForm" />
      <Button :label="t('purchase.supplier.actions.cancel')" severity="secondary" @click="emit('cancel')" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import LocationFields from "@/components/LocationFields.vue";
import { useSuppliersStore } from "../store/suppliers";
import { Supplier } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { usePaymentMethodStore } from "../../shared/store/paymentMethod";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  supplier: Supplier;
}>();

const emit = defineEmits<{
  (e: "submit", supplier: Supplier): void;
  (e: "cancel"): void;
}>();

const supplierStore = useSuppliersStore();
const paymentMethodStore = usePaymentMethodStore();
const toast = useToast();
const { t } = useI18n();

onMounted(async () => {
  await paymentMethodStore.fetchAll();
});

const schema = Yup.object().shape({
  comercialName: Yup.string()
    .required(() => t("purchase.supplier.validation.commercialNameRequired"))
    .max(250, () => t("purchase.supplier.validation.commercialNameMaxLength")),
  vatNumber: Yup.string()
    .required(() => t("purchase.supplier.validation.vatNumberRequired"))
    .max(15, () => t("purchase.supplier.validation.vatNumberMaxLength")),
  taxName: Yup.string().required(() => t("purchase.supplier.validation.taxNameRequired")),
  region: Yup.string().required(() => t("purchase.supplier.validation.regionRequired")),
  city: Yup.string().required(() => t("purchase.supplier.validation.cityRequired")),
  postalCode: Yup.string().required(() => t("purchase.supplier.validation.postalCodeRequired")),
  address: Yup.string().required(() => t("purchase.supplier.validation.addressRequired")),
  phone: Yup.string().required(() => t("purchase.supplier.validation.phoneRequired")),
  accountNumber: Yup.string()
    .required(() => t("purchase.supplier.validation.accountNumberRequired"))
    .max(35, () => t("purchase.supplier.validation.accountNumberMaxLength")),
  supplierTypeId: Yup.string().required(() => t("purchase.supplier.validation.supplierTypeRequired")),
  paymentMethodId: Yup.string().required(() => t("purchase.supplier.validation.paymentMethodRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.supplier);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.supplier);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("purchase.supplier.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
