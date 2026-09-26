<template>
  <Table
    :card-layout="cardLayout"
    :items="filteredData"
    :columns="columns"
    :filter-config="filterConfig"
    v-model:filter-values="filter"
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
    <template #filter-customerId="{ value, update }">
      <DropdownCustomers size="small"
        label=""
        :model-value="value"
        @update:model-value="update"
      />
    </template>
    <template #filter-referenceId="{ value, update }">
      <DropdownReference size="small"
        label=""
        :model-value="value"
        :customer-id="filter.customerId"
        :fullName="true"
        @update:model-value="update"
      />
    </template>
    <template #body-copyAction="{ data }">
      <i
        :class="PrimeIcons.COPY"
        class="grid_copy_column_button"
        @click.stop="copyButton(data)"
      />
    </template>
    <template #card-actions="{ data }">
      <Button
        :icon="PrimeIcons.COPY"
        text
        rounded
        :aria-label="t('production.actions.copy')"
        @click="copyButton(data)"
      />
    </template>
  </Table>

  <Dialog
    v-model:visible="dialogOptions.visible"
    :header="dialogOptions.title"
    :closable="dialogOptions.closable"
    :modal="dialogOptions.modal"
    :style="{ width: '450px' }"
  >
    <Form
      v-if="workmasterStore.workmaster"
      :rows="createRows"
      :initial-values="workmasterStore.workmaster"
      :loading="createLoading"
      @submit="onCreateSubmit"
      @cancel="dialogOptions.visible = false"
    >
      <template #field-referenceId="{ value, setValue, disabled, inputId }">
        <DropdownReference
          :input-id="inputId"
          label=""
          :model-value="typeof value === 'string' ? value : null"
          :full-name="true"
          :disabled="disabled"
          @update:model-value="setValue"
        />
      </template>
    </Form>
  </Dialog>
  <Dialog
    v-model:visible="copyDialogVisible"
    :header="pt('Copiar ruta de fabricació')"
    :closable="!copyLoading"
    :modal="true"
    :style="{ width: '50vw', maxWidth: '700px' }"
  >
    <div v-if="copyInitialValues" class="flex flex-column gap-3">
      <div class="flex flex-column gap-1">
        <label class="font-semibold text-sm text-color-secondary">{{
          t("production.detail.sourceWorkmaster")
        }}</label>
        <span class="text-lg">{{ copySourceName }}</span>
      </div>

      <hr
        class="my-2"
        style="border: none; border-top: 1px solid var(--p-surface-200)"
      />

      <Form
        :rows="copyRows"
        :initial-values="copyInitialValues"
        :loading="copyLoading"
        @submit="onCopySubmit"
        @cancel="copyDialogVisible = false"
      >
        <template
          #field-copyDestinyMode="{ value, setValue, disabled, inputId }"
        >
          <div class="flex flex-column gap-2">
            <div
              v-for="(option, index) in copyDestinyOptions"
              :key="option.value"
              class="flex align-items-center gap-2"
            >
              <RadioButton
                :input-id="index === 0 ? inputId : `${inputId}-${option.value}`"
                :name="inputId"
                :value="option.value"
                :model-value="value"
                :disabled="disabled"
                @update:model-value="setValue"
              />
              <label
                :for="index === 0 ? inputId : `${inputId}-${option.value}`"
                >{{ option.label }}</label
              >
            </div>
          </div>
        </template>

        <template
          #section-existingReference="{
            values,
            errors,
            setFieldValue,
            disabled,
          }"
        >
          <div v-if="values.copyDestinyMode === 'existing'" class="ml-4">
            <label
              class="block text-900 mb-2"
              :for="copyFieldId('referenceId')"
              >{{ pt("Referència") }}</label
            >
            <DropdownReference
              :input-id="copyFieldId('referenceId')"
              label=""
              :model-value="
                typeof values.referenceId === 'string'
                  ? values.referenceId
                  : null
              "
              :full-name="true"
              :disabled="disabled"
              :class="{ 'p-invalid': errors.referenceId }"
              @update:model-value="setFieldValue('referenceId', $event)"
            />
            <small v-if="errors.referenceId" class="p-error" role="alert">
              {{ errors.referenceId }}
            </small>
          </div>
        </template>

        <template
          #section-newReference="{ values, errors, setFieldValue, disabled }"
        >
          <div
            v-if="values.copyDestinyMode === 'new'"
            class="ml-4 flex flex-column gap-2"
          >
            <div>
              <label
                class="block text-900 mb-2"
                :for="copyFieldId('referenceCode')"
                >{{ pt("Codi") }}</label
              >
              <InputText
                :id="copyFieldId('referenceCode')"
                class="w-full"
                :class="{ 'p-invalid': errors.referenceCode }"
                :model-value="
                  typeof values.referenceCode === 'string'
                    ? values.referenceCode
                    : ''
                "
                :disabled="disabled"
                @update:model-value="setFieldValue('referenceCode', $event)"
              />
              <small v-if="errors.referenceCode" class="p-error" role="alert">
                {{ errors.referenceCode }}
              </small>
            </div>
            <div>
              <label
                class="block text-900 mb-2"
                :for="copyFieldId('referenceDescription')"
                >{{ pt("Descripció") }}</label
              >
              <InputText
                :id="copyFieldId('referenceDescription')"
                class="w-full"
                :model-value="
                  typeof values.referenceDescription === 'string'
                    ? values.referenceDescription
                    : ''
                "
                :disabled="disabled"
                @update:model-value="
                  setFieldValue('referenceDescription', $event)
                "
              />
            </div>
          </div>
        </template>
      </Form>
    </div>
  </Dialog>
