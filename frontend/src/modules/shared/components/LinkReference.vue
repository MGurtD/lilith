<template>
  <span
    v-if="referenceStore.module && id"
    class="link"
    @click="navegateToReference"
  >
    {{
      fullName
        ? referenceStore.getFullNameById(id)
        : referenceStore.getShortNameById(id)
    }}
  </span>
</template>
<script setup lang="ts">
import { useReferenceStore } from "../store/reference";
import { useRouter } from "vue-router";

const props = defineProps<{
  id: string;
  category?: string;
  fullName?: boolean;
}>();

const referenceStore = useReferenceStore();
const router = useRouter();

const navegateToReference = () => {
  let path = "";
  if (referenceStore.module === "purchase") {
    let category = props.category;
    let reference = referenceStore.references!.find((r) => r.id === props.id);
    if (!category && reference) {
      category = reference.categoryName;
    }

    path = `/material/${props.id}/${category}`;
  } else if (referenceStore.module === "sales") {
    path = `/sales/reference/${props.id}`;
  }

  router.push({
    path,
  });
};
</script>
<style scoped>
.link {
  text-decoration: none; /* Elimina la subrayado predeterminado */
  color: var(--p-blue-800); /* Color del texto */
  font-weight: bold; /* Fuente en negrita */
  font-size: 0.9rem;
  transition: color 0.3s; /* Agrega una transición suave al color del texto */
}

.link:hover {
  color: var(
    --p-blue-600
  ); /* Cambia el color del texto al pasar el mouse sobre el enlace */
  cursor: pointer;
}
</style>
