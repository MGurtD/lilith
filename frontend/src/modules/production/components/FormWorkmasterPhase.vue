<template>
  <form v-if="phase">
    <div>
      <Button
        label="Guardar Fase"
        class="grid_add_row_button"
        size="small"
        @click="submitForm"
      />
      <br />
    </div>
    <section class="two-columns-2575 mb-2">
      <div>
        <BaseInput label="Codi de la fase" v-model="phase.code" />
      </div>
      <div>
        <BaseInput label="Descripció" v-model="phase.description" />
      </div>
    </section>
    <section class="four-columns mb-2">
      <div>
        <label class="block text-900 mb-2">Tipus de màquina</label>
        <Select
          v-model="phase.workcenterTypeId"
          :options="plantModelStore.workcenterTypes"
          optionValue="id"
          optionLabel="name"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.workcenterTypeId,
          }"
          @change="workcenterTypeUpdated"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Màquina preferida</label>
        <Select
          v-model="phase.preferredWorkcenterId"
          :options="preferredWorkcenters"
          optionValue="id"
          optionLabel="description"
          class="w-full"
          @change="workcenterUpdated"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Marge de benefici</label>
        <Select
          v-if="workcenterProfitPercentages.length > 0"
          v-model="phase.profitPercentage"
          :options="workcenterProfitPercentages"
          optionValue="profitPercentage"
          optionLabel="profitPercentage"
          class="w-full"
          placeholder="Selecciona un percentatge"
          @change="onProfitPercentageChanged"
        >
          <template #value="slotProps">
            <span v-if="slotProps.value">{{ slotProps.value }}%</span>
            <span v-else>{{ slotProps.placeholder }}</span>
          </template>
          <template #option="slotProps">
            {{ slotProps.option.profitPercentage }}%
          </template>
        </Select>
        <InputNumber
          v-else
          v-model="phase.profitPercentage"
          :minFractionDigits="2"
          :maxFractionDigits="2"
          suffix="%"
          class="w-full"
          :disabled="true"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Tipus d'operari</label>
        <Select
          v-model="phase.operatorTypeId"
          :options="plantModelStore.operatorTypes"
          optionValue="id"
          optionLabel="description"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.operatorTypeId,
          }"
        />
      </div>
    </section>
    <section class="four-columns mb-2">
      <div>
        <label class="block text-900 mt-1 mb-1">Externa</label>
        <Checkbox
          v-model="phase.isExternalWork"
          class="w-full"
          :binary="true"
          @change="isExternalWorkChanged"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Servei</label>
        <Select
          v-model="phase.serviceReferenceId"
          :options="serviceReferences"
          optionValue="id"
          :optionLabel="(r) => r.code + ' - ' + r.description"
          :disabled="!phase.isExternalWork"
          class="w-full"
          @change="onServiceReferenceChanged"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Cost servei"
          v-model="phase.externalWorkCost"
          :disabled="!phase.isExternalWork"
          :class="{
            'p-invalid': validation.errors.externalWorkCost,
          }"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Cost transport"
          v-model="phase.transportCost"
          :disabled="!phase.isExternalWork"
          :class="{
            'p-invalid': validation.errors.externalWorkCost,
          }"
        />
      </div>
    </section>
  </form>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  WorkMaster,
  WorkMasterPhase,
  WorkcenterProfitPercentage,
} from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { usePlantModelStore } from "../store/plantmodel";
import { useReferenceStore } from "../../shared/store/reference";
import { Reference, ReferenceCategoryEnum } from "../../shared/types";
import { useExerciseStore } from "../../shared/store/exercise";

const props = defineProps<{
  workmaster: WorkMaster;
  phase: WorkMasterPhase;
}>();

const emit = defineEmits<{
  (e: "submit", phase: WorkMasterPhase): void;
  (e: "cancel"): void;
}>();

const workcenterProfitPercentages = ref([] as WorkcenterProfitPercentage[]);

onMounted(async () => {
  await exerciseStore.fetchActive();
  serviceReferences.value =
    await referencesStore.getReferencesByModuleAndCategory(
      "purchase",
      ReferenceCategoryEnum.SERVICE,
    );
});

const toast = useToast();
const plantModelStore = usePlantModelStore();
const referencesStore = useReferenceStore();
const exerciseStore = useExerciseStore();