</template>
<script setup lang="ts">
import Form from "@/components/forms/Form.vue";
import {
  FormFieldType,
  type FormRowConfig,
  type FormValues,
} from "@/components/forms/types";
import {
  integerValue,
  nullableStringValue,
  stringValue,
} from "@/components/forms/value-utils";
import Table from "@/components/tables/Table.vue";
import {
  ColumnType,
  type CardLayout,
  type Column,
} from "@/components/tables/types";
import type {
  FilterBodyWidth,
  FilterConfig,
} from "@/components/tables/TableFilter.vue";
import { useI18n } from "vue-i18n";
import DropdownReference from "../../shared/components/DropdownReference.vue";
import DropdownCustomers from "../../sales/components/DropdownCustomers.vue";
import { onBeforeRouteLeave, useRouter } from "vue-router";
import { useStore } from "../../../store";
import { computed, onMounted, reactive, ref, useId } from "vue";
import * as Yup from "yup";
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

const filterConfig = computed<FilterConfig[]>(() => [
  {
    key: "customerId",
    label: pt("Client"),
    type: "slot",
    valueLabel: (value) =>
      typeof value === "string"
        ? (customersStore.getCustomerNameById(value) ?? "")
        : "",
  },
  {
    key: "referenceId",
    label: pt("Referència"),
    type: "slot",
    valueLabel: (value) =>
      typeof value === "string"
        ? (referenceStore.getFullNameById(value) ?? "")
        : "",
  },
  {
    key: "dates",
    label: pt("lastUpdated"),
    type: "date-range",
    placeholder: pt("Selecciona periode"),
  },
]);

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

const cardLayout: CardLayout = {
  title: "reference.code",
  subtitle: "reference.customerId",
  trailing: "totalCost",
  meta: ["mode", "baseQuantity", "disabled"],
};

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

const createLoading = ref(false);

const createRows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "referenceId",
        label: pt("Referència"),
        type: FormFieldType.Custom,
        validation: Yup.string()
          .nullable()
          .required(t("production.validation.laReferenciaEsObligatoria")),
      },
    ],
  },
]);

type CopyDestinyMode = "existing" | "new";

interface CopyFormValues {
  copyDestinyMode: CopyDestinyMode;
  referenceId: string | null;
  referenceCode: string;
  referenceDescription: string;
  mode: number;
}

const copyDialogVisible = ref(false);
const copyLoading = ref(false);
// Assigned once per opening: a stable snapshot for the copy form.
const copyInitialValues = ref<CopyFormValues | null>(null);
const copySourceWorkmasterId = ref("");
const copySourceName = ref("");

