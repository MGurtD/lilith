<template>
  <FormRejectionReason
    v-if="rejectionReason"
    :rejection-reason="rejectionReason"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

import { RejectionReason } from "../types";
import { useStore } from "../../../store";
import { FormActionMode } from "../../../types/component";
import router from "../../../router";
import FormRejectionReason from "../components/FormRejectionReason.vue";
import { useRejectionReasonStore } from "../store/rejectionreason";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const store = useStore();
const rejectionReasonStore = useRejectionReasonStore();
const { rejectionReason } = storeToRefs(rejectionReasonStore);
const { t } = useI18n();

const loadView = async () => {
  await rejectionReasonStore.fetchOne(route.params.id as string);
  let pageTitle = "";
  if (!rejectionReason.value) {
    formMode.value = FormActionMode.CREATE;
    rejectionReasonStore.setNew(route.params.id as string);
    pageTitle = t("production.detail.createRejectionReason");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = t("production.detail.rejectionReasonTitle", {
      name: rejectionReason.value.name,
    });
  }

  store.setMenuItem({
    icon: PrimeIcons.EXCLAMATION_TRIANGLE,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = rejectionReason.value as RejectionReason;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await rejectionReasonStore.create(data);
    message = t("production.detail.createdRejectionReason");
  } else {
    result = await rejectionReasonStore.update(data.id, data);
    message = t("production.detail.updatedRejectionReason");
  }

  if (result) {
    toast.add({
      severity: "success",
      summary: message,
      life: 5000,
    });
    router.back();
  }
};
</script>
