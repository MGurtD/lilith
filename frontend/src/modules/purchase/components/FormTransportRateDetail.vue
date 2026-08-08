<template>
  <form v-if="model">
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.minimumWeight") }}</label>
        <InputNumber
          v-model="model.minWeight"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.maximumWeight") }}</label>
        <InputNumber
          v-model="model.maxWeight"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.minimumVolume") }}</label>
        <InputNumber
          v-model="model.minVolume"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.maximumVolume") }}</label>
        <InputNumber
          v-model="model.maxVolume"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.minimumDistance") }}</label>
        <InputNumber
          v-model="model.minDistance"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.maximumDistance") }}</label>
        <InputNumber
          v-model="model.maxDistance"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
        />
      </div>
    </section>
    <section class="two-columns mb-2">
      <div>
        <label class="block text-900 mb-2">{{ $t("purchase.fields.price") }}</label>
        <InputNumber
          v-model="model.price"
          class="w-full"
          :minFractionDigits="2"
          :maxFractionDigits="4"
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
import { TransportRateDetail } from "../types";

const props = defineProps<{
  detail: TransportRateDetail;
}>();

const emit = defineEmits<{
  (e: "submit", detail: TransportRateDetail): void;
}>();

const model = ref<TransportRateDetail>({ ...props.detail });

const submitForm = () => {
  emit("submit", model.value);
};
</script>
