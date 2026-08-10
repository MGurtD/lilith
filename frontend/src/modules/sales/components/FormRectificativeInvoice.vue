<template>
  <form v-if="rectificativeInvoice">
    <section>
      <div class="mt-2">
        <div class="flex align-items-center gap-2">
          <Checkbox
            v-model="rectificativeInvoice.createCorrectionInvoice"
            :binary="true"
            inputId="createCorrectionInvoice"
          />
          <label for="createCorrectionInvoice"
            >{{ t('sales.components.crearFacturaAmbImportCorregit') }}</label
          >
        </div>
      </div>
      <div v-if="rectificativeInvoice.createCorrectionInvoice" class="mt-3">
        <BaseInput
          :label="t('sales.components.importAFacturarSenseIVA')"
          v-model="rectificativeInvoice.quantity"
          :decimals="2"
          :type="BaseInputType.CURRENCY"
        />
      </div>
    </section>

    <Button
      :label="t('sales.components.crear')"
      @click="submitForm"
      style="float: right"
      :size="'small'"
      class="mt-2"
    />
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref } from "vue";
import { CreateRectificativeInvoiceRequest } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";

const { t } = useI18n();
const props = defineProps<{
  rectificativeInvoice: CreateRectificativeInvoiceRequest;
  maximumQuantity: number;
}>();

const emit = defineEmits<{
  (e: "submit", rectificativeInvoice: CreateRectificativeInvoiceRequest): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const correctionSchema = Yup.object().shape({
  quantity: Yup.number()
    .min(1, "La quantitat ha de ser superior a 1")
    .required(t("sales.validation.quantityRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const submitForm = async () => {
  if (props.rectificativeInvoice.createCorrectionInvoice) {
    const formValidation = new FormValidation(correctionSchema);
    validation.value = formValidation.validate(props.rectificativeInvoice);

    if (!validation.value.result) {
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
      return;
    }

    if (props.rectificativeInvoice.quantity > props.maximumQuantity) {
      toast.add({
        severity: "warn",
        summary: t('sales.components.formulariInvalid'),
        detail:
          t('sales.components.laQuantitatIntroduidaNoPotSerSuperiorALaQuantitatDeLaFactura'),
        life: 5000,
      });
      return;
    }
  }

  emit("submit", props.rectificativeInvoice);
};
</script>
