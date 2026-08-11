# Plan de implementación para rendimiento y usabilidad del Grid

**Estado:** propuesto  
**Fecha:** 2026-08-11  
**Ámbito:** `Dnet.Blazor.Components.Grid`, sus servicios, interop JavaScript, pruebas y ejemplos  
**Plan relacionado:** [`dnet-blazor-stabilization-plan.md`](./dnet-blazor-stabilization-plan.md)

## 1. Objetivo

Mejorar el Grid para que mantenga un comportamiento fluido y predecible con grandes volúmenes de datos, columnas pinned, agrupación, filtrado y proveedores remotos. El resultado debe ser además completamente utilizable con teclado y exponer semántica accesible de grid sin romper innecesariamente la API 5.x.

El trabajo se centra en seis resultados:

1. Ventana virtualizada correcta y libre de respuestas obsoletas.
2. Menos renders, asignaciones y viajes JavaScript/.NET durante interacciones frecuentes.
3. Un único estado de selección y foco para las áreas central y pinned.
4. Contrato explícito y cancelable para operaciones locales y remotas.
5. Navegación de teclado, ARIA y estados de carga/vacío/error.
6. Métricas reproducibles para impedir regresiones.

## 2. Fuera de alcance

- Reescribir el Grid completo o sustituirlo por una biblioteca de terceros.
- Introducir edición avanzada de celdas, exportación o pivot tables como parte de esta iniciativa.
- Soportar alturas variables de fila en la primera entrega. La virtualización seguirá requiriendo una altura conocida; el contrato se documentará claramente.
- Eliminar nombres públicos incorrectos en 5.x. Se mantendrán aliases obsoletos cuando se corrijan.

## 3. Problemas observados

### 3.1 Virtualización

- La actualización de distribución inicia `RefreshDataAsync()` sin esperar su finalización y puede intentar renderizar la ventana anterior.
- La cancelación evita aplicar algunos resultados antiguos, pero no existe un número de versión que garantice que solo gana la última solicitud.
- `OverscanCount` aparece tanto como parámetro del componente como en `GridOptions`; el algoritmo no utiliza una única fuente.
- `_loadedItems` se conserva como `IEnumerable` y se enumera varias veces mediante `Count`, `Skip`, `Take` y `ToList`.
- El Grid mantiene una implementación propia de Virtualize que debe compararse continuamente con el comportamiento oficial de Blazor.

### 3.2 Ruta de renderizado

- En cada render se vuelven a ordenar y dividir las columnas en center, pinned left y pinned right.
- Cada fila vuelve a filtrar columnas visibles y vuelve a ejecutar funciones de datos, estilos, disabled y spanning.
- Las filas virtualizadas no tienen una clave de render estable mediante `@key`.
- Una fila puede estar representada en tres árboles de componentes cuando existen columnas pinned.
- Hover y desplazamiento táctil pueden provocar `StateHasChanged` sobre áreas completas del Grid.

### 3.3 Estado y selección

- Cada `BlgBody` conserva parte de su propio estado de selección, aunque una misma fila puede existir en varias áreas.
- Varias operaciones buscan filas mediante LINQ sobre listas completas.
- La selección de grupos recorre repetidamente el árbol para encontrar padres e hijos.
- No existe una clave de fila pública configurable; el ID interno cambia cuando se reconstruyen nodos.
- Cambios in-place de `GridData`, `GridColumns` o `GridOptions` no pueden detectarse de forma fiable usando referencia de objeto.

### 3.4 Procesamiento de datos

- El filtrado crea cadenas normalizadas por cada celda.
- Las definiciones de columnas se buscan repetidamente por `DataField` y `ColumnId`.
- Algunas operaciones por defecto se lanzan como tareas descartadas y pueden solaparse.
- Filtrado simple, filtrado avanzado, ordenación, agrupación y paginación no comparten un modelo inmutable de consulta.

### 3.5 Usabilidad y accesibilidad

- Existen celdas con `role="gridcell"`, pero faltan filas, encabezados y parte de los estados ARIA del patrón completo.
- No existe un modelo consistente de celda activa ni roving tabindex.
- Ordenación, agrupación, expansión y redimensionado utilizan elementos visuales que no siempre son controles nativos.
- No hay estados integrados y anunciables de carga, vacío, sin resultados y error recuperable.
- El foco puede perderse después de ordenar, filtrar, paginar o mover la ventana virtual.

## 4. Definición de terminado

