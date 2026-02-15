<template>
  <form v-if="user" @submit.prevent="submit">
    <section class="three-columns">
      <BaseInput
        id="username"
        class="mb-2"
        :label="$t('forms.user.usernameLabel') as string"
        v-model="user.username"
        disabled="true"
        :class="{
          'p-invalid': validation.errors.email,
        }"
      ></BaseInput>

      <div>
        <label class="block text-900 mb-2">{{
          $t("forms.user.roleLabel")
        }}</label>
        <Select
          v-model="user.roleId"
          :options="roles"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{ 'p-invalid': validation.errors.city }"
        />
      </div>
      <div v-if="profiles?.length">
        <label class="block text-900 mb-2">{{
          $t("forms.user.profileLabel")
        }}</label>
        <Select
          v-model="user.profileId"
          :options="profiles"
          optionValue="id"
          optionLabel="name"
          class="w-full"
        />
      </div>
    </section>

    <section class="three-columns">
      <BaseInput
        id="firstName"
        class="mb-2"
        :label="$t('forms.user.firstNameLabel') as string"
        v-model="user.firstName"
        :class="{
          'p-invalid': validation.errors.firstName,
        }"
      ></BaseInput>
      <BaseInput
        id="lastName"
        class="mb-2"
        :label="$t('forms.user.lastNameLabel') as string"
        v-model="user.lastName"
        :class="{
          'p-invalid': validation.errors.lastName,
        }"
      ></BaseInput>
      <div>
        <label class="block text-900 mb-2">{{
          $t("forms.user.languageLabel")
        }}</label>
        <LanguageSwitcher
          v-model="user.preferredLanguage"
          :changeAppLanguage="appStore.user?.id === user.id"
        />
      </div>
    </section>

    <section
      class="form-user-changepassword"
      v-if="passwordChangeModeOn === true"
    >
      <BaseInput
        :type="BaseInputType.PASSWORD"
        id="passwordOne"
        class="mb-2 w-full"
        :label="$t('forms.user.passwordLabel') as string"
        v-model="passwordOne"
      ></BaseInput>
      <BaseInput
        :type="BaseInputType.PASSWORD"
        id="passwordTwo"
        class="mb-2"
        :label="$t('forms.user.passwordRepeatLabel') as string"
        v-model="passwordTwo"
      ></BaseInput>
      <div>
        <label class="block mb-2" style="color: #fff">...</label>
        <Button
          :label="$t('forms.user.modifyButton') as string"
          class="mr-2"
          @click="changePassword"
        />
      </div>
    </section>

    <div class="mt-2">
      <Button
        type="submit"
        :label="$t('forms.user.saveButton') as string"
        class="mr-2"
      />

      <Button
        v-if="user.disabled"
        severity="success"
        :label="$t('forms.user.activateButton') as string"
        class="mr-2"
        @click="changeUserAvailability(false)"
      />
      <Button
        v-else
        :label="$t('forms.user.deactivateButton') as string"
        severity="danger"
        class="mr-2"
        @click="changeUserAvailability(true)"
      />

      <Button
        severity="secondary"
        :label="$t('forms.user.changePasswordButton') as string"
        class="mr-2"
        @click="enablePasswordMode"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { useStore } from "../../store";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import { BaseInputType } from "../../types/component";
import { UserLogin } from "../../services/authentications.service";
import { User, Role, Profile } from "../../types";
import LanguageSwitcher from "../LanguageSwitcher.vue";

const { t } = useI18n();
const appStore = useStore();

const props = defineProps<{
  roles: Role[] | undefined;
  profiles?: Profile[] | undefined;
  user: User | undefined;
}>();
const passwordChangeModeOn = ref(false);
const passwordOne = ref("");
const passwordTwo = ref("");

const emit = defineEmits<{
  (e: "submit", contact: User): void;
  (e: "change-password", password: UserLogin): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  username: Yup.string()
    .required(t("forms.user.validation.usernameRequired") as string)
    .max(250, t("forms.user.validation.usernameMax") as string),
  firstName: Yup.string()
    .required(t("forms.user.validation.firstNameRequired") as string)
    .max(250, t("forms.user.validation.firstNameMax") as string),
  lastName: Yup.string()
    .required(t("forms.user.validation.lastNameRequired") as string)
    .max(250, t("forms.user.validation.lastNameMax") as string),
  disabled: Yup.boolean().required(),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.user);
};

const changeUserAvailability = (isDisabled: boolean) => {
  const user = props.user as User;
  user.disabled = isDisabled;
  submit();
};

const submit = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.user as User);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: t("forms.user.validation.invalidForm") as string,
      detail: errors,
      life: 5000,
    });
  }
};

const enablePasswordMode = () => {
  passwordChangeModeOn.value = true;
};

const changePassword = () => {
  let isValid =
    passwordOne.value.length > 4 === passwordTwo.value.length > 4 &&
    passwordOne.value === passwordTwo.value;

  if (!isValid) {
    toast.add({
      severity: "warn",
      summary: t("forms.user.validation.invalidPassword") as string,
      detail: t("forms.user.validation.invalidPasswordDetail") as string,
    });
    return;
  }

  emit("change-password", {
    username: props.user?.username,
    password: passwordOne.value,
  } as UserLogin);
};
</script>

<style scoped>
.form-user-changepassword {
  display: grid;
  gap: 1rem;
  grid-template-columns: 0.5fr 0.5fr 0.5fr;
}
</style>
