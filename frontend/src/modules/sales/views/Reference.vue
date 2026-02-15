<template>
  <div v-if="!loading">
    <FormReference
      v-if="reference"
      :reference="reference"
      @submit="submitForm"
    />

    <Tabs v-if="reference" value="0">
      <TabList>
        <Tab value="0">Documentació</Tab>
        <Tab value="1" v-if="formMode === FormActionMode.EDIT"
          >Rutes de fabricació</Tab
        >
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <FileEntityPicker
            title="Documentació"
            entity="referenceMaps"
            :id="reference.id"
            :key="reference.id"
          />
        </TabPanel>
        <TabPanel value="1" v-if="formMode === FormActionMode.EDIT">
          <DataTable
            :value="workmasterStore.workmasters"
            tableStyle="min-width: 100%"
            scrollable
            scrollHeight="flex"
            sort-field="reference.code"
            :sort-order="1"
            @row-click="editWorkmaster"
            :loading="workmasterLoading"
          >
            <template #header>
              <div
                class="flex flex-wrap align-items-center justify-content-between gap-2"
              >
                <div>
                  <label class="block text-900">Rutes de fabricació</label>
                </div>
                <div class="datatable-buttons">
                  <Button
                    :icon="PrimeIcons.PLUS"
                    rounded
                    raised
                    :loading="creatingWorkmaster"
                    @click="createWorkmaster"
                  />
                </div>
              </div>
            </template>
            <!-- ...resto de columnas sin cambios... -->
            <Column
              field="baseQuantity"
              header="Quantitat Base"
              style="width: 10%"
            ></Column>
            <Column
              field="machineCost"
              header="Cost màquina"
              style="width: 10%"
            >
              <template #body="slotProps">
                {{ formatCurrency(slotProps.data.machineCost) }}
              </template>
            </Column>
            <Column
              field="operatorCost"
              header="Cost operari"
              style="width: 10%"
            >
              <template #body="slotProps">
                {{ formatCurrency(slotProps.data.operatorCost) }}
              </template>
            </Column>
            <Column
              field="materialCost"
              header="Cost material"
              style="width: 10%"
            >
              <template #body="slotProps">
                {{ formatCurrency(slotProps.data.materialCost) }}
              </template>
            </Column>
            <Column
              field="externalCost"
              header="Cost extern"
              style="width: 10%"
            >
              <template #body="slotProps">
                {{ formatCurrency(slotProps.data.externalCost) }}
              </template>
            </Column>
            <Column header="Cost total" style="width: 10%">
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
            <Column>
              <template #body="slotProps">
                <i
                  :class="PrimeIcons.TIMES"
                  class="grid_delete_column_button"
                  @click="deleteWorkmaster($event, slotProps.data)"
                />
              </template>
            </Column>
          </DataTable>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </div>
</template>

<script setup lang="ts">
import FormReference from "../components/FormReference.vue";
import FileEntityPicker from "../../../components/FileEntityPicker.vue";
import { onMounted, onUnmounted, ref } from "vue";
import { FormActionMode } from "../../../types/component";
import { PrimeIcons } from "@primevue/core/api";
import { useRoute, useRouter } from "vue-router";
import { storeToRefs } from "pinia";
import { useStore } from "../../../store";
import { useToast } from "primevue/usetoast";
import { useReferenceStore } from "../../shared/store/reference";
import { useTaxesStore } from "../../shared/store/tax";
import { useWorkMasterStore } from "../../production/store/workmaster";
import { Reference, ReferenceCategoryEnum } from "../../shared/types";
import { getNewUuid } from "../../../utils/functions";
import { DataTableRowClickEvent } from "primevue/datatable";
import { useConfirm } from "primevue/useconfirm";
import { formatCurrency } from "../../../utils/functions";

const route = useRoute();
const router = useRouter();
const confirm = useConfirm();

const store = useStore();
const taxesStore = useTaxesStore();
const referenceStore = useReferenceStore();
const workmasterStore = useWorkMasterStore();

