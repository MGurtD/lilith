<template>
  <DataTable
    :value="filteredData"
    tableStyle="min-width: 100%"
    scrollable
    scrollHeight="flex"
    sort-field="reference.code"
    :sort-order="1"
    @row-click="editRow"
    paginator
    :rows="20"
  >
    <template #header>
      <div
        class="flex flex-wrap align-items-center justify-content-between gap-2"
      >
        <div class="datatable-filter">
          <div class="filter-field">
            <label class="block text-900">Client</label>
            <DropdownCustomers
              label=""
              v-model="filter.customerId"
            ></DropdownCustomers>
          </div>
          <div class="filter-field">
            <label class="block text-900">Referència</label>
            <DropdownReference
              label=""
              v-model="filter.referenceId"
              :customer-id="filter.customerId"
              :fullName="true"
            ></DropdownReference>
          </div>
        </div>
        <div class="datatable-buttons">
          <Button
            class="datatable-button mr-2"
            :icon="PrimeIcons.FILTER_SLASH"
            rounded
            raised
            @click="cleanFilter"
          />
          <Button
            :icon="PrimeIcons.PLUS"
            rounded
            raised
            @click="createButtonClick"
          />
        </div>
      </div>
    </template>
    <Column
      field="reference.code"
      sortable
      header="Referencia"
      style="width: 40%"
    >
      <template #body="slotProps">
        {{ referenceStore.getFullName(slotProps.data.reference) }}
      </template>
    </Column>
    <Column sortable header="Client" style="width: 20%">
      <template #body="slotProps">
        {{
          customersStore.getCustomerNameById(
            slotProps.data.reference.customerId,
          )
        }}
      </template>
    </Column>
    <Column
      field="baseQuantity"
      header="Quantitat Base"
      style="width: 10%"
    ></Column>
    <Column header="Cost" style="width: 10%">
      <template #body="slotProps">
        {{
          formatCurrency(
            slotProps.data.machineCost +
              slotProps.data.operatorCost +
              slotProps.data.materialCost +
              slotProps.data.externalCost,
          )
        }}
      </template>
    </Column>
    <Column header="Mode" sytle="width: 30%">
      <template #body="slotProps">
        {{ returnMode(slotProps.data.mode) }}
      </template>
    </Column>
    <Column header="Desactivada" style="width: 10%">
      <template #body="slotProps">
        <BooleanColumn :value="slotProps.data.disabled" />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.COPY"
          class="grid_copy_column_button"
          @click="copyButton($event, slotProps.data)"
        />
      </template>
    </Column>
    <Column>
      <template #body="slotProps">
        <i
          :class="PrimeIcons.TIMES"
          class="grid_delete_column_button"
          @click="deleteButton($event, slotProps.data)"
        />
      </template>
    </Column>
  </DataTable>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <div>
      <DropdownReference
        label="Referència"
        v-model="workmasterStore.workmaster!.referenceId"
        class="w-full"
        :fullName="true"
      ></DropdownReference>
    </div>
    <br />
    <div>
      <Button
        label="Crear"
        style="float: right"
        @click="onCreateSubmit"
      ></Button>
    </div>
  </Dialog>
  <Dialog
    v-model:visible="copyDialogOptions.visible"
    :header="copyDialogOptions.title"
    :closable="true"
    :modal="true"
    :style="{ width: '60vw' }"
  >
    <section class="two-columns">
      <div><h3>Origen</h3></div>
      <div><h3>Destí</h3></div>
    </section>
    <section class="two-columns">
      <div>
        <p>
          {{
            referenceStore.getFullNameById(
              workmasterStore.workmasterToCopy!.workmaster.referenceId,
            )
          }}
        </p>
      </div>
      <div>
        <DropdownReference
          label="Referència existent:"
          v-model="workmasterStore.workmasterToCopy!.referenceId"
          :fullName="true"
          @change="checkDestinyCode"
        ></DropdownReference>
        <label class="block text-900 mb-2">Mode</label>
        <Select
          v-model="workmasterStore.workmasterToCopy!.mode"
          :options="workmasterStore.workmasterModes"
          optionLabel="value"
          optionValue="id"
          placeholder="Seleccione el mode"
          class="w-full mb-2"
        />

        <BaseInput
          class="mb-2"
          label="Nou codi:"
          id="referenceCode"
          v-model="workmasterStore.workmasterToCopy!.referenceCode"
          @change="checkDestinyId"
        ></BaseInput>
      </div>
    </section>
    <div>
      <Button
        label="Copiar"
        style="float: right"
        @click="onCopySubmit"
      ></Button>
    </div>
  </Dialog>
