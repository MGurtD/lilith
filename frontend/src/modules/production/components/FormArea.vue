<template>
  <form v-if="area">
    <section class="two-columns">
      <BaseInput
        class="mb-2"
        label="Nom"
        id="name"
        v-model="area.name"
        :class="{
          'p-invalid': validation.errors.name,
        }"
      ></BaseInput>
      <BaseInput
        class="mb-2"
        label="Descripció"
        id="description"
        v-model="area.description"
        :class="{
          'p-invalid': validation.errors.description,
        }"
      ></BaseInput>
    </section>
    <section class="three-columns">
      <div>
        <label class="block text-900 mb-2">Local</label>
        <Select
          v-model="area.siteId"
          :options="areaStore.sites"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.siteId,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Visible planta</label>
        <Checkbox
          v-model="area.isVisibleInPlant"
          class="w-full"
          :binary="true"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Desactivat</label>
        <Checkbox v-model="area.disabled" class="w-full" :binary="true" />
      </div>
    </section>

    <div class="mt-3">
      <Button label="Guardar" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import BaseInput from "../../../components/BaseInput.vue";
import { Area } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { storeToRefs } from "pinia";
import { usePlantModelStore } from "../store/plantmodel";

const props = defineProps<{
  area: Area;
}>();

onMounted(async () => {
  await areaStore.fetchSites();
});

const emit = defineEmits<{
  (e: "submit", area: Area): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const areaStore = usePlantModelStore();
const { area } = storeToRefs(areaStore);

const schema = Yup.object().shape({
  name: Yup.string()
    .required("El nom és obligatori")
    .max(250, "El nom no pot superar els 250 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció pot superar els 250 carácters"),
  siteId: Yup.string().required("El local es obligatori"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.area);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.area);
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
