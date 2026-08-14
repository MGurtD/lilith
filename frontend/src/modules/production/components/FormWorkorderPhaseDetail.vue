<template>
  <form v-if="detail">
    <section class="three-columns">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('production.components.ordre')"
          v-model="detail.order"
          :class="{
            'p-invalid': validation.errors.order,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("production.components.estat") }}</label>
        <Select
          v-model="detail.machineStatusId"
          :options="plantModelStore.machineStatuses"
          optionValue="id"
          optionLabel="description"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.machineStatusId,
          }"
        />
      </div>
      <div>
        <label class="block text-900v mb-1">{{ t("production.components.tempsDeCicle") }}</label>
        <Checkbox v-model="detail.isCycleTime" class="w-full" :binary="true" />
      </div>
    </section>
    <section class="three-columns mt-2">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('production.components.tempsMaquinaMin')"
          v-model="detail.estimatedTime"
          :class="{
            'p-invalid': validation.errors.estimatedTime,
          }"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          :label="t('production.components.tempsOperariMin')"
          v-model="detail.estimatedOperatorTime"
          :class="{
            'p-invalid': validation.errors.estimatedTime,
          }"
        />
      </div>
    </section>
    <div class="mt-2">
      <label class="block text-900v mb-1">{{ t("production.components.comentariFabricacio") }}</label>
      <Textarea class="w-full" v-model="detail.comment"></Textarea>
    </div>

    <br />
    <div>
      <Button
        :label="t('production.components.guardarPas')"
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
import { ref } from "vue";
import { WorkOrderPhaseDetail } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { usePlantModelStore } from "../store/plantmodel";

const props = defineProps<{
  detail: WorkOrderPhaseDetail;
}>();

const emit = defineEmits<{
  (e: "submit", phase: WorkOrderPhaseDetail): void;
  (e: "cancel"): void;
}>();

const plantModelStore = usePlantModelStore();

const toast = useToast();
const schema = Yup.object().shape({
  order: Yup.number()
    .required(t("production.validation.lOrdreEsObligatori"))
    .positive(t("production.validation.orderMustBePositive")),
  estimatedTime: Yup.number().required(t("production.validation.elTempsEstimatEsObligatori")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.detail);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.detail);
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
