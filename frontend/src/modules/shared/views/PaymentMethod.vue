<template>
  <FormPaymentMethod
    v-if="paymentMethod"
    :payment-method="paymentMethod"
    @submit="submitForm"
  />
</template>
<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";

import { storeToRefs } from "pinia";
import { PaymentMethod } from "../types";
import { useStore } from "../../../store";

import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import FormPaymentMethod from "../components/FormPaymentMethod.vue";
import { usePaymentMethodStore } from "../store/paymentMethod";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const router = useRouter();
const store = useStore();
const paymentMethodStore = usePaymentMethodStore();
const { paymentMethod } = storeToRefs(paymentMethodStore);

const loadView = async () => {
  await paymentMethodStore.fetchOne(route.params.id as string);

  // Comprovar existencia del proveïdor
  let pageTitle = "";
  if (!paymentMethod.value) {
    formMode.value = FormActionMode.CREATE;
    paymentMethodStore.setNew(route.params.id as string);
    pageTitle = "Alta de forma de pagament";
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Forma de pagament ${paymentMethod.value.name}`;
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
  const data = paymentMethod.value as PaymentMethod;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await paymentMethodStore.create(data);
    message = "Forma de pagament creada correctament";
  } else {
    result = await paymentMethodStore.update(data.id, data);
    message = "Forma de pagament actualizada correctament";
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
