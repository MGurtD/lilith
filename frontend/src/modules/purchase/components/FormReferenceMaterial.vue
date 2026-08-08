<template>
  <section class="three-columns">
    <div class="mt-1">
      <DropdownReferenceType
        :label="t('purchase.materials.fields.materialType')"
        v-model="props.reference.referenceTypeId"
      />
    </div>
    <div class="mt-1">
      <label class="block text-900 mb-2">{{ t("purchase.materials.fields.format") }}</label>
      <Select
        v-model="props.reference.referenceFormatId"
        :options="referenceStore.referenceFormats"
        optionValue="id"
        optionLabel="description"
        class="w-full"
      />
    </div>
    <div class="mt-1">
      <label class="block text-900 mb-2">{{ t("purchase.materials.fields.tax") }}</label>
      <Select
        v-model="props.reference.taxId"
        :options="taxesStore.taxes"
        optionValue="id"
        optionLabel="name"
        class="w-full"
      />
    </div>
  </section>
  <section class="three-columns">
    <div class="mt-1">
      <BaseInput
        :type="BaseInputType.CURRENCY"
        :label="t('purchase.materials.fields.lastCost')"
        id="lastCost"
        v-model="reference.lastCost"
        :disabled="disabled"
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
import { useReferenceStore } from "../../shared/store/reference";
import DropdownReferenceType from "../../shared/components/DropdownReferenceType.vue";
import { ref, onMounted } from "vue";
import Services from "../services";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  reference: Reference;
}>();

const taxesStore = useTaxesStore();
const referenceStore = useReferenceStore();
const { t } = useI18n();

const disabled = ref();

const isDisabled = async () => {
  const receipts = await Services.Receipt.GetByReferenceId(props.reference.id);
  if (receipts && receipts?.length > 0) {
    disabled.value = true;
  } else {
    disabled.value = false;
  }
};
onMounted(() => {
  isDisabled();
});
</script>
