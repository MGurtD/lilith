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
      <TableFilter
        :config="[]"
        v-model="filter"
        :show-title="false"
        :show-action-labels="false"
        :show-filter-action="false"
        :body-width="filterBodyWidth"
        embedded
        @clear="cleanFilter"
        @create="createButtonClick"
      >
        <template #prepend>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Client</label
            >
            <DropdownCustomers label="" v-model="filter.customerId" />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Referència</label
            >
            <DropdownReference
              label=""
              v-model="filter.referenceId"
              :customer-id="filter.customerId"
              :fullName="true"
            />
          </div>
          <div
            class="table-filter-prepend-field table-filter-prepend-field--md"
          >
            <label class="filter-label table-filter-prepend-label"
              >Última actual.</label
            >
            <DatePicker
              v-model="filter.dates"
              selectionMode="range"
              dateFormat="dd/mm/yy"
              :showIcon="true"
              class="w-full"
              placeholder="Selecciona periode"
              size="small"
            />
          </div>
        </template>
      </TableFilter>
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
    <Column field="updatedOn" header="Actualitzada" sortable style="width: 10%">
      <template #body="slotProps">
        {{ formatDate(slotProps.data.updatedOn) }}
      </template>
    </Column>
    <Column header="Mode" style="width: 12.5%">
      <template #body="slotProps">
        {{ returnMode(slotProps.data.mode) }}
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
    <Column header="Desactivada" style="width: 5%">
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
    v-model:visible="copyDialogVisible"
    header="Copiar ruta de fabricació"
    :closable="!copyLoading"
    :modal="true"
    :style="{ width: '50vw', maxWidth: '700px' }"
  >
    <div v-if="copyModel" class="flex flex-column gap-3">
      <div class="flex flex-column gap-1">
        <label class="font-semibold text-sm text-color-secondary"
          >Ruta d'origen</label
        >
        <span class="text-lg">{{ copySourceName }}</span>
      </div>

      <hr
        class="my-2"
        style="border: none; border-top: 1px solid var(--p-surface-200)"
      />

      <div class="flex flex-column gap-2">
        <label class="font-semibold text-sm text-color-secondary"
          >Destí de la còpia</label
        >

        <div class="flex align-items-center gap-2">
          <RadioButton
            v-model="copyDestinyMode"
            inputId="destExisting"
            value="existing"
          />
          <label for="destExisting">Referència existent</label>
        </div>
        <div v-if="copyDestinyMode === 'existing'" class="ml-4">
          <DropdownReference
            label=""
            v-model="copyModel.referenceId"
            :fullName="true"
          />
        </div>

        <div class="flex align-items-center gap-2 mt-2">
          <RadioButton
            v-model="copyDestinyMode"
            inputId="destNew"
            value="new"
          />
          <label for="destNew">Crear nova referència</label>
        </div>
        <div
          v-if="copyDestinyMode === 'new'"
          class="ml-4 flex flex-column gap-2"
        >
          <BaseInput
            id="referenceCode"
            label="Codi"
            v-model="copyModel.referenceCode"
          />
          <BaseInput
            id="referenceDescription"
            label="Descripció"
            v-model="copyModel.referenceDescription"
          />
        </div>
      </div>

      <div class="flex flex-column gap-1">
        <label class="font-semibold text-sm text-color-secondary mb-1"
          >Mode de fabricació</label
        >
        <Select
          v-model="copyModel.mode"
          :options="workmasterStore.workmasterModes"
          optionLabel="value"
          optionValue="id"
          placeholder="Selecciona el mode"
          class="w-full"
        />
      </div>
    </div>

    <template #footer>
      <div class="flex justify-content-end gap-2">
        <Button
          label="Cancel·lar"
          severity="secondary"
          text
          :disabled="copyLoading"
          @click="copyDialogVisible = false"
        />
        <Button
          label="Copiar"
          icon="pi pi-copy"
          :loading="copyLoading"
          @click="onCopySubmit"
        />
      </div>
    </template>
  </Dialog>
