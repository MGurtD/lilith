<template>
  <TableMaterials
    :references="referenceStore.references"
    :filter="filter"
    @add="addReference"
    @edit="editReference"
    @delete="deleteReference"
  ></TableMaterials>
</template>
<script setup lang="ts">
import TableMaterials from "../components/TableMaterials.vue";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { onMounted, ref, watch } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { Reference } from "../../../modules/shared/types";
import { useConfirmDelete } from "@/composables/useConfirmDelete";
import { getNewUuid } from "../../../utils/functions";
import { useReferenceStore } from "../../../modules/shared/store/reference";
import { useTaxesStore } from "../../shared/store/tax";
import { useReferenceTypeStore } from "../../shared/store/referenceType";
import { useI18n } from "vue-i18n";

const router = useRouter();
const store = useStore();
const referenceStore = useReferenceStore();
const taxesStore = useTaxesStore();
const referenceTypeStore = useReferenceTypeStore();
const confirmDelete = useConfirmDelete();
const { t, locale } = useI18n();

const filter = ref({
  code: "",
  referenceTypeId: "",
  referenceCategory: "",
});

const setPageTitle = () => {
  store.setMenuItem({
    icon: PrimeIcons.TICKET,
    title: t("purchase.materials.title"),
  });
};

onMounted(async () => {
  setPageTitle();

  await referenceStore.fetchReferencesByModule("purchase");
  taxesStore.fetchAll();
  referenceTypeStore.fetchAll();
});

watch(locale, setPageTitle);

const addReference = () => {
  router.push({
    path: `/material/${getNewUuid()}/${filter.value.referenceCategory}`,
  });
};

const editReference = (reference: Reference) => {
  router.push({
    path: `/material/${reference.id}/${filter.value.referenceCategory}`,
  });
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
