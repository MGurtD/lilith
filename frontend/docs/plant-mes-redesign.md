# Plant MES Redesign

> **Status**: Phases 1–6 done on `feat/plant-mes-redesign`.
> **Created**: 2026-09-26 · **Owner**: Marc Gurt
> **Design canvas**: https://claude.ai/artifact/N7mbr8zHW3Lds7U1b7jfCL, rows "Planta: com és avui", "Planta: sistema" and "Planta: proposta MES" (private; share it from its Share menu)
> **Related**: `ui-redesign-taller.md` (the Taller system this extends), issue #153 (raw DataTables)

## Goal

Make the shop-floor module (`frontend/src/modules/plant`) a professional MES for tablets next to the machine and for phones, consistent with the Taller system. One rule is added to Taller for the plant: **the machine status is the strongest signal on screen; everything else is steel.**

## Design Rules (Taller at the plant)

- **Machine status is a solid signal.** It uses the status's own catalogue colour (`statusColor`) with automatic ink: white when it reaches 4.5:1, otherwise Acer (`#1C2126`). Document lifecycle statuses keep the tinted `Tag`, so live state and paperwork never look alike.
- **The brand colour is only for actions** (Declarar peces, Carrega, Entra a la màquina), active tab and focus. Area headers and chrome are steel.
- **Touch sizes**: primary actions 56px, secondary 48px, minimum 44px, 8px between targets. Plant keeps the 16px root (`plant-mode`).
- **Type for reading at two metres**: IBM Plex Sans Condensed 600 with tabular figures for machine names and timers (placa timer 54/56, tile time 22/26, machine name 19/23), Plex Sans 16/24 for data, Condensed 500 13/16 for cell labels. No monospace timers.
- **Time**: under an hour `38 min`; under a day `4 h 20 min`; a day or more `46 d 8 h`; the placa shows a live `hh:mm:ss` up to 99 hours. The device ticks every second; the server only corrects. No data shows `—`, never an invented timer.
- **No data**: a hatched grey band and `—`.
- **The placa** (signature element): the machine's identification plate, sibling of the caixetí. Status and timer on the left, order, phase and pieces in the middle, operators on the right. Always on top, also on phones.

## Verification

Every task is verified in the running app before it is ticked:

