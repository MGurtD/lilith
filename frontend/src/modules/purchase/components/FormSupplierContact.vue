<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  booleanValue,
  stringValue,
} from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { SupplierContact } from "../types";

const props = defineProps<{
  contact: SupplierContact;
}>();

const emit = defineEmits<{
  (event: "submit", contact: SupplierContact): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "firstName",
        label: t("purchase.supplierContact.fields.firstName"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplierContact.validation.firstNameRequired"))
          .max(
            250,
            t("purchase.supplierContact.validation.firstNameMaxLength"),
          ),
      },
      {
        name: "lastName",
        label: t("purchase.supplierContact.fields.lastName"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplierContact.validation.lastNameRequired"))
          .max(
            250,
            t("purchase.supplierContact.validation.lastNameMaxLength"),
          ),
      },
      {
        name: "charge",
        label: t("purchase.supplierContact.fields.charge"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "email",
        label: t("purchase.supplierContact.fields.email"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplierContact.validation.emailRequired"))
          .email(t("purchase.supplierContact.validation.emailInvalid")),
      },
      {
        name: "phone",
        label: t("purchase.supplierContact.fields.phone"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .trim()
          .required(t("purchase.supplierContact.validation.phoneRequired"))
          .max(15, t("purchase.supplierContact.validation.phoneMaxLength")),
      },
      {
        name: "default",
        label: t("purchase.supplierContact.fields.default"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
        validation: Yup.boolean().required(),
      },
    ],
  },
  {
    fields: [
      {
        name: "observations",
        label: t("purchase.supplierContact.fields.observations"),
        type: FormFieldType.Textarea,
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.contact,
    firstName: stringValue(values.firstName, "").trim(),
    lastName: stringValue(values.lastName, "").trim(),
    charge: stringValue(values.charge, "").trim(),
    email: stringValue(values.email, "").trim(),
    phone: stringValue(values.phone, "").trim(),
    observations: stringValue(values.observations, ""),
    default: booleanValue(values.default, false),
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="contact"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>
