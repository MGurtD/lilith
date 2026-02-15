<template>
  <SplitButton
    label="Guardar"
    @click="submitForm"
    :model="items"
    :size="'small'"
    class="grid_add_row_button"
  />

  <FormBudget class="mt-3 mb-3" ref="budgetForm" @submit="onBudgetSubmit" />

  <Tabs value="0">
    <TabList>
      <Tab value="0">Detall</Tab>
      <Tab value="1">Notes</Tab>
    </TabList>
    <TabPanels>
      <TabPanel value="0">
        <TableBudgetDetails
          v-if="budget && budget.details"
          :budget="budget"
          :details="budget.details"
          @edit="
            (det: BudgetDetail) =>
              openBudgetDetailDialog(FormActionMode.EDIT, det)
          "
          @delete="deleteSalesOrderDetails"
        >
          <template #header>
            <div
              class="flex flex-wrap align-items-center justify-content-between gap-2"
            >
              <span class="text-l text-900 font-bold"
                >Linies del pressupost</span
              >
              <section v-if="!budgetStore.order">
                <Button
                  :size="'small'"
                  label="Afegir línea"
                  @click="
                    openBudgetDetailDialog(FormActionMode.CREATE, {} as any)
                  "
                  class="mr-2"
                />
              </section>
            </div>
          </template>
        </TableBudgetDetails>
      </TabPanel>
      <TabPanel value="1">
        <section v-if="budget" class="mt-2">
          <div>
            <label class="block text-900 mb-2">Notes Internes</label>
            <Textarea
              class="w-full"
              rows="3"
              placeholder="Notes internes"
              v-model="budget.userNotes"
            />
          </div>
        </section>
        <section v-if="budget" class="mt-2">
          <div>
            <BaseInput
              :type="BaseInputType.TEXT"
              label="Notes automàtiques"
              id="notes"
              v-model="budget.notes"
              disabled
            />
          </div>
        </section>
      </TabPanel>
    </TabPanels>
  </Tabs>

  <Dialog
    :closable="true"
    :style="{ width: '100%' }"
    :maximizable="true"
    v-model:visible="isDetailDialogVisible"
    :header="detailDialogTitle"
    :modal="true"
    v-if="budget"
  >
    <FormBudgetOrderDetail
      v-if="budget && budgetDetail"
      :formAction="formDetailMode"
      :header="budget"
      :detail="budgetDetail"
      :readonly="false"
      @submit="onBudgetDetailSubmit"
    />
  </Dialog>
  <!--:readonly="budgetStore.order !== null"-->
</template>
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { PrimeIcons } from "@primevue/core/api";
import { storeToRefs } from "pinia";
import { Budget, BudgetDetail, SalesOrderDetail } from "../types";
import { useStore } from "../../../store";
import { BaseInputType } from "../../../types/component";
import {
  createBlobAndDownloadFile,
  getNewUuid,
} from "../../../utils/functions";
import { useToast } from "primevue/usetoast";
import { FormActionMode } from "../../../types/component";
import { useReferenceStore } from "../../shared/store/reference";
import { useCustomersStore } from "../store/customers";
import { useExerciseStore } from "../../shared/store/exercise";
import { usePlantModelStore } from "../../production/store/plantmodel";
import { useLifecyclesStore } from "../../shared/store/lifecycle";
import { useTaxesStore } from "../../shared/store/tax";
import { REPORTS, ReportService } from "../../../services/report.service";
import Services from "../services";
import { useWorkMasterStore } from "../../production/store/workmaster";
import { useBudgetStore } from "../store/budget";
import TableBudgetDetails from "../components/TableBudgetDetails.vue";
import FormBudget from "../components/FormBudget.vue";
import FormBudgetOrderDetail from "../components/FormBudgetOrderDetail.vue";
import { useSalesOrderStore } from "../store/order";

const formMode = ref(FormActionMode.EDIT);
const budgetForm = ref();

const route = useRoute();
const router = useRouter();
const store = useStore();
const toast = useToast();
const budgetStore = useBudgetStore();
const customerStore = useCustomersStore();
const plantModelStore = usePlantModelStore();
const exerciseStore = useExerciseStore();
const lifeCycleStore = useLifecyclesStore();
const referenceStore = useReferenceStore();
const workMasterStore = useWorkMasterStore();
const taxesStore = useTaxesStore();
const salesOrderStore = useSalesOrderStore();
const { budget } = storeToRefs(budgetStore);

const items = [
  {
    label: "Descarregar",
    icon: PrimeIcons.FILE_WORD,
    command: () => printInvoice(),
  },
  {
    label: "Crear comanda",
    icon: PrimeIcons.FLAG_FILL,
    command: () => createSalesOrder(),
  },
];

const detailDialogTitle = "Línia del pressupost";
const isDetailDialogVisible = ref(false);
const formDetailMode = ref(FormActionMode.EDIT);
const budgetDetail = ref(undefined as undefined | BudgetDetail);