const formMode = ref(FormActionMode.EDIT);
const { reference } = storeToRefs(referenceStore);
const id = ref("");
const module = ref("");

// Estados de carga
const loading = ref(false);
const workmasterLoading = ref(false);
const creatingWorkmaster = ref(false);

const loadView = async () => {
  loading.value = true;

  try {
    // Cargar datos en paralelo donde sea posible
    const promises = [
      taxesStore.fetchAll(),
      referenceStore.fetchReference(id.value),
    ];

    await Promise.all(promises);

    let pageTitle = "";
    if (!reference.value) {
      formMode.value = FormActionMode.CREATE;
      referenceStore.setNewReference(id.value, ReferenceCategoryEnum.PRODUCT);
      pageTitle = `Alta de referència`;
    } else {
      formMode.value = FormActionMode.EDIT;
      pageTitle = `Referència ${reference.value.code} - ${reference.value.description}`;

      // Solo cargar workmasters si estamos en modo edición y tenemos una referencia
      workmasterLoading.value = true;
      try {
        await workmasterStore.fetchByReferenceId(reference.value.id);
      } finally {
        workmasterLoading.value = false;
      }
    }

    store.setMenuItem({
      icon: PrimeIcons.BUILDING,
      backButtonVisible: true,
      title: pageTitle,
    });
  } catch (error) {
    console.error("Error loading view:", error);
    toast.add({
      severity: "error",
      summary: "Error al carregar la vista",
      life: 5000,
    });
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  id.value = route.params.id as string;
  module.value = route.params.module as string;
  await loadView();
});

onUnmounted(() => {
  referenceStore.reference = undefined;
  workmasterStore.workmasters = undefined;
});

const toast = useToast();
const submitForm = async () => {
  const data = reference.value as Reference;
  let result = false;
  let message = "";

  if (formMode.value === FormActionMode.CREATE) {
    result = await referenceStore.createReference(data);
    if (result) message = "Referència creada correctament";
    else message = "La referència + versió introduïda ja existeix";
  } else {
    result = await referenceStore.updateReference(data.id, data);
    message = "Referència actualizada correctament";
  }

  toast.add({
    severity: result ? "success" : "warn",
    summary: message,
    life: 5000,
  });

  if (result) {
    if (formMode.value === FormActionMode.EDIT) {
      router.back();
    } else {
      await loadView();
    }
  }
};

// Workmasters
const createWorkmaster = async () => {
  if (reference.value) {
    creatingWorkmaster.value = true;

    try {
      const newId = getNewUuid();
      workmasterStore.setNew(newId, reference.value.id);

      if (workmasterStore.workmaster) {
        const created = await workmasterStore.create(
          workmasterStore.workmaster,
        );
        if (created) {
          router.push({ path: `/workmaster/${newId}` });
        } else {
          toast.add({
            severity: "error",
            summary: "Error al crear la ruta de fabricació",
            life: 5000,
          });
        }
      }
    } catch (error) {
      console.error("Error creating workmaster:", error);
      toast.add({
        severity: "error",
        summary: "Error al crear la ruta de fabricació",
        life: 5000,
      });
    } finally {
      creatingWorkmaster.value = false;
    }
  }
};

const editWorkmaster = (row: DataTableRowClickEvent) => {
  router.push({ path: `/workmaster/${row.data.id}` });
};

const deleteWorkmaster = async (event: any, workmaster: any) => {
  event.stopPropagation();

  confirm.require({
    target: event.currentTarget,
    message: `Está segur que vol eliminar la ruta seleccionada?`,
    icon: "pi pi-question-circle",
    acceptIcon: "pi pi-check",
    rejectIcon: "pi pi-times",
    accept: async () => {
      const result = await workmasterStore.delete(workmaster.id);
      if (result) {
        toast.add({
          severity: "success",
          summary: "Ruta eliminada correctament",
          life: 5000,
        });

        await workmasterStore.fetchByReferenceId(reference.value!.id);
      } else {
        toast.add({
          severity: "warn",
          summary: "No s'ha pogut eliminar la ruta",
          life: 5000,
        });
      }
    },
  });
};
</script>
