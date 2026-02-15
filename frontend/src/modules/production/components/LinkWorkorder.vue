<template>
  <span v-if="id" class="link" @click="navigateToWorkorder">
    {{ code }}
  </span>
</template>
<script setup lang="ts">
import { useRouter } from "vue-router";

const props = defineProps<{
  id: string;
  code: string;
}>();

const router = useRouter();

const navigateToWorkorder = () => {
  router.push({ path: `/workorder/${props.id}` });
};
</script>
<style scoped>
.link {
  text-decoration: none;
  color: var(--p-blue-800);
  font-weight: bold;
  font-size: 0.9rem;
  transition: color 0.3s;
}

.link:hover {
  color: var(--p-blue-600);
  cursor: pointer;
}
</style>
