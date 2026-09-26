<template>
  <Dialog
    :visible="menuSearch.visible"
    modal
    dismissable-mask
    position="top"
    :show-header="false"
    :draggable="false"
    class="menu-search"
    :pt="{
      root: { 'aria-label': t('ui.menuSearch.dialogLabel') },
      mask: { class: 'menu-search-mask' },
      content: { class: 'menu-search__body' },
    }"
    @update:visible="onVisibleChange"
  >
    <div class="menu-search__bar">
      <i class="pi pi-search menu-search__bar-icon" aria-hidden="true" />
      <label :for="inputId" class="menu-search__sr-only">{{
        t("ui.menuSearch.dialogLabel")
      }}</label>
      <input
        :id="inputId"
        v-model="query"
        autofocus
        type="text"
        autocomplete="off"
        spellcheck="false"
        class="menu-search__input"
        :placeholder="t('ui.menuSearch.placeholder')"
        role="combobox"
        aria-expanded="true"
        aria-autocomplete="list"
        :aria-controls="listId"
        :aria-activedescendant="activeDescendant"
        @keydown="onKeydown"
      />
      <button
        v-if="isPhone"
        type="button"
        class="menu-search__close"
        @click="menuSearch.close()"
      >
        {{ t("ui.menuSearch.close") }}
      </button>
      <button
        v-else
        type="button"
        class="menu-search__kbd menu-search__esc"
        :aria-label="t('ui.menuSearch.closeLabel')"
        @click="menuSearch.close()"
      >
        Esc
      </button>
    </div>

    <template v-if="rows.length">
      <div class="menu-search__section" aria-hidden="true">
        {{
          trimmedQuery ? t("ui.menuSearch.results") : t("ui.menuSearch.recent")
        }}
      </div>
      <ul
        :id="listId"
        role="listbox"
        :aria-label="t('ui.menuSearch.listLabel')"
        class="menu-search__list"
      >
        <li
          v-for="(row, index) in rows"
          :id="optionId(index)"
          :key="row.entry.href"
          role="option"
          :aria-selected="index === activeIndex"
          class="menu-search__option"
          :class="{ 'menu-search__option--active': index === activeIndex }"
          @mousemove="activeIndex = index"
          @mousedown.prevent
          @click="openEntry(row.entry, $event.ctrlKey || $event.metaKey)"
          @auxclick="onAuxClick($event, row.entry)"
        >
          <span class="menu-search__marker" aria-hidden="true" />
          <i
            :class="row.entry.icon || 'pi pi-file'"
            class="menu-search__icon"
            aria-hidden="true"
          />
          <span class="menu-search__text">
            <span class="menu-search__title"
              ><template v-for="(segment, part) in row.segments" :key="part"
                ><mark v-if="segment.match" class="menu-search__match">{{
                  segment.text
                }}</mark
                ><template v-else>{{ segment.text }}</template></template
              ></span
            >
            <span v-if="row.aside" class="menu-search__aside">{{
              row.aside
            }}</span>
          </span>
          <svg
            v-if="index === activeIndex"
            class="menu-search__enter"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path d="M20 5v7a3 3 0 0 1-3 3H5M9 11l-4 4 4 4" />
          </svg>
        </li>
      </ul>
    </template>
    <div v-else class="menu-search__empty">
      <template v-if="trimmedQuery">
        <p class="menu-search__empty-title">
          {{ t("ui.menuSearch.noResults", { query: trimmedQuery }) }}
        </p>
        <p class="menu-search__empty-hint">
          {{ t("ui.menuSearch.noResultsHint") }}
        </p>
      </template>
      <p v-else class="menu-search__empty-hint">
        {{ t("ui.menuSearch.emptyHint") }}
      </p>
    </div>

    <div class="menu-search__footer">
      <span class="menu-search__hint">
        <kbd class="menu-search__kbd">↑</kbd
        ><kbd class="menu-search__kbd">↓</kbd>
        {{ t("ui.menuSearch.hints.navigate") }}
      </span>
      <span class="menu-search__hint">
        <kbd class="menu-search__kbd"
          ><svg
            class="menu-search__kbd-icon"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path d="M20 5v7a3 3 0 0 1-3 3H5M9 11l-4 4 4 4" /></svg
        ></kbd>
        {{ t("ui.menuSearch.hints.open") }}
      </span>
      <span class="menu-search__hint">
        <kbd class="menu-search__kbd"
          >{{ modifierLabel }}&nbsp;<svg
            class="menu-search__kbd-icon"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path d="M20 5v7a3 3 0 0 1-3 3H5M9 11l-4 4 4 4" /></svg
        ></kbd>
        {{ t("ui.menuSearch.hints.newTab") }}
      </span>
      <span role="status" class="menu-search__count">{{ countText }}</span>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { useIsPhone } from "@/composables/useIsPhone";
import { useStore } from "@/store";
import { useMenuSearchStore } from "@/store/menuSearch";
import {
  createMenuSearch,
  findOwningEntry,
  highlightTitle,
  type MenuSearchEntry,
  type TitleSegment,
} from "@/utils/menuSearch";

