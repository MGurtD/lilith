<template>
  <form v-if="detail">
    <section class="three-columns-7525">
      <div>
        <DropdownReference
          label="Referència de compra"
          v-model="detail.referenceId"
          :fullName="true"
          @update:modelValue="updateHandler"
        ></DropdownReference>
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.TEXT"
          label="Format"
          v-model="format"
          disabled
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Preu / Kilo"
          highlightOnFocus
          v-model="detail.kilogramPrice"
          :disabled="isDisabled('kilogramprice')"
          @update:modelValue="calculate"
        />
      </div>
    </section>
    <section>
      <div>
        <BaseInput
          :type="BaseInputType.TEXT"
          label="Descripció"
          highlightOnFocus
          v-model="detail.description"
        />
      </div>
    </section>

    <section class="three-columns mt-2">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          highlightOnFocus
          label="Amplada (mm)"
          :decimals="2"
          v-model="detail.width"
          :disabled="isDisabled('width')"
          @update:modelValue="calculate"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Alçada (mm)"
          highlightOnFocus
          v-model="detail.height"
          :disabled="isDisabled('height')"
          @update:modelValue="calculate"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Longitud (mm)"
          highlightOnFocus
          v-model="detail.lenght"
          :disabled="isDisabled('length')"
          @update:modelValue="calculate"
        />
      </div>
    </section>

    <section class="three-columns mt-2">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Diàmetre (mm)"
          highlightOnFocus
          v-model="detail.diameter"
          :disabled="isDisabled('diameter')"
          @update:modelValue="calculate"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Gruix (mm)"
          highlightOnFocus
          v-model="detail.thickness"
          :disabled="isDisabled('thickness')"
          @update:modelValue="calculate"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          :decimals="2"
          label="Pes (kg)"
          v-model="detail.totalWeight"
          :disabled="isDisabled('totalweight')"
        />
      </div>
    </section>

    <section class="three-columns mt-2">
      <div>
        <BaseInput
          :type="BaseInputType.NUMERIC"
          label="Quantitat"
          highlightOnFocus
          v-model="detail.quantity"
          @update:modelValue="calculate"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Preu unitari"
          highlightOnFocus
          v-model="detail.unitPrice"
          @update:modelValue="calculate"
        />
      </div>
      <div>
        <BaseInput
          :type="BaseInputType.CURRENCY"
          label="Preu"
          v-model="detail.amount"
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
    <!--<Button
      label="Calcular"
      @click="calculate"
      style="float: right"
      :size="'small'"
      class="mt-2 mr-2"
    />-->
  </form>
</template>

<script setup lang="ts">
import DropdownReference from "../../shared/components/DropdownReference.vue";
import { onMounted, ref } from "vue";
import { Receipt, ReceiptDetail } from "../types";
import * as Yup from "yup";
import {
  FormValidation,
  FormValidationResult,
} from "../../../utils/form-validator";
import { useToast } from "primevue/usetoast";
import BaseInput from "../../../components/BaseInput.vue";
import { BaseInputType } from "../../../types/component";
import { useReceiptsStore } from "../store/receipt";
import { useReferenceStore } from "../../shared/store/reference";
import { ReferenceService } from "../../shared/services/reference.service";
import PurchaseServices from "../services";
import { update } from "lodash";

const props = defineProps<{
  detail: ReceiptDetail;
  receipt: Receipt;
}>();

const emit = defineEmits<{
  (e: "submit", detail: ReceiptDetail): void;
  (e: "cancel"): void;
}>();

const toast = useToast();
const receiptStore = useReceiptsStore();
const referenceStore = useReferenceStore();
const referenceService = new ReferenceService("/reference");
const format = ref<string>("");

const getPrice = async (id: string | null) => {
  if (id == null || props.receipt.supplierId == "") {
    return;
  }
  var supplierReference =
    await PurchaseServices.Supplier.getSupplierReferenceBySupplierIdAndReferenceId(
      props.receipt.supplierId,
      id
    );
  if (supplierReference && format.value != "UNITATS") {
    props.detail.kilogramPrice = supplierReference.supplierPrice;
  } else if (supplierReference && format.value == "UNITATS") {
    props.detail.unitPrice = supplierReference.supplierPrice;
  } else if (!supplierReference && format.value != "UNITATS") {
    if (referenceStore.reference)
      props.detail.kilogramPrice = referenceStore.reference?.price;
  } else {
    if (referenceStore.reference)
      props.detail.unitPrice = referenceStore.reference?.price;
  }
};

