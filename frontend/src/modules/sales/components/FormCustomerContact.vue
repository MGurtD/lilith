<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import { booleanValue, stringValue } from "@/components/forms/value-utils";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import type { CustomerContact } from "../types";

const props = defineProps<{
  contact: CustomerContact;
}>();

const emit = defineEmits<{
  (event: "submit", contact: CustomerContact): void;
  (event: "cancel"): void;
}>();

const { t } = useI18n();

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "firstName",
        label: t("sales.components.nom"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.nameRequired"))
          .max(250, t("sales.validation.nameMaxLength")),
      },
      {
        name: "lastName",
        label: t("sales.components.cognoms"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.lastNameRequired"))
          .max(250, t("sales.validation.lastNameMaxLength")),
      },
      {
        name: "charge",
        label: t("sales.components.carrec"),
        type: FormFieldType.Text,
      },
    ],
  },
  {
    columns: { mobile: 1, tablet: 2, desktop: 4 },
    fields: [
      {
        name: "email",
        label: t("sales.components.correuElectronic"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.emailRequired"))
          .email(t("sales.validation.emailInvalid")),
      },
      {
        name: "extension",
        label: t("sales.components.extensio"),
        type: FormFieldType.Text,
      },
      {
        name: "phoneNumber",
        label: t("sales.components.telefon"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("sales.validation.phoneRequired"))
          .max(15, t("sales.validation.phoneMaxLength")),
      },
      {
        name: "main",
        label: t("sales.components.predeterminat"),
        type: FormFieldType.Checkbox,
        defaultValue: false,
        validation: Yup.boolean().required(),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...props.contact,
    firstName: stringValue(values.firstName, ""),
    lastName: stringValue(values.lastName, ""),
    charge: stringValue(values.charge, props.contact.charge),
    email: stringValue(values.email, ""),
    extension: stringValue(values.extension, props.contact.extension),
    phoneNumber: stringValue(values.phoneNumber, ""),
    main: booleanValue(values.main, false),
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
