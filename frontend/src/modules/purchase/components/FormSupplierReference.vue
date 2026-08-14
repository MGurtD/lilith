<template>
  <form v-if="referenceSupplier">
    <section class="three-columns pt-5">
      <DropdownReference
        v-if="!referenceId"
        :label="t('purchase.supplierReference.fields.reference')"
        v-model="referenceSupplier.referenceId"
        :fullName="true"
      ></DropdownReference>
      <DropdownSupplier
        v-if="!supplierId"
        :label="t('purchase.supplierReference.fields.supplier')"
        v-model="referenceSupplier.supplierId"
      />
      <BaseInput
        :label="t('purchase.supplierReference.fields.supplierCode')"
        id="supplierCode"
        v-model="referenceSupplier.supplierCode"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.supplierCode,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('purchase.supplierReference.fields.supplierDescription')"
        id="supplierDescription"
        v-model="referenceSupplier.supplierDescription"
        class="mb-2"
      ></BaseInput>
    </section>
    <section class="three-columns">
      <BaseInput
        :label="t('purchase.supplierReference.fields.supplierPrice')"
        id="supplierPrice"
        v-model="referenceSupplier.supplierPrice"
        :type="BaseInputType.CURRENCY"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.supplierPrice,
        }"
      ></BaseInput>
      <BaseInput
        :label="t('purchase.supplierReference.fields.supplyDays')"
        id="supplyDays"
        v-model="referenceSupplier.supplyDays"
        :type="BaseInputType.NUMERIC"
        class="mb-2"
        :class="{
          'p-invalid': validation.errors.supplyDays,
        }"
      ></BaseInput>
    </section>
    <div class="mt-2 flex justify-content-end gap-2">
      <Button :label="t('purchase.supplierReference.actions.save')" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import BaseInput from "../../../components/BaseInput.vue";
import DropdownSupplier from "../components/DropdownSupplier.vue";
import DropdownReference from "../../../modules/shared/components/DropdownReference.vue";
import { ref } from "vue";
import { SupplierReference } from "../types";
import * as Yup from "yup";
import { BaseInputType } from "../../../types/component";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  referenceId?: string;
  supplierId?: string;
  referenceSupplier: SupplierReference;
}>();

const emit = defineEmits<{
  (e: "submit", reference: SupplierReference): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const { t } = useI18n();

const schema = Yup.object().shape({
  supplierCode: Yup.string().required(() => t("purchase.supplierReference.validation.supplierCodeRequired")),
  supplierPrice: Yup.number().required(() => t("purchase.supplierReference.validation.supplierPriceRequired")),
  supplyDays: Yup.number().required(() => t("purchase.supplierReference.validation.supplyDaysRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.referenceSupplier);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.referenceSupplier);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("purchase.supplierReference.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
