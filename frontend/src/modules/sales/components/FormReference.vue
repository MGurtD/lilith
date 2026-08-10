<template>
  <div>
    <Button
      :label="t('sales.components.guardar')"
      class="grid_add_row_button"
      size="small"
      @click="submitForm"
    />
    <br />
  </div>

  <form v-if="reference">
    <section class="five-columns">
      <div class="mt-1">
        <BaseInput
          class="mb-2"
          :label="t('sales.components.codi')"
          id="code"
          v-model="reference.code"
          :class="{
            'p-invalid': validation.errors.code,
          }"
        ></BaseInput>
      </div>
      <div class="mt-1">
        <BaseInput
          class="mb-2"
          :label="t('sales.components.descripcio')"
          id="description"
          v-model="reference.description"
          :class="{
            'p-invalid': validation.errors.description,
          }"
        ></BaseInput>
      </div>
      <div class="mt-1">        
        <DropdownReferenceType :label="t('sales.components.tipusDeMaterial')" v-model="reference.referenceTypeId" />
      </div>
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.TEXT"
          :label="t('sales.components.versio')"
          id="version"
          v-model="reference.version"
        />
      </div>
      <div class="mt-1">
        <DropdownCustomers :label="t('sales.components.client')" v-model="reference.customerId" />
      </div>
    </section>
    <section class="five-columns">
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          :label="t('sales.components.costTeoricFabricacio')"
          id="workMasterCost"
          v-model="reference.workMasterCost"
          disabled
        />
      </div>
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          :label="t('sales.components.costUltimaFabricacio')"
          id="lastCost"
          v-model="reference.lastCost"
          disabled
        />
      </div>
      <div class="mt-1">
        <BaseInput
          :type="BaseInputType.CURRENCY"
          :label="t('sales.components.preuUnitari')"
          id="price"
          v-model="reference.price"
        />
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">{{ t('sales.components.impost') }}</label>
        <Select
          v-model="reference.taxId"
          :options="taxesStore.taxes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.taxid,
          }"
        />
      </div>
      <div class="mt-1">
        <label class="block text-900 mb-2">{{ t('sales.components.servei') }}</label>
        <Checkbox v-model="reference.isService" class="w-full" :binary="true" />
      </div>
    </section>
  </form>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Reference } from "../../../modules/shared/types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import { useToast } from "primevue/usetoast";
import { BaseInputType } from "../../../types/component";
import { useTaxesStore } from "../../shared/store/tax";
import FormReferenceType from "@/modules/shared/components/FormReferenceType.vue";
import DropdownReferenceType from "@/modules/shared/components/DropdownReferenceType.vue";

const { t } = useI18n();
const props = defineProps<{
  reference: Reference;
  defaultCustomerId?: string;
}>();

const emit = defineEmits<{
  (e: "submit", reference: Reference): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const taxesStore = useTaxesStore();

onMounted(() => {
  // Set customer when optional property is set
  if (props.defaultCustomerId && props.reference) {
    props.reference.customerId = props.defaultCustomerId!;
  }
});

const schema = Yup.object().shape({
  code: Yup.string()
    .required(t("sales.validation.codeRequired"))
    .max(50, "El codi no pot superar els 50 carácters"),
  description: Yup.string()
    .required(t("sales.validation.descriptionRequired"))
    .max(250, "La descripció pot superar els 250 carácters"),
  version: Yup.string()
    .required(t("sales.validation.versionRequired"))
    .max(20, "La versió pot superar els 20 carácters"),
  cost: Yup.number().required(t("sales.validation.costRequired")),
  price: Yup.number().required(t("sales.validation.priceRequired")),
  taxId: Yup.string().required(t("sales.validation.taxRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.reference);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.reference);
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
</script>
