<template>
  <form v-if="detail">
    <section class="three-columns">
      <div>
        <DropdownReference
          label="Referència de compra"
          v-model="detail.referenceId"
          :fullName="true"
          :disabled="detail.receivedQuantity > 0"
          @update:modelValue="getReferenceInfo"
        ></DropdownReference>
      </div>
      <div>
        <DropdownLifecycleStatusTransitions
          label="Estat"
          :statusId="detail.statusId"
          v-model="detail.statusId"
        />
      </div>
      <div>
        <label class="block text-900 mb-2">Data prevista</label>
        <DatePicker
          label="Data prevista"
          v-model="detail.expectedReceiptDate"
          dateFormat="dd/mm/yy"
        />
      </div>
    </section>
    <section>
      <div>
        <BaseInput
          :type="BaseInputType.TEXT"
          label="Descripció"
          id="description"
          v-model="detail.description"
        />
      </div>
    </section>

    <section class="three-columns mt-2">
      <div>
        <BaseInput
          :disabled="detail.receivedQuantity > 0"
          :type="BaseInputType.NUMERIC"
          label="Quantitat"
          v-model="detail.quantity"
          @input="calculateAmount"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Preu"
          v-model="detail.amount"
          :disabled="detail.receivedQuantity > 0"
        />
      </div>
    </section>

    <Button
      label="Crear"
      @click="submitForm"
      style="float: right"
      :size="'small'"
      class="mt-2"
    />
  </form>
</template>

<script setup lang="ts">
import DropdownReference from "../../shared/components/DropdownReference.vue";
import DropdownLifecycleStatusTransitions from "../../shared/components/DropdownLifecycleStatusTransitions.vue";
import { ref, watch } from "vue";
import { PurchaseOrderDetail, PurchaseOrder } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import PurchaseServices from "../services";
import SharedServices from "../../shared/services";
import { useReferenceStore } from "../../shared/store/reference";

const props = defineProps<{
  detail: PurchaseOrderDetail;
  order: PurchaseOrder;
}>();

const emit = defineEmits<{
  (e: "submit", detail: PurchaseOrderDetail): void;
  (e: "cancel"): void;
}>();

const toast = useToast();

const schema = Yup.object().shape({
  quantity: Yup.number().min(1).required("La quantitat ha de ser superior a 1"),
  referenceId: Yup.string().required("La referencia és obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);
const referenceStore = useReferenceStore();

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.detail);
};

watch(
  () => props.detail.quantity,
  async (newValue) => {
    if (newValue) {
      calculateAmount();
    }
  },
);

const getReferenceInfo = async (id: string | null) => {
  if (id == null || props.order.supplierId == "") {
    return;
  }

  var supplierReference =
    await PurchaseServices.Supplier.getSupplierReferenceBySupplierIdAndReferenceId(
      props.order.supplierId,
      id,
    );
  if (supplierReference) {
    props.detail.unitPrice = supplierReference.supplierPrice;
    props.detail.expectedReceiptDate = addDays(supplierReference.supplyDays);
    props.detail.description = supplierReference.supplierDescription;
  } else {
    const reference = await SharedServices.Reference.getById(id);
    if (reference) {
      props.detail.unitPrice = reference.price;
      props.detail.description = reference.description;
    }
  }

  calculateAmount();
};

function addDays(days: number) {
  let currentDate = new Date(); // Get the current date
  currentDate.setDate(currentDate.getDate() + days); // Add the specified number of days
  return currentDate;
}

const calculateAmount = () => {
  const reference = referenceStore.references!.find(
    (r) => r.id == props.detail.referenceId,
  );
  if (reference) {
    if (reference.categoryName == "Service") {
      props.detail.amount = props.detail.unitPrice;
    } else {
      props.detail.amount = props.detail.quantity * props.detail.unitPrice;
    }
  }
};

const submitForm = async () => {
  validate();
  if (validation.value.result) {
    emit("submit", props.detail);
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
