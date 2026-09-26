<template>
  <FileEntityPicker entity="Reports" :id="REPORTS_ENTITY_ID" :title="t('reports.filePickerTitle')" />
</template>
<script setup lang="ts">
import { onMounted } from "vue";
import { useI18n } from "vue-i18n";
import FileEntityPicker from "@/components/FileEntityPicker.vue";
import { useStore } from "@/store";
import { REPORTS_ENTITY_ID } from "@/services/report.service";

const { t } = useI18n();
const store = useStore();

onMounted(() => {
  store.setMenuItem({
    title: t("reports.pageTitle"),
    icon: "",
  });
});
</script>
