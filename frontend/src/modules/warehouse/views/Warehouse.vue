<template>
  <header>
    <FormWarehouse
      v-if="warehouse"
      :warehouse="warehouse"
      @submit="submitForm"
    />
  </header>
  <section class="mt-5" v-if="warehouse">
    <TableLocations
      :warehouse="warehouse"
      :locations="warehouse.locations"
      @add="onAddLocation"
      @edit="onEditLocation"
      @delete="onDeleteLocation"
    ></TableLocations>
  </section>
</template>
<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useWarehouseStore } from "../store/warehouse";
import { storeToRefs } from "pinia";
import { PrimeIcons } from "@primevue/core/api";
import { Warehouse, Location } from "../types";
import { FormActionMode } from "../../../types/component";
import TableLocations from "../components/TableLocations.vue";
import FormWarehouse from "../components/FormWarehouse.vue";

const formMode = ref(FormActionMode.EDIT);
const { t, locale } = useI18n();
const store = useStore();
const route = useRoute();
const router = useRouter();
const warehouseStore = useWarehouseStore();
const { warehouse } = storeToRefs(warehouseStore);

const loadView = async () => {
  await warehouseStore.fetchWarehouse(route.params.id as string);
  let pageTitle = "";
  if (!warehouse.value) {
    formMode.value = FormActionMode.CREATE;
    warehouseStore.setNewWarehouse(route.params.id as string);
    pageTitle = t("warehouse.warehouse.newTitle");
  } else {
    formMode.value = FormActionMode.EDIT;
    pageTitle = t("warehouse.warehouse.editTitle", { name: warehouse.value.name });
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

watch(locale, async () => {
  await loadView();
});

const toast = useToast();
const submitForm = async () => {
  const data = warehouse.value as Warehouse;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    data.defaultLocationId = null;
    result = await warehouseStore.createWarehouse(data);
    message = t("warehouse.messages.warehouseCreated");
  } else {
    if (!data.defaultLocationId) {
      toast.add({
        severity: "warn",
        summary: t("warehouse.messages.selectDefaultLocation"),
        life: 5000,
      });
      return;
    }

    result = await warehouseStore.updateWarehouse(data.id, data);
    message = t("warehouse.messages.warehouseUpdated");
  }

  if (result) {
    toast.add({
      severity: "success",
      summary: message,
      life: 5000,
    });
    if (formMode.value === FormActionMode.EDIT) router.back();
  }
};

const onAddLocation = (location: Location): void => {
  warehouseStore.createLocation(location);
};
const onEditLocation = (location: Location): void => {
  warehouseStore.updateLocation(location.id, location);
};
const onDeleteLocation = (location: Location): void => {
  if (warehouse.value && location.id === warehouse.value.defaultLocationId) {
    toast.add({
      severity: "warn",
      summary: t("warehouse.messages.locationHasDependencies"),
      detail: t("warehouse.messages.defaultLocationCannotBeDeleted"),
      life: 6000,
    });
    return;
  }

  warehouseStore.deleteLocation(location.id);
};
</script>
