# Gasto

## Para qué sirve esta pantalla

Es la ficha de un gasto general: un pago que la empresa registra fuera de las facturas de compra, clasificado por tipo. Se abre al crear un gasto desde «Gestión de gastos» («Alta de gasto») o al abrir uno existente («Modificación de gasto»). Si el gasto se repite, aquí mismo defines cada cuánto y hasta cuándo, y la aplicación genera los pagos futuros.

## Acciones disponibles

- Elegir el «Tipo» de gasto e informar la «Fecha de alta», la «Fecha de pago» y el «Importe».
- Marcar «Recurrente» para activar «Frecuencia», «Día de pago» y «Fecha de fin».
- Escribir una «Descripción».
- Guardar con «Guardar», en la cabecera de la pantalla.

## Flujo habitual

1. Desde «Gestión de gastos», pulsa «+».
2. Elige el «Tipo» e informa la «Fecha de pago» y el «Importe».
3. Si es un pago periódico, marca «Recurrente» y elige la «Frecuencia» (mensual, bimensual, trimestral, semestral o anual), el «Día de pago» y la «Fecha de fin».
4. Añade una «Descripción» que identifique el pago.
5. Pulsa «Guardar»: vuelves a la lista, donde ya aparecen el gasto y, si es recurrente, los pagos generados.

## Aspectos importantes

- «Tipo», «Fecha de alta», «Fecha de pago» e «Importe» son obligatorios. Los tipos se mantienen en «Gestión de tipos de gasto».
- La «Fecha de pago» es la que cuenta en la lista y en el «Panel de gastos».
- Al crear un gasto recurrente, la aplicación genera un pago nuevo por cada período de la «Frecuencia», con el mismo tipo, importe y descripción, hasta llegar a la «Fecha de fin». El último pago puede caer en la fecha de fin o justo después.
- Pon como «Día de pago» el mismo día del mes que la «Fecha de pago». Si son distintos, cada pago generado se desplaza unos días más que el anterior.
- Cuidado al modificar un gasto recurrente: al guardar, se borra toda la serie, incluido el gasto que editas, y solo se vuelven a generar los pagos posteriores a su fecha de pago. Revisa la lista después de guardar; si tienes que cambiar el importe o las fechas de toda la serie, suele ser más limpio eliminarla y crearla de nuevo.
- Eliminar un gasto recurrente, desde la lista, elimina toda la serie.

## Errores frecuentes

- Si «Guardar» no hace nada, revisa los mensajes en rojo: falta el tipo, alguna fecha o el importe.
- Si el desplegable «Tipo» sale vacío, vuelve a «Gestión de gastos» y abre el gasto desde la lista, que es la que carga los tipos.
- Si has marcado «Recurrente», informa siempre la «Frecuencia» y la «Fecha de fin»: el formulario no las exige, pero sin ellas los pagos no se generan correctamente.
- Si después de modificar un gasto recurrente faltan pagos en la lista, es el efecto de regenerar la serie: vuelve a crearla con las fechas correctas.

## Proceso básico

```mermaid
flowchart TD
    A[Crear el gasto] --> B[Tipo, fecha de pago e importe]
    B --> C{Es recurrente?}
    C -->|No| E[Guardar]
    C -->|Sí| D[Frecuencia, día de pago y fecha de fin]
    D --> E
    E --> F[Pagos generados en la lista]
```