</template>
<script setup lang="ts">
import TableFilter from "../../../components/tables/TableFilter.vue";
import type { FilterBodyWidth } from "../../../components/tables/TableFilter.vue";
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
import { WorkMaster, WorkMasterToCopy } from "../types";
import {
  getNewUuid,
  formatCurrency,
  formatDate,
} from "../../../utils/functions";
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

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const filter = ref({
  referenceId: undefined,
  customerId: undefined,
  dates: undefined as Array<Date> | undefined,
});

const cleanFilter = () => {
  filter.value.referenceId = undefined;
  filter.value.customerId = undefined;
  filter.value.dates = undefined;

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

  if (filter.value.dates && filter.value.dates.length > 0) {
    const startDate = filter.value.dates[0];
    if (startDate) {
      filteredWorkmasters = filteredWorkmasters.filter(
        (w) => new Date(w.updatedOn!) >= startDate,
      );
    }
    if (filter.value.dates.length > 1 && filter.value.dates[1]) {
      const endDate = new Date(filter.value.dates[1]);
      endDate.setHours(23, 59, 59, 999);
      filteredWorkmasters = filteredWorkmasters.filter(
        (w) => new Date(w.updatedOn!) <= endDate,
      );
    }
  }

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

const copyDialogVisible = ref(false);
const copyLoading = ref(false);
const copyDestinyMode = ref<"existing" | "new">("existing");
const copyModel = ref<{
  referenceId: string | null;
  referenceCode: string;
  referenceDescription: string;
  mode: number;
} | null>(null);
const copySourceWorkmasterId = ref("");
const copySourceName = ref("");

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
    if (userFilter.dates) filter.value.dates = userFilter.dates;
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
  copySourceWorkmasterId.value = workmaster.id;
  copySourceName.value = referenceStore.getFullNameById(workmaster.referenceId);
  copyDestinyMode.value = "existing";
  copyModel.value = {
    referenceId: null,
    referenceCode: "",
    referenceDescription: "",
    mode: 1,
  };
  copyDialogVisible.value = true;
};

const onCopySubmit = async () => {
  if (!copyModel.value) return;

  if (copyDestinyMode.value === "existing" && !copyModel.value.referenceId) {
    toast.add({
      severity: "warn",
      summary: "Selecciona una referència de destí",
      life: 5000,
    });
    return;
  }

  if (
    copyDestinyMode.value === "new" &&
    !copyModel.value.referenceCode.trim()
  ) {
    toast.add({
      severity: "warn",
      summary: "Introdueix el codi de la nova referència",
      life: 5000,
    });
    return;
  }

  const payload: WorkMasterToCopy = {
    workmasterId: copySourceWorkmasterId.value,
    referenceId:
      copyDestinyMode.value === "existing" ? copyModel.value.referenceId : null,
    referenceCode:
      copyDestinyMode.value === "new"
        ? copyModel.value.referenceCode.trim()
        : "",
    referenceDescription:
      copyDestinyMode.value === "new"
        ? copyModel.value.referenceDescription.trim()
        : "",
    mode: copyModel.value.mode,
  };

  copyLoading.value = true;
  try {
    const copied = await workmasterStore.copy(payload);
    if (copied.result) {
      toast.add({
        severity: "success",
        summary: "Ruta copiada correctament",
        life: 3000,
      });
      copyDialogVisible.value = false;
      await workmasterStore.fetchAll();
      if (copyDestinyMode.value === "new") {
        referenceStore.fetchReferencesByModule("sales");
      }
    } else {
      toast.add({
        severity: copied.errors.length > 0 ? "warn" : "error",
        summary:
          copied.errors.length > 0
            ? copied.errors[0]
            : "Hi ha hagut un error en el procés",
        life: 6000,
      });
    }
  } finally {
    copyLoading.value = false;
  }
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
