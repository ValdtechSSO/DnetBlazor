# Plan de implementación para rendimiento del Grid en tiempo de ejecución

**Estado:** listo para implementación  
**Fecha:** 2026-08-12  
**Ámbito:** `src/Dnet.Blazor/Components/Grid`, pruebas, benchmarks, ejemplos y documentación  
**Plan predecesor:** [`dnet-blazor-grid-performance-usability-plan.md`](./dnet-blazor-grid-performance-usability-plan.md)

## 1. Propósito

Este plan completa la optimización del Grid después del trabajo de estabilización ya realizado. El objetivo no es añadir más overscan para ocultar los saltos, sino reducir de forma medible el trabajo que se ejecuta en cada frame, cada fila y cada celda.

Los resultados esperados son:

1. Scroll horizontal y táctil fluido, sin un render Blazor por evento.
2. Coste de preparación de filas proporcional a la ventana visible, sin cálculos repetidos por panel pinned.
3. Selección y búsquedas de estado O(1) en la ruta habitual.
4. Carga remota virtualizada sin materializar el conjunto completo en memoria.
5. Rendimiento predecible con grids anchos mediante overscan adaptativo y, en una fase posterior, virtualización horizontal.
6. Métricas reproducibles que permitan demostrar cada mejora y detectar regresiones.

## 2. Estado actual verificado

### Ya implementado y que debe conservarse

- Solicitudes virtuales cancelables y versionadas; las respuestas obsoletas se descartan.
- Claves estables de fila mediante `GridOptions.RowKeySelector`, `RowNode.RenderKey` y `@key`.
- Diccionarios `_rowNodesById` y `_rowNodesByKey` para búsquedas directas.
- `GridLayoutSnapshot` para columnas visibles, ordenadas y pinned.
- Materialización eficiente de la ventana local mediante `List<T>.GetRange`.
- Hover visual sincronizado sin callback .NET.
- Altura de fila fija y `GridOptions.OverscanCount` como fuente principal.
- Contadores básicos de diagnóstico para solicitudes virtuales.
- Comparación de texto sin crear copias con `ToUpperInvariant`.

### Cuellos de botella pendientes

| Área | Situación actual | Consecuencia |
|---|---|---|
| Scroll horizontal | Cada `onscroll` entra en .NET, .NET consulta de nuevo a JavaScript y llama a `StateHasChanged` | Trabajo y latencia por frame |
| Scroll táctil | JavaScript limita con `requestAnimationFrame`, pero sigue notificando a .NET cada frame | Se conserva el coste de interop y render |
| Render de celdas | `BlgRow` filtra columnas, crea parámetros y ejecuta funciones repetidamente | Muchas asignaciones y CPU con grids anchos |
| Columnas pinned | Una fila lógica puede crear tres instancias de `BlgRow` | Se triplican parte del lifecycle y parámetros |
| Datos remotos | `_itemsProvider` es privado y siempre usa el proveedor local por defecto | Se limita el DOM, pero no el volumen cargado en memoria |
| Overscan | Valor fijo independiente de filas visibles y número de columnas | Puede montar miles de celdas innecesarias |
| Paginación pequeña | Mantiene infraestructura virtual aunque la página tenga pocas filas | Complejidad y coste sin beneficio real |
| Selección | Algunas operaciones reconstruyen selección recorriendo todas las filas | O(N) por interacción o reconstrucción |
| Sort/filter | Valores typed pasan por `object`, string y parseos posteriores | Boxing, conversiones y comparaciones costosas |
| Resize | Los movimientos del ratón se procesan en Blazor | Render frecuente durante el drag |
| Grid ancho | Todas las columnas centrales se montan aunque no sean visibles | DOM excesivo con 30–60 columnas |
| Medición | No existe baseline completa de tiempo, nodos, frames e interop | No se puede demostrar ni proteger la mejora |

## 3. Principios de implementación

- Medir antes y después de cada fase.
- Mantener el estado semántico en .NET y dejar en JavaScript solo la presentación de alta frecuencia.
- Preparar modelos inmutables fuera del markup.
- Hacer que el trabajo dependa de filas y columnas visibles, no del total de datos.
- Introducir APIs nuevas de forma aditiva y mantener compatibilidad durante la versión 5.x.
- No reducir el overscan hasta haber eliminado primero los renders e interop innecesarios.
- Cada optimización debe tener una prueba funcional que demuestre que el resultado observable no cambia.

