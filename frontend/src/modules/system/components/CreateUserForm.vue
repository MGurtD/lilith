<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import type { CreateManagedUserRequest } from "@/modules/system/services/user.service";
import type { Language, Profile, Role } from "@/types";
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";

const props = defineProps<{
  roles: Role[];
  languages: Language[];
  profiles: Profile[];
  initialLanguage: string;
}>();

const emit = defineEmits<{
  (e: "submit", payload: CreateManagedUserRequest): void;
  (e: "cancel"): void;
}>();

const { t } = useI18n();

// Taken once: the dialog remounts the form every time it opens.
const initialValues: CreateManagedUserRequest = {
  username: "",
  password: "",
  repeatPassword: "",
  firstName: "",
  lastName: "",
  email: "",
  preferredLanguage: props.initialLanguage,
  roleId: "",
  profileId: null,
};

const passwordProps = { feedback: false, toggleMask: true, fluid: true };

const rows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "username",
        label: t("forms.user.usernameLabel"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("forms.user.validation.usernameRequired"),
        ),
      },
      {
        name: "firstName",
        label: t("forms.user.firstNameLabel"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("forms.user.validation.firstNameRequired"),
        ),
      },
      {
        name: "lastName",
        label: t("forms.user.lastNameLabel"),
        type: FormFieldType.Text,
        validation: Yup.string().required(
          t("forms.user.validation.lastNameRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "email",
        label: t("forms.user.emailLabel"),
        type: FormFieldType.Text,
        validation: Yup.string()
          .required(t("forms.user.validation.emailRequired"))
          .email(t("forms.user.validation.emailInvalid")),
      },
      {
        name: "roleId",
        label: t("forms.user.roleLabel"),
        type: FormFieldType.Select,
        props: {
          options: props.roles,
          optionLabel: "name",
          optionValue: "id",
        },
        validation: Yup.string().required(
          t("forms.user.validation.roleRequired"),
        ),
      },
      {
        name: "preferredLanguage",
        label: t("forms.user.languageLabel"),
        type: FormFieldType.Select,
        props: {
          options: props.languages,
          optionLabel: "name",
          optionValue: "code",
        },
        validation: Yup.string().required(
          t("forms.user.validation.languageRequired"),
        ),
      },
    ],
  },
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "profileId",
        label: t("forms.user.profileLabel"),
        type: FormFieldType.Select,
        props: {
          options: props.profiles,
          optionLabel: "name",
          optionValue: "id",
          showClear: true,
        },
      },
      {
        name: "password",
        label: t("forms.user.passwordLabel"),
        type: FormFieldType.Password,
        props: passwordProps,
        validation: Yup.string()
          .required(t("forms.user.validation.passwordRequired"))
          .min(5, t("forms.user.validation.passwordMin")),
      },
      {
        name: "repeatPassword",
        label: t("forms.user.passwordRepeatLabel"),
        type: FormFieldType.Password,
        props: passwordProps,
        validation: Yup.string()
          .required(t("forms.user.validation.repeatPasswordRequired"))
          .oneOf(
            [Yup.ref("password")],
            t("forms.user.validation.passwordMismatch"),
          ),
      },
    ],
  },
]);

const submit = (values: FormValues): void => {
  emit("submit", {
    ...initialValues,
    username: stringValue(values.username, ""),
    password: stringValue(values.password, ""),
    repeatPassword: stringValue(values.repeatPassword, ""),
    firstName: stringValue(values.firstName, ""),
    lastName: stringValue(values.lastName, ""),
    email: stringValue(values.email, ""),
    preferredLanguage: stringValue(
      values.preferredLanguage,
      initialValues.preferredLanguage,
    ),
    roleId: stringValue(values.roleId, ""),
    // An empty selection is sent as null, as before.
    profileId: nullableStringValue(values.profileId, null) || null,
  });
};
</script>

<template>
  <Form
    :rows="rows"
    :initial-values="initialValues"
    @submit="submit"
    @cancel="emit('cancel')"
  />
</template>
