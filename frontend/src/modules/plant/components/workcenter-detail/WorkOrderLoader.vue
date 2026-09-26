<template>
  <Dialog
    :visible="visible"
    modal
    :closable="true"
    class="loader-dialog"
    :style="{ width: '60rem' }"
    :breakpoints="{ '1024px': '94vw', '767px': '98vw' }"
    @update:visible="$emit('update:visible', $event)"
  >
    <template #header>
      <div class="loader-dialog__header">
        <span class="loader-dialog__title">{{ t("plant.loader.title") }}</span>
        <span class="loader-dialog__context">{{
          t("plant.loader.context", {
            order: workOrderCode,
            reference: referenceCode,
            count: quantity,
          })
        }}</span>
      </div>
    </template>

    <Tabs
      v-model:value="activeTab"
      class="loader-tabs"
      @update:value="onTabChange"
    >
      <TabList>
        <Tab value="load">{{ t("plant.loader.tabLoad") }}</Tab>
        <Tab value="create">{{ t("plant.loader.tabCreate") }}</Tab>
      </TabList>

      <TabPanels>
        <TabPanel value="load">
          <div class="tab-content">
            <p v-if="loading" class="loader-empty">
              <ProgressSpinner style="width: 2rem; height: 2rem" />
            </p>
            <ul
              v-else-if="sortedPhases.length"
              class="loader-phases"
              role="radiogroup"
              :aria-label="t('plant.loader.phases')"
            >
              <li v-for="phase in sortedPhases" :key="phase.phaseId">
                <button
                  type="button"
                  role="radio"
                  class="loader-phase"
                  :class="{
                    'loader-phase--selected': selectedPhase?.phaseId === phase.phaseId,
                    'loader-phase--locked': !isCompatible(phase),
                  }"
                  :aria-checked="selectedPhase?.phaseId === phase.phaseId"
                  :disabled="!isCompatible(phase)"
                  @click="selectPhase(phase)"
                >
                  <span class="loader-phase__radio" aria-hidden="true">
                    <i v-if="!isCompatible(phase)" class="pi pi-lock"></i>
                  </span>
                  <span class="loader-phase__code">{{ phase.phaseCode }}</span>
                  <span class="loader-phase__main">
                    <span class="loader-phase__description">{{
                      phase.phaseDescription
                    }}</span>
                    <span class="loader-phase__meta">{{ phaseMeta(phase) }}</span>
                  </span>
                  <span class="loader-phase__qty">
                    <span>{{ phase.quantityOk }}</span>
                    <span class="loader-phase__ko">{{ phase.quantityKo }}</span>
                  </span>
                </button>
              </li>
            </ul>

            <Message
              v-if="hasLoadedWorkOrders && phases.length > 0"
              severity="warn"
              :closable="false"
              >{{ t("plant.loader.busy") }}</Message
            >
            <Message
              v-else-if="!loading && phases.length === 0"
              severity="warn"
              :closable="false"
              >{{ t("plant.loader.noPhases") }}</Message
            >

            <div v-if="!hasLoadedWorkOrders" class="loader-footer">
              <div class="loader-footer__field">
                <label class="loader-footer__label">{{ t("plant.loader.activity") }}</label>
                <SelectWorkOrderPhaseDetail
                  v-model="selectedDetailId"
                  :details="selectedPhase?.details || []"
                  class="activity-dropdown"
                />
              </div>
              <Button
                icon="pi pi-check"
                :label="t('plant.loader.load')"
                :disabled="!selectedDetailId"
                class="loader-footer__action"
                @click="onLoadActivity"
              />
            </div>
          </div>
        </TabPanel>

        <TabPanel value="create">
          <div class="tab-content">
            <PhaseTemplateLoader
              ref="phaseTemplateLoaderRef"
              :workOrderId="workOrderId"
              :workcenterTypeId="workcenterTypeId"
              :preferredWorkcenterId="workcenterId"
              @phase-created="onPhaseCreated"
            />
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>
  </Dialog>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { ref, watch, onMounted, computed } from "vue";
