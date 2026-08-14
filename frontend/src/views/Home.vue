<template>
  <main class="home">
    <img
      :src="brandingStore.mainLogoUrl"
      :alt="`Logotip ${brandingStore.brandName}`"
      class="home__logo"
    />
  </main>
</template>
<script setup lang="ts">
import { onMounted } from "vue";
import { useStore } from "../store";
import { useBrandingStore } from "@/store/branding";

const store = useStore();
const brandingStore = useBrandingStore();

onMounted(() => {
  store.setMenuItem({
    title: "",
    icon: "",
  });
});
</script>

<style scoped>
.home__logo {
  width: min(40vw, 480px);
  height: auto;
  display: block;
  position: absolute;
  top: calc(50% - var(--top-panel-height));
  left: 50%;
  transform: translate(-50%, -50%);
}
</style>
