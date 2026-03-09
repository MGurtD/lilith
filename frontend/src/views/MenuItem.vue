<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import type { MenuItemFlat } from "../types/menuitem";
import {
  getMenuItem,
  createMenuItem,
  updateMenuItem,
} from "../services/menuitem.service";
import FormMenuItem from "../components/forms/FormMenuItem.vue";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "../store";

const route = useRoute();
const router = useRouter();
const toast = useToast();
const store = useStore();

const id = route.params.id as string;
const isNew = ref(false);
const formData = ref<Partial<MenuItemFlat>>({ sortOrder: 0 });
const submitting = ref(false);
const { t } = useI18n();

const load = async () => {
  try {
    const result = await getMenuItem(id);
    if (result) {
      formData.value = result;
      isNew.value = false;
    } else {
      // 404 - menu item does not exist yet
      isNew.value = true;
      formData.value = { id, sortOrder: 0 };
    }
  } catch (error: unknown) {
    // Network or unexpected error
    console.error("Failed to load menu item:", error);
    isNew.value = true;
    formData.value = { id, sortOrder: 0 };
  }
};

const save = async () => {
  submitting.value = true;
  try {
    if (isNew.value) {
      const created = await createMenuItem({
        id,
        key: formData.value.key!,
        title: formData.value.title!,
        icon: formData.value.icon || undefined,
        route: formData.value.route || undefined,
        parentId: formData.value.parentId || undefined,
        sortOrder: formData.value.sortOrder as number,
      });
      toast.add({
        severity: "success",
        summary: t("menuItems.created"),
        life: 3000,
      });
      isNew.value = false;
      router.replace({ path: `/menuitem/${created.id}` });
    } else {
      await updateMenuItem(formData.value.id!, {
        id: formData.value.id!,
        key: formData.value.key!,
        title: formData.value.title!,
        icon: formData.value.icon || undefined,
        route: formData.value.route || undefined,
        parentId: formData.value.parentId || undefined,
        sortOrder: formData.value.sortOrder as number,
      });
      toast.add({
        severity: "success",
        summary: t("menuItems.updated"),
        life: 3000,
      });
    }
  } catch (e: any) {
    toast.add({
      severity: "error",
      summary: t("menuItems.error"),
      detail: e?.response?.data?.errors?.[0] || t("menuItems.error"),
      life: 4000,
    });
  } finally {
    submitting.value = false;
  }
};

onMounted(async () => {
  await load();
  store.setMenuItem({
    icon: PrimeIcons.SITEMAP,
    title: isNew.value ? t("menuItems.newTitle") : t("menuItems.editTitle"),
    backButtonVisible: true,
  });
});
</script>
<template>
  <div class="card">
    <FormMenuItem v-model="formData" :submitting="submitting" @submit="save" />
  </div>
</template>
