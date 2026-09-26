<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import type { MenuItemFlat } from "@/modules/system/types/menuitem";
import {
  getMenuItem,
  createMenuItem,
  updateMenuItem,
} from "@/modules/system/services/menuitem.service";
import FormMenuItem from "@/modules/system/components/FormMenuItem.vue";
import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "@/store";

const route = useRoute();
const router = useRouter();
const toast = useToast();
const store = useStore();

const id = route.params.id as string;
const isNew = ref(false);
const newMenuItem = (): MenuItemFlat => ({
  id,
  key: "",
  title: "",
  sortOrder: 0,
  translations: [],
});
const formData = ref<MenuItemFlat | null>(null);
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
      formData.value = newMenuItem();
    }
  } catch (error: unknown) {
    // Network or unexpected error
    console.error("Failed to load menu item:", error);
    isNew.value = true;
    formData.value = newMenuItem();
  }
};

const save = async (menuItem: MenuItemFlat) => {
  if (submitting.value) return;
  submitting.value = true;
  try {
    if (isNew.value) {
      const created = await createMenuItem({
        id,
        key: menuItem.key,
        icon: menuItem.icon || undefined,
        route: menuItem.route || undefined,
        parentId: menuItem.parentId || undefined,
        sortOrder: menuItem.sortOrder,
        translations: menuItem.translations,
      });
      toast.add({
        severity: "success",
        summary: t("menuItems.created"),
        life: 3000,
      });
      isNew.value = false;
      formData.value = menuItem;
      router.replace({ path: `/menuitem/${created.id}` });
    } else {
      await updateMenuItem(menuItem.id, {
        id: menuItem.id,
        key: menuItem.key,
        icon: menuItem.icon || undefined,
        route: menuItem.route || undefined,
        parentId: menuItem.parentId || undefined,
        sortOrder: menuItem.sortOrder,
        translations: menuItem.translations,
      });
      formData.value = menuItem;
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
    <FormMenuItem
      v-if="formData"
      :menu-item="formData"
      :submitting="submitting"
      @submit="save"
    />
  </div>
</template>
