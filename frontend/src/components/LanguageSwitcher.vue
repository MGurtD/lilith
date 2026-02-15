<script setup lang="ts">
import { computed, onMounted } from "vue";
import { useStore } from "../store";
import { useLanguageStore } from "../store/languages";

const appStore = useStore();
const langStore = useLanguageStore();

const props = defineProps<{
  changeAppLanguage: boolean;
}>();

const options = computed(() => langStore.options);
const model = computed({
  get: () => appStore.language.current,
  set: async (val: string) => {
    if (props.changeAppLanguage) {
      await appStore.changeLanguage(val);
    }
  },
});

onMounted(async () => {
  await langStore.fetchAll();
  if (!appStore.language?.current) await appStore.initLanguage();
});
</script>

<template>
  <Select
    :options="options"
    optionLabel="label"
    optionValue="value"
    v-model="model"
    class="w-full"
  >
    <template #value="slotProps">
      <div class="flex p-ai-center" v-if="slotProps.value">
        <img
          v-if="langStore.byCode(slotProps.value)?.icon"
          :src="langStore.byCode(slotProps.value)?.icon"
          :alt="langStore.byCode(slotProps.value)?.name"
          class="flag"
        />
        <span>{{
          langStore.byCode(slotProps.value)?.name || slotProps.value
        }}</span>
      </div>
      <span v-else>{{ $t("general.select") || "Select" }}</span>
    </template>
    <template #option="slotProps">
      <div class="flex p-ai-center">
        <img
          v-if="slotProps.option.icon"
          :src="slotProps.option.icon"
          :alt="slotProps.option.label"
          class="flag"
        />
        <span>{{ slotProps.option.label }}</span>
      </div>
    </template>
  </Select>
</template>

<style scoped>
.flag {
  width: 18px;
  height: 18px;
  object-fit: cover;
  border-radius: 2px;
  margin-right: 1rem;
}
</style>
