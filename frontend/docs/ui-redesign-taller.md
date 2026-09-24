# UI Redesign "Taller"

> **Status**: In progress. Phases 0–4 done on `feat/ui-design-foundation`; not merged.
> **Created**: 2026-09-23 · **Owner**: Marc Gurt
> **Design canvas**: https://claude.ai/artifact/N7mbr8zHW3Lds7U1b7jfCL (private; share it from its Share menu)

## Executive Summary

Lilith's look came from PrimeVue Lara with a blue preset, a brand-coloured shell, an unloaded "Inter" font (users got the OS font), inverted table hierarchy, three coexisting filter systems and heavy `!important` overrides. "Taller" is a visual system for an industrial ERP whose tenants each bring their own branding: **steel neutrals everywhere, the tenant's colour only for actions, active state and focus**, dense tables with tabular figures, and one rule for where screens save.

Each step was validated with screenshots of the real running app (see *Visual verification*), not only with mockups.

## What Is Done

| Commit | Change |
|---|---|
| `3290c77` | Aura-based `TallerPreset` (`src/theme/preset.ts`): steel surface scale, primary on shade 600 (700 for emerald/teal/orange so white text reaches 4.5:1, see `store/branding.ts`), 4px radii, denser form/table tokens. IBM Plex Sans + Plex Sans Condensed self-hosted via Fontsource and cached by the PWA service worker. 14px root for the office UI; `/plant` keeps 16px (`plant-mode` class in `App.vue`) so shop-floor touch targets do not shrink. Currency/number columns right-aligned in `Table.vue`. Sub-11px font sizes raised. |
| `15d8250` | Sidebar "Grafit": neutral dark steel, branding colour only on the current-screen marker (white for the black palette). Replaced ~300 lines of runtime contrast maths in `TheSidebar.vue` with `vue-sidebar-menu` CSS variables. Only an uploaded *sidebar* logo is shown; otherwise the monogram. |
| `f2f75eb` | Light header: readable page title, owning module (from the sidebar menu tree) above it, visible help button (Alt+H still works), accessible back button and avatar. New i18n keys `ui.back`, `ui.userMenu`. |
| *(mobile shell)* | Below 768px (`composables/useIsPhone.ts`, same breakpoint as `Form.vue`) the sidebar menu moves into a PrimeVue `Drawer` opened from a menu button in the header; header and content take the full width. The drawer closes on navigation, Escape or widening the window. Page actions show their icon only on phones (label kept for screen readers). The current-screen marker now follows third-level entries (module › group › screen) on desktop too. New i18n key `ui.openMenu`. |
| *(user in sidebar)* | The signed-in user (or the clocked-in plant operator) moves from the header to the foot of the sidebar; its menu (language, sign out; "Sortir" for operators) opens above it. The collapse control moves to the brand row, and when collapsed it is the first row of the rail (option A in the canvas); hidden on phones and for plant operators (`hideToggle`). New i18n keys `ui.collapseMenu`, `ui.expandMenu`. |
| *(login and clock-in)* | Option A2 from the canvas. Office login: tenant logo, form and language picker on white (4px brand rule on top, "Zenith ERP, de Rawcraft Software" credit); a shaft drawing inked in the tenant colour and a title block (`components/brand/TitleBlock.vue`, logo + brand rule) on the right; the panel hides below 1024px. Operator clock-in (tablet): masked code field that keeps focus for barcode scanners, 72px numeric keypad ("ABC" opens the on-screen keyboard), inline "operator not found" instead of a toast, and a panel with the time in the tenant colour and the title block; the panel hides below 900px. |
| *(semantic states)* | Lifecycle statuses show as `Tag`s in lists (`ColumnType.Status` in `Table.vue`, `severity` from `lifecycleStore.getStatusColorById`; 11 lists plus work order phases). The colour is the status's own `Color` (option A; lifecycle tags are behavioural and a status may have several). The status form picks colours by meaning (Neutre, En curs, Cal actuar, Fet, Problema, Tancat) over the same stored PrimeVue severities; "help" is retired (Aura has none, it showed the tenant colour) and unknown values render neutral. Preset tag tokens: darker text, amber for `warn`, `contrast` as a quiet outline. Boolean columns show ✓ / a quiet dash instead of radio-like circles. Status colours for the Default database were set through the API with the approved mapping. |
| `f937c65` | **Save convention** (below) applied to all 40 detail screens plus branding. `components/PageActions.vue`, `Form.vue` `page-actions` option. Removed `grid_add_row_button` (a `position: fixed` save that overlapped fields). |

## Conventions Now In Force

