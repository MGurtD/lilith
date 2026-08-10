<template>
    <form v-if="purchaseInvoiceStatus">
        <div class="three-columns">
            <BaseInput
                name="name"
                class="mb-2"
                :label="t('purchase.purchaseInvoiceStatus.fields.name')"
                id="name"
                v-model="purchaseInvoiceStatus.name"
                :class="{
                'p-invalid': validation.errors.name,
                }"
            ></BaseInput>
            <BaseInput
                name="description"
                class="mb-2"
                :label="t('common.description')"
                id="description"
                v-model="purchaseInvoiceStatus.description"
                :class="{
                'p-invalid': validation.errors.description,
                }"
            ></BaseInput>
            <div>
              <label class="block text-900 mb-2">{{ t("purchase.purchaseInvoiceStatus.fields.disabled") }}</label>
              <Checkbox
                v-model="purchaseInvoiceStatus.disabled"
                class="w-full"
                :binary="true"
              />
            </div>
        </div>
    <div class="mt-2">
      <Button :label="t('common.save')" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>
<script setup lang="ts">
import { ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { PurchaseInvoiceStatus } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  purchaseInvoiceStatus: PurchaseInvoiceStatus;
}>();

const emit = defineEmits<{
    (e: "submit", purchaseInvoiceStatus: PurchaseInvoiceStatus): void;
    (e: "cancel"): void;
}>();

const toast = useToast();
const { t } = useI18n();

const schema = Yup.object().shape({
    name: Yup.string()
        .required(t("purchase.purchaseInvoiceStatus.validation.nameRequired"))
        .max(50, t("purchase.purchaseInvoiceStatus.validation.nameMaxLength")),
    description: Yup.string()
        .required(t("purchase.purchaseInvoiceStatus.validation.descriptionRequired"))
        .max(250, t("purchase.purchaseInvoiceStatus.validation.descriptionMaxLength")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.purchaseInvoiceStatus);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.purchaseInvoiceStatus);
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
