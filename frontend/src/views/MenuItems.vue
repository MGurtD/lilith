<script setup lang="ts">
import { onMounted, computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { MenuItemNode } from "../types/menuitem";
import { useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { useStore } from "../store";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useMenusStore } from "../store/menus";
import { v4 as uuidv4 } from "uuid";

const router = useRouter();
const appStore = useStore();
const menusStore = useMenusStore();
const confirm = useConfirm();
const toast = useToast();
const { t } = useI18n();

interface TreeNodeLocal {
  key: string;
  data: MenuItemNode;
  children?: TreeNodeLocal[];
}

const toTreeNode = (n: MenuItemNode): TreeNodeLocal => ({
  key: n.id,
  data: n,
  children: n.children?.map(toTreeNode),
});

const searchFilter = ref("");

const filterNode = (node: MenuItemNode, search: string): boolean => {
  const searchLower = search.toLowerCase();
  const matchesNode =
    node.title.toLowerCase().includes(searchLower) ||
    node.key.toLowerCase().includes(searchLower);

  if (matchesNode) return true;

  // Check if any children match
  if (node.children) {
    return node.children.some((child) => filterNode(child, search));
  }

  return false;
};

const filteredTree = computed<MenuItemNode[]>(() => {
  if (!searchFilter.value) return menusStore.tree;

  const filterTree = (nodes: MenuItemNode[]): MenuItemNode[] => {
    return nodes
      .map((node) => {
        const matches = filterNode(node, searchFilter.value);
        if (matches) {
          // If node or any descendant matches, include it with filtered children
          if (node.children) {
            return {
              ...node,
              children: filterTree(node.children),
            };
          }
          return node;
        }
        return null;
      })
      .filter((node): node is MenuItemNode => node !== null);
  };

  return filterTree(menusStore.tree);
});

const treeData = computed<TreeNodeLocal[]>(() =>
  filteredTree.value.map(toTreeNode)
);

// PrimeVue TreeTable single selection requires selectionKeys binding for node-select to fire reliably
const selectionKeys = ref<any>(null);

// Combined loading flag
const loading = computed(
  () => menusStore.loading || menusStore.treeLoading || menusStore.saving
);

const createNew = () => router.push({ path: `/menuitem/${uuidv4()}` });

const open = (event: any) => {
  console.log(event);
  const id = event?.node?.data?.id;
  if (id) router.push({ path: `/menuitem/${id}` });
};

const remove = (node: any) => {
  const id = node?.data?.id;
  if (!id) return;
  confirm.require({
    message: t("menuItems.confirmDelete"),
    header: t("menuItems.confirmHeader"),
    icon: "pi pi-exclamation-triangle",
    accept: async () => {
      const ok = await menusStore.remove(id);
      if (ok) {
        toast.add({
          severity: "success",
          summary: t("menuItems.deleted"),
          life: 3000,
        });
      } else {
        toast.add({
          severity: "error",
          summary: t("menuItems.error"),
          life: 4000,
        });
      }
    },
  });
};

onMounted(async () => {
  appStore.setMenuItem({
    icon: PrimeIcons.SITEMAP,
    title: t("menuItems.listTitle"),
  });
  await menusStore.fetchHierarchy(true);
});
</script>
<template>
  <div class="card">
    <TreeTable
      :value="treeData"
      v-model:selectionKeys="selectionKeys"
      :loading="loading"
      tableStyle="min-width:55rem"
      @node-select="open"
      selectionMode="single"
      scrollable
    >
      <template #header>
        <div
          class="flex flex-wrap align-items-center justify-content-between gap-2"
        >
          <div class="datatable-filter-1">
            <IconField iconPosition="left">
              <InputIcon>
                <i :class="PrimeIcons.SEARCH" />
              </InputIcon>
              <InputText
                v-model="searchFilter"
                :placeholder="t('common.search')"
                class="w-full"
              />
            </IconField>
          </div>
          <div class="datatable-buttons">
            <Button :icon="PrimeIcons.PLUS" rounded raised @click="createNew" />
          </div>
        </div>
      </template>
      <Column
        expander
        field="title"
        :header="t('menuItems.title')"
        style="width: 28%"
      />
      <Column field="key" :header="t('menuItems.key')" style="width: 18%" />
      <Column field="route" :header="t('menuItems.route')" style="width: 24%" />
      <Column
        field="sortOrder"
        :header="t('menuItems.order')"
        style="width: 10%"
      />
      <Column :header="t('menuItems.icon')" style="width: 10%">
        <template #body="slotProps">
          <i
            v-if="slotProps.node.data.icon"
            :class="slotProps.node.data.icon"
          />
        </template>
      </Column>
      <Column :header="t('menuItems.actions')" style="width: 10%">
        <template #body="slotProps">
          <Button
            icon="pi pi-pencil"
            text
            rounded
            @click.stop="open({ node: slotProps.node })"
          />
          <Button
            icon="pi pi-trash"
            text
            rounded
            severity="danger"
            @click.stop="remove(slotProps.node)"
          />
        </template>
      </Column>
    </TreeTable>
  </div>
</template>
