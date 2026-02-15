<template>
  <main class="plant-layout">
    <!-- Barra de filtros sticky (solo visible cuando hay filtro activo) -->
    <div v-if="showOnlyMyWorkcenters" class="filters-container">
      <Chip
        label="Els meus centres"
        icon="pi pi-user-check"
        class="filter-chip filter-active"
        removable
        @remove="toggleMyWorkcenters"
      />

      <div class="filter-summary">
        <i class="pi pi-info-circle"></i>
        <span
          >Mostrant {{ myWorkcentersCount }} de
          {{ totalWorkcentersCount }} centres</span
        >
      </div>
    </div>

    <div v-for="area in filteredAreas" :key="area.id" class="area-section">
      <header class="area__header" @click="toggleArea(area.id)">
        <div class="area__header-content">
          <i class="pi pi-building area__icon"></i>
          <span class="area__name">{{ area.name }}</span>
          <span class="area__count"
            >{{ getFilteredWorkcenters(area.id).length }} centres</span
          >

          <!-- Indicador si tengo fichajes en esta área -->
          <Badge
            v-if="hasMyWorkcentersInArea(area.id)"
            value="Actiu"
            severity="success"
            class="area__badge"
          />
        </div>
        <i
          :class="
            isAreaVisible(area.id) ? 'pi pi-chevron-up' : 'pi pi-chevron-down'
          "
          class="area__toggle-icon"
        ></i>
      </header>

      <section v-show="isAreaVisible(area.id)" class="area__workcenters">
        <WorkcenterCard
          v-for="view in getFilteredWorkcenters(area.id)"
          :key="view.config.id"
          :workcenter="view"
          @click="(id: string) => $router.push(`/plant/workcenter/${id}`)"
        />
      </section>
    </div>

    <!-- Floating Action Button -->
    <Button
      icon="pi pi-filter"
      rounded
      :severity="showOnlyMyWorkcenters ? 'success' : 'secondary'"
      class="filter-fab"
      @click="toggleMyWorkcenters"
      v-tooltip.left="
        showOnlyMyWorkcenters
          ? 'Veure tots els centres'
          : 'Veure només els meus centres'
      "
      :badge="myWorkcentersCount > 0 ? String(myWorkcentersCount) : undefined"
      badgeSeverity="success"
    />
  </main>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import {
  usePlantDataStore,
  usePlantRealtimeStore,
  usePlantOperatorStore,
} from "../store";
import { WorkcenterViewState } from "../types";
import WorkcenterCard from "../components/WorkcenterCard.vue";
import { useStore } from "../../../store";
import { PrimeIcons } from "@primevue/core/api";
import {
  useWebSocketConnection,
  WS_ENDPOINTS,
} from "../composables/useWebSocketConnection";
import Chip from "primevue/chip";
import Badge from "primevue/badge";
import Button from "primevue/button";

const STORAGE_KEY = "temges.plant-visible-areas";
const FILTER_STORAGE_KEY = "temges.plant-filter-my-workcenters";

const store = useStore();
const dataStore = usePlantDataStore();
const realtimeStore = usePlantRealtimeStore();
const operatorStore = usePlantOperatorStore();
const visibleAreas = ref<Set<string>>(new Set());
const showOnlyMyWorkcenters = ref(false);
const { connect } = useWebSocketConnection();

// Carregar àrees desplegades del localStorage
const loadVisibleAreas = (): void => {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as string[];
      visibleAreas.value = new Set(parsed);
    }
  } catch (error) {
    console.warn("Error carregant àrees desplegades:", error);
    visibleAreas.value = new Set();
  }
};

// Guardar àrees desplegades al localStorage
const saveVisibleAreas = (): void => {
  try {
    const areaIds = Array.from(visibleAreas.value);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(areaIds));
  } catch (error) {
    console.warn("Error guardant àrees desplegades:", error);
  }
};

// Carregar preferència de filtre del localStorage
const loadFilterPreference = (): void => {
  try {
    const stored = localStorage.getItem(FILTER_STORAGE_KEY);
    if (stored) {
      showOnlyMyWorkcenters.value = JSON.parse(stored);
    }
  } catch (error) {
    console.warn("Error carregant preferència de filtre:", error);
    showOnlyMyWorkcenters.value = false;
  }
};

// Guardar preferència de filtre al localStorage
const saveFilterPreference = (): void => {
  try {
    localStorage.setItem(
      FILTER_STORAGE_KEY,
      JSON.stringify(showOnlyMyWorkcenters.value)
    );
  } catch (error) {
    console.warn("Error guardant preferència de filtre:", error);
  }
};

onMounted(async () => {
  // 1. Carregar estat de les àrees desplegades i preferència de filtre
  loadVisibleAreas();
  loadFilterPreference();

  // 2. Carregar dades mestres d'àrees, workcenters i estats de màquina
  await Promise.all([
    dataStore.fetchAreasWithWorkcenters(),
    dataStore.fetchMachineStatuses(),
  ]);

  // 3. Configurar header
  store.setMenuItem({
    icon: PrimeIcons.BUILDING,
    title: `Àrees de ${dataStore.site?.name || "Planta"}`,
  });

  // 4. Connectar WebSocket general i configurar handlers al store
  realtimeStore.connectToGeneral();
  connect(WS_ENDPOINTS.GENERAL, { debug: true });
});

