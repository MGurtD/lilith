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
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { getNewUuid } from "../../../utils/functions";

const router = useRouter();
const store = useStore();
const referenceStore = useReferenceStore();
const confirm = useConfirm();
const toast = useToast();

onMounted(async () => {
  let title = "Referencies de venta";
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

const deleteReference = (reference: Reference) => {
  confirm.require({
    message: `Está segur que vol eliminar la refèrencia ${reference.description}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const response = await referenceStore.deleteReference(reference.id);
      console.log("Delete reference response:", response);
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
          summary: "Referencia eliminada correctament",
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
