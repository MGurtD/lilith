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
import { User, Profile } from "../types";
import { AppProfileService } from "../services/profile.service";

const service = new UserService();
const users = ref(undefined as User[] | undefined);
const profiles = ref<Profile[] | undefined>();
const profileMap = ref<Record<string, string>>({});
const store = useStore();
const router = useRouter();

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

const getProfileName = (profileId?: string, profileObj?: Profile) => {
  if (profileObj?.name) return profileObj.name;
  if (profileId && profileMap.value[profileId])
    return profileMap.value[profileId];
  return "";
};

const openUser = (row: DataTableRowClickEvent) => {
  router.push({ path: `/user/${row.data.id}` });
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.USERS,
    title: "Gestió d'usuaris",
  });

  await Promise.all([fetchUsers(), fetchProfiles()]);
});
</script>