## 4. Objetivos y presupuestos

La fase de baseline fijará los valores finales. Se usarán inicialmente estos gates:

| Métrica | Objetivo inicial |
|---|---:|
| Callbacks .NET durante scroll horizontal continuo | 0 |
| Callbacks .NET durante scroll táctil puramente visual | 0 |
| Renders Blazor provocados por resize mientras se arrastra | 0 |
| Solicitudes remotas aplicadas fuera de orden | 0 |
| Búsqueda y cambio de selección individual | O(1) |
| Filas DOM en modo virtual | filas visibles + overscan efectivo |
| Celdas DOM | por debajo de `MaxMountedCells`, salvo excepción documentada |
| Componentes de fila por fila lógica visible | 1 modelo preparado compartido por todos los paneles |
| Regresión permitida en benchmarks estables | máximo 10 % de la mediana |
| Zonas blancas perceptibles en scroll rápido | 0 en la matriz soportada |

Los presupuestos temporales se compararán con la mediana de al menos cinco ejecuciones, evitando usar un único resultado como gate.

## 5. Fase 0 — Baseline, telemetría y escenarios

### GRID-PERF-001 — Completar `GridDiagnostics`

**Trabajo**

- Añadir contadores de renders de Grid, Header, Body y Row.
- Contar callbacks e invocaciones JS por tipo: scroll, touch, resize, observer y virtualización.
- Medir duración de creación del layout, modelo de filas, filtering y sorting.
- Exponer filas, columnas y celdas montadas, además de overscan solicitado y efectivo.
- Activar la instrumentación solo con una opción de diagnóstico; el modo desactivado no debe crear strings ni eventos.

**Criterios de aceptación**

- Los tests pueden leer un snapshot inmutable de métricas.
- Un test demuestra cero asignaciones de mensajes de diagnóstico cuando la opción está desactivada.
- La instrumentación no usa logging por fila o celda.

### GRID-PERF-002 — Proyecto de benchmarks de CPU

**Trabajo**

- Crear un proyecto BenchmarkDotNet separado de los tests funcionales.
- Medir filtering, sorting, agrupación y preparación de `RowRenderModel` con 1k, 10k y 100k filas.
- Ejecutar escenarios de 10, 30 y 60 columnas con datos string, numeric, date y null.
- Guardar el baseline inicial en `docs/performance/grid` junto con máquina, runtime y configuración.

**Criterios de aceptación**

- Los benchmarks pueden ejecutarse localmente con un comando documentado.
- El resultado incluye tiempo medio/mediano y bytes asignados.
- Los datos de prueba son deterministas.

### GRID-PERF-003 — Escenarios Playwright de render y scroll

**Trabajo**

- Añadir una página de laboratorio fuera de la navegación normal de la demo.
- Medir primera pintura, número de nodos, frames largos, callbacks .NET y estabilidad del scroll.
- Cubrir scroll vertical continuo, cambio rápido de dirección, salto al final, scroll horizontal y resize.
- Ejecutar las combinaciones críticas: 1k/10 columnas, 10k/30 columnas y 100k/60 columnas, con y sin pinned.

**Criterios de aceptación**

- Los tests fallan si aparecen huecos persistentes, respuestas obsoletas o un número inesperado de callbacks.
- Se guarda un trace de Playwright cuando falla un escenario.
- El test no depende de esperas fijas salvo un timeout máximo de seguridad.

## 6. Fase 1 — Interacciones de alta frecuencia fuera del render de Blazor

### GRID-PERF-101 — Scroll horizontal gestionado en JavaScript

**Archivos principales**

- `Components/Grid/blg-interop.js`
- `Components/Grid/BlgGrid/BlgGrid.razor`
- `Components/Grid/BlgGrid/PublicApiMethods.cs`
- `Components/Grid/Infrastructure/Services/BlGridInterop.cs`

**Trabajo**

- Eliminar el `@onscroll` que invoca `OnScroll` por cada evento.
- Registrar un listener pasivo por instancia desde JavaScript.
- Acumular el último `scrollLeft` y aplicar transforms una vez por `requestAnimationFrame`.
- Actualizar directamente header y regiones visuales que deban permanecer sincronizadas.
- Normalizar `scrollLeft` para RTL.
- Notificar a .NET únicamente en `scrollend`, con fallback debounce, si hay estado público que persistir.
- Devolver un handle y retirar listener y frame pendiente durante dispose.