La iniciativa se considerará terminada cuando:

1. Solo la última petición virtual o remota puede modificar el estado visible.
2. No existen zonas vacías durante scroll normal ni al cambiar rápidamente de dirección.
3. El hover no cruza el límite JavaScript/.NET ni provoca un render del Grid.
4. El scroll táctil realiza como máximo una actualización visual por frame y evita un round-trip JS para leer el valor que JavaScript ya conoce.
5. Las filas virtualizadas utilizan una clave estable definida por el consumidor o por un fallback documentado.
6. Center, pinned left y pinned right comparten selección, hover lógico y foco desde un único propietario.
7. Ordenar, filtrar, agrupar, seleccionar o paginar produce un único callback semántico por acción.
8. El Grid implementa el patrón ARIA grid y puede operarse solo con teclado.
9. Existen estados accesibles de loading, empty, no-results y error.
10. Los benchmarks de 1k, 10k y 100k filas quedan almacenados como baseline y CI detecta regresiones acordadas.
11. Las bibliotecas, ejemplos y pruebas compilan con cero errores y sin nuevas advertencias.

## 5. Presupuestos iniciales de rendimiento

Los valores definitivos se fijarán después de la línea base. Como objetivos iniciales:

| Métrica | Objetivo |
|---|---:|
| Filas de datos soportadas con virtualización local | 100.000 |
| Componentes de fila montados | ventana visible + overscan |
| Operaciones .NET por hover | 0 |
| Actualizaciones durante scroll táctil | máximo 1 por frame |
| Búsqueda de fila por ID | O(1) |
| Respuestas remotas aplicadas fuera de orden | 0 |
| Violaciones axe críticas o serias | 0 |
| Callback de consulta por interacción | 1 |

Se medirán tiempo de render, asignaciones, número de renders, nodos DOM y memoria retenida. No se establecerá un gate temporal absoluto hasta disponer de resultados estables en CI.

## 6. Arquitectura objetivo

```text
Grid parameters / public API
          |
          v
GridState (consulta, selección, foco, layout)
          |
          +--> Data pipeline cancelable/versionado --> LocalDataSource o RemoteDataSource
          |
          +--> LayoutSnapshot cacheado
          |
          +--> RenderWindow<RowViewModel>
                    |
                    +--> pinned left
                    +--> center
                    +--> pinned right

JavaScript se limita a medición, observers y transformaciones de scroll.
El estado semántico y los callbacks siguen perteneciendo a Blazor.
```

Principios:

- Un propietario por cada estado mutable.
- Una única actualización de estado por interacción.
- Datos preparados fuera del markup cuando su cálculo sea reutilizable.
- Identidad estable para filas y columnas.
- Cancelación más versionado para toda operación solapable.
- JavaScript gestiona interacciones de alta frecuencia que solo afectan a presentación.

## 7. Fase 0 — Línea base y caracterización

### GRID-BAS-001 — Instrumentación de renders

**Trabajo**

- Añadir contadores internos habilitados solo en modo diagnóstico para renders de Grid, Header, Body y Row.
- Contabilizar solicitudes virtuales iniciadas, canceladas, descartadas y aplicadas.
- Registrar tamaño de ventana, overscan, nodos montados y duración de preparación del modelo.
- Evitar logs por fila o celda en producción.

**Criterios de aceptación**

- Una prueba puede consultar los contadores sin depender de salida de consola.
- El modo diagnóstico desactivado no asigna mensajes ni emite eventos por render.

### GRID-BAS-002 — Benchmarks reproducibles

Crear escenarios para:

- 1k, 10k y 100k filas, con 10, 30 y 60 columnas.
- Sin pinned, pinned left y pinned en ambos lados.
- Scroll continuo, cambio brusco de dirección y salto al final.
- Selección simple, múltiple y de grupos.
- Ordenación y filtrado local.
- Proveedor remoto con latencia, cancelación y respuestas fuera de orden.

Guardar como baseline:

- Tiempo de primera pintura y actualización.
- Tiempo y asignaciones de filtrado/ordenación.
- Número de componentes y nodos DOM.
- Renders por scroll, hover y selección.
- Memoria después de 100 montajes/desmontajes.

### GRID-BAS-003 — Pruebas de caracterización

Añadir tests que congelen el comportamiento público actual de:

