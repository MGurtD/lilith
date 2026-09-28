# Despesa

## Per a que serveix aquesta pantalla

És la fitxa d'una despesa general: un pagament que l'empresa registra fora de les factures de compra, classificat per tipus. S'obre en crear una despesa des de «Gestió de despeses» («Alta de despesa») o en obrir-ne una d'existent («Modificació de despesa»). Si la despesa es repeteix, aquí mateix defineixes cada quan i fins quan, i l'aplicació genera els pagaments futurs.

## Accions disponibles

- Triar el «Tipus» de despesa i informar la «Data d'alta», la «Data de pagament» i l'«Import».
- Marcar «Recurrent» per activar «Freqüència», «Dia de pagament» i «Data de fi».
- Escriure una «Descripció».
- Desar amb «Guardar», a la capçalera de la pantalla.

## Flux habitual

1. Des de «Gestió de despeses», prem «+».
2. Tria el «Tipus» i informa la «Data de pagament» i l'«Import».
3. Si és un pagament periòdic, marca «Recurrent», tria la «Freqüència» (mensual, bimensual, trimestral, semestral o anual), el «Dia de pagament» i la «Data de fi».
4. Afegeix una «Descripció» que identifiqui el pagament.
5. Prem «Guardar»: tornes a la llista, on ja surten la despesa i, si és recurrent, els pagaments generats.

## Aspectes importants

- «Tipus», «Data d'alta», «Data de pagament» i «Import» són obligatoris. Els tipus es mantenen a «Gestió de tipus de despesa».
- La «Data de pagament» és la que compta a la llista i al «Tauler de despeses».
- En crear una despesa recurrent, l'aplicació genera un pagament nou per cada període de la «Freqüència», amb el mateix tipus, import i descripció, fins a arribar a la «Data de fi». L'últim pagament pot caure en la data de fi o just després.
- Posa com a «Dia de pagament» el mateix dia del mes que la «Data de pagament». Si són diferents, cada pagament generat es desplaça uns dies més que l'anterior.
- Compte en modificar una despesa recurrent: en desar, s'esborra tota la sèrie, inclosa la despesa que edites, i només es tornen a generar els pagaments posteriors a la seva data de pagament. Revisa la llista després de desar; si has de canviar l'import o les dates de tota la sèrie, sovint és més net eliminar-la i crear-la de nou.
- Eliminar una despesa recurrent, des de la llista, elimina tota la sèrie.

## Errors frequents

- Si «Guardar» no fa res, revisa els missatges en vermell: falta el tipus, alguna data o l'import.
- Si el desplegable «Tipus» surt buit, torna a «Gestió de despeses» i obre la despesa des de la llista, que és la que carrega els tipus.
- Si has marcat «Recurrent», informa sempre la «Freqüència» i la «Data de fi»: el formulari no les exigeix, però sense elles els pagaments no es generen correctament.
- Si després de modificar una despesa recurrent falten pagaments a la llista, és l'efecte de regenerar la sèrie: torna-la a crear amb les dates correctes.

## Proces basic

```mermaid
flowchart TD
    A[Crear la despesa] --> B[Tipus, data de pagament i import]
    B --> C{És recurrent?}
    C -->|No| E[Guardar]
    C -->|Sí| D[Freqüència, dia de pagament i data de fi]
    D --> E
    E --> F[Pagaments generats a la llista]
```
