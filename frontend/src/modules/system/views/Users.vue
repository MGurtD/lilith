<template>
  <div class="card">
    <Table
      :items="tableItems"
      :columns="columns"
      :filter-config="[]"
      :show-filter-actions="false"
      preset="crud-list"
      removableSort
      tableStyle="min-width: 50rem"
      @row-click="openUser"
      @create="showCreateDialog = true"
    >
      <template #prepend>
        <span class="text-900 font-bold">{{ t("usersView.tableTitle") }}</span>
      </template>
    </Table>

    <Dialog
      v-model:visible="showCreateDialog"
      :header="t('usersView.createDialog.header')"
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
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { UserService } from "@/modules/system/services/user.service";
import { useStore } from "@/store";
import { PrimeIcons } from "@primevue/core/api";
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import { useRouter } from "vue-router";
import { DataTableRowClickEvent } from "primevue/datatable";
import { User, Profile, Role, Language } from "@/types";
import { AppProfileService } from "@/modules/system/services/profile.service";
import { RoleService } from "@/services/role.service";
import LanguageService from "@/services/language.service";
import CreateUserForm from "@/modules/system/components/CreateUserForm.vue";
import { useToast } from "primevue/usetoast";
import type { CreateManagedUserRequest } from "@/modules/system/services/user.service";

const { t } = useI18n();

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
      {} as Record<string, string>,
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

const tableItems = computed(() =>
  (users.value ?? []).map((user) => ({
    ...user,
    profileName: getProfileName(user.profileId ?? undefined, user.profile),
  })),
);

const columns = computed<Column[]>(() => [
  {
    field: "username",
    header: t("usersView.columns.username"),
    sortable: true,
    style: "width: 20%",
  },
  {
    field: "firstName",
    header: t("usersView.columns.firstName"),
    sortable: true,
    style: "width: 20%",
  },
  {
    field: "lastName",
    header: t("usersView.columns.lastName"),
    sortable: true,
    style: "width: 20%",
  },
  {
    field: "profileName",
    header: t("usersView.columns.profile"),
    sortable: true,
    style: "width: 15%",
  },
  {
    field: "disabled",
    header: t("usersView.columns.disabled"),
    columnType: ColumnType.Boolean,
    sortable: true,
    showColor: false,
    style: "width: 20%",
  },
]);

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
    summary: t("usersView.toasts.created"),
    life: 5000,
  });

  showCreateDialog.value = false;
  await fetchUsers();
};

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.USERS,
    title: t("usersView.pageTitle"),
  });

  await Promise.all([
    fetchUsers(),
    fetchProfiles(),
    fetchRoles(),
    fetchLanguages(),
  ]);
});
</script>
