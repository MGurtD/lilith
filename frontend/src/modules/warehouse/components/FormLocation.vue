<template>
  <form v-if="location">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        :label="t('warehouse.fields.name')"
        id="name"
        v-model="location.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        :label="t('common.description')"
        id="description"
        v-model="location.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
    </section>
    <section class="two-columns mt-3">
      <div>
        <label class="block text-900 mb-2">{{ t("warehouse.fields.locationType") }}</label>
        <Select
          v-model="location.locationType"
          :options="locationTypeOptions"
          optionLabel="label"
          optionValue="value"
          :placeholder="t('warehouse.placeholders.noLocationType')"
          class="w-full"
          showClear
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("warehouse.fields.disabled") }}</label>
        <Checkbox v-model="location.disabled" class="w-full" :binary="true" />
      </div>
    </section>
    <div class="pt-4">
      <Button
        :label="t('common.save')"
        size="small"
        style="float: right"
        @click="submitForm"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import BaseInput from "../../../components/BaseInput.vue";
import { Location, LocationTypeOption, getLocationTypeOptions } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";

const toast = useToast();
const { t } = useI18n();

const props = defineProps<{
  location: Location;
}>();

const emit = defineEmits<{
  (e: "submit", location: Location): void;
  (e: "cancel"): void;
}>();

const locationTypeOptions = computed<LocationTypeOption[]>(() => getLocationTypeOptions(t));

const schema = computed(() => Yup.object().shape({
  name: Yup.string()
    .required(t("warehouse.validation.nameRequired"))
    .max(250, t("warehouse.validation.nameMaxLength")),
  description: Yup.string()
    .required(t("warehouse.validation.descriptionRequired"))
    .max(250, t("warehouse.validation.descriptionMaxLength")),
}));
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema.value);
  validation.value = formValidation.validate(props.location);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.location);
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
