<template>
  <div>
    <Form
      ref="form"
      page-actions
      :rows="rows"
      :initial-values="user"
      @submit="submit"
    >
      <template
        #field-preferredLanguage="{ value, setValue, disabled, inputId }"
      >
        <LanguageSwitcher
          :model-value="typeof value === 'string' ? value : undefined"
          :change-app-language="isSignedInUser"
          :input-id="inputId"
          :disabled="disabled"
          @update:model-value="setValue"
        />
      </template>

      <template #actions="{ submit: submitForm, disabled }">
        <SplitButton
          icon="pi pi-save"
          :label="t('forms.user.saveButton')"
          :model="userActions"
          :disabled="disabled"
          @click="save(submitForm)"
        />
      </template>
    </Form>

    <!-- Password change is a separate action with its own endpoint, so it is
         its own form and never touches the user form above. -->
    <section v-if="passwordChangeModeOn" class="form-user-changepassword">
      <Form
        :rows="passwordRows"
        :initial-values="passwordInitialValues"
        @submit="changePassword"
      >
        <template #actions="{ submit: submitPassword, disabled }">
          <Button
            severity="secondary"
            icon="pi pi-times"
            :label="t('forms.user.cancelButton')"
            :disabled="disabled"
            @click="passwordChangeModeOn = false"
          />
          <Button
            icon="pi pi-key"
            :label="t('forms.user.modifyButton')"
            :disabled="disabled"
            @click="submitPassword"
          />
        </template>
      </Form>
    </section>
  </div>
</template>

<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormFieldConfig,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import LanguageSwitcher from "@/components/LanguageSwitcher.vue";
import type { ChangePasswordRequest } from "@/services/authentications.service";
import { useStore } from "@/store";
import type { Profile, Role, User } from "@/types";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";

const props = defineProps<{
  roles: Role[] | undefined;
  profiles?: Profile[] | undefined;
  user: User;
}>();

const emit = defineEmits<{
  (e: "submit", user: User): void;
  (e: "change-password", request: ChangePasswordRequest): void;
}>();

const { t } = useI18n();
const appStore = useStore();
const form = ref<{ submit: () => void } | null>(null);
const passwordChangeModeOn = ref(false);

// Activate/Deactivate waits for the next valid submit, so the availability
// change is saved together with the validated form values and never applied
// to the source user when validation fails.
const pendingDisabled = ref<boolean | null>(null);

const rows = computed<FormRowConfig[]>(() => {
  // The parent mounts this form only after profiles have loaded, so the
  // condition is stable while the form is open.
  const profileField: FormFieldConfig[] = props.profiles?.length
    ? [
        {
          name: "profileId",
          label: t("forms.user.profileLabel"),
          type: FormFieldType.Select,
          props: {
            options: props.profiles,
            optionValue: "id",
            optionLabel: "name",
          },
        },
      ]
    : [];

  return [
    {
      columns: { mobile: 1, desktop: 3 },
      fields: [
        {
          name: "username",
          label: t("forms.user.usernameLabel"),
          type: FormFieldType.Text,
          disabled: true,
          validation: Yup.string()
            .required(t("forms.user.validation.usernameRequired"))
            .max(250, t("forms.user.validation.usernameMax")),
        },
        {
          name: "roleId",
          label: t("forms.user.roleLabel"),
          type: FormFieldType.Select,
          props: {
            options: props.roles ?? [],
            optionValue: "id",
            optionLabel: "name",
          },
        },
        ...profileField,
      ],
    },
    {
      columns: { mobile: 1, desktop: 3 },
      fields: [
        {
          name: "firstName",
          label: t("forms.user.firstNameLabel"),
          type: FormFieldType.Text,
          validation: Yup.string()
            .required(t("forms.user.validation.firstNameRequired"))
            .max(250, t("forms.user.validation.firstNameMax")),
        },
        {
          name: "lastName",
          label: t("forms.user.lastNameLabel"),
          type: FormFieldType.Text,
          validation: Yup.string()
            .required(t("forms.user.validation.lastNameRequired"))
            .max(250, t("forms.user.validation.lastNameMax")),
        },
        {
          name: "preferredLanguage",
          label: t("forms.user.languageLabel"),
          type: FormFieldType.Custom,
        },
      ],
    },
  ];
});

// The change-password endpoint always acts on the signed-in user, so the
// action is only offered on that user's own record.
const isSignedInUser = computed(() => appStore.user?.id === props.user.id);

// Secondary actions live in the Save button's menu (page actions convention).
const userActions = computed(() => [
  props.user.disabled
    ? {
        label: t("forms.user.activateButton"),
        icon: "pi pi-check",
        command: () => submitAvailability(false),
      }
    : {
        label: t("forms.user.deactivateButton"),
        icon: "pi pi-ban",
        command: () => submitAvailability(true),
      },
  ...(isSignedInUser.value
    ? [
        {
          label: t("forms.user.changePasswordButton"),
          icon: "pi pi-key",
          command: () => {
            passwordChangeModeOn.value = true;
          },
        },
      ]
    : []),
]);

const submitAvailability = (disabled: boolean): void => {
  pendingDisabled.value = disabled;
  form.value?.submit();
};

const save = (submitForm: () => void): void => {
  pendingDisabled.value = null;
  submitForm();
};

const submit = (values: FormValues): void => {
  const disabled = pendingDisabled.value ?? props.user.disabled;
  pendingDisabled.value = null;

  emit("submit", {
    ...props.user,
    username: stringValue(values.username, props.user.username),
    roleId: stringValue(values.roleId, props.user.roleId),
    profileId: nullableStringValue(
      values.profileId,
      props.user.profileId ?? null,
    ),
    firstName: stringValue(values.firstName, ""),
    lastName: stringValue(values.lastName, ""),
    preferredLanguage: stringValue(
      values.preferredLanguage,
      props.user.preferredLanguage,
    ),
    disabled,
  });
};

const passwordInitialValues = {
  currentPassword: "",
  newPassword: "",
  repeatPassword: "",
};

const passwordProps = { feedback: false, toggleMask: true, fluid: true };

const passwordRows = computed<FormRowConfig[]>(() => [
  {
    columns: { mobile: 1, desktop: 3 },
    fields: [
      {
        name: "currentPassword",
        label: t("forms.user.currentPasswordLabel"),
        type: FormFieldType.Password,
        props: passwordProps,
        validation: Yup.string().required(
          t("forms.user.toasts.currentPasswordRequired"),
        ),
      },
      {
        name: "newPassword",
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
            [Yup.ref("newPassword")],
            t("forms.user.validation.passwordMismatch"),
          ),
      },
    ],
  },
]);

const changePassword = (values: FormValues): void => {
  emit("change-password", {
    currentPassword: stringValue(values.currentPassword, ""),
    newPassword: stringValue(values.newPassword, ""),
  });
};
</script>

<style scoped>
.form-user-changepassword {
  margin-top: 1.5rem;
}
</style>