// Section fields render their own controls, so their IDs are generated here.
const copyFormId = useId();
const copyFieldId = (name: string): string => `copy-${copyFormId}-${name}`;

const copyDestinyOptions = computed<
  Array<{ value: CopyDestinyMode; label: string }>
>(() => [
  { value: "existing", label: pt("Referència existent") },
  { value: "new", label: pt("Crear nova referència") },
]);

const copyDestinyModeValue = (value: unknown): CopyDestinyMode =>
  value === "new" ? "new" : "existing";

const copyRows = computed<FormRowConfig[]>(() => [
  {
    fields: [
      {
        name: "copyDestinyMode",
        label: pt("Destí de la còpia"),
        type: FormFieldType.Custom,
        defaultValue: "existing",
      },
    ],
  },
  {
    section: "existingReference",
    fields: [
      {
        name: "referenceId",
        label: pt("Referència"),
        type: FormFieldType.Custom,
        validation: Yup.string()
          .nullable()
          .test(
            "existing-reference-required",
            pt("Selecciona una referència de destí"),
            function (value) {
              return (
                copyDestinyModeValue(this.parent.copyDestinyMode) !==
                  "existing" || Boolean(value)
              );
            },
          ),
      },
    ],
  },
  {
    section: "newReference",
    fields: [
      {
        name: "referenceCode",
        label: pt("Codi"),
        type: FormFieldType.Custom,
        validation: Yup.string()
          .nullable()
          .test(
            "new-reference-code-required",
            pt("Introdueix el codi de la nova referència"),
            function (value) {
              return (
                copyDestinyModeValue(this.parent.copyDestinyMode) !== "new" ||
                Boolean(value?.trim())
              );
            },
          ),
      },
      {
        name: "referenceDescription",
        label: pt("Descripció"),
        type: FormFieldType.Custom,
      },
    ],
  },
  {
    fields: [
      {
        name: "mode",
        label: pt("Mode de fabricació"),
        type: FormFieldType.Select,
        props: {
          options: workmasterStore.workmasterModes,
          optionLabel: "value",
          optionValue: "id",
          placeholder: pt("Selecciona el mode"),
        },
      },
    ],
  },
]);

onMounted(async () => {
  store.setMenuItem({
    icon: PrimeIcons.CALENDAR,
    title: pt("Gestió de rutes de fabricació"),
  });

  referenceStore.fetchReferencesByModule("sales");
  // The customer column (and the card's subtitle) resolves names from here.
  if (!customersStore.customers) customersStore.fetchCustomers();
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
  copyInitialValues.value = {
    copyDestinyMode: "existing",
    referenceId: null,
    referenceCode: "",
    referenceDescription: "",
    mode: 1,
  };
  copyDialogVisible.value = true;
};

const onCopySubmit = async (values: FormValues) => {
  const destinyMode = copyDestinyModeValue(values.copyDestinyMode);

  const payload: WorkMasterToCopy = {
    workmasterId: copySourceWorkmasterId.value,
    referenceId:
      destinyMode === "existing"
        ? nullableStringValue(values.referenceId, null)
        : null,
    referenceCode:
      destinyMode === "new" ? stringValue(values.referenceCode, "").trim() : "",
    referenceDescription:
      destinyMode === "new"
        ? stringValue(values.referenceDescription, "").trim()
        : "",
    mode: integerValue(values.mode, 1),
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
      if (destinyMode === "new") {
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

const onCreateSubmit = async (values: FormValues) => {
  if (!workmasterStore.workmaster) return;

  const workmaster: WorkMaster = {
    ...workmasterStore.workmaster,
    referenceId: stringValue(
      values.referenceId,
      workmasterStore.workmaster.referenceId,
    ),
  };

  createLoading.value = true;
  try {
    const created = await workmasterStore.create(workmaster);
    if (created) router.push({ path: `/workmaster/${workmaster.id}` });
  } finally {
    createLoading.value = false;
  }
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