const loadView = async () => {
  const budgetId = route.params.id as string;
  await budgetStore.GetById(budgetId);
  await budgetStore.GetAssociatedSalesOrders(budgetId);

  referenceStore.fetchReferencesByModule("sales");
  workMasterStore.fetchAllActives();
  lifeCycleStore.fetchOneByName("Budget");
  plantModelStore.fetchSites();
  exerciseStore.fetchAll();
  customerStore.fetchCustomers();
  taxesStore.fetchAll();

  let pageTitle = "";
  if (budget.value) {
    formMode.value = FormActionMode.EDIT;
    pageTitle = `Pressupost ${budget.value.number}`;
  }

  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    backButtonVisible: true,
    title: pageTitle,
  });
};

onMounted(async () => {
  await loadView();
});

onUnmounted(() => {
  budgetStore.budget = undefined;
  budgetStore.order = undefined;
});

const submitForm = () => {
  if (!budget.value?.date) {
    toast.add({
      severity: "error",
      summary: "Error al crear la comanda ",
      detail: "La data no pot estar buida",
      life: 5000,
    });
    return false;
  }
  const form = budgetForm.value as any;
  form.submitForm();
};

const openBudgetDetailDialog = (
  formMode: FormActionMode,
  detail: BudgetDetail,
) => {
  if (formMode === FormActionMode.CREATE) {
    const budgetExercise = exerciseStore.exercises?.find(
      (e) => e.id === budget.value!.exerciseId,
    );

    detail = {
      id: getNewUuid(),
      referenceId: "",
      workMasterId: null,
      profit: 0,
      productionProfit: 0,
      materialProfit: budgetExercise?.materialProfit || 30,
      externalProfit: budgetExercise?.externalProfit || 30,
      discount: 0,
      quantity: 1,
      unitCost: 0,
      productionCost: 0,
      materialCost: 0,
      unitPrice: 0,
      serviceCost: 0,
      transportCost: 0,
      totalCost: 0,
      amount: 0,
      budgetId: budget.value!.id,
      description: "",
    } as BudgetDetail;
  }

  budgetDetail.value = Object.assign({}, detail);
  formDetailMode.value = formMode;
  isDetailDialogVisible.value = true;
};

const onBudgetSubmit = async (budget: Budget) => {
  let result = false;
  let message = "";

  result = await budgetStore.Update(budget.id, budget);
  message = result
    ? "Pressupost actualitzat correctament"
    : "Error a l'actualitzar el pressupost";

  toast.add({
    life: 5000,
    severity: result ? "success" : "error",
    summary: message,
  });

  if (result) {
    router.back();
  }
};

const onBudgetDetailSubmit = async (
  detail: BudgetDetail | SalesOrderDetail,
) => {
  detail = detail as BudgetDetail;

  if (formDetailMode.value === FormActionMode.CREATE) {
    await budgetStore.CreateDetail(detail);
    //budget.value!.details!.push(detail);
  } else if (formDetailMode.value === FormActionMode.EDIT) {
    await budgetStore.UpdateDetail(detail);
    const index = budget.value!.details!.findIndex((i) => i.id === detail.id);
    budget.value!.details![index] = detail;
  }

  isDetailDialogVisible.value = false;
};

const deleteSalesOrderDetails = async (detail: BudgetDetail) => {
  if (formMode.value === FormActionMode.EDIT) {
    await budgetStore.DeleteDetail(detail);
  }
  const afterDelete = budget.value!.details!.filter((i) => i.id !== detail.id);
  budget.value!.details = afterDelete;
  isDetailDialogVisible.value = false;
};

const createSalesOrder = async () => {
  if (budgetStore.order) {
    toast.add({
      severity: "warn",
      summary: "Aquest pressupost ja té una comanda associada",
      life: 5000,
    });
    return;
  }

  if (budget.value) {
    const response = await salesOrderStore.CreateFromBudget(budget.value);

    if (response.result) {
      toast.add({
        severity: "success",
        summary: `Comanda ${response.content?.number} creada correctament`,
        life: 5000,
      });

      router.push(`/salesorder/${response.content?.id}`);
    } else {
      toast.add({
        severity: "error",
        summary: "Error al crear la comanda ",
        detail: response.errors[0],
        life: 5000,
      });
    }
  }
};

const printInvoice = async () => {
  const budgetReport = await Services.Budget.GetReportDataById(
    budget.value!.id,
  );

  if (budgetReport) {
    const fileName = `Pressupost_${budget.value?.number}.docx`;

    const reportService = new ReportService();
    const report = await reportService.Download(
      budgetReport,
      REPORTS.Budget,
      fileName,
    );

    if (report) {
      createBlobAndDownloadFile(fileName, report);
    } else {
      toast.add({
        severity: "warn",
        summary: "Error",
        detail: "No s'ha pugut generar fulla del pressupost",
      });
    }
  }
};
</script>
