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
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
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
const confirm = useConfirm();
const toast = useToast();
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

const deleteReference = (reference: Reference) => {
  confirm.require({
    message: t("purchase.materials.messages.confirmDelete", {
      name: reference.description,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const response = await referenceStore.deleteReference(reference.id);
      if (!response.result) {
        toast.add({
          severity: "warn",
          summary: response.errors[0],
          life: 5000,
          closable: true,
        });
      } else {
        toast.add({
          severity: "success",
          summary: t("purchase.materials.messages.deleted"),
          life: 5000,
        });
      }
    },
  });
};
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
