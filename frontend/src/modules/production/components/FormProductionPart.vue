<template>
  <form v-if="productionPart">
    <section class="three-columns mt-2">
      <div>
        <label class="block text-900 mb-2">Màquina</label>
        <Select
          v-model="productionPart.workcenterId"
          :filter="true"
          :options="
            plantModelStore.workcenters?.sort((a, b) =>
              a.description.localeCompare(b.description),
            )
          "
          optionValue="id"
          optionLabel="description"
          class="w-full"
          @change="getWorkOrders(productionPart.workcenterId)"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Operari</label>
        <Select
          v-model="productionPart.operatorId"
          :filter="true"
          :options="
            plantModelStore.operators
              ?.sort((a, b) => a.surname.localeCompare(b.surname))
              .map((operator) => ({
                value: operator.id,
                label: operator.name + ' ' + operator.surname,
              }))
          "
          optionValue="value"
          optionLabel="label"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.operatorid,
          }"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Data Tíquet</label>
        <DatePicker v-model="productionPart.date" dateFormat="dd/mm/yy" />
      </div>
    </section>
    <section class="mt-2">
      <div>
        <label class="block text-900 mb-2"
          >Ordre Fabricació | Fase | Activitat</label
        >
        <Select
          v-model="selectedDetailedWorkOrder"
          :filter="true"
          :options="
            workOrderStore.detailedWorkOrders
              ?.sort((a, b) => a.workOrderCode.localeCompare(b.workOrderCode))
              .map((workorder) => ({
                label:
                  workorder.workOrderCode +
                  '  (' +
                  workorder.referenceDescription +
                  ') - ' +
                  workorder.workOrderPhaseCode +
                  '  (' +
                  workorder.workOrderPhaseDescription +
                  ') | ' +
                  workorder.machineStatusDescription,
                value: workorder,
              }))
          "
          optionLabel="label"
          class="w-full"
          :class="{
            'p-invalid': validation.errors.workOrderId,
          }"
          @change="setWorkOrderDetail"
        />
      </div>
    </section>
    <section class="three-columns mt-2">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Quantitat"
          id="quantity"
          v-model="productionPart.quantity"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Temps centre de treball (minuts)"
          id="time"
          v-model="productionPart.workcenterTime"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Temps operari (minuts)"
          id="time"
          v-model="productionPart.operatorTime"
        />
      </div>
    </section>
    <br />
    <div class="flex-right">
      <Button label="Guardar" size="small" @click="submitForm" />
    </div>
  </form>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { ProductionPart } from "../types";
import { useToast } from "primevue/usetoast";
import { BaseInputType } from "../../../types/component";
import { usePlantModelStore } from "../store/plantmodel";
import { useWorkOrderStore } from "../store/workorder";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import BaseInput from "../../../components/BaseInput.vue";

const props = defineProps<{
  productionPart: ProductionPart;
  avoidWorkOrderRefresh: boolean;
}>();
const emit = defineEmits<{
  (e: "submit", productionPart: ProductionPart): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const plantModelStore = usePlantModelStore();
const workOrderStore = useWorkOrderStore();

const selectedDetailedWorkOrder = ref(undefined as any | undefined);

const setWorkOrderDetail = () => {
  if (selectedDetailedWorkOrder.value.value) {
    props.productionPart.workOrderId =
      selectedDetailedWorkOrder.value.value.workOrderId;
    props.productionPart.workOrderPhaseId =
      selectedDetailedWorkOrder.value.value.workOrderPhaseId;
    props.productionPart.workOrderPhaseDetailId =
      selectedDetailedWorkOrder.value.value.workOrderPhaseDetailId;
  }
};

const schema = Yup.object().shape({
  operatorId: Yup.string().required("Escull un operari"),
  workcenterId: Yup.string().required("Escull una màquina"),
  workOrderId: Yup.string().required("Escull una ordre de fabricació"),
  workOrderPhaseId: Yup.string().required("Escull una fase"),
  workOrderPhaseDetailId: Yup.string().required("Escull una activitat"),
  quantity: Yup.number()
    .required("Has d'introduir una quantitat entera (pot ser 0)")
    .integer(),
  workcenterTime: Yup.number()
    .required("Has d'introduir el temps i ha de ser major que 0")
    .integer(),
  operatorTime: Yup.number()
    .required("Has d'introduir el temps i ha de ser major que 0")
    .integer(),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.productionPart);
};

const getWorkOrders = async (id: string) => {
  if (props.avoidWorkOrderRefresh) return;

  selectedDetailedWorkOrder.value = undefined;
  await workOrderStore.fetchByWorkcenterId(id);
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.productionPart);
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
