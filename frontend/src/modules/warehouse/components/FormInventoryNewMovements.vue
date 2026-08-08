<template>
  <form v-if="newMovement">
    <div>
        <DropdownReference
          :label="t('warehouse.fields.material')"
          :fullName="true"
          v-model="newMovement.referenceId"
          :class="{
            'p-invalid': validation.errors.referenceId,
          }"
        ></DropdownReference>
      </div>

    <section class="three-columns">      
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('warehouse.fields.quantity')"
          v-model="newMovement.newQuantity"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('warehouse.fields.widthMm')"
          :decimals="2"
          v-model="newMovement.width"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('warehouse.fields.heightMm')"
          v-model="newMovement.height"
        />
      </div>
    </section>

    <section class="three-columns">
     
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('warehouse.fields.lengthMm')"
          v-model="newMovement.length"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('warehouse.fields.diameterMm')"
          v-model="newMovement.diameter"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('warehouse.fields.thicknessMm')"
          v-model="newMovement.thickness"
        />
      </div>
    </section>

    <Button
      :label="t('warehouse.actions.create')"
      @click="submitForm"
      style="float: right"
      :size="'small'"
      class="mt-2"
    />
  </form>
</template>

<script setup lang="ts">
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { Inventory } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { useReferenceStore } from "../../shared/store/reference";

const props = defineProps<{
  newMovement: Inventory;
}>();

const emit = defineEmits<{
  (e: "submit", newMovement: Inventory): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const { t } = useI18n();
const referenceStore = useReferenceStore();

onMounted(async () => {
  if (!referenceStore.references || referenceStore.references.length === 0) {
    await referenceStore.fetchReferences();
  }
});

const schema = computed(() => Yup.object().shape({
  newQuantity: Yup.number()
    .min(1, t("warehouse.validation.quantityMinimum"))
    .required(t("warehouse.validation.quantityGreaterThanZero")),
  referenceId: Yup.string().required(t("warehouse.validation.referenceRequired")),
}));
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema.value);
  validation.value = formValidation.validate(props.newMovement);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.newMovement);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("warehouse.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
