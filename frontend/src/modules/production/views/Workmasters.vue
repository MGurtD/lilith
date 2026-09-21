<template>
  <Table
    :items="filteredData"
    :columns="columns"
    :filter-config="[]"
    v-model:filter-values="filter"
    :filter-labels="filterLabels"
    :filter-value-resolvers="filterValueResolvers"
    :filter-body-width="filterBodyWidth"
    :show-filter-action="false"
    page="Workmasters"
    preset="crud-list"
    tableStyle="min-width: 100%"
    sort-field="reference.code"
    :sort-order="1"
    show-delete-column
    @clear="cleanFilter"
    @create="createButtonClick"
    @delete="deleteButton"
    @row-click="editRow"
  >
    <template #prepend>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">{{
          pt("Client")
        }}</label>
        <DropdownCustomers label="" v-model="filter.customerId" />
      </div>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">{{
          pt("Referència")
        }}</label>
        <DropdownReference
          label=""
          v-model="filter.referenceId"
          :customer-id="filter.customerId"
          :fullName="true"
        />
      </div>
      <div class="table-filter-prepend-field table-filter-prepend-field--md">
        <label class="filter-label table-filter-prepend-label">{{
          pt("lastUpdated")
        }}</label>
        <DatePicker
          v-model="filter.dates"
          selectionMode="range"
          dateFormat="dd/mm/yy"
          :showIcon="true"
          class="w-full"
          :placeholder="pt('Selecciona periode')"
          size="small"
        />
      </div>
    </template>
    <template #body-copyAction="{ data }">
      <i
        :class="PrimeIcons.COPY"
        class="grid_copy_column_button"
        @click.stop="copyButton(data)"
      />
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
  >
    <div>
      <DropdownReference
        :label="pt('Referència')"
        v-model="workmasterStore.workmaster!.referenceId"
        class="w-full"
        :fullName="true"
      ></DropdownReference>
    </div>
    <br />
    <div>
      <Button
        :label="pt('Crear')"
        style="float: right"
        @click="onCreateSubmit"
      ></Button>
    </div>
  </Dialog>
  <Dialog
    v-model:visible="copyDialogVisible"
    :header="pt('Copiar ruta de fabricació')"
    :closable="!copyLoading"
    :modal="true"
    :style="{ width: '50vw', maxWidth: '700px' }"
  >
    <div v-if="copyModel" class="flex flex-column gap-3">
      <div class="flex flex-column gap-1">
        <label class="font-semibold text-sm text-color-secondary">{{
          pt("Ruta d'origen")
        }}</label>
        <span class="text-lg">{{ copySourceName }}</span>
      </div>

      <hr
        class="my-2"
        style="border: none; border-top: 1px solid var(--p-surface-200)"
      />

      <div class="flex flex-column gap-2">
        <label class="font-semibold text-sm text-color-secondary">{{
          pt("Destí de la còpia")
        }}</label>

        <div class="flex align-items-center gap-2">
          <RadioButton
            v-model="copyDestinyMode"
            inputId="destExisting"
            value="existing"
          />
          <label for="destExisting">{{ pt("Referència existent") }}</label>
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
          <label for="destNew">{{ pt("Crear nova referència") }}</label>
        </div>
        <div
          v-if="copyDestinyMode === 'new'"
          class="ml-4 flex flex-column gap-2"
        >
          <BaseInput
            id="referenceCode"
            :label="pt('Codi')"
            v-model="copyModel.referenceCode"
          />
          <BaseInput
            id="referenceDescription"
            :label="pt('Descripció')"
            v-model="copyModel.referenceDescription"
          />
        </div>
      </div>

      <div class="flex flex-column gap-1">
        <label class="font-semibold text-sm text-color-secondary mb-1">{{
          pt("Mode de fabricació")
        }}</label>
        <Select
          v-model="copyModel.mode"
          :options="workmasterStore.workmasterModes"
          optionLabel="value"
          optionValue="id"
          :placeholder="pt('Selecciona el mode')"
          class="w-full"
        />
      </div>
    </div>

    <template #footer>
      <div class="flex justify-content-end gap-2">
        <Button
          :label="pt('Cancel·lar')"
          severity="secondary"
          text
          :disabled="copyLoading"
          @click="copyDialogVisible = false"
        />
        <Button
          :label="pt('Copiar')"
          icon="pi pi-copy"
          :loading="copyLoading"
          @click="onCopySubmit"
        />
      </div>
    </template>
  </Dialog>
