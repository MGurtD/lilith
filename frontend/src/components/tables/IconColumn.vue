<template>
  <i :class="value" v-if="value"></i>
</template>

<script setup lang="ts">
defineProps<{
  value?: string;
}>();
</script>