</template>
<script setup lang="ts">
import DropdownReference from "../../shared/components/DropdownReference.vue";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import { useRouter } from "vue-router";
import { useStore } from "../../../store";
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useWorkMasterStore } from "../store/workmaster";
import { useReferenceStore } from "../../shared/store/reference";
import { useCustomersStore } from "../../sales/store/customers";
import { WorkMaster } from "../types";
import { getNewUuid, formatCurrency } from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { useUserFilterStore } from "../../../store/userfilter";

const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();
const workmasterStore = useWorkMasterStore();
const referenceStore = useReferenceStore();
const customersStore = useCustomersStore();

const filter = ref({
  referenceId: undefined,
  customerId: undefined,
});

const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.customerId = undefined;

  userFilterStore.removeFilter("Workmasters", "");
};

const filteredData = computed(() => {
  if (!workmasterStore.workmasters) return [];

  let filteredWorkmasters = workmasterStore.workmasters;

  if (filter.value.referenceId)
    filteredWorkmasters = filteredWorkmasters.filter(
      (w) => w.referenceId === filter.value.referenceId,
    );

  if (filter.value.customerId)
    filteredWorkmasters = filteredWorkmasters.filter(
      (w) =>
        w.reference?.customerId === filter.value.customerId ||
        w.reference.customerId === null,
    );

  return filteredWorkmasters;
});

const dialogOptions = reactive({
  visible: false,
  title: "Crear ruta",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

const returnMode = (mode: number) => {
  return workmasterStore.workmasterModes.find((m) => m.id === mode)?.value;
};

const copyDialogOptions = reactive({
  visible: false,
  title: "Copiar ruta",
  closable: true,
  position: "center",
  modal: true,
} as DialogOptions);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: "Gestió de rutes de fabricació",
  });

  referenceStore.fetchReferencesByModule("sales");
  await workmasterStore.fetchAll();

  const userFilter = userFilterStore.getFilter("Workmasters", "");
  if (userFilter) {
    if (userFilter.referenceId)
      filter.value.referenceId = userFilter.referenceId;
    if (userFilter.customerId) filter.value.customerId = userFilter.customerId;
  }
});
onUnmounted(() => {
  userFilterStore.addFilter("Workmasters", "", filter.value);
});

const createButtonClick = () => {
  const newId = getNewUuid();
  workmasterStore.setNew(newId);

  dialogOptions.visible = true;
};

const copyButton = (event: any, workmaster: WorkMaster) => {
  workmasterStore.workmasterToCopy = {
    workmasterId: workmaster.id,
    workmaster,
    referenceId: null,
    referenceCode: "",
    mode: 1,
  };
  copyDialogOptions.visible = true;
};

const onCopySubmit = async () => {
  if (workmasterStore.workmasterToCopy!.referenceId === "") {
    workmasterStore.workmasterToCopy!.referenceId = null;
  }
  const copied = await workmasterStore.copy(workmasterStore.workmasterToCopy!);
  await workmasterStore.fetchAll();
  copyDialogOptions.visible = false;
  if (copied.result) {
    toast.add({
      severity: "success",
      summary: "Copiada",
      life: 3000,
    });
  } else {
    toast.add({
      severity: copied.errors.length > 0 ? "warn" : "error",
      summary:
        copied.errors.length > 0
          ? copied.errors[0]
          : "Hi ha hagut un error en el proces",
      life: 6000,
    });
  }
};

const checkDestinyCode = () => {
  workmasterStore.workmasterToCopy!.referenceCode = "";
};

const checkDestinyId = () => {
  workmasterStore.workmasterToCopy!.referenceId = "";
};

const editRow = (row: DataTableRowClickEvent) => {
  if (
    !(row.originalEvent.target as any).className.includes(
      "grid_delete_column_button",
    ) &&
    !(row.originalEvent.target as any).className.includes(
      "grid_copy_column_button",
    )
  ) {
    router.push({ path: `/workmaster/${row.data.id}` });
  }
};

const onCreateSubmit = async () => {
  if (!workmasterStore.workmaster) return;

  const created = await workmasterStore.create(workmasterStore.workmaster);
  if (created)
    router.push({ path: `/workmaster/${workmasterStore.workmaster.id}` });
};

const deleteButton = (event: any, workmaster: WorkMaster) => {
  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la ruta ${
      workmaster.reference!.description
    }?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await workmasterStore.delete(workmaster.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: "Eliminada",
          life: 3000,
        });
        await workmasterStore.fetchAll();
      }
    },
  });
};
</script>
