<template>
  <div>
    <Button
      :label="t('purchase.materials.actions.save')"
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
          :label="t('purchase.materials.fields.code')"
          id="code"
          v-model="reference.code"
        ></BaseInput>
      </div>
      <div class="mt-1">
        <BaseInput
          class="mb-2"
          :label="t('purchase.materials.fields.description')"
          id="description"
          v-model="reference.description"
        ></BaseInput>
      </div>
      <div class="mt-1">
        <DropdownReferenceCategory
          :label="t('purchase.materials.fields.category')"
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
import { useI18n } from "vue-i18n";

const props = defineProps<{
  reference: Reference;
}>();
const emit = defineEmits<{
  (e: "submit", reference: Reference): void;
}>();

const toast = useToast();
const { t } = useI18n();
const schema = {
  code: Yup.string()
    .required(t("purchase.materials.validation.codeRequired"))
    .max(50, t("purchase.materials.validation.codeMaxLength")),
  description: Yup.string()
    .required(t("purchase.materials.validation.descriptionRequired"))
    .max(250, t("purchase.materials.validation.descriptionMaxLength")),
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
      taxId: Yup.string().required(t("purchase.materials.validation.taxRequired")),
      referenceTypeId: Yup.string().required(
        t("purchase.materials.validation.referenceTypeRequired"),
      ),
      referenceFormatId: Yup.string().required(
        t("purchase.materials.validation.formatRequired"),
      ),
    });
  } else if (props.reference.categoryName === ReferenceCategoryEnum.SERVICE) {
    categorySchema = Yup.object().shape({
      ...schema,
      price: Yup.number().required(t("purchase.materials.validation.priceRequired")),
      transportAmount: Yup.number().required(
        t("purchase.materials.validation.transportPriceRequired"),
      ),
    });
  } else if (props.reference.categoryName === ReferenceCategoryEnum.TOOL) {
    categorySchema = Yup.object().shape({
      ...schema,
      areaId: Yup.string().required(t("purchase.materials.validation.areaRequired")),
      taxId: Yup.string().required(t("purchase.materials.validation.taxRequired")),
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
      summary: t("purchase.messages.invalidForm"),
      detail: errors,
      life: 5000,
    });
  }
};
</script>