- Paginación, ordenación, filtrado y agrupación.
- Selección entre center y pinned.
- Expansión de grupos.
- Cambio de parámetros con referencia nueva.
- Scroll virtual en primera, intermedia y última ventana.
- Column span y row span.

## 8. Fase 1 — Pipeline virtualizado correcto

### GRID-VIR-001 — Actualización virtual esperable

**Archivos principales**

- `BlgGrid/VirtualizationOperations.cs`
- `Virtualize/VirtualizeJsInterop.cs`
- `Grid/blg-interop.js`

**Trabajo**

- Convertir `UpdateItemDistribution` en una operación `Task` que complete carga y aplicación de ventana.
- No ejecutar `UpdateGridData` hasta que la solicitud correspondiente haya terminado.
- Asignar un número monotónico a cada solicitud y aplicar solo la versión más reciente.
- Cancelar y disponer el CTS anterior antes de crear el siguiente.
- Separar claramente estado requested, loaded y rendered.
- Propagar errores al estado del Grid en lugar de lanzarlos desde el bloque de render.

**Criterios de aceptación**

- Una respuesta antigua nunca reemplaza datos más recientes.
- Cambiar rápidamente la dirección del scroll no deja huecos.
- Cancelar una solicitud no se representa como error.
- El indicador loading no parpadea en cargas que completan dentro del umbral configurado.

### GRID-VIR-002 — Una sola configuración de overscan

- Usar `GridOptions.OverscanCount` como contrato principal.
- Mantener temporalmente `BlgGrid.OverscanCount` como alias `[Obsolete]` si forma parte de la API publicada.
- Validar que el valor sea mayor o igual que cero.
- Documentar el compromiso entre memoria, primera pintura y scroll rápido.

**Implementado (2026-08-11)**

- `GridOptions.OverscanCount` usa 15 filas por defecto, en línea con `Virtualize<TItem>` de .NET 11.
- El observador empieza la siguiente redistribución cuando todavía queda la mitad del buffer de overscan, con un mínimo de 50 px.
- Los conjuntos paginados que caben en dos ventanas virtuales permanecen montados completos. Así se eliminan los huecos al arrastrar la barra entre extremos sin desactivar la virtualización de conjuntos grandes.

### GRID-VIR-003 — Materializar una sola vez

- Almacenar la página cargada como `IReadOnlyList<RowNode<TItem>>`.
- Evitar enumeraciones repetidas de `IEnumerable`.
- No crear una nueva lista si la ventana solicitada y la versión de datos no han cambiado.
- Añadir rutas optimizadas para `List<T>` e `IReadOnlyList<T>`.

**Implementado (2026-08-11)**

- El proveedor local materializa directamente con `List<T>.GetRange` y la ventana renderizada reutiliza esa lista.
- La ruta en memoria no crea ni cancela `CancellationTokenSource`; se conserva la ruta cancelable para futuros proveedores asíncronos.

### GRID-VIR-004 — Evaluar Virtualize oficial

- Crear un prototipo usando `Microsoft.AspNetCore.Components.Web.Virtualization.Virtualize`.
- Comparar soporte de pinned, row span, scroll horizontal y agrupación.
- Mantener implementación propia solo para capacidades que la oficial no pueda cubrir.
- Documentar diferencias y casos de actualización cuando cambie .NET.

## 9. Fase 2 — Reducir el coste de render

### GRID-REN-001 — LayoutSnapshot de columnas

Crear un snapshot que contenga:

- Columnas ordenadas y visibles.
- Listas center, pinned left y pinned right.
- Anchuras totales.
- Strings `grid-template-columns` ya construidos.
- Mapa por `ColumnId` y `DataField`.
- Versión del layout.

Recalcularlo solo cuando cambien definición, orden, pinned, visibilidad o anchura.

### GRID-REN-002 — RowRenderModel

- Preparar una representación por fila/celda para la ventana visible.
- Evaluar una sola vez por ciclo `DisableRow`, `CellDataFn`, `ColumnSpanFn`, `RowSpanFn`, clases y estilos.
- Reutilizar el resultado en las tres áreas pinned.
- Formatear fechas y valores mediante funciones cacheadas por columna.
- Invalidar por versión de fila, columnas, cultura o configuración relevante.

### GRID-REN-003 — Identidad estable

- Añadir `Func<TItem, object> GetRowKey` o contrato equivalente.
- Utilizar la clave en `@key` para filas y celdas.
- Mantener un fallback basado en ID interno con advertencia de limitaciones.
- Conservar selección y foco por clave al reconstruir datos.

