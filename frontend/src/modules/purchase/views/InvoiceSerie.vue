<template>
  <FormInvoiceSerie
    v-if="purchaseInvoiceSerie"
    :purchaseInvoiceSerie="purchaseInvoiceSerie"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { useStore } from "../../../store";
import { storeToRefs } from "pinia";
import { usePurchaseInvoiceSeries } from "../store/purchaseInvoiceSeries";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import FormInvoiceSerie from "../components/FormInvoiceSerie.vue";
import { InvoiceSerie } from "../types";
import { useI18n } from "vue-i18n";

const formMode = ref(FormActionMode.EDIT);
const router = useRouter();
const route = useRoute();
const store = useStore();
const purchaseInvoiceSerieStore = usePurchaseInvoiceSeries();
const { purchaseInvoiceSerie } = storeToRefs(purchaseInvoiceSerieStore);
const { t } = useI18n();

const loadView = async () => {
  await purchaseInvoiceSerieStore.fetchPurchaseInvoiceSerie(
    route.params.id as string
  );

  let pageTitle = "";
  if (!purchaseInvoiceSerie.value) {
    formMode.value = FormActionMode.CREATE;
    purchaseInvoiceSerieStore.setNewPurchaseInvoiceSerie(
      route.params.id as string
    );
    pageTitle = t("purchase.invoiceSeries.createTitle");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = t("purchase.invoiceSeries.detailTitle", {
      name: purchaseInvoiceSerie.value.name,
    });
  }

  store.setMenuItem({
    icon: PrimeIcons.POUND,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = purchaseInvoiceSerie.value as InvoiceSerie;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await purchaseInvoiceSerieStore.createPurchaseInvoiceSerie(data);
    message = t("purchase.invoiceSeries.messages.created");
  } else {
    result = await purchaseInvoiceSerieStore.updatePurchaseInvoiceSerie(
      data.id,
      data
    );
    message = t("purchase.invoiceSeries.messages.updated");
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