**Criterios de aceptación**

- Un scroll horizontal continuo genera cero callbacks .NET y cero renders Blazor.
- Header y body no divergen visualmente más de un frame.
- Funciona en Chromium, Firefox y WebKit, incluyendo RTL.
- Montar y desmontar el Grid 100 veces no deja listeners activos.

### GRID-PERF-102 — Scroll táctil completamente visual en JavaScript

**Trabajo**

- Eliminar `OnTouchMove` de la ruta por frame.
- Reutilizar la misma infraestructura rAF del scroll horizontal.
- Mantener en .NET solo el evento semántico final si la API pública lo requiere.
- Usar listeners pasivos siempre que no sea necesario impedir el gesto nativo.

**Criterios de aceptación**

- El gesto no llama a .NET mientras se mueve.
- No se producen saltos entre contenido central y pinned.
- Touch, pen y trackpad conservan el comportamiento actual.

### GRID-PERF-103 — Preview de resize en JavaScript

**Trabajo**

- Sustituir mouse events Blazor por Pointer Events con pointer capture.
- Actualizar una CSS custom property como máximo una vez por frame durante drag.
- Confirmar el ancho definitivo a .NET una sola vez en `pointerup`.
- Aplicar límites min/max en preview y commit.
- Cancelar correctamente con `pointercancel`, Escape o dispose.
- Evaluar `ResizeObserver` para evitar mediciones posteriores al render.

**Criterios de aceptación**

- Durante el drag hay cero renders Blazor.
- El callback de cambio de ancho se ejecuta una vez por gesto.
- El resize funciona con mouse, touch, pen y teclado.

## 7. Fase 2 — Modelo de render preparado y compartido

### GRID-PERF-201 — Introducir `RowRenderModel` y `CellRenderModel`

**Trabajo**

- Crear modelos internos inmutables para una fila y sus celdas visibles.
- Evaluar una sola vez por versión: `DisableRow`, `CellDataFn`, `ColumnSpanFn`, `RowSpanFn`, clase, estilo, valor formateado y metadatos de interacción.
- Invocar cada función de span una sola vez por celda.
- Preparar el modelo solo para la ventana que va a renderizarse.
- Cachear formatters y accessors por versión de columna.
- Versionar invalidación por datos, layout, selección relevante, cultura y opciones.

**Criterios de aceptación**

- Una prueba con delegates contadores demuestra una invocación máxima por fila/celda y versión.
- El markup de `BlgRow` no busca ni filtra columnas.
- Los paneles left, center y right reciben vistas del mismo modelo preparado.
- El cache nunca muestra un valor anterior después de refresh, sort, filter o cambio de cultura.

### GRID-PERF-202 — Reducir componentes duplicados por pinned

**Trabajo**

- Mantener una única identidad/modelo de componente por fila lógica.
- Convertir los paneles pinned en vistas ligeras del modelo compartido.
- Comparar mediante benchmark dos alternativas: componente de fila único frente a render fragment interno en `BlgBody`.
- Elegir la alternativa con menor coste sin degradar mantenibilidad ni `@key`.

**Criterios de aceptación**

- Activar pinned no vuelve a ejecutar funciones de datos de la fila.
- Selección, disabled, hover y foco cambian de forma atómica en los tres paneles.
- La mejora queda documentada con métricas antes/después.

### GRID-PERF-203 — `ShouldRender` e invalidación dirigida

**Trabajo**

- Introducir versiones explícitas de data, layout, viewport y selection.
- Hacer que Header, Body y Row rendericen solo cuando cambia una versión que consumen.
- Evitar llamadas duplicadas a `StateHasChanged` en parent y children para la misma transición.
- Actualizar únicamente las filas afectadas por selección cuando sea posible.

**Criterios de aceptación**

- Seleccionar una fila no vuelve a renderizar todas las filas visibles.
- Scroll horizontal y resize preview no renderizan componentes.
- Sort/filter realizan una sola invalidación semántica del Grid después de aplicar el nuevo estado.

## 8. Fase 3 — Selección y estado incremental

### GRID-PERF-301 — `SelectionModel` central con claves

**Trabajo**