- `pnpm run typecheck`, `pnpm run i18n:check`, and `pnpm run build` for shared or broad changes.
- `lilith-ui-tester` against the Vite dev server (`pnpm run dev`, port 8100) and the local API, at **tablet 1024x768** and **phone 390x844**, with screenshots. Never against `vite preview` (it targets production).
- Runs are read-only unless a task says otherwise: plant actions (status change, clock-in, load/unload phase, declare quantities) write to the staging database and must be authorised per run.
- Commit once per phase (decided 2026-09-26). Phases 1–5 were committed afterwards in four commits grouped by file, because those phases touched the same files.
- A write-enabled UI run on staging is not required (decided 2026-09-26): what only a consistent loaded phase can show (phase activities, time metrics, the declare dialog's context line, the close-phase dialog) stays listed as not observed.

## Tasks

### Phase 1: Fix the problems found

- [x] **1.1 Workcenter tile timer.** The area tile's elapsed time only moves when a WebSocket message arrives and shows absurd values (`1112:30:09`, `17757232:52:20`). Add a shared duration formatter (the time rules above) and a shared once-per-second clock; tiles tick locally, and a tile with no realtime data shows `—`.
- [x] **1.2 Touch targets.** Bottom action buttons measure 42px and header icon buttons 40px. Raise them to the touch sizes above.
- [x] **1.3 Accessible tiles and area headers.** Workcenter tiles and area headers are clickable `div`s. Make them real buttons with accessible names (machine, status, time).
- [x] **1.4 Hardcoded text.** Replace the Catalan literals left in `SiteAreas.vue` and `WorkcenterCard.vue` ("Sense dades", counts, labels) with i18n keys in `ca`, `es`, `en` (new keys in English camelCase under `plant.*`).
- [x] **1.5 Dead code.** Remove `WorkcenterProduction.vue` after confirming nothing imports it.
- [x] **1.6 Phone detail loses information.** On phones the detail hides status, order and operators. Solved by the placa in phase 4 (task 4.1); until then it is tracked here.

### Phase 2: Plant foundations

- [x] **2.1 Status signal helper.** One helper for the solid machine-status signal (fill + automatic ink) and the hatched no-data style, used by tiles, placa and status buttons.
- [x] **2.2 Operator shell.** Operators on `/plant` do not see the office sidebar. A plant header shows the site, current shift, clock and the operator menu (sign out).

### Phase 3: Areas view

- [x] **3.1 Status filter chips** with counts (All, Running, Stopped, No data) plus the "only mine" switch (replaces the floating filter button). Running/stopped come from the status flags (`statusStopped`, `statusClosed`).
- [x] **3.2 Area headers in steel** with the area name, machine count and a strip of status lights; collapsible, state remembered as today.
- [x] **3.3 Tiles that show only what exists**: idle tiles show name, status signal and time in status; running tiles add order, phase, reference, planned pieces and operators (the realtime snapshot has no pieces done yet, so the tile shows planned only).
- [x] **3.4 Phone rows**: one 72px row per machine (status light, name, status and order, time).

### Phase 4: Workcenter detail

- [x] **4.1 The placa** on tablet and phone (fixes 1.6).
- [x] **4.2 Tabs**: new "Fase actual" (estimated vs real machine and operator time, over-estimate warning); "Fases disponibles" as readable rows with a "Carrega" button; Documentació, Comentaris, Materials.
- [x] **4.3 Action dock**: status buttons from the phase (current one filled with its colour), "Altres estats", "Declarar peces" (brand) and "Finalitzar fase", 56px. Clock in/out moves to the placa ("Entra a la màquina").
- [x] **4.4 Idle machine**: the placa explains "Cap fase carregada"; the phase list is the main action; declare and finish are hidden, not disabled.
- [x] **4.5 Phone layout**: compact placa, operator row, scrollable tabs, bottom dock (Estat, Declarar, Més) with bottom sheets.

### Phase 5: Declare pieces

- [x] **5.1 Declare dialog**: good and bad counters with 72px keys, +5 and +10, reasons for bad pieces (optional and splittable across reasons: decided 2026-09-26, the mockup's "required" is dropped), a submit label that says what it will do ("Declarar 5 bones i 1 dolenta"). Used by "Afegir quantitat" and by the phase close flow.

### Phase 6: Tables and cleanup

- [x] **6.1** Replace the raw `DataTable`s of `WorkOrderLoader.vue` and `AvailableStockDialog.vue` (see #153): the stock dialog uses `Table.vue` with phone cards; the phase picker, a single-selection list that `Table.vue` cards cannot do yet (gap G3 in #153), becomes touch radio rows.
- [x] **6.2** Migrate the legacy `plant.*` sentence-slug i18n keys touched by this work to English camelCase keys.
- [x] **6.3** Update contextual help for the plant screens and record the plant rules in `ui-redesign-taller.md`.

## Log

| Date | Task | Notes |
|---|---|---|
| 2026-09-26 | — | Document created; branch `feat/plant-mes-redesign` from `dev` (`cf2dbb7`). |
| 2026-09-26 | 1.1–1.5 | `utils/elapsed.ts` (time rules) and `composables/useNow.ts` (one shared 1s clock); tiles and the "Estat actual" panel tick locally, pre-2000 start times (DateTime.MinValue) show `—`. Tiles and area headers are buttons (`aria-expanded` on headers). Action buttons 56px, plant header icon buttons and search 44px (`.plant-mode` in `styles.scss`). New keys `plant.areas.*`, `plant.workcenterTile.*`. Removed `WorkcenterProduction.vue` and the unused `calculateDuration`. UI test PASS at 1024x768 and 390x844 (read-only). |
| 2026-09-26 | 2.1–2.2 | `utils/statusSignal.ts` (solid fill, WCAG ink, hatched no-data) used by the area tiles. `PlantHeader.vue` replaces the office header and sidebar in `App.vue` while an operator is clocked in on `/plant` (not on clock-in): back, "Planta" + title, shift, clock, operator popover with "Sortir". Keys `plant.shell.*`. UI test PASS at both viewports (read-only). |
| 2026-09-26 | 3.1–3.4 | `SiteAreas.vue` rewritten: sticky toolbar with status chips and the "only mine" switch (replaces the FAB), steel area headers (`h2` + disclosure button, status lights with a count summary), grid of tiles on tablets and 72px rows on phones (`WorkcenterCard` `compact`). A status missing from the catalogue counts as no data in both the tile and the filter. Plural "peça/peces previstes". UI test PASS at both viewports (read-only). |
| 2026-09-26 | 4.1–4.5 | `WorkcenterPlaca.vue` (tablet and `compact` phone) replaces `WorkcenterRealtimePanel` and its four children, now removed, together with the unused `getStatusCardStyle`, `getBorderTopStyle` and `formatDuration`. New `PhaseTimeSummary.vue` ("Fase actual"); `WorkcenterWorkOrderSelector.vue` rewritten as rows with a load button (no raw DataTable). Dock with status keys (current one filled, disabled) and the phase group only when a phase is loaded; phone dock with bottom sheets (PrimeVue `Drawer`). Keys `plant.placa.*`, `plant.detail.*`. UI test PASS at both viewports (read-only). Not observed on staging: phase activities and time metrics, because `POST /api/WorkOrder/Loaded` returns no phases for the only machine with an order loaded (already the case before this work); verify with a machine whose phase data is consistent. |
| 2026-09-26 | 5.1 | `PhaseQuantityForm.vue` rewritten (same props/emits, so the close-phase dialog gets it too): 72px − / + keys, typed field, +5, +10, reset. `WorkOrderPhaseQuantities.vue`: title and context line, submit label with plurals ("Declarar 8 bones i 1 dolenta"), 56px footer, stacked on phones. Rejection controls 48px. Keys `plant.declare.*`. UI test PASS at both viewports (counters only, nothing submitted). Not observed: the context line (needs the loaded work order, empty on staging) and the close-phase dialog (opening it counts as a write for the tester's guard). |
| 2026-09-26 | 6.1–6.3 | Load dialog: header with context, radiogroup rows (other machine types locked and explained), 56px activity select and "Carrega l'activitat", hardcoded Catalan warnings moved to `plant.loader.*`. Stock dialog on `Table.vue` with `DimensionChips`, `#card-actions` and 48px controls (not observed: no phase with materials on staging). 21 legacy keys renamed to `plant.messages.*` / `plant.stock.*` and 54 plant keys left unused by the redesign removed (0 unused plant keys in `i18n:audit`). Help `plant/areas` and `plant/workcenter/detail` (ca; other locales fall back) with `meta.helpKey` on both routes. Load dialog UI test PASS at both viewports (opened only, nothing loaded). |