const serviceReferences = ref(undefined as undefined | Reference[]);
const currentExercise = computed(() => {
  const now = new Date();
  // Assegura't que els exercicis estiguin carregats
  if (!exerciseStore.exercises) return undefined;

  return exerciseStore.exercises.find(
    (e) =>
      !e.disabled && new Date(e.startDate) <= now && new Date(e.endDate) >= now,
  );
});

const externalProfit = computed(() => {
  return currentExercise.value?.externalProfit || 0;
});

const preferredWorkcenters = computed(() => {
  return props.phase.workcenterTypeId
    ? plantModelStore.getWorkcentersByTypeId(props.phase.workcenterTypeId)
    : [];
});

const workcenterTypeUpdated = () => {
  props.phase.preferredWorkcenterId = null;
  let selectedWorkcenterType = plantModelStore.workcenterTypes?.find(
    (wt) => wt.id === props.phase.workcenterTypeId,
  );
  props.phase.profitPercentage = selectedWorkcenterType!.profitPercentage;
};
const workcenterUpdated = async () => {
  let selectedWorkcenter = plantModelStore.workcenters?.find(
    (wt) => wt.id === props.phase.preferredWorkcenterId,
  );

  // Carregar percentatges de benefici del workcenter
  if (props.phase.preferredWorkcenterId) {
    await plantModelStore.fetchWorkcenterProfitPercentagesByWorkcenterId(
      props.phase.preferredWorkcenterId,
    );
    workcenterProfitPercentages.value =
      plantModelStore.workcenterProfitPercentages || [];
  } else {
    workcenterProfitPercentages.value = [];
  }

  // Establir el valor per defecte
  if (workcenterProfitPercentages.value.length > 0) {
    // Si hi ha percentatges personalitzats, seleccionar el primer
    props.phase.profitPercentage =
      workcenterProfitPercentages.value[0].profitPercentage;
  } else if (selectedWorkcenter && selectedWorkcenter.profitPercentage > 0) {
    // Si no hi ha percentatges personalitzats, usar el del workcenter
    props.phase.profitPercentage = selectedWorkcenter.profitPercentage;
  } else {
    // Si no, usar el del tipus de workcenter
    let selectedWorkcenterType = plantModelStore.workcenterTypes?.find(
      (wt) => wt.id === props.phase.workcenterTypeId,
    );
    props.phase.profitPercentage =
      selectedWorkcenterType?.profitPercentage || 0;
  }
};

const onProfitPercentageChanged = () => {
  // El v-model ja actualitza automàticament el valor
};
const isExternalWorkChanged = async () => {
  if (props.phase.isExternalWork) {
    props.phase.operatorTypeId = null;
    props.phase.workcenterTypeId = null;
    props.phase.preferredWorkcenterId = null;
    props.phase.profitPercentage = externalProfit.value;
  } else {
    props.phase.externalWorkCost = 0;
    props.phase.transportCost = 0;
    props.phase.serviceReferenceId = null;
    props.phase.profitPercentage = 0;
  }
};

const onServiceReferenceChanged = () => {
  if (serviceReferences.value) {
    const selectedReference = serviceReferences.value.find(
      (r) => r.id === props.phase.serviceReferenceId,
    );
    if (selectedReference) {
      props.phase.externalWorkCost = selectedReference.price;
      props.phase.transportCost = selectedReference.transportAmount;
    }
  }
};

const schema = Yup.object().shape({
  code: Yup.string().required("El codi és obligatori"),
  /*.test("unique-code", "El codi ja existeix", function (value) {
      const { phases } = props.workmaster || {}; // Accede a workmaster.phases
      if (!phases || !Array.isArray(phases)) return true; // Si no hay fases, no valida como duplicado
      return  !phases.some((phase) => phase.code === value); // Verifica duplicados
    }),*/
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.phase);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    if (props.phase.preferredWorkcenterId === "") {
      props.phase.preferredWorkcenterId = null;
    }

    emit("submit", props.phase);
  } else {
    let errors = "";
    Object.entries(validation.value.errors).forEach((e) => {
      errors += `${e[1].map((e) => e)}.   `;
    });
    toast.add({
      severity: "warn",
      summary: "Formulari inválid",
      detail: errors,
      life: 5000,
    });
  }
};
</script>