onUnmounted(() => {
  // Guardar estat de les àrees desplegades i preferència de filtre abans de sortir
  saveVisibleAreas();
  saveFilterPreference();
});

const toggleArea = (areaId: string) => {
  if (visibleAreas.value.has(areaId)) {
    visibleAreas.value.delete(areaId);
  } else {
    visibleAreas.value.add(areaId);
  }
};

const isAreaVisible = (areaId: string): boolean => {
  return visibleAreas.value.has(areaId);
};

// ID del operario actual
const currentOperatorId = computed(() => operatorStore.operator?.id);

// Toggle del filtro de mis workcenters
const toggleMyWorkcenters = () => {
  showOnlyMyWorkcenters.value = !showOnlyMyWorkcenters.value;
  saveFilterPreference();
};

// Verificar si un workcenter pertenece al operario actual
const isMyWorkcenter = (view: WorkcenterViewState): boolean => {
  if (!currentOperatorId.value) return false;
  return (
    view.realtime?.operators?.some(
      (op) => op.operatorId === currentOperatorId.value
    ) ?? false
  );
};

// Verificar si un área tiene workcenters del operario actual
const hasMyWorkcentersInArea = (areaId: string): boolean => {
  if (!currentOperatorId.value) return false;

  return realtimeStore.areasWorkcentersView
    .filter((view) => view.config.areaId === areaId)
    .some((view) => isMyWorkcenter(view));
};

// Obtener workcenters filtrados de un área
const getFilteredWorkcenters = (areaId: string): WorkcenterViewState[] => {
  const workcenters = realtimeStore.areasWorkcentersView.filter(
    (view) => view.config.areaId === areaId
  );

  if (!showOnlyMyWorkcenters.value || !currentOperatorId.value) {
    return workcenters;
  }

  return workcenters.filter((view) => isMyWorkcenter(view));
};

// Obtener workcenters de un área usando el getter combinado (LEGACY - mantener por compatibilidad)
const getAreaWorkcenters = (areaId: string): WorkcenterViewState[] => {
  return getFilteredWorkcenters(areaId);
};

// Áreas filtradas (solo mostrar áreas que tienen workcenters visibles)
const filteredAreas = computed(() => {
  return dataStore.areas.filter((area) => {
    const workcenters = getFilteredWorkcenters(area.id);
    return workcenters.length > 0;
  });
});

// Contador de mis workcenters
const myWorkcentersCount = computed(() => {
  if (!currentOperatorId.value) return 0;
  return realtimeStore.areasWorkcentersView.filter((view) =>
    isMyWorkcenter(view)
  ).length;
});

// Total de workcenters
const totalWorkcentersCount = computed(() => {
  return realtimeStore.areasWorkcentersView.length;
});
</script>

<style scoped>
.plant-layout {
  padding: 0.5rem;
  min-height: 100vh;
}

/* Barra de Filtros */
.filters-container {
  position: sticky;
  top: 0;
  z-index: 10;
  background: var(--p-surface-ground);
  padding: 1rem 0.5rem;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  border-bottom: 2px solid var(--p-surface-border);
}

.filter-chip {
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-chip:hover {
  transform: scale(1.05);
}

.filter-chip.filter-active {
  background: var(--p-green-100);
  color: var(--p-green-900);
  border-color: var(--p-green-500);
}

.filter-summary {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-color-secondary);
  font-size: 0.9rem;
}

.filter-summary i {
  color: var(--p-primary-500);
}

.area-section {
  margin-bottom: 1.5rem;
}

/* Area Header */
.area__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: linear-gradient(135deg, var(--p-primary-500), var(--p-primary-600));
  color: white;
  padding: 0.85rem 1.25rem;
  border-radius: 10px;
  cursor: pointer;
  user-select: none;
  margin-bottom: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

.area__header:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15);
}

.area__header-content {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.area__icon {
  font-size: 1.35rem;
  opacity: 0.9;
}

.area__name {
  font-size: 1.25rem;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.area__count {
  background: rgba(255, 255, 255, 0.25);
  padding: 0.25rem 0.65rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
}

.area__badge {
  margin-left: 0.5rem;
}

.area__toggle-icon {
  font-size: 1rem;
  transition: transform 0.3s ease;
}

/* Workcenters Grid */
.area__workcenters {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  animation: fadeIn 0.4s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Responsive */
@media (max-width: 1400px) {
  .area__workcenters {
    grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  }
}

@media (max-width: 768px) {
  .plant-layout {
    padding: 0.5rem;
  }

  .area__header {
    padding: 0.75rem 1rem;
  }

  .area__name {
    font-size: 1.1rem;
  }

  .area__workcenters {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .filters-container {
    flex-direction: column;
    align-items: flex-start;
  }

  .filter-fab {
    bottom: 1rem;
    right: 1rem;
    width: 3.5rem;
    height: 3.5rem;
  }
}

/* Floating Action Button */
.filter-fab {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  width: 4rem;
  height: 4rem;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  z-index: 100;
  transition: all 0.3s ease;
}

.filter-fab:hover {
  transform: scale(1.1);
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.3);
}
</style>
