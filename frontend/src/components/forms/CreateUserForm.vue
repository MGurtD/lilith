<template>
  <form @submit.prevent="submit" class="create-user-form">
    <section class="three-columns">
      <BaseInput
        id="username"
        v-model="model.username"
        label="Nom d'usuari"
        :class="{ 'p-invalid': validation.errors.username }"
      />
      <BaseInput
        id="firstName"
        v-model="model.firstName"
        label="Nom"
        :class="{ 'p-invalid': validation.errors.firstName }"
      />
      <BaseInput
        id="lastName"
        v-model="model.lastName"
        label="Cognoms"
        :class="{ 'p-invalid': validation.errors.lastName }"
      />
    </section>

    <section class="three-columns">
      <BaseInput
        id="email"
        v-model="model.email"
        label="Correu electrònic"
        :class="{ 'p-invalid': validation.errors.email }"
      />
      <div>
        <label class="block text-900 mb-2">Rol</label>
        <Select
          v-model="model.roleId"
          :options="roles"
          optionLabel="name"
          optionValue="id"
          class="w-full"
          :class="{ 'p-invalid': validation.errors.roleId }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Idioma</label>
        <Select
          v-model="model.preferredLanguage"
          :options="languages"
          optionLabel="name"
          optionValue="code"
          class="w-full"
          :class="{ 'p-invalid': validation.errors.preferredLanguage }"
        />
      </div>
    </section>

    <section class="three-columns">
      <div>
        <label class="block text-900 mb-2">Perfil</label>
        <Select
          v-model="model.profileId"
          :options="profiles"
          optionLabel="name"
          optionValue="id"
          class="w-full"
          showClear
        />
      </div>
      <BaseInput
        :type="BaseInputType.PASSWORD"
        id="password"
        v-model="model.password"
        label="Contrasenya"
        :class="{ 'p-invalid': validation.errors.password }"
      />
      <BaseInput
        :type="BaseInputType.PASSWORD"
        id="repeatPassword"
        v-model="model.repeatPassword"
        label="Repeteix la contrasenya"
        :class="{ 'p-invalid': validation.errors.repeatPassword }"
      />
    </section>

    <div class="flex justify-content-end gap-2 mt-4">
      <Button
        type="button"
        label="Cancel.lar"
        severity="secondary"
        @click="emit('cancel')"
      />
      <Button type="submit" label="Crear usuari" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import * as Yup from "yup";
import { useToast } from "primevue/usetoast";
import BaseInput from "@/components/BaseInput.vue";
import { BaseInputType } from "@/types/component";
import { FormValidation, FormValidationResult } from "@/utils/form-validator";
import type { Language, Profile, Role } from "@/types";
import type { CreateManagedUserRequest } from "@/services/user.service";

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

const toast = useToast();

const model = ref<CreateManagedUserRequest>({
  username: "",
  password: "",
  repeatPassword: "",
  firstName: "",
  lastName: "",
  email: "",
  preferredLanguage: props.initialLanguage,
  roleId: "",
  profileId: null,
});

const schema = Yup.object().shape({
  username: Yup.string().required("El nom d'usuari és obligatori"),
  firstName: Yup.string().required("El nom és obligatori"),
  lastName: Yup.string().required("Els cognoms són obligatoris"),
  email: Yup.string()
    .required("El correu electrònic és obligatori")
    .email("El format del correu electrònic no és vàlid"),
  preferredLanguage: Yup.string().required("L'idioma és obligatori"),
  roleId: Yup.string().required("El rol és obligatori"),
  password: Yup.string()
    .required("La contrasenya és obligatòria")
    .min(5, "La contrasenya ha de tenir almenys 5 caràcters"),
  repeatPassword: Yup.string()
    .required("Has de repetir la contrasenya")
    .oneOf([Yup.ref("password")], "Les contrasenyes no coincideixen"),
});

const validation = ref<FormValidationResult>({
  result: false,
  errors: {},
});

const submit = () => {
  validation.value = new FormValidation(schema).validate(model.value);
  if (!validation.value.result) {
    const errors = Object.values(validation.value.errors).flat().join("\n");
    toast.add({
      severity: "warn",
      summary: "Revisa el formulari",
      detail: errors,
      life: 6000,
    });
    return;
  }

  emit("submit", {
    ...model.value,
    profileId: model.value.profileId || null,
  });
};
</script>

<style scoped>
.create-user-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>
