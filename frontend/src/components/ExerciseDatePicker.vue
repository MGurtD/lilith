<script setup lang="ts">
import { Exercise } from "../modules/shared/types";
import { useStore } from "../store";

const store = useStore();

defineProps<{
  exercises: Array<Exercise> | undefined;
}>();

const emit = defineEmits<{
  (event: "rangeSelected", dates: Array<Date>): void;
}>();

const setExerciseDates = () => {
  if (store.exercisePicker.exercise) {
    store.exercisePicker.dates = [
      new Date(store.exercisePicker.exercise!.startDate),
      new Date(store.exercisePicker.exercise!.endDate),
    ];
    emitDateSelected();
  }
};

const emitDateSelected = () =>
  emit("rangeSelected", store.exercisePicker.dates!);
</script>
<template>
  <Select
    v-model="store.exercisePicker.exercise"
    :options="exercises"
    optionLabel="name"
    placeholder="Any"
    @change="setExerciseDates"
  />
  <DatePicker
    placeholder="Període"
    v-model="store.exercisePicker.dates"
    selectionMode="range"
    dateFormat="dd/mm/yy"
    @date-select="emitDateSelected"
  />
</template>
