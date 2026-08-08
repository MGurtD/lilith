<template>
  <form v-if="model">
    <section class="mb-2">
      <BaseInput
        :label="t('purchase.purchaseRate.fields.name')"
        id="prName"
        v-model="model.name"
        class="w-full"
      />
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.purchaseRate.fields.validFrom") }}</label>
        <DatePicker
          v-model="model.validFrom"
          class="w-full"
          dateFormat="dd/mm/yy"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ t("purchase.purchaseRate.fields.validTo") }}</label>
        <DatePicker
          v-model="model.validTo"
          class="w-full"
          dateFormat="dd/mm/yy"
        />
      </div>
    </section>
    <div class="mt-2 text-right">
      <Button :label="t('purchase.purchaseRate.actions.save')" icon="pi pi-save" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { PurchaseRate } from "../types";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  purchaseRate: PurchaseRate;
}>();

const emit = defineEmits<{
  (e: "submit", rate: PurchaseRate): void;
}>();

const model = ref<PurchaseRate>({ ...props.purchaseRate });
const { t } = useI18n();

const submitForm = () => {
  emit("submit", model.value);
};
</script>
