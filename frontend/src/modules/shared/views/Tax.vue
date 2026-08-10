<template>
  <FormTax v-if="tax" :tax="tax" @submit="submitForm" />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { Tax } from "../types";
import { useStore } from "../../../store";
import { useTaxesStore } from "../store/tax";
import { storeToRefs } from "pinia";

import { useToast } from "primevue/usetoast";
import { PrimeIcons } from "@primevue/core/api";
import { FormActionMode } from "../../../types/component";
import FormTax from "../components/FormTax.vue";

const formMode = ref(FormActionMode.EDIT);
const store = useStore();
const route = useRoute();
const router = useRouter();
const taxes = useTaxesStore();
const { tax } = storeToRefs(taxes);
const { t } = useI18n();

const loadView = async () => {
  await taxes.fetchOne(route.params.id as string);

  // Comprovar existencia del proveïdor
  let pageTitle = "";
  if (!tax.value) {
    formMode.value = FormActionMode.CREATE;
    taxes.setNew(route.params.id as string);
    pageTitle = t("shared.taxes.pageTitles.create");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = t("shared.taxes.pageTitles.edit", { name: tax.value.name });
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = tax.value as Tax;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await taxes.create(data);
    message = t("shared.taxes.messages.created");
  } else {
    result = await taxes.update(data.id, data);
    message = t("shared.taxes.messages.updated");
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