- **Screens save from the header; dialogs from their footer.** Wrap a screen's Save (or Save `SplitButton`, secondary actions in its menu) in `PageActions`, or pass `page-actions` to `Form.vue`. No Cancel on screens: the header's back button leaves. A form reused in a dialog takes `inDialog` and renders `<PageActions :inline="inDialog">` at the end of the form (e.g. `FormWorkmasterPhase`, `FormWorkOrderPhase`, `FormMaterial`). `PageActions` hides itself inside inactive tabs. Also recorded in `frontend/AGENTS.md` and the `frontend-form` skill.
- Colours come from preset tokens (`--p-surface-*`, `--p-steel-*`, `--p-primary-*`); no hex in components.
- Density comes from tokens, not `!important`.
- Nothing below 11px of rendered text in the office UI.

## Next Phases

Ordered from lower to higher impact. Validate each with screenshots before committing.

1. **Create button.** Replace the green round `+` (`severity="success"`, 16 uses) with a labelled primary action ("Nou client") in list screens.
2. **Page patterns.** One `FilterBar` replacing `.two-columns…`, `.datatable-filter-*` and `.filter-toolbar`; empty states; a useful Home instead of the big logo.
3. **Caixetí (signature element).** Title-block header for document detail screens (number, customer, dates, status, totals). Pilot on `SalesInvoice`. Mockup in the canvas.
4. Later: dark mode (tokens are ready), unsaved-changes indicator and Ctrl+S.

## Known Issues And Pending Checks

- **Verified against real data** (local backend on the Default database, every write blocked in Playwright): purchase order, work order, work master, receipt, budget, sales order, sales invoice, delivery note and purchase invoice details render with no page errors; phase forms follow both conventions (work order and work master: "new phase" dialog keeps Save in its footer, phase detail shows it in the header); header Save on a work order issues `PUT /api/WorkOrder/{id}`. The material form inside the receipt line dialog keeps its Save in the dialog footer and does not reach the header.
- The "Maximum recursive updates exceeded in DataTable" seen on work orders only happens with mocked data.
- Save labels are inconsistent between modules ("Desar" in purchases, "Guardar" in sales/production); pre-existing translations, unify when touching i18n.
- On phones the shell fits, but most legacy screens keep their desktop grids (three-column forms, wide tables with horizontal scroll). Screens built on `Form.vue` rows already have mobile spans; the rest adapt screen by screen.
- `LanguageSwitcher` shows the language code ("ca") only when `/language` returns nothing (seen with the mocked API); with the backend's languages it shows their names.
- On detail routes (`/customers/:id`) the sidebar loses the current-screen highlight (`vue-sidebar-menu` only matches menu routes).
- The PWA cannot reload offline (`navigateFallback: null`); fonts are cached, navigation is not. Separate decision.
- The product is **Zenith ERP** by Rawcraft Software; "Lilith" is only the repository name. Temges is a tenant. `DEFAULT_BRAND_NAME` in `src/config/branding.ts` is still "Temges" (shown when a tenant has no branding); decide whether it should become "Zenith ERP".
- `RegisterForm.vue` keeps its old card styling; it is unreachable (the register link in `LoginForm.vue` is commented out).
- `pnpm install` also bumped `@babel/*` 7.28 → 7.29 in the lockfile (it was out of sync).

## Visual Verification

`scripts/visual-shots.mjs` renders real screens against a mocked API, no backend needed:

```bash
pnpm exec vite --port 5199 --strictPort --host 127.0.0.1   # in another terminal
node scripts/visual-shots.mjs shots /customers /budget/b0
W=390 H=844 node scripts/visual-shots.mjs shots-mobile /customers
W=1024 H=768 node scripts/visual-shots.mjs shots-plant /plant/clockin
BRAND=orange node scripts/visual-shots.mjs shots-orange /customers
CLICK="#page-actions button" node scripts/visual-shots.mjs save /budget/b0   # then read save/api-calls.txt
NAVFROM=/workmaster/x node scripts/visual-shots.mjs phase /workmaster/x/phase/p1
```

Against real data: run the backend (`dotnet run --project src/Api --launch-profile Api`, HTTPS on 7284) and start Vite with `VITE_API_BASE_URL=https://localhost:7284/api`. The `Default` connection string points at a remote database, so any Playwright run must intercept and block `POST/PUT/PATCH/DELETE` (except `/Authentication/*`) and pass credentials only through environment variables.

For a before/after comparison, run the same command on the previous commit (e.g. `git stash` or a checkout of the files) and compare. Page errors that also appear on the baseline come from the mock data.
