# Modul d'ajuda contextual

## Objectiu

Aquest modul implementa una ajuda contextual global al frontend de Lilith sense dependencies del backend. La resolucio del contingut es fa a partir de la ruta actual i del camp `route.meta.helpKey`.

## Abast actual

- Implementat nomes al frontend.
- Contingut inicial en catala a `src/help/ca`.
- Cobertura completa del modul `sales` per a les pantalles operatives principals.
- El `Drawer` global esta muntat a l'aplicacio, pero el boto del header s'ha retirat temporalment.
- L'obertura continua disponible via shortcut global `Alt + H`.
- Si el drawer esta obert i l'usuari navega, l'ajuda es resincronitza amb la nova ruta.
- El drawer nomes es munta dins del bloc autenticat.

### Rutes de `sales` actualment cobertes

- `Customers` -> `sales/customers/list`
- `Customer` -> `sales/customers/detail`
- `Budgets` -> `sales/budget/list`
- `Budget` -> `sales/budget/detail`
- `SalesOrders` -> `sales/salesorder/list`
- `SalesOrder` -> `sales/salesorder/detail`
- `DeliveryNotes` -> `sales/deliverynote/list`
- `DeliveryNote` -> `sales/deliverynote/detail`
- `SalesInvoices` -> `sales/salesinvoice/list`
- `SalesInvoice` -> `sales/salesinvoice/detail`
- `SalesInvoicesByDates` -> `sales/salesinvoice/by-period`
- `References` -> `sales/reference/list`
- `Reference` -> `sales/reference/detail`

### Cas especial

- `CustomerType` no te `helpKey` propi. La seva explicacio funcional viu dins l'ajuda de `Customers`, perque la gestio real d'aquest cataleg es fa des de la segona pestanya d'aquella pantalla.

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

El component es munta nomes quan hi ha usuari autenticat. Aixo evita que l'ajuda quedi visible sobre la pantalla de login despres d'un logout.

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
- Si el drawer ja esta obert i canvia la ruta, es recarrega el contingut contextual

## Estructura de contingut

Els fitxers d'ajuda viuen a:

```text
src/help/<locale>/<helpKey>.md
```

Exemples actuals:

```text
src/help/ca/sales/customers/list.md
src/help/ca/sales/budget/detail.md
src/help/ca/sales/salesorder/detail.md
src/help/ca/sales/deliverynote/list.md
src/help/ca/sales/salesinvoice/by-period.md
src/help/ca/sales/reference/detail.md
```

## Com afegir ajuda a una pantalla nova

1. Analitzar la vista real i els seus components abans d'escriure el Markdown.
2. Afegir `meta.helpKey` a la ruta, si la pantalla necessita ajuda propia.
3. Escollir un `helpKey` estable i coherent amb l'estructura existent.
4. Crear el fitxer Markdown a `src/help/ca/...`.
5. Si cal, afegir versions futures a `src/help/es/...` o `src/help/en/...`.
6. Validar que `Alt + H` obre el drawer a la pantalla correcta.
7. Revisar que el to, la terminologia i l'estructura siguin consistents amb la resta del modul.

## Regles de llenguatge i forma

Aquestes regles deixen de ser recomanacions i passen a ser el criteri base per a qualsevol ajuda nova o modificada.

### 1. Llengua base

- L'ajuda inicial s'escriu en catala.
- El llenguatge ha de ser funcional, clar i orientat a usuari final.
- S'han d'evitar textos tecnics interns, noms de stores, components o detalls d'implementacio que no aportin valor a l'usuari.

### 2. To

- To directe i professional.
- Frases curtes o mitjanes, faciles d'escanejar.
- Explicar per a que serveix la pantalla i com es fa servir, no descriure la interfície de forma superficial.
- Prioritzar accions, restriccions, bloquejos i errors habituals.

### 3. Estructura obligatoria

Tot fitxer d'ajuda ha de seguir aquest esquelet, en aquest ordre:

1. `# <Titol>`
2. `## Per a que serveix aquesta pantalla`
3. `## Accions disponibles`
4. `## Flux habitual`
5. `## Aspectes importants`
6. `## Errors frequents`
7. `## Proces basic`

### 4. Longitud i profunditat

- Evitar documentacio massa breu que nomes repliqui el nom de la pantalla.
- Cada ajuda ha d'explicar el flux real de treball.
- Si una pantalla te bloquejos per estat, dependencias amb altres documents o passos previs, s'han d'explicar explicitament.
- Si una pantalla es de proces massiu o te un comportament especial, s'ha de diferenciar clarament de la resta.

### 5. Terminologia funcional

- Mantenir terminologia constant a tot el modul.
- Fer servir els noms funcionals habituals del negoci: `pressupost`, `comanda`, `albara`, `factura`, `client`, `referencia`.
- Si una pantalla participa en un flux, cal contextualitzar-la dins del recorregut general quan aporti valor:

```text
pressupost -> comanda -> albara -> factura
```

- No canviar el mateix concepte entre fitxers amb sinonims arbitraris.

### 6. Contingut minim de qualitat

Cada document ha d'incloure, com a minim:

- objectiu funcional clar
- llista d'accions que realment pot fer l'usuari
- flux habitual realista
- aspectes importants o restriccions del comportament
- errors o dubtes habituals
- diagrama Mermaid senzill quan ajudi a entendre el proces

### 7. Criteris per a `Aspectes importants`

En aquesta seccio s'han de prioritzar:

- accions bloquejades per estat
- relacions amb altres documents
- diferencies entre creacio directa i creacio des de dialeg
- diferencies entre llistes operatives i pantalles de proces massiu
- casos especials on l'ajuda d'una subpantalla viu integrada dins una altra pantalla

### 8. Criteris per a `Errors frequents`

- Els errors han de ser accionables.
- Han d'indicar a l'usuari que comprovar primer.
- S'ha d'evitar redactar errors massa generics del tipus `hi ha hagut un error`.
- Si hi ha un bloqueig funcional habitual, s'ha de convertir en un punt explicit d'aquesta seccio.

### 9. Mermaid

- Fer servir diagrames senzills.
- Prioritzar fluxos lineals o amb poques branques.
- El diagrama ha de resumir el proces principal, no intentar representar tota la logica de negoci.

### 10. Consistencia editorial

- Reutilitzar els mateixos noms de seccio a tots els fitxers.
- Mantenir una longitud semblant entre documents del mateix nivell.
- Si una ajuda nova millora clarament el nivell de qualitat, s'ha de revisar la resta del modul per evitar que quedin documents antics amb un format inferior.

## Limitacions conegudes

- No hi ha backend ni analytics associats a l'ajuda en aquesta fase.
- El boto visible al header s'ha tret temporalment per no exposar la funcionalitat a usuari final.
- El multiidioma esta preparat a nivell d'arquitectura, pero el contingut inicial nomes existeix en `ca`.
- El sistema assumeix que els fitxers Markdown d'ajuda son contingut versionat i revisat dins del repo.