import Message from "primevue/message";
import ProgressSpinner from "primevue/progressspinner";
import { WorkOrderPhaseDetailed } from "../../../production/types";
import { WorkOrderPhaseService } from "../../../production/services/workorder.service";
import { useToast } from "primevue/usetoast";
import { usePlantWorkcenterStore } from "../../store/workcenter.store";
import SelectWorkOrderPhaseDetail from "./SelectWorkOrderPhaseDetail.vue";
import PhaseTemplateLoader from "./PhaseTemplateLoader.vue";

const { t } = useI18n();

interface Props {
  visible: boolean;
  workOrderId: string;
  workOrderCode: string;
  referenceCode: string;
  quantity: number;
  workcenterTypeId: string;
  workcenterId: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{
  (e: "update:visible", value: boolean): void;
  (
    e: "phase-detail-selected",
    data: {
      workOrderId: string;
      workOrderPhaseId: string;
      machineStatusId: string;
    },
  ): void;
  (e: "phase-created"): void;
}>();

const toast = useToast();
const phaseService = new WorkOrderPhaseService("WorkOrderPhase");
const workcenterStore = usePlantWorkcenterStore();

const phases = ref<WorkOrderPhaseDetailed[]>([]);
const loading = ref(false);
const selectedDetailId = ref<string>("");
const selectedPhaseId = ref<string>("");
const activeTab = ref<"load" | "create">("load");
const phaseTemplateLoaderRef = ref<InstanceType<
  typeof PhaseTemplateLoader
> | null>(null);

const hasLoadedWorkOrders = computed(() => {
  return workcenterStore.loadedWorkOrdersPhases.length > 0;
});

const autoSelectedPhase = computed(() => {
  const validPhases = phases.value
    .filter(
      (phase) =>
        !phase.endTime && phase.workcenterTypeId === props.workcenterTypeId,
    )
    .sort((a, b) => a.phaseCode.localeCompare(b.phaseCode));
  return validPhases[0];
});

const selectedPhase = computed(() => {
  if (selectedPhaseId.value) {
    return phases.value.find((p) => p.phaseId === selectedPhaseId.value);
  }
  return autoSelectedPhase.value;
});

const selectPhase = (phase: WorkOrderPhaseDetailed) => {
  selectedPhaseId.value = phase.phaseId;
  if (phase.details && phase.details.length > 0) {
    selectedDetailId.value = phase.details[0].machineStatusId || "";
  } else {
    selectedDetailId.value = "";
  }
};

// Only phases for this machine type can be loaded; the rest stay visible
// but locked, so the operator sees why they cannot pick them.
const isCompatible = (phase: WorkOrderPhaseDetailed) =>
  phase.workcenterTypeId === props.workcenterTypeId;

const sortedPhases = computed(() =>
  [...phases.value].sort((a, b) => a.phaseCode.localeCompare(b.phaseCode)),
);

const phaseMeta = (phase: WorkOrderPhaseDetailed) =>
  [
    phase.phaseStatus,
    phase.endTime ? t("plant.loader.finished") : "",
    phase.preferredWorkcenterName,
    isCompatible(phase) ? "" : t("plant.loader.otherMachine"),
  ]
    .filter(Boolean)
    .join(" · ");

const onLoadActivity = () => {
  if (!selectedPhase.value || !selectedDetailId.value) {
    toast.add({
      severity: "warn",
      summary: t("plant.messages.selectPhaseAndActivity"),
      life: 4000,
    });
    return;
  }
  emit("phase-detail-selected", {
    workOrderId: props.workOrderId,
    workOrderPhaseId: selectedPhase.value.phaseId,
    machineStatusId: selectedDetailId.value,
  });
};

const loadPhases = async () => {
  if (!props.workOrderId) return;
  loading.value = true;
  selectedDetailId.value = "";
  selectedPhaseId.value = "";
  try {
    const result = await phaseService.GetWorkOrderPhasesDetailed(
      props.workOrderId,
    );
    if (result) {
      phases.value = result;
      if (autoSelectedPhase.value) {
        selectPhase(autoSelectedPhase.value);
      }
    } else {
      phases.value = [];
    }
  } catch (error) {
    console.error("Error loading work order phases:", error);
    toast.add({
      severity: "error",
      summary: t("plant.messages.workOrderPhasesLoadError"),
      life: 4000,
    });
    phases.value = [];
  } finally {
    loading.value = false;
  }
};

const onPhaseCreated = async () => {
  await loadPhases();
  activeTab.value = "load";
  emit("phase-created");
};

// Reload the template tab content when the user switches to it
const onTabChange = (tab: string | number) => {
  if (tab === "create") {
    phaseTemplateLoaderRef.value?.load();
  }
};

watch(
  () => props.workOrderId,
  () => {
    if (props.visible && props.workOrderId) loadPhases();
  },
);

watch(
  () => props.visible,
  (newValue) => {
    if (newValue && props.workOrderId) {
      activeTab.value = "load";
      loadPhases();
    }
  },
);

onMounted(() => {
  if (props.visible && props.workOrderId) loadPhases();
});
</script>

<style scoped>
.loader-dialog__header {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.loader-dialog__title {
  font-family: var(--font-condensed);
  font-size: 1.625rem;
  line-height: 2rem;
  font-weight: 600;
  color: var(--p-steel-900);
}

.loader-dialog__context {
  font-size: 0.9375rem;
  font-variant-numeric: tabular-nums;
  color: var(--p-steel-700);
}

.loader-tabs :deep(.p-tab) {
  min-height: 52px;
}

.tab-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding-top: 1rem;
}

.loader-empty {
  display: flex;
  justify-content: center;
  margin: 0;
  padding: 2rem;
}

.loader-phases {
  margin: 0;
  padding: 0;
  list-style: none;
  border: 1px solid var(--p-steel-200);
  border-radius: 4px;
  overflow: hidden;
}

.loader-phase {
  width: 100%;
  min-height: 64px;
  display: grid;
  grid-template-columns: 28px 4rem minmax(0, 1fr) auto;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 1rem;
  border: none;
  border-bottom: 1px solid var(--p-steel-100);
  background: var(--p-surface-0);
  font: inherit;
  color: var(--p-steel-900);
  text-align: left;
  cursor: pointer;
}

.loader-phases li:last-child .loader-phase {
  border-bottom: none;
}

.loader-phase:focus-visible {
  outline: 3px solid var(--p-steel-900);
  outline-offset: -3px;
}

.loader-phase--selected {
  background: var(--p-primary-50);
}

.loader-phase--locked {
  color: var(--p-steel-600);
  cursor: default;
}

.loader-phase__radio {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  box-shadow: inset 0 0 0 2px var(--p-steel-400);
  font-size: 0.75rem;
}

.loader-phase--selected .loader-phase__radio {
  box-shadow: inset 0 0 0 7px var(--p-primary-color);
}

.loader-phase--locked .loader-phase__radio {
  box-shadow: none;
  background: var(--p-steel-100);
}

.loader-phase__code {
  font-family: var(--font-condensed);
  font-size: 1.1875rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.loader-phase__main {
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.loader-phase__description {
  font-size: 1rem;
  line-height: 1.375rem;
}

.loader-phase__meta {
  font-size: 0.8125rem;
  color: var(--p-steel-600);
}

.loader-phase__qty {
  display: flex;
  gap: 0.5rem;
  font-family: var(--font-condensed);
  font-size: 1.0625rem;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.loader-phase__ko {
  color: var(--p-red-700);
}

.loader-footer {
  display: flex;
  align-items: flex-end;
  gap: 0.75rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-steel-200);
}

.loader-footer__field {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.loader-footer__label {
  font-family: var(--font-condensed);
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-steel-700);
}

.activity-dropdown :deep(.p-select),
.activity-dropdown.p-select {
  min-height: 56px;
}

.loader-footer__action {
  min-height: 56px;
  padding-inline: 1.25rem;
  font-size: 1.0625rem;
}

@media (max-width: 767.98px) {
  .loader-phase {
    grid-template-columns: 24px 3rem minmax(0, 1fr);
    padding: 0.5rem 0.75rem;
  }

  .loader-phase__qty {
    display: none;
  }

  .loader-footer {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>