const getFormat = async (id: string) => {
  await referenceStore.fetchReference(props.detail.referenceId);

  if (
    referenceStore.reference &&
    referenceStore.reference.referenceFormatId != null
  ) {
    const referenceFormat = await referenceService.getReferenceFormatById(
      referenceStore.reference.referenceFormatId
    );
    if (referenceFormat) {
      format.value = referenceFormat.code;
    }
  }
};
onMounted(async () => {
  await getFormat(props.detail.referenceId);
});
const getReferenceInfo = async (id: string) => {
  if (props.detail.description == "" || props.detail.description == null) {
    const reference = await referenceStore.getFullNameById(id);
    props.detail.description = reference;
  }
};

const updateHandler = async (id: string) => {
  if (id == null || props.receipt.supplierId == "") {
    return;
  }
  await restartInputs();
  await getFormat(id);
  await getPrice(id);
  await getReferenceInfo(id);
};

const restartInputs = async () => {
  props.detail.thickness = 0;
  props.detail.lenght = 0;
  props.detail.diameter = 0;
  props.detail.height = 0;
  props.detail.kilogramPrice = 0;
  props.detail.quantity = 0;
  props.detail.totalWeight = 0;
  props.detail.width = 0;
  props.detail.unitPrice = 0;
  props.detail.unitWeight = 0;
  props.detail.amount = 0;
};

const isDisabled = (field: string) => {
  if (format.value === "UNITATS") {
    return (
      field === "thickness" ||
      field === "width" ||
      field === "height" ||
      field === "kilogramprice" ||
      field === "length" ||
      field === "diameter" ||
      field === "thickness" ||
      field === "totalweight"
    );
  } else if (format.value === "RODO") {
    return (
      field === "thickness" ||
      field === "width" ||
      field === "height" ||
      field === "totalweight"
    );
  } else if (format.value === "TUB") {
    return field === "width" || field === "height" || field === "totalweight";
  } else if (format.value === "PLACA") {
    return (
      field === "thickness" || field === "diameter" || field === "totalweight"
    );
  }
  return false;
};

const calculate = async () => {
  if (
    format.value == "RODO" &&
    props.detail.diameter != 0 &&
    props.detail.lenght != 0
  ) {
    const response = await receiptStore.calculateDetailWeightAndPrice(
      props.detail
    );
    if (response.result) {
      props.detail.unitWeight = response.content!.unitWeight;
      props.detail.totalWeight = response.content!.totalWeight;
      props.detail.unitPrice = response.content!.unitPrice;
      props.detail.amount = response.content!.amount;
    } else {
      toast.add({
        summary: "Calculadora de pes/preu",
        detail: response.errors[0],
        severity: "warn",
        life: 6000,
      });
    }
  } else if (
    format.value == "TUB" &&
    props.detail.diameter != 0 &&
    props.detail.lenght != 0 &&
    props.detail.thickness != 0
  ) {
    const response = await receiptStore.calculateDetailWeightAndPrice(
      props.detail
    );
    if (response.result) {
      props.detail.unitWeight = response.content!.unitWeight;
      props.detail.totalWeight = response.content!.totalWeight;
      props.detail.unitPrice = response.content!.unitPrice;
      props.detail.amount = response.content!.amount;
    } else {
      toast.add({
        summary: "Calculadora de pes/preu",
        detail: response.errors[0],
        severity: "warn",
        life: 6000,
      });
    }
  } else if (
    format.value == "PLACA" &&
    props.detail.lenght != 0 &&
    props.detail.width != 0 &&
    props.detail.height != 0
  ) {
    const response = await receiptStore.calculateDetailWeightAndPrice(
      props.detail
    );
    if (response.result) {
      props.detail.unitWeight = response.content!.unitWeight;
      props.detail.totalWeight = response.content!.totalWeight;
      props.detail.unitPrice = response.content!.unitPrice;
      props.detail.amount = response.content!.amount;
    } else {
      toast.add({
        summary: "Calculadora de pes/preu",
        detail: response.errors[0],
        severity: "warn",
        life: 6000,
      });
    }
  } else if (format.value == "UNITATS") {
    //props.detail.unitPrice = referenceStore.reference.price;
    props.detail.amount = props.detail.unitPrice * props.detail.quantity;
  }
};

const schema = Yup.object().shape({
  quantity: Yup.number().min(1).required("La quantitat ha de ser superior a 1"),
  referenceId: Yup.string().required("La referencia és obligatoria"),
});
const validation = ref({
  result: false,
  errors: {},
} as FormValidationResult);

const validate = () => {
  const formValidation = new FormValidation(schema);
  validation.value = formValidation.validate(props.detail);
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
