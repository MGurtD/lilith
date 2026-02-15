<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount, computed } from "vue";
import { useI18n } from "vue-i18n";
import { PrimeIcons } from "@primevue/core/api";

const props = defineProps<{
  modelValue?: string | null;
  placeholder?: string;
}>();
const emit = defineEmits<{
  (e: "update:modelValue", value: string | null): void;
}>();

// Collect PrimeIcons keys (those starting with 'PI')
const iconKeys = Array.from(
  new Set(
    Object.keys(PrimeIcons)
      .filter((k) => typeof (PrimeIcons as any)[k] === "string")
      .map((k) => (PrimeIcons as any)[k])
  )
).sort();

const search = ref("");
const internal = ref<string | null>(props.modelValue ?? null);
const open = ref(false);
const rootEl = ref<HTMLElement | null>(null);
const { t } = useI18n();

watch(
  () => props.modelValue,
  (v) => {
    internal.value = v ?? null;
  }
);

const filtered = computed(() => {
  if (!search.value) return iconKeys;
  return iconKeys.filter((i) =>
    i.toLowerCase().includes(search.value.toLowerCase())
  );
});

const select = (val: string | null) => {
  internal.value = val;
  emit("update:modelValue", val);
  // close after selection
  open.value = false;
};

const onFocusInput = () => {
  open.value = true;
};

const handleClickOutside = (e: MouseEvent) => {
  if (!rootEl.value) return;
  if (!rootEl.value.contains(e.target as Node)) {
    open.value = false;
  }
};

onMounted(() => {
  if (!internal.value && iconKeys.length) internal.value = null;
  document.addEventListener("mousedown", handleClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener("mousedown", handleClickOutside);
});
</script>
<template>
  <div class="icon-picker" ref="rootEl">
    <span
      class="p-input-icon-left p-input-icon-right w-full"
      @focusin="onFocusInput"
    >
      <InputText
        v-model="search"
        class="w-full"
        :placeholder="internal || placeholder || t('menuItems.searchIcon')"
        @focus="onFocusInput"
        @keydown.escape="open = false"
      />
      <span v-if="internal" class="selected-icon-wrapper" :title="internal">
        <i :class="internal" class="selected-icon" />
        <Button
          type="button"
          size="small"
          class="ml-1 p-1"
          @click.stop="select(null)"
          :title="t('menuItems.none')"
        >
          <i class="pi pi-times" />
        </Button>
      </span>
    </span>
    <transition name="fade">
      <div v-if="open" class="icon-panel surface-overlay shadow-2">
        <div class="icon-grid">
          <div
            v-for="icon in filtered"
            :key="icon"
            :class="['icon-item', { active: icon === internal }]"
            @click="select(icon)"
            :title="icon"
          >
            <i :class="icon" />
          </div>
          <div
            class="icon-item none"
            :class="{ active: internal === null }"
            @click="select(null)"
            :title="t('menuItems.none')"
          >
            <span>{{ t("menuItems.none") }}</span>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>
<style scoped>
.icon-picker {
  position: relative;
  display: block;
}
.icon-panel {
  position: absolute;
  top: 110%;
  left: 0;
  width: 100%;
  background: var(--p-surface-card);
  border: 1px solid var(--p-surface-border);
  border-radius: 6px;
  padding: 6px;
  z-index: 1000;
}
.icon-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(42px, 1fr));
  gap: 6px;
  max-height: 240px;
  overflow: auto;
  padding: 4px;
}
.icon-item {
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  height: 38px;
  border: 1px solid transparent;
  border-radius: 4px;
  font-size: 1.1rem;
}
.icon-item.active {
  background: var(--p-primary-color);
  color: var(--p-primary-color-text);
}
.icon-item:hover {
  border-color: var(--p-primary-color);
}
.icon-item.none {
  font-size: 0.65rem;
  font-weight: 500;
}
.selected-icon-wrapper {
  position: absolute;
  right: 0.5rem;
  top: 50%;
  transform: translateY(-50%);
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
}
.selected-icon {
  font-size: 1.1rem;
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.12s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