### GRID-REN-004 — Limitar renders

- Sustituir `ActiveRender` distribuido por invalidaciones con motivos explícitos.
- No invocar `StateHasChanged` en parent y children para la misma transición.
- Usar CSS `:hover` para el hover puramente visual.
- Renderizar solo header cuando cambia sort/filter/layout.
- Renderizar solo las filas afectadas cuando cambia selección.
- Añadir pruebas de número máximo de renders por operación.

## 10. Fase 3 — Estado, selección y foco centralizados

### GRID-STA-001 — Índices O(1)

- Mantener diccionarios por `RowNodeId`, row key, `ColumnId` y `DataField`.
- Reconstruirlos una sola vez por versión de datos/layout.
- Sustituir búsquedas LINQ en clicks, selección y expansión.
- Usar `HashSet` para conjuntos de IDs durante selección masiva.

### GRID-STA-002 — SelectionModel único

El propietario será `BlgGrid`; `BlgBody` y `BlgRow` recibirán solo estado derivado y emitirán intents.

El modelo debe incluir:

- Claves seleccionadas.
- Anchor y range end para selección con Shift.
- Última fila activa.
- Estado select-all para datos locales y remotos.
- Estrategia de selección de grupos.

**Criterios de aceptación**

- Center y pinned siempre muestran el mismo estado.
- Single nunca contiene más de una clave.
- Disabled no se puede seleccionar.
- La selección sobrevive a ordenación, filtrado y reconstrucción cuando la clave sigue presente.
- La API puede devolver selección visible o selección total explícitamente.

### GRID-STA-003 — FocusModel

- Separar celda activa de fila seleccionada.
- Conservar `(rowKey, columnId)` después de un render.
- Si la celda desaparece, elegir el vecino válido más cercano.
- Desplazar la celda activa a la vista sin mover la selección involuntariamente.

### GRID-STA-004 — Parámetros y mutaciones

- Mantener detección por referencia nueva.
- Añadir `RefreshDataAsync`, `RefreshColumnsAsync` y `RefreshLayoutAsync` para mutaciones in-place.
- Considerar un parámetro opcional `DataVersion`/`ColumnsVersion` para escenarios reactivos.
- Documentar claramente los dos contratos.

## 11. Fase 4 — Interacciones de alta frecuencia e interop

### GRID-INT-001 — Hover sin render

- Eliminar eventos .NET de mouseover/mouseout usados solo para color.
- Implementar hover sincronizado entre pinned y center mediante atributos de row key y CSS, o una clase aplicada por JavaScript sin callback .NET.
- Mantener eventos públicos de hover solo si el consumidor se suscribe y aplicar throttling.

### GRID-INT-002 — Scroll horizontal y táctil

- Usar el `elementScrollLeft` ya calculado en JavaScript.
- Aplicar transformaciones de header/body pinned mediante `requestAnimationFrame` en JavaScript.
- Notificar a .NET únicamente al finalizar el gesto o cuando sea necesario para estado semántico.
- Garantizar como máximo una escritura de estilo por frame.
- Respetar RTL y normalizar diferencias de `scrollLeft` entre navegadores.

### GRID-INT-003 — Resize de columnas

- Aplicar preview visual en JavaScript durante drag.
- Confirmar la nueva anchura una vez en pointerup.
- Utilizar Pointer Events para mouse, touch y pen.
- Añadir límites min/max y navegación de teclado para el separador.
- Liberar listeners incluso si el pointer termina fuera del Grid.

### GRID-INT-004 — Propiedad de recursos

- Mantener handles por instancia para observers/listeners.
- Hacer idempotente la retirada.
- Capturar `JSDisconnectedException` solo durante dispose.
- Probar 100 montajes/desmontajes sin callbacks posteriores ni referencias retenidas.

## 12. Fase 5 — Procesamiento local y datos remotos

### GRID-DAT-001 — Filtrado sin asignaciones evitables

- Usar comparación `StringComparison` sin normalizar cada valor con `ToUpperInvariant`.
- Preprocesar cada filtro una vez.
- Cachear mapas de columnas y accessors.
- Definir cultura configurable para texto, números y fechas.
- Mantener short-circuit entre condiciones.

### GRID-DAT-002 — Ordenación estable

- Conservar posición original como desempate.
- Definir nulls first/last independientemente del sentido.
- Evitar convertir valores numéricos y fechas a string cuando `CellDataFn` devuelve tipos comparables.
- Añadir comparador opcional por columna.
- Documentar estabilidad dentro de grupos.