interface Row {
  entry: MenuSearchEntry;
  segments: TitleSegment[];
  aside: string;
}

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const store = useStore();
const menuSearch = useMenuSearchStore();
const isPhone = useIsPhone();

const inputId = "menu-search-input";
const listId = "menu-search-list";
const optionId = (index: number) => `menu-search-option-${index}`;
const modifierLabel = /Mac|iPhone|iPad/.test(navigator.userAgent)
  ? "⌘"
  : "Ctrl";

const query = ref("");
const activeIndex = ref(0);
const trimmedQuery = computed(() => query.value.trim());

const searcher = computed(() => createMenuSearch(menuSearch.entries));
const currentHref = computed(
  () => findOwningEntry(menuSearch.entries, route.path)?.href,
);

const toRow = (
  entry: MenuSearchEntry,
  ranges: Array<[number, number]>,
): Row => ({
  entry,
  segments: highlightTitle(entry.title, ranges),
  aside:
    entry.href === currentHref.value
      ? t("ui.menuSearch.currentScreen")
      : entry.trail.join(" › "),
});

// No query: the screens opened last, except the one on screen now.
const rows = computed<Row[]>(() => {
  if (trimmedQuery.value) {
    return searcher.value
      .search(trimmedQuery.value, menuSearch.recents)
      .map((result) => toRow(result.entry, result.ranges));
  }
  const byHref = new Map(
    menuSearch.entries.map((entry) => [entry.href, entry]),
  );
  return menuSearch.recents
    .filter((href) => href !== currentHref.value)
    .map((href) => byHref.get(href))
    .filter((entry): entry is MenuSearchEntry => !!entry)
    .map((entry) => toRow(entry, []));
});

const countText = computed(() =>
  trimmedQuery.value
    ? t("ui.menuSearch.resultCount", rows.value.length)
    : t("ui.menuSearch.recentCount", rows.value.length),
);

const activeDescendant = computed(() =>
  rows.value.length ? optionId(activeIndex.value) : undefined,
);

watch(
  () => menuSearch.visible,
  (visible) => {
    if (visible) {
      query.value = "";
      activeIndex.value = 0;
    }
  },
);

watch(query, () => (activeIndex.value = 0));

watch(activeIndex, async (index) => {
  await nextTick();
  document
    .getElementById(optionId(index))
    ?.scrollIntoView({ block: "nearest" });
});

const onVisibleChange = (visible: boolean) => {
  if (!visible) menuSearch.close();
};

const openEntry = (entry: MenuSearchEntry, newTab = false) => {
  menuSearch.close();
  if (newTab) {
    window.open(router.resolve(entry.href).href, "_blank", "noopener");
    return;
  }
  store.sidebar.mobileOpen = false;
  if (route.path !== entry.href) void router.push(entry.href);
};

const onAuxClick = (event: MouseEvent, entry: MenuSearchEntry) => {
  if (event.button !== 1) return;
  event.preventDefault();
  openEntry(entry, true);
};

const onKeydown = (event: KeyboardEvent) => {
  if (event.isComposing) return;
  const count = rows.value.length;
  switch (event.key) {
    case "ArrowDown":
      event.preventDefault();
      if (count) activeIndex.value = (activeIndex.value + 1) % count;
      break;
    case "ArrowUp":
      event.preventDefault();
      if (count) activeIndex.value = (activeIndex.value - 1 + count) % count;
      break;
    case "Enter": {
      event.preventDefault();
      const row = rows.value[activeIndex.value];
      if (row) openEntry(row.entry, event.ctrlKey || event.metaKey);
      break;
    }
  }
};
</script>

<style>
/* The dialog is teleported to <body>, so it is styled globally. */
.p-dialog-mask.p-overlay-mask.menu-search-mask {
  background: rgba(28, 33, 38, 0.32);
}

.p-dialog-mask .p-dialog.menu-search {
  width: min(640px, calc(100vw - 2rem));
  max-height: 76vh;
  margin: 12vh 1rem 1rem;
  overflow: hidden;
}

.menu-search .menu-search__body {
  display: flex;
  flex-direction: column;
  min-height: 0;
  padding: 0;
  overflow: hidden;
}

.menu-search__sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip-path: inset(50%);
  white-space: nowrap;
}

.menu-search__bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-shrink: 0;
  height: 4.2857rem;
  box-sizing: border-box;
  padding: 0 1rem 0 1.25rem;
  border-bottom: 1px solid var(--p-content-border-color);
}

.menu-search__bar-icon {
  font-size: 1.2857rem;
  color: var(--p-text-muted-color);
}

.menu-search__input {
  flex: 1;
  min-width: 0;
  padding: 0;
  border: 0;
  outline: none;
  background: transparent;
  font-family: var(--font-condensed);
  font-size: 1.4286rem;
  font-weight: 500;
  color: var(--p-text-color);
  caret-color: var(--p-primary-color);
}

.menu-search__input::placeholder {
  color: var(--p-form-field-placeholder-color);
  opacity: 1;
}

