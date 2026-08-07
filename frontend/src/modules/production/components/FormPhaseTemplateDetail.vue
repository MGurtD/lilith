<template>
  <form v-if="detail">
    <section class="three-columns">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :label="t('phaseTemplates.details.fields.order')"
          v-model="detail.order"
          :class="{ 'p-invalid': validation.errors.order }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">
          {{ t("phaseTemplates.details.fields.machineStatus") }}
        </label>
        <Select
          v-model="detail.machineStatusId"
          :options="plantModelStore.machineStatuses"
          optionValue="id"
          optionLabel="description"
          class="w-full"
          :class="{ 'p-invalid': validation.errors.machineStatusId }"
        />
      </div>
    </section>
    <div class="mt-2">
      <label class="block text-900 mb-1">
        {{ t("phaseTemplates.details.fields.comment") }}
      </label>
      <Textarea class="w-full" v-model="detail.comment"></Textarea>
    </div>

    <br />
    <div>
      <Button
        :label="t('phaseTemplates.details.actions.save')"
        style="float: right"
        size="small"
        @click="submitForm"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { PhaseTemplateDetail } from "../types";
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
  detail: PhaseTemplateDetail;
}>();

const emit = defineEmits<{
  (e: "submit", detail: PhaseTemplateDetail): void;
  (e: "cancel"): void;
}>();

const plantModelStore = usePlantModelStore();

const toast = useToast();
const { t } = useI18n();
const getSchema = () =>
  Yup.object().shape({
    order: Yup.number()
      .required(t("phaseTemplates.details.validation.orderRequired"))
      .positive(t("phaseTemplates.details.validation.orderPositive")),
    machineStatusId: Yup.string().required(
      t("phaseTemplates.details.validation.machineStatusRequired"),
    ),
  });
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(getSchema());
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
      summary: t("phaseTemplates.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