### GRID-DAT-003 — GridQuery inmutable

Introducir un modelo de consulta que agrupe:

- Ventana/página.
- Sorts.
- Filtros simples y avanzados.
- Grupos y expansión.
- Cultura y versión.

Las operaciones remotas recibirán snapshot y `CancellationToken`; no compartirán el objeto mutable `_searchModel`.

### GRID-DAT-004 — Proveedor remoto unificado

Proponer una API equivalente a:

```csharp
ValueTask<GridDataResult<TItem>> LoadDataAsync(
    GridQuery query,
    CancellationToken cancellationToken);
```

- Emitir una sola petición por interacción.
- Cancelar debounce y consulta anteriores.
- Aplicar solo la última versión.
- Separar `TotalCount`, items y metadata de grupos.
- Conservar callbacks actuales como capa de compatibilidad 5.x.

### GRID-DAT-005 — Debounce cancelable

- Sustituir `System.Timers.Timer` del header por CTS y `Task.Delay`.
- Configurar debounce por Grid.
- Filtrar inmediatamente con Enter y limpiar inmediatamente al pulsar clear.
- Mostrar estado busy solo cuando la operación supera un umbral.

## 13. Fase 6 — Usabilidad y accesibilidad

### GRID-A11Y-001 — Semántica estructural

- Añadir `role="grid"`, `rowgroup`, `row`, `columnheader` y `gridcell` de forma coherente también con pinned.
- Exponer `aria-rowcount`, `aria-colcount`, `aria-rowindex` y `aria-colindex` con virtualización.
- Añadir `aria-selected`, `aria-disabled` y `aria-expanded`.
- Añadir `aria-sort` al encabezado ordenado.
- Evitar árboles ARIA duplicados por columnas pinned: elegir un único árbol semántico.

### GRID-A11Y-002 — Navegación de celdas

- Implementar roving tabindex con una sola celda `tabindex="0"`.
- Flechas: mover una celda.
- Home/End: primera/última columna; Ctrl+Home/Ctrl+End: extremos del Grid.
- PageUp/PageDown: mover una ventana visible.
- Enter: activar acción principal o entrar en modo interacción.
- Space: seleccionar fila cuando corresponda.
- Escape: cerrar paneles/filtros o salir del modo interacción.

### GRID-A11Y-003 — Controles de encabezado

- Convertir sort, group, expand y filtros en botones nativos con nombre accesible.
- Implementar separador de resize con `role="separator"`, orientación y valores ARIA.
- Permitir cambiar anchura con teclado.
- Asociar filtros con su columna mediante labels.
- Añadir nombre al checkbox select-all y estado indeterminado.

### GRID-UX-001 — Estados del Grid

Añadir templates y defaults para:

- Loading inicial.
- Actualización en segundo plano.
- Vacío sin datos.
- Sin resultados por filtros.
- Error con acción de reintento.

Los cambios se anunciarán con `aria-live` sin interrumpir navegación normal.

### GRID-UX-002 — Persistencia y restauración

- Restaurar foco después de sort, filter, pagination y refresh.
- Mantener scroll cuando una actualización conserva la fila activa.
- Normalizar página si filtros reducen el total.
- Conservar anchos, orden y pinned mediante un modelo de estado serializable opcional.

### GRID-UX-003 — Densidad y responsive

- Definir densidades compact, normal y comfortable.
- Mantener target táctil adecuado para controles aunque la fila sea compacta.
- Documentar comportamiento recomendado en móvil.
- Evitar que pinned consuma todo el viewport; establecer un ancho mínimo visible para center.

## 14. Fase 7 — Validación y documentación

### GRID-VAL-001 — Matriz funcional

Probar combinaciones representativas:

| Datos | Columnas | Pinned | Agrupación | Modo |
|---:|---:|---|---|---|
| 0 | 10 | no | no | local |
| 1.000 | 10 | left | no | local |
| 10.000 | 30 | ambos | sí | local |
| 100.000 | 60 | ambos | sí | virtual/remoto |

Para cada una validar scroll, teclado, selección, resize, sort, filter, pagination, loading, error y desmontaje.

### GRID-VAL-002 — Navegadores y entradas

- Chromium, Firefox y WebKit.
- Blazor Server y WebAssembly.
- Mouse, teclado, touch y pen cuando esté disponible.
- Viewport desktop y móvil; zoom 200 %.
- Culturas `es-ES`, `en-US` y una cultura RTL.

