<template>
  <form>
    <div class="mb-2">
      <label class="block text-900 mb-2">{{ t('sales.components.client') }}</label>
      <DropdownCustomers
        label=""
        placeholder=""
        v-model="createRequest.customerId"
      />
    </div>
    <div class="mb-2">
      <label class="block text-900 mb-2">{{ t('sales.components.exercici') }}</label>
      <Select
        class="w-full"
        v-model="createRequest.exerciseId"
        :options="exerciseStore.exercises"
        optionValue="id"
        optionLabel="name"
      />
    </div>
    <div class="mb-2">
      <label class="block text-900 mb-2">{{ t('sales.components.data') }}</label>
      <DatePicker v-model="createRequest.date" />
    </div>

    <footer class="mt-4">
      <Button :label="t('sales.components.crear')" @click="onSubmit" style="float: right" />
    </footer>
  </form>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { onMounted, ref } from "vue";
import { useToast } from "primevue/usetoast";
import DropdownCustomers from "../components/DropdownCustomers.vue";
import { CreateSalesHeaderRequest } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { convertDateTimeToJSON } from "../../../utils/functions";
import { useExerciseStore } from "../../shared/store/exercise";

const { t } = useI18n();
const toast = useToast();
const exerciseStore = useExerciseStore();

const props = defineProps<{
  createRequest: CreateSalesHeaderRequest;
}>();
const emit = defineEmits<{
  (e: "submit", createRequest: CreateSalesHeaderRequest): void;
}>();

onMounted(async () => {
  if (!exerciseStore.exercises?.length) {
    await exerciseStore.fetchActive();
  }

  var currentExercise = exerciseStore.exercises?.find(
    (e) => e.name === new Date().getFullYear().toString(),
  );

  if (currentExercise) {
    props.createRequest.exerciseId = currentExercise.id;
  }
});

const schema = Yup.object().shape({
  exerciseId: Yup.string().required(t("sales.validation.exerciseRequired")),
  customerId: Yup.string().required(t("sales.validation.customerRequired")),
  date: Yup.date().required(t("sales.validation.dateRequired")),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.createRequest);
};

const onSubmit = () => {
  validate();
  if (validation.value.result) {
    const submitPayload = {
      ...props.createRequest,
      date: convertDateTimeToJSON(props.createRequest.date),
    };
    emit("submit", submitPayload);
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
