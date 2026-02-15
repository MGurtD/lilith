<template>
  <FileEntityPicker entity="Reports" :id="REPORTS_ENTITY_ID" title="Informes" />
</template>
<script setup lang="ts">
import { onMounted } from "vue";
import FileEntityPicker from "../components/FileEntityPicker.vue";
import { useStore } from "../store";
import { REPORTS_ENTITY_ID } from "../services/report.service";

const store = useStore();

onMounted(() => {
  store.setMenuItem({
    title: "Gestor d'informes",
    icon: "",
  });
});
</script>