### GRID-VAL-003 — Documentación

Documentar:

- Configuración mínima.
- Virtualización, altura de fila y overscan.
- Clave estable y refresco de mutaciones in-place.
- Fuente local y proveedor remoto.
- Selección y persistencia.
- Teclado y accesibilidad.
- Rendimiento de templates y funciones de celda.
- Migración desde callbacks remotos actuales.

## 15. Estrategia de compatibilidad

- Las correcciones internas no cambiarán firmas públicas.
- `GridOptions.OverscanCount` será el nombre definitivo; cualquier alias se mantendrá obsoleto durante una versión menor.
- Los callbacks actuales continuarán funcionando mientras se introduce el proveedor unificado.
- Los nombres corregidos seguirán la política de aliases `[Obsolete]` de la guía de estabilización.
- `GetRowKey` será opcional en 5.x, pero se recomendará para selección persistente y virtualización dinámica.
- Los nuevos roles y botones conservarán clases CSS existentes para reducir roturas visuales.

## 16. Secuencia propuesta de PR

1. **GRID-PR-01:** instrumentación, benchmarks y caracterización.
2. **GRID-PR-02:** pipeline virtualizado versionado y overscan único.
3. **GRID-PR-03:** LayoutSnapshot, índices y claves de render.
4. **GRID-PR-04:** RowRenderModel y reducción de renders.
5. **GRID-PR-05:** SelectionModel y FocusModel centralizados.
6. **GRID-PR-06:** hover, touch, scroll y resize de alta frecuencia.
7. **GRID-PR-07:** filtrado/ordenación optimizados y GridQuery.
8. **GRID-PR-08:** proveedor remoto cancelable y compatibilidad.
9. **GRID-PR-09:** patrón ARIA grid y navegación con teclado.
10. **GRID-PR-10:** estados UX, documentación y matriz final.

Los PR deben permanecer suficientemente pequeños para comparar métricas antes/después. Cada optimización deberá incluir una prueba funcional que demuestre que no cambia el resultado observable.

## 17. Gates por PR

Cada PR debe cumplir:

- Build de solución y ejemplos sin errores.
- Tests unitarios y de componentes relacionados.
- `git diff --check` limpio.
- Sin nuevas tareas descartadas, timers no liberados o listeners sin propietario.
- Comparativa de métricas si modifica render, datos, interop o virtualización.
- Prueba de teclado y axe si modifica markup interactivo.
- Captura o vídeo si modifica comportamiento visual.
- Nota de compatibilidad si modifica API o clases CSS.

## 18. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| El cache muestra datos obsoletos. | Versionar datos, layout y cultura; pruebas explícitas de invalidación. |
| `@key` recrea más componentes de los esperados. | Clave estable obligatoria en benchmarks y medición antes/después. |
| Centralizar selección cambia callbacks existentes. | Pruebas de caracterización y adapter de compatibilidad. |
| Mover scroll a JavaScript desincroniza estado Blazor. | JS solo controla presentación de alta frecuencia y confirma estado al finalizar. |
| ARIA duplica filas por pinned. | Un árbol semántico central; áreas visuales pinned como presentación. |
| Proveedor remoto rompe integraciones actuales. | Introducción aditiva y periodo de obsolescencia. |
| Optimizar templates limita personalización. | Cachear solo resultados cuya dependencia esté versionada y permitir invalidación explícita. |

## 19. Checklist final

- [ ] Baseline de rendimiento almacenada.
- [ ] Pipeline virtual cancelable, esperado y versionado.
- [x] Overscan con una única fuente de configuración.
- [ ] Layout y mapas de columnas cacheados.
- [ ] Filas y celdas con identidad estable.
- [ ] Sin renders .NET por hover visual.
- [ ] Scroll táctil limitado a una actualización por frame.
- [ ] Selección y foco centralizados.
- [ ] Búsquedas de fila/columna O(1).
- [ ] Procesamiento local sin asignaciones evitables por celda.
- [ ] Proveedor remoto unificado y compatible.
- [ ] Patrón ARIA grid completo.
- [ ] Navegación de teclado cubierta por E2E.
- [ ] Estados loading/empty/no-results/error disponibles.
- [ ] Matriz 100k filas validada en Server y WebAssembly.
- [ ] Documentación y guía de migración publicadas.
