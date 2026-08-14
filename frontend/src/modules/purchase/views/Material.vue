<template>
  <section v-if="reference">
    <FormMaterial :reference="reference" @submit="submitForm" />
  </section>

  <section class="mt-4">
    <TableSupplierReferences
      v-if="reference && referenceStore.referenceSuppliers"
      :title="t('purchase.materials.suppliersTitle')"
      :referenceId="reference.id"
      :supplier-references="referenceStore.referenceSuppliers"
      :formActionMode="formMode"
      @create="addSupplier"
      @update="editSupplier"
      @delete="removeSupplier"
    />
  </section>
</template>
<script setup lang="ts">
import TableSupplierReferences from "../components/TableSupplierReferences.vue";
import { onMounted, ref, watch } from "vue";
import { FormActionMode } from "../../../types/component";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useReferenceStore } from "../../shared/store/reference";
import {
  Reference,
  ReferenceCategory,
  ReferenceCategoryEnum,
} from "../../shared/types";
import { SupplierReference } from "../types";
import { useSuppliersStore } from "../store/suppliers";
import FormMaterial from "../components/FormMaterial.vue";
import { useI18n } from "vue-i18n";

const formMode = ref(FormActionMode.EDIT);
const route = useRoute();
const router = useRouter();
const store = useStore();
const supplierStore = useSuppliersStore();
const referenceStore = useReferenceStore();
const { t, locale } = useI18n();

const { reference } = storeToRefs(referenceStore);
const id = ref("");
const category = ref({} as ReferenceCategory);

onMounted(async () => {
  id.value = route.params.id as string;
  category.value = referenceStore.referenceCategories.find(
    (c) => c.code === route.params.category,
  )!;

  await loadView();
});

const loadView = async () => {
  supplierStore.fetchSuppliers();
  referenceStore.fetchReferenceSuppliers(id.value);
  await referenceStore.fetchReference(id.value);

  if (!reference.value) {
    formMode.value = FormActionMode.CREATE;
    referenceStore.setNewReference(
      id.value,
      category.value.code as ReferenceCategoryEnum,
    );
  } else {
    formMode.value = FormActionMode.EDIT;
  }

  setPageTitle();
};

const setPageTitle = () => {
  const pageTitle = !reference.value
    ? t("purchase.materials.createTitle", {
        category: category.value.description,
      })
    : t("purchase.materials.detailTitle", {
        category: category.value.description,
        code: reference.value.code,
        description: reference.value.description,
      });

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });
};

watch(locale, setPageTitle);

const toast = useToast();
const submitForm = async () => {
  const data = reference.value as Reference;
  let result = false;
  let message = "";

  if (data.categoryName === ReferenceCategoryEnum.TOOL) {
    const unitFormat = referenceStore.referenceFormats?.find(
      (f) => f.code === "UNITATS",
    );
    if (unitFormat) {
      data.referenceFormatId = unitFormat.id;
    }
  }

  if (formMode.value === FormActionMode.CREATE) {
    result = await referenceStore.createReference(data);
    if (result) message = t("purchase.materials.messages.created");
    else message = t("purchase.materials.messages.alreadyExists");
  } else {
    result = await referenceStore.updateReference(data.id, data);
    message = t("purchase.materials.messages.updated");
  }

  toast.add({
    severity: result ? "success" : "warn",
    summary: message,
    life: 5000,
  });

  if (result) {
    if (formMode.value === FormActionMode.CREATE) {
      loadView();
    } else {
      router.back();
    }
  }
};

const addSupplier = async (supplier: SupplierReference) => {
  const result = await referenceStore.addSupplier(supplier);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.materials.messages.referenceAdded"),
      life: 5000,
    });
  }
};

const editSupplier = async (supplier: SupplierReference) => {
  const result = await referenceStore.updateSupplier(supplier);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.materials.messages.referenceUpdated"),
      life: 5000,
    });
  }
};

const removeSupplier = async (supplier: SupplierReference) => {
  const result = await referenceStore.deleteSupplier(supplier);
  if (result) {
    toast.add({
      severity: "success",
      summary: t("purchase.materials.messages.referenceDeleted"),
      life: 5000,
    });
  }
};
</script>
