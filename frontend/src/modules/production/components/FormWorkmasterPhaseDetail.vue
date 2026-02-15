<template>
  <form v-if="detail">
    <section class="three-columns">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Ordre"
          v-model="detail.order"
          :class="{
            'p-invalid': validation.errors.order,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Estat</label>
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
        <label class="block text-900v mb-1">Temps de cicle</label>
        <Checkbox v-model="detail.isCycleTime" class="w-full" :binary="true" />
      </div>
    </section>
    <section class="three-columns mt-2">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Temps màquina (min)"
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
          label="Temps operari (min)"
          v-model="detail.estimatedOperatorTime"
          :class="{
            'p-invalid': validation.errors.estimatedTime,
          }"
        />
      </div>
    </section>
    <div class="mt-2">
      <label class="block text-900v mb-1">Comentari fabricació</label>
      <Textarea class="w-full" v-model="detail.comment"></Textarea>
    </div>

    <br />
    <div>
      <Button
        label="Guardar pas"
        style="float: right"
        size="small"
        @click="submitForm"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { WorkMasterPhaseDetail } from "../types";
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
  detail: WorkMasterPhaseDetail;
}>();

const emit = defineEmits<{
  (e: "submit", phase: WorkMasterPhaseDetail): void;
  (e: "cancel"): void;
}>();

const plantModelStore = usePlantModelStore();

const toast = useToast();
const schema = Yup.object().shape({
  order: Yup.number()
    .required("L'ordre és obligatori")
    .positive("L'ordre ha de ser positiu"),
  estimatedTime: Yup.number().required("El temps estimat és obligatori"),
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
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>