- Mover la propiedad de selección a `BlgGrid` usando `HashSet<object>`.
- Actualizar la selección incrementalmente en select, deselect, range y select page.
- Evitar reconstruir el conjunto seleccionado recorriendo `_rowNodes`.
- Hacer `BlgBody` y `BlgRow` stateless respecto a selección.
- Mantener selección de elementos remotos que no estén cargados.
- Materializar `SelectedRows` solo cuando un callback compatible lo solicite.

**Criterios de aceptación**

- Selección individual y consulta por clave son O(1).
- Center y pinned no mantienen copias independientes.
- La selección persiste tras sort/filter/refresh si la clave sigue siendo válida.
- Las APIs y callbacks actuales siguen funcionando.

### GRID-PERF-302 — Índices incrementales de filas y grupos

**Trabajo**

- Mantener índices de parent, children, visible index y key durante la construcción del árbol.
- Sustituir búsquedas LINQ restantes en rutas de interacción.
- Actualizar índices por versión, no por render.

**Criterios de aceptación**

- Expand, collapse, range select y group select no escanean el árbol completo en la ruta habitual.
- Los índices se reconstruyen exactamente una vez por versión de datos/grupos.

## 9. Fase 4 — Estrategia virtual adaptativa

### GRID-PERF-401 — Desactivar virtualización en páginas pequeñas

**Trabajo**

- Calcular `EffectiveVirtualization` a partir de filas de página, viewport y umbral configurable.
- Renderizar directamente páginas pequeñas, inicialmente hasta dos ventanas o un máximo configurable.
- Conservar exactamente el mismo markup de fila y comportamiento de selección.
- Permitir forzar on/off para diagnóstico y compatibilidad.

**Criterios de aceptación**

- Páginas habituales de 25, 50 y 100 filas no pagan observers ni redistribución si caben en el umbral.
- Cambiar entre rutas directa y virtual no pierde foco, selección ni posición lógica.
- Los escenarios grandes siguen usando virtualización.

### GRID-PERF-402 — Overscan adaptativo con presupuesto de celdas

**Trabajo**

- Añadir `MinOverscanCount`, `MaxOverscanCount` y `MaxMountedCells` internos o públicos según la validación de API.
- Calcular overscan efectivo usando filas visibles, columnas visibles, coste estimado del template y velocidad/dirección del scroll.
- Aumentar temporalmente el buffer durante desplazamiento rápido y reducirlo al quedar idle.
- Mantener `OverscanCount` actual como fallback compatible.
- Registrar el valor efectivo en diagnostics.

**Criterios de aceptación**

- El número de celdas montadas respeta el presupuesto en grids anchos.
- El scroll rápido no reintroduce bandas blancas en la matriz soportada.
- La política no oscila continuamente cuando la velocidad está cerca de un umbral.

### GRID-PERF-403 — Mejorar callbacks del observer virtual

**Trabajo**

- Cambiar callbacks `[JSInvokable]` que inician trabajo async de `void` a `Task`.
- Observar y clasificar errores de interop.
- Conservar cancelación y control monotónico de versión.

**Criterios de aceptación**

- No quedan tareas async descartadas en la ruta virtual.
- Los errores reales llegan al estado/diagnóstico; una cancelación esperada no se registra como error.

## 10. Fase 5 — Proveedor remoto virtualizado

### GRID-PERF-501 — Contratos `GridQuery` y `GridDataResult`

**Trabajo**

- Crear un snapshot inmutable con `StartIndex`, `Count`, sorts, filters, groups, página, cultura y versión.
- Crear un resultado con items, total count y metadata opcional de agrupación.
- Definir igualdad estable para evitar solicitudes duplicadas.

### GRID-PERF-502 — API pública `GridItemsProvider<TItem>`

API objetivo orientativa:

```csharp
public delegate ValueTask<GridDataResult<TItem>> GridItemsProvider<TItem>(
    GridQuery query,
    CancellationToken cancellationToken);
```

**Trabajo**

- Exponer el provider como parámetro público alternativo a `GridData`.
- Validar que `GridData` y provider no se usen de forma ambigua.
- Conectar el provider con el pipeline versionado existente.
- Cancelar la consulta anterior y aplicar únicamente la versión más reciente.
- Mantener callbacks remotos actuales mediante un adapter compatible y deprecación gradual.
- Soportar total count sin crear un `RowNode` por cada fila remota.

**Criterios de aceptación**

