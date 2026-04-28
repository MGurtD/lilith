# Modul d'ajuda contextual

## Objectiu

Aquest modul implementa una ajuda contextual global al frontend de Lilith sense dependencias del backend. La resolucio del contingut es fa a partir de la ruta actual i del camp `route.meta.helpKey`.

## Abast actual

- Implementat nomes al frontend.
- Contingut inicial en catala a `src/help/ca`.
- Pilot activat per a rutes de vendes:
  - `Customers`
  - `Customer`
  - `SalesOrders`
  - `SalesOrder`
- El `Drawer` global esta muntat a l'aplicacio, pero el boto del header s'ha retirat temporalment.
- L'obertura continua disponible via shortcut global `Alt + H`.
- Si el drawer esta obert i l'usuari navega, la ajuda es resincronitza amb la nova ruta.
- El drawer nomes es munta dins del bloc autenticat.

## Peces principals

### 1. Metadades de ruta

Tipatge ampliat a `src/types/vue-router.d.ts`:

```ts
interface RouteMeta {
  public?: boolean;
  roles?: string[];
  helpKey?: string;
}
```

Cada pantalla amb ajuda ha d'afegir un `meta.helpKey` a la seva definicio de ruta.

### 2. Store global

Fitxer: `src/store/help.ts`

Responsabilitats:

- Controlar l'estat del drawer.
- Carregar el Markdown segons ruta i idioma.
- Fer fallback a `ca` si no existeix el fitxer de l'idioma actual.
- Gestionar missatges d'error o absencia de documentacio.
- Evitar sobrescriptures d'estat si hi ha carregues consecutives.

Estat principal:

- `visible`
- `loading`
- `key`
- `markdown`
- `error`

Accions principals:

- `openForRoute(helpKey?: string)`
- `toggleForRoute(helpKey?: string)`
- `close()`
- `reset()`

### 3. Drawer global

Fitxer: `src/components/help/HelpDrawer.vue`

Es renderitza una sola vegada a `src/App.vue` i mostra:

- estat de carrega
- error amable
- contingut Markdown renderitzat

El component es munta nomes quan hi ha usuari autenticat. Això evita que la ajuda quedi visible sobre la pantalla de login despres d'un logout.

### 4. Renderitzat Markdown + Mermaid

Fitxer: `src/components/help/MarkdownRenderer.vue`

Decisions tecnologiques:

- `markdown-it` per parsejar Markdown
- `DOMPurify` per sanejar l'HTML generat a partir del Markdown
- `mermaid` per renderitzar diagrames en client

Flux de renderitzat:

1. Es parseja el Markdown.
2. Els blocs ```mermaid``` es substitueixen per placeholders.
3. L'HTML general es saneja amb `DOMPurify`.
4. Mermaid genera l'SVG al client i s'injecta directament al placeholder.

Nota: es va provar de sanejar l'SVG de Mermaid amb `DOMPurify`, pero els labels interns deixaven de veure's. La causa es que Mermaid utilitza parts d'HTML incrustat dins l'SVG, especialment en `foreignObject`, per renderitzar alguns textos. Per aquest motiu, el sistema saneja el Markdown d'entrada pero no torna a sanejar l'SVG que genera Mermaid. La seguretat es recolza en dos punts: `securityLevel: "strict"` de Mermaid i el fet que els fitxers d'ajuda son contingut versionat dins del repo.

### 5. Shortcut global

Implementat a `src/App.vue` amb listener global de `keydown`.

Regles actuals:

- Obre o tanca l'ajuda amb `Alt + H`
- Ignora la combinacio si el focus es en `input`, `textarea`, `select` o contingut editable
- Usa la ruta actual per resoldre `meta.helpKey`
- Si el drawer ja esta obert i canvia la ruta, es recarrega el contingut contextual.

## Estructura de contingut

Els fitxers d'ajuda viuen a:

```text
src/help/<locale>/<helpKey>.md
```

Exemples actuals:

```text
src/help/ca/sales/customers/list.md
src/help/ca/sales/customers/detail.md
src/help/ca/sales/salesorder/list.md
src/help/ca/sales/salesorder/detail.md
```

## Com afegir ajuda a una pantalla nova

1. Afegir `meta.helpKey` a la ruta.
2. Crear el fitxer Markdown a `src/help/ca/...`.
3. Si cal, afegir versions futures a `src/help/es/...` o `src/help/en/...`.
4. Validar que `Alt + H` obre el drawer a la pantalla.

## Criteris de contingut recomanats

Cada fitxer Markdown hauria de contenir com a minim:

- titol
- per a que serveix la pantalla
- accions disponibles
- flux habitual
- errors frequents
- un diagrama Mermaid senzill si aporta valor

## Limitacions conegudes

- No hi ha backend ni analytics associats a l'ajuda en aquesta fase.
- El boto visible al header s'ha tret temporalment per no exposar la funcionalitat a usuari final.
- El multiidioma esta preparat a nivell d'arquitectura, pero el contingut inicial nomes existeix en `ca`.
- El sistema assumeix que els fitxers Markdown d'ajuda son contingut versionat i revisat dins del repo.
