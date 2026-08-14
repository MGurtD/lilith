<template>
  <section class="four-columns">
    <div class="mt-1">
      <BaseInput
        :type="BaseInputType.CURRENCY"
        :label="t('purchase.materials.fields.servicePrice')"
        v-model="reference.price"
      />
    </div>
    <div class="mt-1">
      <BaseInput
        :type="BaseInputType.CURRENCY"
        :label="t('purchase.materials.fields.transportPrice')"
        v-model="reference.transportAmount"
      />
    </div>
    <div class="mt-1">
      <label class="block text-900 mb-2">{{ t("purchase.materials.fields.tax") }}</label>
      <Select
        v-model="reference.taxId"
        :options="taxesStore.taxes"
        optionValue="id"
        optionLabel="name"
        class="w-full"
      />
    </div>
    <div>
      <label class="block text-900 mb-2">{{ t("purchase.materials.fields.disabled") }}</label>
      <Checkbox v-model="reference.disabled" :binary="true" />
    </div>
  </section>
</template>
<script setup lang="ts">
import BaseInput from "../../../components/BaseInput.vue";
import { Reference } from "../../shared/types";
import { BaseInputType } from "../../../types/component";
import { useTaxesStore } from "../../shared/store/tax";
import { onMounted } from "vue";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  reference: Reference;
}>();

const taxesStore = useTaxesStore();
const { t } = useI18n();

onMounted(async () => {
  if (!taxesStore.taxes) await taxesStore.fetchAll();
});

const emit = defineEmits<{
  (e: "submit", reference: Reference): void;
  (e: "cancel"): void;
}>();
</script>
