<template>
  <FormUser
    v-if="user"
    :roles="roles"
    :profiles="profiles"
    :user="user"
    @change-password="changePassword"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { useToast } from "primevue/usetoast";
import { Role } from "@/services/authentications.service";
import { AuthenticationService, ChangePasswordRequest } from "@/services/authentications.service";
import { UserService } from "@/modules/system/services/user.service";
import { RoleService } from "@/services/role.service";
import { Profile, User } from "@/types";
import { AppProfileService } from "@/modules/system/services/profile.service";
import FormUser from "@/modules/system/components/FormUser.vue";

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const store = useStore();
const user = ref(undefined as undefined | User);
const roles = ref<Role[]>();
const profiles = ref<Profile[]>();

const roleService = new RoleService();
const service = new UserService();

// The user is assigned last: FormUser mounts with it and needs the roles and
// profiles already loaded (its profile field depends on them).
const loadView = async () => {
  const [loadedUser, loadedRoles, loadedProfiles] = await Promise.all([
    service.GetById(route.params.id as string),
    roleService.GetAll(),
    AppProfileService.GetAll(),
  ]);
  roles.value = loadedRoles;
  profiles.value = loadedProfiles;
  user.value = loadedUser;

  if (user.value) {
    store.setMenuItem({
      icon: PrimeIcons.USER,
      title: t("userView.pageTitle", { username: user.value.username }),
      backButtonVisible: true,
    });
  }
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async (data: User) => {
  const updated = await service.Update(data);
  if (updated) {
    toast.add({
      severity: "success",
      summary: t("userView.toasts.updated"),
      life: 5000,
    });
    router.back();
  }
};

const changePassword = async (request: ChangePasswordRequest) => {
  const service = new AuthenticationService();
  const changed = await service.ChangePassword(request);

  if (changed) {
    toast.add({
      severity: "success",
      summary: t("userView.toasts.passwordChangedSummary"),
      detail: t("userView.toasts.passwordChanged"),
      life: 10000,
    });
  } else {
    toast.add({
      severity: "error",
      summary: t("userView.toasts.passwordChangedSummary"),
      detail: t("userView.toasts.passwordError"),
      life: 10000,
    });
  }
};
</script>