- Abrir un dataset remoto de 100k filas solo materializa la ventana cargada.
- Cambios rápidos de sort/filter cancelan trabajo anterior y nunca aplican respuestas fuera de orden.
- El total y paginator permanecen correctos con páginas vacías o total reducido.
- La cancelación no muestra estado de error.

### GRID-PERF-503 — Cache remoto acotado

**Trabajo**

- Añadir cache opcional de ventanas por clave de consulta.
- Prefetch como máximo de la siguiente ventana cuando la red esté idle.
- Limitar por número de ventanas o memoria estimada con política LRU.
- Invalidar por cambio de query/data version.

**Criterios de aceptación**

- Volver a una ventana reciente no repite la petición cuando el cache sigue válido.
- El cache nunca crece sin límite ni mezcla resultados de queries distintas.

## 11. Fase 6 — Procesamiento typed y debounce

### GRID-PERF-601 — Accessors y comparadores typed

**Trabajo**

- Introducir de forma aditiva una definición `GridColumn<TItem, TValue>` o accessors typed equivalentes.
- Ordenar números, fechas y enums sin convertir a string ni volver a parsear.
- Evitar boxing en la ruta typed.
- Compilar/cachear accessors una vez por columna.
- Mantener `CellDataFn` actual como fallback compatible.

**Criterios de aceptación**

- Sorting typed conserva estabilidad y política de nulls.
- Los benchmarks demuestran menos asignaciones que la ruta legacy.
- Columnas existentes no necesitan migración inmediata.

### GRID-PERF-602 — Filtrado preparado por consulta

**Trabajo**

- Parsear texto, números y fechas del filtro una vez por cambio.
- Reutilizar comparadores y accessors cacheados.
- Añadir índice de búsqueda opcional para datos locales estables y muchas búsquedas repetidas.
- Invalidar el índice con data version o cultura.

**Criterios de aceptación**

- No se parsea el valor del filtro por cada celda.
- El índice opcional tiene límite de memoria y no se activa por defecto sin beneficio demostrado.

### GRID-PERF-603 — Usar `FilterDebounceMilliseconds`

**Trabajo**

- Conectar `GridOptions.FilterDebounceMilliseconds` con los inputs del header.
- Sustituir timers por `CancellationTokenSource` y `Task.Delay` cancelable.
- Aplicar inmediatamente con Enter y limpiar inmediatamente con clear.
- Unificar debounce y consulta para emitir una sola operación por intención.

**Criterios de aceptación**

- Teclear rápidamente no dispara una operación por carácter.
- No se pierde foco durante la escritura.
- No queda ningún timer o tarea activa después de dispose.

## 12. Fase 7 — Virtualización horizontal de columnas

Esta fase se inicia únicamente si las métricas después de las fases 1–6 siguen excediendo el presupuesto con 30–60 columnas.

### GRID-PERF-701 — Ventana horizontal central

**Trabajo**

- Mantener siempre montadas las columnas pinned.
- Calcular las columnas center que intersectan el viewport más un overscan horizontal pequeño.
- Conservar el ancho total mediante spacers left/right.
- Recalcular prefix sums solo al cambiar layout o widths.
- Integrar navegación de teclado y `scrollIntoView` de la celda activa.
- Definir restricciones iniciales para column span/row span y ampliar solo con tests.

**Criterios de aceptación**

- Un Grid de 60 columnas monta únicamente pinned + ventana horizontal + overscan.
- Scroll horizontal rápido no muestra huecos persistentes.
- Resize, reorder, sort, filter, focus y selección funcionan con columnas desmontadas.
- Los índices ARIA reflejan la posición lógica, no la posición en la ventana.

## 13. Secuencia de entregas

| PR | Contenido | Dependencias |
|---|---|---|
| GRID-RUNTIME-01 | Diagnostics, laboratorio, benchmarks y baseline | ninguna |
| GRID-RUNTIME-02 | Scroll horizontal/touch en JS y lifecycle de listeners | 01 |
| GRID-RUNTIME-03 | Resize preview en JS | 01–02 |
| GRID-RUNTIME-04 | `RowRenderModel`, `CellRenderModel` y delegates cacheados | 01 |
| GRID-RUNTIME-05 | Render compartido pinned e invalidación dirigida | 04 |
| GRID-RUNTIME-06 | `SelectionModel` e índices incrementales | 01, 04 |
| GRID-RUNTIME-07 | `EffectiveVirtualization` y overscan adaptativo | 01–06 |
| GRID-RUNTIME-08 | `GridQuery` y provider remoto público | 01, 07 |
| GRID-RUNTIME-09 | Columnas typed, filtering y debounce | 01, 04 |
| GRID-RUNTIME-10 | Virtualización horizontal, solo si las métricas la justifican | 01–09 |

