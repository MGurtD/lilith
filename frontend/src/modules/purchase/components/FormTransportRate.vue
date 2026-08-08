<template>
  <form v-if="model">
    <section class="two-columns mb-2">
      <BaseInput
        :label="$t('purchase.fields.name')"
        id="trName"
        v-model="model.name"
      />
      <BaseInput
        :label="$t('purchase.fields.description')"
        id="trDescription"
        v-model="model.description"
      />
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.startDate") }}</label>
        <DatePicker
          v-model="model.validFrom"
          class="w-full"
          dateFormat="dd/mm/yy"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.endDate") }}</label>
        <DatePicker
          v-model="model.validTo"
          class="w-full"
          dateFormat="dd/mm/yy"
        />
      </div>
    </section>
    <div class="mt-2">
      <Button :label="$t('common.save')" class="mr-2" @click="submitForm" />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { TransportRate } from "../types";

const props = defineProps<{
  transportRate: TransportRate;
}>();

const emit = defineEmits<{
  (e: "submit", rate: TransportRate): void;
}>();

const model = ref<TransportRate>({ ...props.transportRate });


const submitForm = () => {
  emit("submit", model.value);
};
</script>
