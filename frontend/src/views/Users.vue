<template>
  <div class="card">
    <DataTable
      :value="users"
      removableSort
      tableStyle="min-width: 50rem"
      @row-click="openUser"
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <span class="text-900 font-bold">Usuaris</span>
          <Button label="Nou usuari" icon="pi pi-plus" @click="showCreateDialog = true" />
        </div>
      </template>
      <Column
        field="username"
        header="Nom d'usuari"
        sortable
        style="width: 20%"
      ></Column>
      <Column
        field="firstName"
        header="Nom"
        sortable
        style="width: 20%"
      ></Column>
      <Column
        field="lastName"
        header="Cognoms"
        sortable
        style="width: 20%"
      ></Column>
      <Column header="Perfil" sortable style="width: 15%">
        <template #body="slotProps">
          {{ getProfileName(slotProps.data.profileId, slotProps.data.profile) }}
        </template>
      </Column>
      <Column header="Desactivat" sortable style="width: 20%">
        <template #body="slotProps">
          <BooleanColumn :value="slotProps.data.disabled" :showColor="false" />
        </template>
      </Column>
    </DataTable>

    <Dialog
      v-model:visible="showCreateDialog"
      header="Nou usuari"
      :modal="true"
      :style="{ width: '70rem', maxWidth: '95vw' }"
    >
      <CreateUserForm
        v-if="roles.length > 0 && languages.length > 0"
        :roles="roles"
        :profiles="profiles ?? []"
        :languages="languages"
        :initial-language="defaultLanguage"
        @submit="createUser"
        @cancel="showCreateDialog = false"
      />
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { UserService } from "../services/user.service";
import { useStore } from "../store";
import { PrimeIcons } from "@primevue/core/api";
import BooleanColumn from "../components/tables/BooleanColumn.vue";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { User, Profile, Role, Language } from "../types";
import { AppProfileService } from "../services/profile.service";
import { RoleService } from "@/services/role.service";
import LanguageService from "@/services/language.service";
import CreateUserForm from "@/components/forms/CreateUserForm.vue";
import { useToast } from "primevue/usetoast";
import type { CreateManagedUserRequest } from "@/services/user.service";

const service = new UserService();
const roleService = new RoleService();
const languageService = new LanguageService();
const users = ref(undefined as User[] | undefined);
const profiles = ref<Profile[] | undefined>();
const profileMap = ref<Record<string, string>>({});
const roles = ref<Role[]>([]);
const languages = ref<Language[]>([]);
const defaultLanguage = ref("ca");
const showCreateDialog = ref(false);
const store = useStore();
const router = useRouter();
const toast = useToast();

const fetchUsers = async () => {
  users.value = await service.GetAll();
};

const fetchProfiles = async () => {
  profiles.value = await AppProfileService.GetAll();
  if (profiles.value) {
    profileMap.value = profiles.value.reduce(
      (acc, p) => ({ ...acc, [p.id]: p.name }),
      {} as Record<string, string>
    );
  }
};

const fetchRoles = async () => {
  roles.value = (await roleService.GetAll()) ?? [];
};

const fetchLanguages = async () => {
  languages.value = (await languageService.GetAll()) ?? [];
  defaultLanguage.value =
    languages.value.find((language) => language.isDefault)?.code ?? "ca";
};

const getProfileName = (profileId?: string, profileObj?: Profile) => {
  if (profileObj?.name) return profileObj.name;
  if (profileId && profileMap.value[profileId])
    return profileMap.value[profileId];
  return "";
};

const openUser = (row: DataTableRowClickEvent) => {
  router.push({ path: `/user/${row.data.id}` });
};

const createUser = async (request: CreateManagedUserRequest) => {
  const createdUser = await service.CreateManaged(request);
  if (!createdUser) {
    return;
  }

  toast.add({
    severity: "success",
    summary: "Usuari creat correctament",
    life: 5000,
  });

  showCreateDialog.value = false;
  await fetchUsers();
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.USERS,
    title: "Gestió d'usuaris",
  });

  await Promise.all([fetchUsers(), fetchProfiles(), fetchRoles(), fetchLanguages()]);
});
</script>
