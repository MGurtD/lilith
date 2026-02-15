<template>
  <div>
    <Button
      label="Guardar"
      class="grid_add_row_button"
      size="small"
      @click="submitForm"
    />
    <br />
  </div>
  <form v-if="reference">
    <section class="three-columns">
      <div class="mt-1">
        <BaseInput
          class="mb-2"
          label="Codi"
          id="code"
          v-model="reference.code"
        ></BaseInput>
      </div>
      <div class="mt-1">
        <BaseInput
          class="mb-2"
          label="Descripció"
          id="description"
          v-model="reference.description"
        ></BaseInput>
      </div>
      <div class="mt-1">
        <DropdownReferenceCategory
          label="Categoria"
          v-model="reference.categoryName"
          disabled
        />
      </div>
    </section>
    <FormReferenceMaterial
      v-if="reference.categoryName === ReferenceCategoryEnum.MATERIAL"
      :reference="reference"
    />
    <FormReferenceTool
      v-if="reference.categoryName === ReferenceCategoryEnum.TOOL"
      :reference="reference"
    />
    <FormReferenceService
      v-if="reference.categoryName === ReferenceCategoryEnum.SERVICE"
      :reference="reference"
    />
  </form>
</template>
<script setup lang="ts">
import BaseInput from "../../../components/BaseInput.vue";
import DropdownReferenceCategory from "../../shared/components/DropdownReferenceCategory.vue";
import FormReferenceMaterial from "../components/FormReferenceMaterial.vue";
import FormReferenceTool from "../components/FormReferenceTool.vue";
import FormReferenceService from "../components/FormReferenceService.vue";
import { Reference, ReferenceCategoryEnum } from "../../shared/types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { ref } from "vue";
import { useToast } from "primevue/usetoast";

const props = defineProps<{
  reference: Reference;
}>();
const emit = defineEmits<{
  (e: "submit", reference: Reference): void;
}>();

const toast = useToast();
const schema = {
  code: Yup.string()
    .required("El codi és obligatoria")
    .max(50, "El codi no pot superar els 50 carácters"),
  description: Yup.string()
    .required("La descripció és obligatori")
    .max(250, "La descripció pot superar els 250 carácters"),
};

const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);
const validate = () => {
  let categorySchema = {} as Yup.ObjectSchema<any>;

  if (props.reference.categoryName === ReferenceCategoryEnum.MATERIAL) {
    categorySchema = Yup.object().shape({
      ...schema,
      taxId: Yup.string().required("L'IVA és obligatori"),
      referenceTypeId: Yup.string().required(
        "El tipus de referència és obligatori",
      ),
      referenceFormatId: Yup.string().required("El format és obligatori"),
    });
  } else if (props.reference.categoryName === ReferenceCategoryEnum.SERVICE) {
    categorySchema = Yup.object().shape({
      ...schema,
      price: Yup.number().required("El preu és obligatori"),
      transportAmount: Yup.number().required(
        "El preu de transport és obligatori",
      ),
    });
  } else if (props.reference.categoryName === ReferenceCategoryEnum.TOOL) {
    categorySchema = Yup.object().shape({
      ...schema,
      areaId: Yup.string().required("L'àrea és obligatoria"),
      taxId: Yup.string().required("L'IVA és obligatori"),
    });
  }

  const formValidation = new FormValidation(categorySchema);
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
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>
