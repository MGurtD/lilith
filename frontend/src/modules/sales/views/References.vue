<template>
  <TableReferences
    :references="referenceStore.references"
    @add="addReference"
    @edit="editReference"
    @delete="deleteReference"
  ></TableReferences>
</template>
<script setup lang="ts">
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useReferenceStore } from "../../../modules/shared/store/reference";
import { onMounted, onUnmounted } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import TableReferences from "../components/TableReferences.vue";
import { Reference } from "../../../modules/shared/types";
import { useConfirmDelete } from "@/composables/useConfirmDelete";
import { getNewUuid } from "../../../utils/functions";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const referenceStore = useReferenceStore();
const confirmDelete = useConfirmDelete();
const { t } = useI18n();

onMounted(async () => {
  const title = t("sales.references.title");
  store.setMenuItem({
    icon: PrimeIcons.TICKET,
    title,
  });

  await referenceStore.fetchReferencesByModule("sales");
});

onUnmounted(() => {
  referenceStore.references = undefined;
});

const addReference = () => {
  router.push({ path: `/sales/reference/${getNewUuid()}` });
};

const editReference = (reference: Reference) => {
  router.push({ path: `/sales/reference/${reference.id}` });
};

const deleteReference = (reference: Reference) =>
  confirmDelete({
    name: reference.description,
    remove: async () =>
      (await referenceStore.deleteReference(reference.id)).result,
  });
</script>
<style scoped>
.references-header {
  display: grid;
  grid-template-columns: 3fr 0.1fr;
}
.references-filter {
  display: grid;
  grid-template-columns: 0.2fr 0.8fr;
  align-items: center;
  width: 25vw;
}
</style>