.menu-search__kbd {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.4286rem;
  height: 1.4286rem;
  box-sizing: border-box;
  padding: 0 0.3571rem;
  border: 1px solid var(--p-surface-300);
  border-radius: var(--p-border-radius-sm);
  background: var(--p-surface-0);
  color: var(--p-surface-700);
  font-family: inherit;
  font-size: 0.7857rem;
  font-weight: 500;
  line-height: 1;
}

.menu-search__kbd-icon {
  width: 0.8571rem;
  height: 0.8571rem;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.menu-search__esc {
  border-color: var(--p-surface-200);
  background: var(--p-surface-50);
  color: var(--p-text-muted-color);
  cursor: pointer;
}

.menu-search__esc:hover {
  background: var(--p-surface-100);
}

.menu-search__esc:focus-visible,
.menu-search__close:focus-visible {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 1px;
}

.menu-search__close {
  height: 44px;
  padding: 0 0.75rem;
  border: 0;
  border-radius: var(--p-border-radius-md);
  background: transparent;
  color: var(--p-surface-700);
  font: inherit;
  font-size: 1.0714rem;
  cursor: pointer;
}

.menu-search__section {
  flex-shrink: 0;
  padding: 0.75rem 1.125rem 0.25rem;
  font-size: 0.8571rem;
  color: var(--p-text-muted-color);
}

.menu-search__list {
  flex: 1;
  min-height: 0;
  margin: 0;
  padding: 0 0.375rem 0.375rem;
  overflow-y: auto;
  list-style: none;
}

.menu-search__option {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  min-height: 3.1429rem;
  box-sizing: border-box;
  padding: 0 0.75rem 0 0.625rem;
  border-radius: var(--p-border-radius-md);
  color: var(--p-text-color);
  cursor: pointer;
}

.menu-search__option--active {
  background: var(--p-highlight-background);
}

/* The sidebar marks the current screen with the same small square. */
.menu-search__marker {
  flex-shrink: 0;
  width: 5px;
  height: 5px;
  border-radius: 1px;
}

.menu-search__option--active .menu-search__marker {
  background: var(--p-primary-color);
}

.menu-search__icon {
  flex-shrink: 0;
  width: 1.25rem;
  text-align: center;
  font-size: 1.0714rem;
  color: var(--p-text-muted-color);
}

.menu-search__option--active .menu-search__icon {
  color: var(--p-primary-color);
}

.menu-search__text {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: baseline;
  gap: 1rem;
}

.menu-search__title {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: pre;
  font-size: 1.0714rem;
}

.menu-search__option--active .menu-search__title {
  color: var(--p-highlight-color);
}

.menu-search__match {
  background: transparent;
  color: var(--p-highlight-color);
  font-weight: 600;
}

.menu-search__aside {
  margin-left: auto;
  flex-shrink: 1;
  min-width: 0;
  max-width: 45%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 0.8929rem;
  color: var(--p-text-muted-color);
}

.menu-search__enter {
  flex-shrink: 0;
  width: 15px;
  height: 15px;
  fill: none;
  stroke: var(--p-primary-color);
  stroke-width: 1.75;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.menu-search__empty {
  padding: 1.75rem 1.25rem 1.625rem;
}

.menu-search__empty p {
  margin: 0;
}

.menu-search__empty-title {
  font-size: 1.0714rem;
  color: var(--p-text-color);
}

.menu-search__empty-hint {
  margin-top: 0.25rem !important;
  font-size: 0.8929rem;
  color: var(--p-text-muted-color);
}

.menu-search__footer {
  display: flex;
  align-items: center;
  gap: 1.125rem;
  flex-shrink: 0;
  padding: 0.5rem 1.125rem;
  border-top: 1px solid var(--p-content-border-color);
  background: var(--p-surface-50);
  font-size: 0.8571rem;
  color: var(--p-text-muted-color);
}

.menu-search__hint {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
}

.menu-search__hint .menu-search__kbd:last-of-type {
  margin-right: 0.125rem;
}

.menu-search__count {
  margin-left: auto;
}

/* Phones: a full-screen sheet; titles over their breadcrumb, no key hints. */
@media (max-width: 767.98px) {
  .p-dialog-mask .p-dialog.menu-search {
    width: 100vw;
    height: 100dvh;
    max-height: 100dvh;
    margin: 0;
    border: 0;
    border-radius: 0;
  }

  .menu-search__bar {
    padding: 0 0.25rem 0 1rem;
    border-bottom-color: var(--p-surface-200);
  }

  .menu-search__input {
    font-size: 1.3571rem;
  }

  .menu-search__option {
    min-height: 4.1429rem;
    padding-block: 0.375rem;
  }

  .menu-search__text {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.125rem;
  }

  .menu-search__title {
    max-width: 100%;
    font-size: 1.1429rem;
  }

  .menu-search__aside {
    margin-left: 0;
    max-width: 100%;
  }

  .menu-search__footer {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    overflow: hidden;
    clip-path: inset(50%);
    white-space: nowrap;
  }
}

@media (prefers-reduced-motion: reduce) {
  .menu-search-mask,
  .menu-search-mask .p-dialog {
    transition: none !important;
    animation: none !important;
  }
}
</style>
