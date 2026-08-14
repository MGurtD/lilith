<template>
  <form v-if="bomItem">
    <div>
      <DropdownReference
        :label="t('production.components.material')"
        :fullName="true"
        v-model="bomItem.referenceId"
        :class="{
          'p-invalid': validation.errors.referenceId,
        }"
      ></DropdownReference>
    </div>

    <section class="three-columns">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('production.components.quantitat')"
          v-model="bomItem.quantity"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('production.components.ampladaMm')"
          :decimals="2"
          v-model="bomItem.width"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('production.components.alcadaMm')"
          v-model="bomItem.height"
        />
      </div>
    </section>

    <section class="three-columns">
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('production.components.longitudMm')"
          v-model="bomItem.length"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('production.components.diametreMm')"
          v-model="bomItem.diameter"
        />
      </div>
      <div class="mt-2">
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('production.components.gruixMm')"
          v-model="bomItem.thickness"
        />
      </div>
    </section>

    <br />
    <div>
      <Button
        :label="t('production.components.guardarMaterial')"
        style="float: right"
        size="small"
        @click="submitForm"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { ref } from "vue";
import { WorkOrderPhaseBillOfMaterials } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";

const props = defineProps<{
  bomItem: WorkOrderPhaseBillOfMaterials;
}>();

const emit = defineEmits<{
  (e: "submit", bomItem: WorkOrderPhaseBillOfMaterials): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const schema = Yup.object().shape({
  referenceId: Yup.string().required(t("production.validation.elMaterialDeConsumEsObligatori")),
  quantity: Yup.number()
    .min(1, t("production.validation.laQuantitatAConsumirHaDeSerPositiva"))
    .required(t("production.validation.laQuantitatAConsumirEsObligatoria")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.bomItem);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.bomItem);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("production.components.formulariInvalid"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