Cada PR debe incluir el resultado antes/después del escenario que pretende mejorar. No se mezclarán cambios visuales de otros componentes con estos PR.

## 14. Estrategia de pruebas

### Unitarias y bUnit

- Invalidación y reutilización de `RowRenderModel`.
- Número de ejecuciones de delegates por versión.
- Selección incremental, range, group y persistencia por key.
- Construcción e igualdad de `GridQuery`.
- Cancelación, orden de respuestas y cache remoto.
- Cálculo de overscan efectivo y presupuesto de celdas.
- Sorting/filtering typed, nulls y cultura.

### Playwright

- Scroll vertical y horizontal en Chromium, Firefox y WebKit.
- Touch/pointer, resize y desmontaje.
- Sin huecos blancos en cambio rápido de dirección.
- Sin callbacks .NET en interacción visual de alta frecuencia.
- Pinned left/right sincronizado.
- Navegación y foco con ventanas verticales y horizontales.

### Compatibilidad

- Blazor WebAssembly y aplicación hosted/Kestrel de los samples.
- API legacy basada en `GridData` y callbacks.
- Grupos, spans, templates, pinned, paginator y selección en combinaciones representativas.
- Culturas `es-ES`, `en-US` y RTL.

## 15. Gates por entrega

- `dotnet build` de la solución y samples sin errores.
- Tests unitarios y browser tests relacionados en verde.
- `git diff --check` limpio.
- Sin listeners, observers, CTS, timers o frames pendientes después de dispose.
- Sin tareas async descartadas.
- Baseline y resultado posterior adjuntos para cambios de rendimiento.
- Ninguna regresión funcional o de accesibilidad en la matriz afectada.
- Si una métrica empeora más del 10 %, el PR debe justificar el tradeoff y recibir decisión explícita antes de continuar.

## 16. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| JavaScript y Blazor pierden sincronización | JS solo controla el estado visual de alta frecuencia; confirma el estado estable al terminar el gesto |
| Cache de render muestra datos obsoletos | Versiones explícitas de data, layout, culture y selection; tests de invalidación |
| Overscan adaptativo oscila o deja huecos | Histeresis, límites min/max y tests con cambio de dirección |
| Provider remoto rompe integraciones | API aditiva, adapter legacy y periodo de obsolescencia |
| Optimización typed duplica API | Ruta typed opcional y fallback legacy común |
| Virtualización horizontal rompe spans/foco | Entrega condicionada a métricas, alcance inicial explícito y pruebas E2E |
| Benchmarks CI son inestables | Medianas, warmup, comparación relativa y separación entre microbenchmarks y browser tests |

## 17. Criterio global de terminado

- [ ] Baseline reproducible almacenado.
- [ ] Scroll horizontal y táctil sin callbacks/renders .NET por frame.
- [ ] Resize visual sin renders Blazor durante drag.
- [ ] Modelo de fila/celda calculado una vez y compartido con pinned.
- [ ] Selección incremental y centralizada por claves.
- [ ] Virtualización desactivada automáticamente cuando no aporta beneficio.
- [ ] Overscan adaptativo dentro de un presupuesto de celdas.
- [ ] Provider remoto público, cancelable y versionado.
- [ ] Sorting/filtering typed sin conversiones innecesarias.
- [ ] Debounce configurable conectado y cancelable.
- [ ] Matriz 100k filas/60 columnas validada sin bandas blancas persistentes.
- [ ] Virtualización horizontal implementada o descartada con evidencia medida.
- [ ] Documentación de configuración, tuning, provider remoto y migración actualizada.

## 18. Referencias técnicas

- [Prácticas recomendadas de rendimiento de renderizado en Blazor](https://learn.microsoft.com/aspnet/core/blazor/performance/rendering)
- [Virtualización de componentes ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/components/virtualization)
- [Componente QuickGrid de Blazor](https://learn.microsoft.com/aspnet/core/blazor/components/quickgrid)
- [Rendimiento de la interoperabilidad JavaScript en Blazor](https://learn.microsoft.com/aspnet/core/blazor/performance/javascript-interoperability)

