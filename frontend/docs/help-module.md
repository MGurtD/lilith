# Contextual help module

## Purpose

Lilith ships end-user contextual help for every screen. It is frontend-only: no backend or analytics are involved. The content is versioned Markdown resolved from the current route's `route.meta.helpKey`.

Users open it with the `?` button in the page header or with `Alt + H`.

## Current scope

- Every authenticated screen route declares a `meta.helpKey`, and each key has a Markdown file in `ca`, `es` and `en`.
- Routes without their own help, on purpose:
  - `Login` is public, and the drawer is only mounted for authenticated users.
  - `Home` (`/`) and `MainPlant` (`/plant`) are a landing page and a redirect.
  - `CustomerType` (`/customer-types/:id`) is explained inside `sales/customers/list`, because customer types are managed from the second tab of the customers screen.
- Supplier types follow the same pattern: they are edited in a dialog inside the suppliers screen and are explained in `purchase/supplier/list`.
- `pnpm run help:check` enforces coverage, locale parity, headings and Mermaid syntax. Run it after any change to help files or to a route's `helpKey`.

## Main pieces

### 1. Route metadata

The typing is extended in `src/types/vue-router.d.ts`:

```ts
interface RouteMeta {
  public?: boolean;
  roles?: string[];
  helpKey?: string;
}
```

Each screen declares its key in its module's `routes.ts`. Several routes may share a key only when they render the same screen with the same behaviour.

### 2. Store

`src/store/help.ts` controls the drawer state, loads the Markdown for the route and locale, and handles missing or failing content. Only the latest request wins when loads overlap.

Resolution order for a key:

1. `src/help/<current locale>/<helpKey>.md`
2. `src/help/ca/<helpKey>.md` (fallback to the source culture)
3. The "no help available" message

A route without `helpKey` shows the "this screen has no help key" message.

### 3. Drawer and entry points

- `src/components/help/HelpDrawer.vue` is rendered once in `src/App.vue`, only for an authenticated user, so help never stays visible over the login screen after a logout.
- `src/components/TheHeader.vue` and `src/modules/plant/components/PlantHeader.vue` show the help button only when the route declares a `helpKey`. A declared key without a file therefore shows a button that leads to "not available"; `help:check` prevents that.
- `Alt + H` toggles the drawer from `src/App.vue`. The shortcut is ignored while the focus is in an `input`, `textarea`, `select` or editable content.
- If the drawer is open and the route changes, it reloads the help for the new route.

### 4. Markdown and Mermaid rendering

`src/components/help/MarkdownRenderer.vue` uses `markdown-it` to parse, `DOMPurify` to sanitize the generated HTML, and `mermaid` to render diagrams on the client.

1. The Markdown is parsed.
2. `mermaid` code blocks are replaced by placeholders.
3. The HTML is sanitized with `DOMPurify`.
4. Mermaid renders each SVG into its placeholder.

The Mermaid SVG is not sanitized again: Mermaid renders some labels as HTML inside `foreignObject`, and sanitizing removed them. Security relies on Mermaid's `securityLevel: "strict"` and on the help files being reviewed, versioned repository content.

## Content layout

```text
src/help/<locale>/<helpKey>.md
```

Keys follow `<module>/<entity>/<list|detail>`, with explicit names for special screens, for example `sales/salesinvoice/by-period`, `purchase/purchaseinvoice/import`, `production/workorder/phase` and `analytics/abc/customers`.

## Adding or changing help

1. Analyse the real screen before writing: the view, its owned components, dialogs and tabs, the store actions, the service calls and the backend service behind them (validations, lifecycle restrictions and returned error messages).
2. Add `meta.helpKey` to the route if it has none.
3. Write `ca` from that evidence, then `es` and `en` as adaptations that use each locale's own UI labels from `src/i18n/<locale>.ts`.
4. Run `pnpm run help:check`.
5. Open the screen, press `Alt + H` and check that the right document renders, including the diagram.
6. Review sibling files in the module for consistent terminology and depth.

The `contextual-help` skill in `.claude/skills/` holds the step-by-step procedure for agents.

## Writing rules

### Language and tone

- `ca` is the source culture. `es` and `en` carry the same content, phrased naturally rather than translated word for word.
- Write for end users: direct, professional, short to medium sentences that are easy to scan.
- Explain what the screen is for and how to work with it, not a superficial tour of the interface.
- Never mention components, stores, endpoints, database tables or other implementation details.
- Quote visible labels exactly as the UI shows them in that locale: «…» in `ca` and `es`, "…" in `en`.
- Use correct orthography in the body: Catalan accents and `l·l`, Spanish accents and `ñ`.

### Mandatory structure

Every file has exactly one H1 title followed by these H2 sections, in this order:

| # | `ca` | `es` | `en` |
|---|---|---|---|
| 1 | `Per a que serveix aquesta pantalla` | `Para qué sirve esta pantalla` | `What this screen is for` |
| 2 | `Accions disponibles` | `Acciones disponibles` | `Available actions` |
| 3 | `Flux habitual` | `Flujo habitual` | `Usual flow` |
| 4 | `Aspectes importants` | `Aspectos importantes` | `Important notes` |
| 5 | `Errors frequents` | `Errores frecuentes` | `Common errors` |
| 6 | `Proces basic` | `Proceso básico` | `Basic process` |

The Catalan headings keep their historical unaccented spelling because `help:check` matches them literally.

### What each section must contain

- **Purpose**: what the user achieves and where the screen sits in the business flow, for example `pressupost -> comanda -> albarà -> factura` or `ruta de fabricació -> ordre de fabricació -> fases -> declaració de peces`.
- **Available actions**: one bullet per real action, starting with a verb and naming the button or tab, including row actions and what each tab is for.
- **Usual flow**: four to seven numbered steps of a realistic task.
- **Important notes**: the most valuable section. Cover actions blocked by status, fields that become read-only, what is generated or updated automatically (numbering, stock, costs, linked documents), what delete really does, dependencies on configuration screens, the difference between direct creation and creation from a dialog, and cases where a sub-screen is explained inside another screen's help.
- **Common errors**: actionable items ("if X happens, check Y first") taken from real validations and backend messages. No generic "an error occurred".
- **Basic process**: one simple Mermaid `flowchart` with four to eight nodes that summarises the main process rather than every rule. Node labels avoid parentheses, quotes, colons and semicolons.

### Evidence over templates

- Document only behaviour confirmed in the current source. Never infer filters, statuses, permissions or workflows from a route name or from a sibling screen.
- Keep business terms consistent across modules and use the term the UI uses.
- Keep documents of the same level at a similar depth. When a new document clearly raises the bar, review its module siblings.
- Screens reserved for administrators (`meta.roles: ["Admin"]`) say so.

## Known limitations

- There is no backend storage, search or analytics for help content.
- Help is one document per route key. Tabs and dialogs are explained inside their screen's document, not as separate entries.
