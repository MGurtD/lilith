<template>
  <div class="production-content"></div>
</template>

<script setup lang="ts">
import { WorkcenterViewState } from "../../types";

interface Props {
  workcenter: WorkcenterViewState;
}

const props = defineProps<Props>();
</script>

<style scoped>
.production-content {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
</style>
