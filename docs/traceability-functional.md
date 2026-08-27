# Traçabilitat de lots — Guia funcional

Guia orientada a l'usuari de negoci: què és la traçabilitat a Lilith, quins conceptes
maneja, com es configura i com s'utilitza al dia a dia. Per al detall tècnic (model de
dades, consultes, API), vegeu `backend/docs/traceability.md`.

---

## 1. Què és i per a què serveix

La traçabilitat permet seguir el rastre d'un material o producte al llarg de tota la seva
vida a l'empresa: des que es **compra**, passant per la seva **transformació** en fabricació,
fins que es **ven** a un client.

Respon preguntes com:

- "Aquesta taula que hem venut, ¿de quina fusta i de quin proveïdor prové?"
- "Aquest lot de fusta defectuós, ¿a quins productes ha anat i a quins clients?"
- "Si hem de fer una retirada (recall), ¿a quins clients hem d'avisar?"

És una eina clau per a **control de qualitat**, **gestió d'incidències** i **compliment
normatiu**.

---

## 2. Conceptes bàsics

### Lot

Un **lot** és una agrupació identificable d'unitats d'una mateixa referència que comparteixen
origen (una compra concreta, una fabricació concreta). Cada lot té un **codi** que el
distingeix.

- Els lots de **compra** identifiquen una recepció de material d'un proveïdor.
- Els lots de **fabricació** identifiquen el resultat d'una ordre de fabricació (OF).

### Referència loteada vs no loteada

No tot es controla per lot. Un cargol, cola o consumibles genèrics normalment **no**
necessiten lot. En canvi, matèries primeres crítiques (fusta, acer, components amb
caducitat…) sí.

Cada referència té un indicador **"Requereix lot"**:

- **Activat:** cada moviment d'aquesta referència queda associat a un lot i és traçable.
- **Desactivat:** la referència no es tracça per lot; els seus moviments no apareixen als
  informes de traçabilitat (és el comportament correcte per a material no crític).

> Aquest indicador és el que decideix si un material entra o no al sistema de traçabilitat.

### Traçabilitat cap enrere i cap endavant

- **Cap enrere (backward):** partint d'un producte fabricat o venut, es reconstrueix cap a
  l'origen: quins materials el componen i de quins proveïdors venen.
- **Cap endavant (forward):** partint d'un material comprat, es veu on ha acabat: quins
  productes s'hi han fabricat i a quins clients s'han venut.

### Informe de recall

A partir de la traçabilitat cap endavant, genera la llista de **clients i albarans
afectats** si un lot s'ha de retirar del mercat.

---

## 3. Configuració: marcar una referència com a loteada

A **Referències**, en editar una referència, hi ha la casella **"Requereix lot"**.

- Activeu-la per a materials i productes que voleu poder traçar per lot.
- Deixeu-la desactivada per a consumibles i material no crític.

> Recomanació: activeu-la només quan realment necessiteu la traça. Marcar-ho tot com a
> loteat genera lots sense valor i complica la gestió.

---

## 4. El dia a dia

### 4.1 Comprar / rebre material

En registrar una **recepció de compra**:

- Si la referència **requereix lot**, apareix el selector de lot: podeu triar-ne un
  d'existent o crear-ne un de nou (per exemple, el codi de lot del proveïdor).
- Si la referència **no requereix lot**, el selector no apareix; el material entra sense lot.

### 4.2 Aprovisionar i consumir en fabricació

Durant la fabricació:

- El material es **aprovisiona** al centre de treball (moviment d'entrada/sortida entre
  ubicacions).
- Es **consumeix** dins la fase de l'OF.

Aquests moviments queden associats al lot del material i són els que teixeixen la traça
entre el material consumit i el producte fabricat.

### 4.3 Fabricar (lot del producte acabat)

En crear una **ordre de fabricació** d'una referència que requereix lot:

- Si el sistema té activat el **codi automàtic de lot** (paràmetre `Production.AutoBatch`),
  el lot del producte s'assigna automàticament (codi de l'OF).
- Si el codi automàtic està **desactivat**, cal indicar un **codi de lot** al formulari de
  l'OF; el sistema no permet deixar-lo buit.

Si la referència **no** requereix lot, el producte fabricat no rep lot.

### 4.4 Vendre

En fer un **albarà de venda**, la sortida s'associa al lot del producte (el que es va
assignar en fabricació o el que s'indiqui). Aquest enllaç és el que permet, en un recall,
saber quins clients han rebut un lot concret.

---

## 5. Consultar la traçabilitat

A la vista **Traçabilitat de lots**:

1. Seleccioneu una **referència** i després un **lot**.
2. Escolliu la pestanya:
   - **Cap enrere** — des d'un lot fabricat/venut cap als seus orígens de compra.
   - **Cap endavant** — des d'un lot comprat cap als productes i clients.
3. L'arbre mostra, nivell a nivell:
   - Els **lots** implicats i la seva quantitat.
   - Els **orígens de compra** (proveïdor, número de rebut, data).
   - Els **destins de venda** (client, albarà, data).
   - Les **línies de moviment** de cada lot (aprovisionament, consum, producció…), amb una
     etiqueta de color segons el tipus, la ubicació i la data.

Per a un **recall**, premeu el botó **Informe de recall**: obté la llista de clients
afectats, amb els seus albarans i les quantitats totals implicades.

---

## 6. Preguntes freqüents

**¿Per què no veig un material a la traçabilitat?**
Perquè la seva referència no té activat "Requereix lot", o el moviment es va fer sense lot.
Els materials no loteats no formen part de la traça per disseny.

**¿Puc reutilitzar un codi de lot?**
Mentre el lot estigui obert, sí (s'hi acumula). Quan un lot s'esgota es tanca i no es
reobre; un codi nou crea un lot nou.

**¿Què passa amb els lots buits antics?**
En posar en marxa aquest model es van netejar els lots sense codi: el material que no
necessitava lot va passar a "sense lot", i les ordres antigues sense codi de lot real van
quedar com a no traçables. És coherent amb el nou funcionament.

**¿La traçabilitat és exacta a nivell d'unitat física?**
Treballa a nivell de **lot**, no d'unitat individual. Si un producte comparteix lot amb
altres, la traça els agrupa. Per a més precisió, cal treballar amb lots més petits.

---

## 7. Documents relacionats

- Detall tècnic complet: `backend/docs/traceability.md`
- Mapa funcional general: `docs/functional-map.md`
