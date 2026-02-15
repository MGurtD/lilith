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
    <section class="three-columns mb-2">
      <div>
        <BaseInput label="Codi de la fase" v-model="phase.code" />
      </div>
      <div>
        <BaseInput label="Descripció" v-model="phase.description" />
      </div>
      <div>
        <DropdownLifecycleStatusTransitions
          label="Estat"
          :statusId="phase.statusId"
          v-model="phase.statusId"
          :class="{
            'p-invalid': validation.errors.statusId,
          }"
        />
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
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :minFractionDigits="2"
          class="mb-2"
          label="Marge de benefici"
          id="profitPercentage"
          v-model="phase.profitPercentage"
          suffix="%"
          @change="workcenterUpdated"
        ></BaseInput>
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
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { computed, onMounted, ref } from "vue";
import { WorkOrder, WorkOrderPhase } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { usePlantModelStore } from "../store/plantmodel";
import { Reference, ReferenceCategoryEnum } from "../../shared/types";
import { useReferenceStore } from "../../shared/store/reference";

const props = defineProps<{
  workorder: WorkOrder;
  phase: WorkOrderPhase;
}>();

const emit = defineEmits<{
  (e: "submit", phase: WorkOrderPhase): void;
  (e: "cancel"): void;
}>();

onMounted(async () => {
  serviceReferences.value =
    await referencesStore.getReferencesByModuleAndCategory(
      "purchase",
      ReferenceCategoryEnum.SERVICE,
    );
});

const toast = useToast();
const plantModelStore = usePlantModelStore();
const referencesStore = useReferenceStore();

const serviceReferences = ref(undefined as undefined | Reference[]);

const preferredWorkcenters = computed(() => {
  return props.phase.workcenterTypeId
    ? plantModelStore.getWorkcentersByTypeId(props.phase.workcenterTypeId)
    : [];
});

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

const workcenterTypeUpdated = () => {
  props.phase.preferredWorkcenterId = null;
  let selectedWorkcenterType = plantModelStore.workcenterTypes?.find(
    (wt) => wt.id === props.phase.workcenterTypeId,
  );
  props.phase.profitPercentage = selectedWorkcenterType!.profitPercentage;
};

const workcenterUpdated = () => {
  let selectedWorkcenter = plantModelStore.workcenters?.find(
    (wt) => wt.id === props.phase.preferredWorkcenterId,
  );
  if (selectedWorkcenter!.profitPercentage > 0) {
    props.phase.profitPercentage = selectedWorkcenter!.profitPercentage;
  } else {
    let selectedWorkcenterType = plantModelStore.workcenterTypes?.find(
      (wt) => wt.id === props.phase.workcenterTypeId,
    );
    props.phase.profitPercentage = selectedWorkcenterType!.profitPercentage;
  }
};

const isExternalWorkChanged = async () => {
  if (props.phase.isExternalWork) {
    props.phase.operatorTypeId = null;
    props.phase.workcenterTypeId = null;
    props.phase.preferredWorkcenterId = null;
  } else {
    props.phase.externalWorkCost = 0;
    props.phase.transportCost = 0;
    props.phase.serviceReferenceId = null;
  }
};

const schema = Yup.object().shape({
  code: Yup.string().required("El codi és obligatori"),
  statusId: Yup.string().required("L'estat és obligatori"),
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
