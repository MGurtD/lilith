<template>
  <DataTable
    class="p-datatable-sm"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    :value="groupedDetails"
    rowGroupMode="subheader"
    groupRowsBy="deliveryNoteNumber"
  >
    <template #header>
      <slot name="header"></slot>
    </template>
    <template #groupheader="slotProps">
      <i
        v-if="canDelete && slotProps.data.deliveryNoteNumber !== 'Sense albarà'"
        :class="PrimeIcons.TIMES"
        class="grid_delete_column_button"
        @click="onGroupDeleteRow($event, slotProps.data)"
      />
      &nbsp;
      <span class="vertical-align-middle ml-2 font-bold line-height-3">{{
        slotProps.data.deliveryNoteNumber
      }}</span>
    </template>
    <Column header="Albarà" field="deliveryNoteNumber" style="width: 5%" />
    <Column header="" field="" style="width: 2%" />

    <Column header="Quantitat" field="quantity" style="width: 10%"></Column>
    <Column header="Referència" field="reference.code" style="width: 15%">
      <template #body="slotProps">
        <span v-if="slotProps.data.deliveryNoteDetail">
          <LinkReference :id="slotProps.data.deliveryNoteDetail.referenceId" />
        </span>
        <span v-else>--</span>
      </template>
    </Column>
    <Column header="Descripció" field="description" style="width: 40%"></Column>
    <Column header="Preu unitat" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.unitPrice) }}
      </template>
    </Column>
    <Column header="Impost" style="width: 10%">
      <template #body="slotProps">
        {{ getTaxNameById(slotProps.data.taxId) }}
      </template>
    </Column>
    <Column header="Total" field="totalCost" style="width: 10%">
      <template #body="slotProps">
        {{ formatCurrency(slotProps.data.amount) }}
      </template>
    </Column>
    <Column style="width: 10%">
      <template #body="slotProps">
        <i
          v-if="!slotProps.data.deliveryNoteDetailId && canDelete"
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="onDeleteRow($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
<script setup lang="ts">
import LinkReference from "../../shared/components/LinkReference.vue";
import { computed, onMounted } from "vue";
import { DeliveryNote, SalesInvoiceDetail } from "../types";
import { PrimeIcons } from "@primevue/core/api";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useReferenceStore } from "../../shared/store/reference";
import { formatDate, formatCurrency } from "../../../utils/functions";
import { useConfirm } from "primevue/useconfirm";
import _ from "lodash";
import { Reference } from "../../shared/types";
import { useTaxesStore } from "../../shared/store/tax";

const confirm = useConfirm();
const lifecycleStore = useLifecyclesStore();
const taxesStore = useTaxesStore();
const referenceStore = useReferenceStore();

const props = defineProps<{
  details: Array<SalesInvoiceDetail> | undefined;
  deliveryNotes: Array<DeliveryNote> | undefined;
  canDelete: boolean;
}>();

const groupedDetails = computed(() => {
  let grDetails = [] as any;
  if (!props.details) return grDetails;

  const freeDetails = props.details
    .filter((d) => !d.deliveryNoteDetailId)
    .map((d) => {
      return {
        deliveryNoteId: "",
        deliveryNoteNumber: "Sense albarà",
        deliveryNoteDate: "",
        ...d,
      };
    });
  if (freeDetails.length > 0) grDetails.push(...freeDetails);

  const deliveryNoteDetails = props.details
    .filter((d) => d.deliveryNoteDetailId)
    .map((d) => {
      const deliveryNote = props.deliveryNotes?.find(
        (dn) => dn.id === d.deliveryNoteDetail?.deliveryNoteId,
      );

      const deliveredDate =
        deliveryNote && deliveryNote.deliveryDate
          ? `Entregat ${formatDate(deliveryNote.deliveryDate)}`
          : "No entregat";

      if (referenceStore.references) {
        const reference = referenceStore.references!.find(
          (r: Reference) => r.id === d.deliveryNoteDetail?.referenceId,
        );
        if (reference) d.reference = reference;
      }

      return {
        deliveryNoteId: deliveryNote?.id,
        deliveryNoteNumber: `Albarà ${
          deliveryNote ? `${deliveryNote.number} - ${deliveredDate}` : "--"
        }`,
        deliveryNoteDate:
          deliveryNote && deliveryNote.deliveryDate
            ? ` - ${formatDate(deliveryNote.deliveryDate)}`
            : "",
        ...d,
      };
    });
  if (deliveryNoteDetails.length > 0) grDetails.push(...deliveryNoteDetails);

  return _.orderBy(grDetails, (d) => [d.deliveryNoteNumber, d.description]);
});

onMounted(async () => {
  await lifecycleStore.fetchOneByName("SalesInvoice");
});
const getTaxNameById = (taxId: string) => {
  const tax = taxesStore.taxes?.find((t) => t.id === taxId);
  if (tax) return `${tax.name}`;
};

const emit = defineEmits<{
  (e: "delete", detail: SalesInvoiceDetail): void;
  (e: "deleteDeliveryNote", deliveryNote: DeliveryNote): void;
}>();

const onDeleteRow = (event: any, SalesInvoiceDetail: any) => {
  confirm.require({
    message: `Està segur que vol eliminar la línea ${SalesInvoiceDetail.description}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      emit("delete", SalesInvoiceDetail);
    },
  });
};

const onGroupDeleteRow = (event: any, clickedDeliveryNote: any) => {
  if (!props.deliveryNotes) return;
  const deliveryNote = props.deliveryNotes.find(
    (o) => o.id === clickedDeliveryNote.deliveryNoteId,
  );
  if (!deliveryNote) return;

  confirm.require({
    message: `Està segur que vol treure l'${clickedDeliveryNote.deliveryNoteNumber}?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      emit("deleteDeliveryNote", deliveryNote);
    },
  });
};
</script>
