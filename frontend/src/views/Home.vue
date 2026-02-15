<template>
  <main class="home">
    <img src="../assets/images/logo.jpg" alt="Logo Temges" class="home__logo" />
  </main>
</template>
<script setup lang="ts">
import { onMounted } from "vue";
import { useStore } from "../store";

const store = useStore();

onMounted(() => {
  store.setMenuItem({
    title: "",
    icon: "",
  });
});
</script>

<style scoped>
.home__logo {
  height: 30vh;
  display: block;
  position: absolute;
  top: calc(50% - var(--top-panel-height));
  left: 50%;
  transform: translate(-50%, -50%);
}
</style>
