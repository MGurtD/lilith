<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../store";
import { useProfilesStore } from "../store/profiles";
import { PrimeIcons } from "@primevue/core/api";
import { useI18n } from "vue-i18n";
import FormProfile from "../components/forms/FormProfile.vue";
import ProfileMenuAssignment from "../components/ProfileMenuAssignment.vue";
import { useToast } from "primevue/usetoast";
import type { ProfileDetail } from "../types/profile";

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const id = route.params.id as string;
const store = useStore();
const profiles = useProfilesStore();
const toast = useToast();
const isNew = ref(false);
const ready = ref(false);

// Datos iniciales para el formulario (snapshot, no reactivo)
const initialFormData = ref<Partial<ProfileDetail>>({});

const refreshHeader = () => {
  store.setMenuItem({
    icon: PrimeIcons.USER,
    title: t(isNew.value ? "profiles.newTitle" : "profiles.editTitle"),
    backButtonVisible: true,
  });
};

onMounted(async () => {
  await profiles.fetchOne(id);

  if (!profiles.current) {
    isNew.value = true;
    profiles.setNew(id);
  }

  initialFormData.value = { ...profiles.current };
  refreshHeader();
  ready.value = true;
});

const saveProfile = async (formData: Partial<ProfileDetail>) => {
  let ok = false;

  if (isNew.value) {
    ok = await profiles.create({
      name: formData.name || "",
      description: formData.description,
    });
  } else if (profiles.current) {
    ok = await profiles.update(profiles.current.id, {
      id: profiles.current.id,
      name: formData.name || "",
      description: formData.description,
    });
  }

  if (ok) {
    toast.add({
      severity: "success",
      summary: t(isNew.value ? "profiles.created" : "profiles.updated"),
      life: 2500,
    });

    if (isNew.value) {
      isNew.value = false;
      router.replace({ path: `/profile/${profiles.current!.id}` });
    }
  } else {
    toast.add({
      severity: "error",
      summary: t("profiles.error"),
      life: 3000,
    });
  }
};
</script>
<template>
  <div class="grid" v-if="ready">
    <div class="col-12">
      <div class="card h-full">
        <FormProfile
          :initial-data="initialFormData"
          :submitting="profiles.saving"
          :readonly-system="!!initialFormData.isSystem"
          @submit="saveProfile"
        />
      </div>
    </div>
    <div class="col-12">
      <div class="card">
        <ProfileMenuAssignment :profileId="id" />
      </div>
    </div>
  </div>
</template>