</template>
<script setup lang="ts">
import Table from "@/components/tables/Table.vue";
import { ColumnType, type Column } from "@/components/tables/types";
import type { FilterBodyWidth } from "@/components/tables/TableFilter.vue";
import { useI18n } from "vue-i18n";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import { onBeforeRouteLeave, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { computed, onMounted, reactive, ref } from "vue";
import { PrimeIcons } from "@primevue/core/api";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useWorkMasterStore } from "../store/workmaster";
import { useReferenceStore } from "../../shared/store/reference";
import { useCustomersStore } from "../../sales/store/customers";
import { WorkMaster, WorkMasterToCopy } from "../types";
import { getNewUuid } from "../../../utils/functions";
import { DialogOptions } from "../../../types/component";
import { useUserFilterStore } from "../../../store/userfilter";

const { t } = useI18n();
const pt = (key: string): string => t(`production.ui.${key}`);
const router = useRouter();
const store = useStore();
const userFilterStore = useUserFilterStore();
const toast = useToast();
const confirm = useConfirm();
const workmasterStore = useWorkMasterStore();
const referenceStore = useReferenceStore();
const customersStore = useCustomersStore();

const filterBodyWidth: FilterBodyWidth = { desktop: "66%", tablet: "100%" };

const filterLabels = computed<Record<string, string>>(() => ({
  customerId: pt("Client"),
  referenceId: pt("Referència"),
  dates: pt("lastUpdated"),
}));

const filterValueResolvers: Record<string, (value: unknown) => string> = {
  customerId: (value) =>
    typeof value === "string"
      ? (customersStore.getCustomerNameById(value) ?? "")
      : "",
  referenceId: (value) =>
    typeof value === "string"
      ? (referenceStore.getFullNameById(value) ?? "")
      : "",
};

const columns = computed<Column[]>(() => [
  {
    field: "reference.code",
    header: pt("Referencia"),
    sortable: true,
    resolver: (_value, data) => {
      const workmaster = data as WorkMaster;
      return workmaster.reference
        ? referenceStore.getFullName(workmaster.reference)
        : "";
    },
    style: "width: 40%",
  },
  {
    field: "reference.customerId",
    header: pt("Client"),
    sortable: true,
    resolver: (value) =>
      typeof value === "string"
        ? customersStore.getCustomerNameById(value)
        : "",
    style: "width: 20%",
  },
  {
    field: "updatedOn",
    header: pt("Actualitzada"),
    sortable: true,
    columnType: ColumnType.Date,
    style: "width: 10%",
  },
  {
    field: "mode",
    header: pt("Mode"),
    resolver: (value) =>
      typeof value === "number" ? (returnMode(value) ?? "") : "",
    style: "width: 12.5%",
  },
  {
    field: "baseQuantity",
    header: pt("Quantitat Base"),
    columnType: ColumnType.Number,
    style: "width: 10%",
  },
  {
    field: "totalCost",
    header: pt("Cost"),
    columnType: ColumnType.Currency,
    resolver: (_value, data) => {
      const workmaster = data as WorkMaster;
      return (
        workmaster.machineCost +
        workmaster.operatorCost +
        workmaster.materialCost +
        workmaster.externalCost
      );
    },
    style: "width: 10%",
  },
  {
    field: "disabled",
    header: pt("Desactivada"),
    columnType: ColumnType.Boolean,
    style: "width: 5%",
  },
  {
    field: "copyAction",
    header: "",
    style: "width: 3%",
    truncate: false,
  },
]);

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
        w.reference?.customerId === null,
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
  title: pt("Crear ruta"),
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
    title: pt("Gestió de rutes de fabricació"),
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
onBeforeRouteLeave(async () => {
  await userFilterStore.addFilter("Workmasters", "", filter.value);
});

const createButtonClick = () => {
  const newId = getNewUuid();
  workmasterStore.setNew(newId);

  dialogOptions.visible = true;
};

const copyButton = (workmaster: WorkMaster) => {
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
      summary: pt("Selecciona una referència de destí"),
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
      summary: pt("Introdueix el codi de la nova referència"),
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
        summary: pt("Ruta copiada correctament"),
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
  router.push({ path: `/workmaster/${row.data.id}` });
};

const onCreateSubmit = async () => {
  if (!workmasterStore.workmaster) return;

  const created = await workmasterStore.create(workmasterStore.workmaster);
  if (created)
    router.push({ path: `/workmaster/${workmasterStore.workmaster.id}` });
};

const deleteButton = (workmaster: WorkMaster) => {
  confirm.require({
    message: t("production.messages.confirmDeleteWorkmaster", {
      name: workmaster.reference!.description,
    }),
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const deleted = await workmasterStore.delete(workmaster.id);

      if (deleted) {
        toast.add({
          severity: "success",
          summary: pt("Eliminada"),
          life: 3000,
        });
        await workmasterStore.fetchAll();
      }
    },
  });
};
</script>
